using System;
using System.Collections.Generic;

namespace Sapientia.Collections
{
	public readonly struct ListSegment<T>
	{
		private readonly IReadOnlyList<T> _list;
		private readonly int _start;
		private readonly int _end;

		public int Count { get; }
		public T this[int index] { get => _list[_start + index]; }

		public ListSegment(IReadOnlyList<T> list, int start, int end)
		{
			if (end >= list.Count)
				throw new ArgumentOutOfRangeException(nameof(end));
			if (start > end)
				throw new ArgumentOutOfRangeException(nameof(start));

			_list = list;
			_start = start;
			_end = end;

			Count = _end - _start + 1;
		}

		public Enumerator GetEnumerator() =>
			_list != null ?
			new Enumerator(_list, _start, _end) :
			default;

		public struct Enumerator
		{
			private readonly IReadOnlyList<T> _list;
			private readonly int _end;
			private int _index;

			public T Current { get => _list[_index]; }

			internal Enumerator(IReadOnlyList<T> list, int start, int end)
			{
				_list = list;
				_end = end;
				_index = start - 1;
			}

			public bool MoveNext() => _list != null && ++_index <= _end;
		}
	}

	public static class ListSegmentExtensions
	{
		public static bool IsEmpty<T>(this ListSegment<T> segment)
			=> segment.Count == 0;

		public static ListSegment<T> SliceSegment<T>(this IReadOnlyList<T> list, int startIndex, int endIndex)
			=> new ListSegment<T>(list, startIndex, endIndex);

		public static ListSegment<T> SliceSegmentFromStart<T>(this IReadOnlyList<T> list, int endIndex)
			=> new ListSegment<T>(list, 0, endIndex);

		public static ListSegment<T> SliceSegmentTillEnd<T>(this IReadOnlyList<T> list, int startIndex)
			=> new ListSegment<T>(list, startIndex, list.Count - 1);
	}
}
