using System;
using System.Collections.Generic;
using Sapientia.Collections;

namespace Sapientia.MemoryAllocator
{
	public static class MemListExt
	{
		public static void Sort<T>(this MemList<T> list, WorldState worldState, Comparison<T> comparison) where T: unmanaged
		{
			var span = list.GetSpan(worldState);
			span.QuickSort(comparison);
		}

		public static void Sort<T>(this MemList<T> list, WorldState worldState) where T: unmanaged, IComparable<T>
		{
			var span = list.GetSpan(worldState);
			span.QuickSort();
		}

		public static void Sort<T>(this MemList<T> list, WorldState worldState, int index, int count) where T: unmanaged, IComparable<T>
		{
			var span = list.GetSpan(worldState);
			span.QuickSort(index, count);
		}

		public static void Sort<T, TComparer>(this MemList<T> list, WorldState worldState, TComparer comparer) where TComparer : IComparer<T> where T: unmanaged
		{
			var span = list.GetSpan(worldState);
			span.QuickSort(comparer);
		}

		public static void Sort<T, TComparer>(this MemList<T> list, WorldState worldState, int index, int count, TComparer comparer)
			where TComparer : IComparer<T>
			where T: unmanaged
		{
			var span = list.GetSpan(worldState);
			span.QuickSort<T, TComparer>(index, count, comparer);
		}

		public static void BinaryRemove<T>(this MemList<T> list, WorldState worldState, T value) where T: unmanaged, IComparable<T>
		{
			BinaryRemove(list, worldState, value, new DefaultComparer<T>());
		}

		public static void BinaryRemove<T, TComparer>(this MemList<T> list, WorldState worldState, T value, TComparer comparer)
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

		public static void BinaryInsert<T>(this MemList<T> list, WorldState worldState, T value) where T: unmanaged, IComparable<T>
		{
			BinaryInsert(list, worldState, value, new DefaultComparer<T>());
		}

		public static void BinaryInsert<T, TComparer>(this MemList<T> list, WorldState worldState, T value, TComparer comparer)
			where TComparer : IComparer<T>
			where T: unmanaged
		{
			var index = BinarySearch(list, worldState, list.Count, value, comparer);
			if (index < 0)
				index = ~index;
			list.Insert(worldState, index, value);
		}

		public static int BinarySearch<T>(this MemList<T> list, WorldState worldState, T value) where T: unmanaged, IComparable<T>
		{
			return BinarySearch(list, worldState, list.Count, value, new DefaultComparer<T>());
		}

		public static int BinarySearch<T, TComparer>(this MemList<T> list, WorldState worldState, T value, TComparer comparer)
			where TComparer : IComparer<T>
			where T: unmanaged
		{
			return BinarySearch(list, worldState, list.Count, value, comparer);
		}

		public static int BinarySearch<T, TComparer>(this MemList<T> list, WorldState worldState, int length, T value, TComparer comparer)
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
