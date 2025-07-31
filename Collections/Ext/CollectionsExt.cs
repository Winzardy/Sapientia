using System;
using System.Collections.Generic;
using System.Linq;
using Sapientia.Extensions;
using Sapientia.Pooling;
#if UNITY_5_3_OR_NEWER
using Random = UnityEngine.Random;
#endif

namespace Sapientia.Collections
{
	/// <summary>
	/// https://www.notion.so/Extension-b985410501c742dabb3a08ca171a319c?pvs=4#b48166b7e5c44ce8957d060d93577fe3
	/// </summary>
	public static class CollectionsExt
	{
		public static void SetCount<T>(this List<T> list, int targetCount, in Func<T> defaultValueCreator)
		{
			if (list.Count > targetCount)
				list.RemoveRange(targetCount, list.Count - targetCount);
			else
			{
				for (var i = list.Count; i < targetCount; i++)
				{
					list.Add(defaultValueCreator.Invoke());
				}
			}
		}

		public static void SetCount<T>(this List<T> list, int targetCount, in T defaultValue = default!)
		{
			if (list.Count > targetCount)
				list.RemoveRange(targetCount, list.Count - targetCount);
			else
			{
				list.AddRangeRepeated(defaultValue, (targetCount - list.Count));
			}
		}

		public static void FillIndexes(this int[] array)
		{
			var length = array.Length;
			for (var i = 0; i < length; i++)
			{
				array[i] = i;
			}
		}

		public static void FillIndexes(this SimpleList<int> list, int length)
		{
			list.Expand(length);
			for (var i = 0; i < length; i++)
			{
				list.AddWithoutExpand(i);
			}
		}

		public static void Shuffle<T>(this T[] array)
		{
			var length = array.Length;
			for (var i = 0; i < length; i++)
			{
#if UNITY_5_3_OR_NEWER
				var indexToSwap = Random.Range(i, length);
#else
				var indexToSwap = System.Random.Shared.Next(i, length);
#endif
				(array[i], array[indexToSwap]) = (array[indexToSwap], array[i]);
			}
		}

		public static void Shuffle<T>(this SimpleList<T> list)
		{
			ShuffleInternal(list, list.Count);
		}

		public static void Shuffle<T>(this SimpleList<T> list, int count)
		{
			count = count.Min(list.Count);
			ShuffleInternal(list, count);
		}

		private static void ShuffleInternal<T>(this SimpleList<T> list, int count)
		{
			for (var i = 0; i < count; i++)
			{
#if UNITY_5_3_OR_NEWER
				var indexToSwap = Random.Range(i, list.Count);
#else
				var indexToSwap = System.Random.Shared.Next(i, list.Count);
#endif
				(list[i], list[indexToSwap]) = (list[indexToSwap], list[i]);
			}
		}

		public static void Shuffle<T>(this List<T> list)
		{
			var length = list.Count;
			for (var i = 0; i < length; i++)
			{
#if UNITY_5_3_OR_NEWER
				var indexToSwap = Random.Range(i, length);
#else
				var indexToSwap = System.Random.Shared.Next(i, length);
#endif
				(list[i], list[indexToSwap]) = (list[indexToSwap], list[i]);
			}
		}

		public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
		{
			return collection == null || IsEmpty(collection);
		}

		public static bool IsEmpty<T>(this ICollection<T> collection)
		{
			return collection.Count == 0;
		}

		public static bool Any<T>(this ICollection<T> collection)
			=> !collection.IsNullOrEmpty();

		public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
		{
			return enumerable == null || IsEmpty(enumerable);
		}

		public static bool ReferenceContains<T>(this IList<T> collection, T item)
		{
			if (!Any(collection))
				return false;

			// ReSharper disable once ForCanBeConvertedToForeach
			// ReSharper disable once LoopCanBeConvertedToQuery
			for (var i = 0; i < collection.Count; i++)
			{
				if (ReferenceEquals(collection[i], item))
					return true;
			}

			return false;
		}

		public static bool ReferenceContains<T>(this IEnumerable<T> collection, T target)
		{
			foreach (var item in collection)
			{
				if (ReferenceEquals(item, target))
					return true;
			}

			return false;
		}

		public static bool Any<T>(this IList<T> collection, Func<T, bool> predicate)
		{
			if (!Any(collection))
				return false;

			// ReSharper disable once ForCanBeConvertedToForeach
			// ReSharper disable once LoopCanBeConvertedToQuery
			//TODO:CollectionsMarshal.AsSpan() в .NET 5+
			for (var i = 0; i < collection.Count; i++)
			{
				if (predicate(collection[i]))
					return true;
			}

			return false;
		}

		public static bool All<T>(this IList<T> collection, Func<T, bool> predicate)
		{
			// ReSharper disable once ForCanBeConvertedToForeach
			// ReSharper disable once LoopCanBeConvertedToQuery

			//TODO:CollectionsMarshal.AsSpan() в .NET 5+
			for (var i = 0; i < collection.Count; i++)
			{
				if (!predicate(collection[i]))
					return false;
			}

			return true;
		}

