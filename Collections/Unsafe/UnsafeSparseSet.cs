using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Data;

namespace Sapientia.Collections
{
	[StructLayout(LayoutKind.Sequential)]
	public struct UnsafeSparseSet<T> : IEnumerable<(int id, T value)> where T: unmanaged
	{
		public readonly int expandStep;

		private UnsafeArray<T> _values;
		private UnsafeArray<int> _dense;
		private UnsafeArray<int> _sparse;

		private int _count;

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _count;
		}

		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _values.Length;
		}

		public int SparseCapacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _sparse.Length;
		}

		public bool IsFull
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _count >= _values.Length;
		}

		public bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _values.IsCreated;
		}

		public UnsafeSparseSet(int capacity, int sparseCapacity, int expandStep = 0)
		{
			this.expandStep = expandStep == 0 ? capacity : expandStep;

			_count = 0;

			_values = new(capacity, true);
			_dense = new(capacity, true);
			_sparse = new(sparseCapacity, true);
		}

#if UNITY_5_3_OR_NEWER
		public UnsafeSparseSet(int capacity, int sparseCapacity, Unity.Collections.Allocator allocator, int expandStep = 0)
		{
			this.expandStep = expandStep == 0 ? capacity : expandStep;

			_count = 0;

			_values = new(capacity, allocator, true);
			_dense = new(capacity, allocator, true);
			_sparse = new(sparseCapacity, allocator, true);
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly SafePtr<T> GetValuePtr()
		{
			return _values.ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly Span<T> GetValuesSpan()
		{
			return _values.ptr.GetSpan(_count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtrByDenseId(int denseId)
		{
			return  _values.ptr + denseId;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetIdByDenseId(int denseId)
		{
			return _dense[denseId];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T Get(int id)
		{
			return ref _values[_sparse[id]];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetByDenseId(int denseId)
		{
			return ref  _values[denseId];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetDenseId(int id, out int denseId)
		{
			if (_sparse.Length <= id)
			{
				denseId = 0;
				return false;
			}

			denseId = _sparse[id];
			return denseId < _count && _dense[denseId] == id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(int id)
		{
			if (_sparse.Length <= id)
				return false;
			var denseId = _sparse[id];
			return denseId < _count && _dense[denseId] == id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T EnsureGet(int id)
		{
			ExpandSparseIfNeeded(id + 1);
			ref var denseId = ref _sparse[id];
			if (denseId >= _count || _dense[denseId] != id)
			{
				ExpandIfNeeded( _count + 1);
				_dense[_count] = id;
				denseId = _count++;
			}

			return ref _values[denseId];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveSwapBack(int id)
		{
			if (id >= _sparse.Length)
				return false;
			var denseId = _sparse[id];
			if (denseId >= _count)
				return false;

			var denseRaw = _dense.ptr;
			if (denseRaw[denseId] != id)
				return false;

			var sparseId = denseRaw[denseId] = denseRaw[--_count];
			_sparse[sparseId] = denseId;

			ref var valueA = ref (_values.ptr + denseId).Value();
			ref var valueB = ref (_values.ptr + _count).Value();

			valueA = valueB;
			valueB = default;

			E.ASSERT(_count >= 0);
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveSwapBackByDenseId(int denseId)
		{
			var denseRaw = _dense.ptr;

			var sparseId = denseRaw[denseId] = denseRaw[--_count];
			_sparse[sparseId] = denseId;

			ref var valueA = ref (_values.ptr + denseId).Value();
			ref var valueB = ref (_values.ptr + _count).Value();

			(valueA, valueB) = (valueB, valueA);
			valueB = default;

			E.ASSERT(_count >= 0);
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ExpandSparseIfNeeded( int newCapacity)
		{
			if (_sparse.Length >= newCapacity)
				return;

			newCapacity = SnapCeilCapacity(newCapacity);

			ExpandSparse(newCapacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ExpandSparse(int newCapacity)
		{
			_sparse.Resize(newCapacity, ResizeSettings.ClearMemory);
		}

		private void ExpandIfNeeded(int newCapacity)
		{
			if (Capacity >= newCapacity)
				return;

			newCapacity = SnapCeilCapacity(newCapacity);

			Expand(newCapacity);
		}

		private void Expand(int newCapacity)
		{
			_dense.Resize(newCapacity, ResizeSettings.ClearMemory);
			_values.Resize(newCapacity, ResizeSettings.ClearMemory);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int SnapCeilCapacity(int newCapacity)
		{
			return ((newCapacity + expandStep - 1) / expandStep) * expandStep;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			_sparse.Dispose();
			_dense.Dispose();
			_values.Dispose();
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

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<(int id, T value)> IEnumerable<(int id, T value)>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public struct Enumerator : IEnumerator<(int id, T value)>
		{
			private int _index;

			private UnsafeArray<T> _values;
			private UnsafeArray<int> _dense;
			private readonly int _count;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Enumerator(in UnsafeSparseSet<T> sparseSet)
			{
				_index = -1;

				_values = sparseSet._values;
				_dense = sparseSet._dense;
				_count = sparseSet._count;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext()
			{
				return ++_index < _count;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Reset()
			{
				_index = -1;
			}

			public (int id, T value) Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => (_dense[_index], _values[_index]);
			}

			object IEnumerator.Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Current;
			}

			public void Dispose()
			{
				this = default;
			}
		}
	}
}
