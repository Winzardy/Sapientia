using System;
using Sapientia.Memory;

namespace Sapientia.MemoryAllocator
{
	internal struct WorldStateData : IDisposable
	{
		public WorldId worldId;

		public Allocator allocator;
		public ServiceRegistry serviceRegistry; // Работает с состоянием мира

		public UnsafeServiceRegistry noStateServiceRegistry; // Работает с локальными данными (Вне мира)

		public ushort version;

		public uint tick;
		public float time;

		public WorldStateData(WorldId worldId, int initialSize)
		{
			this.worldId = worldId;
			version = 1;
			tick = 0u;
			time = 0f;

			allocator = new Allocator();
			allocator.Initialize(initialSize);

			// При добавлении сервиса происходит инициализация
			serviceRegistry = default;
			// При добавлении сервиса происходит инициализация
			noStateServiceRegistry = default;
		}

		public static WorldStateData Deserialize(ref StreamBufferReader stream)
		{
			var world = new WorldStateData();

			stream.Read(ref world.worldId);
			world.allocator = Allocator.Deserialize(ref stream);
			stream.Read(ref world.serviceRegistry);
			stream.Read(ref world.version);
			stream.Read(ref world.tick);
			stream.Read(ref world.time);

			world.version++;
			// При добавлении сервиса происходит инициализация
			world.noStateServiceRegistry = default;

			return world;
		}

		public void SetupNewWorldId(WorldId newWorldId)
		{
			worldId = newWorldId;
		}

		public void Reset()
		{
			// При добавлении сервиса происходит инициализация
			// Серввисы выделяются в аллокаторе, который будет очищен
			serviceRegistry = default;
			// Обязательно нужно очистить, т.к. сервисы выделяются в неуправляемой памяти
			noStateServiceRegistry.Clear();
			allocator.Clear();

			tick = 0u;
			time = 0f;
			version++;
		}

		public void Dispose()
		{
			// Не обязательно диспозить, т.к. сервисы выделяются в аллокаторе, который будет задиспожен
			serviceRegistry = default;
			// Обязательно нужно задиспозить, т.к. сервисы выделяются в неуправляемой памяти
			noStateServiceRegistry.Dispose();
			allocator.Dispose();
			this = default;
		}
	}
}
