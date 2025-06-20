using System.Collections.Generic;

namespace Sapientia.Collections
{
	public struct ArrayReference<T>
	{
		private T[] _array;
		private int _index;

		public ArrayReference(T[] array, int index)
		{
			_array = array;
			_index = index;
		}

		public ref readonly T Value => ref _array[_index];

		public bool IsEmpty => _array is not {Length: > 0};
	}

	/// <summary>
	/// Есть ArraySegment, но у него доступ по индексу не ref!
	/// </summary>
	public struct ArraySection<T>
	{
		private T[] _array;

		private int _start;
		private int _end;

		public ArraySection(T[] array, int start, int end)
		{
			_array = array;

			_start = start;
			_end = end;
		}

		public ref readonly T this[int index] => ref _array[index];

		public bool Contains(int index)
		{
			if (index < _start)
				return false;

			if (index > _end)
				return false;

			return true;
		}

		public IEnumerator<int> GetEnumerator()
		{
			for (int i = _start; i < _end; i++)
				yield return i;
		}
	}
}
