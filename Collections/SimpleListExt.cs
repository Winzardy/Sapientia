using System;
using System.Collections.Generic;

namespace Sapientia.Collections
{
	public static class SimpleListExt
	{
		public struct LambdaComparer<T> : IComparer<T>
		{
			private Comparison<T> _comparison;

			public LambdaComparer(Comparison<T> comparison)
			{
				_comparison = comparison;
			}

			public int Compare(T x, T y) => _comparison.Invoke(x, y);
		}

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

		public static void Sort<T>(this SimpleList<T> list, Comparison<T> comparison)
		{
			list.Sort(new LambdaComparer<T>(comparison));
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

		public static void BinaryRemove<T>(this SimpleList<T> list, T value) where T: IComparable<T>
		{
			BinaryRemove(list, value, new DefaultComparer<T>());
		}

		public static void BinaryRemove<T, TComparer>(this SimpleList<T> list, T value, TComparer comparer)
			where TComparer : IComparer<T>
		{
			var index = BinarySearch(list, list.Count, value, comparer);
			if (index < 0)
				return;

			if (list[index].Equals(value))
			{
				list.RemoveAt(index);
				return;
			}

			var indexToBottom = index - 1;
			while (indexToBottom >= 0 && comparer.Compare(list[indexToBottom], value) == 0)
			{
				if (list[indexToBottom].Equals(value))
				{
					list.RemoveAt(indexToBottom);
					return;
				}
				indexToBottom--;
			}

			var indexToTop = index + 1;
			while (indexToTop < list.Count && comparer.Compare(list[indexToTop], value) == 0)
			{
				if (list[indexToTop].Equals(value))
				{
					list.RemoveAt(indexToTop);
					return;
				}
				indexToTop++;
			}
		}

		public static void BinaryInsert<T>(this SimpleList<T> list, T value) where T: IComparable<T>
		{
			BinaryInsert(list, value, new DefaultComparer<T>());
		}

		public static void BinaryInsert<T, TComparer>(this SimpleList<T> list, T value, TComparer comparer)
			where TComparer : IComparer<T>
		{
			var index = BinarySearch(list, list.Count, value, comparer);
			if (index < 0)
				index = ~index;
			list.Insert(index, value);
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
