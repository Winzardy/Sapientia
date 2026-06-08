#if UNITY_5_4_OR_NEWER
using NUnit.Framework;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// Фаза 3: <see cref="CompiledBlueprintStorage"/>. Сторедж ничего не знает о <c>Blueprint</c> —
	/// принимает уже скомпилированные <see cref="CompiledBlueprint"/> с аренами (<see cref="CompiledBlueprintStorage.Add"/>);
	/// компиляция здесь, в тесте, через <see cref="CompiledBlueprint.CompileLayout"/>. Off-allocator,
	/// worldState не нужен. Ничего не удаляется по одной — только Dispose. Проверяют Add/Count/Has/Get,
	/// дедуп, рантайм-add нового id, сосуществование версий, Dispose.
	/// </summary>
	public class CompiledBlueprintStorageTests
	{
		private static Blueprint Bp(int id, int version)
		{
			return StubBlueprint.Of(id, version, new StubNode(staticSize: 8));
		}

		// Компилируем снаружи и отдаём в сторедж (владение ареной переходит стореджу).
		private static void Add(ref CompiledBlueprintStorage storage, int id, int version)
		{
			var arena = CompiledBlueprint.CompileLayout(Bp(id, version), out var offset);
			storage.Add(arena, offset);
		}

		[Test]
		public void Storage_AddCountAndGet()
		{
			var storage = CompiledBlueprintStorage.Create();
			try
			{
				Add(ref storage, 1, 1);
				Add(ref storage, 2, 1);

				Assert.AreEqual(2, storage.Count, "Должно быть 2 compiled.");
				Assert.IsTrue(storage.Has(1, 1), "bp(1,1) не найден.");
				Assert.IsTrue(storage.Has(2, 1), "bp(2,1) не найден.");
				Assert.IsFalse(storage.Has(1, 2), "Несуществующая версия → false.");
				Assert.IsFalse(storage.Has(3, 1), "Несуществующий id → false.");

				ref var compiled = ref storage.Get(1, 1);
				Assert.AreEqual((Id<Blueprint>)1, compiled.id, "Неверный id у compiled.");
				Assert.AreEqual(1, (int)compiled.version, "Неверная version у compiled.");
			}
			finally
			{
				storage.Dispose();
			}
		}

		[Test]
		public void Storage_Dedup_SameIdVersion()
		{
			var storage = CompiledBlueprintStorage.Create();
			try
			{
				Add(ref storage, 1, 1);
				Add(ref storage, 1, 1); // та же (id,version) → дедуп (входная арена освобождается)

				Assert.IsTrue(storage.Has(1, 1));
				Assert.AreEqual(1, storage.Count, "Дедуп не должен создавать второй compiled.");
			}
			finally
			{
				storage.Dispose();
			}
		}

		[Test]
		public void Storage_RuntimeAdd_NewIdGrowsIndex()
		{
			var storage = CompiledBlueprintStorage.Create(blueprintCapacity: 1);
			try
			{
				Add(ref storage, 5, 1); // новый id за пределами начальной ёмкости
				Assert.IsTrue(storage.Has(5, 1), "Новый id должен резолвиться (список вырос).");
				Assert.AreEqual(1, storage.Count);
			}
			finally
			{
				storage.Dispose();
			}
		}

		[Test]
		public void Storage_VersionsCoexist()
		{
			var storage = CompiledBlueprintStorage.Create();
			try
			{
				Add(ref storage, 1, 1);
				Add(ref storage, 1, 2);
				Add(ref storage, 1, 3); // текущая

				// Ничего не удаляется — все версии живут и резолвятся (jump-by-id + walk по старым).
				Assert.IsTrue(storage.Has(1, 1) && storage.Has(1, 2) && storage.Has(1, 3), "Все версии живы.");
				Assert.AreEqual(3, storage.Count);
				Assert.AreEqual(1, (int)storage.Get(1, 1).version, "v1 (старая) резолвится.");
				Assert.AreEqual(2, (int)storage.Get(1, 2).version, "v2 (старая) резолвится.");
				Assert.AreEqual(3, (int)storage.Get(1, 3).version, "v3 (текущая) резолвится.");
			}
			finally
			{
				storage.Dispose();
			}
		}

		[Test]
		public void Storage_DisposeFreesAll()
		{
			var storage = CompiledBlueprintStorage.Create();
			Add(ref storage, 1, 1);
			Add(ref storage, 1, 2); // старая версия остаётся жить
			Add(ref storage, 2, 1);
			Assert.AreEqual(3, storage.Count);

			// Единый teardown освобождает все арены — без падений/двойного free.
			storage.Dispose();
			Assert.IsFalse(storage.IsCreated);
		}
	}
}
#endif
