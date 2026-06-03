using System;
using System.Diagnostics;
using Sapientia.Data;
using Sapientia.Extensions;
using Submodules.Sapientia.Data;
using Submodules.Sapientia.Memory;

namespace Sapientia.Collections
{
#if UNITY_5_3_OR_NEWER
	[Unity.Burst.BurstCompile]
#endif
	[DebuggerTypeProxy(typeof(UnsafeBitArray.UnsafeBitArrayProxy))]
	public struct UnsafeBitArray : IDisposable
	{
		/// <summary>
		/// Pointer to the data.
		/// </summary>
		/// <value>Pointer to the data.</value>
		public SafePtr<ulong> ptr;

		/// <summary>
		/// The number of bits.
		/// </summary>
		/// <value>The number of bits.</value>
		public int Length;

		/// <summary>
		/// The capacity number of bits.
		/// </summary>
		/// <value>The capacity number of bits.</value>
		public int Capacity;

		public readonly Id<MemoryManager> memoryId;

		/// <summary>
		/// Initializes and returns an instance of UnsafeBitArray.
		/// </summary>
		/// <param name="numBits">Number of bits.</param>
		/// <param name="allocator">The allocator to use.</param>
		/// <param name="options">Whether newly allocated bytes should be zeroed out.</param>
		public UnsafeBitArray(Id<MemoryManager> memoryId, int numBits, ClearOptions clearMemory = ClearOptions.ClearMemory)
		{
			this.memoryId = memoryId;

			ptr = default;
			Length = 0;
			Capacity = 0;

			Resize(numBits, clearMemory);
		}

		/// <summary>
		/// Whether this array has been allocated (and not yet deallocated).
		/// </summary>
		/// <value>True if this array has been allocated (and not yet deallocated).</value>
		public readonly bool IsCreated => ptr.IsValid;

		/// <summary>
		/// Whether the container is empty.
		/// </summary>
		/// <value>True if the container is empty or the container has not been constructed.</value>
		public readonly bool IsEmpty => !IsCreated || Length == 0;

		private void Realloc(int capacityInBits)
		{
			var newCapacity = capacityInBits.AlignUp(64);
			var sizeInBytes = newCapacity / 8;

			SafePtr<ulong> newPointer = default;

			if (sizeInBytes > 0)
			{
				newPointer = memoryId.GetManager().MakeArray<ulong>(sizeInBytes, 16, ClearOptions.UninitializedMemory);

				if (Capacity > 0)
				{
					var itemsToCopy = newCapacity.Min(Capacity);
					var bytesToCopy = itemsToCopy / 8;
					MemoryExt.MemCopy(ptr.Cast<byte>(), newPointer.Cast<byte>(), bytesToCopy);
				}
			}

			ptr = newPointer;
			Capacity = newCapacity;
			Length = Length.Min(newCapacity);
		}

		public void EnsureLength(int length, ClearOptions clearMemory = ClearOptions.ClearMemory)
		{
			if (Length < length)
				Resize(length, clearMemory);
		}

		/// <summary>
		/// Sets the length, expanding the capacity if necessary.
		/// </summary>
		/// <param name="numBits">The new length in bits.</param>
		/// <param name="clearMemory">Whether newly allocated data should be zeroed out.</param>
		public void Resize(int numBits, ClearOptions clearMemory = ClearOptions.ClearMemory)
		{
			var minCapacity = numBits.Max(1);

			if (minCapacity > Capacity)
			{
				SetCapacity(minCapacity);
			}

			var oldLength = Length;
			Length = numBits;

			if (clearMemory == ClearOptions.ClearMemory && oldLength < Length)
			{
				SetBits(oldLength, false, Length - oldLength);
			}
		}

		/// <summary>
		/// Sets the capacity.
		/// </summary>
		/// <param name="capacityInBits">The new capacity.</param>
		public void SetCapacity(int capacityInBits)
		{
			if (Capacity == capacityInBits)
			{
				return;
			}

			Realloc(capacityInBits);
		}

		/// <summary>
		/// Sets the capacity to match what it would be if it had been originally initialized with all its entries.
		/// </summary>
		public void TrimExcess()
		{
			SetCapacity(Length);
		}

		/// <summary>
		/// Releases all resources (memory and safety handles).
		/// </summary>
		public void Dispose()
		{
			if (!IsCreated)
			{
				return;
			}

			memoryId.GetManager().MemFree(ptr);
			this = default;
		}

