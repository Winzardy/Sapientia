using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Data;

namespace Sapientia.Collections
{
	[StructLayout(LayoutKind.Sequential)]
	public struct UnsafeIndexAllocSparseSet<T> where T: unmanaged
	{
		private UnsafeList<int> _ids;
		private UnsafeSparseSet<T> _sparseSet;

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

		public int SparseCapacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _sparseSet.Capacity;
		}

		public bool IsFull
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _sparseSet.IsFull;
		}

		public bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _sparseSet.IsCreated;
		}

		public UnsafeIndexAllocSparseSet(int capacity, int sparseCapacity, int expandStep = 0)
		{
			_ids = new UnsafeList<int>(capacity);
			_sparseSet = new UnsafeSparseSet<T>(capacity, sparseCapacity, expandStep);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly SafePtr<T> GetValuePtr()
		{
			return _sparseSet.GetValuePtr();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T Get(int id)
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
			if (_ids.count <= 0)
				_ids.Add(Count);

			_ids.count--;
			var id = _ids[_ids.count];

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
			_ids.Add(id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			_ids.Dispose();
			_sparseSet.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			_ids.Clear();
			_sparseSet.Clear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast()
		{
			_ids.Clear();
			_sparseSet.ClearFast();
		}
	}
}
