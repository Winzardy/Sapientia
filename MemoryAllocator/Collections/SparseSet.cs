using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Generic.Extensions;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator.Collections
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct SparseSet : IEnumerable<IntPtr>
	{
		public struct IntPtrEnumerator : IEnumerator<IntPtr>
		{
			private byte* _valuePtr;
			private uint _elementSize;
			private uint _index;
			private uint _count;

			public IntPtr Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => (IntPtr)(&_valuePtr[(_index - 1) * _elementSize]);
			}

			object IEnumerator.Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Current;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public IntPtrEnumerator(in Allocator allocator, ref SparseSet sparseSet)
			{
				_valuePtr = (byte*)sparseSet._values.GetPtr(allocator);
				_elementSize = sparseSet._values.ElementSize;
				_index = 0;
				_count = sparseSet._count;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext()
			{
				_index++;
				return _index <= _count;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Reset()
			{
				_index = 0;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose() {}
		}

		public readonly struct Enumerable<T> : IEnumerable<T> where T: unmanaged
		{
			private readonly Enumerator<T> _enumerator;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal Enumerable(in Allocator allocator, ref SparseSet sparseSet)
			{
				_enumerator = new Enumerator<T>(allocator, ref sparseSet);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public IEnumerator<T> GetEnumerator()
			{
				return _enumerator;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public struct Enumerator<T> : IEnumerator<T> where T: unmanaged
		{
			private T* _valuePtr;
			private uint _index;
			private uint _count;

			public T Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => _valuePtr[_index - 1];
			}

			object IEnumerator.Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Current;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Enumerator(in Allocator allocator, ref SparseSet sparseSet)
			{
				_valuePtr = sparseSet.GetValuePtr<T>(allocator);
				_index = 0;
				_count = sparseSet._count;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext()
			{
				_index++;
				return _index <= _count;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Reset()
			{
				_index = 0;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose() {}
		}

		public readonly uint expandStep;

		private MemArray _values;
		private MemArray<uint> _dense;
		private MemArray<uint> _sparse;

		private uint _count;
		private uint _capacity;
		private uint _sparseCapacity;

		public uint Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _count;
		}

		public uint Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _capacity;
		}

		public uint FreeIndexesCapacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _sparseCapacity;
		}

		public bool IsFull
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _count >= _capacity;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref Allocator GetAllocator()
		{
			return ref _values.GetAllocator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Allocator* GetAllocatorPtr()
		{
			return _values.GetAllocatorPtr();
		}

		public uint ElementSize
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _values.ElementSize;
		}

		public SparseSet(uint valueSize, uint capacity, uint sparseCapacity, uint expandStep = 0) : this(ref AllocatorManager.CurrentAllocator, valueSize, capacity, sparseCapacity, expandStep) {}

		public SparseSet(AllocatorId allocatorId, uint valueSize, uint capacity, uint sparseCapacity, uint expandStep = 0) : this(ref allocatorId.GetAllocator(), valueSize, capacity, sparseCapacity, expandStep) {}

		public SparseSet(ref Allocator allocator, uint valueSize, uint capacity, uint sparseCapacity, uint expandStep = 0)
		{
			this.expandStep = expandStep == 0 ? capacity : expandStep;

			_count = 0;

			_values = new (ref allocator, valueSize, capacity, ClearOptions.ClearMemory);
			_dense = new (ref allocator, capacity, ClearOptions.ClearMemory);
			_sparse = new (ref allocator, sparseCapacity, ClearOptions.ClearMemory);

			_capacity = _dense.Length.Min(_values.Length);
			_sparseCapacity = _sparse.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetValuePtr<T>(Allocator* allocator) where T: unmanaged
		{
			return _values.GetValuePtr<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetValuePtr<T>(in Allocator allocator) where T: unmanaged
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
			return _values.GetValuePtr();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void* GetValuePtr(Allocator* allocator)
		{
			return _values.GetValuePtr(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void* GetValuePtr(uint id)
		{
			return _values.GetValuePtr(_sparse[id]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void* GetValuePtr(Allocator* allocator, uint id)
		{
			return _values.GetValuePtr(allocator, _sparse[allocator, id]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetValuePtr<T>(Allocator* allocator, uint id) where T: unmanaged
		{
			return _values.GetValuePtr<T>(allocator, _sparse[allocator, id]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get<T>(Allocator* allocator, uint id) where T: unmanaged
		{
			return ref _values.GetValue<T>(allocator, _sparse[allocator, id]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get<T>(uint id) where T: unmanaged
		{
			return ref _values.GetValue<T>(_sparse[id]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(Allocator* allocator, uint id)
		{
			var denseId = _sparse[allocator, id];
			return denseId < _count && _dense[allocator, denseId] == id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(uint id)
		{
			var denseId = _sparse[id];
			return denseId < _count && _dense[denseId] == id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T EnsureGet<T>(Allocator* allocator, uint id) where T: unmanaged
		{
			ExpandSparseIfNeeded(id);
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
		public ref T EnsureGet<T>(uint id) where T: unmanaged
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
		public void RemoveSwapBack(uint id)
		{
			if (id >= _sparseCapacity)
				return;
			ref var allocator = ref GetAllocator();
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
		public void RemoveSwapBack(Allocator* allocator, uint id)
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
		private void ExpandSparseIfNeeded(uint newCapacity)
		{
			if (_sparseCapacity >= newCapacity)
				return;

			newCapacity = SnapCeilCapacity(newCapacity);

			ExpandSparse(ref GetAllocator(), newCapacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ExpandSparse(ref Allocator allocator, uint newCapacity)
		{
			_sparse.Resize(ref allocator, newCapacity, ClearOptions.ClearMemory);
			_sparseCapacity = _sparse.Length;
		}

		private void ExpandIfNeeded(uint newCapacity)
		{
			if (_capacity >= newCapacity)
				return;

			newCapacity = SnapCeilCapacity(newCapacity);

			Expand(ref GetAllocator(), newCapacity);
		}

		private void Expand(ref Allocator allocator, uint newCapacity)
		{
			_dense.Resize(ref allocator, newCapacity, ClearOptions.ClearMemory);
			_values.Resize(ref allocator, newCapacity, ClearOptions.ClearMemory);

			_capacity = _dense.Length.Min(_values.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint SnapCeilCapacity(uint newCapacity)
		{
			return ((newCapacity + expandStep - 1u) / expandStep) * expandStep;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			Dispose(ref GetAllocator());
		}

		public void Dispose(ref Allocator allocator)
		{
			_sparse.Dispose(ref allocator);
			_dense.Dispose(ref allocator);
			_values.Dispose(ref allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(Allocator* allocator)
		{
			_values.Clear(allocator, 0u, _count);
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(in Allocator allocator)
		{
			_values.Clear(allocator, 0u, _count);
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			_values.Clear(0u, _count);
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast()
		{
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T> GetEnumerable<T>(in Allocator allocator) where T: unmanaged
		{
			return new Enumerable<T>(allocator, ref this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T> GetEnumerable<T>() where T: unmanaged
		{
			return new Enumerable<T>(GetAllocator(), ref this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerator<T> GetEnumerator<T>(in Allocator allocator) where T: unmanaged
		{
			return new Enumerator<T>(allocator, ref this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerator<T> GetEnumerator<T>() where T: unmanaged
		{
			return new Enumerator<T>(GetAllocator(), ref this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IntPtrEnumerator GetEnumerator(in Allocator allocator)
		{
			return new IntPtrEnumerator(allocator, ref this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IntPtrEnumerator GetEnumerator()
		{
			return new IntPtrEnumerator(GetAllocator(), ref this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator<IntPtr> IEnumerable<IntPtr>.GetEnumerator()
		{
			return GetEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
