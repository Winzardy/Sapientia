using System;
using Sapientia.Collections;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Хранилище живых <see cref="BlueprintInstanceHeader"/> одного домена исполнения. Аналог
	/// <see cref="CompiledBlueprintStorage"/> (static-блобы), но для <b>мутабельного per-instance стейта</b>.
	/// <b>Ничего не знает о памяти инстансов, её источнике и <c>WorldState</c></b> — держит инстансы (identity +
	/// абстрактные офсеты) и раздаёт хендлы; память слайсов выделяет/освобождает владелец (<see cref="ExecutionScope"/>).
	///
	/// <b>Хранилище — <see cref="UnsafeIndexAllocSparseSet{T}"/></b> (off-allocator): O(1) доступ/удаление по
	/// индексу, переиспользование слотов. Поверх — <b>generation</b> на слот (<see cref="_generations"/>): при
	/// <see cref="Remove"/> generation слота инкрементируется, поэтому <b>stale</b>-хендл на переиспользованный
	/// слот перестаёт резолвиться (паттерн <c>Entity.generation</c>).
	/// </summary>
	public struct BlueprintInstanceStorage
	{
		private UnsafeIndexAllocSparseSet<BlueprintInstanceHeader> _instances;
		// Generation на каждый слот (id): 0 — невалид, >=1 — живой/переиспользуемый.
		private UnsafeList<int> _generations;

		public bool IsCreated => _instances.IsCreated;
		public int Count => _instances.Count;

		public BlueprintInstanceStorage(int capacity = 8)
		{
			var cap = capacity > 0 ? capacity : 1;
			_instances = new UnsafeIndexAllocSparseSet<BlueprintInstanceHeader>(cap);
			_generations = new UnsafeList<int>(cap);
		}

		/// <summary>
		/// Берёт <b>уже собранный снаружи</b> инстанс (с офсетами на готовые слайсы), выделяет ему слот в
		/// sparse set, проставляет хендл <c>(id, generation)</c> и трекает. Память инстанса сторедж не трогает.
		/// </summary>
		public BlueprintInstanceId Add(BlueprintInstanceHeader instance)
		{
			var id = _instances.AllocateId();
			// Новые слоты заводим с generation = 1 (0 — невалид); переиспользуемые держат свой инкремент.
			_generations.EnsureCount(id + 1, 1);
			ref var generation = ref _generations[id];
			generation++;

			_instances.Get(id) = instance;
			return new BlueprintInstanceId { id = id, generation = generation };
		}

		/// <summary>Жив ли хендл: слот занят и generation совпадает (не stale).</summary>
		public bool Has(BlueprintInstanceId id)
		{
			return id.IsValid && _instances.Has(id.id) && _generations[id.id] == id.generation;
		}

		public bool TryGet(BlueprintInstanceId id, out BlueprintInstanceHeader instance)
		{
			if (Has(id))
			{
				instance = _instances.Get(id.id);
				return true;
			}
			instance = default;
			return false;
		}

		/// <summary>
		/// Снимает инстанс с трека (только bookkeeping — память освобождает владелец до вызова) и
		/// инкрементирует generation слота (инвалидирует все старые хендлы на него). No-op для stale/неизвестного.
		/// </summary>
		public void Remove(BlueprintInstanceId id)
		{
			if (!Has(id))
				return;

			_generations[id.id]++;
			_instances.ReleaseId(id.id);
		}

		/// <summary>Плотный (dense) набор живых инстансов — для итерации владельцем (освобождение/исполнение).</summary>
		public Span<BlueprintInstanceHeader> Values => _instances.GetValuesSpan();

		/// <summary>Освобождает только структуры стореджа (память инстансов — забота владельца). Идемпотентно.</summary>
		public void Dispose()
		{
			if (!IsCreated)
				return;

			_instances.Dispose();
			_generations.Dispose();
			this = default;
		}
	}
}
