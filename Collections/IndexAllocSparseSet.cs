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

		private int _count;

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _count;
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
			_count = 0;
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
				_ids.Push(_count + 1);

			var id = _ids.Pop();
			_count++;

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
			_count--;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			_ids.Clear();
			_sparseSet.Dispose();
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			_ids.Clear();
			_sparseSet.Clear();
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast()
		{
			_ids.Clear();
			_sparseSet.ClearFast();
			_count = 0;
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
