using Sapientia.Data;
using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	internal unsafe struct Bitwise
	{
		private const int _ulongSize = sizeof(ulong);

		[INLINE(256)]
		public static int GetMinLength(int bitsCount1, int bitsCount2)
		{
			var length = GetLength(bitsCount1).Min(GetLength(bitsCount2));
			return length;
		}

		[INLINE(256)]
		public static int GetMaxLength(int bitsCount1, int bitsCount2)
		{
			var length = GetLength(bitsCount1).Max(GetLength(bitsCount2));
			return length;
		}

		[INLINE(256)]
		public static int GetLength(int bitsCount)
		{
			return AlignULongBits(bitsCount) / _ulongSize;
			//if (bitsCount > 0u && bitsCount < 64u) bitsCount = 64u;
			//return bitsCount / Bitwise.ULONG_SIZE;
		}

		[INLINE(256)]
		public static int AlignULongBits(int bitsCount)
		{
			var delta = bitsCount % 64;
			if (delta < 64 && delta > 0u)
				bitsCount += 64 - delta;
			return bitsCount / 8;
		}

		[INLINE(256)]
		internal static int AlignDown(int value, int alignPow2)
		{
			return value & ~(alignPow2 - 1);
		}

		[INLINE(256)]
		internal static int AlignUp(int value, int alignPow2)
		{
			return AlignDown(value + alignPow2 - 1, alignPow2);
		}

		[INLINE(256)]
		internal static int FromBool(bool value)
		{
			return value ? 1 : 0;
		}

		// 32-bit uint

		[INLINE(256)]
		internal static uint ExtractBits(uint input, int pos, uint mask)
		{
			var tmp0 = input >> pos;
			return tmp0 & mask;
		}

		[INLINE(256)]
		internal static uint ReplaceBits(uint input, int pos, uint mask, uint value)
		{
			var tmp0 = (value & mask) << pos;
			var tmp1 = input & ~(mask << pos);
			return tmp0 | tmp1;
		}

		[INLINE(256)]
		internal static uint SetBits(uint input, int pos, uint mask, bool value)
		{
			return ReplaceBits(input, pos, mask, (uint)-FromBool(value));
		}

		// 64-bit ulong

		[INLINE(256)]
		internal static ulong ExtractBits(ulong input, int pos, ulong mask)
		{
			var tmp0 = input >> pos;
			return tmp0 & mask;
		}

		[INLINE(256)]
		internal static ulong ReplaceBits(ulong input, int pos, ulong mask, ulong value)
		{
			var tmp0 = (value & mask) << pos;
			var tmp1 = input & ~(mask << pos);
			return tmp0 | tmp1;
		}

		[INLINE(256)]
		internal static ulong SetBits(ulong input, int pos, ulong mask, bool value)
		{
			return ReplaceBits(input, pos, mask, (ulong)-(long)FromBool(value));
		}

		[INLINE(256)]
		private static int LeadingZeroCount(byte value)
		{
			return ((uint)value).LeadingZeroCount() - 24;
		}

		[INLINE(256)]
		private static int LeadingZeroCount(ushort value)
		{
			return ((uint)value).LeadingZeroCount() - 16;
		}

		[INLINE(256)]
		private static int TrailingZeroCount(byte value)
		{
			return 8.Min(((uint)value).TrailingZeroCount());
		}

		[INLINE(256)]
		private static int TrailingZeroCount(ushort value)
		{
			return 16.Min(((uint)value).TrailingZeroCount());
		}

		[INLINE(256)]
		private static int FindUlong(ulong* ptr, int beginBit, int endBit, int numBits)
		{
			var bits = ptr;
			var numSteps = (numBits + 63) >> 6;
			var numBitsPerStep = 64;
			var maxBits = numSteps * numBitsPerStep;

			for (int i = beginBit / numBitsPerStep, end = AlignUp(endBit, numBitsPerStep) / numBitsPerStep;
			     i < end;
			     ++i)
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

		[INLINE(256)]
		private static int FindUint(ulong* ptr, int beginBit, int endBit, int numBits)
		{
			var bits = (uint*)ptr;
			var numSteps = (numBits + 31) >> 5;
			var numBitsPerStep = 32;
			var maxBits = numSteps * numBitsPerStep;

			for (int i = beginBit / numBitsPerStep, end = AlignUp(endBit, numBitsPerStep) / numBitsPerStep;
			     i < end;
			     ++i)
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

		[INLINE(256)]
		private static int FindUshort(ulong* ptr, int beginBit, int endBit, int numBits)
		{
			var bits = (ushort*)ptr;
			var numSteps = (numBits + 15) >> 4;
			var numBitsPerStep = 16;
			var maxBits = numSteps * numBitsPerStep;

			for (int i = beginBit / numBitsPerStep, end = AlignUp(endBit, numBitsPerStep) / numBitsPerStep;
			     i < end;
			     ++i)
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
					var newIdx = beginBit.Max(idx - LeadingZeroCount(test));

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
					num += endBit.Min(pos + TrailingZeroCount(test)) - pos;

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

		[INLINE(256)]
		private static int FindByte(ulong* ptr, int beginBit, int endBit, int numBits)
		{
			var bits = (byte*)ptr;
			var numSteps = (numBits + 7) >> 3;
			var numBitsPerStep = 8;
			var maxBits = numSteps * numBitsPerStep;

			for (int i = beginBit / numBitsPerStep, end = AlignUp(endBit, numBitsPerStep) / numBitsPerStep;
			     i < end;
			     ++i)
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
					var newIdx = beginBit.Max(idx - LeadingZeroCount(test));

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
					num += endBit.Min(pos + TrailingZeroCount(test)) - pos;

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

		[INLINE(256)]
		private static int FindUpto14Bits(ulong* ptr, int beginBit, int endBit, int numBits)
		{
			var bits = (byte*)ptr;

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
				var tz = endBit.Min(pos + TrailingZeroCount(test)) - pos;

				if (lz + tz >= numBits)
				{
					return pos - lz;
				}

				lz = LeadingZeroCount(test);

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

		[INLINE(256)]
		private static int FindUpto6Bits(ulong* ptr, int beginBit, int endBit, int numBits)
		{
			var bits = (byte*)ptr;

			var beginMask = (byte)~(0xff << (beginBit & 7));
			var endMask = (byte)~(0xff >> ((8 - (endBit & 7)) & 7));

			var mask = 1 << (numBits - 1);

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
					var tz = TrailingZeroCount((byte)(test ^ 0xff));
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

		[INLINE(256)]
		internal static int FindWithBeginEnd(ulong* ptr, int beginBit, int endBit, int numBits)
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

		[INLINE(256)]
		internal static int Find(ulong* ptr, int pos, int count, int numBits)
		{
			var v = FindWithBeginEnd(ptr, pos, pos + count, numBits);
			if (v == int.MaxValue) return -1;
			return v;
		}
	}

	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(UnsafeBitArrayDebugView))]
	public unsafe struct MemBitArray
	{
		/// <summary>
		/// Pointer to the data.
		/// </summary>
		/// <value>Pointer to the data.</value>
		//[NativeDisableUnsafePtrRestriction]
		//public ulong* Ptr;
		public MemPtr ptr;

		/// <summary>
		/// The number of bits.
		/// </summary>
		/// <value>The number of bits.</value>
		public int length;

		/// <summary>
		/// Initializes and returns an instance of UnsafeBitArray which aliases an existing buffer.
		/// </summary>
		/// <param name="ptr">An existing buffer.</param>
		/// <param name="allocator">The allocator that was used to allocate the bytes. Needed to dispose this array.</param>
		/// <param name="sizeInBytes">The number of bytes. The length will be `sizeInBytes * 8`.</param>
		[INLINE(256)]
		public MemBitArray(MemPtr ptr, int sizeInBytes)
		{
			this.ptr = ptr;
			length = sizeInBytes * 8;
		}

		/// <summary>
		/// Initializes and returns an instance of UnsafeBitArray.
		/// </summary>
		/// <param name="numBits">Number of bits.</param>
		/// <param name="allocator">The allocator to use.</param>
		/// <param name="options">Whether newly allocated bytes should be zeroed out.</param>
		[INLINE(256)]
		public MemBitArray(SafePtr<Allocator> allocator, int numBits, ClearOptions options = ClearOptions.ClearMemory)
		{
			var sizeInBytes = Bitwise.AlignUp(numBits, 64) / 8;
			ptr = allocator.Value().MemAlloc(sizeInBytes);
			length = numBits;

			if (options == ClearOptions.ClearMemory)
			{
				allocator.Value().MemClear(ptr, 0, sizeInBytes);
			}
		}

		[INLINE(256)]
		public void Resize(SafePtr<Allocator> allocator, int numBits)
		{
			if (numBits > length)
			{
				var newList = new MemBitArray(allocator, numBits, ClearOptions.ClearMemory);
				newList.Copy(allocator, 0, ref this, 0, length);
				Dispose(allocator);
				this = newList;
			}
		}

		/// <summary>
		/// Whether this array has been allocated (and not yet deallocated).
		/// </summary>
		/// <value>True if this array has been allocated (and not yet deallocated).</value>
		public bool IsCreated => ptr.IsCreated();

		/// <summary>
		/// Releases all resources (memory and safety handles).
		/// </summary>
		[INLINE(256)]
		public void Dispose(SafePtr<Allocator> allocator)
		{
			allocator.Value().MemFree(ptr);
			this = default;
		}

		/// <summary>
		/// Sets all the bits to 0.
		/// </summary>
		[INLINE(256)]
		public void Clear(SafePtr<Allocator> allocator)
		{
			var sizeInBytes = Bitwise.AlignUp(length, 64) / 8;
			allocator.Value().MemClear(ptr, 0, sizeInBytes);
		}

		/// <summary>
		/// Sets the bit at an index to 0 or 1.
		/// </summary>
		/// <param name="pos">Index of the bit to set.</param>
		/// <param name="value">True for 1, false for 0.</param>
		[INLINE(256)]
		public void Set(SafePtr<Allocator> allocator, int pos, bool value)
		{
			var ptr = allocator.Value().GetSafePtr<ulong>(in this.ptr);
			var idx = pos >> 6;
			var shift = pos & 0x3f;
			var mask = 1ul << shift;
			var bits = (ptr[idx] & ~mask) | ((ulong)-Bitwise.FromBool(value) & mask);
			/*ref var p = ref ptr[idx];
			ulong initialValue;
			ulong targetValue;
			do {
			    initialValue = p;
			    targetValue = bits;
			} while (initialValue != CompareExchange(ref p, targetValue, initialValue));*/
			ptr[idx] = bits;
		}

		[INLINE(256)]
		private static ulong CompareExchange(ref ulong target, ulong v, ulong cmp)
		{
			fixed (ulong* p = &target)
			{
				return (ulong)System.Threading.Interlocked.CompareExchange(ref *(long*)p, (long)v, (long)cmp);
			}
		}

		/// <summary>
		/// Sets a range of bits to 0 or 1.
		/// </summary>
		/// <remarks>
		/// The range runs from index `pos` up to (but not including) `pos + numBits`.
		/// No exception is thrown if `pos + numBits` exceeds the length.
		/// </remarks>
		/// <param name="pos">Index of the first bit to set.</param>
		/// <param name="value">True for 1, false for 0.</param>
		/// <param name="numBits">Number of bits to set.</param>
		/// <exception cref="System.ArgumentException">Thrown if pos is out of bounds or if numBits is less than 1.</exception>
		[INLINE(256)]
		public void SetBits(SafePtr<Allocator> allocator, int pos, bool value, int numBits)
		{
			var rawPtr = allocator.Value().GetSafePtr<ulong>(in ptr);
			var end = length.Min(pos + numBits);
			var idxB = pos >> 6;
			var shiftB = pos & 0x3f;
			var idxE = (end - 1) >> 6;
			var shiftE = end & 0x3f;
			var maskB = 0xfffffffffffffffful << shiftB;
			var maskE = 0xfffffffffffffffful >> (64 - shiftE);
			var orBits = (ulong)-Bitwise.FromBool(value);
			var orBitsB = maskB & orBits;
			var orBitsE = maskE & orBits;
			var cmaskB = ~maskB;
			var cmaskE = ~maskE;

			if (idxB == idxE)
			{
				var maskBe = maskB & maskE;
				var cmaskBe = ~maskBe;
				var orBitsBe = orBitsB & orBitsE;
				rawPtr[idxB] = (rawPtr[idxB] & cmaskBe) | orBitsBe;
				return;
			}

			rawPtr[idxB] = (rawPtr[idxB] & cmaskB) | orBitsB;

			for (var idx = idxB + 1; idx < idxE; ++idx)
			{
				rawPtr[idx] = orBits;
			}

			rawPtr[idxE] = (rawPtr[idxE] & cmaskE) | orBitsE;
		}

		/// <summary>
		/// Copies bits of a ulong to bits in this array.
		/// </summary>
		/// <remarks>
		/// The destination bits in this array run from index `pos` up to (but not including) `pos + numBits`.
		/// No exception is thrown if `pos + numBits` exceeds the length.
		///
		/// The lowest bit of the ulong is copied to the first destination bit; the second-lowest bit of the ulong is
		/// copied to the second destination bit; and so forth.
		/// </remarks>
		/// <param name="pos">Index of the first bit to set.</param>
		/// <param name="value">Unsigned long from which to copy bits.</param>
		/// <param name="numBits">Number of bits to set (must be between 1 and 64).</param>
		/// <exception cref="System.ArgumentException">Thrown if pos is out of bounds or if numBits is not between 1 and 64.</exception>
		[INLINE(256)]
		public void SetBits(SafePtr<Allocator> allocator, int pos, ulong value, int numBits = 1)
		{
			var ptr = allocator.Value().GetSafePtr<ulong>(in this.ptr);
			var idxB = pos >> 6;
			var shiftB = pos & 0x3f;

			if (shiftB + numBits <= 64)
			{
				var mask = 0xfffffffffffffffful >> (64 - numBits);
				ptr[idxB] = Bitwise.ReplaceBits(ptr[idxB], shiftB, mask, value);

				return;
			}

			var end = length.Min(pos + numBits);
			var idxE = (end - 1) >> 6;
			var shiftE = end & 0x3f;

			var maskB = 0xfffffffffffffffful >> shiftB;
			ptr[idxB] = Bitwise.ReplaceBits(ptr[idxB], shiftB, maskB, value);

			var valueE = value >> (64 - shiftB);
			var maskE = 0xfffffffffffffffful >> (64 - shiftE);
			ptr[idxE] = Bitwise.ReplaceBits(ptr[idxE], 0, maskE, valueE);
		}

		/// <summary>
		/// Returns a ulong which has bits copied from this array.
		/// </summary>
		/// <remarks>
		/// The source bits in this array run from index `pos` up to (but not including) `pos + numBits`.
		/// No exception is thrown if `pos + numBits` exceeds the length.
		///
		/// The first source bit is copied to the lowest bit of the ulong; the second source bit is copied to the second-lowest bit of the ulong; and so forth. Any remaining bits in the ulong will be 0.
		/// </remarks>
		/// <param name="pos">Index of the first bit to get.</param>
		/// <param name="numBits">Number of bits to get (must be between 1 and 64).</param>
		/// <exception cref="System.ArgumentException">Thrown if pos is out of bounds or if numBits is not between 1 and 64.</exception>
		/// <returns>A ulong which has bits copied from this array.</returns>
		[INLINE(256)]
		public ulong GetBits(SafePtr<Allocator> allocator, int pos, int numBits = 1)
		{
			var ptr = allocator.Value().GetSafePtr<ulong>(in this.ptr);
			var idxB = pos >> 6;
			var shiftB = pos & 0x3f;

			if (shiftB + numBits <= 64)
			{
				var mask = 0xfffffffffffffffful >> (64 - numBits);
				return Bitwise.ExtractBits(ptr[idxB], shiftB, mask);
			}

			var end = length.Min(pos + numBits);
			var idxE = (end - 1) >> 6;
			var shiftE = end & 0x3f;

			var maskB = 0xfffffffffffffffful >> shiftB;
			var valueB = Bitwise.ExtractBits(ptr[idxB], shiftB, maskB);

			var maskE = 0xfffffffffffffffful >> (64 - shiftE);
			var valueE = Bitwise.ExtractBits(ptr[idxE], 0, maskE);

			return (valueE << (64 - shiftB)) | valueB;
		}

		/// <summary>
		/// Returns true if the bit at an index is 1.
		/// </summary>
		/// <param name="pos">Index of the bit to test.</param>
		/// <returns>True if the bit at the index is 1.</returns>
		/// <exception cref="System.ArgumentException">Thrown if `pos` is out of bounds.</exception>
		[INLINE(256)]
		public bool IsSet(SafePtr<Allocator> allocator, int pos)
		{
			var ptr = allocator.Value().GetSafePtr<ulong>(in this.ptr);
			var idx = pos >> 6;
			var shift = pos & 0x3f;
			var mask = 1ul << shift;
			return 0ul != (ptr[idx] & mask);
		}

		[INLINE(256)]
		internal void CopyUlong(SafePtr<Allocator> allocator, int dstPos, ref MemBitArray srcBitArray, int srcPos,
			int numBits)
		{
			SetBits(allocator, dstPos, srcBitArray.GetBits(allocator, srcPos, numBits), numBits);
		}

		/// <summary>
		/// Copies a range of bits from this array to another range in this array.
		/// </summary>
		/// <remarks>
		/// The bits to copy run from index `srcPos` up to (but not including) `srcPos + numBits`.
		/// The bits to set run from index `dstPos` up to (but not including) `dstPos + numBits`.
		///
		/// The ranges may overlap, but the result in the overlapping region is undefined.
		/// </remarks>
		/// <param name="dstPos">Index of the first bit to set.</param>
		/// <param name="srcPos">Index of the first bit to copy.</param>
		/// <param name="numBits">Number of bits to copy.</param>
		/// <exception cref="System.ArgumentException">Thrown if either `dstPos + numBits` or `srcPos + numBits` exceed the length of this array.</exception>
		[INLINE(256)]
		public void Copy(SafePtr<Allocator> allocator, int dstPos, int srcPos, int numBits)
		{
			if (dstPos == srcPos)
			{
				return;
			}

			Copy(allocator, dstPos, ref this, srcPos, numBits);
		}

		/// <summary>
		/// Copies a range of bits from an array to a range of bits in this array.
		/// </summary>
		/// <remarks>
		/// The bits to copy in the source array run from index srcPos up to (but not including) `srcPos + numBits`.
		/// The bits to set in the destination array run from index dstPos up to (but not including) `dstPos + numBits`.
		///
		/// It's fine if source and destination array are one and the same, even if the ranges overlap, but the result in the overlapping region is undefined.
		/// </remarks>
		/// <param name="dstPos">Index of the first bit to set.</param>
		/// <param name="srcBitArray">The source array.</param>
		/// <param name="srcPos">Index of the first bit to copy.</param>
		/// <param name="numBits">The number of bits to copy.</param>
		/// <exception cref="System.ArgumentException">Thrown if either `dstPos + numBits` or `srcBitArray + numBits` exceed the length of this array.</exception>
		[INLINE(256)]
		public void Copy(SafePtr<Allocator> allocator, int dstPos, ref MemBitArray srcBitArray, int srcPos, int numBits)
		{
			var ptr = allocator.Value().GetSafePtr<ulong>(in this.ptr);
			if (numBits == 0)
			{
				return;
			}

			if (numBits <= 64) // 1x CopyUlong
			{
				CopyUlong(allocator, dstPos, ref srcBitArray, srcPos, numBits);
			}
			else if (numBits <= 128) // 2x CopyUlong
			{
				CopyUlong(allocator, dstPos, ref srcBitArray, srcPos, 64);
				numBits -= 64;

				if (numBits > 0)
				{
					CopyUlong(allocator, dstPos + 64, ref srcBitArray, srcPos + 64, numBits);
				}
			}
			else if ((dstPos & 7) == (srcPos & 7)) // aligned copy
			{
				var dstPosInBytes = MemoryExt.Align(dstPos, 8) >> 3;
				var srcPosInBytes = MemoryExt.Align(srcPos, 8) >> 3;
				var numPreBits = dstPosInBytes * 8 - dstPos;

				if (numPreBits > 0)
				{
					CopyUlong(allocator, dstPos, ref srcBitArray, srcPos, numPreBits);
				}

				var numBitsLeft = numBits - numPreBits;
				var numBytes = numBitsLeft / 8;

				if (numBytes > 0)
				{
					allocator.Value().MemMove(srcBitArray.ptr, srcPosInBytes, this.ptr, dstPosInBytes, numBytes);
				}

				var numPostBits = numBitsLeft & 7;

				if (numPostBits > 0)
				{
					CopyUlong(allocator, (dstPosInBytes + numBytes) * 8, ref srcBitArray, (srcPosInBytes + numBytes) * 8, numPostBits);
				}
			}
			else // unaligned copy
			{
				var dstPosAligned = MemoryExt.Align(dstPos, 64);
				var numPreBits = dstPosAligned - dstPos;

				if (numPreBits > 0)
				{
					CopyUlong(allocator, dstPos, ref srcBitArray, srcPos, numPreBits);
					numBits -= numPreBits;
					dstPos += numPreBits;
					srcPos += numPreBits;
				}

				for (; numBits >= 64; numBits -= 64, dstPos += 64, srcPos += 64)
				{
					ptr[dstPos >> 6] = srcBitArray.GetBits(allocator, srcPos, 64);
				}

				if (numBits > 0)
				{
					CopyUlong(allocator, dstPos, ref srcBitArray, srcPos, numBits);
				}
			}
		}

		/// <summary>
		/// Returns the index of the first occurrence in this array of *N* contiguous 0 bits.
		/// </summary>
		/// <remarks>The search is linear.</remarks>
		/// <param name="pos">Index of the bit at which to start searching.</param>
		/// <param name="numBits">Number of contiguous 0 bits to look for.</param>
		/// <returns>The index of the first occurrence in this array of `numBits` contiguous 0 bits. Range is pos up to (but not including) the length of this array. Returns -1 if no occurrence is found.</returns>
		[INLINE(256)]
		public int Find(SafePtr<Allocator> allocator, int pos, int numBits)
		{
			var safePtr = allocator.Value().GetSafePtr<ulong>(in this.ptr);
			var count = length - pos;
			return Bitwise.Find(safePtr.ptr, pos, count, numBits);
		}

		/// <summary>
		/// Returns the index of the first occurrence in this array of a given number of contiguous 0 bits.
		/// </summary>
		/// <remarks>The search is linear.</remarks>
		/// <param name="pos">Index of the bit at which to start searching.</param>
		/// <param name="numBits">Number of contiguous 0 bits to look for.</param>
		/// <param name="count">Number of indexes to consider as the return value.</param>
		/// <returns>The index of the first occurrence in this array of `numBits` contiguous 0 bits. Range is pos up to (but not including) `pos + count`. Returns -1 if no occurrence is found.</returns>
		[INLINE(256)]
		public int Find(SafePtr<Allocator> allocator, int pos, int count, int numBits)
		{
			var safePtr = allocator.Value().GetSafePtr<ulong>(in this.ptr);
			return Bitwise.Find(safePtr.ptr, pos, count, numBits);
		}

		/// <summary>
		/// Returns true if none of the bits in a range are 1 (*i.e.* all bits in the range are 0).
		/// </summary>
		/// <param name="pos">Index of the bit at which to start searching.</param>
		/// <param name="numBits">Number of bits to test. Defaults to 1.</param>
		/// <returns>Returns true if none of the bits in range `pos` up to (but not including) `pos + numBits` are 1.</returns>
		/// <exception cref="System.ArgumentException">Thrown if `pos` is out of bounds or `numBits` is less than 1.</exception>
		[INLINE(256)]
		public bool TestNone(SafePtr<Allocator> allocator, int pos, int numBits = 1)
		{
			var rawPtr = allocator.Value().GetSafePtr<ulong>(in ptr);
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
				return 0ul == (rawPtr[idxB] & mask);
			}

			if (0ul != (rawPtr[idxB] & maskB))
			{
				return false;
			}

			for (var idx = idxB + 1; idx < idxE; ++idx)
			{
				if (0ul != rawPtr[idx])
				{
					return false;
				}
			}

			return 0ul == (rawPtr[idxE] & maskE);
		}

		/// <summary>
		/// Returns true if at least one of the bits in a range is 1.
		/// </summary>
		/// <param name="pos">Index of the bit at which to start searching.</param>
		/// <param name="numBits">Number of bits to test. Defaults to 1.</param>
		/// <returns>True if one or more of the bits in range `pos` up to (but not including) `pos + numBits` are 1.</returns>
		/// <exception cref="System.ArgumentException">Thrown if `pos` is out of bounds or `numBits` is less than 1.</exception>
		[INLINE(256)]
		public bool TestAny(SafePtr<Allocator> allocator, int pos, int numBits = 1)
		{
			var rawPtr = allocator.Value().GetSafePtr<ulong>(in ptr);
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
				return 0ul != (rawPtr[idxB] & mask);
			}

			if (0ul != (rawPtr[idxB] & maskB))
			{
				return true;
			}

			for (var idx = idxB + 1; idx < idxE; ++idx)
			{
				if (0ul != rawPtr[idx])
				{
					return true;
				}
			}

			return 0ul != (rawPtr[idxE] & maskE);
		}

		/// <summary>
		/// Returns true if all of the bits in a range are 1.
		/// </summary>
		/// <param name="pos">Index of the bit at which to start searching.</param>
		/// <param name="numBits">Number of bits to test. Defaults to 1.</param>
		/// <returns>True if all of the bits in range `pos` up to (but not including) `pos + numBits` are 1.</returns>
		/// <exception cref="System.ArgumentException">Thrown if `pos` is out of bounds or `numBits` is less than 1.</exception>
		[INLINE(256)]
		public bool TestAll(SafePtr<Allocator> allocator, int pos, int numBits = 1)
		{
			var rawPtr = allocator.Value().GetSafePtr<ulong>(in ptr);
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
				return mask == (rawPtr[idxB] & mask);
			}

			if (maskB != (rawPtr[idxB] & maskB))
			{
				return false;
			}

			for (var idx = idxB + 1; idx < idxE; ++idx)
			{
				if (0xfffffffffffffffful != rawPtr[idx])
				{
					return false;
				}
			}

			return maskE == (rawPtr[idxE] & maskE);
		}

		/// <summary>
		/// Returns the number of bits in a range that are 1.
		/// </summary>
		/// <param name="pos">Index of the bit at which to start searching.</param>
		/// <param name="numBits">Number of bits to test. Defaults to 1.</param>
		/// <returns>The number of bits in a range of bits that are 1.</returns>
		/// <exception cref="System.ArgumentException">Thrown if `pos` is out of bounds or `numBits` is less than 1.</exception>
		[INLINE(256)]
		public int CountBits(SafePtr<Allocator> allocator, int pos, int numBits = 1)
		{
			var rawPtr = allocator.Value().GetSafePtr<ulong>(in ptr);
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
				return (rawPtr[idxB] & mask).CountBits();
			}

			var count = (rawPtr[idxB] & maskB).CountBits();

			for (var idx = idxB + 1; idx < idxE; ++idx)
			{
				count += rawPtr[idx].CountBits();
			}

			count += (rawPtr[idxE] & maskE).CountBits();

			return count;
		}

		[INLINE(256)]
		public void RemoveExcept(SafePtr<Allocator> allocator, MemBitArray bits)
		{
			var p = allocator.Value().GetSafePtr<ulong>(ptr);
			var p2 = allocator.Value().GetSafePtr<ulong>(bits.ptr);
			var length = this.length / 64;
			for (var i = 0; i < length; ++i)
			{
				var val2 = p2[i];
				p[i] &= val2;
			}
		}
	}

	internal sealed unsafe class UnsafeBitArrayDebugView
	{
		private MemBitArray _data;

		public UnsafeBitArrayDebugView(MemBitArray data)
		{
			_data = data;
		}

		public bool[] Bits
		{
			get
			{
				var allocator = AllocatorManager.CurrentAllocatorPtr;
				var array = new bool[_data.length];
				for (var i = 0; i < _data.length; ++i)
				{
					array[i] = _data.IsSet(allocator, i);
				}

				return array;
			}
		}
	}
}
