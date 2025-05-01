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
		public AllocatorId allocatorId;

#if ENABLE_ENTITY_NAMES
		public string Name
		{
			get
			{
				var allocator = allocatorId.GetAllocator();
				return allocator.GetService<EntityStatePart>().GetEntityName(allocator, this);
			}
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Allocator GetAllocator()
		{
			return allocatorId.GetAllocator();
		}

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
			var allocator = AllocatorManager.CurrentAllocator;
#if UNITY_EDITOR
			return allocator.GetService<EntityStatePart>().CreateEntity(allocator, name);
#else
			return allocator.GetService<EntityStatePart>().CreateEntity(allocator);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create()
		{
			var allocator = AllocatorManager.CurrentAllocator;
			return allocator.GetService<EntityStatePart>().CreateEntity(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create(Allocator allocator, string name)
		{
#if UNITY_EDITOR
			return allocator.GetService<EntityStatePart>().CreateEntity(allocator, name);
#else
			return allocator.GetService<EntityStatePart>().CreateEntity(allocator);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create(Allocator allocator)
		{
			return allocator.GetService<EntityStatePart>().CreateEntity(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create(AllocatorId allocatorId, string name)
		{
			var allocator = allocatorId.GetAllocator();
#if UNITY_EDITOR
			return allocator.GetService<EntityStatePart>().CreateEntity(allocator, name);
#else
			return allocator.GetService<EntityStatePart>().CreateEntity(allocator);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create(AllocatorId allocatorId)
		{
			var allocator = allocatorId.GetAllocator();
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
			return a.id == b.id && a.generation == b.generation && a.allocatorId.id == b.allocatorId.id;
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
