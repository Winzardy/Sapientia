using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Extensions;

namespace Sapientia.Collections
{
	public class SimpleList<T> : IDisposable, IEnumerable<T>
	{
		private const int DEFAULT_CAPACITY = 8;

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

		public bool IsEmpty
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _count == 0;
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

		public SimpleList(int capacity = DEFAULT_CAPACITY)
		{
			_count = 0;
			_capacity = capacity;
			_array = ArrayPool<T>.Shared.Rent(_capacity);
		}

		public SimpleList(T[] array) : this(array.Length)
		{
			array.CopyTo(_array, 0);
			_count = array.Length;
		}

		public SimpleList(int capacity, T defaultValue)
		{
			_count = 0;
			_capacity = capacity;
			_array = ArrayPool<T>.Shared.Rent(capacity);
			Array.Fill(_array, defaultValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddWithoutExpand<T1>(T1 value) where T1: T
		{
			_array[_count++] = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add<T1>(T1 value) where T1: T
		{
			Expand(_count + 1);
			_array[_count++] = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddRange<T1>(SimpleList<T1> values) where T1: T
		{
			Expand(_count + values.Count);
			for (var i = 0; i < values.Count; i++)
			{
				_array[_count++] = values[i];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Allocate()
		{
			return _count++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Swap(int indexA, int indexB)
		{
			(_array[indexA], _array[indexB]) = (_array[indexB], _array[indexA]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Move(int from, int to)
		{
			_array.Move(from, to);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T ExtractAtSwapBack(int index)
		{
			_count--;
			var value = _array[index];
			_array[index] = _array[_count];
			return value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveAtSwapBack(int index)
		{
			_count--;
			_array[index] = _array[_count];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveAtSwapBack_Clean(int index)
		{
			_count--;
			_array[index] = _array[_count];
			_array[_count] = default;
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

		public void InsertAt(int index, in T value)
		{
			Expand(_count + 1);

			Array.Copy(_array, index, _array, index + 1, _count - index);
			_array[index] = value;

			_count++;
		}

		public void Fill(int lenght, T value)
		{
			var targetCount = _count + lenght;
			Expand(targetCount);
			for (var i = _count; i < targetCount; i++)
			{
				_array[i] = value;
			}
			_count = targetCount;
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
		public void Clear()
		{
			Array.Fill(_array, default);
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearPartial()
		{
			if (_count == 0)
				return;
			Array.Fill(_array, default, 0, _count);
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast()
		{
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			if (_array == null)
				return;

			ArrayPool<T>.Shared.Return(_array);
			_array = null;
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

		~SimpleList()
		{
			Dispose();
		}

		public T[] ToArray()
		{
			var result = new T[_count];
			Array.Copy(_array, 0, result, 0, _count);
			return result;
		}

		public T[] GetInnerArray()
		{
			return _array;
		}

		public struct Enumerator : IEnumerator<T>
		{
			private SimpleList<T> _list;
			private int _index;

			public T Current => _list[_index];

			object IEnumerator.Current => Current;

			internal Enumerator(SimpleList<T> list)
			{
				_list = list;
				_index = -1;
			}

			public bool MoveNext()
			{
				_index++;
				return _index < _list._count;
			}

			public void Reset()
			{
				_index = -1;
			}

			public void Dispose() {}
		}
	}

	public static class SimpleListExt
	{
		public struct DefaultComparer<T> : IComparer<T> where T : IComparable<T>
		{
			/// <summary>
			/// Compares two values.
			/// </summary>
			/// <param name="x">First value to compare.</param>
			/// <param name="y">Second value to compare.</param>
			/// <returns>A signed integer that denotes the relative values of `x` and `y`:
			/// 0 if they're equal, negative if `x &lt; y`, and positive if `x &gt; y`.</returns>
			public int Compare(T x, T y) => x.CompareTo(y);
		}

		public static void Sort<T>(this SimpleList<T> list) where T: IComparable<T>
		{
			list.Sort(0, list.Count, new DefaultComparer<T>());
		}

		public static void Sort<T>(this SimpleList<T> list, int index, int count) where T: IComparable<T>
		{
			list.Sort(index, count, new DefaultComparer<T>());
		}

		public static void Sort<T, TComparer>(this SimpleList<T> list, TComparer comparer) where TComparer : IComparer<T>
		{
			list.Sort(0, list.Count, comparer);
		}

		public static void Sort<T, TComparer>(this SimpleList<T> list, int index, int count, TComparer comparer) where TComparer : IComparer<T>
		{
			var array = list.GetInnerArray();
			Array.Sort(array, index, count, comparer);
		}

		public static int BinarySearch<T>(this SimpleList<T> list, T value) where T: IComparable<T>
		{
			return BinarySearch(list, list.Count, value, new DefaultComparer<T>());
		}

		public static int BinarySearch<T, TComparer>(this SimpleList<T> list, T value, TComparer comparer)
			where TComparer : IComparer<T>
		{
			return BinarySearch(list, list.Count, value, comparer);
		}

		public static int BinarySearch<T, TComparer>(this SimpleList<T> list, int length, T value, TComparer comparer) where TComparer: IComparer<T>
		{
			var offset = 0;

			for (var l = length; l != 0; l >>= 1)
			{
				var idx = offset + (l >> 1);
				var curr = list[idx];
				var r = comparer.Compare(value, curr);
				if (r == 0)
				{
					return idx;
				}

				if (r > 0)
				{
					offset = idx + 1;
					--l;
				}
			}

			return ~offset;
		}
	}
}