using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sapientia.Collections
{
	public readonly struct ReadOnlySimpleList<T> : IReadOnlyList<T>
	{
		private readonly SimpleList<T> _simpleList;

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _simpleList.Count;
		}

		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _simpleList.Capacity;
		}

		public T Last
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _simpleList.Last;
		}

		public bool IsFull
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _simpleList.IsFull;
		}

		public T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _simpleList[index];
		}

		public ReadOnlySimpleList(T[] array, int capacity = SimpleList<T>.DEFAULT_CAPACITY, bool isRented = true)
		{
			_simpleList = new SimpleList<T>(array, capacity, isRented);
		}

		public ReadOnlySimpleList(SimpleList<T> simpleList)
		{
			_simpleList = simpleList;
		}

		public static ReadOnlySimpleList<T> WrapArray(T[] array)
		{
			return new ReadOnlySimpleList<T>(SimpleList<T>.WrapArray(array));
		}

		public T[] ToArray()
		{
			return _simpleList.ToArray();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _simpleList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}