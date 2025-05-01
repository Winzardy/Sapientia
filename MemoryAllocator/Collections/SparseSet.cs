using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct SparseSet
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public World GetAllocator()
		{
			return _values.GetAllocator();
		}

		public SparseSet(int valueSize, int capacity, int sparseCapacity, int expandStep = 0) : this(WorldManager.CurrentWorld, valueSize, capacity, sparseCapacity, expandStep) {}

		public SparseSet(WorldId worldId, int valueSize, int capacity, int sparseCapacity, int expandStep = 0) : this(worldId.GetWorld(), valueSize, capacity, sparseCapacity, expandStep) {}

		public SparseSet(World world, int valueSize, int capacity, int sparseCapacity, int expandStep = 0)
		{
			this.expandStep = expandStep == 0 ? capacity : expandStep;

			_count = 0;

			_values = new (world, valueSize, capacity, ClearOptions.ClearMemory);
			_dense = new (world, capacity, ClearOptions.ClearMemory);
			_sparse = new (world, sparseCapacity, ClearOptions.ClearMemory);

			_capacity = _dense.Length.Min(_values.Length);
			_sparseCapacity = _sparse.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr<T>(World world) where T: unmanaged
		{
			return _values.GetValuePtr<T>(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr<T>() where T: unmanaged
		{
			return _values.GetValuePtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetValuePtr()
		{
			return _values.GetPtr();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetValuePtr(World world)
		{
			return _values.GetValuePtr(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetValuePtr(int id)
		{
			return GetValuePtr(GetAllocator(), id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetValuePtr(World world, int id)
		{
			return _values.GetValuePtr(world, _sparse[world, id]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr<T>(World world, int id) where T: unmanaged
		{
			return _values.GetValuePtr<T>(world, _sparse[world, id]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetValuePtrByDenseId(World world, int denseId)
		{
			return _values.GetValuePtr(world, denseId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtrByDenseId<T>(World world, int denseId) where T: unmanaged
		{
			return _values.GetValuePtr<T>(world, denseId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetIdByIndex(World world, int denseId)
		{
			return _sparse[world, denseId];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get<T>(World world, int id) where T: unmanaged
		{
			return ref _values.GetValue<T>(world, _sparse[world, id]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get<T>(int id) where T: unmanaged
		{
			return ref Get<T>(GetAllocator(), id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetByDenseId<T>(World world, int denseId) where T: unmanaged
		{
			return ref _values.GetValue<T>(world, denseId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetDenseId(World world, int id, out int denseId)
		{
			if (_sparseCapacity <= id)
			{
				denseId = 0;
				return false;
			}
			denseId = _sparse[world, id];
			return denseId < _count && _dense[world, denseId] == id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(World world, int id)
		{
			if (_sparseCapacity <= id)
				return false;
			var denseId = _sparse[world, id];
			return denseId < _count && _dense[world, denseId] == id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(int id)
		{
			return Has(GetAllocator(), id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T EnsureGet<T>(World world, int id) where T: unmanaged
		{
			ExpandSparseIfNeeded(id + 1);
			ref var denseId = ref _sparse[world, id];
			if (denseId >= _count || _dense[world, denseId] != id)
			{
				ExpandIfNeeded(_count + 1);
				_dense[world, _count] = id;
				denseId = _count++;
			}
			return ref _values.GetValue<T>(world, denseId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T EnsureGet<T>(int id) where T: unmanaged
		{
			ExpandSparseIfNeeded(id + 1);
			ref var denseId = ref _sparse[id];
			if (denseId >= _count || _dense[denseId] != id)
			{
				ExpandIfNeeded(_count + 1);
				_dense[_count] = id;
				denseId = _count++;
			}
			return ref _values.GetValue<T>(denseId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveSwapBack(int id)
		{
			var allocator = GetAllocator();
			return RemoveSwapBack(allocator, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveSwapBack(World world, int id)
		{
			if (id >= _sparseCapacity)
				return false;
			var denseId = _sparse[world, id];
			if (denseId >= _count)
				return false;

			var denseRaw = _dense.GetValuePtr(world);
			if (denseRaw[denseId] != id)
				return false;

			var sparseId = denseRaw[denseId] = denseRaw[--_count];
			_sparse[world, sparseId] = denseId;

			var valueA = _values.GetValuePtr(world, denseId);
			var valueB = _values.GetValuePtr(world, _count);
			var size = _values.ElementSize;

			MemoryExt.MemMove(valueB, valueA, size);
			MemoryExt.MemClear(valueB, size);

			E.ASSERT(_count >= 0);
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveSwapBackByDenseId(World world, int denseId)
		{
			var denseRaw = _dense.GetValuePtr(world);

			var sparseId = denseRaw[denseId] = denseRaw[--_count];
			_sparse[world, sparseId] = denseId;

			var valueA = _values.GetValuePtr(world, denseId);
			var valueB = _values.GetValuePtr(world, _count);
			var size = _values.ElementSize;

			MemoryExt.MemMove(valueB, valueA, size);
			MemoryExt.MemClear(valueB, size);

			E.ASSERT(_count >= 0);
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ExpandSparseIfNeeded(int newCapacity)
		{
			if (_sparseCapacity >= newCapacity)
				return;

			newCapacity = SnapCeilCapacity(newCapacity);

			ExpandSparse(GetAllocator(), newCapacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ExpandSparse(World world, int newCapacity)
		{
			_sparse.Resize(world, newCapacity, ClearOptions.ClearMemory);
			_sparseCapacity = _sparse.Length;
		}

		private void ExpandIfNeeded(int newCapacity)
		{
			if (_capacity >= newCapacity)
				return;

			newCapacity = SnapCeilCapacity(newCapacity);

			Expand(GetAllocator(), newCapacity);
		}

		private void Expand(World world, int newCapacity)
		{
			_dense.Resize(world, newCapacity, ClearOptions.ClearMemory);
			_values.Resize(world, newCapacity, ElementSize, ClearOptions.ClearMemory);

			_capacity = _dense.Length.Min(_values.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int SnapCeilCapacity(int newCapacity)
		{
			return ((newCapacity + expandStep - 1) / expandStep) * expandStep;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			Dispose(GetAllocator());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(World world)
		{
			_sparse.Dispose(world);
			_dense.Dispose(world);
			_values.Dispose(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(World world)
		{
			_values.Clear(world, 0, _count);
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			_values.Clear(0, _count);
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast()
		{
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator<T> GetEnumerator<T>(World world) where T: unmanaged
		{
			return new ListEnumerator<T>(GetValuePtr<T>(world), Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator<T> GetEnumerator<T>() where T: unmanaged
		{
			return new ListEnumerator<T>(GetValuePtr<T>(), Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListPtrEnumerator<T> GetPtrEnumerator<T>(World world) where T: unmanaged
		{
			return new ListPtrEnumerator<T>(GetValuePtr(world), ElementSize, Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListPtrEnumerator<T> GetPtrEnumerator<T>() where T: unmanaged
		{
			return new ListPtrEnumerator<T>(GetValuePtr(), ElementSize, Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable<T>(World world) where T: unmanaged
		{
			return new (new (GetValuePtr<T>(world), Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable<T>() where T: unmanaged
		{
			return new (new (GetValuePtr<T>(), Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<SafePtr<T>, ListPtrEnumerator<T>> GetPtrEnumerable<T>(World world) where T: unmanaged
		{
			return new (new (GetValuePtr(world), ElementSize, Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<SafePtr<T>, ListPtrEnumerator<T>> GetPtrEnumerable<T>() where T: unmanaged
		{
			return new (new (GetValuePtr(), ElementSize, Count));
		}
	}
}
