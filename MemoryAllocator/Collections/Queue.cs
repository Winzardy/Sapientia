using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(QueueProxy<>))]
	public unsafe struct Queue<T> : ICircularBufferEnumerable<T> where T : unmanaged
	{
		private const int _minimumGrow = 4;
		private const int _growFactor = 200;

		private MemArray<T> _array;
		private int _headIndex;
		private int _tailIndex;
		private int _count;

		public readonly bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _array.IsCreated;
		}

		public int HeadIndex
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _headIndex;
		}

		public readonly int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _count;
		}

		public readonly int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _array.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Queue(SafePtr<Allocator> allocator, int capacity)
		{
			this = default;
			_array = new MemArray<T>(allocator, capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(SafePtr<Allocator> allocator)
		{
			_array.Dispose(allocator);
			this = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr()
		{
			return _array.GetValuePtr();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr(SafePtr<Allocator> allocator)
		{
			return _array.GetValuePtr(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			_headIndex = 0;
			_tailIndex = 0;
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Enqueue(SafePtr<Allocator> allocator, T item)
		{
			if (_count == _array.Length)
			{
				var newCapacity = (int)((long)_array.Length * (long)_growFactor / 100L);
				if (newCapacity < _array.Length + _minimumGrow)
				{
					newCapacity = _array.Length + _minimumGrow;
				}

				SetCapacity(allocator, newCapacity);
			}

			_array[allocator, _tailIndex] = item;
			_tailIndex = (_tailIndex + 1) % _array.Length;
			_count++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Dequeue(SafePtr<Allocator> allocator)
		{
			var removed = _array[allocator, _headIndex];
			_array[allocator, _headIndex] = default;
			_headIndex = (_headIndex + 1) % _array.Length;
			_count--;
			return removed;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Peek(SafePtr<Allocator> allocator)
		{
			return _array[allocator, _headIndex];
		}

		public bool Contains<TU>(SafePtr<Allocator> allocator, TU item) where TU : System.IEquatable<T>
		{
			var index = _headIndex;
			var count = _count;

			while (count-- > 0)
			{
				if (item.Equals(_array[allocator, index]))
				{
					return true;
				}

				index = (index + 1) % _array.Length;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private T GetElement(SafePtr<Allocator> allocator, int i)
		{
			return _array[allocator, (_headIndex + i) % _array.Length];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetCapacity(SafePtr<Allocator> allocator, int capacity)
		{
			_array.Resize(allocator, capacity);
			_headIndex = 0;
			_tailIndex = _count == capacity ? 0 : _count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CircularBufferEnumerator<T> GetEnumerator(SafePtr<Allocator> allocator)
		{
			return new CircularBufferEnumerator<T>(GetValuePtr(allocator), HeadIndex, Count, Capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public new CircularBufferEnumerator<T> GetEnumerator()
		{
			return new CircularBufferEnumerator<T>(GetValuePtr(), HeadIndex, Count, Capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CircularBufferPtrEnumerator<T> GetPtrEnumerator(SafePtr<Allocator> allocator)
		{
			return new CircularBufferPtrEnumerator<T>(GetValuePtr(allocator), HeadIndex, Count, Capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CircularBufferPtrEnumerator<T> GetPtrEnumerator()
		{
			return new CircularBufferPtrEnumerator<T>(GetValuePtr(), HeadIndex, Count, Capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, CircularBufferEnumerator<T>> GetEnumerable(SafePtr<Allocator> allocator)
		{
			return new (new (GetValuePtr(allocator), HeadIndex, Count, Capacity));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, CircularBufferEnumerator<T>> GetEnumerable()
		{
			return new (new (GetValuePtr(), HeadIndex, Count, Capacity));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<SafePtr<T>, CircularBufferPtrEnumerator<T>> GetPtrEnumerable(SafePtr<Allocator> allocator)
		{
			return new (new (GetValuePtr(allocator), HeadIndex, Count, Capacity));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<SafePtr<T>, CircularBufferPtrEnumerator<T>> GetPtrEnumerable()
		{
			return new (new (GetValuePtr(), HeadIndex, Count, Capacity));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return GetEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
