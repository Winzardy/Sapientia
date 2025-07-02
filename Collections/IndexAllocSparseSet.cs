using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sapientia.Collections
{
	public class IndexAllocSparseSet<T> : IDisposable, IEnumerable<T>
	{
		private Stack<int> _ids;
		private SparseSet<T> _sparseSet;
		private int _nextIdToAllocate;

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _sparseSet.Count;
		}

		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _sparseSet.Capacity;
		}

		public bool IsFull
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _sparseSet.IsFull;
		}

		public IndexAllocSparseSet(int capacity, int sparseCapacity, int expandStep = 0)
		{
			_ids = new Stack<int>(capacity);
			_sparseSet = new SparseSet<T>(capacity, sparseCapacity, expandStep);
			_nextIdToAllocate = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get(int id)
		{
			return ref _sparseSet.Get(id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(int id)
		{
			return _sparseSet.Has(id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int AllocateId()
		{
			if (_ids.Count <= 0)
				_ids.Push(_nextIdToAllocate++);

			var id = _ids.Pop();
			_sparseSet.EnsureGet(id);
			return id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReleaseIndex(int index)
		{
			var id = _sparseSet.GetIdByDenseId(index);
			ReleaseId(id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReleaseId(int id)
		{
			_sparseSet.RemoveSwapBack(id);
			_ids.Push(id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			_ids.Clear();
			_sparseSet.Dispose();
			_nextIdToAllocate = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			_ids.Clear();
			_sparseSet.Clear();
			_nextIdToAllocate = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast()
		{
			_ids.Clear();
			_sparseSet.ClearFast();
			_nextIdToAllocate = 0;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _sparseSet.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
