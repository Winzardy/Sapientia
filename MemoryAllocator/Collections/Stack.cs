using System;
using System.Collections;
using System.Collections.Generic;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(StackProxy<>))]
	public unsafe struct Stack<T> : IListEnumerable<T> where T : unmanaged
	{
		private const int _defaultCapacity = 4;

		private MemArray<T> _array;
		private int _count;

		public readonly bool IsCreated
		{
			[INLINE(256)] get => _array.IsCreated;
		}

		public readonly int Count
		{
			[INLINE(256)] get => _count;
		}

		public readonly int Capacity
		{
			[INLINE(256)] get => _array.Length;
		}

		public int ElementSize
		{
			[INLINE(256)] get => _array.ElementSize;
		}

		public bool IsFull
		{
			[INLINE(256)] get => _array.Length <= _count;
		}

		[INLINE(256)]
		public Allocator* GetAllocatorPtr()
		{
			return _array.GetAllocatorPtr();
		}

		[INLINE(256)]
		public Stack(Allocator* allocator, int capacity, byte growFactor = 1)
		{
			this = default;
			_array = new MemArray<T>(allocator, capacity, growFactor: growFactor);
		}

		[INLINE(256)]
		public T* GetValuePtr()
		{
			return _array.GetValuePtr();
		}

		[INLINE(256)]
		public T* GetValuePtr(Allocator* allocator)
		{
			return _array.GetValuePtr(allocator);
		}

		[INLINE(256)]
		public void Dispose()
		{
			Dispose(GetAllocatorPtr());
		}

		[INLINE(256)]
		public void Dispose(Allocator* allocator)
		{
			_array.Dispose(allocator);
			this = default;
		}

		[INLINE(256)]
		public void Clear()
		{
			_count = 0;
		}

		[INLINE(256)]
		public bool Contains<TU>(Allocator* allocator, TU item) where TU : IEquatable<T>
		{
			var count = _count;
			while (count-- > 0)
			{
				if (item.Equals(_array[allocator, count]))
				{
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public readonly T Peek(Allocator* allocator)
		{
			return _array[allocator, _count - 1];
		}

		[INLINE(256)]
		public T Pop(Allocator* allocator)
		{
			var item = _array[allocator, --_count];
			_array[allocator, _count] = default;
			return item;
		}

		[INLINE(256)]
		public void Push(Allocator* allocator, T item)
		{
			if (_count == _array.Length)
			{
				_array.Resize(allocator, _array.Length == 0 ? _defaultCapacity : 2 * _array.Length);
			}

			_array[allocator, _count++] = item;
		}

		[INLINE(256)]
		public void PushNoChecks(T item, T* ptr)
		{
			*ptr = item;
			++_count;
		}

		[INLINE(256)]
		public int GetReservedSizeInBytes()
		{
			return _array.GetReservedSizeInBytes();
		}

		[INLINE(256)]
		public ListEnumerator<T> GetEnumerator(Allocator* allocator)
		{
			return new ListEnumerator<T>(GetValuePtr(allocator), Count);
		}

		[INLINE(256)]
		public new ListEnumerator<T> GetEnumerator()
		{
			return new ListEnumerator<T>(GetValuePtr(), Count);
		}

		[INLINE(256)]
		public ListPtrEnumerator GetPtrEnumerator(Allocator* allocator)
		{
			return new ListPtrEnumerator((byte*)GetValuePtr(allocator), ElementSize, Count);
		}

		[INLINE(256)]
		public ListPtrEnumerator GetPtrEnumerator()
		{
			return new ListPtrEnumerator((byte*)GetValuePtr(), ElementSize, Count);
		}

		[INLINE(256)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable(Allocator* allocator)
		{
			return new (new (GetValuePtr(allocator), Count));
		}

		[INLINE(256)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable()
		{
			return new (new (GetValuePtr(), Count));
		}

		[INLINE(256)]
		public Enumerable<IntPtr, ListPtrEnumerator> GetPtrEnumerable(Allocator* allocator)
		{
			return new (new ((byte*)GetValuePtr(allocator), ElementSize, Count));
		}

		[INLINE(256)]
		public Enumerable<IntPtr, ListPtrEnumerator> GetPtrEnumerable()
		{
			return new (new ((byte*)GetValuePtr(), ElementSize, Count));
		}

		[INLINE(256)]
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return GetEnumerator();
		}

		[INLINE(256)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
