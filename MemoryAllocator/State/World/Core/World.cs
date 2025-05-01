using System.Runtime.CompilerServices;
using Sapientia.MemoryAllocator.Core;

namespace Sapientia.MemoryAllocator
{
	public partial class World
	{
		public WorldId worldId;

		private Allocator _allocator;
		private ServiceRegistry _serviceRegistry;

		public ushort version;

		public bool IsValid => version > 0;

		public void Initialize(WorldId worldId, int initialSize)
		{
			E.ASSERT(!IsValid);

			this.worldId = worldId;
			this.version = 1;

			_allocator = new Allocator();
			_allocator.Initialize(initialSize);

			_serviceRegistry = ServiceRegistry.Create(this);
		}

		public static World Deserialize(ref StreamBufferReader stream)
		{
			var world = new World();

			stream.Read(ref world.worldId);
			world._allocator = Allocator.Deserialize(ref stream);
			stream.Read(ref world._serviceRegistry);
			stream.Read(ref world.version);

			world.version++;

			return world;
		}

		public void Reset(WorldId worldId)
		{
			E.ASSERT(IsValid);
			version++;

			_serviceRegistry = default;
			_allocator.Reset();
			this.worldId = worldId;
		}
	}
}
