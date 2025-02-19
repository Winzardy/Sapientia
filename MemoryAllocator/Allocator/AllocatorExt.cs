using System;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Data;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{

#if BURST
	[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
	public static unsafe class AllocatorExt
	{
		[INLINE(256)]
		public static byte* GetUnsafePtr(this in Allocator allocator, in MemPtr ptr)
		{
#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			if (ptr.zoneId < allocator.zonesListCount && allocator.zonesList[ptr.zoneId] != null &&
			    allocator.zonesList[ptr.zoneId]->size < ptr.offset)
			{
				throw new System.Exception();
			}
#endif

			return (byte*)allocator.zonesList[ptr.zoneId] + ptr.offset;
		}

		[INLINE(256)]
		public static byte* GetUnsafePtr(this in Allocator allocator, in MemPtr ptr, uint offset)
		{
			return (byte*)allocator.zonesList[ptr.zoneId] + ptr.offset + offset;
		}

		[INLINE(256)]
		public static byte* GetUnsafePtr(this in Allocator allocator, in MemPtr ptr, long offset)
		{
			return (byte*)allocator.zonesList[ptr.zoneId] + ptr.offset + offset;
		}

#if BURST
		[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
		[INLINE(256)]
		public static MemPtr ReAlloc(this ref Allocator allocator, in MemPtr ptr, int size)
		{
			return ReAlloc(ref allocator, ptr, size, out _);
		}

#if BURST
		[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
		[INLINE(256)]
		public static MemPtr ReAlloc(this ref Allocator allocator, in MemPtr ptr, int size, out void* voidPtr)
		{
			size = Allocator.Align(size);

			if (!ptr.IsValid())
				return Alloc(ref allocator, size, out voidPtr);

			ValidateConsistency(ref allocator);
			allocator.locker.SetBusyIgnoreThread();

			voidPtr = GetUnsafePtr(allocator, ptr);
			var block = (MemBlock*)((byte*)voidPtr - TSize<MemBlock>.size);
			var blockSize = block->size;
			var blockDataSize = blockSize - TSize<MemBlock>.size;
			if (blockDataSize > size)
			{
				allocator.locker.SetFreeIgnoreThread();
				ValidateConsistency(ref allocator);
				return ptr;
			}

			if (blockDataSize < 0)
			{
				allocator.locker.SetFreeIgnoreThread();
				ValidateConsistency(ref allocator);
				throw new System.Exception();
			}

			{
				var zone = allocator.zonesList[ptr.zoneId];
				var nextBlock = block->next.Ptr(zone);
				var requiredSize = size - blockDataSize;
				// next block is free and its size is enough for current size
				if (nextBlock != null &&
				    nextBlock->state == Allocator.BLOCK_STATE_FREE &&
				    nextBlock->size - TSize<MemBlock>.size > requiredSize)
				{
					// mark current block as free
					// freePrev is false because it must not collapse block with previous one
					// [!] may be we need to add case, which move data on collapse
					if (!Allocator.MzFree(zone, (byte*)block + TSize<MemBlock>.size, freePrev: false))
					{
						// Something went wrong
						allocator.locker.SetFreeIgnoreThread();
						ValidateConsistency(ref allocator);
						throw new System.Exception();
					}

					// alloc block again
					var newPtr = Allocator.MzAlloc(zone, block, size + TSize<MemBlock>.size);
#if MEMORY_ALLOCATOR_BOUNDS_CHECK
					{
						var memPtr = allocator.GetSafePtr(newPtr, ptr.zoneId);
						if (memPtr != ptr)
						{
							// Something went wrong
							allocator.locker.SetFreeIgnoreThread();
							throw new System.Exception();
						}
					}
#endif
					voidPtr = newPtr;
					allocator.locker.SetFreeIgnoreThread();
					ValidateConsistency(ref allocator);
					return ptr;
				}
			}

			allocator.locker.SetFreeIgnoreThread();
			ValidateConsistency(ref allocator);

			{
				var newPtr = Alloc(ref allocator, size, out voidPtr);
				allocator.MemMove(newPtr, 0, ptr, 0, blockDataSize);
				allocator.Free(ptr);

				return newPtr;
			}
		}

#if BURST
		[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
		[INLINE(256)]
		public static MemPtr Alloc(this ref Allocator allocator, long size)
		{
			return Alloc(ref allocator, size, out _);
		}

		[System.Diagnostics.ConditionalAttribute(COND.ALLOCATOR_VALIDATION)]
		public static void ValidateConsistency(this ref Allocator allocator)
		{
			CheckConsistency(ref allocator);
		}

		public static void CheckConsistency(this ref Allocator allocator)
		{
			allocator.locker.SetBusyIgnoreThread();

			for (var i = 0; i < allocator.zonesListCount; ++i)
			{
				var zone = allocator.zonesList[i];
				if (zone == null)
				{
					continue;
				}

				if (!Allocator.MzCheckHeap(zone, out var blockIndex, out var index))
				{
#if UNITY_5_3_OR_NEWER
					UnityEngine.Debug.LogError($"zone {i}, block {blockIndex}, index {index}, thread {Unity.Jobs.LowLevel.Unsafe.JobsUtility.ThreadIndex}");
#else
					System.Diagnostics.Debug.WriteLine($"zone {i}, block {blockIndex}, index {index}, thread {System.Threading.Thread.CurrentThread.ManagedThreadId}");
#endif
				}
			}

			allocator.locker.SetFreeIgnoreThread();
		}

#if BURST
		[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
		[INLINE(256)]
		public static MemPtr Alloc(this ref Allocator allocator, long size, out void* ptr)
		{
			size = Allocator.Align(size);

			ValidateConsistency(ref allocator);

			allocator.locker.SetBusyIgnoreThread();

			for (int i = 0, cnt = allocator.zonesListCount; i < cnt; ++i)
			{
				var zone = allocator.zonesList[i];
				if (zone == null)
					continue;

				ptr = Allocator.MzMalloc(zone, (int)size);
				if (ptr != null)
				{
					var memPtr = allocator.GetSafePtr(ptr, i);
#if LOGS_ENABLED
					Allocator.LogAdd(memPtr, size);
#endif
					allocator.locker.SetFreeIgnoreThread();
					ValidateConsistency(ref allocator);

					return memPtr;
				}
			}

			{
				var zone = Allocator.MzCreateZone((int)Math.Max(size, allocator.initialSize));
				var zoneIndex = allocator.AddZone(zone);
				ptr = Allocator.MzMalloc(zone, (int)size);
				var memPtr = allocator.GetSafePtr(ptr, zoneIndex);
#if LOGS_ENABLED
				Allocator.LogAdd(memPtr, size);
#endif

				allocator.locker.SetFreeIgnoreThread();
				ValidateConsistency(ref allocator);

				return memPtr;
			}
		}

#if BURST
		[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
		[INLINE(256)]
		public static bool Free(this ref Allocator allocator, in MemPtr ptr)
		{
			if (!ptr.IsValid())
				return false;

			ValidateConsistency(ref allocator);

			allocator.locker.SetBusyIgnoreThread();

			var zoneIndex = ptr.zoneId;

#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			if (zoneIndex >= allocator.zonesListCount || allocator.zonesList[zoneIndex] == null)
			{
				throw new System.Exception();
			}
#endif

			var zone = allocator.zonesList[zoneIndex];

#if LOGS_ENABLED
			if (Allocator.startLog)
			{
				Allocator.LogRemove(ptr);
			}
#endif

			var success = false;
			if (zone != null)
			{
				success = Allocator.MzFree(zone, GetUnsafePtr(allocator, ptr));

				if (Allocator.IsEmptyZone(zone))
				{
					Allocator.MzFreeZone(zone);
					allocator.zonesList[zoneIndex] = null;
				}
			}

			allocator.locker.SetFreeIgnoreThread();

			ValidateConsistency(ref allocator);

			return success;
		}

		[INLINE(256)]
		public static MemPtr CopyPtrTo(this ref Allocator srcAllocator, Allocator* dstAllocator, in MemPtr ptr)
		{
			if (!ptr.IsValid())
				return MemPtr.Invalid;

			var size = srcAllocator.GetSize(ptr);
			var dstPtr = dstAllocator->Alloc(size);

			var srcData = srcAllocator.GetUnsafePtr(ptr);
			var dstData = dstAllocator->GetUnsafePtr(dstPtr);

			MemoryExt.MemCopy(srcData, dstData, size);

			return dstPtr;
		}
	}
}
