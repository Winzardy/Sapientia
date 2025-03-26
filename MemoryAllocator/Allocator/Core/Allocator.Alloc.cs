using System.Runtime.CompilerServices;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Align(int size)
		{
			E.ASSERT(size > 0);
			return ((size + 3) & ~3).Max(MIN_BLOCK_SIZE);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr MemAlloc<T>()
			where T : unmanaged
		{
			return MemAlloc(TSize<T>.size, out _);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr MemAlloc<T>(out T* tValuePtr)
			where T : unmanaged
		{
			var result = MemAlloc(TSize<T>.size, out var valuePtr);
			tValuePtr = (T*)valuePtr;

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr MemAlloc<T>(in T value, out T* tValuePtr)
			where T : unmanaged
		{
			var result = MemAlloc(TSize<T>.size, out var valuePtr);
			tValuePtr = (T*)valuePtr;
			*tValuePtr = value;

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr MemAlloc(int size)
		{
			return MemAlloc(size, out _);
		}

		public MemPtr MemAlloc(int size, out byte* valuePtr)
		{
			if (size == 0)
			{
				valuePtr = (byte*)this.AsPointer();
				return MemPtr.CreateZeroSized(allocatorId);
			}

			var blockSize = Align(size + TSize<MemoryBlock>.size);
			var blockRef = AllocateBlock(blockSize, out var blockPtr);

			if (blockPtr->blockSize < MIN_BLOCK_SIZE)
				;
			E.ASSERT(blockPtr->blockSize >= MIN_BLOCK_SIZE);

			var result = new MemPtr(blockRef.memoryZoneId, blockRef.memoryZoneOffset + TSize<MemoryBlock>.size, allocatorId);
			valuePtr = (byte*)(blockPtr + 1);

			return result;
		}

		public MemPtr MemReAlloc(in MemPtr memPtr, int size, out byte* valuePtr)
		{
			if (!memPtr.IsValid() || memPtr.IsZeroSized())
				return MemAlloc(size, out valuePtr);

			var blockSize = Align(size + TSize<MemoryBlock>.size);
			var blockRef = ReAllocateBlock(memPtr, blockSize, out var blockPtr);

			E.ASSERT(blockPtr->blockSize >= MIN_BLOCK_SIZE);

			var result = new MemPtr(blockRef.memoryZoneId, blockRef.memoryZoneOffset + TSize<MemoryBlock>.size, allocatorId);
			valuePtr = (byte*)(blockPtr + 1);

			return result;
		}

		public bool MemFree(MemPtr memPtr)
		{
			if (!memPtr.IsValid())
				return false;
			if (memPtr.IsZeroSized())
				return true;

			FreeBlock(memPtr);

			return true;
		}

		public readonly void MemSwap(in MemPtr a, int aOffset, in MemPtr b, int bOffset, int length)
		{
			var aRawPtr = GetUnsafePtr(a) + aOffset;
			var bRawPtr = GetUnsafePtr(b) + bOffset;

			MemoryExt.MemSwap(bRawPtr, aRawPtr, length);
		}

		public readonly void MemCopy(in MemPtr source, int sourceOffset, in MemPtr dest, int destOffset, int length)
		{
			var sourceRawPtr = GetUnsafePtr(source) + sourceOffset;
			var destRawPtr = GetUnsafePtr(dest) + destOffset;
			MemoryExt.MemCopy(sourceRawPtr, destRawPtr, length);
		}

		public readonly void MemCopy<T>(in MemPtr source, int sourceIndex, in MemPtr dest, int destIndex, int length) where T : unmanaged
		{
			var sourceRawPtr = (T*)GetUnsafePtr(source) + sourceIndex;
			var destRawPtr = (T*)GetUnsafePtr(dest) + destIndex;
			MemoryExt.MemCopy(sourceRawPtr, destRawPtr, length);
		}

		public readonly void MemMove(in MemPtr source, int sourceOffset, in MemPtr dest, int destOffset, int length)
		{
			var sourceRawPtr = GetUnsafePtr(source) + sourceOffset;
			var destRawPtr = GetUnsafePtr(dest) + destOffset;
			MemoryExt.MemMove(sourceRawPtr, destRawPtr, length);
		}

		public readonly void MemMove<T>(in MemPtr source, int sourceIndex, in MemPtr dest, int destIndex, int length) where T : unmanaged
		{
			var sourceRawPtr = (T*)GetUnsafePtr(source) + sourceIndex;
			var destRawPtr = (T*)GetUnsafePtr(dest) + destIndex;
			MemoryExt.MemMove<T>(sourceRawPtr, destRawPtr, length);
		}

		public readonly void MemFill<T>(in MemPtr dest, in T value, int index, int length) where T : unmanaged
		{
			MemoryExt.MemFill<T>(value, (T*)GetUnsafePtr(dest) + index, length);
		}

		public readonly void MemClear(in MemPtr dest, int destOffset, int size)
		{
			MemoryExt.MemClear(GetUnsafePtr(dest) + destOffset, size);
		}
	}
}