		/// <summary>
		/// Sets all the bits to 0.
		/// </summary>
		public void Clear()
		{
			var sizeInBytes = Length.AlignUp(64) / 8;
			MemoryExt.MemClear(ptr.Cast<byte>(), sizeInBytes);
		}

		/// <summary>
		/// Sets the bit at an index to 0 or 1.
		/// </summary>
		/// <param name="pos">Index of the bit to set.</param>
		/// <param name="value">True for 1, false for 0.</param>
		public void Set(int pos, bool value)
		{
			CheckArgs(pos, 1);

			var idx = pos >> 6;
			var shift = pos & 0x3f;
			var mask = 1ul << shift;
			var bits = (ptr[idx] & ~mask) | ((ulong)-value.ToInt() & mask);
			ptr[idx] = bits;
		}

		/// <summary>
		/// Sets the bit at an index to 0 or 1.
		/// If out of range - resize array and set.
		/// </summary>
		/// <param name="pos">Index of the bit to set.</param>
		/// <param name="value">True for 1, false for 0.</param>
		public void EnsureSet(int pos, bool value)
		{
			if (Length <= pos)
				Resize(pos + 1, ClearOptions.ClearMemory);
			Set(pos, value);
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
		/// <exception cref="ArgumentException">Thrown if pos is out of bounds or if numBits is less than 1.</exception>
		public void SetBits(int pos, bool value, int numBits)
		{
			CheckArgs(pos, numBits);

			var end = Length.Min(pos + numBits);
			var idxB = pos >> 6;
			var shiftB = pos & 0x3f;
			var idxE = (end - 1) >> 6;
			var shiftE = end & 0x3f;
			var maskB = 0xfffffffffffffffful << shiftB;
			var maskE = 0xfffffffffffffffful >> (64 - shiftE);
			var orBits = (ulong)-value.ToInt();
			var orBitsB = maskB & orBits;
			var orBitsE = maskE & orBits;
			var cmaskB = ~maskB;
			var cmaskE = ~maskE;

			if (idxB == idxE)
			{
				var maskBE = maskB & maskE;
				var cmaskBE = ~maskBE;
				var orBitsBE = orBitsB & orBitsE;
				ptr[idxB] = (ptr[idxB] & cmaskBE) | orBitsBE;
				return;
			}

			ptr[idxB] = (ptr[idxB] & cmaskB) | orBitsB;

			for (var idx = idxB + 1; idx < idxE; ++idx)
			{
				ptr[idx] = orBits;
			}

			ptr[idxE] = (ptr[idxE] & cmaskE) | orBitsE;
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
		/// <exception cref="ArgumentException">Thrown if pos is out of bounds or if numBits is not between 1 and 64.</exception>
		public void SetBits(int pos, ulong value, int numBits = 1)
		{
			CheckArgsUlong(pos, numBits);

			var idxB = pos >> 6;
			var shiftB = pos & 0x3f;

			if (shiftB + numBits <= 64)
			{
				var mask = 0xfffffffffffffffful >> (64 - numBits);
				ptr[idxB] = ptr[idxB].ReplaceBits(shiftB, mask, value);

				return;
			}

			var end = Length.Min(pos + numBits);
			var idxE = (end - 1) >> 6;
			var shiftE = end & 0x3f;

			var maskB = 0xfffffffffffffffful >> shiftB;
			ptr[idxB] = ptr[idxB].ReplaceBits(shiftB, maskB, value);

			var valueE = value >> (64 - shiftB);
			var maskE = 0xfffffffffffffffful >> (64 - shiftE);
			ptr[idxE] = ptr[idxE].ReplaceBits(0, maskE, valueE);
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
		/// <exception cref="ArgumentException">Thrown if pos is out of bounds or if numBits is not between 1 and 64.</exception>
		/// <returns>A ulong which has bits copied from this array.</returns>
		public ulong GetBits(int pos, int numBits = 1)
		{
			CheckArgsUlong(pos, numBits);
			return BitExt.GetBits(ptr, Length, pos, numBits);
		}

		/// <summary>
		/// Returns true if the bit at an index is 1.
		/// </summary>
		/// <param name="pos">Index of the bit to test.</param>
		/// <returns>True if the bit at the index is 1.</returns>
		/// <exception cref="ArgumentException">Thrown if `pos` is out of bounds.</exception>
		public bool IsSet(int pos)
		{
			CheckArgs(pos, 1);
			return BitExt.IsSet(ptr, pos);
		}

		/// <summary>
		/// Returns true if the bit at an index is 1.
		/// Returns false if out of range.
		/// </summary>
		/// <param name="pos">Index of the bit to test.</param>
		/// <returns>True if the bit at the index is 1.</returns>
		/// <exception cref="ArgumentException">Thrown if `pos` is out of bounds.</exception>
		public bool EnsureIsSet(int pos)
		{
			if (pos < 0 || Length <= pos)
				return false;
			return IsSet(pos);
		}

		internal void CopyUlong(int dstPos, ref UnsafeBitArray srcBitArray, int srcPos, int numBits) =>
			SetBits(dstPos, srcBitArray.GetBits(srcPos, numBits), numBits);

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
		/// <exception cref="ArgumentException">Thrown if either `dstPos + numBits` or `srcPos + numBits` exceed the length of this array.</exception>
		public void Copy(int dstPos, int srcPos, int numBits)
		{
			if (dstPos == srcPos)
			{
				return;
			}

			Copy(dstPos, ref this, srcPos, numBits);
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
		/// <exception cref="ArgumentException">Thrown if either `dstPos + numBits` or `srcBitArray + numBits` exceed the length of this array.</exception>
		public void Copy(int dstPos, ref UnsafeBitArray srcBitArray, int srcPos, int numBits)
		{
			if (numBits == 0)
			{
				return;
			}

			CheckArgsCopy(ref this, dstPos, ref srcBitArray, srcPos, numBits);

			if (numBits <= 64) // 1x CopyUlong
			{
				CopyUlong(dstPos, ref srcBitArray, srcPos, numBits);
			}
			else if (numBits <= 128) // 2x CopyUlong
			{
				CopyUlong(dstPos, ref srcBitArray, srcPos, 64);
				numBits -= 64;

				if (numBits > 0)
				{
					CopyUlong(dstPos + 64, ref srcBitArray, srcPos + 64, numBits);
				}
			}
			else if ((dstPos & 7) == (srcPos & 7)) // aligned copy
			{
				var dstPosInBytes = MemoryExt.Align(dstPos, 8) >> 3;
				var srcPosInBytes = MemoryExt.Align(srcPos, 8) >> 3;
				var numPreBits = dstPosInBytes * 8 - dstPos;

				if (numPreBits > 0)
				{
					CopyUlong(dstPos, ref srcBitArray, srcPos, numPreBits);
				}

				var numBitsLeft = numBits - numPreBits;
				var numBytes = numBitsLeft / 8;

				if (numBytes > 0)
				{
					MemoryExt.MemMove(srcBitArray.ptr.Cast<byte>() + srcPosInBytes, ptr.Cast<byte>() + dstPosInBytes, numBytes);
				}

				var numPostBits = numBitsLeft & 7;

				if (numPostBits > 0)
				{
					CopyUlong((dstPosInBytes + numBytes) * 8, ref srcBitArray, (srcPosInBytes + numBytes) * 8, numPostBits);
				}
			}
			else // unaligned copy
			{
				var dstPosAligned = MemoryExt.Align(dstPos, 64);
				var numPreBits = dstPosAligned - dstPos;

				if (numPreBits > 0)
				{
					CopyUlong(dstPos, ref srcBitArray, srcPos, numPreBits);
					numBits -= numPreBits;
					dstPos += numPreBits;
					srcPos += numPreBits;
				}

				for (; numBits >= 64; numBits -= 64, dstPos += 64, srcPos += 64)
				{
					ptr[dstPos >> 6] = srcBitArray.GetBits(srcPos, 64);
				}

				if (numBits > 0)
				{
					CopyUlong(dstPos, ref srcBitArray, srcPos, numBits);
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
		public int Find(int pos, int numBits)
		{
			var count = Length - pos;
			CheckArgsPosCount(pos, count, numBits);
			return BitExt.Find(ptr, pos, count, numBits);
		}

		/// <summary>
		/// Returns the index of the first occurrence in this array of a given number of contiguous 0 bits.
		/// </summary>
		/// <remarks>The search is linear.</remarks>
		/// <param name="pos">Index of the bit at which to start searching.</param>
		/// <param name="numBits">Number of contiguous 0 bits to look for.</param>
		/// <param name="count">Number of indexes to consider as the return value.</param>
		/// <returns>The index of the first occurrence in this array of `numBits` contiguous 0 bits. Range is pos up to (but not including) `pos + count`. Returns -1 if no occurrence is found.</returns>
		public int Find(int pos, int count, int numBits)
		{
			CheckArgsPosCount(pos, count, numBits);
			return BitExt.Find(ptr, pos, count, numBits);
		}

		/// <summary>
		/// Returns true if none of the bits in a range are 1 (*i.e.* all bits in the range are 0).
		/// </summary>
		/// <param name="pos">Index of the bit at which to start searching.</param>
		/// <param name="numBits">Number of bits to test. Defaults to 1.</param>
		/// <returns>Returns true if none of the bits in range `pos` up to (but not including) `pos + numBits` are 1.</returns>
		/// <exception cref="ArgumentException">Thrown if `pos` is out of bounds or `numBits` is less than 1.</exception>
		public bool TestNone(int pos, int numBits = 1)
		{
			CheckArgs(pos, numBits);
			return BitExt.TestNone(ptr, Length, pos, numBits);
		}

		/// <summary>
		/// Returns true if at least one of the bits in a range is 1.
		/// </summary>
		/// <param name="pos">Index of the bit at which to start searching.</param>
		/// <param name="numBits">Number of bits to test. Defaults to 1.</param>
		/// <returns>True if one or more of the bits in range `pos` up to (but not including) `pos + numBits` are 1.</returns>
		/// <exception cref="ArgumentException">Thrown if `pos` is out of bounds or `numBits` is less than 1.</exception>
		public bool TestAny(int pos, int numBits = 1)
		{
			CheckArgs(pos, numBits);
			return BitExt.TestAny(ptr, Length, pos, numBits);
		}

		/// <summary>
		/// Returns true if all of the bits in a range are 1.
		/// </summary>
		/// <param name="pos">Index of the bit at which to start searching.</param>
		/// <param name="numBits">Number of bits to test. Defaults to 1.</param>
		/// <returns>True if all of the bits in range `pos` up to (but not including) `pos + numBits` are 1.</returns>
		/// <exception cref="ArgumentException">Thrown if `pos` is out of bounds or `numBits` is less than 1.</exception>
		public bool TestAll(int pos, int numBits = 1)
		{
			CheckArgs(pos, numBits);
			return BitExt.TestAll(ptr, Length, pos, numBits);
		}

		/// <summary>
		/// Returns the number of bits in a range that are 1.
		/// </summary>
		/// <param name="pos">Index of the bit at which to start searching.</param>
		/// <param name="numBits">Number of bits to test. Defaults to 1.</param>
		/// <returns>The number of bits in a range of bits that are 1.</returns>
		/// <exception cref="ArgumentException">Thrown if `pos` is out of bounds or `numBits` is less than 1.</exception>
		public int CountBits(int pos, int numBits = 1)
		{
			CheckArgs(pos, numBits);
			return BitExt.CountBits(ptr, Length, pos, numBits);
		}

		/// <summary>
		/// Returns a readonly version of this UnsafeBitArray instance.
		/// </summary>
		/// <remarks>ReadOnly containers point to the same underlying data as the UnsafeBitArray it is made from.</remarks>
		/// <returns>ReadOnly instance for this.</returns>
		public ReadOnly AsReadOnly()
		{
			return new ReadOnly(ptr, Length);
		}

		/// <summary>
		/// A read-only alias for the value of a UnsafeBitArray. Does not have its own allocated storage.
		/// </summary>
		public struct ReadOnly
		{
			/// <summary>
			/// Pointer to the data.
			/// </summary>
			/// <value>Pointer to the data.</value>
			public readonly SafePtr<ulong> Ptr;

			/// <summary>
			/// The number of bits.
			/// </summary>
			/// <value>The number of bits.</value>
			public readonly int Length;

			/// <summary>
			/// Whether this array has been allocated (and not yet deallocated).
			/// </summary>
			/// <value>True if this array has been allocated (and not yet deallocated).</value>
			public readonly bool IsCreated => Ptr != null;

			/// <summary>
			/// Whether the container is empty.
			/// </summary>
			/// <value>True if the container is empty or the container has not been constructed.</value>
			public readonly bool IsEmpty => !IsCreated || Length == 0;

			internal ReadOnly(SafePtr<ulong> ptr, int length)
			{
				Ptr = ptr;
				Length = length;
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
			/// <exception cref="ArgumentException">Thrown if pos is out of bounds or if numBits is not between 1 and 64.</exception>
			/// <returns>A ulong which has bits copied from this array.</returns>
			public readonly ulong GetBits(int pos, int numBits = 1)
			{
				CheckArgsUlong(pos, numBits);
				return BitExt.GetBits(Ptr, Length, pos, numBits);
			}

			/// <summary>
			/// Returns true if the bit at an index is 1.
			/// </summary>
			/// <param name="pos">Index of the bit to test.</param>
			/// <returns>True if the bit at the index is 1.</returns>
			/// <exception cref="ArgumentException">Thrown if `pos` is out of bounds.</exception>
			public readonly bool IsSet(int pos)
			{
				CheckArgs(pos, 1);
				return BitExt.IsSet(Ptr, pos);
			}

			/// <summary>
			/// Returns the index of the first occurrence in this array of *N* contiguous 0 bits.
			/// </summary>
			/// <remarks>The search is linear.</remarks>
			/// <param name="pos">Index of the bit at which to start searching.</param>
			/// <param name="numBits">Number of contiguous 0 bits to look for.</param>
			/// <returns>The index of the first occurrence in this array of `numBits` contiguous 0 bits. Range is pos up to (but not including) the length of this array. Returns -1 if no occurrence is found.</returns>
			public readonly int Find(int pos, int numBits)
			{
				var count = Length - pos;
				CheckArgsPosCount(pos, count, numBits);
				return BitExt.Find(Ptr, pos, count, numBits);
			}

			/// <summary>
			/// Returns the index of the first occurrence in this array of a given number of contiguous 0 bits.
			/// </summary>
			/// <remarks>The search is linear.</remarks>
			/// <param name="pos">Index of the bit at which to start searching.</param>
			/// <param name="numBits">Number of contiguous 0 bits to look for.</param>
			/// <param name="count">Number of indexes to consider as the return value.</param>
			/// <returns>The index of the first occurrence in this array of `numBits` contiguous 0 bits. Range is pos up to (but not including) `pos + count`. Returns -1 if no occurrence is found.</returns>
			public readonly int Find(int pos, int count, int numBits)
			{
				CheckArgsPosCount(pos, count, numBits);
				return BitExt.Find(Ptr, pos, count, numBits);
			}

			/// <summary>
			/// Returns true if none of the bits in a range are 1 (*i.e.* all bits in the range are 0).
			/// </summary>
			/// <param name="pos">Index of the bit at which to start searching.</param>
			/// <param name="numBits">Number of bits to test. Defaults to 1.</param>
			/// <returns>Returns true if none of the bits in range `pos` up to (but not including) `pos + numBits` are 1.</returns>
			/// <exception cref="ArgumentException">Thrown if `pos` is out of bounds or `numBits` is less than 1.</exception>
			public readonly bool TestNone(int pos, int numBits = 1)
			{
				CheckArgs(pos, numBits);
				return BitExt.TestNone(Ptr, Length, pos, numBits);
			}

			/// <summary>
			/// Returns true if at least one of the bits in a range is 1.
			/// </summary>
			/// <param name="pos">Index of the bit at which to start searching.</param>
			/// <param name="numBits">Number of bits to test. Defaults to 1.</param>
			/// <returns>True if one or more of the bits in range `pos` up to (but not including) `pos + numBits` are 1.</returns>
			/// <exception cref="ArgumentException">Thrown if `pos` is out of bounds or `numBits` is less than 1.</exception>
			public readonly bool TestAny(int pos, int numBits = 1)
			{
				CheckArgs(pos, numBits);
				return BitExt.TestAny(Ptr, Length, pos, numBits);
			}

			/// <summary>
			/// Returns true if all of the bits in a range are 1.
			/// </summary>
			/// <param name="pos">Index of the bit at which to start searching.</param>
			/// <param name="numBits">Number of bits to test. Defaults to 1.</param>
			/// <returns>True if all of the bits in range `pos` up to (but not including) `pos + numBits` are 1.</returns>
			/// <exception cref="ArgumentException">Thrown if `pos` is out of bounds or `numBits` is less than 1.</exception>
			public readonly bool TestAll(int pos, int numBits = 1)
			{
				CheckArgs(pos, numBits);
				return BitExt.TestAll(Ptr, Length, pos, numBits);
			}

			/// <summary>
			/// Returns the number of bits in a range that are 1.
			/// </summary>
			/// <param name="pos">Index of the bit at which to start searching.</param>
			/// <param name="numBits">Number of bits to test. Defaults to 1.</param>
			/// <returns>The number of bits in a range of bits that are 1.</returns>
			/// <exception cref="ArgumentException">Thrown if `pos` is out of bounds or `numBits` is less than 1.</exception>
			public readonly int CountBits(int pos, int numBits = 1)
			{
				CheckArgs(pos, numBits);
				return BitExt.CountBits(Ptr, Length, pos, numBits);
			}

			[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS"), Conditional("UNITY_DOTS_DEBUG")]
			readonly void CheckArgs(int pos, int numBits)
			{
				if (pos < 0
					|| pos >= Length
					|| numBits < 1)
				{
					throw new ArgumentException(
						$"BitArray invalid arguments: pos {pos} (must be 0-{Length - 1}), numBits {numBits} (must be greater than 0).");
				}
			}

			[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS"), Conditional("UNITY_DOTS_DEBUG")]
			readonly void CheckArgsPosCount(int begin, int count, int numBits)
			{
				if (begin < 0 || begin >= Length)
				{
					throw new ArgumentException($"BitArray invalid argument: begin {begin} (must be 0-{Length - 1}).");
				}

				if (count < 0 || count > Length)
				{
					throw new ArgumentException($"BitArray invalid argument: count {count} (must be 0-{Length}).");
				}

				if (numBits < 1 || count < numBits)
				{
					throw new ArgumentException($"BitArray invalid argument: numBits {numBits} (must be greater than 0).");
				}
			}

			[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS"), Conditional("UNITY_DOTS_DEBUG")]
			readonly void CheckArgsUlong(int pos, int numBits)
			{
				CheckArgs(pos, numBits);

				if (numBits < 1 || numBits > 64)
				{
					throw new ArgumentException($"BitArray invalid arguments: numBits {numBits} (must be 1-64).");
				}

				if (pos + numBits > Length)
				{
					throw new ArgumentException(
						$"BitArray invalid arguments: Out of bounds pos {pos}, numBits {numBits}, Length {Length}.");
				}
			}
		}

		[Conditional(E.DEBUG)]
		private void CheckArgs(int pos, int numBits)
		{
			if (pos < 0
				|| pos >= Length
				|| numBits < 1)
			{
				throw new ArgumentException(
					$"BitArray invalid arguments: pos {pos} (must be 0-{Length - 1}), numBits {numBits} (must be greater than 0).");
			}
		}

		[Conditional(E.DEBUG)]
		private void CheckArgsPosCount(int begin, int count, int numBits)
		{
			if (begin < 0 || begin >= Length)
			{
				throw new ArgumentException($"BitArray invalid argument: begin {begin} (must be 0-{Length - 1}).");
			}

			if (count < 0 || count > Length)
			{
				throw new ArgumentException($"BitArray invalid argument: count {count} (must be 0-{Length}).");
			}

			if (numBits < 1 || count < numBits)
			{
				throw new ArgumentException($"BitArray invalid argument: numBits {numBits} (must be greater than 0).");
			}
		}

		[Conditional(E.DEBUG)]
		private void CheckArgsUlong(int pos, int numBits)
		{
			CheckArgs(pos, numBits);

			if (numBits < 1 || numBits > 64)
			{
				throw new ArgumentException($"BitArray invalid arguments: numBits {numBits} (must be 1-64).");
			}

			if (pos + numBits > Length)
			{
				throw new ArgumentException($"BitArray invalid arguments: Out of bounds pos {pos}, numBits {numBits}, Length {Length}.");
			}
		}

		[Conditional(E.DEBUG)]
		private static void CheckArgsCopy(ref UnsafeBitArray dstBitArray, int dstPos, ref UnsafeBitArray srcBitArray, int srcPos, int numBits)
		{
			if (srcPos + numBits > srcBitArray.Length)
			{
				throw new ArgumentException(
					$"BitArray invalid arguments: Out of bounds - source position {srcPos}, numBits {numBits}, source bit array Length {srcBitArray.Length}.");
			}

			if (dstPos + numBits > dstBitArray.Length)
			{
				throw new ArgumentException(
					$"BitArray invalid arguments: Out of bounds - destination position {dstPos}, numBits {numBits}, destination bit array Length {dstBitArray.Length}.");
			}
		}

		private class UnsafeBitArrayProxy
		{
			UnsafeBitArray _bitArray;

			public UnsafeBitArrayProxy(UnsafeBitArray bitArray)
			{
				this._bitArray = bitArray;
			}

			public bool[] Bits
			{
				get
				{
					var array = new bool[_bitArray.Length];
					for (var i = 0; i < _bitArray.Length; ++i)
					{
						array[i] = _bitArray.IsSet(i);
					}

					return array;
				}
			}
		}
	}
}
