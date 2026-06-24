using System;
using Sapientia.Collections;
using Submodules.Sapientia.Data;
using Submodules.Sapientia.Memory;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Рантайм-оркестратор исполнения: из Static-топологии (<see cref="NodeMapHeader"/>) инстанцирует
	/// <b>батч-DAG</b> <b>подграфа, достижимого от входной ноды</b> (<see cref="Inject"/> — «начать с этой ноды»),
	/// и обходит его в <b>детерминированном порядке зависимостей</b> (<see cref="Drain"/>, single-thread).
	/// <b>Батч = линейная цепочка нод</b>: идут последовательно, никого
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
		private UnsafeList<Id<ExecutionBatch>> _startBatches;

		public readonly bool IsCreated => _batches.IsCreated;
		public readonly int BatchCount => _batches.count;

		public static ExecutionGraph Create(Id<MemoryManager> memoryId = default)
		{
			return new ExecutionGraph
			{
				_memoryId = memoryId,
				_batches = new UnsafeList<ExecutionBatch>(memoryId, 8),
				_startBatches = new UnsafeList<Id<ExecutionBatch>>(memoryId, 8),
			};
		}

		/// <summary>
		/// Инстанцирует батч-DAG <b>подграфа, достижимого вперёд от входной ноды</b> <paramref name="entry"/>
		/// (а не всего блюпринта): обход по outputs от <see cref="NodeInstanceId.nodeId"/> → chain-декомпозиция
		/// этого подграфа → батчи с <see cref="NodeInstanceId"/> (несут <see cref="NodeInstanceId.blueprintId"/>
		/// инстанса). Это модель входных нод / ExecRef: «начать исполнение с этой ноды». Батчи накапливаются
		/// (несколько входов/инстансов), индексы потомков — глобальные. Возвращает <b>число инжектнутых нод</b>
		/// (|R|) — столько запишет <see cref="Drain"/>; владелец-оркестратор суммирует это в размер буфера.
		/// </summary>
		/// <remarks><b>inDegree считается ТОЛЬКО по рёбрам внутри подграфа</b> (предшественники вне достижимого
		/// множества не учитываются) — иначе downstream-join ждал бы предка, которого этот прогон не исполняет, и
		/// <c>remainingDeps</c> не дошёл бы до 0. outputs форвард-замкнуты ⇒ все цели рёбер из <c>R</c> тоже в
		/// <c>R</c>. <b>Дедуп между разными Inject не делается</b>: если два входа достигают общей ноды, она
		/// попадёт в оба прогона (ответственность вызывающего; кросс-входной дедуп — позже).</remarks>
		public int Inject(ref CompiledBlueprintHeader compiled, NodeInstanceId entry)
		{
			var baseBatch = _batches.count;
			var nodeCount = compiled.NodesCount;
			if (nodeCount == 0)
				return 0;

			// inR[node] — достижима ли нода от entry (множество R); inDegR — inDegree В ПРЕДЕЛАХ R;
			// batchOf[node] — глобальный индекс батча ноды (-1 = не назначена). Индекс — 0-based id ноды.
			var inR = new UnsafeArray<bool>(_memoryId, nodeCount);
			var inDegR = new UnsafeArray<Id<NodeHeader>>(_memoryId, nodeCount);
			var batchOf = new UnsafeArray<Id<ExecutionBatch>>(_memoryId, nodeCount);
			var frontier = new UnsafeList<Id<NodeHeader>>(_memoryId, 8);
			var injected = 0;
			try
			{
				for (var i = 0; i < nodeCount; i++)
					batchOf[i] = -1;

				// Pass 0: достижимость вперёд от entry (BFS по outputs). injected == |R|.
				var entryNode = entry.nodeId;
				inR[entryNode] = true;
				frontier.Add(entryNode);
				var cursor = 0;
				while (cursor < frontier.count)
				{
					var u = frontier[cursor++];
					injected++;
					ref var rel = ref compiled.GetNodeRelatives(u);
					for (var i = 0; i < rel.outputs.Length; i++)
					{
						var v = rel.outputs.Get(i);
						if (inR[v])
							continue;
						inR[v] = true;
						frontier.Add(v);
					}
				}

				// Pass 0.5: inDegree в пределах R (outputs форвард-замкнуты ⇒ цель ∈ R).
				for (var u = 0; u < nodeCount; u++)
				{
					if (!inR[u])
						continue;
					ref var rel = ref compiled.GetNodeRelatives(u);
					for (var i = 0; i < rel.outputs.Length; i++)
					{
						var v = rel.outputs.Get(i);
						inDegR[v]++;
					}
				}

				// Pass 1: от каждой головы подграфа — линейная цепочка нод в один батч.
				for (var n = 0; n < nodeCount; n++)
				{
					if (!inR[n] || batchOf[n] != -1)
						continue;
					if (!IsChainHead(ref compiled, n, inR, inDegR))
						continue;

					var batchIndex = _batches.count;
					var batch = new ExecutionBatch
					{
						nextBatches = new UnsafeList<Id<ExecutionBatch>>(_memoryId, 2),
						nodesOrder = new UnsafeList<NodeInstanceId>(_memoryId, 2),
					};

					var cur = n;
					while (true)
					{
						E.ASSERT(batchOf[cur] == -1, "[ExecutionGraph] Нода назначена двум батчам (цикл/некорректный граф).");
						batchOf[cur] = batchIndex;
						batch.nodesOrder.Add(new NodeInstanceId { blueprintId = entry.blueprintId, nodeId = cur });

						ref var rel = ref compiled.GetNodeRelatives(cur);
						if (rel.outputs.Length != 1)
							break;
						var next = rel.outputs.Get(0);
						// Цепочка продолжается только в ещё не назначенную ноду с единственным (в подграфе) входом
						// (batchOf[next]==-1 — страховка от зацикливания на некорректном (циклическом) графе).
						if (inDegR[next] != 1 || batchOf[next] != -1)
							break;
						cur = next;
					}

					_batches.Add(batch);
				}

				// Pass 2: рёбра + счётчики. По свойству цепочки: предшественники головы — из РАЗНЫХ батчей ⇒
				// inDegree(батча) == inDegR(головы); межбатчевые рёбра выходят только из хвоста.
				for (var b = baseBatch; b < _batches.count; b++)
				{
					ref var batch = ref _batches[b];
					var head = batch.nodesOrder[0].nodeId;
					var tail = batch.nodesOrder[batch.nodesOrder.count - 1].nodeId;

					batch.inDegree = inDegR[head];
					batch.remainingDeps = batch.inDegree;
					if (batch.inDegree == 0)
						_startBatches.Add(b);

					ref var tailRel = ref compiled.GetNodeRelatives(tail);
					for (var i = 0; i < tailRel.outputs.Length; i++)
					{
						var target = tailRel.outputs.Get(i);
						E.ASSERT(batchOf[target] >= 0, "[ExecutionGraph] Потомок хвоста не назначен батчу (цикл/некорректный граф).");
						batch.nextBatches.Add(batchOf[target]);
					}
				}
			}
			finally
			{
				inR.Dispose();
				inDegR.Dispose();
				batchOf.Dispose();
				frontier.Dispose();
			}

			return injected;
		}

		/// <summary>Нода — голова батча в подграфе: вход/корень (<c>inDegR!=1</c>) либо её единственный
		/// достижимый предшественник ветвится. Степени — в пределах <paramref name="inR"/> (см. <see cref="Inject"/>).</summary>
		private static bool IsChainHead(ref CompiledBlueprintHeader compiled, Id<NodeHeader> nodeId, in UnsafeArray<bool> inR, in UnsafeArray<Id<NodeHeader>> inDegR)
		{
			if (inDegR[nodeId] != 1)
				return true; // 0 (вход/корень подграфа) или join (>1) — всегда голова

			// inDegR == 1: продолжение возможно, только если единственный достижимый предшественник не ветвится.
			// rel/inputs трогаем ТОЛЬКО через ref: BumpArray self-relative (Get резолвит offset от своего адреса) —
			// копия по значению на стек сломала бы базу (инвариант «не копировать BumpHeader по значению»).
			ref var rel = ref compiled.GetNodeRelatives(nodeId);
			for (var i = 0; i < rel.inputs.Length; i++)
			{
				var p = rel.inputs.Get(i);
				if (inR[p])
					return compiled.GetNodeRelatives(p).outputs.Length != 1;
			}
			return true; // недостижимо: inDegR==1 ⇒ существует ровно один предшественник в R
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
			var queue = new UnsafeList<Id<ExecutionBatch>>(_memoryId, _startBatches.count > 0 ? _startBatches.count : 1);
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

		public UnsafeList<Id<ExecutionBatch>> nextBatches;            // индексы зависимых батчей (параллелизм — M7)
		public UnsafeList<NodeInstanceId> nodesOrder;  // ноды цепочки последовательно
	}

	/// <summary>Бэкенд исполнения ноды. Бакетинг батчей по нему + чередование Burst↔Managed — M7 (форки 6,8).</summary>
	public enum RuntimeType : byte
	{
		Unmanaged,
		Managed,
	}
}
