using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public struct IndexAllocSparseSet<T> where T: unmanaged
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

		public IndexAllocSparseSet(WorldState worldState, int capacity, int sparseCapacity, int expandStep = 0)
		{
			_ids = new Stack<int>(worldState, capacity);
			_sparseSet = new SparseSet<T>(worldState, capacity, sparseCapacity, expandStep);
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr(WorldState worldState)
		{
			return _sparseSet.GetValuePtr(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get(WorldState worldState, int id)
		{
			return ref _sparseSet.Get(worldState, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(WorldState worldState, int id)
		{
			return _sparseSet.Has(worldState, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int AllocateId(WorldState worldState)
		{
			if (_ids.Count <= 0)
				_ids.Push(worldState, _count + 1);

			var id = _ids.Pop(worldState);
			_count++;

			_sparseSet.EnsureGet(worldState, id);
			return id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReleaseIndex(WorldState worldState, int index)
		{
			var id = _sparseSet.GetIdByDenseId(worldState, index);
			ReleaseId(worldState, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReleaseId(WorldState worldState, int id)
		{
			_sparseSet.RemoveSwapBack(worldState, id);
			_ids.Push(worldState, id);
			_count--;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(WorldState worldState)
		{
			_ids.Dispose(worldState);
			_sparseSet.Dispose(worldState);
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(WorldState worldState)
		{
			_ids.Clear();
			_sparseSet.Clear(worldState);
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast()
		{
			_ids.Clear();
			_sparseSet.ClearFast();
			_count = 0;
		}
	}
}
