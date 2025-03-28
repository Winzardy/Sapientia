using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(BitArrayDebugView))]
	public unsafe struct BitArray
	{
		private const int _bitsInUlong = sizeof(ulong) * 8;

		public MemPtr memPtr;
		public int length;
#if UNITY_5_3_OR_NEWER
		[Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
		private SafePtr<ulong> _cachedPtr;

		public bool IsCreated => memPtr.IsValid();

		[INLINE(256)]
		public BitArray(SafePtr<Allocator> allocator, int length, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			var sizeInBytes = Bitwise.AlignULongBits(length);
			_cachedPtr = default;
			this.memPtr = allocator.Value().MemAlloc(sizeInBytes, out var safePtr);
			_cachedPtr = safePtr;
			this.length = length;

			if (clearOptions == ClearOptions.ClearMemory)
			{
				allocator.Value().MemClear(this.memPtr, 0, sizeInBytes);
			}
		}

		[INLINE(256)]
		public BitArray(SafePtr<Allocator> allocator, BitArray source)
		{
			var sizeInBytes = Bitwise.AlignULongBits(source.length);
			_cachedPtr = default;
			this.memPtr = allocator.Value().MemAlloc(sizeInBytes, out var safePtr);
			_cachedPtr = safePtr;
			length = source.length;
			var sourcePtr = allocator.Value().GetSafePtr(source.memPtr);

			MemoryExt.MemCopy(sourcePtr.ptr, safePtr.ptr, sizeInBytes);
		}

		[INLINE(256)]
		public void Set(SafePtr<Allocator> allocator, BitArray source)
		{
			var sizeInBytes = Bitwise.AlignULongBits(source.length);
			Resize(allocator, source.length);
			var sourcePtr = allocator.Value().GetSafePtr(in source.memPtr);

			MemoryExt.MemCopy(sourcePtr.ptr, allocator.Value().GetSafePtr(in memPtr).ptr, sizeInBytes);
		}

		[INLINE(256)]
		public bool ContainsAll(SafePtr<Allocator> allocator, BitArray other)
		{
			var len = Bitwise.GetMinLength(other.length, length);
			var unsafePtr = allocator.Value().GetSafePtr<ulong>(in memPtr);
			var ptrOther = allocator.Value().GetSafePtr<ulong>(in other.memPtr);
			for (var index = 0; index < len; index++)
			{
				if ((unsafePtr[index] & ptrOther[index]) != ptrOther[index])
					return false;
			}

			return true;
		}

		[INLINE(256)]
		public void Resize(SafePtr<Allocator> allocator, int newLength, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			if (newLength > length)
			{
				memPtr = allocator.Value().MemReAlloc(in memPtr, TSize<ulong>.size * Bitwise.AlignULongBits(length), out var rawPtr);
				_cachedPtr = rawPtr;

				if (clearOptions == ClearOptions.ClearMemory)
				{
					var clearSize = Bitwise.AlignULongBits(newLength - length);
					MemoryExt.MemClear((byte*)_cachedPtr.ptr + Bitwise.AlignULongBits(length), clearSize);
				}

				length = newLength;
			}
		}

		[INLINE(256)]
		public void BurstMode(SafePtr<Allocator> allocator, bool state)
		{
			if (state && IsCreated)
			{
				_cachedPtr = allocator.Value().GetSafePtr<ulong>(memPtr);
			}
			else
			{
				_cachedPtr = default;
			}
		}

		/// <summary>
		/// Sets all the bits in the bitmap to the specified value.
		/// </summary>
		/// <param name="value">The value to set each bit to.</param>
		/// <returns>The instance of the modified bitmap.</returns>
		[INLINE(256)]
		public void SetAllBits(SafePtr<Allocator> allocator, bool value)
		{
			var unsafePtr = allocator.Value().GetSafePtr<ulong>(in memPtr);
			var len = Bitwise.GetLength(length);
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
		public bool IsSet(SafePtr<Allocator> allocator, int index)
		{
			E.RANGE(index, 0, length);
			var unsafePtr = allocator.Value().GetSafePtr<ulong>(in memPtr);
			return (unsafePtr[index / _bitsInUlong] & (0x1ul << (index % _bitsInUlong))) > 0;
		}

		/// <summary>
		/// Sets the value of the bit at the specified index to the specified value.
		/// </summary>
		/// <param name="allocator"></param>
		/// <param name="index">The index of the bit to set.</param>
		/// <param name="value">The value to set the bit to.</param>
		/// <returns>The instance of the modified bitmap.</returns>
		[INLINE(256)]
		public void Set(SafePtr<Allocator> allocator, int index, bool value)
		{
			E.RANGE(index, 0, length);
			var unsafePtr = allocator.Value().GetSafePtr<ulong>(in memPtr);
			if (value)
			{
				unsafePtr[index / _bitsInUlong] |= 0x1ul << (index % _bitsInUlong);
			}
			else
			{
				unsafePtr[index / _bitsInUlong] &= ~(0x1ul << (index % _bitsInUlong));
			}
		}

		/// <summary>
		/// Takes the union of this bitmap and the specified bitmap and stores the result in this
		/// instance.
		/// </summary>
		/// <param name="bitmap">The bitmap to union with this instance.</param>
		/// <returns>A reference to this instance.</returns>
		[INLINE(256)]
		public void Union(SafePtr<Allocator> allocator, BitArray bitmap)
		{
			Resize(allocator, bitmap.length > length ? bitmap.length : length);
			E.RANGE(bitmap.length - 1, 0, length);
			var unsafePtr = allocator.Value().GetSafePtr<ulong>(in memPtr);
			var otherPtr = allocator.Value().GetSafePtr<ulong>(in bitmap.memPtr);
			var len = Bitwise.GetMinLength(bitmap.length, length);

			for (var index = 0; index < len; ++index)
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
		public void Intersect(SafePtr<Allocator> allocator, BitArray bitmap)
		{
			E.RANGE(bitmap.length - 1, 0, length);
			var unsafePtr = allocator.Value().GetSafePtr<ulong>(in memPtr);
			var otherPtr = allocator.Value().GetSafePtr<ulong>(in bitmap.memPtr);
			var len = Bitwise.GetMinLength(bitmap.length, length);
			for (var index = 0; index < len; ++index)
			{
				unsafePtr[index] &= otherPtr[index];
			}
		}

		[INLINE(256)]
		public void Remove(SafePtr<Allocator> allocator, BitArray bitmap)
		{
			var unsafePtr = allocator.Value().GetSafePtr<ulong>(in memPtr);
			var otherPtr = allocator.Value().GetSafePtr<ulong>(in bitmap.memPtr);
			var len = Bitwise.GetMinLength(bitmap.length, length);
			for (var index = 0; index < len; ++index)
			{
				unsafePtr[index] &= ~otherPtr[index];
			}
		}
		/// <summary>
		/// Inverts all the bits in this bitmap.
		/// </summary>
		/// <returns>A reference to this instance.</returns>
		[INLINE(256)]
		public void Invert(SafePtr<Allocator> allocator)
		{
			var unsafePtr = allocator.Value().GetSafePtr<ulong>(in memPtr);
			var len = Bitwise.GetLength(length);
			for (var index = 0; index < len; ++index)
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
		public void SetRange(SafePtr<Allocator> allocator, int start, int end, bool value)
		{
			if (start == end)
			{
				Set(allocator, start, value);
				return;
			}

			var unsafePtr = allocator.Value().GetSafePtr<ulong>(in memPtr);
			var startBucket = start / _bitsInUlong;
			var startOffset = start % _bitsInUlong;
			var endBucket = end / _bitsInUlong;
			var endOffset = end % _bitsInUlong;

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
				unsafePtr[endBucket] |= ulong.MaxValue >> (_bitsInUlong - endOffset - 1);
			}
			else
			{
				unsafePtr[endBucket] &= ~(ulong.MaxValue >> (_bitsInUlong - endOffset - 1));
			}
		}

		[INLINE(256)]
		public void Clear(SafePtr<Allocator> allocator)
		{
			SetAllBits(allocator, false);
		}

		[INLINE(256)]
		public void Dispose(SafePtr<Allocator> allocator)
		{
			allocator.Value().MemFree(memPtr);
			this = default;
		}

		public int GetReservedSizeInBytes()
		{
			var sizeInBytes = Bitwise.AlignULongBits(length);
			return sizeInBytes;
		}
	}

	internal sealed unsafe class BitArrayDebugView
	{
		private BitArray _data;

		public BitArrayDebugView(BitArray data)
		{
			this._data = data;
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

		public int[] BitIndexes
		{
			get
			{
				var allocator = AllocatorManager.CurrentAllocatorPtr;
				var array = new System.Collections.Generic.List<int>(_data.length);
				for (var i = 0; i < _data.length; ++i)
				{
					if (_data.IsSet(allocator, i)) array.Add(i);
				}

				return array.ToArray();
			}
		}
	}
}
