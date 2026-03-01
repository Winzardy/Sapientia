using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sapientia.Extensions
{
	/// <summary>
	/// https://www.notion.so/Extension-b985410501c742dabb3a08ca171a319c?pvs=4#475c07dd42b6492fbfe02c809358689a
	/// </summary>
	public static partial class IntMathExt
	{
		public const int BitsCount = 32;
		public const int FIRST_TO_LAST_SHIFT = 32 - 1;

		public static int DivRem(this int value, int divider, out int remainder)
		{
			remainder = value % divider;
			return value / divider;
		}

		public static int Log2(this int value)
		{
			var result = 0;
			while ((value >>= 1) != 0)
				result++;
			return result;
		}

		public static int NextPowerOfTwo(this int n)
		{
			--n;
			n |= n >> 1;
			n |= n >> 2;
			n |= n >> 4;
			n |= n >> 8;
			n |= n >> 16;
			++n;
			return n;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Clamp(this int value, int min, int max)
		{
			return value < min ? min : (value > max ? max : value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Abs(this int value)
		{
			var mask = value >> FIRST_TO_LAST_SHIFT;
			return (value + mask) ^ mask;
		}

		// ~5% faster: value >= 0 ? 1 : -1;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Sign(this int value)
		{
			return ((value & int.MinValue) >> FIRST_TO_LAST_SHIFT) | 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEven(this int value)
		{
			return (value & 1) == 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsOdd(this int value)
		{
			return (value & 1) == 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Pow(this int value, int power)
		{
			return (int)Math.Pow(value, power);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort Max(this ushort a, ushort b)
		{
			return a > b ? a : b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort Max(this ushort a, int b)
		{
			return a > b ? a : (ushort)b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Max(this uint a, uint b)
		{
			return a > b ? a : b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Max(this int a, int b, out bool aMoreEqual)
		{
			aMoreEqual = a >= b;
			return aMoreEqual ? a : b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Max(this int a, int b)
		{
			return a > b ? a : b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Min(this int a, int b, out bool aLessEqual)
		{
			aLessEqual = a <= b;
			return aLessEqual ? a : b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Min(this int a, int b)
		{
			return a < b ? a : b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Min(this uint a, uint b)
		{
			return a < b ? a : b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Sqrt(this int v)
		{
			return FloatMathExt.Sqrt(v);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsPowerOfTwo(this int v)
		{
			return (v & (v - 1)) == 0;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct IntFloatUnion
		{
			[FieldOffset(0)]
			public int intValue;
			[FieldOffset(0)]
			public float floatValue;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct LongDoubleUnion
		{
			[FieldOffset(0)]
			public long longValue;
			[FieldOffset(0)]
			public double doubleValue;
		}
	}
}
