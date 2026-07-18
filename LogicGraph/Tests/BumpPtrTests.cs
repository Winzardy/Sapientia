#if UNITY_5_4_OR_NEWER
using NUnit.Framework;
using Sapientia.Memory;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// Тесты <see cref="BumpPtr{T}"/> — копируемого рантайм-хэндла на значение в bump-арене.
	/// В отличие от self-relative контейнеров (<see cref="BumpArray{T}"/>) копия хэндла легальна
	/// и указывает на те же данные.
	/// </summary>
	public class BumpPtrTests
	{
		[Test]
		public void Default_IsInvalid()
		{
			BumpPtr<int> ptr = default;

			Assert.IsFalse(ptr.IsValid, "Default-хэндл обязан быть невалидным.");
		}

		[Test]
		public void GetBumpPtr_RoundTripsValue()
		{
			var arena = new RawBumpAllocator(256);
			try
			{
				ref var slot = ref arena.Value.MemAlloc<int>(out var offset);
				slot = 42;

				var ptr = arena.GetBumpPtr(offset);
				Assert.IsTrue(ptr.IsValid, "Хэндл на живую аллокацию обязан быть валидным.");
				Assert.AreEqual(42, ptr.Value, "Хэндл прочитал не то значение.");
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void Copy_PointsToSameData()
		{
			var arena = new RawBumpAllocator(256);
			try
			{
				ref var slot = ref arena.Value.MemAlloc<int>(out var offset);
				slot = 1;

				var ptr = arena.GetBumpPtr(offset);
				var copy = ptr;
				ptr.Value = 2;

				Assert.AreEqual(2, copy.Value, "Копия хэндла обязана видеть те же данные (это указатель, не значение).");
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void GetRootPtr_ResolvesFirstAllocation()
		{
			var arena = new RawBumpAllocator(256);
			try
			{
				ref var root = ref arena.Value.MemAlloc<long>(out _);
				root = 777;

				Assert.AreEqual(777, arena.GetRootPtr<long>().Value,
					"GetRootPtr обязан указывать на первую аллокацию сразу за заголовком.");
			}
			finally
			{
				arena.Dispose();
			}
		}
	}
}
#endif
