using System;
using System.Runtime.CompilerServices;

namespace Sapientia.TypeIndexer
{
	public struct TypeIndex
	{
		internal int index;

		public Type Type
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => IndexedTypes.GetType(this);
		}

		public static implicit operator int(TypeIndex typeIndex)
		{
			return typeIndex.index;
		}

		public static implicit operator TypeIndex(int index)
		{
			return new TypeIndex{ index = index, };
		}
	}
}
