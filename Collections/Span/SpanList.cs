using System;

namespace Sapientia.Collections
{
	public ref struct SpanList<T> where T: unmanaged
	{
		private Span<T> _array;
		private int _count;

		public int Count => _count;
		public int Capacity => _array.Length;

		public SpanList(Span<T> array)
		{
			_array = array;
			_count = 0;
		}

		public void Add(T item)
		{
			_array[_count++] = item;
		}

		public Span<T> GetSpan()
		{
			return _array[.._count];
		}

		public Span<T>.Enumerator GetEnumerator()
		{
			return GetSpan().GetEnumerator();
		}

		public static implicit operator SpanList<T>(Span<T> span)
		{
			return new SpanList<T>(span);
		}
	}
}
