using System;
using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	public struct AllocatorId : IEquatable<AllocatorId>
	{
		public readonly ushort id;
		public ushort index;

		[INLINE(256)]
		public AllocatorId(ushort index, ushort id)
		{
			this.index = index;
			this.id = id;
		}

		[INLINE(256)]
		public static implicit operator AllocatorId((ushort index, ushort id) indexId)
		{
			return new AllocatorId(indexId.index, indexId.id);
		}

		[INLINE(256)]
		public static bool operator ==(AllocatorId a, AllocatorId b)
		{
			return a.id == b.id;
		}

		[INLINE(256)]
		public static bool operator !=(AllocatorId a, AllocatorId b)
		{
			return a.id != b.id;
		}

		[INLINE(256)]
		public override string ToString() => $"index: {index}, id: {id}";

		[INLINE(256)]
		public bool Equals(AllocatorId other)
		{
			return id == other.id;
		}

		[INLINE(256)]
		public override bool Equals(object obj)
		{
			return obj is AllocatorId other && Equals(other);
		}

		[INLINE(256)]
		public override int GetHashCode()
		{
			return id;
		}
	}
}
