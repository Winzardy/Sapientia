using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Sapientia.Extensions
{
	public static class EnumValues<T> where T : unmanaged, Enum
	{
		public static readonly T[] VALUES = (T[])Enum.GetValues(typeof(T));
		public static readonly int ENUM_LENGHT = VALUES.Length;
	}

	public static class EnumNames<T> where T : unmanaged, Enum
	{
		public static readonly string[] NAMES = Enum.GetNames(typeof(T));
		public static readonly int ENUM_LENGHT = NAMES.Length;
	}

	public static class EnumToIndex<T> where T : unmanaged, Enum
	{
		private static readonly Dictionary<T, int> VALUE_TO_INDEX;

		static EnumToIndex()
		{
			var values = EnumValues<T>.VALUES;

			VALUE_TO_INDEX = new Dictionary<T, int>(values.Length);
			for (var i = 0; i < values.Length; i++)
			{
				VALUE_TO_INDEX.Add(values[i], i);
			}
		}

		public static bool TryGetIndex(T value, out int index)
		{
			return VALUE_TO_INDEX.TryGetValue(value, out index);
		}

		public static int GetIndex(T value)
		{
			return VALUE_TO_INDEX[value];
		}
	}

	/// <summary>
	/// https://www.notion.so/Extension-b985410501c742dabb3a08ca171a319c?pvs=4#dd036a4692ab425cb8ff0fb407a38de7
	/// </summary>
	public static class EnumExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe T Min<T>(this T a, T b) where T : unmanaged, Enum
		{
			return *(int*)(&a) < *(int*)(&b) ? a : b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe T Max<T>(this T a, T b) where T : unmanaged, Enum
		{
			return *(int*)(&a) > *(int*)(&b) ? a : b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe int ToInt<T>(this T value) where T : unmanaged, Enum
		{
			return *(int*)(&value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe uint ToUInt<T>(this T value) where T : unmanaged, Enum
		{
			return *(uint*)(&value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe byte ToByte<T>(this T value) where T : unmanaged, Enum
		{
			return *(byte*)(&value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe short ToShort<T>(this T value) where T : unmanaged, Enum
		{
			return *(short*)(&value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe T1 ToEnum<T, T1>(this T value) where T : unmanaged, Enum where T1 : unmanaged, Enum
		{
			return *(T1*)(&value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T ToEnum<T>(this string value) where T : unmanaged, Enum
		{
			return Enum.Parse<T>(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryToEnum<T>(this string value, out T result) where T : unmanaged, Enum
		{
			return Enum.TryParse<T>(value, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe T ToEnum<T>(this int value) where T : unmanaged, Enum
		{
			return *(T*)(&value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe T ToEnum<T>(this byte value) where T : unmanaged, Enum
		{
			return *(T*)(&value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasIntFlag<T>(this T value, T flag) where T : unmanaged, Enum
		{
			return HasIntFlag(value.ToInt(), flag);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasIntFlag<T>(this int valueInt, T flag) where T : unmanaged, Enum
		{
			var flagInt = flag.ToInt();
			return (valueInt & flagInt) == flagInt;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasNotIntFlag<T>(this T value, T flag) where T : unmanaged, Enum
		{
			return HasNotIntFlag(value.ToInt(), flag);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasNotIntFlag<T>(this int valueInt, T flag) where T : unmanaged, Enum
		{
			var flagInt = flag.ToInt();
			return (valueInt & flagInt) != flagInt;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasIntFlagIntersection<T>(this T value, T flag) where T : unmanaged, Enum
		{
			return HasIntFlagIntersection(value.ToInt(), flag);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasIntFlagIntersection<T>(this int valueInt, T flag) where T : unmanaged, Enum
		{
			var flagInt = flag.ToInt();
			return (valueInt & flagInt) != 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasNotIntFlagIntersection<T>(this T value, T flag) where T : unmanaged, Enum
		{
			return HasNotIntFlagIntersection(value.ToInt(), flag);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasNotIntFlagIntersection<T>(this int valueInt, T flag) where T : unmanaged, Enum
		{
			var flagInt = flag.ToInt();
			return (valueInt & flagInt) == 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T AddIntFlag<T>(this T value, T flag) where T : unmanaged, Enum
		{
			var valueInt = value.ToInt();
			return AddIntFlag(valueInt, flag).ToEnum<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int AddIntFlag<T>(this int valueInt, T flag) where T : unmanaged, Enum
		{
			var flagInt = flag.ToInt();
			return flagInt | valueInt;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T RemoveIntFlag<T>(this T value, T flag) where T : unmanaged, Enum
		{
			var valueInt = value.ToInt();
			return RemoveIntFlag(valueInt, flag).ToEnum<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RemoveIntFlag<T>(this int valueInt, T flag) where T : unmanaged, Enum
		{
			var flagInt = flag.ToInt();
			return valueInt & ~flagInt;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int AddIntFlag_Interlocked_ReturnInt<T>(this ref int valueInt, T flag) where T : unmanaged, Enum
		{
			var flagInt = flag.ToInt();
#if NET5_0_OR_GREATER
			return Interlocked.Or(ref valueInt, flagInt);
#else
			return Or(ref valueInt, flagInt);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T AddIntFlag_Interlocked<T>(this ref int valueInt, T flag) where T : unmanaged, Enum
		{
			var flagInt = flag.ToInt();
#if NET5_0_OR_GREATER
			return Interlocked.Or(ref valueInt, flagInt).ToEnum<T>();
#else
			return Or(ref valueInt, flagInt).ToEnum<T>();
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Interlocked_RemoveIntFlag_ReturnInt<T>(this ref int valueInt, T flag)
			where T : unmanaged, Enum
		{
			var flagInt = flag.ToInt();
#if NET5_0_OR_GREATER
			return Interlocked.And(ref valueInt, ~flagInt);
#else
			return And(ref valueInt, ~flagInt);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Interlocked_RemoveIntFlag<T>(this ref int valueInt, T flag) where T : unmanaged, Enum
		{
			var flagInt = flag.ToInt();
#if NET5_0_OR_GREATER
			return Interlocked.And(ref valueInt, ~flagInt).ToEnum<T>();
#else
			return And(ref valueInt, ~flagInt).ToEnum<T>();
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Interlocked_CompareExchangeIntEnum<T>(this ref int location1, T value, T comparand)
			where T : unmanaged, Enum
		{
			return Interlocked.CompareExchange(ref location1, value.ToInt(), comparand.ToInt()).ToEnum<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Interlocked_CompareExchangeIntEnum_ReturnInt<T>(this ref int location1, T value, T comparand)
			where T : unmanaged, Enum
		{
			return Interlocked.CompareExchange(ref location1, value.ToInt(), comparand.ToInt());
		}

#if !NET5_0_OR_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int And(ref int location1, int value)
		{
			var current = location1;
			while (true)
			{
				var newValue = current & value;
				var oldValue = Interlocked.CompareExchange(ref location1, newValue, current);
				if (oldValue == current)
				{
					return oldValue;
				}

				current = oldValue;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Or(ref int location1, int value)
		{
			var current = location1;
			while (true)
			{
				var newValue = current | value;
				var oldValue = Interlocked.CompareExchange(ref location1, newValue, current);
				if (oldValue == current)
				{
					return oldValue;
				}

				current = oldValue;
			}
		}
#endif
	}
}
