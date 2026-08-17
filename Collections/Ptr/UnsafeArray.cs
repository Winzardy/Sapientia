using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Submodules.Sapientia.Data;
using Submodules.Sapientia.Memory;

namespace Sapientia.Collections
{
#if UNITY_5_3_OR_NEWER
	[Unity.Burst.BurstCompile]
#endif
	[DebuggerTypeProxy(typeof(PtrArray<>.PtrArrayProxy))]
	public struct PtrArray<T>
		where T : unmanaged
	{
		public SafePtr<T> ptr;
		private int _length;

		public readonly int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _length;
		}

		public readonly bool IsCreated => ptr != default;

		public PtrArray(SafePtr<T> ptr, int length)
		{
			this.ptr = ptr;
			this._length = length;
		}

		public ref T Last
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref ptr[Length - 1];
		}

		public SafePtr<T> LastPtr
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ptr.Slice(Length - 1, 1);
		}

		public readonly ref T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref ptr[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly Span<T> GetSpan()
		{
			return ptr.GetSpan(0, _length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Fill(T value)
		{
			MemoryExt.MemFill<T>(value, ptr, Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Fill(T value, int index, int count)
		{
			MemoryExt.MemFill<T>(value, ptr + index, count);
		}

		public void CopyFrom(PtrArray<T> other)
		{
			E.ASSERT(_length >= other._length);
			MemoryExt.MemCopy(other.ptr, ptr, Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			MemoryExt.MemClear(ptr, Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(int startIndex, int count)
		{
			MemoryExt.MemClear(ptr + startIndex, count);
		}

		public Span<T>.Enumerator GetEnumerator()
		{
			return GetSpan().GetEnumerator();
		}

		public class PtrArrayProxy
		{
			private PtrArray<T> _arr;

			public PtrArrayProxy(PtrArray<T> arr)
			{
				_arr = arr;
			}

			public int Length => _arr._length;

			public T[] Items
			{
				get
				{
					var arr = new T[_arr._length];
					for (var i = 0; i < _arr._length; ++i)
					{
						arr[i] = _arr[i];
					}

					return arr;
				}
			}
		}
	}
}
