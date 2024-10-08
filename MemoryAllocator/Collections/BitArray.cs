using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(BitArrayDebugView))]
	public unsafe struct BitArray
	{
		private const int _bitsInUlong = sizeof(ulong) * 8;

		public MemPtr ptr;
		public int length;
#if UNITY_5_3_OR_NEWER
		[Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
		private ulong* _cachedPtr;

		public bool IsCreated => ptr.IsValid();

		[INLINE(256)]
		public BitArray(Allocator* allocator, int length, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			var sizeInBytes = Bitwise.AlignULongBits(length);
			_cachedPtr = null;
			this.ptr = allocator->Alloc(sizeInBytes, out var ptr);
			_cachedPtr = (ulong*)ptr;
			this.length = length;

			if (clearOptions == ClearOptions.ClearMemory)
			{
				allocator->MemClear(this.ptr, 0u, sizeInBytes);
			}
		}

		[INLINE(256)]
		public BitArray(Allocator* allocator, BitArray source)
		{
			var sizeInBytes = Bitwise.AlignULongBits(source.length);
			_cachedPtr = null;
			this.ptr = allocator->Alloc(sizeInBytes, out var ptr);
			_cachedPtr = (ulong*)ptr;
			length = source.length;
			var sourcePtr = allocator->GetUnsafePtr(source.ptr);
			allocator->ValidateConsistency();
			MemoryExt.MemCopy(sourcePtr, ptr, sizeInBytes);
			allocator->ValidateConsistency();
		}

		[INLINE(256)]
		public void Set(Allocator* allocator, BitArray source)
		{
			var sizeInBytes = Bitwise.AlignULongBits(source.length);
			Resize(allocator, source.length);
			var sourcePtr = allocator->GetUnsafePtr(in source.ptr);
			MemoryExt.MemCopy(sourcePtr, allocator->GetUnsafePtr(in ptr), sizeInBytes);
		}

		[INLINE(256)]
		public bool ContainsAll(Allocator* allocator, BitArray other)
		{
			var len = Bitwise.GetMinLength(other.length, length);
			var unsafePtr = (ulong*)allocator->GetUnsafePtr(in ptr);
			var ptrOther = (ulong*)allocator->GetUnsafePtr(in other.ptr);
			for (var index = 0; index < len; index++)
			{
				if ((unsafePtr[index] & ptrOther[index]) != ptrOther[index]) return false;
			}

			return true;
		}

		[INLINE(256)]
		public void Resize(Allocator* allocator, int newLength, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			if (newLength > length)
			{
				ptr = allocator->ReAllocArray(in ptr, Bitwise.AlignULongBits(length), out _cachedPtr);
				if (clearOptions == ClearOptions.ClearMemory)
				{
					var clearSize = Bitwise.AlignULongBits(newLength - length);
					MemoryExt.MemClear((byte*)_cachedPtr + Bitwise.AlignULongBits(length), clearSize);
				}

				length = newLength;
			}
		}

		[INLINE(256)]
		public void BurstMode(Allocator* allocator, bool state)
		{
			if (state && IsCreated)
			{
				_cachedPtr = (ulong*)allocator->GetUnsafePtr(ptr);
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
		public void SetAllBits(Allocator* allocator, bool value)
		{
			var unsafePtr = (ulong*)allocator->GetUnsafePtr(in ptr);
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
		public bool IsSet(Allocator* allocator, int index)
		{
			E.RANGE(index, 0, length);
			var unsafePtr = (ulong*)allocator->GetUnsafePtr(in ptr);
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
		public void Set(Allocator* allocator, int index, bool value)
		{
			E.RANGE(index, 0, length);
			var unsafePtr = (ulong*)allocator->GetUnsafePtr(in ptr);
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
		public void Union(Allocator* allocator, BitArray bitmap)
		{
			Resize(allocator, bitmap.length > length ? bitmap.length : length);
			E.RANGE(bitmap.length - 1, 0, length);
			var unsafePtr = (ulong*)allocator->GetUnsafePtr(in ptr);
			var otherPtr = (ulong*)allocator->GetUnsafePtr(in bitmap.ptr);
			var len = Bitwise.GetMinLength(bitmap.length, length);
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
		public void Intersect(Allocator* allocator, BitArray bitmap)
		{
			E.RANGE(bitmap.length - 1, 0, length);
			var unsafePtr = (ulong*)allocator->GetUnsafePtr(in ptr);
			var otherPtr = (ulong*)allocator->GetUnsafePtr(in bitmap.ptr);
			var len = Bitwise.GetMinLength(bitmap.length, length);
			for (var index = 0u; index < len; ++index)
			{
				unsafePtr[index] &= otherPtr[index];
			}
		}

		[INLINE(256)]
		public void Remove(Allocator* allocator, BitArray bitmap)
		{
			var unsafePtr = (ulong*)allocator->GetUnsafePtr(in ptr);
			var otherPtr = (ulong*)allocator->GetUnsafePtr(in bitmap.ptr);
			var len = Bitwise.GetMinLength(bitmap.length, length);
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
		public void Invert(Allocator* allocator)
		{
			var unsafePtr = (ulong*)allocator->GetUnsafePtr(in ptr);
			var len = Bitwise.GetLength(length);
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
		public void SetRange(Allocator* allocator, int start, int end, bool value)
		{
			if (start == end)
			{
				Set(allocator, start, value);
				return;
			}

			var unsafePtr = (ulong*)allocator->GetUnsafePtr(in ptr);
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
		public void Clear(Allocator* allocator)
		{
			SetAllBits(allocator, false);
		}

		[INLINE(256)]
		public void Dispose(Allocator* allocator)
		{
			allocator->Free(ptr);
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
