using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sapientia.MemoryAllocator
{
	public readonly struct Enumerable<T, TEnumerator> : IEnumerable<T> where T : unmanaged where TEnumerator: IEnumerator<T>
	{
		private readonly TEnumerator _enumerator;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Enumerable(in TEnumerator enumerator)
		{
			_enumerator = enumerator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TEnumerator GetEnumerator()
		{
			return _enumerator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return _enumerator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
