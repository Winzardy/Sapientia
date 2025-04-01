using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Collections.Ext;
using Sapientia.Extensions;

namespace Sapientia.Collections
{
	public class IndexAllocSparseSet<T> : IDisposable, IEnumerable<T>
	{
		public struct IndexData
		{
			public int indexToId;
			public int idToIndex;
			public int id;
		}

		public delegate void ResetAction(ref T item);

		public readonly int expandStep;

		private T[] _values;
		private IndexData[] _indexData;

		private readonly bool _useIndexPool;
		private readonly bool _useValuePool;
		private readonly ResetAction _resetValue;

		private int _count;
		private int _capacity;

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

		public bool IsFull
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _count >= _capacity;
		}

		public IndexAllocSparseSet(int capacity = 8) : this(capacity, null){}

		public IndexAllocSparseSet(int capacity, ResetAction resetValue) : this(capacity, capacity, true, true, resetValue) {}

		public IndexAllocSparseSet(int capacity, int expandStep, bool useIndexPool = true, bool useValuePool = true, ResetAction resetValue = null)
		{
			this.expandStep = expandStep;

			_count = 0;
			_capacity = capacity;

			_useIndexPool = useIndexPool;
			_useValuePool = useValuePool;
			_resetValue = resetValue;

			_indexData = _useIndexPool ? ArrayPool<IndexData>.Shared.Rent(capacity) : new IndexData[capacity];
			_values = _useValuePool ? ArrayPool<T>.Shared.Rent(capacity) : new T[capacity];

			FillIndexes(0, capacity);
		}

		public ref readonly T[] GetValueArray()
		{
			return ref _values;
		}

		public ref T GetValue(int id)
		{
			var valueIndex = _indexData[id].idToIndex;
			return ref _values[valueIndex];
		}

		public int IndexToIndexId(int index)
		{
			return _indexData[index].indexToId;
		}

		public int IndexIdToIndex(int id)
		{
			return _indexData[id].idToIndex;
		}

		public bool HasIndexId(int id)
		{
			return _indexData[id].idToIndex < _count;
		}

		public int AllocateIndexId()
		{
			ExpandIfNeeded(_count + 1);

			return AllocateIndexId_NoExpand();
		}

		public int AllocateIndexId_NoExpand()
		{
			var index = _count++;
			var id = _indexData[index].id;

			_indexData[id].idToIndex = index;
			_indexData[index].indexToId = id;
			return id;
		}

		public void ReleaseIndexId(int id, bool clear)
		{
			if (clear)
				GetValue(id) = default;
			ReleaseIndexId(id);
		}

		public void ReleaseIndexId(int id)
		{
			var index = _indexData[id].idToIndex;

			var lastIndex = --_count;
			var lastId = _indexData[lastIndex].indexToId;

			_indexData[index].indexToId = lastId;
			_indexData[index].id = lastId;

			_indexData[lastIndex].id = id;
			_indexData[lastId].idToIndex = index;

			_values[index] = _values[lastIndex];
			_values[lastIndex] = default;
		}

		public void ReleaseIndex(int index)
		{
			ReleaseIndexId(_indexData[index].indexToId);
		}

		private void ExpandIfNeeded(int newCapacity)
		{
			if (_capacity >= newCapacity)
				return;

			newCapacity = SnapCeilCapacity(newCapacity);

			Expand(newCapacity);
		}

		private void Expand(int newCapacity)
		{
			if (_useIndexPool)
				ArrayExt.Expand_WithPool(ref _indexData, newCapacity);
			else
				ArrayExt.Expand(ref _indexData, newCapacity);
			if (_useValuePool)
				ArrayExt.Expand_WithPool(ref _values, newCapacity);
			else
				ArrayExt.Expand(ref _values, newCapacity);

			FillIndexes(_capacity, newCapacity);

			_capacity = newCapacity;
		}

		private int SnapCeilCapacity(int newCapacity)
		{
			return ((newCapacity + expandStep - 1) / expandStep) * expandStep;
		}

		private void FillIndexes(int from, int to)
		{
			for (var i = from; i < to; i++)
			{
				_indexData[i].id = i;
			}
			if (_resetValue == null)
				return;
			for (var i = from; i < to; i++)
			{
				_resetValue.Invoke(ref _values[i]);
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public void Dispose(bool clearArray)
		{
			if (_useIndexPool)
				ArrayPool<IndexData>.Shared.Return(_indexData, clearArray);
			if (_useValuePool)
				ArrayPool<T>.Shared.Return(_values, clearArray);

			_indexData = null;
			_values = null;
		}

		public void ClearFast()
		{
			_count = 0;
		}

		public void Clear()
		{
			Array.Clear(_indexData, 0, _indexData.Length);
			Array.Clear(_values, 0, _values.Length);
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
			private readonly IndexAllocSparseSet<T> _sparseSet;
			private int _index;

			public T Current => _sparseSet._values[_index];

			object IEnumerator.Current => Current;

			internal Enumerator(IndexAllocSparseSet<T> sparseSet)
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
