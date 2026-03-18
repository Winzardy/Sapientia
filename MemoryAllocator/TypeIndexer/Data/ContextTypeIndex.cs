using System;
using System.Runtime.CompilerServices;

namespace Sapientia.TypeIndexer
{
	public struct ContextTypeIndex : IEquatable<ContextTypeIndex>
	{
		public static readonly ContextTypeIndex Empty = -1;

		internal int index;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator int(ContextTypeIndex contextTypeIndex)
		{
			return contextTypeIndex.index;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ContextTypeIndex(int index)
		{
			return new ContextTypeIndex { index = index, };
		}

		public static bool operator ==(ContextTypeIndex a, ContextTypeIndex b)
		{
			return a.index == b.index;
		}

		public static bool operator !=(ContextTypeIndex a, ContextTypeIndex b)
		{
			return a.index != b.index;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(ContextTypeIndex other)
		{
			return index == other.index;
		}

		public override bool Equals(object obj)
		{
			return obj is ContextTypeIndex other && Equals(other);
		}

		public override int GetHashCode()
		{
			return index;
		}
	}
}
