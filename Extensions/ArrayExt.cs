using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Sapientia.Collections;

namespace Sapientia.Extensions
{
	/// <summary>
	/// https://www.notion.so/Extension-b985410501c742dabb3a08ca171a319c?pvs=4#5dd46b19b2fa4e1c8d6ac4fabf6a19cd
	/// </summary>
	public static class ArrayExt
	{
		public static bool IsArrayEqual<T>(this T[] a, T[] b)
		{
			if (Equals(a, b))
				return true;
			if (a == null || b == null || a.Length != b.Length)
				return false;
			for (var i = 0; i < a.Length; i++)
			{
				if (a[i] is IEquatable<T> equatable)
				{
					if (!equatable.Equals(b[i]))
						return false;
				}
				else if (!a[i].Equals(b[i]))
					return false;
			}
			return true;
		}

		public static void Move<T>(this T[] array, int from, int to)
		{
			var value = array[from];
			if (to < from)
			{
				Array.Copy(array, to, array, to + 1, from - to);
			}
			else if (to > from)
			{
				Array.Copy(array, from + 1, array, from, to - from);
			}
			else return;

			array[to] = value;
		}

		public static void MoveToEnd<T>(this T[] array, int index)
		{
			var value = array[index];
			var sourceIndex = index + 1;
			Array.Copy(array, sourceIndex, array, index, array.Length - sourceIndex);
			array[^1] = value;
		}

		public static void Expand_WithPool_DontReturn<T>(ref T[] array, int newCapacity)
		{
			var newArray = ArrayPool<T>.Shared.Rent(newCapacity);
			Array.Copy(array, newArray, array.Length);

			array = newArray;
		}

		public static void Expand_WithPool<T>(ref T[] array, int newCapacity)
		{
			var newArray = ArrayPool<T>.Shared.Rent(newCapacity);
			Array.Copy(array, newArray, array.Length);

			ArrayPool<T>.Shared.Return(array);
			array = newArray;
		}

		public static void SetLength<T>(ref T[] array, int targetLength, in T defaultValue)
		{
			if (array.Length > targetLength)
				Reduce(ref array, targetLength);
			else if (array.Length < targetLength)
				Expand(ref array, targetLength, defaultValue);
		}

		public static void Reduce<T>(ref T[] array, int newCapacity)
		{
			var newArray = new T[newCapacity];
			Array.Copy(array, newArray, newCapacity);

			array = newArray;
		}

		public static void Expand<T>(ref T[] array, int newCapacity, T defaultValue)
		{
			var newArray = new T[newCapacity];
			Array.Copy(array, newArray, array.Length);
			Array.Fill(newArray, defaultValue, array.Length, newCapacity - array.Length);

			array = newArray;
		}

		public static void Expand<T>(ref T[] array, int newCapacity)
		{
			var newArray = new T[newCapacity];
			Array.Copy(array, newArray, array.Length);

			array = newArray;
		}

		public static T[] Copy_WithPool<T>(this T[] array)
		{
			var newArray = ArrayPool<T>.Shared.Rent(array.Length);
			Array.Copy(array, newArray, array.Length);

			ArrayPool<T>.Shared.Return(array);
			return newArray;
		}

		public static T[] Copy<T>(this T[] array)
		{
			var newArray = new T[array.Length];
			Array.Copy(array, newArray, array.Length);

			return newArray;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Fill<T>(this T[] array, T defaultValue)
		{
			Array.Fill(array, defaultValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Sort<T>(this T[] array)
		{
			Array.Sort(array);
		}

		public static void ShiftRight<T>(this T[] array, int index, int shiftLenght)
		{
			var destinationIndex = index + shiftLenght;
			var count = array.Length - destinationIndex;

			if (count > 0)
				Array.Copy(array, index, array, destinationIndex, count);
		}

		// Test and remove "Obsolete" attribute
		[Obsolete("Not Tested")]
		public static void ShiftLeft<T>(this T[] array, int index, int shiftLenght)
		{
			var destinationIndex = index - shiftLenght;
			if (destinationIndex < 0)
			{
				index += destinationIndex;
				destinationIndex = 0;
			}
			var count = array.Length - index;

			if (count > 0)
				Array.Copy(array, index, array, destinationIndex, count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ReadOnlySimpleList<T> WrapToReadOnlySimpleList<T>(this T[] array)
		{
			return ReadOnlySimpleList<T>.WrapArray(array);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SimpleList<T> WrapToSimpleList<T>(this T[] array)
		{
			return SimpleList<T>.WrapArray(array);
		}

		public static void RemoveAt<T>(ref T[] array, int index)
		{
			var newArray = new T[array.Length - 1];
			if (index != newArray.Length)
			{
				Array.Copy(array, index + 1, newArray, index, newArray.Length - index);
			}
			if (index != 0)
			{
				Array.Copy(array, 0, newArray, 0, index);
			}
			array = newArray;
		}

		public static void Add<T>(ref T[] array, T value)
		{
			Expand(ref array, array.Length + 1);
			array[^1] = value;
		}

		public static void AddRange<T>(ref T[] array, T[] values)
		{
			var destinationIndex = array.Length;
			Expand(ref array, array.Length + values.Length);
			Array.Copy(values, 0, array, destinationIndex, array.Length);
		}

		public static int IndexOf<T>(this T[] array, T element) => Array.IndexOf(array, element);
	}
}
