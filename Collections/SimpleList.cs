using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Sapientia.Extensions;

namespace Sapientia.Collections
{
	public class SimpleList<T> : IDisposable
	{
		private T[] _array;
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

		public ref T Last
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _array[Count - 1];
		}

		public bool IsFull
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _count >= _capacity;
		}

		public ref T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _array[index];
		}

		public SimpleList(int capacity)
		{
			_count = 0;
			_capacity = capacity;
			_array = ArrayPool<T>.Shared.Rent(capacity);
		}

		public SimpleList(int capacity, T defaultValue)
		{
			_count = 0;
			_capacity = capacity;
			_array = ArrayPool<T>.Shared.Rent(capacity);
			Array.Fill(_array, defaultValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add<T1>(T1 value) where T1: T
		{
			_array[_count++] = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddWithExpand<T1>(T1 value) where T1: T
		{
			Expand(_count + 1);
			_array[_count++] = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Allocate()
		{
			return _count++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveAtSwapBack(int index)
		{
			_count--;
			_array[index] = _array[_count];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveAt(int index)
		{
			_count--;
			if (_count > index)
			{
				Array.Copy(_array, index + 1, _array, index, _count - index);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Expand(int newCapacity)
		{
			if (newCapacity < _capacity)
				return;
			if (newCapacity < _array.Length)
			{
				_capacity = newCapacity;
				return;
			}

			ArrayExtensions.Expand_WithPool(ref _array, newCapacity);
			_capacity = newCapacity;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Expand(int newCapacity, T defaultValue)
		{
			if (newCapacity < _capacity)
				return;
			if (newCapacity < _array.Length)
			{
				_capacity = newCapacity;
				return;
			}

			var previousLenght = _array.Length;
			ArrayExtensions.Expand_WithPool(ref _array, newCapacity);
			Array.Fill(_array, defaultValue, previousLenght, _array.Length - previousLenght);
			_capacity = newCapacity;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void FullClear()
		{
			Array.Fill(_array, default);
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			Array.Fill(_array, default, 0, _count);
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast()
		{
			_count = 0;
		}

		public void Dispose()
		{
			ArrayPool<T>.Shared.Return(_array);
		}
	}
}