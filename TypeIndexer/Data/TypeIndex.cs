using System;
using System.Runtime.CompilerServices;

namespace Sapientia.TypeIndexer
{
	public static class TypeIndex<T>
	{
		public static readonly TypeIndex typeIndex = IndexedTypes.GetTypeIndex(typeof(T));
	}

	public struct TypeIndex : IEquatable<TypeIndex>
	{
		internal int index;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TypeIndex Create<T>()
		{
			return TypeIndex<T>.typeIndex;
		}

		public Type Type
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => IndexedTypes.GetType(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator int(TypeIndex typeIndex)
		{
			return typeIndex.index;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator TypeIndex(int index)
		{
			return new TypeIndex{ index = index, };
		}

		public static bool operator ==(TypeIndex a, TypeIndex b)
		{
			return a.index == b.index;
		}

		public static bool operator !=(TypeIndex a, TypeIndex b)
		{
			return a.index != b.index;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(TypeIndex other)
		{
			return index == other.index;
		}
	}
}
