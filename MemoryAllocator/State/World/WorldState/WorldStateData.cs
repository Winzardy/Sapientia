using Sapientia.Data;
using Sapientia.Memory;
using Sapientia.MemoryAllocator.State;
using Sapientia.TypeIndexer;
using Submodules.Sapientia.Memory;

namespace Sapientia.MemoryAllocator
{
	internal struct WorldStateData
	{
		public Allocator allocator;

		/// <summary>
		/// In-state хранилище unmanaged сервисов (StatePart-ы, системы, конфиги, save-структуры).
		/// Маркер <see cref="IWorldService"/>. Попадает в снапшот.
		/// </summary>
		public IndexedRegistry<IWorldService, IndexedPtr> serviceRegistry;
		/// <summary>
		/// Heap-хранилище unmanaged runtime-сервисов (Logic'и, unmanaged local part-ы).
		/// Маркер <see cref="IWorldLocalUnmanagedService"/>. Не попадает в снапшот.
		/// </summary>
		public UnsafeIndexedRegistry<IWorldLocalUnmanagedService, SafePtr> noStateServiceRegistry;
		/// <summary>
		/// In-state хранилище <see cref="ComponentSet"/>-ов, проиндексированное по <see cref="TypeId{IComponent}"/>.
		/// Попадает в снапшот.
		/// </summary>
		public IndexedRegistry<IComponent, CachedPtr<ComponentSet>> componentsManager;

		public ushort version;

		/// <summary>
		/// По сути своей является количиством обновлений мира.
		/// Из этого значения нельзя вычислить время напрямую, т.к. время может идти с разной скоростью в каждый апдейт.
		/// </summary>
		public uint tick;
		public float time;

		public WorldStateData(int initialSize)
		{
			version = 1;
			tick = 0u;
			time = 0f;

			allocator = new Allocator();
			allocator.Initialize(initialSize);

			// in-state registries требуют WorldState handle для allocator — инициализируются в WorldState constructor
			serviceRegistry = default;
			componentsManager = default;
			// heap-based: можно создать сразу
			noStateServiceRegistry = UnsafeIndexedRegistry<IWorldLocalUnmanagedService, SafePtr>.Create();
		}

		public static WorldStateData Deserialize(ref StreamBufferReader stream)
		{
			var world = new WorldStateData();

			world.allocator = Allocator.Deserialize(ref stream);
			stream.Read(ref world.serviceRegistry);
			stream.Read(ref world.componentsManager);
			stream.Read(ref world.version);
			stream.Read(ref world.tick);
			stream.Read(ref world.time);

			world.version++;
			// heap registry пересоздаётся при загрузке (runtime-only, не в стейте)
			world.noStateServiceRegistry = UnsafeIndexedRegistry<IWorldLocalUnmanagedService, SafePtr>.Create();
			// Note: если snapshot был сделан до первого Register'а — in-state registries придут как default(IndexedRegistry).
			// WorldState constructor / SetupNewWorldId должны вызвать InitializeInStateRegistries после Deserialize
			// (это ответственность caller'а — WorldState.Deserialize).

			return world;
		}

		public void Reset()
		{
			// in-state registries: их MemArray лежат в allocator который сейчас очистится
			serviceRegistry = default;
			componentsManager = default;
			// heap registry — освобождаем SafePtr payload'ы перед сбросом
			FreeNoStateSafePtrs();
			noStateServiceRegistry.Clear();
			allocator.Clear();

			tick = 0u;
			time = 0f;
			version++;
		}

		public void Dispose()
		{
			// in-state — диспозятся вместе с allocator
			serviceRegistry = default;
			componentsManager = default;
			// heap — обязательная очистка
			FreeNoStateSafePtrs();
			noStateServiceRegistry.Dispose();
			allocator.Dispose();
			this = default;
		}

		private void FreeNoStateSafePtrs()
		{
			if (!noStateServiceRegistry.IsCreated)
				return;
			for (var i = 0; i < noStateServiceRegistry.Length; i++)
			{
				ref var slot = ref noStateServiceRegistry.GetByIndex(i);
				if (slot.IsValid)
				{
					MemoryExt.MemFree(slot);
					slot = default;
				}
			}
		}
	}
}
