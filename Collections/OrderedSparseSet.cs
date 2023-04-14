using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Sapientia.Extensions;

namespace Sapientia.Collections
{
	public class OrderedSparseSet<T> : IDisposable
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
		private bool _useValuePool;
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

		public OrderedSparseSet(int capacity, ResetAction resetValue = null) : this(capacity, capacity, true, true, resetValue) {}

		public OrderedSparseSet(int capacity, int expandStep, bool useIndexPool = true, bool useValuePool = true, ResetAction resetValue = null)
		{
			this.expandStep = expandStep;

			_count = 0;
			_capacity = capacity;

			_useIndexPool = useIndexPool;
			_useValuePool = useValuePool;
			if (_useValuePool)
				_resetValue = resetValue;

			_indexData = _useIndexPool ? ArrayPool<IndexData>.Shared.Rent(capacity) : new IndexData[capacity];
			_values = _useValuePool ? ArrayPool<T>.Shared.Rent(capacity) : new T[capacity];

			FillIndexes(0, capacity);
		}

		public ref readonly T[] GetValueArray()
		{
			return ref _values;
		}

		public int AllocateIndexId_NoExpand()
		{
			var index = _count++;
			var id = _indexData[index].id;

			_indexData[id].idToIndex = index;
			_indexData[index].indexToId = id;
			return id;
		}

		public ref T GetValue(int id)
		{
			var valueIndex = _indexData[id].idToIndex;
			return ref _values[valueIndex];
		}

		public int AllocateIndexId()
		{
			ExpandIfNeeded(_count + 1);

			return AllocateIndexId_NoExpand();
		}

		public void ReleaseIndexId(int id)
		{
			var index = _indexData[id].idToIndex;

			var indexB = --_count;
			var idB = _indexData[indexB].indexToId;

			 _values[index] = _values[indexB];
			_indexData[index].indexToId = idB;
			_indexData[idB].idToIndex = index;
			_indexData[indexB].id = id;
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
				ArrayExtensions.Expand_WithPool(ref _indexData, newCapacity);
			else
				ArrayExtensions.Expand(ref _indexData, newCapacity);
			if (_useValuePool)
				ArrayExtensions.Expand_WithPool(ref _values, newCapacity);
			else
				ArrayExtensions.Expand(ref _values, newCapacity);

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
			Dispose(false);
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
	}
}