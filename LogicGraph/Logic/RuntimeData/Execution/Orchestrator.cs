using System;
using Sapientia.Collections;
using Submodules.Sapientia.Data;
using Submodules.Sapientia.Memory;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// <b>Оркестратор прогона</b> (M7-A): владеет <see cref="ExecutionGraph"/> (батч-DAG) и off-allocator
	/// буфером порядка обхода (<see cref="_order"/>), переиспользуемым между прогонами. <see cref="Run"/> —
	/// <see cref="ExecutionGraph.ResetDeps"/> → детерминированный обход (<see cref="ExecutionGraph.Drain"/>) →
	/// per-node <see cref="NodeInvoker.Invoke"/>. <b>Кеш не сбрасывает</b> — это делает вызывающий раз за
	/// итерацию апдейта (см. тело <see cref="Run"/>).
	/// Переезд временного seam'а <c>NodeInvoker.Run</c> сюда — без изменения поведения (память
	/// <c>logicgraph-run-temporary</c>); <see cref="NodeInvoker"/> остаётся чистым диспатчем.
	/// </summary>
	/// <remarks><b>Single-thread, один compiled-блоб за прогон</b> (мульти-инстанс <b>одного</b> блюпринта —
	/// ОК: общий <c>compiled</c>, память различается по <see cref="NodeInstanceId.blueprintId"/>). Бакетинг по
	/// <c>runtimeType</c> + Burst/Managed wave-passes — M7-B; группа <b>разных</b> блюпринтов (per-node резолв
	/// <c>compiled</c> через storage) — M7-C; джоб-параллелизм внутри wave + next-wave-буфер — M7-D.</remarks>
	public struct Orchestrator : IDisposable
	{
		private Id<MemoryManager> _memoryId;
		private ExecutionGraph _graph;
		// Буфер порядка обхода (Drain пишет сюда): ленивый, переиспользуется, растёт под _nodeCount.
		private UnsafeArray<NodeInstanceId> _order;
		// Суммарное число нод по всем Inject'ам (== числу записей Drain) — владелец буфера сам считает размер.
		private int _nodeCount;

		public readonly bool IsCreated => _graph.IsCreated;

		public static Orchestrator Create(Id<MemoryManager> memoryId = default)
		{
			return new Orchestrator
			{
				_memoryId = memoryId,
				_graph = ExecutionGraph.Create(memoryId),
				// _order — ленивый: создаётся/растёт в Run под накопленный _nodeCount.
			};
		}

		/// <summary>
		/// Инстанцирует подграф, достижимый от входной ноды <paramref name="entry"/> (форвард в
		/// <see cref="ExecutionGraph.Inject"/>; батчи накапливаются — несколько входов/инстансов). Возвращает
		/// число инжектнутых нод; оркестратор суммирует его в размер буфера порядка.
		/// </summary>
		public int Inject(ref CompiledBlueprintHeader compiled, NodeInstanceId entry)
		{
			// Размер буфера = сумма инжектнутых нод по всем входам (== числу записей Drain).
			var injected = _graph.Inject(ref compiled, entry);
			_nodeCount += injected;
			return injected;
		}

		/// <summary>
		/// <b>Прогон блюпринта</b> (single-thread): гарантирует буфер порядка ≥ суммарного числа нод (накоплено в
		/// <see cref="Inject"/>), сбрасывает счётчики зависимостей (<c>ResetDeps</c>) → <see cref="ExecutionGraph.Drain"/>
		/// → <see cref="NodeInvoker.Invoke"/> каждой ноды по порядку. Возвращает число исполненных нод.
		/// <b>Кеш не трогает</b> — его сброс делает вызывающий раз за итерацию апдейта (см. тело).
		/// </summary>
		public int Run(ref ExecutionScope scope, ref CompiledBlueprintHeader compiled)
		{
			EnsureOrder(_nodeCount);

			// Кеш здесь НЕ сбрасываем: сброс — забота вызывающего, один раз перед итерацией апдейта нод (напр.
			// перед Update-пачкой / перед LateUpdate-пачкой по тем же блюпринтам), а не на каждый Run/wave. Внутри
			// итерации нода с валидным кешем повторно не исполняется (pull-based мемоизация — M8).
			_graph.ResetDeps();

			// Пустой граф (Run без Inject) — буфер не создаём, Drain получает пустой span и пишет 0 (как было
			// со stackalloc[0]); иначе MakeArray(0) роняет DEBUG-assert.
			var order = _order.IsCreated ? _order.GetSpan() : Span<NodeInstanceId>.Empty;
			var count = _graph.Drain(order);
			for (var i = 0; i < count; i++)
				NodeInvoker.Invoke(ref scope, ref compiled, order[i]);

			return count;
		}

		/// <summary>Лениво создаёт/наращивает owned-буфер порядка под <paramref name="need"/> нод (без per-run alloc,
		/// пока топология не выросла). Re-alloc только при росте — старый блок освобождается.</summary>
		private void EnsureOrder(int need)
		{
			// need == 0 (пустой граф) — не аллоцируем (MakeArray(0) — DEBUG-assert); Run подаёт пустой span.
			if (need == 0 || (_order.IsCreated && _order.Length >= need))
				return;

			if (_order.IsCreated)
				_order.Dispose();
			_order = new UnsafeArray<NodeInstanceId>(_memoryId, need);
		}

		public void Dispose()
		{
			if (!IsCreated)
				return;

			if (_order.IsCreated)
				_order.Dispose();
			_graph.Dispose();
			this = default;
		}
	}
}
