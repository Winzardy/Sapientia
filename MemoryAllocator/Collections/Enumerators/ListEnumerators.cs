using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sapientia.MemoryAllocator
{
	public unsafe interface IListEnumerable<T> : IEnumerable<T> where T: unmanaged
	{
		public int Count { get; }
		public int ElementSize { get; }
		public T* GetValuePtr();
		public T* GetValuePtr(Allocator* allocator);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator<T> GetEnumerator(Allocator* allocator)
		{
			return new ListEnumerator<T>(GetValuePtr(allocator), Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public new ListEnumerator<T> GetEnumerator()
		{
			return new ListEnumerator<T>(GetValuePtr(), Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListPtrEnumerator GetPtrEnumerator(Allocator* allocator)
		{
			return new ListPtrEnumerator((byte*)GetValuePtr(allocator), ElementSize, Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListPtrEnumerator GetPtrEnumerator()
		{
			return new ListPtrEnumerator((byte*)GetValuePtr(), ElementSize, Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable(Allocator* allocator)
		{
			return new (new (GetValuePtr(allocator), Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable()
		{
			return new (new (GetValuePtr(), Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<IntPtr, ListPtrEnumerator> GetPtrEnumerable(Allocator* allocator)
		{
			return new (new ((byte*)GetValuePtr(allocator), ElementSize, Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<IntPtr, ListPtrEnumerator> GetPtrEnumerable()
		{
			return new (new ((byte*)GetValuePtr(), ElementSize, Count));
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

	public unsafe interface IListEnumerable : IEnumerable<IntPtr>
	{
		public int Count { get; }
		public int ElementSize { get; }
		public void* GetValuePtr();
		public void* GetValuePtr(Allocator* allocator);
		public T* GetValuePtr<T>() where T: unmanaged;
		public T* GetValuePtr<T>(Allocator* allocator) where T: unmanaged;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator<T> GetEnumerator<T>(Allocator* allocator) where T: unmanaged
		{
			return new ListEnumerator<T>(GetValuePtr<T>(allocator), Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator<T> GetEnumerator<T>() where T: unmanaged
		{
			return new ListEnumerator<T>(GetValuePtr<T>(), Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListPtrEnumerator GetPtrEnumerator(Allocator* allocator)
		{
			return new ListPtrEnumerator((byte*)GetValuePtr(allocator), ElementSize, Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListPtrEnumerator GetPtrEnumerator()
		{
			return new ListPtrEnumerator((byte*)GetValuePtr(), ElementSize, Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable<T>(Allocator* allocator) where T: unmanaged
		{
			return new (new (GetValuePtr<T>(allocator), Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable<T>() where T: unmanaged
		{
			return new (new (GetValuePtr<T>(), Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<IntPtr, ListPtrEnumerator> GetPtrEnumerable(Allocator* allocator)
		{
			return new (new ((byte*)GetValuePtr(allocator), ElementSize, Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<IntPtr, ListPtrEnumerator> GetPtrEnumerable()
		{
			return new (new ((byte*)GetValuePtr(), ElementSize, Count));
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

	public unsafe struct ListPtrEnumerator : IEnumerator<IntPtr>
	{
		private readonly byte* _valuePtr;
		private readonly int _elementSize;
		private readonly int _bytesCount;

		private int _bytesIndex;

		public IntPtr Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (IntPtr)(_valuePtr + _bytesIndex);
		}

		object IEnumerator.Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Current;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListPtrEnumerator(byte* valuePtr, int elementSize, int startBytesIndex, int count)
		{
			_valuePtr = valuePtr;
			_elementSize = -elementSize;
			_bytesIndex = startBytesIndex;
			_bytesCount = count * elementSize;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListPtrEnumerator(byte* valuePtr, int elementSize, int count)
		{
			_valuePtr = valuePtr;
			_elementSize = elementSize;
			_bytesIndex = -elementSize;
			_bytesCount = count * elementSize;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			_bytesIndex += _elementSize;
			return _bytesIndex < _bytesCount;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			_bytesIndex = -_elementSize;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			this = default;
		}
	}

	public unsafe struct ListEnumerator<T> : IEnumerator<T> where T: unmanaged
	{
		private readonly T* _valuePtr;
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
		public ListEnumerator(T* valuePtr, int count)
		{
			_valuePtr = valuePtr;
			_index = -1;
			_count = count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator(T* valuePtr, int startIndex, int count)
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
