using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sapientia.Extensions
{
	/// <summary>
	/// https://www.notion.so/Extension-b985410501c742dabb3a08ca171a319c?pvs=4#475c07dd42b6492fbfe02c809358689a
	/// </summary>
#if UNITY_5_3_OR_NEWER
	[Unity.Burst.BurstCompile]
#endif
	public static partial class IntMathExt
	{
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
			return MathF.Sqrt(v);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsPowerOfTwo(this int v)
		{
			return (v & (v - 1)) == 0;
		}

		/// <summary>
		/// Подсчитывает количество ведущих нулей в двоичном представлении числа, начиная со старшего бита.
		/// Функция измеряет количество нулей, начиная с самого старшего бита числа, до первого встреченного бита, равного единице.
		/// Например lzcnt(00010010) = 3
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int LeadingZeroCount(this uint x)
		{
			// Copy of Unity math.lzcnt
			if (x == 0)
				return 32;

			LongDoubleUnion u;
			u.doubleValue = 0.0;
			u.longValue = 0x4330000000000000L + x;
			u.doubleValue -= 4503599627370496.0;
			return 0x41E - (int)(u.longValue >> 52);
		}

		/// <summary>
		/// Подсчитывает количество ведущих нулей в двоичном представлении числа, начиная со старшего бита.
		/// Функция измеряет количество нулей, начиная с самого старшего бита числа, до первого встреченного бита, равного единице.
		/// Например lzcnt(00010010) = 3
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int LeadingZeroCount(this ulong x)
		{
			// Copy of Unity math.lzcnt
			if (x == 0)
				return 64;

			var xh = (uint)(x >> 32);
			var bits = xh != 0 ? xh : (uint)x;
			var offset = xh != 0 ? 0x41E : 0x43E;

			LongDoubleUnion u;
			u.doubleValue = 0.0;
			u.longValue = 0x4330000000000000L + bits;
			u.doubleValue -= 4503599627370496.0;
			return offset - (int)(u.longValue >> 52);
		}

		/// <summary>
		/// Подсчитывает количество последовательных нулевых бит в двоичном представлении числа, начиная с младшего бита.
		/// Функция измеряет количество нулей, начиная с самого младшего бита числа, до первого встреченного бита, равного единице.
		/// Например tzcnt(01001000) = 3
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int TrailingZeroCount(this uint x)
		{
			// Copy of Unity math.tzcnt
			if (x == 0)
				return 32;

			x &= (uint)-x;
			LongDoubleUnion u;
			u.doubleValue = 0.0;
			u.longValue = 0x4330000000000000L + x;
			u.doubleValue -= 4503599627370496.0;
			return (int)(u.longValue >> 52) - 0x3FF;
		}

		/// <summary>
		/// Подсчитывает количество последовательных нулевых бит в двоичном представлении числа, начиная с младшего бита.
		/// Функция измеряет количество нулей, начиная с самого младшего бита числа, до первого встреченного бита, равного единице.
		/// Например tzcnt(01001000) = 3
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int TrailingZeroCount(this ulong x)
		{
			// Copy of Unity math.tzcnt
			if (x == 0)
				return 64;

			x = x & (ulong)-(long)x;
			var xl = (uint)x;

			var bits = xl != 0 ? xl : (uint)(x >> 32);
			var offset = xl != 0 ? 0x3FF : 0x3DF;

			LongDoubleUnion u;
			u.doubleValue = 0.0;
			u.longValue = 0x4330000000000000L + bits;
			u.doubleValue -= 4503599627370496.0;
			return (int)(u.longValue >> 52) - offset;
		}

		/// <summary>Returns number of 1-bits in the binary representation of a uint value. Also known as the Hamming weight, popcnt on x86, and vcnt on ARM.</summary>
		/// <param name="x">Number in which to count bits.</param>
		/// <returns>Number of bits set to 1 within x.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CountBits(this uint x)
		{
			// Copy of Unity math.countbits
			x = x - ((x >> 1) & 0x55555555);
			x = (x & 0x33333333) + ((x >> 2) & 0x33333333);
			return (int)((((x + (x >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24);
		}

		/// <summary>Returns number of 1-bits in the binary representation of a ulong value. Also known as the Hamming weight, popcnt on x86, and vcnt on ARM.</summary>
		/// <param name="x">Number in which to count bits.</param>
		/// <returns>Number of bits set to 1 within x.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CountBits(this ulong x)
		{
			// Copy of Unity math.countbits
			x = x - ((x >> 1) & 0x5555555555555555);
			x = (x & 0x3333333333333333) + ((x >> 2) & 0x3333333333333333);
			return (int)((((x + (x >> 4)) & 0x0F0F0F0F0F0F0F0F) * 0x0101010101010101) >> 56);
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
