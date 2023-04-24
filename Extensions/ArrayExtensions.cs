using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Sapientia.Extensions
{
	public static class ArrayExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsArrayEqual<T>(this T[] a, T[] b)
		{
			if (a.Equals(b))
				return true;
			if (b == null || a.Length != b.Length)
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MoveToEnd<T>(this T[] array, int index)
		{
			var value = array[index];
			var sourceIndex = index + 1;
			Array.Copy(array, sourceIndex, array, index, array.Length - sourceIndex);
			array[^1] = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Expand_WithPool<T>(ref T[] array, int newCapacity)
		{
			var newArray = ArrayPool<T>.Shared.Rent(newCapacity);
			Array.Copy(array, newArray, array.Length);

			ArrayPool<T>.Shared.Return(array);
			array = newArray;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Expand<T>(ref T[] array, int newCapacity)
		{
			var newArray = new T[newCapacity];
			Array.Copy(array, newArray, array.Length);

			array = newArray;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] Copy_WithPool<T>(this T[] array)
		{
			var newArray = ArrayPool<T>.Shared.Rent(array.Length);
			Array.Copy(array, newArray, array.Length);

			ArrayPool<T>.Shared.Return(array);
			return newArray;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ShiftRight<T>(this T[] array, int index, int shiftLenght)
		{
			var destinationIndex = index + shiftLenght;
			var count = array.Length - destinationIndex;

			if (count > 0)
				Array.Copy(array, index, array, destinationIndex, count);
		}
	}
}