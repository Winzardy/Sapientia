#if UNITY_5_4_OR_NEWER
using NUnit.Framework;
using Sapientia.Extensions;
using Sapientia.Memory;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// Фаза 2: раскладка 5 областей в скомпилированном блюпринте (sizing-only, stub-ноды).
	/// Проверяют размеры блоков, выравнивание/non-overlap офсетов, инвариант lockstep
	/// (расчётный резерв == фактический bump), чистоту zero-size и адресуемость static-слайсов.
	/// </summary>
	public class LayoutTests
	{
		[Test]
		public void Layout_PerNodeSizesSumToBlockSizes()
		{
			// Все размеры кратны DataSizes.Alignment(8) — сумма сырых размеров == размер блока.
			var bp = StubBlueprint.Of(
				new StubNode(staticSize: 16, staticCacheSize: 8, staticPersistentSize: 24, instanceCacheSize: 8, instancePersistentSize: 16),
				new StubNode(staticSize: 8, staticCacheSize: 16, staticPersistentSize: 8, instanceCacheSize: 24, instancePersistentSize: 8));
			var arena = CompiledBlueprint.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				for (var s = 0; s < DataSizes.Count; s++)
				{
					var scope = s.ToEnum<DataLayout>();
					var expected = 0;
					foreach (var node in bp.nodes)
						expected += node.DataSizes[scope];
					Assert.AreEqual(expected, compiled.GetBlockSize(scope), $"Блок {scope} не равен сумме размеров нод.");
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
				new StubNode(staticSize: 4, staticCacheSize: 12, staticPersistentSize: 1, instanceCacheSize: 0, instancePersistentSize: 32),
				new StubNode(staticSize: 8, staticCacheSize: 4, staticPersistentSize: 16, instanceCacheSize: 8, instancePersistentSize: 1),
				new StubNode(staticSize: 1, staticCacheSize: 0, staticPersistentSize: 7, instanceCacheSize: 24, instancePersistentSize: 8));
			var arena = CompiledBlueprint.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				for (var s = 0; s < DataSizes.Count; s++)
				{
					var scope = s.ToEnum<DataLayout>();
					var prevEnd = 0;
					for (var n = 0; n < bp.nodes.Length; n++)
					{
						var off = compiled.GetNodeOffset(n, scope).byteOffset;
						var rawSize = bp.nodes[n].DataSizes[scope];

						Assert.AreEqual(0, off % DataSizes.Alignment, $"Офсет ноды {n} области {scope} не выровнен.");
						Assert.GreaterOrEqual(off, prevEnd, $"Офсет ноды {n} области {scope} перекрывает предыдущую.");
						Assert.LessOrEqual(off + rawSize, compiled.GetBlockSize(scope), $"Слайс ноды {n} области {scope} выходит за блок.");

						prevEnd = off + rawSize.AlignUp(DataSizes.Alignment);
					}
					Assert.AreEqual(prevEnd, compiled.GetBlockSize(scope), $"Сумма слотов области {scope} != размер блока.");
				}
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void Layout_AlignmentPadsSlots()
		{
			// Не кратные 8 размеры -> блок == сумма AlignUp(size); офсеты идут по выровненным слотам.
			var bp = StubBlueprint.Of(
				new StubNode(staticSize: 1),  // слот 8
				new StubNode(staticSize: 9),  // слот 16
				new StubNode(staticSize: 8)); // слот 8
			var arena = CompiledBlueprint.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				Assert.AreEqual(8 + 16 + 8, compiled.GetBlockSize(DataLayout.Static));
				Assert.AreEqual(0, compiled.GetNodeOffset(0, DataLayout.Static).byteOffset);
				Assert.AreEqual(8, compiled.GetNodeOffset(1, DataLayout.Static).byteOffset);
				Assert.AreEqual(24, compiled.GetNodeOffset(2, DataLayout.Static).byteOffset);
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
				new StubNode(staticSize: 16, staticCacheSize: 8, instancePersistentSize: 24),
				new StubNode(staticSize: 8, instanceCacheSize: 16)));
			// static-блок нулевой, но другие области заняты (таблица офсетов есть, static-блок не выделяется).
			AssertLockstep(StubBlueprint.Of(
				new StubNode(staticCacheSize: 8, instanceCacheSize: 16, instancePersistentSize: 8),
				new StubNode(instancePersistentSize: 24)));
			// Все области нулевые, но ноды есть.
			AssertLockstep(StubBlueprint.Of(new StubNode(), new StubNode()));
			// Нод нет вовсе (только структура).
			AssertLockstep(StubBlueprint.Of(System.Array.Empty<INode>()));
			// Только static.
			AssertLockstep(StubBlueprint.Of(new StubNode(staticSize: 1)));
		}

		private static void AssertLockstep(Blueprint bp)
		{
			var reserve = CompiledBlueprint.CalculateLayoutSizeToReserve(bp);
			var arena = CompiledBlueprint.CompileLayout(bp, out _);
			try
			{
				var used = arena.Value.UsedBytes - BumpHeader.HeaderSize;
				Assert.AreEqual(reserve, used, $"Резерв scope-раскладки разошёлся с фактическим bump (нод: {bp.nodes.Length}).");
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void Layout_ZeroSizeNodesLayoutCleanly()
		{
			// Все области нулевые: блоки нулевые, static-блок не выделяется, без ассертов MemAlloc.
			var bp = StubBlueprint.Of(new StubNode(), new StubNode());

			var reserve = CompiledBlueprint.CalculateLayoutSizeToReserve(bp);
			var arena = CompiledBlueprint.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				for (var s = 0; s < DataSizes.Count; s++)
				{
					var scope = s.ToEnum<DataLayout>();
					Assert.AreEqual(0, compiled.GetBlockSize(scope));
					Assert.AreEqual(0, compiled.GetNodeOffset(0, scope).byteOffset);
					Assert.AreEqual(0, compiled.GetNodeOffset(1, scope).byteOffset);
				}
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
			var arena = CompiledBlueprint.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);

				// Пишем уникальное значение в начало слайса каждой ноды.
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
