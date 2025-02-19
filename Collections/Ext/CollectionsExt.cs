using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sapientia.Collections;
using Sapientia.Collections.Archetypes;

namespace Sapientia.Extensions
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

		public static void SetCount<T>(this List<T> list, int targetCount, in T defaultValue)
		{
			if (list.Count > targetCount)
				list.RemoveRange(targetCount, list.Count - targetCount);
			else
			{
				for (var i = list.Count; i < targetCount; i++)
				{
					list.Add(defaultValue);
				}
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

		public static void FillEntities(this SimpleList<Entity> list, int length, string entitiesName)
		{
			list.Expand(length);
			for (var i = 0; i < length; i++)
			{
				list.AddWithoutExpand(Entity.Create(entitiesName));
			}
		}

		public static void Shuffle<T>(this T[] array)
		{
			var length = array.Length;
			for (var i = 0; i < length; i++)
			{
#if UNITY_5_3_OR_NEWER
				var indexToSwap = UnityEngine.Random.Range(i, length);
#else
				var indexToSwap = System.Random.Shared.Next(i, length);
#endif
				(array[i], array[indexToSwap]) = (array[indexToSwap], array[i]);
			}
		}

		public static void Shuffle<T>(this SimpleList<T> list)
		{
			var length = list.Count;
			for (var i = 0; i < length; i++)
			{
#if UNITY_5_3_OR_NEWER
				var indexToSwap = UnityEngine.Random.Range(i, length);
#else
				var indexToSwap = System.Random.Shared.Next(i, length);
#endif
				(list[i], list[indexToSwap]) = (list[indexToSwap], list[i]);
			}
		}

		public static void Shuffle<T>(this System.Collections.Generic.List<T> list)
		{
			var length = list.Count;
			for (var i = 0; i < length; i++)
			{
#if UNITY_5_3_OR_NEWER
				var indexToSwap = UnityEngine.Random.Range(i, length);
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

		public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
		{
			return !enumerable.Any();
		}

		public static List<T> AddRangeRepeated<T>(this List<T> list, T repeatedItem, int count)
		{
			list.AddRange(Enumerable.Repeat(repeatedItem, count));
			return list;
		}

		public static IEnumerable<(T, int)> WithIndex<T>(this IEnumerable<T> enumerable)
		{
			int index = 0;

			foreach (var value in enumerable)
			{
				yield return (value, index);
				index++;
			}
		}
	}
}
