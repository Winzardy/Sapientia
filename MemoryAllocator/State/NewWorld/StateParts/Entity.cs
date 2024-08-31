using System;
using System.Runtime.CompilerServices;
using Sapientia.Collections.Archetypes;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator.State.NewWorld
{
	public struct Entity : IEquatable<Entity>
	{
		public const ushort GENERATION_ZERO = 0;

		public static readonly Entity EMPTY = new (0, GENERATION_ZERO, default);

		public readonly ushort id;
		public readonly ushort generation;
		public AllocatorId allocatorId;

#if UNITY_EDITOR
		public string Name => id >= 0 ? ServiceLocator<EntitiesState>.Instance.entitiesNames[id] : string.Empty;
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Entity(ushort id, ushort generation, AllocatorId allocatorId)
		{
			this.id = id;
			this.generation = generation;
			this.allocatorId = allocatorId;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create(string name)
		{
			ref var allocator = ref AllocatorManager.CurrentAllocator;
#if UNITY_EDITOR
			return allocator.serviceLocator.GetService<EntitiesStatePart>().CreateEntity(ref allocator, name);
#else
			return allocator.serviceLocator.GetService<EntitiesStatePart>().CreateEntity(ref allocator);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create()
		{
			ref var allocator = ref AllocatorManager.CurrentAllocator;
			return allocator.serviceLocator.GetService<EntitiesStatePart>().CreateEntity(ref allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create(ref Allocator allocator, string name)
		{
#if UNITY_EDITOR
			return allocator.serviceLocator.GetService<EntitiesStatePart>().CreateEntity(ref allocator, name);
#else
			return allocator.serviceLocator.GetService<EntitiesStatePart>().CreateEntity(ref allocator);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create(ref Allocator allocator)
		{
			return allocator.serviceLocator.GetService<EntitiesStatePart>().CreateEntity(ref allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create(AllocatorId allocatorId, string name)
		{
			ref var allocator = ref allocatorId.GetAllocator();
#if UNITY_EDITOR
			return allocator.serviceLocator.GetService<EntitiesStatePart>().CreateEntity(ref allocator, name);
#else
			return allocator.serviceLocator.GetService<EntitiesStatePart>().CreateEntity(ref allocator);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create(AllocatorId allocatorId)
		{
			ref var allocator = ref allocatorId.GetAllocator();
			return allocator.serviceLocator.GetService<EntitiesStatePart>().CreateEntity(ref allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsEmpty()
		{
			return generation == GENERATION_ZERO;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Entity a, Entity b)
		{
			return a.id == b.id && a.generation == b.generation && a.allocatorId == b.allocatorId;
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
			return HashCode.Combine(this.As<Entity, int>(), allocatorId.id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString()
		{
			return $"Entity Id: {id} Gen: {generation}";
		}
	}
}
