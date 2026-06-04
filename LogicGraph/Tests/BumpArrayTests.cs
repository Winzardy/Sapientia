#if UNITY_5_4_OR_NEWER
using NUnit.Framework;
using Sapientia.Data;
using Sapientia.Memory;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// Тесты коллекции <see cref="BumpArray{T}"/> (массив в bump-арене):
	/// round-trip элементов, GetSpan и чистота пустого массива.
	/// </summary>
	public class BumpArrayTests
	{
		// Вспомогательная структура-обёртка: BumpArray должен жить в арене (self-relative allocatorOffset).
		private struct ArrayHolder<T> where T : unmanaged
		{
			public BumpArray<T> array;
		}

		private static PtrOffset<ArrayHolder<T>> AllocInArena<T>(ref BumpHeader allocator, int length)
			where T : unmanaged
		{
			var holderOffset = allocator.MemAlloc<ArrayHolder<T>>();
			ref var holder = ref allocator.GetValue(holderOffset);
			holder.array.Alloc(ref allocator, length); // in-place: self-relative offset от финального адреса поля
			return holderOffset;
		}

		[Test]
		public void BumpArray_RoundTripsElements()
		{
			var arena = new RawBumpAllocator(256);
			try
			{
				ref var allocator = ref arena.Value;
				var holderOffset = AllocInArena<int>(ref allocator, 4);
				ref var array = ref allocator.GetValue(holderOffset).array;

				Assert.IsTrue(array.IsValid, "Непустой массив должен быть валиден.");
				Assert.AreEqual(4, array.Length);

				for (var i = 0; i < array.Length; i++)
					array.Get(i) = i * 10;
				for (var i = 0; i < array.Length; i++)
					Assert.AreEqual(i * 10, array.Get(i), $"Элемент {i} не прочитался обратно.");
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void BumpArray_GetSpanRoundTrips()
		{
			var arena = new RawBumpAllocator(256);
			try
			{
				ref var allocator = ref arena.Value;
				var holderOffset = AllocInArena<int>(ref allocator, 3);
				ref var array = ref allocator.GetValue(holderOffset).array;

				var span = array.GetSpan();
				span[0] = 10;
				span[1] = 20;
				span[2] = 30;

				Assert.AreEqual(10, array.Get(0));
				Assert.AreEqual(20, array.Get(1));
				Assert.AreEqual(30, array.Get(2));
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void BumpArray_EmptyIsInvalidAndZeroLength()
		{
			var arena = new RawBumpAllocator(256);
			try
			{
				ref var allocator = ref arena.Value;
				var holderOffset = AllocInArena<int>(ref allocator, 0);
				ref var array = ref allocator.GetValue(holderOffset).array;

				Assert.IsFalse(array.IsValid, "Пустой массив не должен быть валиден.");
				Assert.AreEqual(0, array.Length);
			}
			finally
			{
				arena.Dispose();
			}
		}
	}
}
#endif
