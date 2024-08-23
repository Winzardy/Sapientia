using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.MemoryAllocator
{
	using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(BitArrayDebugView))]
	public unsafe struct BitArray
	{
		private const int BITS_IN_ULONG = sizeof(ulong) * 8;

		public MemPtr ptr;
		public uint Length;
#if UNITY_5_3_OR_NEWER
		[Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
		private ulong* cachedPtr;

		public bool IsCreated => ptr.IsValid();

		[INLINE(256)]
		public BitArray(ref Allocator allocator, uint length, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			var sizeInBytes = Bitwise.AlignULongBits(length);
			cachedPtr = null;
			this.ptr = allocator.Alloc(sizeInBytes, out var ptr);
			cachedPtr = (ulong*)ptr;
			Length = length;

			if (clearOptions == ClearOptions.ClearMemory)
			{
				allocator.MemClear(this.ptr, 0u, sizeInBytes);
			}
		}

		[INLINE(256)]
		public BitArray(ref Allocator allocator, BitArray source)
		{
			var sizeInBytes = Bitwise.AlignULongBits(source.Length);
			cachedPtr = null;
			this.ptr = allocator.Alloc(sizeInBytes, out var ptr);
			cachedPtr = (ulong*)ptr;
			Length = source.Length;
			var sourcePtr = allocator.GetUnsafePtr(source.ptr);
			allocator.ValidateConsistency();
			MemoryExt.MemCopy(sourcePtr, ptr, sizeInBytes);
			allocator.ValidateConsistency();
		}

		[INLINE(256)]
		public void Set(ref Allocator allocator, BitArray source)
		{
			var sizeInBytes = Bitwise.AlignULongBits(source.Length);
			Resize(ref allocator, source.Length);
			var sourcePtr = allocator.GetUnsafePtr(in source.ptr);
			MemoryExt.MemCopy(sourcePtr, allocator.GetUnsafePtr(in ptr), sizeInBytes);
		}

		[INLINE(256)]
		public bool ContainsAll(in Allocator allocator, BitArray other)
		{
			var len = Bitwise.GetMinLength(other.Length, Length);
			var unsafePtr = (ulong*)allocator.GetUnsafePtr(in ptr);
			var ptrOther = (ulong*)allocator.GetUnsafePtr(in other.ptr);
			for (var index = 0; index < len; index++)
			{
				if ((unsafePtr[index] & ptrOther[index]) != ptrOther[index]) return false;
			}

			return true;
		}

		[INLINE(256)]
		public void Resize(ref Allocator allocator, uint newLength, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			if (newLength > Length)
			{
				ptr = allocator.ReAllocArray(in ptr, Bitwise.AlignULongBits(Length), out cachedPtr);
				if (clearOptions == ClearOptions.ClearMemory)
				{
					var clearSize = Bitwise.AlignULongBits(newLength - Length);
					MemoryExt.MemClear((byte*)cachedPtr + Bitwise.AlignULongBits(Length), clearSize);
				}

				Length = newLength;
			}
		}

		[INLINE(256)]
		public void BurstMode(in Allocator allocator, bool state)
		{
			if (state && IsCreated)
			{
				cachedPtr = (ulong*)allocator.GetUnsafePtr(ptr);
			}
			else
			{
				cachedPtr = default;
			}
		}

		/// <summary>
		/// Sets all the bits in the bitmap to the specified value.
		/// </summary>
		/// <param name="value">The value to set each bit to.</param>
		/// <returns>The instance of the modified bitmap.</returns>
		[INLINE(256)]
		public void SetAllBits(in Allocator allocator, bool value)
		{
			var unsafePtr = (ulong*)allocator.GetUnsafePtr(in ptr);
			var len = Bitwise.GetLength(Length);
			var setValue = value ? ulong.MaxValue : ulong.MinValue;
			for (var index = 0; index < len; index++)
			{
				unsafePtr[index] = setValue;
			}
		}

		/// <summary>
		/// Gets the value of the bit at the specified index.
		/// </summary>
		/// <param name="index">The index of the bit.</param>
		/// <returns>The value of the bit at the specified index.</returns>
		[INLINE(256)]
		public bool IsSet(in Allocator allocator, int index)
		{
			E.RANGE(index, 0, Length);
			var unsafePtr = (ulong*)allocator.GetUnsafePtr(in ptr);
			return (unsafePtr[index / BITS_IN_ULONG] & (0x1ul << (index % BITS_IN_ULONG))) > 0;
		}

		/// <summary>
		/// Sets the value of the bit at the specified index to the specified value.
		/// </summary>
		/// <param name="allocator"></param>
		/// <param name="index">The index of the bit to set.</param>
		/// <param name="value">The value to set the bit to.</param>
		/// <returns>The instance of the modified bitmap.</returns>
		[INLINE(256)]
		public void Set(in Allocator allocator, int index, bool value)
		{
			E.RANGE(index, 0, Length);
			var unsafePtr = (ulong*)allocator.GetUnsafePtr(in ptr);
			if (value)
			{
				unsafePtr[index / BITS_IN_ULONG] |= 0x1ul << (index % BITS_IN_ULONG);
			}
			else
			{
				unsafePtr[index / BITS_IN_ULONG] &= ~(0x1ul << (index % BITS_IN_ULONG));
			}
		}

		/// <summary>
		/// Takes the union of this bitmap and the specified bitmap and stores the result in this
		/// instance.
		/// </summary>
		/// <param name="bitmap">The bitmap to union with this instance.</param>
		/// <returns>A reference to this instance.</returns>
		[INLINE(256)]
		public void Union(ref Allocator allocator, BitArray bitmap)
		{
			Resize(ref allocator, bitmap.Length > Length ? bitmap.Length : Length);
			E.RANGE(bitmap.Length - 1u, 0u, Length);
			var unsafePtr = (ulong*)allocator.GetUnsafePtr(in ptr);
			var otherPtr = (ulong*)allocator.GetUnsafePtr(in bitmap.ptr);
			var len = Bitwise.GetMinLength(bitmap.Length, Length);
			for (var index = 0u; index < len; ++index)
			{
				unsafePtr[index] |= otherPtr[index];
			}
		}

		/// <summary>
		/// Takes the intersection of this bitmap and the specified bitmap and stores the result in
		/// this instance.
		/// </summary>
		/// <param name="bitmap">The bitmap to intersect with this instance.</param>
		/// <returns>A reference to this instance.</returns>
		[INLINE(256)]
		public void Intersect(in Allocator allocator, BitArray bitmap)
		{
			E.RANGE(bitmap.Length - 1u, 0u, Length);
			var unsafePtr = (ulong*)allocator.GetUnsafePtr(in ptr);
			var otherPtr = (ulong*)allocator.GetUnsafePtr(in bitmap.ptr);
			var len = Bitwise.GetMinLength(bitmap.Length, Length);
			for (var index = 0u; index < len; ++index)
			{
				unsafePtr[index] &= otherPtr[index];
			}
		}

		[INLINE(256)]
		public void Remove(in Allocator allocator, BitArray bitmap)
		{
			var unsafePtr = (ulong*)allocator.GetUnsafePtr(in ptr);
			var otherPtr = (ulong*)allocator.GetUnsafePtr(in bitmap.ptr);
			var len = Bitwise.GetMinLength(bitmap.Length, Length);
			for (var index = 0u; index < len; ++index)
			{
				unsafePtr[index] &= ~otherPtr[index];
			}
		}
		/// <summary>
		/// Inverts all the bits in this bitmap.
		/// </summary>
		/// <returns>A reference to this instance.</returns>
		[INLINE(256)]
		public void Invert(in Allocator allocator)
		{
			var unsafePtr = (ulong*)allocator.GetUnsafePtr(in ptr);
			var len = Bitwise.GetLength(Length);
			for (var index = 0u; index < len; ++index)
			{
				unsafePtr[index] = ~unsafePtr[index];
			}
		}

		/// <summary>
		/// Sets a range of bits to the specified value.
		/// </summary>
		/// <param name="start">The index of the bit at the start of the range (inclusive).</param>
		/// <param name="end">The index of the bit at the end of the range (inclusive).</param>
		/// <param name="value">The value to set the bits to.</param>
		/// <returns>A reference to this instance.</returns>
		[INLINE(256)]
		public void SetRange(in Allocator allocator, int start, int end, bool value)
		{
			if (start == end)
			{
				Set(in allocator, start, value);
				return;
			}

			var unsafePtr = (ulong*)allocator.GetUnsafePtr(in ptr);
			var startBucket = start / BITS_IN_ULONG;
			var startOffset = start % BITS_IN_ULONG;
			var endBucket = end / BITS_IN_ULONG;
			var endOffset = end % BITS_IN_ULONG;

			if (value)
			{
				unsafePtr[startBucket] |= ulong.MaxValue << startOffset;
			}
			else
			{
				unsafePtr[startBucket] &= ~(ulong.MaxValue << startOffset);
			}

			for (var bucketIndex = startBucket + 1; bucketIndex < endBucket; bucketIndex++)
			{
				unsafePtr[bucketIndex] = value ? ulong.MaxValue : ulong.MinValue;
			}

			if (value)
			{
				unsafePtr[endBucket] |= ulong.MaxValue >> (BITS_IN_ULONG - endOffset - 1);
			}
			else
			{
				unsafePtr[endBucket] &= ~(ulong.MaxValue >> (BITS_IN_ULONG - endOffset - 1));
			}
		}

		[INLINE(256)]
		public void Clear(in Allocator allocator)
		{
			SetAllBits(in allocator, false);
		}

		[INLINE(256)]
		public void Dispose(ref Allocator allocator)
		{
			allocator.Free(ptr);
			this = default;
		}

		public uint GetReservedSizeInBytes()
		{
			var sizeInBytes = Bitwise.AlignULongBits(Length);
			return sizeInBytes;
		}
	}

	internal sealed unsafe class BitArrayDebugView
	{
		private BitArray data;

		public BitArrayDebugView(BitArray data)
		{
			this.data = data;
		}

		/*public bool[] Bits
		{
			get
			{
				var allocator = Context.world.state->allocator;
				var array = new bool[data.Length];
				for (var i = 0; i < data.Length; ++i)
				{
					array[i] = data.IsSet(in allocator, i);
				}

				return array;
			}
		}

		public int[] BitIndexes
		{
			get
			{
				var allocator = Context.world.state->allocator;
				var array = new System.Collections.Generic.List<int>((int)data.Length);
				for (var i = 0; i < data.Length; ++i)
				{
					if (data.IsSet(in allocator, i)) array.Add(i);
				}

				return array.ToArray();
			}
		}*/
	}
}
