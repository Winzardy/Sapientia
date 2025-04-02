using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Extensions;

namespace Sapientia.Collections
{
	public class SparseSet<T> : IDisposable, IEnumerable<T>
	{
		public delegate void ResetAction(ref T item);

		public readonly int expandStep;

		private T[] _values;
		private int[] _dense;
		private int[] _sparse;

		private readonly bool _useIndexPool;
		private readonly bool _useValuePool;
		private readonly ResetAction _resetValue;

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

		public SparseSet(int capacity, int sparseCapacity, ResetAction resetValue = null) : this(capacity, capacity, sparseCapacity, true, true, resetValue) {}

		public SparseSet(int capacity, int expandStep, int sparseCapacity, bool useIndexPool = true, bool useValuePool = true, ResetAction resetValue = null)
		{
			this.expandStep = expandStep;

			_count = 0;

			_useIndexPool = useIndexPool;
			_useValuePool = useValuePool;
			_resetValue = resetValue;

			_values = _useValuePool ? ArrayPool<T>.Shared.Rent(capacity) : new T[capacity];
			_dense = _useIndexPool ? ArrayPool<int>.Shared.Rent(capacity) : new int[capacity];
			_sparse = _useIndexPool ? ArrayPool<int>.Shared.Rent(sparseCapacity) : new int[sparseCapacity];

			_capacity = Math.Min(_dense.Length, _values.Length);
			_sparseCapacity = _sparse.Length;

			Fill(0, capacity);
		}

		public ref readonly T[] GetValueArray()
		{
			return ref _values;
		}

		public ref T Get(int id)
		{
			return ref _values[_sparse[id]];
		}

		public bool Has(int id)
		{
			if (_sparseCapacity <= id)
				return false;
			var denseId = _sparse[id];
			return denseId < _count && _dense[denseId] == id;
		}

		public ref T EnsureGet(int id)
		{
			ExpandSparseIfNeeded(id + 1);
			ref var denseId = ref _sparse[id];
			if (denseId >= _count || _dense[denseId] != id)
			{
				ExpandDenseIfNeeded(_count + 1);
				_dense[_count] = id;
				denseId = _count++;
			}
			return ref _values[denseId];
		}

		public bool TryRemoveSwapBack(int id, out T value)
		{
			value = default;
			if (id >= _sparseCapacity)
				return false;
			var denseId = _sparse[id];
			if (denseId >= _count || _dense[denseId] != id)
				return false;

			var sparseId = _dense[denseId] = _dense[--_count];
			_sparse[sparseId] = denseId;

			value = _values[denseId];

			_values[denseId] = _values[_count];
			_values[_count] = default;

			return true;
		}

		public void RemoveSwapBack(int id)
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

		private void ExpandSparseIfNeeded(int newCapacity)
		{
			if (_sparseCapacity >= newCapacity)
				return;

			newCapacity = SnapCeilCapacity(newCapacity);

			ExpandSparse(newCapacity);
		}

		private void ExpandSparse(int newCapacity)
		{
			if (_useIndexPool)
				ArrayExt.Expand_WithPool(ref _sparse, newCapacity);
			else
				ArrayExt.Expand(ref _sparse, newCapacity);

			FillSparse(_sparseCapacity, newCapacity);

			_sparseCapacity = _sparse.Length;
		}

		private void ExpandDenseIfNeeded(int newCapacity)
		{
			if (_capacity >= newCapacity)
				return;

			newCapacity = SnapCeilCapacity(newCapacity);

			ExpandDense(newCapacity);
		}

		private void ExpandDense(int newCapacity)
		{
			if (_useIndexPool)
				ArrayExt.Expand_WithPool(ref _dense, newCapacity);
			else
				ArrayExt.Expand(ref _dense, newCapacity);
			if (_useValuePool)
				ArrayExt.Expand_WithPool(ref _values, newCapacity);
			else
				ArrayExt.Expand(ref _values, newCapacity);

			Fill(_capacity, newCapacity);

			_capacity = Math.Min(_dense.Length, _values.Length);
		}

		private int SnapCeilCapacity(int newCapacity)
		{
			return ((newCapacity + expandStep - 1) / expandStep) * expandStep;
		}

		private void FillSparse(int from, int to)
		{
			Array.Fill(_sparse, 0, from, to - from);
		}

		private void Fill(int from, int to)
		{
			Array.Fill(_dense, 0, from, to - from);

			if (_resetValue == null)
			{
				Array.Fill(_values, default, from, to - from);
				return;
			}
			for (var i = from; i < to; i++)
			{
				_resetValue.Invoke(ref _values[i]);
			}
		}

		public void Dispose()
		{
			Dispose(false);
		}

		public void Dispose(bool clearArray)
		{
			if (_useIndexPool)
			{
				ArrayPool<int>.Shared.Return(_sparse, clearArray);
				ArrayPool<int>.Shared.Return(_dense, clearArray);
			}
			if (_useValuePool)
				ArrayPool<T>.Shared.Return(_values, clearArray);

			_sparse = null;
			_dense = null;
			_values = null;
		}

		public void ClearFast()
		{
			_count = 0;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public struct Enumerator : IEnumerator<T>
		{
			private readonly SparseSet<T> _sparseSet;
			private int _index;

			public T Current => _sparseSet._values[_index];

			object IEnumerator.Current => Current;

			internal Enumerator(SparseSet<T> sparseSet)
			{
				_sparseSet = sparseSet;
				_index = -1;
			}

			public bool MoveNext()
			{
				_index++;
				return _index < _sparseSet._count;
			}

			public void Reset()
			{
				_index = -1;
			}

			public void Dispose() {}
		}
	}
}
