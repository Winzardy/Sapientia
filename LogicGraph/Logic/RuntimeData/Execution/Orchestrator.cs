using System;
using Sapientia.Collections;
using Sapientia.Extensions;
using Submodules.Sapientia.Data;
using Submodules.Sapientia.Memory;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// <b>Оркестратор прогона</b> (M7-B): демандный <b>work-list</b> вместо статического батч-DAG. Сидится
	/// несколькими входными нодами (<see cref="Inject"/>), затем <see cref="Run"/> исполняет их и достижимый
	/// граф в порядке зависимостей: нода исполняется, когда все её <b>ноды-предшественники</b> посчитаны
	/// (ready-check по узловой топологии <see cref="CompiledBlueprintHeader.GetNodeRelatives"/>), результат
	/// <b>мемоизируется</b> (общий предок — один раз), а консьюмеры ставятся в очередь (push).
	/// </summary>
	/// <remarks>
	/// <b>Eager, single-thread, один рантайм</b> (forceManaged): тела нод атомарны, инпуты готовы до запуска
	/// (нода читает их из кеша). <b>Lazy</b> (`Get`-yield) — M7-D; <b>runtime-wave</b> (Burst↔managed) — M7-C;
	/// мульти-блюпринт + command-buffer — M7-E; параллелизм — вне M7. Per-node-планирование (биты <c>queued</c>/
	/// <c>computed</c>) — в per-instance байт-массиве (<see cref="_schedule"/>, слот = инстанс, размер
	/// <c>NodesCount</c>), сбрасывается в начале <see cref="Run"/>.
	/// <para><b>Инвариант:</b> pull предшественников / push консьюмеров остаются <b>в том же инстансе</b>
	/// (<see cref="NodeInstanceId.blueprintId"/> не меняется) ⇒ во время цикла новые инстансы не появляются;
	/// <see cref="_schedule"/> пре-аллоцируется под инстансы входов до цикла и <b>не ресайзится</b> внутри.</para>
	/// </remarks>
	public struct Orchestrator : IDisposable
	{
		private const byte FLAG_QUEUED = 1;
		private const byte FLAG_COMPUTED = 2;

		private Id<MemoryManager> _memoryId;
		// Work-list (FIFO по cursor'у): растёт по ходу (pull предшественников + push консьюмеров).
		private UnsafeList<NodeInstanceId> _queue;
		// Планирование per-(инстанс, нода): байт-флаги. Индекс слота = blueprintId.id (как _cache в ExecutionScope).
		private UnsafeList<UnsafeArray<byte>> _schedule;

		public readonly bool IsCreated => _queue.IsCreated;

		public static Orchestrator Create(Id<MemoryManager> memoryId = default)
		{
			return new Orchestrator
			{
				_memoryId = memoryId,
				_queue = new UnsafeList<NodeInstanceId>(memoryId, 16),
				_schedule = new UnsafeList<UnsafeArray<byte>>(memoryId, 4),
			};
		}

		/// <summary>Сидит work-list входными нодами (несколько входов/инстансов). Дедуп/планирование — в <see cref="Run"/>.</summary>
		public void Inject(ReadOnlySpan<NodeInstanceId> entries)
		{
			for (var i = 0; i < entries.Length; i++)
				_queue.Add(entries[i]);
		}

		/// <summary>
		/// <b>Прогон</b>: сброс планирования → обработка work-list'а до опустошения. Для ноды: посчитана →
		/// пропустить; не готова → pull предшественников (саму ноду ре-активирует push последнего предка); готова →
		/// <see cref="NodeInvoker.Invoke"/> + пометить computed + push консьюмеров. Возвращает число <b>исполненных</b>
		/// нод (мемоизированные пропуски не считаются). Кеш не трогает (сброс — у вызывающего раз за итерацию).
		/// </summary>
		public int Run(ref ExecutionScope scope, ref CompiledBlueprintHeader compiled)
		{
			var nodeCount = compiled.NodesCount;
			ResetSchedules();

			// Пре-аллокация планирования под инстансы входов (pull/push не создают новых) ⇒ _schedule не растёт в цикле.
			for (var i = 0; i < _queue.count; i++)
				EnsureSchedule(_queue[i].blueprintId, nodeCount);
			// Начальные входы — queued (дедуп при последующих push/pull).
			for (var i = 0; i < _queue.count; i++)
				SetFlag(_queue[i].blueprintId, _queue[i].nodeId, FLAG_QUEUED);

			var executed = 0;
			var cursor = 0;
			while (cursor < _queue.count)
			{
				var node = _queue[cursor++];
				ClearFlag(node.blueprintId, node.nodeId, FLAG_QUEUED); // вышла из очереди

				if (HasFlag(node.blueprintId, node.nodeId, FLAG_COMPUTED))
					continue; // уже посчитана (мемоизация — общий предок один раз)

				// ready-check: все ноды-предшественники посчитаны? Непосчитанных — pull (поставить в очередь).
				ref var rel = ref compiled.GetNodeRelatives(node.nodeId);
				var ready = true;
				for (var i = 0; i < rel.inputs.Length; i++)
				{
					var pred = rel.inputs.Get(i);
					if (HasFlag(node.blueprintId, pred, FLAG_COMPUTED))
						continue;
					ready = false;
					Enqueue(node.blueprintId, pred);
				}

				// Не готова: предшественники запулены выше; ноду НЕ ре-энкьюим — её ре-активирует push последнего
				// досчитавшегося предшественника (топология симметрична: pred∈inputs(N) ⟺ N∈outputs(pred)). Это даёт
				// терминирование даже на циклах (нода в цикле так и не станет ready ⇒ частичный обход, как старый Drain),
				// и не плодит спин-дубли в очереди.
				if (!ready)
					continue;

				NodeInvoker.Invoke(ref scope, ref compiled, node);
				SetFlag(node.blueprintId, node.nodeId, FLAG_COMPUTED);
				executed++;

				// push: консьюмеры могли стать готовы (рефетч relatives — Invoke трогал scope/cache, не блоб).
				ref var relOut = ref compiled.GetNodeRelatives(node.nodeId);
				for (var i = 0; i < relOut.outputs.Length; i++)
					Enqueue(node.blueprintId, relOut.outputs.Get(i));
			}

			_queue.Clear();
			return executed;
		}

		/// <summary>Ставит ноду в очередь, если она ещё не там и не посчитана (дедуп).</summary>
		private void Enqueue(BlueprintInstanceId instance, Id<NodeHeader> node)
		{
			ref var flags = ref ScheduleByte(instance, node);
			if ((flags & (FLAG_QUEUED | FLAG_COMPUTED)) != 0)
				return;
			flags |= FLAG_QUEUED;
			_queue.Add(new NodeInstanceId { blueprintId = instance, nodeId = node });
		}

		private bool HasFlag(BlueprintInstanceId instance, Id<NodeHeader> node, byte flag)
		{
			return (ScheduleByte(instance, node) & flag) != 0;
		}

		private void SetFlag(BlueprintInstanceId instance, Id<NodeHeader> node, byte flag)
		{
			ScheduleByte(instance, node) |= flag;
		}

		private void ClearFlag(BlueprintInstanceId instance, Id<NodeHeader> node, byte flag)
		{
			ref var b = ref ScheduleByte(instance, node);
			b = (byte)(b & ~flag);
		}

		/// <summary><c>ref</c> на байт флагов ноды в per-instance массиве. Берётся в <b>уже пре-аллоцированный</b>
		/// слот (см. инвариант) — без <c>EnsureCount</c>, поэтому <see cref="_schedule"/> не ресайзится, а
		/// возвращаемый <c>ref</c> указывает в стабильный байт-буфер.</summary>
		private ref byte ScheduleByte(BlueprintInstanceId instance, Id<NodeHeader> node)
		{
			return ref _schedule[instance.id].ptr[node];
		}

		/// <summary>Гарантирует per-instance массив планирования (слот = <c>blueprintId.id</c>, размер <paramref name="nodeCount"/>).</summary>
		private void EnsureSchedule(BlueprintInstanceId instance, int nodeCount)
		{
			_schedule.EnsureCount(instance.id + 1, default);
			ref var arr = ref _schedule[instance.id];
			if (!arr.IsCreated)
				arr = new UnsafeArray<byte>(_memoryId, nodeCount);
			// Слот инстанса связан с одним блюпринтом ⇒ размер не меняется. Reuse под больший граф — рассинхрон.
			E.ASSERT(arr.Length >= nodeCount, "[Orchestrator] schedule-слот меньше NodesCount (слот переиспользован под больший блюпринт?).");
		}

		/// <summary>Сброс планирования перед прогоном: чистит все существующие per-instance массивы (queued/computed → 0).</summary>
		private void ResetSchedules()
		{
			for (var i = 0; i < _schedule.count; i++)
			{
				ref var arr = ref _schedule[i];
				if (arr.IsCreated)
					arr.Clear();
			}
		}

		public void Dispose()
		{
			if (!IsCreated)
				return;

			for (var i = 0; i < _schedule.count; i++)
				_schedule[i].Dispose();
			_schedule.Dispose();
			_queue.Dispose();
			this = default;
		}
	}
}
