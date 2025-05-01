using System;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	public struct WorldId : IEquatable<WorldId>
	{
		// Always more then 0
		public readonly ushort id;
		public ushort index;

		[INLINE(256)]
		public WorldId(ushort index, ushort id)
		{
			this.index = index;
			this.id = id;
		}

		[INLINE(256)]
		public static implicit operator WorldId((ushort index, ushort id) indexId)
		{
			return new WorldId(indexId.index, indexId.id);
		}

		[INLINE(256)]
		public static bool operator ==(WorldId a, WorldId b)
		{
			return a.id == b.id;
		}

		[INLINE(256)]
		public static bool operator !=(WorldId a, WorldId b)
		{
			return a.id != b.id;
		}

		[INLINE(256)]
		public override string ToString() => $"index: {index}, id: {id}";

		[INLINE(256)]
		public bool Equals(WorldId other)
		{
			return id == other.id;
		}

		[INLINE(256)]
		public override bool Equals(object obj)
		{
			return obj is WorldId other && Equals(other);
		}

		[INLINE(256)]
		public override int GetHashCode()
		{
			return id;
		}
	}
}
