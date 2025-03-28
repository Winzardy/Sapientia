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
				valuePtr = new SafePtr(this.AsPointer());
				return MemPtr.CreateZeroSized(allocatorId);
			}

			var blockSize = Align(size + TSize<MemoryBlock>.size);
			var blockRef = AllocateBlock(blockSize, out var blockPtr);

			E.ASSERT(blockPtr->blockSize >= MIN_BLOCK_SIZE);
			E.ASSERT(blockPtr->blockSize >= blockSize);

			var memPtr = new MemPtr(blockRef.memoryZoneId, blockRef.memoryZoneOffset + TSize<MemoryBlock>.size, allocatorId);
			valuePtr = new SafePtr((blockPtr + 1), blockPtr->blockSize);

			E.ASSERT(valuePtr.ptr - (byte*)blockPtr == TSize<MemoryBlock>.size);
			E.ASSERT(GetUnsafePtr(memPtr) == valuePtr.ptr);
			E.ASSERT(GetUnsafePtr(memPtr) - TSize<MemoryBlock>.size == blockPtr);

			return memPtr;
		}

		public MemPtr MemReAlloc(in MemPtr memPtr, int size, out SafePtr valuePtr)
		{
			if (!memPtr.IsValid() || memPtr.IsZeroSized())
				return MemAlloc(size, out valuePtr);

			var blockSize = Align(size + TSize<MemoryBlock>.size);
			var blockRef = (MemoryBlockRef)memPtr;

			var newBlockRef = ReAllocateBlock(blockRef, blockSize, out var blockPtr);

			E.ASSERT(blockPtr->blockSize >= MIN_BLOCK_SIZE);
			E.ASSERT(blockPtr->blockSize >= blockSize);

			var newMemPtr = new MemPtr(newBlockRef.memoryZoneId, newBlockRef.memoryZoneOffset + TSize<MemoryBlock>.size, allocatorId);
			valuePtr = new SafePtr((blockPtr + 1), blockPtr->blockSize);

			E.ASSERT(valuePtr.ptr - (byte*)blockPtr == TSize<MemoryBlock>.size);
			E.ASSERT(GetUnsafePtr(newMemPtr) == valuePtr.ptr);
			E.ASSERT(GetUnsafePtr(newMemPtr) - TSize<MemoryBlock>.size == blockPtr);

			return newMemPtr;
		}

		public bool MemFree(MemPtr memPtr)
		{
			if (!memPtr.IsValid())
				return false;
			if (memPtr.IsZeroSized())
				return true;

			var blockRef = (MemoryBlockRef)memPtr;
			FreeBlock(blockRef);

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
			var sourceRawPtr = GetSafePtr(source) + sourceOffset;
			var destRawPtr = GetSafePtr(dest) + destOffset;

			MemoryExt.MemCopy(sourceRawPtr.ptr, destRawPtr.ptr, length);
		}

		public readonly void MemCopy<T>(in MemPtr source, int sourceIndex, in MemPtr dest, int destIndex, int length) where T : unmanaged
		{
			var sourceRawPtr = GetSafePtr<T>(source) + sourceIndex;
			var destRawPtr = GetSafePtr<T>(dest) + destIndex;

			MemoryExt.MemCopy(sourceRawPtr.ptr, destRawPtr.ptr, length);
		}

		public readonly void MemMove(in MemPtr source, int sourceOffset, in MemPtr dest, int destOffset, int length)
		{
			var sourceRawPtr = GetSafePtr(source) + sourceOffset;
			var destRawPtr = GetSafePtr(dest) + destOffset;

			MemoryExt.MemMove(sourceRawPtr.ptr, destRawPtr.ptr, length);
		}

		public readonly void MemMove<T>(in MemPtr source, int sourceIndex, in MemPtr dest, int destIndex, int length) where T : unmanaged
		{
			var sourceRawPtr = GetSafePtr<T>(source) + sourceIndex;
			var destRawPtr = GetSafePtr<T>(dest) + destIndex;

			MemoryExt.MemMove<T>(sourceRawPtr.ptr, destRawPtr.ptr, length);
		}

		public readonly void MemFill<T>(in MemPtr dest, in T value, int index, int length) where T : unmanaged
		{
			MemoryExt.MemFill<T>(value, (GetSafePtr<T>(dest) + index).ptr, length);
		}

		public readonly void MemClear(in MemPtr dest, int destOffset, int size)
		{
			MemoryExt.MemClear((GetSafePtr(dest) + destOffset).ptr, size);
		}
	}
}
