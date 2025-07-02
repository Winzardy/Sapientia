using System;
using System.Collections.Generic;

namespace Sapientia.MemoryAllocator
{
	public static unsafe class ListExt
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

		public static void Sort<T>(this List<T> list, WorldState worldState, Comparison<T> comparison) where T: unmanaged
		{
			list.Sort(worldState, new LambdaComparer<T>(comparison));
		}

		public static void Sort<T>(this List<T> list, WorldState worldState) where T: unmanaged, IComparable<T>
		{
			list.Sort(worldState, 0, list.Count, new DefaultComparer<T>());
		}

		public static void Sort<T>(this List<T> list, WorldState worldState, int index, int count) where T: unmanaged, IComparable<T>
		{
			list.Sort(worldState, index, count, new DefaultComparer<T>());
		}

		public static void Sort<T, TComparer>(this List<T> list, WorldState worldState, TComparer comparer) where TComparer : IComparer<T> where T: unmanaged
		{
			list.Sort(worldState, 0, list.Count, comparer);
		}

		public static void Sort<T, TComparer>(this List<T> list, WorldState worldState, int index, int count, TComparer comparer)
			where TComparer : IComparer<T>
			where T: unmanaged
		{
			if (count <= 1)
				return;
			var array = list.GetValuePtr(worldState);

			// Используем алгоритм быстрой сортировки
			var stack = stackalloc int[2 * 32];
			var sp = 0;

			stack[sp++] = index;
			stack[sp++] = index + count - 1;

			while (sp > 0)
			{
				var right = stack[--sp];
				var left = stack[--sp];

				var i = left;
				var j = right;
				var pivot = array[(left + right) / 2];

				while (i <= j)
				{
					while (comparer.Compare(array[i], pivot) < 0)
						i++;
					while (comparer.Compare(array[j], pivot) > 0)
						j--;

					if (i <= j)
					{
						(array[i], array[j]) = (array[j], array[i]);
						i++;
						j--;
					}
				}

				if (left < j)
				{
					stack[sp++] = left;
					stack[sp++] = j;
				}
				if (i < right)
				{
					stack[sp++] = i;
					stack[sp++] = right;
				}
			}
		}

		public static void BinaryRemove<T>(this List<T> list, WorldState worldState, T value) where T: unmanaged, IComparable<T>
		{
			BinaryRemove(list, worldState, value, new DefaultComparer<T>());
		}

		public static void BinaryRemove<T, TComparer>(this List<T> list, WorldState worldState, T value, TComparer comparer)
			where TComparer : IComparer<T>
			where T: unmanaged
		{
			var index = BinarySearch(list, worldState, list.Count, value, comparer);
			if (index < 0)
				return;

			if (comparer.Compare(list[worldState, index], value) == 0)
			{
				list.RemoveAt(worldState, index);
				return;
			}

			var indexToBottom = index - 1;
			while (indexToBottom >= 0 && comparer.Compare(list[worldState, indexToBottom], value) == 0)
			{
				if (list[worldState, indexToBottom].Equals(value))
				{
					list.RemoveAt(worldState, indexToBottom);
					return;
				}
				indexToBottom--;
			}

			var indexToTop = index + 1;
			while (indexToTop < list.Count && comparer.Compare(list[worldState, indexToTop], value) == 0)
			{
				if (list[worldState, indexToTop].Equals(value))
				{
					list.RemoveAt(worldState, indexToTop);
					return;
				}
				indexToTop++;
			}
		}

		public static void BinaryInsert<T>(this List<T> list, WorldState worldState, T value) where T: unmanaged, IComparable<T>
		{
			BinaryInsert(list, worldState, value, new DefaultComparer<T>());
		}

		public static void BinaryInsert<T, TComparer>(this List<T> list, WorldState worldState, T value, TComparer comparer)
			where TComparer : IComparer<T>
			where T: unmanaged
		{
			var index = BinarySearch(list, worldState, list.Count, value, comparer);
			if (index < 0)
				index = ~index;
			list.Insert(worldState, index, value);
		}

		public static int BinarySearch<T>(this List<T> list, WorldState worldState, T value) where T: unmanaged, IComparable<T>
		{
			return BinarySearch(list, worldState, list.Count, value, new DefaultComparer<T>());
		}

		public static int BinarySearch<T, TComparer>(this List<T> list, WorldState worldState, T value, TComparer comparer)
			where TComparer : IComparer<T>
			where T: unmanaged
		{
			return BinarySearch(list, worldState, list.Count, value, comparer);
		}

		public static int BinarySearch<T, TComparer>(this List<T> list, WorldState worldState, int length, T value, TComparer comparer)
			where TComparer: IComparer<T>
			where T: unmanaged
		{
			var offset = 0;

			for (var l = length; l != 0; l >>= 1)
			{
				var idx = offset + (l >> 1);
				var curr = list[worldState, idx];
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
