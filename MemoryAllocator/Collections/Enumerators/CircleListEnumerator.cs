using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public unsafe interface ICircularBufferEnumerable<T> : IEnumerable<T>, IEnumerable<SafePtr<T>>
		where T: unmanaged
	{
		public int HeadIndex { get; }
		public int Count { get; }
		public int Capacity { get; }
		public SafePtr<T> GetValuePtr();
		public SafePtr<T> GetValuePtr(SafePtr<Allocator> allocator);

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator<SafePtr<T>> IEnumerable<SafePtr<T>>.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public unsafe struct CircularBufferPtrEnumerator<T> : IEnumerator<SafePtr>, IEnumerator<SafePtr<T>>
		where T: unmanaged
	{
		private readonly SafePtr<T> _valuePtr;
		private readonly int _headBytesIndex;
		private readonly int _count;
		private readonly int _capacity;

		private int _index;
		private int _currentCount;

		SafePtr<T> IEnumerator<SafePtr<T>>.Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (_valuePtr + _index);
		}

		public SafePtr Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (_valuePtr + _index);
		}

		object IEnumerator.Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Current;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CircularBufferPtrEnumerator(SafePtr<T> valuePtr, int headIndex, int count, int capacity)
		{
			_valuePtr = valuePtr;
			_headBytesIndex = headIndex;
			_count = count;
			_capacity = capacity;

			_index = _headBytesIndex;
			_currentCount = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			if (_currentCount == _count)
				return false;

			_index = (_headBytesIndex + _currentCount) % _capacity;
			_currentCount ++;
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

	public unsafe struct CircularBufferEnumerator<T> : IEnumerator<T>, IEnumerator<SafePtr<T>>
		where T: unmanaged
	{
		private readonly SafePtr<T> _valuePtr;
		private readonly int _headIndex;
		private readonly int _count;
		private readonly int _capacity;

		private int _index;
		private int _currentCount;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal CircularBufferEnumerator(SafePtr<T> valuePtr, int headIndex, int count, int capacity)
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

		SafePtr<T> IEnumerator<SafePtr<T>>.Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (_valuePtr + _index);
		}

		public T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (_valuePtr + _index).Value();
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
