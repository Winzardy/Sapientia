using System;
using System.Runtime.CompilerServices;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator.State
{
	public unsafe struct Entity : IEquatable<Entity>
	{
		public const ushort GENERATION_ZERO = 0;

		public static readonly Entity EMPTY = new (0, GENERATION_ZERO, default);

		public readonly ushort id;
		public readonly ushort generation;
		public WorldId worldId;

#if ENABLE_ENTITY_NAMES
		public string Name
		{
			get
			{
				var allocator = worldId.GetWorld();
				return allocator.GetService<EntityStatePart>().GetEntityName(allocator, this);
			}
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public World GetAllocator()
		{
			return worldId.GetWorld();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Entity(ushort id, ushort generation, WorldId worldId)
		{
			this.id = id;
			this.generation = generation;
			this.worldId = worldId;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create(string name)
		{
			var allocator = WorldManager.CurrentWorld;
#if UNITY_EDITOR
			return allocator.GetService<EntityStatePart>().CreateEntity(allocator, name);
#else
			return allocator.GetService<EntityStatePart>().CreateEntity(allocator);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create()
		{
			var allocator = WorldManager.CurrentWorld;
			return allocator.GetService<EntityStatePart>().CreateEntity(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create(World world, string name)
		{
#if UNITY_EDITOR
			return world.GetService<EntityStatePart>().CreateEntity(world, name);
#else
			return allocator.GetService<EntityStatePart>().CreateEntity(allocator);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create(World world)
		{
			return world.GetService<EntityStatePart>().CreateEntity(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create(WorldId worldId, string name)
		{
			var allocator = worldId.GetWorld();
#if UNITY_EDITOR
			return allocator.GetService<EntityStatePart>().CreateEntity(allocator, name);
#else
			return allocator.GetService<EntityStatePart>().CreateEntity(allocator);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create(WorldId worldId)
		{
			var allocator = worldId.GetWorld();
			return allocator.GetService<EntityStatePart>().CreateEntity(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsEmpty()
		{
			return generation == GENERATION_ZERO;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Entity a, Entity b)
		{
			return a.id == b.id && a.generation == b.generation && a.worldId.id == b.worldId.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Entity a, Entity b)
		{
			return !(a == b);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Entity other)
		{
			return this == other;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj)
		{
			if (obj is Entity ent)
			{
				return Equals(ent);
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(Entity other)
		{
			return id.CompareTo(other.id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return HashCode.Combine(this.As<Entity, int>(), worldId.id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString()
		{
			return $"Entity Id: {id} Gen: {generation}";
		}
	}
}
