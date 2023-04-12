using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Sapientia.Extensions;

namespace Sapientia.Collections
{
	public class OrderedSparseSet<T> : IDisposable
	{
		public struct Value
		{
			public int indexId;
			public T value;
		}

		public delegate void ResetAction(ref T item);

		public readonly int expandStep;

		private Value[] _values;
		private int[] _valueIndexes;
		private int[] _indexIds;

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

			_indexIds = _useIndexPool ? ArrayPool<int>.Shared.Rent(capacity) : new int[capacity];
			_valueIndexes = _useIndexPool ? ArrayPool<int>.Shared.Rent(capacity) : new int[capacity];
			_values = _useValuePool ? ArrayPool<Value>.Shared.Rent(capacity) : new Value[capacity];

			FillIndexes(0, capacity);
		}

		public Value[] GetValueArray()
		{
			return _values;
		}

		public int AllocateIndexId_NoExpand()
		{
			var indexId = _indexIds[_count];
			_valueIndexes[indexId] = _count;
			_values[_count++].indexId = indexId;
			return indexId;
		}

		public ref T GetValue(int indexId)
		{
			var valueIndex = _valueIndexes[indexId];
			return ref _values[valueIndex].value;
		}

		public int AllocateIndexId()
		{
			ExpandIfNeeded(_count + 1);

			return AllocateIndexId_NoExpand();
		}

		public void ReleaseIndexId(int indexId)
		{
			var valueIndex = _valueIndexes[indexId];

			_values[valueIndex] = _values[_count - 1];
			_valueIndexes[_values[valueIndex].indexId] = valueIndex;

			_indexIds[--_count] = indexId;
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
			{
				ArrayExtensions.Expand_WithPool(ref _valueIndexes, newCapacity);
				ArrayExtensions.Expand_WithPool(ref _indexIds, newCapacity);
			}
			else
			{
				ArrayExtensions.Expand(ref _valueIndexes, newCapacity);
				ArrayExtensions.Expand(ref _indexIds, newCapacity);
			}
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
				_indexIds[i] = i;
			}
			if (_resetValue == null)
				return;
			for (var i = from; i < to; i++)
			{
				_resetValue.Invoke(ref _values[i].value);
			}
		}

		public void Dispose()
		{
			Dispose(false);
		}

		public void Dispose(bool clearArray)
		{
			if (_useIndexPool)
				ArrayPool<int>.Shared.Return(_valueIndexes, clearArray);
			if (_useValuePool)
				ArrayPool<Value>.Shared.Return(_values, clearArray);

			_valueIndexes = null;
			_values = null;
		}
	}
}