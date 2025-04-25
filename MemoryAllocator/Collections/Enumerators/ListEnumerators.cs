using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public unsafe interface IListEnumerable<T> : IEnumerable<T>, IEnumerable<SafePtr<T>>
		where T: unmanaged
	{
		public int Count { get; }
		public SafePtr<T> GetValuePtr();
		public SafePtr<T> GetValuePtr(Allocator allocator);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator<T> GetEnumerator(Allocator allocator)
		{
			return new ListEnumerator<T>(GetValuePtr(allocator), Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public new ListEnumerator<T> GetEnumerator()
		{
			return new ListEnumerator<T>(GetValuePtr(), Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListPtrEnumerator<T> GetPtrEnumerator(Allocator allocator)
		{
			return new ListPtrEnumerator<T>(GetValuePtr(allocator), 0, Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListPtrEnumerator<T> GetPtrEnumerator()
		{
			return new ListPtrEnumerator<T>(GetValuePtr(), 0, Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable(Allocator allocator)
		{
			return new (new (GetValuePtr(allocator), Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable()
		{
			return new (new (GetValuePtr(), Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<SafePtr<T>, ListPtrEnumerator<T>> GetPtrEnumerable(Allocator allocator)
		{
			return new (new (GetValuePtr(allocator), 0, Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<SafePtr<T>, ListPtrEnumerator<T>> GetPtrEnumerable()
		{
			return new (new (GetValuePtr(), 0, Count));
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
			return GetPtrEnumerator();
		}
	}

	public unsafe struct ListPtrEnumerator<T> : IEnumerator<SafePtr>, IEnumerator<SafePtr<T>>
		where T: unmanaged
	{
		private readonly SafePtr<T> _valuePtr;
		private readonly int _count;

		private int _index;

		SafePtr<T> IEnumerator<SafePtr<T>>.Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _valuePtr + _index;
		}

		public SafePtr Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _valuePtr + _index;
		}

		object IEnumerator.Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Current;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListPtrEnumerator(SafePtr<T> valuePtr, int startIndex, int count)
		{
			_valuePtr = valuePtr;
			_index = startIndex - 1;
			_count = count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			return ++_index < _count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			_index = -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			this = default;
		}
	}

	public unsafe struct ListEnumerator<T> : IEnumerator<T> where T: unmanaged
	{
		private readonly SafePtr<T> _valuePtr;
		private readonly int _count;

		private int _index;

		public T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _valuePtr[_index];
		}

		object IEnumerator.Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Current;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator(SafePtr<T> valuePtr, int count)
		{
			_valuePtr = valuePtr;
			_index = -1;
			_count = count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator(SafePtr<T> valuePtr, int startIndex, int count)
		{
			_valuePtr = valuePtr;
			_index = startIndex;
			_count = count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			_index++;
			return _index < _count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			_index = -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose() {}
	}
}
