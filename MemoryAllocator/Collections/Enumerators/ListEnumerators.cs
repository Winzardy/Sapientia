using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public interface IListEnumerable<T>
		where T: unmanaged
	{
		public int Count { get; }
		public SafePtr<T> GetValuePtr(World world);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator<T> GetEnumerator(World world)
		{
			return new ListEnumerator<T>(GetValuePtr(world), Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListPtrEnumerator<T> GetPtrEnumerator(World world)
		{
			return new ListPtrEnumerator<T>(GetValuePtr(world), 0, Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable(World world)
		{
			return new (new (GetValuePtr(world), Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<SafePtr<T>, ListPtrEnumerator<T>> GetPtrEnumerable(World world)
		{
			return new (new (GetValuePtr(world), 0, Count));
		}
	}

	public struct ListPtrEnumerator<T> : IEnumerator<SafePtr>, IEnumerator<SafePtr<T>>
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

	public struct ListEnumerator<T> : IEnumerator<T> where T: unmanaged
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
			return ++_index < _count;
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
