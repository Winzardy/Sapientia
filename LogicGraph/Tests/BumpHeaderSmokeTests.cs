using NUnit.Framework;
using Sapientia.Memory;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// Smoke-тесты Фазы 0: проверяют, что EditMode-harness обнаруживает и запускает тесты LogicGraph,
	/// и что <see cref="ArenaAllocator"/>, на котором строятся последующие фазы, корректно
	/// проходит запись и чтение значения насквозь.
	/// </summary>
	public class ArenaAllocatorSmokeTests
	{
		[Test]
		public void Harness_Runs()
		{
			// Если Test Runner обнаружил и выполнил эту сборку, значит harness работает.
			Assert.Pass("Harness Sapientia.LogicGraph.Tests подключён и работает.");
		}

		[Test]
		public void Arena_RoundTripsOneInt()
		{
			var allocatorPtr = ArenaAllocator.Create(1024);
			try
			{
				ref var arena = ref allocatorPtr.Value();

				ref var slot = ref arena.MemAlloc<int>(out var offset);
				slot = 42;

				Assert.AreEqual(42, arena.GetRef(offset), "Значение, записанное по смещению в арене, не прочиталось обратно.");
			}
			finally
			{
				allocatorPtr.Value().Dispose();
			}
		}
	}
}
