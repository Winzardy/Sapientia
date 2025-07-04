using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public struct MemSparseSet<T> : IMemListEnumerable<T> where T : unmanaged
	{
		private MemSparseSet _innerSet;

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _innerSet.Count;
		}

		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _innerSet.Capacity;
		}

		public int FreeIndexesCapacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _innerSet.FreeIndexesCapacity;
		}

		public bool IsFull
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _innerSet.IsFull;
		}

		public bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _innerSet.IsCreated;
		}

		public int ExpandStep
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _innerSet.expandStep;
		}

		public MemSparseSet(int capacity, int sparseCapacity, int expandStep = 0) : this(WorldManager.CurrentWorldState, capacity, sparseCapacity, expandStep) {}

		public MemSparseSet(WorldId worldId, int capacity, int sparseCapacity, int expandStep = 0) : this(worldId.GetWorldState(), capacity, sparseCapacity, expandStep) {}

		public MemSparseSet(WorldState worldState, int capacity, int sparseCapacity, int expandStep = 0)
		{
			_innerSet = new MemSparseSet(worldState, TSize<T>.size, capacity, sparseCapacity, expandStep);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr(WorldState worldState)
		{
			return _innerSet.GetValuePtr<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> GetSpan(WorldState worldState)
		{
			return _innerSet.GetSpan<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get(WorldState worldState, int id)
		{
			return ref _innerSet.Get<T>(worldState, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetIdByDenseId(WorldState worldState, int denseId)
		{
			return _innerSet.GetIdByDenseId(worldState, denseId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(WorldState worldState, int id)
		{
			return _innerSet.Has(worldState, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T EnsureGet(WorldState worldState, int id)
		{
			return ref _innerSet.EnsureGet<T>(worldState, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveSwapBack(WorldState worldState, int id)
		{
			_innerSet.RemoveSwapBack(worldState, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(WorldState worldState)
		{
			_innerSet.Dispose(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(WorldState worldState)
		{
			_innerSet.Clear(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast()
		{
			_innerSet.ClearFast();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemListEnumerator<T> GetEnumerator(WorldState worldState)
		{
			return new MemListEnumerator<T>(GetValuePtr(worldState), Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemListEnumerable<T> GetEnumerable(WorldState worldState)
		{
			return new (GetEnumerator(worldState));
		}
	}
}
