using NUnit.Framework;
using Sapientia.Memory;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// Smoke-тесты Фазы 0: проверяют, что EditMode-harness обнаруживает и запускает тесты LogicGraph,
	/// и что <see cref="BumpHeader"/>, на котором строятся последующие фазы, корректно
	/// проходит запись и чтение значения насквозь.
	/// </summary>
	public class BumpHeaderSmokeTests
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
			var arena = RawBumpAllocator.Create(1024);
			try
			{
				ref var header = ref arena.Value;

				ref var slot = ref header.MemAlloc<int>(out var offset);
				slot = 42;

				Assert.AreEqual(42, header.GetRef(offset), "Значение, записанное по смещению в арене, не прочиталось обратно.");
			}
			finally
			{
				arena.Dispose();
			}
		}
	}
}
