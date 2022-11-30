using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Sapientia.Extensions
{
	public static class ArrayExtensions
	{
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
	}
}