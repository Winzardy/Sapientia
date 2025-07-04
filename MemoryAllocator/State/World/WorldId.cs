using System;
using System.Runtime.CompilerServices;

namespace Sapientia.MemoryAllocator
{
	public struct WorldId : IEquatable<WorldId>
	{
		// Always more then 0
		public readonly ushort id;
		public ushort index;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public WorldId(ushort index, ushort id)
		{
			this.index = index;
			this.id = id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator WorldId((ushort index, ushort id) indexId)
		{
			return new WorldId(indexId.index, indexId.id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(WorldId a, WorldId b)
		{
			return a.id == b.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(WorldId a, WorldId b)
		{
			return a.id != b.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => $"index: {index}, id: {id}";

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(WorldId other)
		{
			return id == other.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj)
		{
			return obj is WorldId other && Equals(other);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return id;
		}
	}
}
