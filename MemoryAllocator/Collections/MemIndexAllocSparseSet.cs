using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public struct MemIndexAllocSparseSet<T> where T: unmanaged
	{
		private MemStack<int> _ids;
		private MemSparseSet<T> _sparseSet;
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

		public MemIndexAllocSparseSet(WorldState worldState, int capacity, int sparseCapacity, int expandStep = 0)
		{
			_ids = new MemStack<int>(worldState, capacity);
			_sparseSet = new MemSparseSet<T>(worldState, capacity, sparseCapacity, expandStep);
			_nextIdToAllocate = 0;
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
				_ids.Push(worldState, _nextIdToAllocate++);

			var id = _ids.Pop(worldState);
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
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(WorldState worldState)
		{
			_ids.Dispose(worldState);
			_sparseSet.Dispose(worldState);
			_nextIdToAllocate = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(WorldState worldState)
		{
			_ids.Clear();
			_sparseSet.Clear(worldState);
			_nextIdToAllocate = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast()
		{
			_ids.Clear();
			_sparseSet.ClearFast();
			_nextIdToAllocate = 0;
		}
	}
}
