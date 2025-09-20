using System;
using System.Runtime.CompilerServices;

namespace Sapientia.Deterministic
{
	/// <summary>
	/// Represents a Q31.32 fixed-point number.
	/// </summary>
	public partial struct Fix64
	{
		/// <summary>
		/// Adds x and y. Performs saturating addition, i.e. in case of overflow,
		/// rounds to MinValue or MaxValue depending on sign of operands.
		/// </summary>
		public static Fix64 operator +(Fix64 x, Fix64 y)
		{
			var xl = x.m_rawValue;
			var yl = y.m_rawValue;
			var sum = xl + yl;
			// if signs of operands are equal and signs of sum and x are different
			if (((~(xl ^ yl) & (xl ^ sum)) & MIN_VALUE) != 0)
			{
				sum = xl > 0 ? MAX_VALUE : MIN_VALUE;
			}

			return new Fix64(sum);
		}

		/// <summary>
		/// Subtracts y from x. Performs saturating substraction, i.e. in case of overflow,
		/// rounds to MinValue or MaxValue depending on sign of operands.
		/// </summary>
		public static Fix64 operator -(Fix64 x, Fix64 y)
		{
			var xl = x.m_rawValue;
			var yl = y.m_rawValue;
			var diff = xl - yl;
			// if signs of operands are different and signs of sum and x are different
			if ((((xl ^ yl) & (xl ^ diff)) & MIN_VALUE) != 0)
			{
				diff = xl < 0 ? MIN_VALUE : MAX_VALUE;
			}

			return new Fix64(diff);
		}

		public static Fix64 operator *(Fix64 x, Fix64 y)
		{
			var xl = x.m_rawValue;
			var yl = y.m_rawValue;

			var xlo = (ulong) (xl & 0x00000000FFFFFFFF);
			var xhi = xl >> FRACTIONAL_PLACES;
			var ylo = (ulong) (yl & 0x00000000FFFFFFFF);
			var yhi = yl >> FRACTIONAL_PLACES;

			var lolo = xlo * ylo;
			var lohi = (long) xlo * yhi;
			var hilo = xhi * (long) ylo;
			var hihi = xhi * yhi;

			var loResult = lolo >> FRACTIONAL_PLACES;
			var midResult1 = lohi;
			var midResult2 = hilo;
			var hiResult = hihi << FRACTIONAL_PLACES;

			bool overflow = false;
			var sum = AddOverflowHelper((long) loResult, midResult1, ref overflow);
			sum = AddOverflowHelper(sum, midResult2, ref overflow);
			sum = AddOverflowHelper(sum, hiResult, ref overflow);

			bool opSignsEqual = ((xl ^ yl) & MIN_VALUE) == 0;

			// if signs of operands are equal and sign of result is negative,
			// then multiplication overflowed positively
			// the reverse is also true
			if (opSignsEqual)
			{
				if (sum < 0 || (overflow && xl > 0))
				{
					return MaxValue;
				}
			}
			else
			{
				if (sum > 0)
				{
					return MinValue;
				}
			}

			// if the top 32 bits of hihi (unused in the result) are neither all 0s or 1s,
			// then this means the result overflowed.
			var topCarry = hihi >> FRACTIONAL_PLACES;
			if (topCarry != 0 && topCarry != -1 /*&& xl != -17 && yl != -17*/)
			{
				return opSignsEqual ? MaxValue : MinValue;
			}

			// If signs differ, both operands' magnitudes are greater than 1,
			// and the result is greater than the negative operand, then there was negative overflow.
			if (!opSignsEqual)
			{
				long posOp, negOp;
				if (xl > yl)
				{
					posOp = xl;
					negOp = yl;
				}
				else
				{
					posOp = yl;
					negOp = xl;
				}

				if (sum > negOp && negOp < -ONE && posOp > ONE)
				{
					return MinValue;
				}
			}

			return new Fix64(sum);
		}

		public static Fix64 operator /(Fix64 x, Fix64 y)
		{
			var xl = x.m_rawValue;
			var yl = y.m_rawValue;

			if (yl == 0)
			{
				throw new DivideByZeroException();
			}

			var remainder = (ulong) (xl >= 0 ? xl : -xl);
			var divider = (ulong) (yl >= 0 ? yl : -yl);
			var quotient = 0UL;
			var bitPos = NUM_BITS / 2 + 1;

			// If the divider is divisible by 2^n, take advantage of it.
			while ((divider & 0xF) == 0 && bitPos >= 4)
			{
				divider >>= 4;
				bitPos -= 4;
			}

			while (remainder != 0 && bitPos >= 0)
			{
				int shift = CountLeadingZeroes(remainder);
				if (shift > bitPos)
				{
					shift = bitPos;
				}

				remainder <<= shift;
				bitPos -= shift;

				var div = remainder / divider;
				remainder = remainder % divider;
				quotient += div << bitPos;

				// Detect overflow
				if ((div & ~(0xFFFFFFFFFFFFFFFF >> bitPos)) != 0)
				{
					return ((xl ^ yl) & MIN_VALUE) == 0 ? MaxValue : MinValue;
				}

				remainder <<= 1;
				--bitPos;
			}

			// rounding
			++quotient;
			var result = (long) (quotient >> 1);
			if (((xl ^ yl) & MIN_VALUE) != 0)
			{
				result = -result;
			}

			return new Fix64(result);
		}

		public static Fix64 operator %(Fix64 x, Fix64 y)
		{
			return new Fix64(
				x.m_rawValue == MIN_VALUE & y.m_rawValue == -1 ? 0 : x.m_rawValue % y.m_rawValue);
		}

		public static Fix64 operator -(Fix64 x)
		{
			return x.m_rawValue == MIN_VALUE ? MaxValue : new Fix64(-x.m_rawValue);
		}

		public static bool operator ==(Fix64 x, Fix64 y)
		{
			return x.m_rawValue == y.m_rawValue;
		}

		public static bool operator !=(Fix64 x, Fix64 y)
		{
			return x.m_rawValue != y.m_rawValue;
		}

		public static bool operator >(Fix64 x, Fix64 y)
		{
			return x.m_rawValue > y.m_rawValue;
		}

		public static bool operator <(Fix64 x, Fix64 y)
		{
			return x.m_rawValue < y.m_rawValue;
		}

		public static bool operator >=(Fix64 x, Fix64 y)
		{
			return x.m_rawValue >= y.m_rawValue;
		}

		public static bool operator <=(Fix64 x, Fix64 y)
		{
			return x.m_rawValue <= y.m_rawValue;
		}

		private static long AddOverflowHelper(long x, long y, ref bool overflow)
		{
			var sum = x + y;
			// x + y overflows if sign(x) ^ sign(y) != sign(sum)
			overflow |= ((x ^ y ^ sum) & MIN_VALUE) != 0;
			return sum;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int CountLeadingZeroes(ulong x)
		{
			int result = 0;
			while ((x & 0xF000000000000000) == 0)
			{
				result += 4;
				x <<= 4;
			}

			while ((x & 0x8000000000000000) == 0)
			{
				result += 1;
				x <<= 1;
			}

			return result;
		}
	}
}
