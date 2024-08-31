using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Generic.Extensions;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct SparseSet<T> : IEnumerable<T> where T : unmanaged
	{
		public readonly struct IntPtrEnumerable : IEnumerable<IntPtr>
		{
			private readonly IntPtrEnumerator _enumerator;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal IntPtrEnumerable(in Allocator allocator, ref SparseSet<T> sparseSet)
			{
				_enumerator = new IntPtrEnumerator(allocator, ref sparseSet);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public IEnumerator<IntPtr> GetEnumerator()
			{
				return _enumerator;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public struct IntPtrEnumerator : IEnumerator<IntPtr>
		{
			private T* _valuePtr;
			private uint _index;
			private uint _count;

			public IntPtr Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => (IntPtr)(&_valuePtr[_index - 1]);
			}

			object IEnumerator.Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Current;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal IntPtrEnumerator(in Allocator allocator, ref SparseSet<T> sparseSet)
			{
				_valuePtr = sparseSet.GetValuePtr(allocator);
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

		public struct Enumerator : IEnumerator<T>
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
			internal Enumerator(in Allocator allocator, ref SparseSet<T> sparseSet)
			{
				_valuePtr = sparseSet.GetValuePtr(allocator);
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

		private MemArray<T> _values;
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

		public SparseSet(uint capacity, uint sparseCapacity, uint expandStep = 0) : this(ref AllocatorManager.CurrentAllocator, capacity, sparseCapacity, expandStep) {}

		public SparseSet(AllocatorId allocatorId, uint capacity, uint sparseCapacity, uint expandStep = 0) : this(ref allocatorId.GetAllocator(), capacity, sparseCapacity, expandStep) {}

		public SparseSet(ref Allocator allocator, uint capacity, uint sparseCapacity, uint expandStep = 0)
		{
			this.expandStep = expandStep == 0 ? capacity : expandStep;

			_count = 0;

			_values = new (ref allocator, capacity, ClearOptions.ClearMemory);
			_dense = new (ref allocator, capacity, ClearOptions.ClearMemory);
			_sparse = new (ref allocator, sparseCapacity, ClearOptions.ClearMemory);

			_capacity = _dense.Length.Min(_values.Length);
			_sparseCapacity = _sparse.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetValuePtr(in Allocator allocator)
		{
			return _values.GetValuePtr(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetValuePtr()
		{
			return _values.GetValuePtr();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get(uint id)
		{
			return ref _values[_sparse[id]];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(uint id)
		{
			var denseId = _sparse[id];
			return denseId < _count && _dense[denseId] == id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T EnsureGet(uint id)
		{
			ExpandSparseIfNeeded(id);
			ref var denseId = ref _sparse[id];
			if (denseId >= _count || _dense[denseId] != id)
			{
				ExpandIfNeeded(_count + 1);
				_dense[_count] = id;
				denseId = _count++;
			}
			return ref _values[denseId];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveSwapBack(uint id)
		{
			if (id >= _sparseCapacity)
				return;
			var denseId = _sparse[id];
			if (denseId >= _count || _dense[denseId] != id)
				return;

			var sparseId = _dense[denseId] = _dense[--_count];
			_sparse[sparseId] = denseId;

			_values[denseId] = _values[_count];
			_values[_count] = default;
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
		public void ClearFast()
		{
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerator GetEnumerator(in Allocator allocator)
		{
			return new Enumerator(allocator, ref this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerator GetEnumerator()
		{
			return new Enumerator(GetAllocator(), ref this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
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
