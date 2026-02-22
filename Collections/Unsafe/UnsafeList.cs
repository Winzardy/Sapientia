using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Submodules.Sapientia.Data;
using Submodules.Sapientia.Memory;

namespace Sapientia.Collections
{
	[DebuggerTypeProxy(typeof(UnsafeList<>.UnsafeListProxy))]
	public struct UnsafeList<T> : IDisposable
		where T : unmanaged
	{
		public SafePtr<T> ptr;
		public int count;
		public int capacity;

		public readonly Id<MemoryManager> memoryId;

		public bool IsCreated => ptr != default;

		public UnsafeList(int capacity) : this(default, capacity)
		{
		}

		public UnsafeList(Id<MemoryManager> memoryId, int capacity)
		{
			if (capacity <= 0)
			{
				this = default;
				return;
			}

			this.ptr = memoryId.GetManager().MakeArray<T>(capacity, ClearOptions.UninitializedMemory);
			this.count = 0;
			this.capacity = capacity;
			this.memoryId = memoryId;
		}

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
		public void AddRange(in UnsafeList<T> items)
		{
			EnsureCapacity(count + items.count);

			items.GetSpan().CopyTo(ptr.GetSpan(count, items.count));
			count += items.count;
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
		public readonly Span<T> GetSpan()
		{
			return ptr.GetSpan(0, count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureCapacity(int newCapacity)
		{
			if (newCapacity <= capacity)
				return;
			memoryId.GetManager().ResizeArray<T>(ref ptr, ref capacity, newCapacity, true, ClearOptions.UninitializedMemory);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			if (!IsCreated)
				return;

			memoryId.GetManager().MemFree(ptr);

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

		public Span<T>.Enumerator GetEnumerator()
		{
			return GetSpan().GetEnumerator();
		}
	}
}
