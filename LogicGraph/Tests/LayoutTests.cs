#if UNITY_5_4_OR_NEWER
using NUnit.Framework;
using Sapientia.Extensions;
using Sapientia.Memory;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// Раскладка регионов в Static-заголовке (sizing-only, stub-ноды). Проверяют суммарные размеры блоков,
	/// выравнивание/non-overlap per-node офсетов <b>Persistence</b> (Static-слайсы — отдельные аллокации,
	/// адресуются напрямую; Cache per-node офсет на заголовке не хранится — ушёл в DataCache, проверяется только
	/// суммарный размер блока), lockstep (резерв == bump), чистоту zero-size.
	/// </summary>
	public class LayoutTests
	{

		[Test]
		public void Layout_PerNodeSizesSumToBlockSizes()
		{
			// Все размеры кратны DataSizes.Alignment(8) — сумма сырых размеров == размер блока.
			var bp = StubBlueprint.Of(
				new StubNode(staticSize: 16, cacheSize: 8, persistanceSize: 24),
				new StubNode(staticSize: 8, cacheSize: 16, persistanceSize: 8));
			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				for (var s = 0; s < DataSizes.Count; s++)
				{
					var region = s.ToEnum<MemoryRegion>();
					var expected = 0;
					foreach (var node in bp.nodes)
						expected += node.DataSizes[region];
					Assert.AreEqual(expected, compiled.GetBlockSize(region), $"Блок {region} не равен сумме размеров нод.");
				}
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void Layout_OffsetsAlignedAndNonOverlapping()
		{
			var bp = StubBlueprint.Of(
				new StubNode(staticSize: 4, cacheSize: 12, persistanceSize: 1),
				new StubNode(staticSize: 8, cacheSize: 4, persistanceSize: 16),
				new StubNode(staticSize: 1, cacheSize: 0, persistanceSize: 7));
			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				// Per-node офсеты хранятся только для Persistence (Cache — per-instance через DataCache).
				const MemoryRegion region = MemoryRegion.Persistence;
				var prevEnd = 0;
				for (var n = 0; n < bp.nodes.Length; n++)
				{
					var off = compiled.GetNodePersistenceOffset(n).byteOffset;
					var rawSize = bp.nodes[n].DataSizes[region];

					Assert.AreEqual(0, off % DataSizes.Alignment, $"Офсет ноды {n} региона {region} не выровнен.");
					Assert.GreaterOrEqual(off, prevEnd, $"Офсет ноды {n} региона {region} перекрывает предыдущую.");
					Assert.LessOrEqual(off + rawSize, compiled.GetBlockSize(region), $"Слайс ноды {n} региона {region} выходит за блок.");

					prevEnd = off + rawSize.AlignUp(DataSizes.Alignment);
				}
				Assert.AreEqual(prevEnd, compiled.GetBlockSize(region), $"Сумма слотов региона {region} != размер блока.");
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void Layout_AlignmentPadsSlots()
		{
			// Не кратные 8 размеры -> блок == сумма AlignUp(size); per-node офсеты Persistence идут по выровненным слотам.
			var bp = StubBlueprint.Of(
				new StubNode(persistanceSize: 1),  // слот 8
				new StubNode(persistanceSize: 9),  // слот 16
				new StubNode(persistanceSize: 8)); // слот 8
			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				Assert.AreEqual(8 + 16 + 8, compiled.GetBlockSize(MemoryRegion.Persistence));
				Assert.AreEqual(0, compiled.GetNodePersistenceOffset(0).byteOffset);
				Assert.AreEqual(8, compiled.GetNodePersistenceOffset(1).byteOffset);
				Assert.AreEqual(24, compiled.GetNodePersistenceOffset(2).byteOffset);
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void Layout_LockstepReserveEqualsBump()
		{
			// Покрываем асимметричные формы, где lockstep легче всего сломать (находки adversarial-review).
			AssertLockstep(StubBlueprint.Of(
				new StubNode(staticSize: 16, cacheSize: 8, persistanceSize: 24),
				new StubNode(staticSize: 8, cacheSize: 16)));
			AssertLockstep(StubBlueprint.Of(
				new StubNode(cacheSize: 8, persistanceSize: 8),
				new StubNode(persistanceSize: 24)));
			// Все регионы нулевые, но ноды есть.
			AssertLockstep(StubBlueprint.Of(new StubNode(), new StubNode()));
			// Нод нет вовсе (только структура).
			AssertLockstep(StubBlueprint.Of(System.Array.Empty<INode>()));
			// Только Static (отдельный слайс).
			AssertLockstep(StubBlueprint.Of(new StubNode(staticSize: 1)));
		}

		private static void AssertLockstep(Blueprint bp)
		{
			var reserve = BlueprintCompiler.CalculateLayoutSizeToReserve(bp);
			var arena = BlueprintCompiler.CompileLayout(bp, out _);
			try
			{
				var used = arena.Value.UsedBytes - BumpHeader.HeaderSize;
				Assert.AreEqual(reserve, used, $"Резерв раскладки разошёлся с фактическим bump (нод: {bp.nodes.Length}).");
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void Layout_ZeroSizeNodesLayoutCleanly()
		{
			// Все регионы нулевые: блоки нулевые, static-слайсы не выделяются, без ассертов MemAlloc.
			var bp = StubBlueprint.Of(new StubNode(), new StubNode());

			var reserve = BlueprintCompiler.CalculateLayoutSizeToReserve(bp);
			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				for (var s = 0; s < DataSizes.Count; s++)
					Assert.AreEqual(0, compiled.GetBlockSize(s.ToEnum<MemoryRegion>()));
				Assert.AreEqual(0, compiled.GetNodePersistenceOffset(0).byteOffset);
				Assert.AreEqual(0, compiled.GetNodePersistenceOffset(1).byteOffset);
				Assert.AreEqual(reserve, arena.Value.UsedBytes - BumpHeader.HeaderSize, "Lockstep сломан на zero-size графе.");
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void Layout_StaticSliceAddressableAndIsolated()
		{
			var bp = StubBlueprint.Of(
				new StubNode(staticSize: 8),
				new StubNode(staticSize: 8),
				new StubNode(staticSize: 8));
			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);

				// Пишем уникальное значение в начало слайса каждой ноды (прямой self-relative адрес).
				for (var n = 0; n < bp.nodes.Length; n++)
					compiled.GetStaticNodeSlice(n).Value<long>() = 1000 + n;

				// Читаем обратно — соседние слайсы не затёрты.
				for (var n = 0; n < bp.nodes.Length; n++)
					Assert.AreEqual(1000 + n, compiled.GetStaticNodeSlice(n).Value<long>(), $"Слайс ноды {n} затёрт/нечитаем.");
			}
			finally
			{
				arena.Dispose();
			}
		}
	}
}
#endif
