using System;
using Sapientia.Data;
using Sapientia.MemoryAllocator.Core;

namespace Sapientia.MemoryAllocator
{
	public struct UnsafeWorld : IDisposable
	{
		public WorldId worldId;

		public Allocator allocator;
		public ServiceRegistry serviceRegistry; // Работает с состоянием мира

		public UnsafeServiceRegistry localServiceRegistry; // Работает с локальными данными (Вне мира)

		public ushort version;

		public void Initialize(WorldId worldId, int initialSize)
		{
			this.worldId = worldId;
			version = 1;

			allocator = new Allocator();
			allocator.Initialize(initialSize);

			// При добавлении сервиса происходит инициализация
			serviceRegistry = default;
			// При добавлении сервиса происходит инициализация
			localServiceRegistry = default;
		}

		public static UnsafeWorld Deserialize(ref StreamBufferReader stream)
		{
			var world = new UnsafeWorld();

			stream.Read(ref world.worldId);
			world.allocator = Allocator.Deserialize(ref stream);
			stream.Read(ref world.serviceRegistry);
			stream.Read(ref world.version);

			world.version++;
			// При добавлении сервиса происходит инициализация
			world.localServiceRegistry = default;

			return world;
		}

		public void Reset(WorldId worldId)
		{
			version++;
			this.worldId = worldId;
		}

		public void Clear()
		{
			// При добавлении сервиса происходит инициализация
			// Серввисы выделяются в аллокаторе, который будет очищен
			serviceRegistry = default;
			// Обязательно нужно очистить, т.к. сервисы выделяются в неуправляемой памяти
			localServiceRegistry.Clear();
			allocator.Clear();
		}

		public void Dispose()
		{
			// Не обязательно диспозить, т.к. сервисы выделяются в аллокаторе, который будет задиспожен
			serviceRegistry = default;
			// Обязательно нужно задиспозить, т.к. сервисы выделяются в неуправляемой памяти
			localServiceRegistry.Dispose();
			allocator.Dispose();
			this = default;
		}
	}
}
