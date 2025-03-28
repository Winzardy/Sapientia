using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Extensions;

namespace Sapientia.Collections
{
	[DebuggerTypeProxy(typeof(UnsafeList<>.UnsafeListProxy))]
	public unsafe struct UnsafeList<T> : IDisposable
		where T : unmanaged
	{
		public T* array;
		public int count;
		public int capacity;

		public bool IsValid => array != null;

		public UnsafeList(int capacity = 8)
		{
			this.array = MemoryExt.MakeArray<T>(capacity, false);
			this.count = 0;
			this.capacity = capacity;
		}

		public ref T Last
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref array[count - 1];
		}

		public T* LastPtr
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => &array[count - 1];
		}

		public T* this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => &array[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T item)
		{
			EnsureCapacity(count + 1);

			array[count] = item;
			count++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T RemoveAt(int index)
		{
			var result = array[index];

			count--;
			if (index < count)
				MemoryExt.MemMove<T>(array + index + 1, array + index, count - index);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T RemoveAtSwapBack(int index)
		{
			var result = array[index];
			array[index] = array[--count];

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T RemoveLast()
		{
			count--;
			return array[count];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnsureCapacity(int newCapacity)
		{
			MemoryExt.ResizeArray<T>(ref array, ref capacity, newCapacity, true, false);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			if (!IsValid)
				return;
			MemoryExt.MemFree(array);
			this = default;
		}

		public unsafe class UnsafeListProxy
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
						arr[i] = *_arr[i];
					}

					return arr;
				}
			}
		}
	}
}
