using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sapientia.MemoryAllocator
{
	public unsafe interface ICircularBufferEnumerable<T> : IEnumerable<T> where T: unmanaged
	{
		public int HeadIndex { get; }
		public int Count { get; }
		public int Capacity { get; }
		public int ElementSize { get; }
		public T* GetValuePtr();
		public T* GetValuePtr(Allocator* allocator);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CircularBufferEnumerator<T> GetEnumerator(Allocator* allocator)
		{
			return new CircularBufferEnumerator<T>(GetValuePtr(allocator), HeadIndex, Count, Capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public new CircularBufferEnumerator<T> GetEnumerator()
		{
			return new CircularBufferEnumerator<T>(GetValuePtr(), HeadIndex, Count, Capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CircularBufferPtrEnumerator GetPtrEnumerator(Allocator* allocator)
		{
			return new CircularBufferPtrEnumerator((byte*)GetValuePtr(allocator), ElementSize, HeadIndex, Count, Capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CircularBufferPtrEnumerator GetPtrEnumerator()
		{
			return new CircularBufferPtrEnumerator((byte*)GetValuePtr(), ElementSize, HeadIndex, Count, Capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, CircularBufferEnumerator<T>> GetEnumerable(Allocator* allocator)
		{
			return new (new (GetValuePtr(allocator), HeadIndex, Count, Capacity));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, CircularBufferEnumerator<T>> GetEnumerable()
		{
			return new (new (GetValuePtr(), HeadIndex, Count, Capacity));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<IntPtr, CircularBufferPtrEnumerator> GetPtrEnumerable(Allocator* allocator)
		{
			return new (new ((byte*)GetValuePtr(allocator), ElementSize, HeadIndex, Count, Capacity));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<IntPtr, CircularBufferPtrEnumerator> GetPtrEnumerable()
		{
			return new (new ((byte*)GetValuePtr(), ElementSize, HeadIndex, Count, Capacity));
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

	public unsafe interface ICircularBufferEnumerable : IEnumerable<IntPtr>
	{
		public int HeadIndex { get; }
		public int Count { get; }
		public int Capacity { get; }
		public int ElementSize { get; }
		public void* GetValuePtr();
		public void* GetValuePtr(Allocator* allocator);
		public T* GetValuePtr<T>() where T: unmanaged;
		public T* GetValuePtr<T>(Allocator* allocator) where T: unmanaged;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CircularBufferEnumerator<T> GetEnumerator<T>(Allocator* allocator) where T: unmanaged
		{
			return new CircularBufferEnumerator<T>(GetValuePtr<T>(allocator), HeadIndex, Count, Capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CircularBufferEnumerator<T> GetEnumerator<T>() where T: unmanaged
		{
			return new CircularBufferEnumerator<T>(GetValuePtr<T>(), HeadIndex, Count, Capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CircularBufferPtrEnumerator GetPtrEnumerator(Allocator* allocator)
		{
			return new CircularBufferPtrEnumerator((byte*)GetValuePtr(allocator), ElementSize, HeadIndex, Count, Capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CircularBufferPtrEnumerator GetPtrEnumerator()
		{
			return new CircularBufferPtrEnumerator((byte*)GetValuePtr(), ElementSize, HeadIndex, Count, Capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, CircularBufferEnumerator<T>> GetEnumerable<T>(Allocator* allocator) where T: unmanaged
		{
			return new (new (GetValuePtr<T>(allocator), HeadIndex, Count, Capacity));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, CircularBufferEnumerator<T>> GetEnumerable<T>() where T: unmanaged
		{
			return new (new (GetValuePtr<T>(), HeadIndex, Count, Capacity));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<IntPtr, CircularBufferPtrEnumerator> GetPtrEnumerable(Allocator* allocator)
		{
			return new (new ((byte*)GetValuePtr(allocator), ElementSize, HeadIndex, Count, Capacity));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<IntPtr, CircularBufferPtrEnumerator> GetPtrEnumerable()
		{
			return new (new ((byte*)GetValuePtr(), ElementSize, HeadIndex, Count, Capacity));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator<IntPtr> IEnumerable<IntPtr>.GetEnumerator()
		{
			return GetPtrEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetPtrEnumerator();
		}
	}

	public unsafe struct CircularBufferPtrEnumerator : IEnumerator<IntPtr>
	{
		private readonly byte* _valuePtr;
		private readonly int _elementSize;
		private readonly int _headBytesIndex;
		private readonly int _bytesCount;
		private readonly int _bytesCapacity;

		private int _index;
		private int _currentBytesCount;

		public IntPtr Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (IntPtr)(_valuePtr + _index);
		}

		object IEnumerator.Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Current;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CircularBufferPtrEnumerator(byte* valuePtr, int elementSize, int headIndex, int count, int capacity)
		{
			_valuePtr = valuePtr;
			_elementSize = elementSize;
			_headBytesIndex = headIndex * elementSize;
			_bytesCount = count * elementSize;
			_bytesCapacity = capacity * elementSize;

			_index = _headBytesIndex;
			_currentBytesCount = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			if (_currentBytesCount == _bytesCount)
				return false;

			_index = (_headBytesIndex + _currentBytesCount) % _bytesCapacity;
			_currentBytesCount += _elementSize;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			_index = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			this = default;
		}
	}

	public unsafe struct CircularBufferEnumerator<T> : IEnumerator<T> where T: unmanaged
	{
		private readonly T* _valuePtr;
		private readonly int _headIndex;
		private readonly int _count;
		private readonly int _capacity;

		private int _index;
		private int _currentCount;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal CircularBufferEnumerator(T* valuePtr, int headIndex, int count, int capacity)
		{
			_valuePtr = valuePtr;
			_headIndex = headIndex;
			_count = count;
			_capacity = capacity;

			_currentCount = 0;
			_index = headIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			if (_currentCount == _count)
				return false;

			_index = (_headIndex + _currentCount) % _capacity;
			_currentCount++;
			return true;
		}

		public T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => *(_valuePtr + _index);
		}

		object IEnumerator.Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Current;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			_index = _headIndex;
			_currentCount = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			this = default;
		}
	}
}
