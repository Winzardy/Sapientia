#if UNITY_5_4_OR_NEWER
using NUnit.Framework;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.Memory;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// Static.Map: на ноду блок In/Out (массив <see cref="RegionPtr"/>: In'ы, затем Out'ы), на который указывает
	/// <see cref="NodeHeader.inOut"/> (офсет от позиции заголовка). Проверяют: вывод региона из типа порта
	/// (precalculated → Static, persistent → Persistence, иначе Cache), размещение Out'ов (Static — слайсы нод с
	/// бейком дефолта; Cache/Persistence — офсетом в блоке), что In указывает на данные источника, lockstep.
	/// </summary>
	public class MapTests
	{
		/// <summary>i-й указатель блока In/Out «на месте» (ref — Static резолвится из позиции слота).</summary>
		private static ref RegionPtr Port(SafePtr block, int index)
		{
			return ref (block + index * TSize<RegionPtr>.size).Value<RegionPtr>();
		}

		/// <summary>Константа: precalculated-Out без ноды-владельца (живёт в Static, дефолт бейкается).</summary>
		private sealed class ConstOutput<T> : NodeOutput<T> where T : unmanaged
		{
			private readonly T _value;

			public ConstOutput(T value)
			{
				_value = value;
			}

			public override bool IsPreCalculated => true;
			public override T DefaultValue => _value;
		}

		[Test]
		public void Map_OutRegionDerivedFromPortType()
		{
			var bp = StubBlueprint.Of(
				new StubNode(cacheSize: 8, outputs: new NodeOutput[] { new NodeOutput<long>() }),
				new StubNode(persistanceSize: 8, outputs: new NodeOutput[] { new NodeStateOutput<long>() }),
				new StubNode(staticSize: 8, outputs: new NodeOutput[] { new ConstOutput<long>(7) }));
			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);

				// Обычный Out → Cache; нода 0 — голова Cache-блока (офсет 0).
				ref var cacheOut = ref Port(compiled.GetNodeInOut(0), 0);
				Assert.AreEqual(MemoryRegion.Cache, cacheOut.region);
				Assert.AreEqual(0, cacheOut.data.byteOffset);

				// NodeStateOutput → Persistence.
				ref var persistentOut = ref Port(compiled.GetNodeInOut(1), 0);
				Assert.AreEqual(MemoryRegion.Persistence, persistentOut.region);
				Assert.AreEqual(compiled.GetNodePersistenceOffset(1).byteOffset, persistentOut.data.byteOffset);

				// Precalculated-Out → Static; дефолт забейкан, резолв на месте.
				ref var staticOut = ref Port(compiled.GetNodeInOut(2), 0);
				Assert.AreEqual(MemoryRegion.Static, staticOut.region);
				Assert.AreEqual(7, ((SafePtr)staticOut.data.GetPtr()).Value<long>(), "Дефолт Static-Out не забейкался.");
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void Map_MultipleOutsStackAlignedWithinSlice()
		{
			// Два Cache-out'а одной ноды: офсеты от головы слайса, шаг выровнен (long → 8, int → слот 8).
			var bp = StubBlueprint.Of(
				new StubNode(cacheSize: 24, outputs: new NodeOutput[] { new NodeOutput<long>(), new NodeOutput<int>() }));
			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				var block = compiled.GetNodeInOut(0);
				var sliceStart = 0; // нода 0 — голова Cache-блока

				ref var o0 = ref Port(block, 0);
				ref var o1 = ref Port(block, 1);
				Assert.AreEqual(sliceStart, o0.data.byteOffset);
				Assert.AreEqual(sliceStart + 8, o1.data.byteOffset, "Второй Out должен лечь за выровненным слотом первого.");
				Assert.AreEqual(MemoryRegion.Cache, o1.region);
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void Map_InPointsAtSourceOut()
		{
			// Cache-источник: In и Out (его источник) указывают в один и тот же офсет блока Cache.
			var aOut = new NodeOutput<long>();
			var bIn = new NodeInput<long>();
			var bp = StubBlueprint.Of(
				new StubNode(cacheSize: 8, outputs: new NodeOutput[] { aOut }),
				new StubNode(inputs: new NodeInput[] { bIn }));
			bp.inputToOutput[bIn] = aOut;

			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				ref var source = ref Port(compiled.GetNodeInOut(0), 0); // Out ноды 0
				ref var input = ref Port(compiled.GetNodeInOut(1), 0);  // In ноды 1

				Assert.AreEqual(source.region, input.region, "In должен читать из региона источника.");
				Assert.AreEqual(source.data.byteOffset, input.data.byteOffset, "In должен указывать на данные источника.");
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void Map_StaticInReadsSourceConstant()
		{
			// Static-источник (константа): In резолвится на месте и читает забейканный дефолт.
			var constOut = new ConstOutput<long>(42);
			var bIn = new NodeInput<long>();
			var bp = StubBlueprint.Of(new StubNode(staticSize: 8, inputs: new NodeInput[] { bIn }));
			bp.outputs = new NodeOutput[] { constOut };
			bp.inputToOutput[bIn] = constOut;

			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);

				// blockSizes[Static] = слайс ноды (8) + константа (8).
				Assert.AreEqual(16, compiled.GetBlockSize(MemoryRegion.Static), "Static-байты должны включать константу.");

				ref var input = ref Port(compiled.GetNodeInOut(0), 0);
				Assert.AreEqual(MemoryRegion.Static, input.region, "Константа должна жить в Static.");
				Assert.AreEqual(42, ((SafePtr)input.data.GetPtr()).Value<long>(), "Дефолт константы не забейкался / In не указывает на неё.");
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void Map_LockstepWithPorts()
		{
			// Lockstep (резерв == bump) с блоками In/Out: связный граф, константы, ноды без портов.
			var aOut = new NodeOutput<long>();
			var bIn = new NodeInput<long>();
			var connected = StubBlueprint.Of(
				new StubNode(cacheSize: 8, outputs: new NodeOutput[] { aOut }),
				new StubNode(inputs: new NodeInput[] { bIn }),
				new StubNode());
			connected.inputToOutput[bIn] = aOut;
			AssertLockstep(connected);

			var cIn = new NodeInput<long>();
			var constOut = new ConstOutput<long>(1);
			var withConst = StubBlueprint.Of(new StubNode(inputs: new NodeInput[] { cIn }));
			withConst.outputs = new NodeOutput[] { constOut };
			withConst.inputToOutput[cIn] = constOut;
			AssertLockstep(withConst);
		}

		[Test]
		public void Map_NodeOwnedConstantNotDuplicated()
		{
			// blueprint.outputs — реестр ВСЕХ аутпутов: precalculated-Out ноды попадает и туда, и в GetOutputs().
			// Отдельная аллокация-константа не делается повторно (lockstep цел), дефолт забейкан в слайс ноды.
			var dOut = new ConstOutput<long>(5);
			var bp = StubBlueprint.Of(new StubNode(staticSize: 8, outputs: new NodeOutput[] { dOut }));
			bp.outputs = new NodeOutput[] { dOut };

			AssertLockstep(bp);

			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				Assert.AreEqual(8, compiled.GetBlockSize(MemoryRegion.Static), "Node-owned константа не должна дублироваться.");
				ref var output = ref Port(compiled.GetNodeInOut(0), 0);
				Assert.AreEqual(MemoryRegion.Static, output.region);
				Assert.AreEqual(5, ((SafePtr)output.data.GetPtr()).Value<long>());
			}
			finally
			{
				arena.Dispose();
			}
		}

		private static void AssertLockstep(Blueprint bp)
		{
			var reserve = BlueprintCompiler.CalculateLayoutSizeToReserve(bp);
			var arena = BlueprintCompiler.CompileLayout(bp, out _);
			try
			{
				var used = arena.Value.UsedBytes - BumpHeader.HeaderSize;
				Assert.AreEqual(reserve, used, $"Резерв раскладки с In/Out разошёлся с фактическим bump (нод: {bp.nodes.Length}).");
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void Map_NoPortsInOutInvalid()
		{
			var bp = StubBlueprint.Of(new StubNode(staticSize: 8), new StubNode());
			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				Assert.IsFalse(compiled.GetNodeInOut(0).IsValid, "Нода без портов — невалидный In/Out.");
				Assert.IsFalse(compiled.GetNodeInOut(1).IsValid);
			}
			finally
			{
				arena.Dispose();
			}
		}
	}
}
#endif
