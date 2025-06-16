using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.Collections
{
	[DebuggerTypeProxy(typeof(UnsafeList<>.UnsafeListProxy))]
	public struct UnsafeList<T> : IDisposable
		where T : unmanaged
	{
		public SafePtr<T> ptr;
		public int count;
		public int capacity;
#if UNITY_5_3_OR_NEWER
		private Unity.Collections.Allocator _allocator;
#endif

		public bool IsCreated => ptr != default;

		public UnsafeList(int capacity)
		{
			this.ptr = MemoryExt.MakeArray<T>(capacity, false, false);
			this.count = 0;
			this.capacity = capacity;
#if UNITY_5_3_OR_NEWER
			_allocator = Unity.Collections.Allocator.None;
#endif
		}

#if UNITY_5_3_OR_NEWER
		public UnsafeList(int capacity, Unity.Collections.Allocator allocator)
		{
			this.ptr = MemoryExt.MakeArray<T>(capacity, allocator, false, false);
			this.count = 0;
			this.capacity = capacity;
			_allocator = allocator;
		}
#endif

		public ref T Last
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref ptr[count - 1];
		}

		public SafePtr<T> LastPtr
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ptr.Slice(count - 1, 1);
		}

		public ref T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref (ptr + index).Value();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T item)
		{
			EnsureCapacity(count + 1);

			ptr[count] = item;
			count++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T RemoveAt(int index)
		{
			var result = ptr[index];

			count--;
			if (index < count)
			{
				MemoryExt.MemMove<T>((ptr + (index + 1)), (ptr + index), count - index);
			}

			return result;
		}

		public void Insert(int index, T value)
		{
			EnsureCapacity(count + 1);

			MemoryExt.MemMove(ptr + index, ptr + index + 1, count - index);
			ptr[index] = value;

			count++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T RemoveAtSwapBack(int index)
		{
			var result = ptr[index];
			ptr[index] = ptr[--count];

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T RemoveLast()
		{
			count--;
			return ptr[count];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureCapacity(int newCapacity)
		{
#if UNITY_5_3_OR_NEWER
			if (_allocator != Unity.Collections.Allocator.None)
				MemoryExt.ResizeArray<T>(ref ptr, ref capacity, newCapacity, _allocator, true, false, false);
			else
#endif
			{
				MemoryExt.ResizeArray<T>(ref ptr, ref capacity, newCapacity, true, false, false);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			if (!IsCreated)
				return;

#if UNITY_5_3_OR_NEWER
			if (_allocator != Unity.Collections.Allocator.None)
				MemoryExt.MemFree(ptr, _allocator, false);
			else
#endif
			{
				MemoryExt.MemFree(ptr, false);
			}
			this = default;
		}

		public class UnsafeListProxy
		{
			private UnsafeList<T> _arr;

			public UnsafeListProxy(UnsafeList<T> arr)
			{
				_arr = arr;
			}

			public int Capacity => _arr.capacity;

			public int Count => _arr.count;

			public T[] Items
			{
				get
				{
					var arr = new T[_arr.count];
					for (var i = 0; i < _arr.count; ++i)
					{
						arr[i] = _arr[i];
					}

					return arr;
				}
			}
		}
	}
}
