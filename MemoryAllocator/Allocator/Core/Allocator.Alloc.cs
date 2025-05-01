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
			return ((size + BLOCK_ALIGN_MINUS_ONE) & ~BLOCK_ALIGN_MINUS_ONE).Max(MIN_BLOCK_SIZE);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllocatorPtr MemAlloc<T>()
			where T : unmanaged
		{
			return MemAlloc(TSize<T>.size, out _);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllocatorPtr MemAlloc<T>(out SafePtr<T> tValuePtr)
			where T : unmanaged
		{
			var result = MemAlloc(TSize<T>.size, out var valuePtr);
			tValuePtr = valuePtr;

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllocatorPtr MemAlloc<T>(in T value, out SafePtr<T> tValuePtr)
			where T : unmanaged
		{
			var result = MemAlloc(TSize<T>.size, out var valuePtr);
			tValuePtr = valuePtr;
			*tValuePtr.ptr = value;

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllocatorPtr MemAlloc(int size)
		{
			return MemAlloc(size, out _);
		}

		public AllocatorPtr MemAlloc(int size, out SafePtr valuePtr)
		{
			if (size == 0)
			{
				valuePtr = default;
				return AllocatorPtr.CreateZeroSized();
			}

			var blockSize = Align(size + TSize<MemoryBlock>.size);
			var blockRef = AllocateBlock(blockSize, out var blockPtr);

			E.ASSERT(blockPtr.ptr->blockSize >= TSize<MemoryBlock>.size + BLOCK_ALIGN);
			E.ASSERT(blockPtr.ptr->blockSize >= blockSize);

			var memPtr = new AllocatorPtr(blockRef.memoryZoneId, blockRef.memoryZoneOffset + TSize<MemoryBlock>.size);
			valuePtr = new SafePtr((blockPtr + 1).ptr, blockPtr.ptr->blockSize - TSize<MemoryBlock>.size);

			E.ASSERT(valuePtr.ptr - (byte*)blockPtr.ptr == TSize<MemoryBlock>.size);
			E.ASSERT(GetSafePtr(memPtr).ptr - TSize<MemoryBlock>.size == blockPtr.ptr);

			return memPtr;
		}

		public AllocatorPtr MemReAlloc(in AllocatorPtr worldPtr, int size, out SafePtr valuePtr)
		{
			if (!worldPtr.IsValid() || worldPtr.IsZeroSized())
				return MemAlloc(size, out valuePtr);

			var blockSize = Align(size + TSize<MemoryBlock>.size);
			var blockRef = (MemoryBlockRef)worldPtr;

			var newBlockRef = ReAllocateBlock(blockRef, blockSize, out var blockPtr);

			E.ASSERT(blockPtr.ptr->blockSize >= MIN_BLOCK_SIZE);
			E.ASSERT(blockPtr.ptr->blockSize >= blockSize);

			var newMemPtr = new AllocatorPtr(newBlockRef.memoryZoneId, newBlockRef.memoryZoneOffset + TSize<MemoryBlock>.size);
			valuePtr = new SafePtr((blockPtr + 1).ptr, blockPtr.ptr->blockSize - TSize<MemoryBlock>.size);

			E.ASSERT(valuePtr.ptr - (byte*)blockPtr.ptr == TSize<MemoryBlock>.size);
			E.ASSERT(GetSafePtr(newMemPtr).ptr - TSize<MemoryBlock>.size == blockPtr.ptr);

			return newMemPtr;
		}

		public bool MemFree(AllocatorPtr allocatorPtr)
		{
			if (!allocatorPtr.IsValid())
				return false;
			if (allocatorPtr.IsZeroSized())
				return true;

			var blockRef = (MemoryBlockRef)allocatorPtr;
			FreeBlock(blockRef);

			return true;
		}

		public void MemSwap(in AllocatorPtr a, int aOffset, in AllocatorPtr b, int bOffset, int length)
		{
			if (length == 0)
				return;

			var aRawPtr = GetSafePtr(a) + aOffset;
			var bRawPtr = GetSafePtr(b) + bOffset;

			MemoryExt.MemSwap(bRawPtr, aRawPtr, length);
		}

		public void MemCopy(in AllocatorPtr source, int sourceOffset, in AllocatorPtr dest, int destOffset, int length)
		{
			if (length == 0)
				return;

			var sourcePtr = GetSafePtr(source) + sourceOffset;
			var destPtr = GetSafePtr(dest) + destOffset;

			MemoryExt.MemCopy(sourcePtr, destPtr, length);
		}

		public void MemCopy<T>(in AllocatorPtr source, int sourceIndex, in AllocatorPtr dest, int destIndex, int length) where T : unmanaged
		{
			if (length == 0)
				return;

			var sourcePtr = GetSafePtr<T>(source) + sourceIndex;
			var destPtr = GetSafePtr<T>(dest) + destIndex;

			MemoryExt.MemCopy<T>(sourcePtr, destPtr, length);
		}

		public void MemMove(in AllocatorPtr source, int sourceOffset, in AllocatorPtr dest, int destOffset, int length)
		{
			if (length == 0)
				return;

			var sourcePtr = GetSafePtr(source) + sourceOffset;
			var destPtr = GetSafePtr(dest) + destOffset;

			MemoryExt.MemMove(sourcePtr, destPtr, length);
		}

		public void MemMove<T>(in AllocatorPtr source, int sourceIndex, in AllocatorPtr dest, int destIndex, int length) where T : unmanaged
		{
			if (length == 0)
				return;

			var sourcePtr = GetSafePtr<T>(source) + sourceIndex;
			var destPtr = GetSafePtr<T>(dest) + destIndex;

			MemoryExt.MemMove<T>(sourcePtr, destPtr, length);
		}

		public void MemFill<T>(in AllocatorPtr dest, in T value, int index, int length) where T : unmanaged
		{
			if (length == 0)
				return;

			var destPtr = GetSafePtr<T>(dest) + index;

			MemoryExt.MemFill<T>(value, destPtr, length);
		}

		public void MemClear(in AllocatorPtr dest, int index, int size)
		{
			if (size == 0)
				return;

			var destPtr = GetSafePtr(dest) + index;

			MemoryExt.MemClear(destPtr, size);
		}
	}
}
