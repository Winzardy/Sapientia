using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Extensions;

namespace Sapientia.Collections
{
	public class SimpleList<T> : IDisposable, IList<T>
	{
		public static SimpleList<T> CreateEmpty() => new SimpleList<T>();

		public const int DEFAULT_CAPACITY = 8;

		private T[] _array;
		private int _count;
		private bool _isRented;

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _count;
		}

		bool ICollection<T>.IsReadOnly => false;

		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _array.Length;
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
			get => _count >= _array.Length;
		}

		public ref T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _array[index];
		}

		T IList<T>.this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _array[index];
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _array[index] = value;
		}

		private SimpleList()
		{
			_count = 0;
			_array = Array.Empty<T>();
			_isRented = false;
		}

		public SimpleList(bool isRented) : this(DEFAULT_CAPACITY, isRented)
		{
		}

		public SimpleList(int capacity = DEFAULT_CAPACITY, bool isRented = true)
		{
			_count = 0;
			_array = isRented ? ArrayPool<T>.Shared.Rent(capacity) : new T[capacity];
			_isRented = isRented;
		}

		public SimpleList(T[] array, int capacity = DEFAULT_CAPACITY, bool isRented = true)
		{
			_count = array.Length;
			if (capacity < _count)
				capacity = _count;
			_array = isRented ? ArrayPool<T>.Shared.Rent(capacity) : new T[capacity];
			_isRented = isRented;

			array.CopyTo(_array, 0);
		}

		public SimpleList(IEnumerable<T> enumerable, int capacity = DEFAULT_CAPACITY, bool isRented = true) : this(capacity, isRented)
		{
			AddRange(enumerable);
		}

		public SimpleList(ICollection<T> collection, int capacity = DEFAULT_CAPACITY, bool isRented = true) : this(capacity, isRented)
		{
			_count = collection.Count;
			if (capacity < _count)
				capacity = _count;
			_array = isRented ? ArrayPool<T>.Shared.Rent(capacity) : new T[capacity];
			_isRented = isRented;

			collection.CopyTo(_array, 0);
		}

		public SimpleList(int capacity, T defaultValue, bool isRented = true) : this(capacity, isRented)
		{
			Array.Fill(_array, defaultValue);
		}

		public static SimpleList<T> WrapArray(T[] array)
		{
			return new SimpleList<T>
			{
				_count = array.Length,
				_array = array,
				_isRented = false,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddWithoutExpand<T1>(T1 value) where T1 : T
		{
			_array[_count++] = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T value)
		{
			Expand(_count + 1);
			_array[_count++] = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add<T1>(T1 value) where T1 : T
		{
			Expand(_count + 1);
			_array[_count++] = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddRange<T1>(IEnumerable<T1> values) where T1 : T
		{
			foreach (var value in values)
				Add(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddRange<T1>(SimpleList<T1> values) where T1 : T
		{
			Expand(_count + values._count);
			Array.Copy(values._array, 0, _array, _count, values._count);
			_count += values._count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddRange<T1>(T1[] values) where T1 : T
		{
			AddRange(values, 0, values.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddRange<T1>(T1[] values, int count) where T1 : T
		{
			AddRange(values, 0, count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddRange<T1>(T1[] values, int sourceIndex, int count) where T1 : T
		{
			Expand(_count + count);
			Array.Copy(values, sourceIndex, _array, _count, count);
			_count += count;
		}

		public void AddRange(T value, int count)
		{
			var targetCount = _count + count;
			Expand(targetCount);

			Array.Fill(_array, value, _count, count);
			_count = targetCount;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Allocate()
		{
			return _count++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetCount(int count)
		{
			_count = count;
			Expand(count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ExpandCount(int count)
		{
			if (_count < count)
			{
				Expand(count);
				_count = count;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ExpandCount(int count, T defaultValue)
		{
			if (_count < count)
			{
				AddRange(defaultValue, count - _count);
			}
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
		public T ExtractLast()
		{
			_count--;
			return _array[_count];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveLast()
		{
			_count--;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T RemoveAtSwapBack(int index)
		{
			_count--;
			return _array[index] = _array[_count];
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(T value)
		{
			for (var i = 0; i < _count; i++)
			{
				if (value!.Equals(_array[i]))
				{
					RemoveAt(i);
					return true;
				}
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveSwapBack(T value)
		{
			for (var i = 0; i < _count; i++)
			{
				if (value.Equals(_array[i]))
				{
					RemoveAtSwapBack(i);
					return true;
				}
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveSwapBack_Clean(T value)
		{
			for (var i = 0; i < _count; i++)
			{
				if (value.Equals(_array[i]))
				{
					RemoveAtSwapBack_Clean(i);
					return true;
				}
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove<T1>(T1 value) where T1 : T, IEquatable<T>
		{
			for (var i = 0; i < _count; i++)
			{
				if (value.Equals(_array[i]))
				{
					RemoveAt(i);
					return true;
				}
			}

			GC.SuppressFinalize(value);
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(T value)
		{
			for (var i = 0; i < _count; i++)
			{
				if (value.Equals(_array[i]))
					return true;
			}

			return false;
		}

		public int IndexOf(T value)
		{
			for (var i = 0; i < _count; i++)
			{
				if (value.Equals(_array[i]))
					return i;
			}

			return -1;
		}

		public void Insert(int index, T value)
		{
			Expand(_count + 1);

			Array.Copy(_array, index, _array, index + 1, _count - index);
			_array[index] = value;

			_count++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Expand(int newCapacity)
		{
			if (newCapacity < _array.Length)
				return;

			if (_isRented)
				ArrayExt.Expand_WithPool(ref _array, newCapacity);
			else
			{
				ArrayExt.Expand_WithPool_DontReturn(ref _array, newCapacity);
				_isRented = true;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Expand(int newCapacity, T defaultValue)
		{
			if (newCapacity < _array.Length)
				return;

			var previousLength = _array.Length;
			if (_isRented)
				ArrayExt.Expand_WithPool(ref _array, newCapacity);
			else
			{
				ArrayExt.Expand_WithPool_DontReturn(ref _array, newCapacity);
				_isRented = true;
			}

			Array.Fill(_array, defaultValue, previousLength, _array.Length - previousLength);
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
			if (_array == null!)
				return;

			if (_isRented)
				ArrayPool<T>.Shared.Return(_array, true);
			_array = null!;

			GC.SuppressFinalize(this);
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

		public void CopyTo(T[] array, int arrayIndex)
		{
			var count = array.Length - arrayIndex;
			if (_count < count)
				count = _count;
			Array.Copy(_array, 0, array, arrayIndex, count);
		}

		public T[] GetInnerArray()
		{
			return _array;
		}

		public ReadOnlySimpleList<T> ToReadOnly()
		{
			return new ReadOnlySimpleList<T>(this);
		}

		public static implicit operator ReadOnlySimpleList<T>(SimpleList<T> list)
		{
			return list.ToReadOnly();
		}

		public struct Enumerator : IEnumerator<T>
		{
			private readonly SimpleList<T> _list;
			private int _index;

			public T Current => _list[_index];

			object IEnumerator.Current => Current!;

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

			public void Dispose()
			{
			}
		}
	}
}
