using System;
using System.Collections.Generic;

namespace Sapientia.Collections
{
	public static class UnsafeListExt
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

		public static void BinaryRemove<T>(this UnsafeList<T> list, T value) where T: unmanaged, IComparable<T>
		{
			BinaryRemove(list, value, new DefaultComparer<T>());
		}

		public static void BinaryRemove<T, TComparer>(this UnsafeList<T> list, T value, TComparer comparer)
			where T: unmanaged
			where TComparer : IComparer<T>
		{
			var index = BinarySearch(list, list.count, value, comparer);
			if (index < 0)
				return;

			if (comparer.Compare(list[index], value) == 0)
			{
				list.RemoveAt(index);
				return;
			}

			var indexToBottom = index - 1;
			while (indexToBottom >= 0 && comparer.Compare(list[indexToBottom], value) == 0)
			{
				if (comparer.Compare(list[indexToBottom], value) == 0)
				{
					list.RemoveAt(indexToBottom);
					return;
				}
				indexToBottom--;
			}

			var indexToTop = index + 1;
			while (indexToTop < list.count && comparer.Compare(list[indexToTop], value) == 0)
			{
				if (comparer.Compare(list[indexToTop], value) == 0)
				{
					list.RemoveAt(indexToTop);
					return;
				}
				indexToTop++;
			}
		}

		public static void BinaryInsert<T>(this UnsafeList<T> list, T value) where T: unmanaged, IComparable<T>
		{
			BinaryInsert(list, value, new DefaultComparer<T>());
		}

		public static void BinaryInsert<T, TComparer>(this UnsafeList<T> list, T value, TComparer comparer)
			where T: unmanaged
			where TComparer : IComparer<T>
		{
			var index = BinarySearch(list, list.count, value, comparer);
			if (index < 0)
				index = ~index;
			list.Insert(index, value);
		}

		public static int BinarySearch<T>(this UnsafeList<T> list, T value) where T: unmanaged, IComparable<T>
		{
			return BinarySearch(list, list.count, value, new DefaultComparer<T>());
		}

		public static int BinarySearch<T, TComparer>(this UnsafeList<T> list, T value, TComparer comparer)
			where T: unmanaged
			where TComparer : IComparer<T>
		{
			return BinarySearch(list, list.count, value, comparer);
		}

		public static int BinarySearch<T, TComparer>(this UnsafeList<T> list, int length, T value, TComparer comparer)
			where T: unmanaged
			where TComparer: IComparer<T>
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
