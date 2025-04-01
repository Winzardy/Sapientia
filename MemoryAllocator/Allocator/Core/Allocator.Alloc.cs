using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Align(int size)
		{
			E.ASSERT(size > 0);
			return (size + MIN_BLOCK_ALIGN) & ~MIN_BLOCK_ALIGN;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr MemAlloc<T>()
			where T : unmanaged
		{
			return MemAlloc(TSize<T>.size, out _);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr MemAlloc<T>(out SafePtr<T> tValuePtr)
			where T : unmanaged
		{
			var result = MemAlloc(TSize<T>.size, out var valuePtr);
			tValuePtr = valuePtr;

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr MemAlloc<T>(in T value, out SafePtr<T> tValuePtr)
			where T : unmanaged
		{
			var result = MemAlloc(TSize<T>.size, out var valuePtr);
			tValuePtr = valuePtr;
			*tValuePtr.ptr = value;

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr MemAlloc(int size)
		{
			return MemAlloc(size, out _);
		}

		public MemPtr MemAlloc(int size, out SafePtr valuePtr)
		{
			if (size == 0)
			{
				valuePtr = this.AsSafePtr();
				return MemPtr.CreateZeroSized(allocatorId);
			}

			var blockSize = Align(size + TSize<MemoryBlock>.size);
			var blockRef = AllocateBlock(blockSize, out var blockPtr);

			E.ASSERT(blockPtr.ptr->blockSize >= MIN_BLOCK_SIZE);
			E.ASSERT(blockPtr.ptr->blockSize >= blockSize);

			var memPtr = new MemPtr(blockRef.memoryZoneId, blockRef.memoryZoneOffset + TSize<MemoryBlock>.size, allocatorId);
			valuePtr = new SafePtr((blockPtr + 1).ptr, blockPtr.ptr->blockSize - TSize<MemoryBlock>.size);

			E.ASSERT(valuePtr.ptr - (byte*)blockPtr.ptr == TSize<MemoryBlock>.size);
			E.ASSERT(GetSafePtr(memPtr).ptr - TSize<MemoryBlock>.size == blockPtr.ptr);

			return memPtr;
		}

		public MemPtr MemReAlloc(in MemPtr memPtr, int size, out SafePtr valuePtr)
		{
			if (!memPtr.IsCreated() || memPtr.IsZeroSized())
				return MemAlloc(size, out valuePtr);

			var blockSize = Align(size + TSize<MemoryBlock>.size);
			var blockRef = (MemoryBlockRef)memPtr;

			var newBlockRef = ReAllocateBlock(blockRef, blockSize, out var blockPtr);

			E.ASSERT(blockPtr.ptr->blockSize >= MIN_BLOCK_SIZE);
			E.ASSERT(blockPtr.ptr->blockSize >= blockSize);

			var newMemPtr = new MemPtr(newBlockRef.memoryZoneId, newBlockRef.memoryZoneOffset + TSize<MemoryBlock>.size, allocatorId);
			valuePtr = new SafePtr((blockPtr + 1).ptr, blockPtr.ptr->blockSize - TSize<MemoryBlock>.size);

			E.ASSERT(valuePtr.ptr - (byte*)blockPtr.ptr == TSize<MemoryBlock>.size);
			E.ASSERT(GetSafePtr(newMemPtr).ptr - TSize<MemoryBlock>.size == blockPtr.ptr);

			return newMemPtr;
		}

		public bool MemFree(MemPtr memPtr)
		{
			if (!memPtr.IsCreated())
				return false;
			if (memPtr.IsZeroSized())
				return true;

			var blockRef = (MemoryBlockRef)memPtr;
			FreeBlock(blockRef);

			return true;
		}

		public readonly void MemSwap(in MemPtr a, int aOffset, in MemPtr b, int bOffset, int length)
		{
			if (length == 0)
				return;

			var aRawPtr = GetSafePtr(a) + aOffset;
			var bRawPtr = GetSafePtr(b) + bOffset;

			MemoryExt.MemSwap(bRawPtr, aRawPtr, length);
		}

		public readonly void MemCopy(in MemPtr source, int sourceOffset, in MemPtr dest, int destOffset, int length)
		{
			if (length == 0)
				return;

			var sourcePtr = GetSafePtr(source) + sourceOffset;
			var destPtr = GetSafePtr(dest) + destOffset;

			MemoryExt.MemCopy(sourcePtr, destPtr, length);
		}

		public readonly void MemCopy<T>(in MemPtr source, int sourceIndex, in MemPtr dest, int destIndex, int length) where T : unmanaged
		{
			if (length == 0)
				return;

			var sourcePtr = GetSafePtr<T>(source) + sourceIndex;
			var destPtr = GetSafePtr<T>(dest) + destIndex;

			MemoryExt.MemCopy<T>(sourcePtr, destPtr, length);
		}

		public readonly void MemMove(in MemPtr source, int sourceOffset, in MemPtr dest, int destOffset, int length)
		{
			if (length == 0)
				return;

			var sourcePtr = GetSafePtr(source) + sourceOffset;
			var destPtr = GetSafePtr(dest) + destOffset;

			MemoryExt.MemMove(sourcePtr, destPtr, length);
		}

		public readonly void MemMove<T>(in MemPtr source, int sourceIndex, in MemPtr dest, int destIndex, int length) where T : unmanaged
		{
			if (length == 0)
				return;

			var sourcePtr = GetSafePtr<T>(source) + sourceIndex;
			var destPtr = GetSafePtr<T>(dest) + destIndex;

			MemoryExt.MemMove<T>(sourcePtr, destPtr, length);
		}

		public readonly void MemFill<T>(in MemPtr dest, in T value, int index, int length) where T : unmanaged
		{
			if (length == 0)
				return;

			var destPtr = GetSafePtr<T>(dest) + index;

			MemoryExt.MemFill<T>(value, destPtr, length);
		}

		public readonly void MemClear(in MemPtr dest, int index, int size)
		{
			if (size == 0)
				return;

			var destPtr = GetSafePtr(dest) + index;

			MemoryExt.MemClear(destPtr, size);
		}
	}
}
