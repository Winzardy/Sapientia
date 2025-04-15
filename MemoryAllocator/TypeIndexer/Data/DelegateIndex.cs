using System.Runtime.CompilerServices;

namespace Sapientia.TypeIndexer
{
	public struct DelegateIndex
	{
		internal ushort index;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator int(DelegateIndex typeIndex)
		{
			return typeIndex.index;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator DelegateIndex(ushort index)
		{
			return new DelegateIndex{ index = index, };
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator DelegateIndex(int index)
		{
			return new DelegateIndex{ index = (ushort)index, };
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(DelegateIndex a, DelegateIndex b)
		{
			return a.index == b.index;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(DelegateIndex a, DelegateIndex b)
		{
			return a.index == b.index;
		}
	}
}
