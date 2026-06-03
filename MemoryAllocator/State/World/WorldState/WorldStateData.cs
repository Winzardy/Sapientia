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
		/// Heap-хранилище unmanaged сервисов помеченных <see cref="IWorldService"/> (StatePart-ы, системы,
		/// конфиги, save-структуры). Payload хранит <see cref="IndexedPtr"/> со ссылкой в allocator.
		/// Попадает в снапшот вручную через <see cref="Serialize"/> / <see cref="Deserialize"/>.
		/// </summary>
		public UnsafeIndexedRegistry<IWorldService, IndexedPtr> serviceRegistry;
		/// <summary>
		/// Heap-хранилище unmanaged runtime-сервисов (Logic'и, unmanaged local part-ы).
		/// Маркер <see cref="IWorldLocalUnmanagedService"/>. Не попадает в снапшот.
		/// </summary>
		public UnsafeIndexedRegistry<IWorldLocalUnmanagedService, SafePtr> noStateServiceRegistry;
		/// <summary>
		/// Heap-хранилище <see cref="ComponentSet"/>-ов, проиндексированное по <see cref="TypeId{IComponent}"/>.
		/// Payload — <see cref="CachedPtr{ComponentSet}"/> со ссылкой в allocator. Попадает в снапшот.
		/// </summary>
		public UnsafeIndexedRegistry<IComponent, CachedPtr<ComponentSet>> componentsManager;

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

			// Все регистры в heap — можно создать сразу, без зависимости от WorldId.version.
			serviceRegistry = UnsafeIndexedRegistry<IWorldService, IndexedPtr>.Create();
			noStateServiceRegistry = UnsafeIndexedRegistry<IWorldLocalUnmanagedService, SafePtr>.Create();
			componentsManager = UnsafeIndexedRegistry<IComponent, CachedPtr<ComponentSet>>.Create();
		}

		public void Serialize(ref StreamBufferWriter stream)
		{
			allocator.Serialize(ref stream);
			serviceRegistry.Serialize(ref stream);
			componentsManager.Serialize(ref stream);
			stream.Write(version);
			stream.Write(tick);
			stream.Write(time);
			// noStateServiceRegistry — heap-only runtime данные, в снапшот не попадают.
		}

		public static WorldStateData Deserialize(ref StreamBufferReader stream)
		{
			// default init — все поля перезаписываются ниже, параметризованный ctor с initialSize не нужен.
			var world = new WorldStateData();

			world.allocator = Allocator.Deserialize(ref stream);
			world.serviceRegistry = UnsafeIndexedRegistry<IWorldService, IndexedPtr>.Deserialize(ref stream);
			world.componentsManager = UnsafeIndexedRegistry<IComponent, CachedPtr<ComponentSet>>.Deserialize(ref stream);
			stream.Read(ref world.version);
			stream.Read(ref world.tick);
			stream.Read(ref world.time);

			world.version++;
			// heap-only registry пересоздаётся при загрузке (runtime-only, не в стейте)
			world.noStateServiceRegistry = UnsafeIndexedRegistry<IWorldLocalUnmanagedService, SafePtr>.Create();

			return world;
		}

		public void Reset()
		{
			serviceRegistry.Clear();
			componentsManager.Clear();
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
			serviceRegistry.Dispose();
			componentsManager.Dispose();
			// heap registry — обязательная очистка payload'ов перед dispose
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
