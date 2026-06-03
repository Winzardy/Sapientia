using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Data;

namespace Sapientia.Extensions
{
	public static class BitExt
	{
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
		/// Подсчитывает количество ведущих нулей в двоичном представлении числа, начиная со старшего бита.
		/// Функция измеряет количество нулей, начиная с самого старшего бита числа, до первого встреченного бита, равного единице.
		/// Например lzcnt(00010010) = 3
		/// </summary>
		public static int LeadingZeroCount(this byte value)
		{
			return ((uint)value).LeadingZeroCount() - 24;
		}

		public static int LeadingZeroCount(this ushort value)
		{
			return ((uint)value).LeadingZeroCount() - 16;
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

		public static int TrailingZeroCount(this byte value)
		{
			return ((uint)value).TrailingZeroCount().Min(8);
		}

		public static int TrailingZeroCount(this ushort value)
		{
			return ((uint)value).TrailingZeroCount().Min(16);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int TruncateBits(this int value, int n)
		{
			E.ASSERT(n is > 0 and <= IntMathExt.BitsCount);

			var mask = (1 << n) - 1;
			return value & mask;
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

		public static int AlignDown(this int value, int alignPow2)
		{
			return value & ~(alignPow2 - 1);
		}

		public static int AlignUp(this int value, int alignPow2)
		{
			return AlignDown(value + alignPow2 - 1, alignPow2);
		}

		public static int ToInt(this bool value)
		{
			return value ? 1 : 0;
		}

		// 32-bit uint

		public static uint ExtractBits(this uint input, int pos, uint mask)
		{
			var tmp0 = input >> pos;
			return tmp0 & mask;
		}

		public static uint ReplaceBits(this uint input, int pos, uint mask, uint value)
		{
			var tmp0 = (value & mask) << pos;
			var tmp1 = input & ~(mask << pos);
			return tmp0 | tmp1;
		}

		public static uint SetBits(this uint input, int pos, uint mask, bool value)
		{
			return ReplaceBits(input, pos, mask, (uint)-value.ToInt());
		}

		// 64-bit ulong

		public static ulong ExtractBits(this ulong input, int pos, ulong mask)
		{
			var tmp0 = input >> pos;
			return tmp0 & mask;
		}

		public static ulong ReplaceBits(this ulong input, int pos, ulong mask, ulong value)
		{
			var tmp0 = (value & mask) << pos;
			var tmp1 = input & ~(mask << pos);
			return tmp0 | tmp1;
		}

		public static ulong SetBits(this ulong input, int pos, ulong mask, bool value)
		{
			return ReplaceBits(input, pos, mask, (ulong)-(long)value.ToInt());
		}

		public static int FindUlong(SafePtr<ulong> ptr, int beginBit, int endBit, int numBits)
		{
			var bits = ptr;
			var numBitsPerStep = 64;

			for (int i = beginBit / numBitsPerStep, end = AlignUp(endBit, numBitsPerStep) / numBitsPerStep; i < end; ++i)
			{
				if (bits[i] != 0)
				{
					continue;
				}

				var idx = i * numBitsPerStep;
				var num = endBit.Min(idx + numBitsPerStep) - idx;

				if (idx != beginBit)
				{
					var test = bits[idx / numBitsPerStep - 1];
					var newIdx = beginBit.Max(idx - test.LeadingZeroCount());

					num += idx - newIdx;
					idx = newIdx;
				}

				for (++i; i < end; ++i)
				{
					if (num >= numBits)
					{
						return idx;
					}

					var test = bits[i];
					var pos = i * numBitsPerStep;
					num += endBit.Min(pos + test.TrailingZeroCount()) - pos;

					if (test != 0)
					{
						break;
					}
				}

				if (num >= numBits)
				{
					return idx;
				}
			}

			return endBit;
		}

		public static int FindUint(SafePtr<ulong> ptr, int beginBit, int endBit, int numBits)
		{
			var bits = ptr.Cast<uint>();
			var numBitsPerStep = 32;

			for (int i = beginBit / numBitsPerStep, end = AlignUp(endBit, numBitsPerStep) / numBitsPerStep; i < end; ++i)
			{
				if (bits[i] != 0)
				{
					continue;
				}

				var idx = i * numBitsPerStep;
				var num = endBit.Min(idx + numBitsPerStep) - idx;

				if (idx != beginBit)
				{
					var test = bits[idx / numBitsPerStep - 1];
					var newIdx = beginBit.Max(idx - test.LeadingZeroCount());

					num += idx - newIdx;
					idx = newIdx;
				}

				for (++i; i < end; ++i)
				{
					if (num >= numBits)
					{
						return idx;
					}

					var test = bits[i];
					var pos = i * numBitsPerStep;
					num += endBit.Min(pos + test.TrailingZeroCount()) - pos;

					if (test != 0)
					{
						break;
					}
				}

				if (num >= numBits)
				{
					return idx;
				}
			}

			return endBit;
		}

		public static int FindUshort(SafePtr<ulong> ptr, int beginBit, int endBit, int numBits)
		{
			var bits = ptr.Cast<ushort>();
			var numBitsPerStep = 16;

			for (int i = beginBit / numBitsPerStep, end = AlignUp(endBit, numBitsPerStep) / numBitsPerStep; i < end; ++i)
			{
				if (bits[i] != 0)
				{
					continue;
				}

				var idx = i * numBitsPerStep;
				var num = endBit.Max(idx + numBitsPerStep) - idx;

				if (idx != beginBit)
				{
					var test = bits[idx / numBitsPerStep - 1];
					var newIdx = beginBit.Max(idx - test.LeadingZeroCount());

					num += idx - newIdx;
					idx = newIdx;
				}

				for (++i; i < end; ++i)
				{
					if (num >= numBits)
					{
						return idx;
					}

					var test = bits[i];
					var pos = i * numBitsPerStep;
					num += endBit.Min(pos + test.TrailingZeroCount()) - pos;

					if (test != 0)
					{
						break;
					}
				}

				if (num >= numBits)
				{
					return idx;
				}
			}

			return endBit;
		}

		public static int FindByte(SafePtr<ulong> ptr, int beginBit, int endBit, int numBits)
		{
			var bits = ptr.Cast<byte>();
			var numBitsPerStep = 8;

			for (int i = beginBit / numBitsPerStep, end = AlignUp(endBit, numBitsPerStep) / numBitsPerStep; i < end; ++i)
			{
				if (bits[i] != 0)
				{
					continue;
				}

				var idx = i * numBitsPerStep;
				var num = endBit.Min(idx + numBitsPerStep) - idx;

				if (idx != beginBit)
				{
					var test = bits[idx / numBitsPerStep - 1];
					var newIdx = beginBit.Max(idx - test.LeadingZeroCount());

					num += idx - newIdx;
					idx = newIdx;
				}

				for (++i; i < end; ++i)
				{
					if (num >= numBits)
					{
						return idx;
					}

					var test = bits[i];
					var pos = i * numBitsPerStep;
					num += endBit.Min(pos + test.TrailingZeroCount()) - pos;

					if (test != 0)
					{
						break;
					}
				}

				if (num >= numBits)
				{
					return idx;
				}
			}

			return endBit;
		}

		public static int FindUpto14Bits(SafePtr<ulong> ptr, int beginBit, int endBit, int numBits)
		{
			var bits = ptr.Cast<byte>();

			var bit = (byte)(beginBit & 7);
			var beginMask = (byte)~(0xff << bit);

			var lz = 0;
			for (int begin = beginBit / 8, end = AlignUp(endBit, 8) / 8, i = begin; i < end; ++i)
			{
				var test = bits[i];
				test |= i == begin ? beginMask : (byte)0;

				if (test == 0xff)
				{
					continue;
				}

				var pos = i * 8;
				var tz = endBit.Min(pos + test.TrailingZeroCount()) - pos;

				if (lz + tz >= numBits)
				{
					return pos - lz;
				}

				lz = test.LeadingZeroCount();

				var idx = pos + 8;
				var newIdx = beginBit.Max(idx - lz);
				lz = endBit.Min(idx) - newIdx;

				if (lz >= numBits)
				{
					return newIdx;
				}
			}

			return endBit;
		}

		public static int FindUpto6Bits(SafePtr<ulong> ptr, int beginBit, int endBit, int numBits)
		{
			var bits = ptr.Cast<byte>();

			byte beginMask = (byte)~(0xff << (beginBit & 7));
			byte endMask = (byte)~(0xff >> ((8 - (endBit & 7) & 7)));

			var mask = 1 << numBits - 1;

			for (int begin = beginBit / 8, end = AlignUp(endBit, 8) / 8, i = begin; i < end; ++i)
			{
				var test = bits[i];
				test |= i == begin ? beginMask : (byte)0;
				test |= i == end - 1 ? endMask : (byte)0;

				if (test == 0xff)
				{
					continue;
				}

				for (int pos = i * 8, posEnd = pos + 7; pos < posEnd; ++pos)
				{
					var tz = ((byte)(test ^ 0xff)).TrailingZeroCount();
					test >>= tz;

					pos += tz;

					if ((test & mask) == 0)
					{
						return pos;
					}

					test >>= 1;
				}
			}

			return endBit;
		}

		public static int FindWithBeginEnd(SafePtr<ulong> ptr, int beginBit, int endBit, int numBits)
		{
			int idx;

			if (numBits >= 127)
			{
				idx = FindUlong(ptr, beginBit, endBit, numBits);
				if (idx != endBit)
				{
					return idx;
				}
			}

			if (numBits >= 63)
			{
				idx = FindUint(ptr, beginBit, endBit, numBits);
				if (idx != endBit)
				{
					return idx;
				}
			}

			if (numBits >= 128)
			{
				// early out - no smaller step will find this gap
				return int.MaxValue;
			}

			if (numBits >= 31)
			{
				idx = FindUshort(ptr, beginBit, endBit, numBits);
				if (idx != endBit)
				{
					return idx;
				}
			}

			if (numBits >= 64)
			{
				// early out - no smaller step will find this gap
				return int.MaxValue;
			}

			idx = FindByte(ptr, beginBit, endBit, numBits);
			if (idx != endBit)
			{
				return idx;
			}

			if (numBits < 15)
			{
				idx = FindUpto14Bits(ptr, beginBit, endBit, numBits);

				if (idx != endBit)
				{
					return idx;
				}

				if (numBits < 7)
				{
					// The worst case scenario when every byte boundary bit is set (pattern 0x81),
					// and we're looking for 6 or less bits. It will rescan byte-by-byte to find
					// any inner byte gap.
					idx = FindUpto6Bits(ptr, beginBit, endBit, numBits);

					if (idx != endBit)
					{
						return idx;
					}
				}
			}

			return int.MaxValue;
		}

		public static int Find(SafePtr<ulong> ptr, int pos, int count, int numBits) => FindWithBeginEnd(ptr, pos, pos + count, numBits);

		public static bool TestNone(SafePtr<ulong> ptr, int length, int pos, int numBits = 1)
		{
			var end = length.Min(pos + numBits);
			var idxB = pos >> 6;
			var shiftB = pos & 0x3f;
			var idxE = (end - 1) >> 6;
			var shiftE = end & 0x3f;
			var maskB = 0xfffffffffffffffful << shiftB;
			var maskE = 0xfffffffffffffffful >> (64 - shiftE);

			if (idxB == idxE)
			{
				var mask = maskB & maskE;
				return 0ul == (ptr[idxB] & mask);
			}

			if (0ul != (ptr[idxB] & maskB))
			{
				return false;
			}

			for (var idx = idxB + 1; idx < idxE; ++idx)
			{
				if (0ul != ptr[idx])
				{
					return false;
				}
			}

			return 0ul == (ptr[idxE] & maskE);
		}

		public static bool TestAny(SafePtr<ulong> ptr, int length, int pos, int numBits = 1)
		{
			var end = length.Min(pos + numBits);
			var idxB = pos >> 6;
			var shiftB = pos & 0x3f;
			var idxE = (end - 1) >> 6;
			var shiftE = end & 0x3f;
			var maskB = 0xfffffffffffffffful << shiftB;
			var maskE = 0xfffffffffffffffful >> (64 - shiftE);

			if (idxB == idxE)
			{
				var mask = maskB & maskE;
				return 0ul != (ptr[idxB] & mask);
			}

			if (0ul != (ptr[idxB] & maskB))
			{
				return true;
			}

			for (var idx = idxB + 1; idx < idxE; ++idx)
			{
				if (0ul != ptr[idx])
				{
					return true;
				}
			}

			return 0ul != (ptr[idxE] & maskE);
		}

		public static bool TestAll(SafePtr<ulong> ptr, int length, int pos, int numBits = 1)
		{
			var end = length.Min(pos + numBits);
			var idxB = pos >> 6;
			var shiftB = pos & 0x3f;
			var idxE = (end - 1) >> 6;
			var shiftE = end & 0x3f;
			var maskB = 0xfffffffffffffffful << shiftB;
			var maskE = 0xfffffffffffffffful >> (64 - shiftE);

			if (idxB == idxE)
			{
				var mask = maskB & maskE;
				return mask == (ptr[idxB] & mask);
			}

			if (maskB != (ptr[idxB] & maskB))
			{
				return false;
			}

			for (var idx = idxB + 1; idx < idxE; ++idx)
			{
				if (0xfffffffffffffffful != ptr[idx])
				{
					return false;
				}
			}

			return maskE == (ptr[idxE] & maskE);
		}

		public static int CountBits(SafePtr<ulong> ptr, int length, int pos, int numBits = 1)
		{
			var end = length.Min(pos + numBits);
			var idxB = pos >> 6;
			var shiftB = pos & 0x3f;
			var idxE = (end - 1) >> 6;
			var shiftE = end & 0x3f;
			var maskB = 0xfffffffffffffffful << shiftB;
			var maskE = 0xfffffffffffffffful >> (64 - shiftE);

			if (idxB == idxE)
			{
				var mask = maskB & maskE;
				return CountBits(ptr[idxB] & mask);
			}

			var count = CountBits(ptr[idxB] & maskB);

			for (var idx = idxB + 1; idx < idxE; ++idx)
			{
				count += CountBits(ptr[idx]);
			}

			count += CountBits(ptr[idxE] & maskE);

			return count;
		}

		public static bool IsSet(SafePtr<ulong> ptr, int pos)
		{
			var idx = pos >> 6;
			var shift = pos & 0x3f;
			var mask = 1ul << shift;
			return 0ul != (ptr[idx] & mask);
		}

		public static ulong GetBits(SafePtr<ulong> ptr, int length, int pos, int numBits = 1)
		{
			var idxB = pos >> 6;
			var shiftB = pos & 0x3f;

			if (shiftB + numBits <= 64)
			{
				var mask = 0xfffffffffffffffful >> (64 - numBits);
				return ExtractBits(ptr[idxB], shiftB, mask);
			}

			var end = length.Min(pos + numBits);
			var idxE = (end - 1) >> 6;
			var shiftE = end & 0x3f;

			var maskB = 0xfffffffffffffffful >> shiftB;
			var valueB = ExtractBits(ptr[idxB], shiftB, maskB);

			var maskE = 0xfffffffffffffffful >> (64 - shiftE);
			var valueE = ExtractBits(ptr[idxE], 0, maskE);

			return (valueE << (64 - shiftB)) | valueB;
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

	/// <summary>
	/// A 32-bit array of bits.
	/// </summary>
	/// <remarks>
	/// Stack allocated, so it does not require thread safety checks or disposal.
	/// </remarks>
	[DebuggerTypeProxy(typeof(BitField32.BitField32DebugView))]
	public struct BitField32
	{
		/// <summary>
		/// The 32 bits, stored as a uint.
		/// </summary>
		/// <value>The 32 bits, stored as a uint.</value>
		public uint value;

		/// <summary>
		/// Initializes and returns an instance of BitField32.
		/// </summary>
		/// <param name="initialValue">Initial value of the bit field. Default is 0.</param>
		public BitField32(uint initialValue = 0u)
		{
			value = initialValue;
		}

		/// <summary>
		/// Clears all the bits to 0.
		/// </summary>
		public void Clear()
		{
			value = 0u;
		}

		/// <summary>
		/// Sets a single bit to 1 or 0.
		/// </summary>
		/// <param name="pos">Position in this bit field to set (must be 0-31).</param>
		/// <param name="value">If true, sets the bit to 1. If false, sets the bit to 0.</param>
		/// <exception cref="ArgumentException">Thrown if `pos`is out of range.</exception>
		public void SetBits(int pos, bool value)
		{
			CheckArgs(pos, 1);
			this.value = this.value.SetBits(pos, 1, value);
		}

		/// <summary>
		/// Sets one or more contiguous bits to 1 or 0.
		/// </summary>
		/// <param name="pos">Position in the bit field of the first bit to set (must be 0-31).</param>
		/// <param name="value">If true, sets the bits to 1. If false, sets the bits to 0.</param>
		/// <param name="numBits">Number of bits to set (must be 1-32).</param>
		/// <exception cref="ArgumentException">Thrown if `pos` or `numBits` are out of bounds or if `pos + numBits` exceeds 32.</exception>
		public void SetBits(int pos, bool value, int numBits)
		{
			CheckArgs(pos, numBits);
			var mask = 0xffffffffu >> (32 - numBits);
			this.value = this.value.SetBits(pos, mask, value);
		}

		/// <summary>
		/// Returns one or more contiguous bits from the bit field as the lower bits of a uint.
		/// </summary>
		/// <param name="pos">Position in the bit field of the first bit to get (must be 0-31).</param>
		/// <param name="numBits">Number of bits to get (must be 1-32).</param>
		/// <exception cref="ArgumentException">Thrown if `pos` or `numBits` are out of bounds or if `pos + numBits` exceeds 32.</exception>
		/// <returns>The requested range of bits from the bit field stored in the least-significant bits of a uint. All other bits of the uint will be 0.</returns>
		public uint GetBits(int pos, int numBits = 1)
		{
			CheckArgs(pos, numBits);
			var mask = 0xffffffffu >> (32 - numBits);
			return value.ExtractBits(pos, mask);
		}

		/// <summary>
		/// Returns true if the bit at a position is 1.
		/// </summary>
		/// <param name="pos">Position in the bit field (must be 0-31).</param>
		/// <returns>True if the bit at the position is 1.</returns>
		public bool IsSet(int pos)
		{
			return 0 != GetBits(pos);
		}

		/// <summary>
		/// Returns true if none of the bits in a contiguous range are 1.
		/// </summary>
		/// <param name="pos">Position in the bit field (must be 0-31).</param>
		/// <param name="numBits">Number of bits to test (must be 1-32).</param>
		/// <exception cref="ArgumentException">Thrown if `pos` or `numBits` are out of bounds or if `pos + numBits` exceeds 32.</exception>
		/// <returns>True if none of the bits in the contiguous range are 1.</returns>
		public bool TestNone(int pos, int numBits = 1)
		{
			return 0u == GetBits(pos, numBits);
		}

		/// <summary>
		/// Returns true if any of the bits in a contiguous range are 1.
		/// </summary>
		/// <param name="pos">Position in the bit field (must be 0-31).</param>
		/// <param name="numBits">Number of bits to test (must be 1-32).</param>
		/// <exception cref="ArgumentException">Thrown if `pos` or `numBits` are out of bounds or if `pos + numBits` exceeds 32.</exception>
		/// <returns>True if at least one bit in the contiguous range is 1.</returns>
		public bool TestAny(int pos, int numBits = 1)
		{
			return 0u != GetBits(pos, numBits);
		}

		/// <summary>
		/// Returns true if all of the bits in a contiguous range are 1.
		/// </summary>
		/// <param name="pos">Position in the bit field (must be 0-31).</param>
		/// <param name="numBits">Number of bits to test (must be 1-32).</param>
		/// <exception cref="ArgumentException">Thrown if `pos` or `numBits` are out of bounds or if `pos + numBits` exceeds 32.</exception>
		/// <returns>True if all bits in the contiguous range are 1.</returns>
		public bool TestAll(int pos, int numBits = 1)
		{
			CheckArgs(pos, numBits);
			var mask = 0xffffffffu >> (32 - numBits);
			return mask == value.ExtractBits(pos, mask);
		}

		/// <summary>
		/// Returns the number of bits that are 1.
		/// </summary>
		/// <returns>The number of bits that are 1.</returns>
		public int CountBits()
		{
			return value.CountBits();
		}

		/// <summary>
		/// Returns the number of leading zeroes.
		/// </summary>
		/// <returns>The number of leading zeros.</returns>
		public int CountLeadingZeros()
		{
			return value.LeadingZeroCount();
		}

		/// <summary>
		/// Returns the number of trailing zeros.
		/// </summary>
		/// <returns>The number of trailing zeros.</returns>
		public int CountTrailingZeros()
		{
			return value.TrailingZeroCount();
		}

		[Conditional(E.DEBUG)]
		private static void CheckArgs(int pos, int numBits)
		{
			if (pos > 31
				|| numBits == 0
				|| numBits > 32
				|| pos + numBits > 32)
			{
				throw new ArgumentException($"BitField32 invalid arguments: pos {pos} (must be 0-31), numBits {numBits} (must be 1-32).");
			}
		}

		private sealed class BitField32DebugView
		{
			private BitField32 _bitField;

			public BitField32DebugView(BitField32 bitfield)
			{
				_bitField = bitfield;
			}

			public bool[] Bits
			{
				get
				{
					var array = new bool[32];
					for (var i = 0; i < 32; ++i)
					{
						array[i] = _bitField.IsSet(i);
					}

					return array;
				}
			}
		}
	}

	/// <summary>
	/// A 64-bit array of bits.
	/// </summary>
	/// <remarks>
	/// Stack allocated, so it does not require thread safety checks or disposal.
	/// </remarks>
	[DebuggerTypeProxy(typeof(BitField64DebugView))]
	public struct BitField64
	{
		/// <summary>
		/// The 64 bits, stored as a ulong.
		/// </summary>
		/// <value>The 64 bits, stored as a uint.</value>
		public ulong value;

		/// <summary>
		/// Initializes and returns an instance of BitField64.
		/// </summary>
		/// <param name="initialValue">Initial value of the bit field. Default is 0.</param>
		public BitField64(ulong initialValue = 0ul)
		{
			value = initialValue;
		}

		/// <summary>
		/// Clears all bits to 0.
		/// </summary>
		public void Clear()
		{
			value = 0ul;
		}

		/// <summary>
		/// Sets a single bit to 1 or 0.
		/// </summary>
		/// <param name="pos">Position in this bit field to set (must be 0-63).</param>
		/// <param name="value">If true, sets the bit to 1. If false, sets the bit to 0.</param>
		/// <exception cref="ArgumentException">Thrown if `pos`is out of range.</exception>
		public void SetBits(int pos, bool value)
		{
			CheckArgs(pos, 1);
			this.value = this.value.SetBits(pos, 1, value);
		}


		/// <summary>
		/// Sets one or more contiguous bits to 1 or 0.
		/// </summary>
		/// <param name="pos">Position in the bit field of the first bit to set (must be 0-63).</param>
		/// <param name="value">If true, sets the bits to 1. If false, sets the bits to 0.</param>
		/// <param name="numBits">Number of bits to set (must be 1-64).</param>
		/// <exception cref="ArgumentException">Thrown if `pos` or `numBits` are out of bounds or if `pos + numBits` exceeds 64.</exception>
		public void SetBits(int pos, bool value, int numBits = 1)
		{
			CheckArgs(pos, numBits);
			var mask = 0xfffffffffffffffful >> (64 - numBits);
			this.value = this.value.SetBits(pos, mask, value);
		}

		/// <summary>
		/// Returns one or more contiguous bits from the bit field as the lower bits of a ulong.
		/// </summary>
		/// <param name="pos">Position in the bit field of the first bit to get (must be 0-63).</param>
		/// <param name="numBits">Number of bits to get (must be 1-64).</param>
		/// <exception cref="ArgumentException">Thrown if `pos` or `numBits` are out of bounds or if `pos + numBits` exceeds 64.</exception>
		/// <returns>The requested range of bits from the bit field stored in the least-significant bits of a ulong. All other bits of the ulong will be 0.</returns>
		public ulong GetBits(int pos, int numBits = 1)
		{
			CheckArgs(pos, numBits);
			var mask = 0xfffffffffffffffful >> (64 - numBits);
			return value.ExtractBits(pos, mask);
		}

		/// <summary>
		/// Returns true if the bit at a position is 1.
		/// </summary>
		/// <param name="pos">Position in the bit field (must be 0-63).</param>
		/// <returns>True if the bit at the position is 1.</returns>
		public bool IsSet(int pos)
		{
			return 0ul != GetBits(pos);
		}

		/// <summary>
		/// Returns true if none of the bits in a contiguous range are 1.
		/// </summary>
		/// <param name="pos">Position in the bit field (must be 0-63).</param>
		/// <param name="numBits">Number of bits to test (must be 1-64).</param>
		/// <exception cref="ArgumentException">Thrown if `pos` or `numBits` are out of bounds or if `pos + numBits` exceeds 64.</exception>
		/// <returns>True if none of the bits in the contiguous range are 1.</returns>
		public bool TestNone(int pos, int numBits = 1)
		{
			return 0ul == GetBits(pos, numBits);
		}

		/// <summary>
		/// Returns true if any of the bits in a contiguous range are 1.
		/// </summary>
		/// <param name="pos">Position in the bit field (must be 0-63).</param>
		/// <param name="numBits">Number of bits to test (must be 1-64).</param>
		/// <exception cref="ArgumentException">Thrown if `pos` or `numBits` are out of bounds or if `pos + numBits` exceeds 64.</exception>
		/// <returns>True if at least one bit in the contiguous range is 1.</returns>
		public bool TestAny(int pos, int numBits = 1)
		{
			return 0ul != GetBits(pos, numBits);
		}

		/// <summary>
		/// Returns true if all of the bits in a contiguous range are 1.
		/// </summary>
		/// <param name="pos">Position in the bit field (must be 0-63).</param>
		/// <param name="numBits">Number of bits to test (must be 1-64).</param>
		/// <exception cref="ArgumentException">Thrown if `pos` or `numBits` are out of bounds or if `pos + numBits` exceeds 64.</exception>
		/// <returns>True if all bits in the contiguous range are 1.</returns>
		public bool TestAll(int pos, int numBits = 1)
		{
			CheckArgs(pos, numBits);
			var mask = 0xfffffffffffffffful >> (64 - numBits);
			return mask == value.ExtractBits(pos, mask);
		}

		/// <summary>
		/// Returns the number of bits that are 1.
		/// </summary>
		/// <returns>The number of bits that are 1.</returns>
		public int CountBits()
		{
			return value.CountBits();
		}

		/// <summary>
		/// Returns the number of leading zeroes.
		/// </summary>
		/// <returns>The number of leading zeros.</returns>
		public int CountLeadingZeros()
		{
			return value.LeadingZeroCount();
		}

		/// <summary>
		/// Returns the number of trailing zeros.
		/// </summary>
		/// <returns>The number of trailing zeros.</returns>
		public int CountTrailingZeros()
		{
			return value.TrailingZeroCount();
		}

		[Conditional(E.DEBUG)]
		private static void CheckArgs(int pos, int numBits)
		{
			if (pos > 63
				|| numBits == 0
				|| numBits > 64
				|| pos + numBits > 64)
			{
				throw new ArgumentException($"BitField32 invalid arguments: pos {pos} (must be 0-63), numBits {numBits} (must be 1-64).");
			}
		}

		private sealed class BitField64DebugView
		{
			private BitField64 _bitField;

			public BitField64DebugView(BitField64 bitField)
			{
				_bitField = bitField;
			}

			public bool[] Bits
			{
				get
				{
					var array = new bool[64];
					for (int i = 0; i < 64; ++i)
					{
						array[i] = _bitField.IsSet(i);
					}
					return array;
				}
			}
		}
	}
}
