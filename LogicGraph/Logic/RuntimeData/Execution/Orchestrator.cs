using System;
using Sapientia.Collections;
using Sapientia.Data;
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
		// Кеш compiled-блоба на инстанс (M7-E, мульти-блюпринт): резолв key→storage.Get раз/инстанс, не на ноду.
		// SafePtr в off-allocator storage-блоб (стабилен, не владеем — не диспозим элементы).
		private UnsafeList<SafePtr<CompiledBlueprintHeader>> _compiled;

		public readonly bool IsCreated => _queue.IsCreated;

		public static Orchestrator Create(Id<MemoryManager> memoryId = default)
		{
			return new Orchestrator
			{
				_memoryId = memoryId,
				_queue = new UnsafeList<NodeInstanceId>(memoryId, 16),
				_schedule = new UnsafeList<UnsafeArray<byte>>(memoryId, 4),
				_compiled = new UnsafeList<SafePtr<CompiledBlueprintHeader>>(memoryId, 4),
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
		public int Run(ref ExecutionScope scope, ref CompiledBlueprintStorage storage)
		{
			ResetSchedules();

			// Пре-резолв инстансов входов (compiled-ptr + schedule под NodesCount); pull/push не создают новых
			// инстансов ⇒ _schedule/_compiled не растут в цикле, рефы в их буферы стабильны.
			for (var i = 0; i < _queue.count; i++)
				EnsureInstance(ref scope, ref storage, _queue[i].blueprintId);
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

				ref var compiled = ref CompiledOf(node.blueprintId); // блоб именно этого инстанса (мульти-блюпринт)

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
				ref var relOut = ref CompiledOf(node.blueprintId).GetNodeRelatives(node.nodeId);
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

		/// <summary>Резолвит инстанс (слот = <c>blueprintId.id</c>): кеширует его compiled-ptr (key→storage.Get) и
		/// заводит schedule-массив под <c>NodesCount</c>. Раз/инстанс (по первому касанию); compiled-ptr и
		/// schedule переживают run'ы (storage-блоб стабилен, schedule лишь чистится).</summary>
		private void EnsureInstance(ref ExecutionScope scope, ref CompiledBlueprintStorage storage, BlueprintInstanceId instance)
		{
			_schedule.EnsureCount(instance.id + 1, default);
			_compiled.EnsureCount(instance.id + 1, default);
			ref var arr = ref _schedule[instance.id];
			if (arr.IsCreated)
				return; // уже резолвлен

			var key = scope.GetBlueprintKey(instance);
			ref var compiled = ref storage.Get(key);
			_compiled[instance.id] = compiled.AsSafePtr();
			arr = new UnsafeArray<byte>(_memoryId, compiled.NodesCount);
		}

		/// <summary><c>ref</c> на compiled-блоб инстанса из кеша (<see cref="EnsureInstance"/>). Адрес в стабильном
		/// off-allocator storage-блобе.</summary>
		private ref CompiledBlueprintHeader CompiledOf(BlueprintInstanceId instance)
		{
			return ref _compiled[instance.id].Value();
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
			_compiled.Dispose(); // элементы — SafePtr в чужой storage, не диспозим
			_queue.Dispose();
			this = default;
		}
	}
}
