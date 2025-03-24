using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Sapientia.Extensions;

namespace Sapientia.Collections
{
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

		public ref T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref array[index];
		}

		public void Add(T item)
		{
			EnsureCapacity(count + 1);

			array[count] = item;
			count++;
		}

		public T RemoveAt(int index)
		{
			var result = array[index];

			count--;
			if (index < count)
				MemoryExt.MemMove(array + index + 1, array + index, count - index);

			return result;
		}

		public T RemoveAtSwapBack(int index)
		{
			var result = array[index];

			array[index] = array[count - 1];
			count--;

			return result;
		}

		public T RemoveLast()
		{
			count--;
			return array[count];
		}

		public void Clear()
		{
			count = 0;
		}

		private void EnsureCapacity(int newCapacity)
		{
			if (newCapacity <= capacity)
				return;

			var newArray = MemoryExt.MakeArray<T>(newCapacity, false);

			MemoryExt.MemCopy(array, newArray, capacity);
			MemoryExt.MemFree(array);

			capacity = newCapacity;
			array = newArray;
		}

		public void Dispose()
		{
			if (!IsValid)
				return;
			MemoryExt.MemFree(array);
			this = default;
		}
	}
}
