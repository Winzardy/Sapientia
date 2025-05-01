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
		public Allocator GetAllocator()
		{
			return _values.GetAllocator();
		}

		public SparseSet(int valueSize, int capacity, int sparseCapacity, int expandStep = 0) : this(AllocatorManager.CurrentAllocator, valueSize, capacity, sparseCapacity, expandStep) {}

		public SparseSet(AllocatorId allocatorId, int valueSize, int capacity, int sparseCapacity, int expandStep = 0) : this(allocatorId.GetAllocator(), valueSize, capacity, sparseCapacity, expandStep) {}

		public SparseSet(Allocator allocator, int valueSize, int capacity, int sparseCapacity, int expandStep = 0)
		{
			this.expandStep = expandStep == 0 ? capacity : expandStep;

			_count = 0;

			_values = new (allocator, valueSize, capacity, ClearOptions.ClearMemory);
			_dense = new (allocator, capacity, ClearOptions.ClearMemory);
			_sparse = new (allocator, sparseCapacity, ClearOptions.ClearMemory);

			_capacity = _dense.Length.Min(_values.Length);
			_sparseCapacity = _sparse.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr<T>(Allocator allocator) where T: unmanaged
		{
			return _values.GetValuePtr<T>(allocator);
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
		public SafePtr GetValuePtr(Allocator allocator)
		{
			return _values.GetValuePtr(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetValuePtr(int id)
		{
			return GetValuePtr(GetAllocator(), id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetValuePtr(Allocator allocator, int id)
		{
			return _values.GetValuePtr(allocator, _sparse[allocator, id]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr<T>(Allocator allocator, int id) where T: unmanaged
		{
			return _values.GetValuePtr<T>(allocator, _sparse[allocator, id]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetValuePtrByDenseId(Allocator allocator, int denseId)
		{
			return _values.GetValuePtr(allocator, denseId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtrByDenseId<T>(Allocator allocator, int denseId) where T: unmanaged
		{
			return _values.GetValuePtr<T>(allocator, denseId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetIdByIndex(Allocator allocator, int denseId)
		{
			return _sparse[allocator, denseId];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get<T>(Allocator allocator, int id) where T: unmanaged
		{
			return ref _values.GetValue<T>(allocator, _sparse[allocator, id]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get<T>(int id) where T: unmanaged
		{
			return ref Get<T>(GetAllocator(), id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetByDenseId<T>(Allocator allocator, int denseId) where T: unmanaged
		{
			return ref _values.GetValue<T>(allocator, denseId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetDenseId(Allocator allocator, int id, out int denseId)
		{
			if (_sparseCapacity <= id)
			{
				denseId = 0;
				return false;
			}
			denseId = _sparse[allocator, id];
			return denseId < _count && _dense[allocator, denseId] == id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(Allocator allocator, int id)
		{
			if (_sparseCapacity <= id)
				return false;
			var denseId = _sparse[allocator, id];
			return denseId < _count && _dense[allocator, denseId] == id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(int id)
		{
			return Has(GetAllocator(), id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T EnsureGet<T>(Allocator allocator, int id) where T: unmanaged
		{
			ExpandSparseIfNeeded(id + 1);
			ref var denseId = ref _sparse[allocator, id];
			if (denseId >= _count || _dense[allocator, denseId] != id)
			{
				ExpandIfNeeded(_count + 1);
				_dense[allocator, _count] = id;
				denseId = _count++;
			}
			return ref _values.GetValue<T>(allocator, denseId);
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
		public bool RemoveSwapBack(Allocator allocator, int id)
		{
			if (id >= _sparseCapacity)
				return false;
			var denseId = _sparse[allocator, id];
			if (denseId >= _count)
				return false;

			var denseRaw = _dense.GetValuePtr(allocator);
			if (denseRaw[denseId] != id)
				return false;

			var sparseId = denseRaw[denseId] = denseRaw[--_count];
			_sparse[allocator, sparseId] = denseId;

			var valueA = _values.GetValuePtr(allocator, denseId);
			var valueB = _values.GetValuePtr(allocator, _count);
			var size = _values.ElementSize;

			MemoryExt.MemMove(valueB, valueA, size);
			MemoryExt.MemClear(valueB, size);

			E.ASSERT(_count >= 0);
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveSwapBackByDenseId(Allocator allocator, int denseId)
		{
			var denseRaw = _dense.GetValuePtr(allocator);

			var sparseId = denseRaw[denseId] = denseRaw[--_count];
			_sparse[allocator, sparseId] = denseId;

			var valueA = _values.GetValuePtr(allocator, denseId);
			var valueB = _values.GetValuePtr(allocator, _count);
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
		private void ExpandSparse(Allocator allocator, int newCapacity)
		{
			_sparse.Resize(allocator, newCapacity, ClearOptions.ClearMemory);
			_sparseCapacity = _sparse.Length;
		}

		private void ExpandIfNeeded(int newCapacity)
		{
			if (_capacity >= newCapacity)
				return;

			newCapacity = SnapCeilCapacity(newCapacity);

			Expand(GetAllocator(), newCapacity);
		}

		private void Expand(Allocator allocator, int newCapacity)
		{
			_dense.Resize(allocator, newCapacity, ClearOptions.ClearMemory);
			_values.Resize(allocator, newCapacity, ElementSize, ClearOptions.ClearMemory);

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
		public void Dispose(Allocator allocator)
		{
			_sparse.Dispose(allocator);
			_dense.Dispose(allocator);
			_values.Dispose(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(Allocator allocator)
		{
			_values.Clear(allocator, 0, _count);
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
		public ListEnumerator<T> GetEnumerator<T>(Allocator allocator) where T: unmanaged
		{
			return new ListEnumerator<T>(GetValuePtr<T>(allocator), Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator<T> GetEnumerator<T>() where T: unmanaged
		{
			return new ListEnumerator<T>(GetValuePtr<T>(), Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListPtrEnumerator<T> GetPtrEnumerator<T>(Allocator allocator) where T: unmanaged
		{
			return new ListPtrEnumerator<T>(GetValuePtr(allocator), ElementSize, Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListPtrEnumerator<T> GetPtrEnumerator<T>() where T: unmanaged
		{
			return new ListPtrEnumerator<T>(GetValuePtr(), ElementSize, Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable<T>(Allocator allocator) where T: unmanaged
		{
			return new (new (GetValuePtr<T>(allocator), Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable<T>() where T: unmanaged
		{
			return new (new (GetValuePtr<T>(), Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<SafePtr<T>, ListPtrEnumerator<T>> GetPtrEnumerable<T>(Allocator allocator) where T: unmanaged
		{
			return new (new (GetValuePtr(allocator), ElementSize, Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<SafePtr<T>, ListPtrEnumerator<T>> GetPtrEnumerable<T>() where T: unmanaged
		{
			return new (new (GetValuePtr(), ElementSize, Count));
		}
	}
}
