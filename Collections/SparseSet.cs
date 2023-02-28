using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Sapientia.Extensions;

namespace Sapientia.Collections
{
	public class SparseSet<T> : IDisposable
	{
		public delegate void ResetAction(ref T item);

		public readonly int expandStep;

		private T[] _values;
		private int[] _valueIndexes;

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

		public SparseSet(int capacity, ResetAction resetValue = null) : this(capacity, capacity, true, true, resetValue) {}

		public SparseSet(int capacity, int expandStep, bool useIndexPool = true, bool useValuePool = true, ResetAction resetValue = null)
		{
			this.expandStep = expandStep;

			_count = 0;
			_capacity = capacity;

			_useIndexPool = useIndexPool;
			_useValuePool = useValuePool;
			if (_useValuePool)
				_resetValue = resetValue;

			_valueIndexes = _useIndexPool ? ArrayPool<int>.Shared.Rent(capacity) : new int[capacity];
			_values = _useValuePool ? ArrayPool<T>.Shared.Rent(capacity) : new T[capacity];

			Fill(0, capacity);
		}

		public T[] GetValueArray()
		{
			return _values;
		}

		public int AllocateValueIndex()
		{
			return _valueIndexes[_count++];
		}

		public ref T GetValue(int valueIndex)
		{
			return ref _values[valueIndex];
		}

		public int AllocateValueIndexWithExpand()
		{
			ExpandIfNeeded(_count + 1);

			return AllocateValueIndex();
		}

		public void ReleaseValueIndex(int valueIndex)
		{
			_valueIndexes[--_count] = valueIndex;
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
				ArrayExtensions.Expand_WithPool(ref _valueIndexes, newCapacity);
			else
				ArrayExtensions.Expand(ref _valueIndexes, newCapacity);
			if (_useValuePool)
				ArrayExtensions.Expand_WithPool(ref _values, newCapacity);
			else
				ArrayExtensions.Expand(ref _values, newCapacity);

			Fill(_capacity, newCapacity);

			_capacity = newCapacity;
		}

		private int SnapCeilCapacity(int newCapacity)
		{
			return ((newCapacity + expandStep - 1) / expandStep) * expandStep;
		}

		private void Fill(int from, int to)
		{
			for (var i = from; i < to; i++)
			{
				_valueIndexes[i] = i;
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
				ArrayPool<int>.Shared.Return(_valueIndexes, clearArray);
			if (_useValuePool)
				ArrayPool<T>.Shared.Return(_values, clearArray);

			_valueIndexes = null;
			_values = null;
		}
	}
}