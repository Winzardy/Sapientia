using System;
using Sapientia.Collections;
using Submodules.Sapientia.Data;
using Submodules.Sapientia.Memory;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Рантайм-оркестратор исполнения: из Static-топологии (<see cref="NodeMapHeader"/>) инстанцирует
	/// <b>батч-DAG</b> под инстанс(ы) и обходит его в <b>детерминированном порядке зависимостей</b>
	/// (<see cref="Drain"/>, single-thread). <b>Батч = линейная цепочка нод</b>: идут последовательно, никого
	/// не ждут и не ветвятся; цепочка <c>P→N</c> сливается ⟺ <c>outDeg(P)==1 &amp;&amp; inDeg(N)==1</c>.
	/// Память — off-allocator (<see cref="UnsafeList{T}"/>); тела нод <b>не исполняются</b> (диспатч — M6),
	/// джоб-параллелизм и wave-модель (Burst↔Managed, RuntimeType-бакетинг) — M7.
	/// </summary>
	public struct ExecutionGraph : IDisposable
	{
		private Id<MemoryManager> _memoryId;
		// Батч-DAG (накопительно по Inject); индекс в списке = глобальный id батча.
		private UnsafeList<ExecutionBatch> _batches;
		// Индексы батчей без зависимостей (inDegree == 0) — старты обхода.
		private UnsafeList<int> _startBatches;

		public readonly bool IsCreated => _batches.IsCreated;
		public readonly int BatchCount => _batches.count;

		public static ExecutionGraph Create(Id<MemoryManager> memoryId = default)
		{
			return new ExecutionGraph
			{
				_memoryId = memoryId,
				_batches = new UnsafeList<ExecutionBatch>(memoryId, 8),
				_startBatches = new UnsafeList<int>(memoryId, 8),
			};
		}

		/// <summary>
		/// Инстанцирует батч-DAG блюпринта под <paramref name="instance"/>: chain-декомпозиция Static-топологии
		/// → батчи с <see cref="NodeInstanceId"/>. Батчи накапливаются (multi-instance), индексы потомков —
		/// глобальные. Возвращает индекс первого добавленного батча (смещение этого инстанса).
		/// </summary>
		public int Inject(ref CompiledBlueprintHeader compiled, BlueprintInstanceId instance)
		{
			var baseBatch = _batches.count;
			var nodeCount = compiled.NodesCount;
			if (nodeCount == 0)
				return baseBatch;

			// batchOf[node] = глобальный индекс батча (каждая нода принадлежит ровно одной цепочке); -1 = не назначена.
			var batchOf = new UnsafeArray<int>(_memoryId, nodeCount);
			try
			{
				for (var i = 0; i < nodeCount; i++)
					batchOf[i] = -1;

				// Pass 1: от каждой головы — линейная цепочка нод в один батч.
				for (var n = 0; n < nodeCount; n++)
				{
					if (!IsChainHead(ref compiled, n))
						continue;

					var batchIndex = _batches.count;
					var batch = new ExecutionBatch
					{
						nextBatches = new UnsafeList<int>(_memoryId, 2),
						nodesOrder = new UnsafeList<NodeInstanceId>(_memoryId, 2),
					};

					var cur = n;
					while (true)
					{
						E.ASSERT(batchOf[cur] == -1, "[ExecutionGraph] Нода назначена двум батчам (цикл/некорректный граф).");
						batchOf[cur] = batchIndex;
						batch.nodesOrder.Add(new NodeInstanceId { blueprintId = instance, nodeId = cur });

						ref var rel = ref compiled.GetNodeRelatives(cur);
						if (rel.outputs.Length != 1)
							break;
						var next = (int)rel.outputs.Get(0);
						// Цепочка продолжается только в ещё не назначенную ноду с единственным входом
						// (условие batchOf[next]==-1 — страховка от зацикливания на некорректном (циклическом) графе).
						if (compiled.GetNodeInDegree(next) != 1 || batchOf[next] != -1)
							break;
						cur = next;
					}

					_batches.Add(batch);
				}

				// Pass 2: рёбра + счётчики. По свойству цепочки: предшественники головы — из РАЗНЫХ батчей ⇒
				// inDegree(батча) == inDegree(головы); межбатчевые рёбра выходят только из хвоста.
				for (var b = baseBatch; b < _batches.count; b++)
				{
					ref var batch = ref _batches[b];
					var head = (int)batch.nodesOrder[0].nodeId;
					var tail = (int)batch.nodesOrder[batch.nodesOrder.count - 1].nodeId;

					batch.inDegree = compiled.GetNodeInDegree(head);
					batch.remainingDeps = batch.inDegree;
					if (batch.inDegree == 0)
						_startBatches.Add(b);

					ref var tailRel = ref compiled.GetNodeRelatives(tail);
					for (var i = 0; i < tailRel.outputs.Length; i++)
					{
						var target = batchOf[(int)tailRel.outputs.Get(i)];
						E.ASSERT(target >= 0, "[ExecutionGraph] Потомок хвоста не назначен батчу (цикл/некорректный граф).");
						batch.nextBatches.Add(target);
					}
				}
			}
			finally
			{
				batchOf.Dispose();
			}

			return baseBatch;
		}

		/// <summary>Нода — голова батча: корень (<c>inDeg!=1</c>) либо её единственный предшественник ветвится.</summary>
		private static bool IsChainHead(ref CompiledBlueprintHeader compiled, int node)
		{
			var inDegree = compiled.GetNodeInDegree(node);
			if (inDegree != 1)
				return true; // 0 (корень) или join (>1) — всегда голова

			// inDeg == 1: продолжение цепочки возможно, только если единственный предшественник не ветвится.
			var pred = (int)compiled.GetNodeRelatives(node).inputs.Get(0);
			return compiled.GetNodeRelatives(pred).outputs.Length != 1;
		}

		/// <summary>Сбрасывает счётчики зависимостей перед прогоном: <c>remainingDeps = inDegree</c>.</summary>
		public void ResetDeps()
		{
			for (var i = 0; i < _batches.count; i++)
				_batches[i].remainingDeps = _batches[i].inDegree;
		}

		/// <summary>
		/// Детерминированный обход батч-DAG: ready-queue от <see cref="_startBatches"/> (FIFO), декремент
		/// <see cref="ExecutionBatch.remainingDeps"/> у потомков. Пишет порядок нод в <paramref name="orderOut"/>
		/// (должен вмещать все ноды графа), возвращает их число. <b>Тела нод не исполняются</b> (диспатч — M6).
		/// Расходует счётчики — повторный прогон только после <see cref="ResetDeps"/>.
		/// <b>Граф предполагается ацикличным</b> (DAG); на циклическом входе (валидация графа — позже) обход
		/// частичный (<c>written</c> &lt; числа нод): ноды в цикле никогда не достигнут <c>remainingDeps==0</c>.
		/// </summary>
		public int Drain(Span<NodeInstanceId> orderOut)
		{
			var written = 0;
			var queue = new UnsafeList<int>(_memoryId, _startBatches.count > 0 ? _startBatches.count : 1);
			try
			{
				for (var i = 0; i < _startBatches.count; i++)
					queue.Add(_startBatches[i]);

				var cursor = 0;
				while (cursor < queue.count)
				{
					var b = queue[cursor++];
					ref var batch = ref _batches[b];

					for (var i = 0; i < batch.nodesOrder.count; i++)
					{
						E.ASSERT(written < orderOut.Length, "[ExecutionGraph] orderOut меньше числа нод графа.");
						orderOut[written++] = batch.nodesOrder[i];
					}

					for (var i = 0; i < batch.nextBatches.count; i++)
					{
						var nextIndex = batch.nextBatches[i];
						ref var next = ref _batches[nextIndex];
						next.remainingDeps--;
						if (next.remainingDeps == 0)
							queue.Add(nextIndex);
					}
				}
			}
			finally
			{
				queue.Dispose();
			}

			return written;
		}

		public void Dispose()
		{
			if (!IsCreated)
				return;

			for (var i = 0; i < _batches.count; i++)
			{
				ref var batch = ref _batches[i];
				batch.nextBatches.Dispose();
				batch.nodesOrder.Dispose();
			}
			_batches.Dispose();
			_startBatches.Dispose();
			this = default;
		}
	}

	/// <summary>
	/// Батч — линейная цепочка нод (<see cref="nodesOrder"/>, последовательно). В батч-DAG зависит от
	/// <see cref="inDegree"/> батчей; <see cref="remainingDeps"/> — синхронный счётчик невыполненных
	/// зависимостей (без <c>Interlocked</c>: потоки делятся по стартовым батчам ⇒ счётчик правит один поток).
	/// </summary>
	public struct ExecutionBatch
	{
		public int inDegree;      // исходное число батчей-предшественников (для ResetDeps)
		public int remainingDeps; // оставшиеся невыполненные зависимости; 0 ⇒ батч готов

		public UnsafeList<int> nextBatches;            // индексы зависимых батчей (параллелизм — M7)
		public UnsafeList<NodeInstanceId> nodesOrder;  // ноды цепочки последовательно
	}

	/// <summary>Бэкенд исполнения ноды. Бакетинг батчей по нему + чередование Burst↔Managed — M7 (форки 6,8).</summary>
	public enum RuntimeType : byte
	{
		Unmanaged,
		Managed,
	}
}
