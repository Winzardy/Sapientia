using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(BitArrayDebugView))]
	public unsafe struct BitArray
	{
		private const int _bitsInUlong = sizeof(ulong) * 8;

		public CachedPtr<ulong> ptr;
		public int length;

		public bool IsCreated => ptr.IsValid();

		[INLINE(256)]
		public SafePtr<ulong> GetPtr(World world)
		{
			return ptr.GetPtr(world);
		}

		[INLINE(256)]
		public BitArray(World world, int length, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			var sizeInBytes = Bitwise.AlignULongBits(length);

			ptr = new CachedPtr<ulong>(world, world.MemAlloc(sizeInBytes, out var safePtr), safePtr);
			this.length = length;

			if (clearOptions == ClearOptions.ClearMemory)
			{
				world.MemClear(ptr.memPtr, 0, sizeInBytes);
			}
		}

		[INLINE(256)]
		public BitArray(World world, BitArray source)
		{
			var sizeInBytes = Bitwise.AlignULongBits(source.length);

			ptr = new CachedPtr<ulong>(world, world.MemAlloc(sizeInBytes, out var safePtr), safePtr);
			length = source.length;

			MemoryExt.MemCopy(source.ptr.GetPtr(world).Cast<byte>(), safePtr.Cast<byte>(), sizeInBytes);
		}

		[INLINE(256)]
		public void Set(World world, BitArray source)
		{
			var sizeInBytes = Bitwise.AlignULongBits(source.length);
			Resize(world, source.length);

			MemoryExt.MemCopy(source.ptr.GetPtr(world).Cast<byte>(), ptr.GetPtr(world).Cast<byte>(), sizeInBytes);
		}

		[INLINE(256)]
		public bool ContainsAll(World world, BitArray other)
		{
			var len = Bitwise.GetMinLength(other.length, length);
			var unsafePtr = ptr.GetPtr(world);
			var ptrOther = other.ptr.GetPtr(world);
			for (var index = 0; index < len; index++)
			{
				if ((unsafePtr[index] & ptrOther[index]) != ptrOther[index])
					return false;
			}

			return true;
		}

		[INLINE(256)]
		public void Resize(World world, int newLength, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			if (newLength > length)
			{
				var memPtr = world.MemReAlloc(ptr.memPtr, TSize<ulong>.size * Bitwise.AlignULongBits(length), out var rawPtr);
				ptr = new CachedPtr<ulong>(world, memPtr, rawPtr);

				if (clearOptions == ClearOptions.ClearMemory)
				{
					var clearSize = Bitwise.AlignULongBits(newLength - length);
					MemoryExt.MemClear((rawPtr.Cast<ulong>() + Bitwise.AlignULongBits(length)), clearSize);
				}

				length = newLength;
			}
		}

		/// <summary>
		/// Sets all the bits in the bitmap to the specified value.
		/// </summary>
		/// <param name="value">The value to set each bit to.</param>
		/// <returns>The instance of the modified bitmap.</returns>
		[INLINE(256)]
		public void SetAllBits(World world, bool value)
		{
			var unsafePtr = ptr.GetPtr(world);
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
		/// <param name="worldator"></param>
		/// <param name="index">The index of the bit.</param>
		/// <returns>The value of the bit at the specified index.</returns>
		[INLINE(256)]
		public bool IsSet(World world, int index)
		{
			E.RANGE(index, 0, length);
			var unsafePtr = ptr.GetPtr(world);
			return (unsafePtr[index / _bitsInUlong] & (0x1ul << (index % _bitsInUlong))) > 0;
		}

		/// <summary>
		/// Gets the value of the bit at the specified index.
		/// </summary>
		/// <returns>The value of the bit at the specified index.</returns>
		[INLINE(256)]
		public static bool IsSet(SafePtr<ulong> unsafePtr, int index)
		{
			return (unsafePtr[index / _bitsInUlong] & (0x1ul << (index % _bitsInUlong))) > 0;
		}

		/// <summary>
		/// Sets the value of the bit at the specified index to the specified value.
		/// </summary>
		/// <param name="worldator"></param>
		/// <param name="index">The index of the bit to set.</param>
		/// <param name="value">The value to set the bit to.</param>
		/// <returns>The instance of the modified bitmap.</returns>
		[INLINE(256)]
		public void Set(World world, int index, bool value)
		{
			E.RANGE(index, 0, length);
			var unsafePtr = ptr.GetPtr(world);
			if (value)
			{
				unsafePtr[index / _bitsInUlong] |= 0x1ul << (index % _bitsInUlong);
			}
			else
			{
				unsafePtr[index / _bitsInUlong] &= ~(0x1ul << (index % _bitsInUlong));
			}
		}

		[INLINE(256)]
		public static void Add(SafePtr<ulong> unsafePtr, int index)
		{
			unsafePtr[index / _bitsInUlong] |= 0x1ul << (index % _bitsInUlong);
		}

		[INLINE(256)]
		public static void Remove(SafePtr<ulong> unsafePtr, int index)
		{
			unsafePtr[index / _bitsInUlong] &= ~(0x1ul << (index % _bitsInUlong));
		}

		/// <summary>
		/// Takes the union of this bitmap and the specified bitmap and stores the result in this
		/// instance.
		/// </summary>
		/// <param name="bitmap">The bitmap to union with this instance.</param>
		/// <returns>A reference to this instance.</returns>
		[INLINE(256)]
		public void Union(World world, BitArray bitmap)
		{
			Resize(world, bitmap.length > length ? bitmap.length : length);
			E.RANGE(bitmap.length - 1, 0, length);
			var unsafePtr = ptr.GetPtr(world);
			var otherPtr = bitmap.ptr.GetPtr(world);
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
		public void Intersect(World world, BitArray bitmap)
		{
			E.RANGE(bitmap.length - 1, 0, length);
			var unsafePtr = ptr.GetPtr(world);
			var otherPtr = bitmap.ptr.GetPtr(world);
			var len = Bitwise.GetMinLength(bitmap.length, length);
			for (var index = 0; index < len; ++index)
			{
				unsafePtr[index] &= otherPtr[index];
			}
		}

		[INLINE(256)]
		public void Remove(World world, BitArray bitmap)
		{
			var unsafePtr = ptr.GetPtr(world);
			var otherPtr = bitmap.ptr.GetPtr(world);
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
		public void Invert(World world)
		{
			var unsafePtr = ptr.GetPtr(world);
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
		public void SetRange(World world, int start, int end, bool value)
		{
			if (start == end)
			{
				Set(world, start, value);
				return;
			}

			var unsafePtr = ptr.GetPtr(world);
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
		public void Clear(World world)
		{
			SetAllBits(world, false);
		}

		[INLINE(256)]
		public void Dispose(World world)
		{
			world.MemFree(ptr.memPtr);
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
				var world = WorldManager.CurrentWorld;
				var array = new bool[_data.length];
				for (var i = 0; i < _data.length; ++i)
				{
					array[i] = _data.IsSet(world, i);
				}

				return array;
			}
		}

		public int[] BitIndexes
		{
			get
			{
				var world = WorldManager.CurrentWorld;
				var array = new System.Collections.Generic.List<int>(_data.length);
				for (var i = 0; i < _data.length; ++i)
				{
					if (_data.IsSet(world, i)) array.Add(i);
				}

				return array.ToArray();
			}
		}
	}
}