		public static bool Any<T>(this T[] array, Predicate<T> predicate)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));

			// ReSharper disable once ForCanBeConvertedToForeach
			// ReSharper disable once LoopCanBeConvertedToQuery
			for (var i = 0; i < array.Length; i++)
			{
				if (predicate(in array[i]))
					return true;
			}

			return false;
		}

		public static bool All<T>(this T[] array, Predicate<T> predicate)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));

			// ReSharper disable once ForCanBeConvertedToForeach
			// ReSharper disable once LoopCanBeConvertedToQuery
			for (var i = 0; i < array.Length; i++)
			{
				if (!predicate(in array[i]))
					return false;
			}

			return true;
		}

		public static bool AnySafe<T>(this T[] array, Predicate<T> predicate)
		{
			if (array == null)
				return false;

			// ReSharper disable once ForCanBeConvertedToForeach
			// ReSharper disable once LoopCanBeConvertedToQuery
			for (var i = 0; i < array.Length; i++)
			{
				if (predicate(in array[i]))
					return true;
			}

			return false;
		}

		public static bool AllSafe<T>(this T[] array, Predicate<T> predicate)
		{
			if (array == null)
				return false;

			// ReSharper disable once ForCanBeConvertedToForeach
			// ReSharper disable once LoopCanBeConvertedToQuery
			for (var i = 0; i < array.Length; i++)
			{
				if (!predicate(in array[i]))
					return false;
			}

			return true;
		}

		public static bool ContainsIndex<T>(this T[] array, int index)
		{
			return index >= 0 && index < array.Length;
		}

		public static bool ContainsIndexSafe<T>(this T[] array, int index)
		{
			if (array == null)
				return false;

			return index >= 0 && index < array.Length;
		}

		public static bool ContainsIndexSafe<T>(this IList<T> list, int index)
		{
			if (list == null)
				return false;

			return index >= 0 && index < list.Count;
		}

		public static T First<T>(this IList<T> list) => list[0];

		public static T FirstOrDefault<T>(this IList<T>? list, T defaultValue = default)
			=> ElementAtOrDefault(list, 0, defaultValue);

		public static T Second<T>(this IList<T> list) => list[1];

		public static T SecondOrDefault<T>(this IList<T>? list, T defaultValue = default)
			=> ElementAtOrDefault(list, 1, defaultValue);

		public static T Third<T>(this IList<T> list) => list[2];

		public static T ElementAtOrDefault<T>(this IList<T>? list, int index, T defaultValue = default)
		{
			if (list == null || list.Count - 1 < index)
				return defaultValue;

			return list[index];
		}

		public static T ThirdOrDefault<T>(this IList<T>? list, T defaultValue = default)
			=> ElementAtOrDefault(list, 2, defaultValue);

		public static T Last<T>(this IList<T> list) => list[^1];

		public static T LastOrDefault<T>(this IList<T> list, T defaultValue = default)
		{
			if (IsNullOrEmpty(list))
				return defaultValue;

			return Last(list);
		}

		public static bool IsEmpty<T>(this IEnumerable<T> enumerable) => !enumerable.Any();

		public static List<T> AddRangeRepeated<T>(this List<T> list, T repeatedItem, int count)
		{
			list.AddRange(Enumerable.Repeat(repeatedItem, count));
			return list;
		}

		public static IEnumerable<(T, int)> WithIndex<T>(this IList<T> list)
		{
			if (list == null)
				throw new ArgumentException("List is null", nameof(list));

			for (int i = 0; i < list.Count; i++)
				yield return (list[i], i);
		}

		public static T[] ToArray<T>(this ICollection<T>? collection)
		{
			if (collection?.IsNullOrEmpty() ?? true)
				return Array.Empty<T>();

			var array = new T[collection.Count];
			collection.CopyTo(array, 0);
			return array;
		}

		public static IEnumerable<(T, int)> WithIndexSafe<T>(this IList<T> list)
		{
			if (list == null)
				yield break;

			for (int i = 0; i < list.Count; i++)
				yield return (list[i], i);
		}

		public static IEnumerable<(T, int)> WithIndexSafe<T>(this IEnumerable<T> enumerable)
		{
			if (enumerable == null)
				yield break;

			var index = 0;

			foreach (var value in enumerable)
				yield return (value, index++);
		}

		public static IEnumerable<(T, int)> WithIndex<T>(this IEnumerable<T> enumerable)
		{
			if (enumerable == null)
				throw new ArgumentException("Enumerable is null", nameof(enumerable));

			var index = 0;

			foreach (var value in enumerable)
				yield return (value, index++);
		}

		public delegate bool Predicate<T>(in T value);
	}
}
