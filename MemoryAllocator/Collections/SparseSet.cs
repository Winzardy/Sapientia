using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct SparseSet : IListEnumerable
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

		public int ElementSize
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _values.ElementSize;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Allocator* GetAllocatorPtr()
		{
			return _values.GetAllocatorPtr();
		}

		public SparseSet(int valueSize, int capacity, int sparseCapacity, int expandStep = 0) : this(AllocatorManager.CurrentAllocatorPtr, valueSize, capacity, sparseCapacity, expandStep) {}

		public SparseSet(AllocatorId allocatorId, int valueSize, int capacity, int sparseCapacity, int expandStep = 0) : this(allocatorId.GetAllocatorPtr(), valueSize, capacity, sparseCapacity, expandStep) {}

		public SparseSet(Allocator* allocator, int valueSize, int capacity, int sparseCapacity, int expandStep = 0)
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
		public T* GetValuePtr<T>(Allocator* allocator) where T: unmanaged
		{
			return _values.GetValuePtr<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetValuePtr<T>() where T: unmanaged
		{
			return _values.GetValuePtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void* GetValuePtr()
		{
			return _values.GetPtr();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void* GetValuePtr(Allocator* allocator)
		{
			return _values.GetValuePtr(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void* GetValuePtr(int id)
		{
			return GetValuePtr(GetAllocatorPtr(), id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void* GetValuePtr(Allocator* allocator, int id)
		{
			return _values.GetValuePtr(allocator, _sparse[allocator, id]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetValuePtr<T>(Allocator* allocator, int id) where T: unmanaged
		{
			return _values.GetValuePtr<T>(allocator, _sparse[allocator, id]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get<T>(Allocator* allocator, int id) where T: unmanaged
		{
			return ref _values.GetValue<T>(allocator, _sparse[allocator, id]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get<T>(int id) where T: unmanaged
		{
			return ref Get<T>(GetAllocatorPtr(), id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(Allocator* allocator, int id)
		{
			if (_sparseCapacity <= id)
				return false;
			var denseId = _sparse[allocator, id];
			return denseId < _count && _dense[allocator, denseId] == id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(int id)
		{
			return Has(GetAllocatorPtr(), id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T EnsureGet<T>(Allocator* allocator, int id) where T: unmanaged
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
			ExpandSparseIfNeeded(id);
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
		public void RemoveSwapBack(int id)
		{
			if (id >= _sparseCapacity)
				return;
			var allocator = GetAllocatorPtr();
			var denseId = _sparse[allocator, id];
			if (denseId >= _count || _dense[allocator, denseId] != id)
				return;

			var sparseId = _dense[allocator, denseId] = _dense[allocator, --_count];
			_sparse[allocator, sparseId] = denseId;

			var valueA = _values.GetValuePtr(allocator, denseId);
			var valueB = _values.GetValuePtr(allocator, _count);
			var size = _values.ElementSize;

			MemoryExt.MemCopy(valueB, valueA, size);
			MemoryExt.MemClear(valueB, size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveSwapBack(Allocator* allocator, int id)
		{
			if (id >= _sparseCapacity)
				return;
			var denseId = _sparse[allocator, id];
			if (denseId >= _count || _dense[allocator, denseId] != id)
				return;

			var sparseId = _dense[allocator, denseId] = _dense[allocator, --_count];
			_sparse[allocator, sparseId] = denseId;

			var valueA = _values.GetValuePtr(allocator, denseId);
			var valueB = _values.GetValuePtr(allocator, _count);
			var size = _values.ElementSize;

			MemoryExt.MemCopy(valueB, valueA, size);
			MemoryExt.MemClear(valueB, size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ExpandSparseIfNeeded(int newCapacity)
		{
			if (_sparseCapacity >= newCapacity)
				return;

			newCapacity = SnapCeilCapacity(newCapacity);

			ExpandSparse(GetAllocatorPtr(), newCapacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ExpandSparse(Allocator* allocator, int newCapacity)
		{
			_sparse.Resize(allocator, newCapacity, ClearOptions.ClearMemory);
			_sparseCapacity = _sparse.Length;
		}

		private void ExpandIfNeeded(int newCapacity)
		{
			if (_capacity >= newCapacity)
				return;

			newCapacity = SnapCeilCapacity(newCapacity);

			Expand(GetAllocatorPtr(), newCapacity);
		}

		private void Expand(Allocator* allocator, int newCapacity)
		{
			_dense.Resize(allocator, newCapacity, ClearOptions.ClearMemory);
			_values.Resize(allocator, newCapacity, ClearOptions.ClearMemory);

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
			Dispose(GetAllocatorPtr());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(Allocator* allocator)
		{
			_sparse.Dispose(allocator);
			_dense.Dispose(allocator);
			_values.Dispose(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(Allocator* allocator)
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
		public ListEnumerator<T> GetEnumerator<T>(Allocator* allocator) where T: unmanaged
		{
			return new ListEnumerator<T>(GetValuePtr<T>(allocator), Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator<T> GetEnumerator<T>() where T: unmanaged
		{
			return new ListEnumerator<T>(GetValuePtr<T>(), Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListPtrEnumerator GetPtrEnumerator(Allocator* allocator)
		{
			return new ListPtrEnumerator((byte*)GetValuePtr(allocator), ElementSize, Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListPtrEnumerator GetPtrEnumerator()
		{
			return new ListPtrEnumerator((byte*)GetValuePtr(), ElementSize, Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable<T>(Allocator* allocator) where T: unmanaged
		{
			return new (new (GetValuePtr<T>(allocator), Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable<T>() where T: unmanaged
		{
			return new (new (GetValuePtr<T>(), Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<IntPtr, ListPtrEnumerator> GetPtrEnumerable(Allocator* allocator)
		{
			return new (new ((byte*)GetValuePtr(allocator), ElementSize, Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<IntPtr, ListPtrEnumerator> GetPtrEnumerable()
		{
			return new (new ((byte*)GetValuePtr(), ElementSize, Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator<IntPtr> IEnumerable<IntPtr>.GetEnumerator()
		{
			return GetPtrEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetPtrEnumerator();
		}
	}
}
