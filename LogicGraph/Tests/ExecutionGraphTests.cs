#if UNITY_5_4_OR_NEWER
using System;
using NUnit.Framework;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// 4B: <see cref="ExecutionGraph"/> — батч-DAG (батч = линейная цепочка нод) из Static-топологии (4A) +
	/// детерминированный обход (<see cref="ExecutionGraph.Drain"/>). Тела нод не исполняются (M6). Инстанс —
	/// синтетический <see cref="BlueprintInstanceId"/> (без <c>ExecutionScope</c>, он 4F). Паттерн:
	/// <c>CompileLayout</c> → <c>Inject(ref compiled, …)</c> → assert/Drain → <c>Dispose</c> в <c>finally</c>.
	/// </summary>
	public class ExecutionGraphTests
	{
		private static BlueprintInstanceId Instance(int id)
		{
			return new BlueprintInstanceId { id = id, generation = 1 };
		}

		/// <summary>Входная нода прогона: <see cref="NodeInstanceId"/> = (инстанс, <paramref name="nodeId"/>). Inject строит подграф от неё.</summary>
		private static NodeInstanceId Entry(int instance, Id<NodeHeader> nodeId)
		{
			return new NodeInstanceId { blueprintId = Instance(instance), nodeId = nodeId };
		}

		/// <summary>Индекс первой ноды <paramref name="nodeId"/> в порядке обхода (или −1).</summary>
		private static int IndexOf(ReadOnlySpan<NodeInstanceId> order, int count, int nodeId)
		{
			for (var i = 0; i < count; i++)
			{
				if ((int)order[i].nodeId == nodeId)
					return i;
			}
			return -1;
		}

		/// <summary>Цепочка A(0)→B(1)→C(2).</summary>
		private static Blueprint Chain3()
		{
			var aOut = new NodeOutput<long>();
			var bIn = new NodeInput<long>();
			var bOut = new NodeOutput<long>();
			var cIn = new NodeInput<long>();
			var bp = StubBlueprint.Of(
				new StubNode(cacheSize: 16, outputs: new NodeOutput[] { aOut }),
				new StubNode(cacheSize: 16, inputs: new NodeInput[] { bIn }, outputs: new NodeOutput[] { bOut }),
				new StubNode(inputs: new NodeInput[] { cIn }));
			bp.inputToOutput[bIn] = aOut;
			bp.inputToOutput[cIn] = bOut;
			return bp;
		}

		/// <summary>Ромб A(0)→B(1),A→C(2); B→D(3),C→D.</summary>
		private static Blueprint Diamond()
		{
			var aOut = new NodeOutput<long>();
			var bIn = new NodeInput<long>();
			var bOut = new NodeOutput<long>();
			var cIn = new NodeInput<long>();
			var cOut = new NodeOutput<long>();
			var dInB = new NodeInput<long>();
			var dInC = new NodeInput<long>();
			var bp = StubBlueprint.Of(
				new StubNode(cacheSize: 16, outputs: new NodeOutput[] { aOut }),
				new StubNode(cacheSize: 16, inputs: new NodeInput[] { bIn }, outputs: new NodeOutput[] { bOut }),
				new StubNode(cacheSize: 16, inputs: new NodeInput[] { cIn }, outputs: new NodeOutput[] { cOut }),
				new StubNode(inputs: new NodeInput[] { dInB, dInC }));
			bp.inputToOutput[bIn] = aOut;
			bp.inputToOutput[cIn] = aOut;
			bp.inputToOutput[dInB] = bOut;
			bp.inputToOutput[dInC] = cOut;
			return bp;
		}

		/// <summary>Цепочка A(0)→B(1), затем хвост B ветвится: B→C(2), B→D(3) (fan-out из хвоста цепочки длины ≥2).</summary>
		private static Blueprint ChainThenFork()
		{
			var aOut = new NodeOutput<long>();
			var bIn = new NodeInput<long>();
			var bOut = new NodeOutput<long>();
			var cIn = new NodeInput<long>();
			var dIn = new NodeInput<long>();
			var bp = StubBlueprint.Of(
				new StubNode(cacheSize: 16, outputs: new NodeOutput[] { aOut }),
				new StubNode(cacheSize: 16, inputs: new NodeInput[] { bIn }, outputs: new NodeOutput[] { bOut }),
				new StubNode(inputs: new NodeInput[] { cIn }),
				new StubNode(inputs: new NodeInput[] { dIn }));
			bp.inputToOutput[bIn] = aOut;
			bp.inputToOutput[cIn] = bOut;
			bp.inputToOutput[dIn] = bOut;
			return bp;
		}

		/// <summary>Цепочка A(0)→B(1) (для multi-instance).</summary>
		private static Blueprint Chain2()
		{
			var aOut = new NodeOutput<long>();
			var bIn = new NodeInput<long>();
			var bp = StubBlueprint.Of(
				new StubNode(cacheSize: 16, outputs: new NodeOutput[] { aOut }),
				new StubNode(inputs: new NodeInput[] { bIn }));
			bp.inputToOutput[bIn] = aOut;
			return bp;
		}

		[Test]
		public void Execution_ChainCoalesced()
		{
			var arena = BlueprintCompiler.CompileLayout(Chain3(), out var offset);
			var graph = ExecutionGraph.Create();
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				graph.Inject(ref compiled, Entry(1, 0));

				Assert.AreEqual(1, graph.BatchCount, "Линейная цепочка должна схлопнуться в один батч.");

				Span<NodeInstanceId> order = stackalloc NodeInstanceId[3];
				var n = graph.Drain(order);
				Assert.AreEqual(3, n);
				Assert.AreEqual(0, (int)order[0].nodeId);
				Assert.AreEqual(1, (int)order[1].nodeId);
				Assert.AreEqual(2, (int)order[2].nodeId);
				Assert.IsTrue(order[0].blueprintId == Instance(1), "NodeInstanceId должен нести инстанс.");
			}
			finally
			{
				graph.Dispose();
				arena.Dispose();
			}
		}

		[Test]
		public void Execution_Diamond()
		{
			var arena = BlueprintCompiler.CompileLayout(Diamond(), out var offset);
			var graph = ExecutionGraph.Create();
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				graph.Inject(ref compiled, Entry(1, 0));

				Assert.AreEqual(4, graph.BatchCount, "Ветвление/join не сливаются: 4 батча.");

				Span<NodeInstanceId> order = stackalloc NodeInstanceId[4];
				var n = graph.Drain(order);
				Assert.AreEqual(4, n);

				var posA = IndexOf(order, n, 0);
				var posB = IndexOf(order, n, 1);
				var posC = IndexOf(order, n, 2);
				var posD = IndexOf(order, n, 3);
				Assert.IsTrue(posA >= 0 && posB >= 0 && posC >= 0 && posD >= 0, "Все ноды должны быть в порядке.");
				Assert.Less(posA, posB, "A до B.");
				Assert.Less(posA, posC, "A до C.");
				Assert.Less(posB, posD, "B до D (join ждёт).");
				Assert.Less(posC, posD, "C до D (join ждёт).");
			}
			finally
			{
				graph.Dispose();
				arena.Dispose();
			}
		}

		[Test]
		public void Execution_ParallelIndependent()
		{
			var arena = BlueprintCompiler.CompileLayout(StubBlueprint.Of(new StubNode(), new StubNode(), new StubNode()), out var offset);
			var graph = ExecutionGraph.Create();
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				// Три несвязанные ноды — три независимых входа (каждый подграф = одна нода).
				graph.Inject(ref compiled, Entry(1, 0));
				graph.Inject(ref compiled, Entry(1, 1));
				graph.Inject(ref compiled, Entry(1, 2));

				Assert.AreEqual(3, graph.BatchCount, "Три несвязанные ноды — три батча.");

				Span<NodeInstanceId> order = stackalloc NodeInstanceId[3];
				Assert.AreEqual(3, graph.Drain(order));
			}
			finally
			{
				graph.Dispose();
				arena.Dispose();
			}
		}

		[Test]
		public void Execution_MultiInstance()
		{
			var arena = BlueprintCompiler.CompileLayout(Chain2(), out var offset);
			var graph = ExecutionGraph.Create();
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				graph.Inject(ref compiled, Entry(1, 0));
				graph.Inject(ref compiled, Entry(2, 0));

				Assert.AreEqual(2, graph.BatchCount, "Два инстанса по одному батчу (цепочка из 2 нод).");

				Span<NodeInstanceId> order = stackalloc NodeInstanceId[4];
				var n = graph.Drain(order);
				Assert.AreEqual(4, n);

				// Накопление: первый инстанс целиком, затем второй (FIFO от двух стартов).
				Assert.IsTrue(order[0].blueprintId == Instance(1) && order[1].blueprintId == Instance(1));
				Assert.IsTrue(order[2].blueprintId == Instance(2) && order[3].blueprintId == Instance(2));
				// Внутри инстанса — порядок цепочки.
				Assert.AreEqual(0, (int)order[0].nodeId);
				Assert.AreEqual(1, (int)order[1].nodeId);
			}
			finally
			{
				graph.Dispose();
				arena.Dispose();
			}
		}

		[Test]
		public void Execution_ResetDepsReproducible()
		{
			var arena = BlueprintCompiler.CompileLayout(Diamond(), out var offset);
			var graph = ExecutionGraph.Create();
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				graph.Inject(ref compiled, Entry(1, 0));

				Span<NodeInstanceId> first = stackalloc NodeInstanceId[4];
				var n1 = graph.Drain(first);

				graph.ResetDeps();

				Span<NodeInstanceId> second = stackalloc NodeInstanceId[4];
				var n2 = graph.Drain(second);

				Assert.AreEqual(n1, n2);
				for (var i = 0; i < n1; i++)
					Assert.AreEqual((int)first[i].nodeId, (int)second[i].nodeId, $"Порядок должен воспроизводиться (поз {i}).");
			}
			finally
			{
				graph.Dispose();
				arena.Dispose();
			}
		}

		[Test]
		public void Execution_FanOutFromChainTail()
		{
			var arena = BlueprintCompiler.CompileLayout(ChainThenFork(), out var offset);
			var graph = ExecutionGraph.Create();
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				graph.Inject(ref compiled, Entry(1, 0));

				Assert.AreEqual(3, graph.BatchCount, "[A,B] схлопнуты; C и D — отдельные батчи.");

				Span<NodeInstanceId> order = stackalloc NodeInstanceId[4];
				var n = graph.Drain(order);
				Assert.AreEqual(4, n);

				var posA = IndexOf(order, n, 0);
				var posB = IndexOf(order, n, 1);
				var posC = IndexOf(order, n, 2);
				var posD = IndexOf(order, n, 3);
				Assert.Less(posA, posB, "Внутри цепочки A до B.");
				Assert.Less(posB, posC, "Хвост B до своих потомков C.");
				Assert.Less(posB, posD, "Хвост B до своих потомков D.");
			}
			finally
			{
				graph.Dispose();
				arena.Dispose();
			}
		}

		[Test]
		public void Execution_RepeatedDrainWithoutResetDrainsFewer()
		{
			var arena = BlueprintCompiler.CompileLayout(Diamond(), out var offset);
			var graph = ExecutionGraph.Create();
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				graph.Inject(ref compiled, Entry(1, 0));

				Span<NodeInstanceId> first = stackalloc NodeInstanceId[4];
				Assert.AreEqual(4, graph.Drain(first), "Первый прогон — все ноды.");

				// Без ResetDeps счётчики израсходованы: потомки стартов уже не достигнут 0.
				Span<NodeInstanceId> second = stackalloc NodeInstanceId[4];
				var n2 = graph.Drain(second);
				Assert.Less(n2, 4, "Повторный Drain без ResetDeps — частичный обход.");
				Assert.AreEqual(1, n2, "Дренится только стартовый батч (A).");
			}
			finally
			{
				graph.Dispose();
				arena.Dispose();
			}
		}

		[Test]
		public void Execution_DisposeFreesNested()
		{
			var arena = BlueprintCompiler.CompileLayout(StubBlueprint.Of(new StubNode(), new StubNode()), out var offset);
			var graph = ExecutionGraph.Create();
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				// Две несвязанные ноды — два входа (вложенные списки батчей должны освободиться в Dispose).
				graph.Inject(ref compiled, Entry(1, 0));
				graph.Inject(ref compiled, Entry(1, 1));
				Assert.IsTrue(graph.IsCreated);
			}
			finally
			{
				arena.Dispose();
			}

			graph.Dispose();
			Assert.IsFalse(graph.IsCreated, "После Dispose граф не создан.");
			Assert.DoesNotThrow(() => graph.Dispose(), "Повторный Dispose — no-op.");
		}
	}
}
#endif
