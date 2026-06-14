#if UNITY_5_4_OR_NEWER
using NUnit.Framework;
using Sapientia.Data;
using Sapientia.Memory;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// Фаза 4: <see cref="BlueprintInstanceStorage"/> — WorldState-free <c>UnsafeIndexAllocSparseSet</c> с
	/// <b>generation</b>. Принимает собранный <see cref="BlueprintInstanceHeader"/> (память — снаружи, офсеты здесь
	/// нулевые), раздаёт хендлы <c>(id, generation)</c>, переиспользует слоты, детектит stale. Память
	/// инстансов не трогает. Проверяют Add/Count/TryGet, generation-staleness + reuse слота, Dispose. WorldState не нужен.
	/// </summary>
	public class BlueprintInstanceStorageTests
	{
		private static RawBumpAllocator Compile(out PtrOffset<CompiledBlueprintHeader> offset)
		{
			return CompiledBlueprintHeader.CompileLayout(StubBlueprint.Of(1, 1, new StubNode(cacheSize: 16)), out offset);
		}

		[Test]
		public void InstanceStorage_AddCountAndTryGet()
		{
			var storage = new BlueprintInstanceStorage(8);
			var arena = Compile(out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);

				var a = storage.Add(BlueprintInstanceHeader.Create(compiled, default, default));
				var b = storage.Add(BlueprintInstanceHeader.Create(compiled, default, default));

				Assert.AreEqual(2, storage.Count, "Должно быть 2 инстанса.");
				Assert.AreNotEqual(a, b, "Хендлы должны быть различны.");
				Assert.IsTrue(storage.TryGet(a, out var ga), "Инстанс a не найден.");
				Assert.IsTrue(storage.TryGet(b, out _), "Инстанс b не найден.");
				Assert.IsFalse(storage.TryGet(BlueprintInstanceId.Invalid, out _), "Invalid хендл → false.");
			}
			finally
			{
				storage.Dispose();
				arena.Dispose();
			}
		}

		[Test]
		public void InstanceStorage_GenerationStalenessAndSlotReuse()
		{
			var storage = new BlueprintInstanceStorage(8);
			var arena = Compile(out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);

				var a = storage.Add(BlueprintInstanceHeader.Create(compiled, default, default));

				storage.Remove(a);
				Assert.AreEqual(0, storage.Count, "После Remove пусто.");
				Assert.IsFalse(storage.TryGet(a, out _), "Stale хендл не должен резолвиться.");
				storage.Remove(a); // повторный Remove stale — no-op
				Assert.AreEqual(0, storage.Count);

				// Новый инстанс переиспользует слот, но получает новый generation.
				var b = storage.Add(BlueprintInstanceHeader.Create(compiled, default, default));
				Assert.AreEqual(a.id, b.id, "Слот должен быть переиспользован (SparseSet).");
				Assert.AreNotEqual(a.generation, b.generation, "Generation должен смениться.");
				Assert.IsFalse(storage.TryGet(a, out _), "Старый хендл остаётся stale.");
				Assert.IsTrue(storage.TryGet(b, out _), "Новый хендл валиден.");
			}
			finally
			{
				storage.Dispose();
				arena.Dispose();
			}
		}

		[Test]
		public void InstanceStorage_DisposeIdempotent()
		{
			var storage = new BlueprintInstanceStorage(8);
			var arena = Compile(out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				storage.Add(BlueprintInstanceHeader.Create(compiled, default, default));
				storage.Add(BlueprintInstanceHeader.Create(compiled, default, default));
				Assert.AreEqual(2, storage.Count);

				storage.Dispose();
				Assert.IsFalse(storage.IsCreated, "После Dispose сторедж должен стать неинициализированным.");

				// Идемпотентно.
				storage.Dispose();
			}
			finally
			{
				arena.Dispose();
			}
		}
	}
}
#endif
