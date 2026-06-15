#if UNITY_5_4_OR_NEWER
using NUnit.Framework;
using Sapientia.Collections;
using Sapientia.Data;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// 4C-2: <see cref="CacheHeader"/> — per-instance Cache (ячейки <see cref="DataCache{T}"/>): write/read +
	/// мемоизация Is-Calculated + reset + резолв passthrough-link. Блок ячеек — off-alloc raw
	/// (<see cref="UnsafeArray{T}"/>); владелец инстанса (<c>ExecutionScope</c>) — 4F. <see cref="CacheHeader"/>
	/// держим в локальной переменной (self-relative: не копировать).
	/// </summary>
	public class CacheHeaderTests
	{
		private const int Cell = 16; // TSize<DataCache<long>> для T<=8

		private static CacheHandler<long> H(int cellIndex)
		{
			return new CacheHandler<long> { offset = new PtrOffset<DataCache<long>>(cellIndex * Cell) };
		}

		[Test]
		public void Cache_WriteReadRoundtrip()
		{
			var block = new UnsafeArray<byte>(default, Cell);
			try
			{
				CacheHeader header = default;
				header.Setup((SafePtr)block.ptr, Cell);

				var h = H(0);
				Assert.IsFalse(header.Read(h, out _), "Свежая ячейка — не посчитана.");
				header.Write(h, 42L);
				Assert.IsTrue(header.Read(h, out var v));
				Assert.AreEqual(42L, v);
			}
			finally
			{
				block.Dispose();
			}
		}

		[Test]
		public void Cache_IsCalculated()
		{
			var block = new UnsafeArray<byte>(default, Cell);
			try
			{
				CacheHeader header = default;
				header.Setup((SafePtr)block.ptr, Cell);

				var h = H(0);
				Assert.IsFalse(header.IsCalculated(h), "Uninitialized — не посчитано.");
				header.Write(h, 1L);
				Assert.IsTrue(header.IsCalculated(h), "После Write — посчитано.");
			}
			finally
			{
				block.Dispose();
			}
		}

		[Test]
		public void Cache_ResetClears()
		{
			var block = new UnsafeArray<byte>(default, Cell);
			try
			{
				CacheHeader header = default;
				header.Setup((SafePtr)block.ptr, Cell);

				var h = H(0);
				header.Write(h, 7L);
				Assert.IsTrue(header.Read(h, out _));

				header.Reset();
				Assert.IsFalse(header.Read(h, out _), "После Reset ячейка снова Uninitialized.");
			}
			finally
			{
				block.Dispose();
			}
		}

		[Test]
		public void Cache_SetupClearsDirtyBlock()
		{
			// Доказывает, что чистит именно Setup (а не zero-init аллокатора): пишем значение, затем re-Setup.
			var block = new UnsafeArray<byte>(default, Cell);
			try
			{
				CacheHeader header = default;
				header.Setup((SafePtr)block.ptr, Cell);
				header.Write(H(0), 9L);
				Assert.IsTrue(header.Read(H(0), out _));

				header.Setup((SafePtr)block.ptr, Cell); // повторный Setup обнуляет блок
				Assert.IsFalse(header.Read(H(0), out _), "Setup должен обнулить блок.");
			}
			finally
			{
				block.Dispose();
			}
		}

		[Test]
		public void Cache_NeighborIsolationAndResetAll()
		{
			var block = new UnsafeArray<byte>(default, 2 * Cell);
			try
			{
				CacheHeader header = default;
				header.Setup((SafePtr)block.ptr, 2 * Cell);

				header.Write(H(0), 1L);
				header.Write(H(1), 2L);
				Assert.IsTrue(header.Read(H(0), out var a));
				Assert.IsTrue(header.Read(H(1), out var b));
				Assert.AreEqual(1L, a, "Соседняя ячейка не должна повлиять.");
				Assert.AreEqual(2L, b);

				header.Reset();
				Assert.IsFalse(header.Read(H(0), out _), "Reset чистит все ячейки.");
				Assert.IsFalse(header.Read(H(1), out _));
			}
			finally
			{
				block.Dispose();
			}
		}

		[Test]
		public void Cache_LinkPassthrough()
		{
			var block = new UnsafeArray<byte>(default, 2 * Cell);
			try
			{
				CacheHeader header = default;
				header.Setup((SafePtr)block.ptr, 2 * Cell);

				// A → link → B(Value=7).
				header.Write(H(1), 7L);
				header.WriteLink(H(0), H(1));

				Assert.IsTrue(header.Read(H(0), out var v), "Link должен резолвиться в значение.");
				Assert.AreEqual(7L, v);
			}
			finally
			{
				block.Dispose();
			}
		}

		[Test]
		public void Cache_LinkChain()
		{
			var block = new UnsafeArray<byte>(default, 3 * Cell);
			try
			{
				CacheHeader header = default;
				header.Setup((SafePtr)block.ptr, 3 * Cell);

				// A → B → C(Value=9).
				header.Write(H(2), 9L);
				header.WriteLink(H(1), H(2));
				header.WriteLink(H(0), H(1));

				Assert.IsTrue(header.Read(H(0), out var v), "Цепочка link'ов должна резолвиться.");
				Assert.AreEqual(9L, v);
			}
			finally
			{
				block.Dispose();
			}
		}

		[Test]
		public void Cache_InReadsSourceOut()
		{
			// In и Out, указывающие на одну ячейку (In читает значение источника — как в Static.Map).
			var block = new UnsafeArray<byte>(default, Cell);
			try
			{
				CacheHeader header = default;
				header.Setup((SafePtr)block.ptr, Cell);

				var outPort = new NodeOut<long> { output = H(0) };
				var inPort = new NodeIn<long> { input = H(0) };

				Assert.IsFalse(inPort.Read(ref header, out _), "До записи Out вход не посчитан.");
				outPort.Write(ref header, 5L);
				Assert.IsTrue(inPort.Read(ref header, out var v));
				Assert.AreEqual(5L, v);
			}
			finally
			{
				block.Dispose();
			}
		}
	}
}
#endif
