using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Submodules.Sapientia.Memory;

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
				valuePtr = default;
				return MemPtr.CreateZeroSized();
			}

			var requiredBlockSize = Align(size + TSize<MemoryBlock>.size);
			var blockRef = AllocateBlock(requiredBlockSize, out var blockPtr);

			E.ASSERT(blockPtr.ptr->blockSize >= TSize<MemoryBlock>.size + BLOCK_ALIGN, $"{nameof(MemAlloc)}. Выделенный размер блока меньше минимального.");
			E.ASSERT(blockPtr.ptr->blockSize >= requiredBlockSize, $"{nameof(MemAlloc)}. Выделенный размер блока меньше запрашиваемого.");

			var memPtr = new MemPtr(blockRef.memoryZoneId, blockRef.memoryZoneOffset + TSize<MemoryBlock>.size);
			valuePtr = new SafePtr((blockPtr + 1).ptr, blockPtr.ptr->blockSize - TSize<MemoryBlock>.size);

			E.ASSERT(valuePtr.ptr - (byte*)blockPtr.ptr == TSize<MemoryBlock>.size, $"{nameof(MemAlloc)}. Некорректный указатель на блок памяти.");
			E.ASSERT(GetSafePtr(memPtr).ptr - TSize<MemoryBlock>.size == blockPtr.ptr, $"{nameof(MemAlloc)}. Некорректное MemPtr на блок памяти.");

			return memPtr;
		}

		public MemPtr MemReAlloc(in MemPtr worldPtr, int size, out SafePtr valuePtr)
		{
			if (!worldPtr.IsValid() || worldPtr.IsZeroSized())
				return MemAlloc(size, out valuePtr);

			var requiredBlockSize = Align(size + TSize<MemoryBlock>.size);
			var blockRef = (MemoryBlockRef)worldPtr;

			var newBlockRef = ReAllocateBlock(blockRef, requiredBlockSize, out var blockPtr);

			E.ASSERT(blockPtr.ptr->blockSize >= MIN_BLOCK_SIZE, $"{nameof(MemReAlloc)}. Выделенный размер блока меньше минимального.");
			E.ASSERT(blockPtr.ptr->blockSize >= requiredBlockSize, $"{nameof(MemReAlloc)}. Выделенный размер блока меньше запрашиваемого.");

			var newMemPtr = new MemPtr(newBlockRef.memoryZoneId, newBlockRef.memoryZoneOffset + TSize<MemoryBlock>.size);
			valuePtr = new SafePtr((blockPtr + 1).ptr, blockPtr.ptr->blockSize - TSize<MemoryBlock>.size);

			E.ASSERT((valuePtr.ptr - (byte*)blockPtr.ptr) == TSize<MemoryBlock>.size, $"{nameof(MemReAlloc)}. Некорректный указатель на блок памяти.");
			E.ASSERT(GetSafePtr(newMemPtr).ptr - TSize<MemoryBlock>.size == blockPtr.ptr, $"{nameof(MemReAlloc)}. Некорректное MemPtr на блок памяти.");

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

		public void MemSwap(in MemPtr a, int aOffset, in MemPtr b, int bOffset, int length)
		{
			if (length == 0)
				return;

			var aRawPtr = GetSafePtr(a) + aOffset;
			var bRawPtr = GetSafePtr(b) + bOffset;

			MemoryExt.MemSwap(bRawPtr, aRawPtr, length);
		}

		public void MemCopy(in MemPtr source, int sourceOffset, in MemPtr dest, int destOffset, int length)
		{
			if (length == 0)
				return;

			var sourcePtr = GetSafePtr(source) + sourceOffset;
			var destPtr = GetSafePtr(dest) + destOffset;

			MemoryExt.MemCopy(sourcePtr, destPtr, length);
		}

		public void MemCopy<T>(in MemPtr source, int sourceIndex, in MemPtr dest, int destIndex, int length) where T : unmanaged
		{
			if (length == 0)
				return;

			var sourcePtr = GetSafePtr<T>(source) + sourceIndex;
			var destPtr = GetSafePtr<T>(dest) + destIndex;

			MemoryExt.MemCopy<T>(sourcePtr, destPtr, length);
		}

		public void MemMove(in MemPtr source, int sourceOffset, in MemPtr dest, int destOffset, int length)
		{
			if (length == 0)
				return;

			var sourcePtr = GetSafePtr(source) + sourceOffset;
			var destPtr = GetSafePtr(dest) + destOffset;

			MemoryExt.MemMove(sourcePtr, destPtr, length);
		}

		public void MemMove<T>(in MemPtr source, int sourceIndex, in MemPtr dest, int destIndex, int length) where T : unmanaged
		{
			if (length == 0)
				return;

			var sourcePtr = GetSafePtr<T>(source) + sourceIndex;
			var destPtr = GetSafePtr<T>(dest) + destIndex;

			MemoryExt.MemMove<T>(sourcePtr, destPtr, length);
		}

		public void MemFill<T>(in MemPtr dest, in T value, int index, int length) where T : unmanaged
		{
			if (length == 0)
				return;

			var destPtr = GetSafePtr<T>(dest) + index;

			MemoryExt.MemFill<T>(value, destPtr, length);
		}

		public void MemClear(in MemPtr dest, int index, int size)
		{
			if (size == 0)
				return;

			var destPtr = GetSafePtr(dest) + index;

			MemoryExt.MemClear(destPtr, size);
		}
	}
}
