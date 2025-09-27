using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Submodules.Sapientia.Memory;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public struct MemSparseSet
	{
		public readonly int expandStep;

		private MemArray _values;
		private MemArray<int> _dense;
		private MemArray<int> _sparse;

		private int _count;
		private int _capacity;
		private int _sparseCapacity;

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _count;
		}

		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _capacity;
		}

		public int FreeIndexesCapacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _sparseCapacity;
		}

		public bool IsFull
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _count >= _capacity;
		}

		public bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _values.IsCreated;
		}

		public int ElementSize
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _values.ElementSize;
		}

		public MemSparseSet(int valueSize, int capacity, int sparseCapacity, int expandStep = 0) : this(WorldManager.CurrentWorldState, valueSize, capacity, sparseCapacity, expandStep) {}

		public MemSparseSet(WorldId worldId, int valueSize, int capacity, int sparseCapacity, int expandStep = 0) : this(worldId.GetWorldState(), valueSize, capacity, sparseCapacity, expandStep) {}

		public MemSparseSet(WorldState worldState, int valueSize, int capacity, int sparseCapacity, int expandStep = 0)
		{
			this.expandStep = expandStep == 0 ? capacity : expandStep;

			_count = 0;

			_values = new (worldState, valueSize, capacity, ClearOptions.ClearMemory);
			_dense = new (worldState, capacity, ClearOptions.ClearMemory);
			_sparse = new (worldState, sparseCapacity, ClearOptions.ClearMemory);

			_capacity = _dense.Length.Min(_values.Length);
			_sparseCapacity = _sparse.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr<T>(WorldState worldState) where T: unmanaged
		{
			return _values.GetValuePtr<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetValuePtr(WorldState worldState)
		{
			return _values.GetValuePtr(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetValuePtr(WorldState worldState, int id)
		{
			return _values.GetValuePtr(worldState, _sparse[worldState, id]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr<T>(WorldState worldState, int id) where T: unmanaged
		{
			return _values.GetValuePtr<T>(worldState, _sparse[worldState, id]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> GetSpan<T>(WorldState worldState) where T: unmanaged
		{
			return GetValuePtr<T>(worldState).GetSpan(0, _count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetValuePtrByDenseId(WorldState worldState, int denseId)
		{
			return _values.GetValuePtr(worldState, denseId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtrByDenseId<T>(WorldState worldState, int denseId) where T: unmanaged
		{
			return _values.GetValuePtr<T>(worldState, denseId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetIdByDenseId(WorldState worldState, int denseId)
		{
			return _dense[worldState, denseId];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get<T>(WorldState worldState, int id) where T: unmanaged
		{
			return ref _values.GetValue<T>(worldState, _sparse[worldState, id]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetByDenseId<T>(WorldState worldState, int denseId) where T: unmanaged
		{
			return ref _values.GetValue<T>(worldState, denseId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetDenseId(WorldState worldState, int id, out int denseId)
		{
			if (_sparseCapacity <= id)
			{
				denseId = 0;
				return false;
			}
			denseId = _sparse[worldState, id];
			return denseId < _count && _dense[worldState, denseId] == id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(WorldState worldState, int id)
		{
			if (_sparseCapacity <= id)
				return false;
			var denseId = _sparse[worldState, id];
			return denseId < _count && _dense[worldState, denseId] == id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T EnsureGet<T>(WorldState worldState, int id) where T: unmanaged
		{
			ExpandSparseIfNeeded(worldState, id + 1);
			ref var denseId = ref _sparse[worldState, id];
			if (denseId >= _count || _dense[worldState, denseId] != id)
			{
				ExpandIfNeeded(worldState, _count + 1);
				_dense[worldState, _count] = id;
				denseId = _count++;
			}
			return ref _values.GetValue<T>(worldState, denseId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveSwapBack(WorldState worldState, int id)
		{
			if (id >= _sparseCapacity)
				return false;
			var denseId = _sparse[worldState, id];
			if (denseId >= _count)
				return false;

			var denseRaw = _dense.GetValuePtr(worldState);
			if (denseRaw[denseId] != id)
				return false;

			var sparseId = denseRaw[denseId] = denseRaw[--_count];
			_sparse[worldState, sparseId] = denseId;

			var valueA = _values.GetValuePtr(worldState, denseId);
			var valueB = _values.GetValuePtr(worldState, _count);
			var size = _values.ElementSize;

			MemoryExt.MemMove(valueB, valueA, size);
			MemoryExt.MemClear(valueB, size);

			E.ASSERT(_count >= 0);
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveSwapBackByDenseId(WorldState worldState, int denseId)
		{
			var denseRaw = _dense.GetValuePtr(worldState);

			var sparseId = denseRaw[denseId] = denseRaw[--_count];
			_sparse[worldState, sparseId] = denseId;

			var valueA = _values.GetValuePtr(worldState, denseId);
			var valueB = _values.GetValuePtr(worldState, _count);
			var size = _values.ElementSize;

			MemoryExt.MemMove(valueB, valueA, size);
			MemoryExt.MemClear(valueB, size);

			E.ASSERT(_count >= 0);
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ExpandSparseIfNeeded(WorldState worldState, int newCapacity)
		{
			if (_sparseCapacity >= newCapacity)
				return;

			newCapacity = SnapCeilCapacity(newCapacity);

			ExpandSparse(worldState, newCapacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ExpandSparse(WorldState worldState, int newCapacity)
		{
			_sparse.Resize(worldState, newCapacity, ClearOptions.ClearMemory);
			_sparseCapacity = _sparse.Length;
		}

		private void ExpandIfNeeded(WorldState worldState, int newCapacity)
		{
			if (_capacity >= newCapacity)
				return;

			newCapacity = SnapCeilCapacity(newCapacity);

			Expand(worldState, newCapacity);
		}

		private void Expand(WorldState worldState, int newCapacity)
		{
			_dense.Resize(worldState, newCapacity, ClearOptions.ClearMemory);
			_values.Resize(worldState, newCapacity, ElementSize, ClearOptions.ClearMemory);

			_capacity = _dense.Length.Min(_values.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int SnapCeilCapacity(int newCapacity)
		{
			return ((newCapacity + expandStep - 1) / expandStep) * expandStep;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(WorldState worldState)
		{
			_sparse.Dispose(worldState);
			_dense.Dispose(worldState);
			_values.Dispose(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(WorldState worldState)
		{
			_values.Clear(worldState, 0, _count);
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast()
		{
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemListEnumerator<T> GetEnumerator<T>(WorldState worldState) where T: unmanaged
		{
			return new MemListEnumerator<T>(GetValuePtr<T>(worldState), Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemListEnumerable<T> GetEnumerable<T>(WorldState worldState) where T: unmanaged
		{
			return new (GetEnumerator<T>(worldState));
		}
	}
}
