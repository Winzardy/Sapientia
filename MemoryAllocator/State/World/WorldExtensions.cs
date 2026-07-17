using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Sapientia.Collections.FixedString;
using Sapientia.MemoryAllocator.State;

namespace Sapientia.MemoryAllocator
{
	public static class WorldExtensions
	{
		[CanBeNull]
		public static World ToWorld(this Entity entity)
		{
			if (!entity.IsValid())
				return null;

			return WorldManager.GetWorld(entity.worldId);
		}

		[CanBeNull]
		public static World ToWorld(this WorldState worldState)
		{
			if (!worldState.IsValid)
				return null;

			return WorldManager.GetWorld(worldState.WorldId);
		}

		public static bool IsValid(this Entity entity)
		{
			return entity.worldId.IsValid() && entity.IsExist(entity.GetWorldState());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity CreateEntity(this World world, in FixedString64Bytes name = default)
		{
			return world.GetService<EntityStatePart>().CreateEntity(world.worldState, name);
		}
	}
}
