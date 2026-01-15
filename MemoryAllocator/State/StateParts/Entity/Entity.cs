using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sapientia.MemoryAllocator.State
{
	[StructLayout(LayoutKind.Explicit)]
	public struct Entity : IEquatable<Entity>
	{
		/// <summary>
		/// Все <see cref="Entity"/> c <see cref="Entity.generation"/> == <see cref="Entity.GENERATION_ZERO"/> являются пустыми и не валидными.
		/// </summary>
		public const ushort GENERATION_ZERO = 0;

		public static readonly Entity EMPTY = new (default, GENERATION_ZERO, default);

		[FieldOffset(0)]
		// Для операций сравнения + HashCode
		private readonly int _idGeneration;

		[FieldOffset(0)]
		public readonly ushort id;
		[FieldOffset(2)]
		public readonly ushort generation;
		[FieldOffset(4)]
		public WorldId worldId;

		public string Name
		{
			get
			{
#if ENABLE_ENTITY_NAMES
				if (!worldId.IsValid())
					return "No World State";
				var worldState = worldId.GetWorldState();
				return worldState.GetService<EntityStatePart>().GetEntityName(worldState, this);
#else
				return "[ENABLE_ENTITY_NAMES] is Disabled";
#endif
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public WorldState GetWorldState()
		{
			return worldId.GetWorldState();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public World GetWorld()
		{
			return worldId.GetWorld();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Entity(int id, int generation, WorldId worldId) : this((ushort)id, (ushort)generation, worldId)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Entity(ushort id, ushort generation, WorldId worldId)
		{
			this._idGeneration = 0;

			this.id = id;
			this.generation = generation;
			this.worldId = worldId;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create(string name)
		{
			var worldState = WorldManager.CurrentWorldState;
#if UNITY_EDITOR
			return worldState.GetService<EntityStatePart>().CreateEntity(worldState, name);
#else
			return worldState.GetService<EntityStatePart>().CreateEntity(worldState);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create()
		{
			var worldState = WorldManager.CurrentWorldState;
			return worldState.GetService<EntityStatePart>().CreateEntity(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create(WorldState worldState, string name)
		{
#if UNITY_EDITOR
			return worldState.GetService<EntityStatePart>().CreateEntity(worldState, name);
#else
			return worldState.GetService<EntityStatePart>().CreateEntity(worldState);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create(WorldState worldState)
		{
			return worldState.GetService<EntityStatePart>().CreateEntity(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create(WorldId worldId, string name)
		{
			var worldState = worldId.GetWorldState();
#if UNITY_EDITOR
			return worldState.GetService<EntityStatePart>().CreateEntity(worldState, name);
#else
			return worldState.GetService<EntityStatePart>().CreateEntity(worldState);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity Create(WorldId worldId)
		{
			var worldState = worldId.GetWorldState();
			return worldState.GetService<EntityStatePart>().CreateEntity(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsEmpty()
		{
			return generation == GENERATION_ZERO;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Entity a, Entity b)
		{
			return a._idGeneration == b._idGeneration && a.worldId == b.worldId;
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

		[Obsolete("Убрать, т.к. работает не корректно, а в местах использования нужно сравнивать Entity.id")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(Entity other)
		{
			return id.CompareTo(other.id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return _idGeneration;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString()
		{
#if ENABLE_ENTITY_NAMES
			return $"Entity Id: {id} Gen: {generation} Name: {Name}";
#else
			return $"Entity Id: {id} Gen: {generation}";
#endif
		}
	}
}
