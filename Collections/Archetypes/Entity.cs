using System;
using Sapientia.Extensions;
using Sapientia.ServiceManagement;

namespace Sapientia.Collections.Archetypes
{
	public readonly struct Entity : IEquatable<Entity>
	{
		public const ushort GENERATION_ZERO = 0;

		public static readonly Entity EMPTY = new (0, GENERATION_ZERO);

		public readonly ushort id;
		public readonly ushort generation;

#if UNITY_EDITOR
		public string Name => id >= 0 ? (ServiceLocator<EntitiesState>.Instance.entitiesNames[id] + (this.IsExist() ? string.Empty : " [NotExist]")) : string.Empty;
#endif

		internal Entity(ushort id, ushort generation)
		{
			this.id = id;
			this.generation = generation;
		}

		public static Entity Create(string name)
		{
#if UNITY_EDITOR
			return ServiceLocator<EntitiesState>.Instance.CreateEntity(name);
#else
			return ServiceLocator<EntitiesState>.Instance.CreateEntity();
#endif
		}

		public static Entity Create()
		{
			return ServiceLocator<EntitiesState>.Instance.CreateEntity();
		}

		public bool IsEmpty()
		{
			return generation == GENERATION_ZERO;
		}

		public static bool operator ==(Entity a, Entity b)
		{
			return a.id == b.id && a.generation == b.generation;
		}

		public static bool operator !=(Entity a, Entity b)
		{
			return !(a == b);
		}

		public bool Equals(Entity other)
		{
			return this == other;
		}

		public override bool Equals(object obj)
		{
			if (obj is Entity ent)
			{
				return Equals(ent);
			}

			return false;
		}

		public int CompareTo(Entity other)
		{
			return id.CompareTo(other.id);
		}

		public override int GetHashCode()
		{
			return id ^ generation;
		}

		public override string ToString()
		{
			return $"Entity Id: {this.id} Gen: {this.generation}";
		}
	}
}
