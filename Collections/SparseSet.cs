using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Sapientia.Extensions;

namespace Sapientia.Collections
{
	public class SparseSet<T> : IDisposable
	{
		public readonly int expandStep;

		private T[] _values;
		private int[] _valueIndexes;

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

		public SparseSet(int capacity) : this(capacity, capacity) {}

		public SparseSet(int capacity, int expandStep)
		{
			this.expandStep = expandStep;

			_count = 0;
			_capacity = capacity;

			_values = ArrayPool<T>.Shared.Rent(capacity);
			_valueIndexes = ArrayPool<int>.Shared.Rent(capacity);

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
			ArrayExtensions.Expand_WithPool(ref _values, newCapacity);
			ArrayExtensions.Expand_WithPool(ref _valueIndexes, newCapacity);

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
		}

		public void Dispose()
		{
			ArrayPool<T>.Shared.Return(_values);
		}
	}
}