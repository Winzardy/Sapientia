#if UNITY_5_4_OR_NEWER
using NUnit.Framework;
using Sapientia.Memory;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// Static-топология (<see cref="NodeMapHeader"/>): на ноду ноды-предшественники/потомки
	/// (<see cref="NodeRelativesHeader"/>, дедуп по ноде) + корни (<c>inDegree == 0</c>). Рёбра строятся из
	/// связей (<see cref="Blueprint.inputToOutput"/>); константы (Out без ноды-владельца) рёбер не создают.
	/// Substrate под батч-шедулинг (4B). Паттерн как в <c>MapTests</c>: <c>CompileLayout</c> → <c>ref compiled</c>
	/// → assert → <c>arena.Dispose()</c> в <c>finally</c>.
	/// </summary>
	public class NodeMapTests
	{
		/// <summary>Константа: precalculated-Out без ноды-владельца (источник In, рёбер не создаёт).</summary>
		private sealed class ConstOutput<T> : NodeOutput<T> where T : unmanaged
		{
			public override bool IsPreCalculated => true;
		}

		/// <summary>Есть ли в массиве соседей нода <paramref name="nodeId"/> (резолв на месте, через ref).</summary>
		private static bool Contains(ref BumpArray<Id<NodeHeader>> nodes, int nodeId)
		{
			for (var i = 0; i < nodes.Length; i++)
			{
				if ((int)nodes.Get(i) == nodeId)
					return true;
			}
			return false;
		}

		private static bool StartNodesContain(ref CompiledBlueprintHeader compiled, int nodeId)
		{
			for (var i = 0; i < compiled.StartNodeCount; i++)
			{
				if ((int)compiled.GetStartNode(i) == nodeId)
					return true;
			}
			return false;
		}

		[Test]
		public void NodeMap_LinearChain()
		{
			// A(0) -> B(1) -> C(2).
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

			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);

				Assert.AreEqual(0, compiled.GetNodeInDegree(0), "A — корень.");
				Assert.AreEqual(1, compiled.GetNodeInDegree(1), "B зависит от A.");
				Assert.AreEqual(1, compiled.GetNodeInDegree(2), "C зависит от B.");

				Assert.AreEqual(1, compiled.StartNodeCount, "Корень один — A.");
				Assert.IsTrue(StartNodesContain(ref compiled, 0));

				ref var relB = ref compiled.GetNodeRelatives(1);
				Assert.AreEqual(1, relB.inputs.Length, "preds(B) ровно один.");
				Assert.AreEqual(1, relB.outputs.Length, "succs(B) ровно один (промежуточная нода: и предок, и потомок).");
				Assert.IsTrue(Contains(ref relB.inputs, 0), "preds(B) = [A].");
				Assert.IsTrue(Contains(ref relB.outputs, 2), "succs(B) = [C].");
				ref var relA = ref compiled.GetNodeRelatives(0);
				Assert.AreEqual(1, relA.outputs.Length, "succs(A) ровно один (лишний потомок не должен появиться).");
				Assert.IsTrue(Contains(ref relA.outputs, 1), "succs(A) = [B].");
				Assert.AreEqual(0, compiled.GetNodeRelatives(0).inputs.Length, "У A нет предшественников.");
				Assert.AreEqual(0, compiled.GetNodeRelatives(2).outputs.Length, "У C нет потомков.");
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void NodeMap_Diamond()
		{
			// A(0) -> B(1), A -> C(2); B -> D(3), C -> D.
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

			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);

				Assert.AreEqual(0, compiled.GetNodeInDegree(0), "A — корень.");
				Assert.AreEqual(2, compiled.GetNodeInDegree(3), "D зависит от B и C (join, inDegree 2).");

				ref var relA = ref compiled.GetNodeRelatives(0);
				Assert.AreEqual(2, relA.outputs.Length, "succs(A) = [B, C].");
				Assert.IsTrue(Contains(ref relA.outputs, 1));
				Assert.IsTrue(Contains(ref relA.outputs, 2));

				ref var relD = ref compiled.GetNodeRelatives(3);
				Assert.IsTrue(Contains(ref relD.inputs, 1));
				Assert.IsTrue(Contains(ref relD.inputs, 2));

				Assert.AreEqual(1, compiled.StartNodeCount);
				Assert.IsTrue(StartNodesContain(ref compiled, 0));
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void NodeMap_ParallelIndependent()
		{
			// Три несвязанные ноды — все корни, рёбер нет.
			var bp = StubBlueprint.Of(new StubNode(), new StubNode(), new StubNode());

			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);

				Assert.AreEqual(3, compiled.StartNodeCount, "Все три — корни.");
				for (var i = 0; i < 3; i++)
				{
					Assert.AreEqual(0, compiled.GetNodeInDegree(i));
					Assert.AreEqual(0, compiled.GetNodeRelatives(i).outputs.Length);
					Assert.IsTrue(StartNodesContain(ref compiled, i));
				}
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void NodeMap_DuplicateEdgeDeduped()
		{
			// Две связи B<-A через два In'а одной ноды-источника A -> одно ребро (inDegree 1).
			var aOut1 = new NodeOutput<long>();
			var aOut2 = new NodeOutput<long>();
			var bIn1 = new NodeInput<long>();
			var bIn2 = new NodeInput<long>();
			var bp = StubBlueprint.Of(
				new StubNode(cacheSize: 32, outputs: new NodeOutput[] { aOut1, aOut2 }),
				new StubNode(inputs: new NodeInput[] { bIn1, bIn2 }));
			bp.inputToOutput[bIn1] = aOut1;
			bp.inputToOutput[bIn2] = aOut2;

			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);

				Assert.AreEqual(1, compiled.GetNodeInDegree(1), "Дубль ребра в одну ноду должен схлопнуться.");
				Assert.AreEqual(1, compiled.GetNodeRelatives(1).inputs.Length);
				Assert.AreEqual(1, compiled.GetNodeRelatives(0).outputs.Length, "succs(A) = [B] (один раз).");
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void NodeMap_ConstantSourceIsRoot()
		{
			// In от константы (нет ноды-владельца) -> нода без предшественников (корень).
			var constOut = new ConstOutput<long>();
			var aIn = new NodeInput<long>();
			var bp = StubBlueprint.Of(new StubNode(staticSize: 8, inputs: new NodeInput[] { aIn }));
			bp.outputs = new NodeOutput[] { constOut };
			bp.inputToOutput[aIn] = constOut;

			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);

				Assert.AreEqual(0, compiled.GetNodeInDegree(0), "Константа зависимости не создаёт.");
				Assert.AreEqual(1, compiled.StartNodeCount);
				Assert.IsTrue(StartNodesContain(ref compiled, 0));
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void NodeMap_PrecalculatedSourceNoEdge()
		{
			// Источник — precalculated-Out, ПРИНАДЛЕЖАЩИЙ ноде A (в её GetOutputs). Значение забейкано в Static
			// → B не ждёт исполнения A → ребра нет, оба корня. Критерий «готового» = IsPreCalculated, как в Map.
			var constOut = new ConstOutput<long>();
			var bIn = new NodeInput<long>();
			var bp = StubBlueprint.Of(
				new StubNode(staticSize: 8, outputs: new NodeOutput[] { constOut }),
				new StubNode(inputs: new NodeInput[] { bIn }));
			bp.inputToOutput[bIn] = constOut;

			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);

				Assert.AreEqual(0, compiled.GetNodeInDegree(1), "Precalculated-источник зависимости не создаёт.");
				Assert.AreEqual(0, compiled.GetNodeRelatives(0).outputs.Length, "У A не должно появиться ребро на B.");
				Assert.AreEqual(2, compiled.StartNodeCount, "Обе ноды — корни.");
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void NodeMap_SelfLoopIgnored()
		{
			// Нода читает собственный Out (вырожденный случай) — ни ребра, ни inDegree; нода остаётся корнем.
			var aOut = new NodeOutput<long>();
			var aIn = new NodeInput<long>();
			var bp = StubBlueprint.Of(
				new StubNode(cacheSize: 16, inputs: new NodeInput[] { aIn }, outputs: new NodeOutput[] { aOut }));
			bp.inputToOutput[aIn] = aOut;

			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);

				Assert.AreEqual(0, compiled.GetNodeInDegree(0), "Самопетля не создаёт зависимости.");
				Assert.AreEqual(0, compiled.GetNodeRelatives(0).outputs.Length, "Самопетля не создаёт потомка.");
				Assert.AreEqual(1, compiled.StartNodeCount);
				Assert.IsTrue(StartNodesContain(ref compiled, 0));
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void NodeMap_EmptyBlueprintLockstep()
		{
			// nodeCount == 0: ранний выход; nodeMap не аллоцируется; lockstep и StartNodeCount чисты.
			var bp = StubBlueprint.Of();
			AssertLockstep(bp);

			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				Assert.AreEqual(0, compiled.StartNodeCount, "Пустой блюпринт — нет корней.");
				Assert.AreEqual(0, compiled.NodesCount);
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void NodeMap_LockstepWithNodeMap()
		{
			// Резерв == bump на графах с топологией (цепочка, ромб, параллель, дедуп, константа).
			var aOut = new NodeOutput<long>();
			var bIn = new NodeInput<long>();
			var chain = StubBlueprint.Of(
				new StubNode(cacheSize: 16, outputs: new NodeOutput[] { aOut }),
				new StubNode(inputs: new NodeInput[] { bIn }));
			chain.inputToOutput[bIn] = aOut;
			AssertLockstep(chain);

			AssertLockstep(StubBlueprint.Of(new StubNode(), new StubNode(), new StubNode()));

			var cOut = new NodeOutput<long>();
			var dInA = new NodeInput<long>();
			var dInB = new NodeInput<long>();
			var dOut = new NodeOutput<long>();
			var eIn = new NodeInput<long>();
			var diamondLike = StubBlueprint.Of(
				new StubNode(cacheSize: 16, outputs: new NodeOutput[] { cOut }),
				new StubNode(cacheSize: 16, inputs: new NodeInput[] { dInA }, outputs: new NodeOutput[] { dOut }),
				new StubNode(inputs: new NodeInput[] { dInB, eIn }));
			diamondLike.inputToOutput[dInA] = cOut;
			diamondLike.inputToOutput[dInB] = cOut;
			diamondLike.inputToOutput[eIn] = dOut;
			AssertLockstep(diamondLike);

			// Precalculated-источник (ребра нет) + нода-владелец константы.
			var constOut = new ConstOutput<long>();
			var fIn = new NodeInput<long>();
			var withConst = StubBlueprint.Of(
				new StubNode(staticSize: 8, outputs: new NodeOutput[] { constOut }),
				new StubNode(inputs: new NodeInput[] { fIn }));
			withConst.inputToOutput[fIn] = constOut;
			AssertLockstep(withConst);
		}

		private static void AssertLockstep(Blueprint bp)
		{
			var reserve = BlueprintCompiler.CalculateLayoutSizeToReserve(bp);
			var arena = BlueprintCompiler.CompileLayout(bp, out _);
			try
			{
				var used = arena.Value.UsedBytes - BumpHeader.HeaderSize;
				Assert.AreEqual(reserve, used, $"Резерв раскладки с nodeMap разошёлся с фактическим bump (нод: {bp.nodes.Length}).");
			}
			finally
			{
				arena.Dispose();
			}
		}
	}
}
#endif
