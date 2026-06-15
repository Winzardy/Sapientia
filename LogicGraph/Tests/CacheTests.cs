#if UNITY_5_4_OR_NEWER
using NUnit.Framework;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// 4F: <see cref="InstanceCache"/> — per-instance Cache (метаданные <see cref="DataCache"/> + значения раздельно):
	/// write/read + мемоизация Is-Calculated + reset + резолв passthrough-link. Два off-allocator-блока
	/// (<see cref="InstanceCache.Create"/>/<see cref="InstanceCache.Dispose"/>); позиционно-независима.
	/// </summary>
	public class CacheTests
	{
		private static int CellSize => TSize<DataCache>.size;

		/// <summary>Хендл ячейки <paramref name="index"/>: метаданные по index*CellSize, значение по index*8 (long).</summary>
		private static CacheHandler<long> H(int index)
		{
			return new CacheHandler<long>
			{
				cell = new PtrOffset<DataCache>(index * CellSize),
				value = new PtrOffset(index * sizeof(long)),
			};
		}

		private static InstanceCache Create(int cellCount)
		{
			return InstanceCache.Create(default, cellCount, cellCount * sizeof(long));
		}

		[Test]
		public void Cache_WriteReadRoundtrip()
		{
			var cache = Create(1);
			try
			{
				var h = H(0);
				Assert.IsFalse(cache.Read(h, out _), "Свежая ячейка — не посчитана.");
				cache.Write(h, 42L);
				Assert.IsTrue(cache.Read(h, out var v));
				Assert.AreEqual(42L, v);
			}
			finally
			{
				cache.Dispose();
			}
		}

		[Test]
		public void Cache_IsCalculated()
		{
			var cache = Create(1);
			try
			{
				var h = H(0);
				Assert.IsFalse(cache.IsCalculated(h), "Uninitialized — не посчитано.");
				cache.Write(h, 1L);
				Assert.IsTrue(cache.IsCalculated(h), "После Write — посчитано.");
			}
			finally
			{
				cache.Dispose();
			}
		}

		[Test]
		public void Cache_ResetClears()
		{
			var cache = Create(1);
			try
			{
				var h = H(0);
				cache.Write(h, 7L);
				Assert.IsTrue(cache.Read(h, out _));

				cache.Reset();
				Assert.IsFalse(cache.Read(h, out _), "После Reset ячейка снова Uninitialized.");
			}
			finally
			{
				cache.Dispose();
			}
		}

		[Test]
		public void Cache_CreateGivesCleanBlock()
		{
			// Массивы создаются с ClearMemory ⇒ свежие ячейки — Uninitialized (state 0), без мусора.
			var cache = Create(2);
			try
			{
				Assert.IsFalse(cache.Read(H(0), out _), "Свежий блок — все ячейки Uninitialized.");
				Assert.IsFalse(cache.Read(H(1), out _));
			}
			finally
			{
				cache.Dispose();
			}
		}

		[Test]
		public void Cache_EmptyIsValidFalseAndResetNoop()
		{
			// cellCount <= 0 ⇒ пустой Cache (нода без Cache-портов): невалиден, Reset — no-op (без краша).
			var cache = InstanceCache.Create(default, 0, 0);
			Assert.IsFalse(cache.IsValid, "Пустой Cache невалиден.");
			cache.Reset();   // не должно бросать
			cache.Dispose(); // идемпотентно
		}

		[Test]
		public void Cache_NeighborIsolationAndResetAll()
		{
			var cache = Create(2);
			try
			{
				cache.Write(H(0), 1L);
				cache.Write(H(1), 2L);
				Assert.IsTrue(cache.Read(H(0), out var a));
				Assert.IsTrue(cache.Read(H(1), out var b));
				Assert.AreEqual(1L, a, "Соседняя ячейка не должна повлиять.");
				Assert.AreEqual(2L, b);

				cache.Reset();
				Assert.IsFalse(cache.Read(H(0), out _), "Reset чистит все ячейки.");
				Assert.IsFalse(cache.Read(H(1), out _));
			}
			finally
			{
				cache.Dispose();
			}
		}

		[Test]
		public void Cache_LinkPassthrough()
		{
			var cache = Create(2);
			try
			{
				// A → link → B(Value=7).
				cache.Write(H(1), 7L);
				cache.WriteLink(H(0), H(1));

				Assert.IsTrue(cache.Read(H(0), out var v), "Link должен резолвиться в значение.");
				Assert.AreEqual(7L, v);
			}
			finally
			{
				cache.Dispose();
			}
		}

		[Test]
		public void Cache_LinkChain()
		{
			var cache = Create(3);
			try
			{
				// A → B → C(Value=9).
				cache.Write(H(2), 9L);
				cache.WriteLink(H(1), H(2));
				cache.WriteLink(H(0), H(1));

				Assert.IsTrue(cache.Read(H(0), out var v), "Цепочка link'ов должна резолвиться.");
				Assert.AreEqual(9L, v);
			}
			finally
			{
				cache.Dispose();
			}
		}

		[Test]
		public void Cache_InReadsSourceOut()
		{
			// In и Out, указывающие на одну ячейку (In читает значение источника — как в Static.Map).
			var cache = Create(1);
			try
			{
				var outPort = new NodeOut<long> { output = H(0) };
				var inPort = new NodeIn<long> { input = H(0) };

				Assert.IsFalse(inPort.Read(ref cache, out _), "До записи Out вход не посчитан.");
				outPort.Write(ref cache, 5L);
				Assert.IsTrue(inPort.Read(ref cache, out var v));
				Assert.AreEqual(5L, v);
			}
			finally
			{
				cache.Dispose();
			}
		}
	}
}
#endif
