using System;
using Sapientia.MemoryAllocator.Core;

namespace Sapientia.MemoryAllocator
{
	public struct UnsafeWorld : IDisposable
	{
		public WorldId worldId;

		public Allocator allocator;
		public ServiceRegistry serviceRegistry;

		public ushort version;

		public static UnsafeWorld Deserialize(ref StreamBufferReader stream)
		{
			var world = new UnsafeWorld();

			stream.Read(ref world.worldId);
			world.allocator = Allocator.Deserialize(ref stream);
			stream.Read(ref world.serviceRegistry);
			stream.Read(ref world.version);

			world.version++;

			return world;
		}

		public void Reset(WorldId worldId)
		{
			version++;

			serviceRegistry = default;
			allocator.Reset();
			this.worldId = worldId;
		}

		public void Dispose()
		{
			allocator.Dispose();
			this = default;
		}
	}
}
