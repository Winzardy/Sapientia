using System;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Core;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
	{
#if BURST
		[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
		[INLINE(256)]
		public MemPtr MemReAlloc(in MemPtr ptr, int size)
		{
			return MemReAlloc(ptr, size, out _);
		}

		[INLINE(256)]
		public MemPtr MemReAlloc(in MemPtr ptr, int size, out void* voidPtr)
		{
			if (!ptr.IsValid())
				return MemAlloc(size, out voidPtr);

			ValidateConsistency();
			locker.SetBusy(true);

			voidPtr = GetUnsafePtr(ptr);
			if (ptr.IsZeroSized())
			{
				locker.SetFree(true);
				ValidateConsistency();
				return ptr;
			}

			var block = (MemBlock*)((byte*)voidPtr - TSize<MemBlock>.size);
			var blockSize = block->size;
			var blockDataSize = blockSize - TSize<MemBlock>.size;
			size = Align(size);
			if (blockDataSize > size)
			{
				locker.SetFree(true);
				ValidateConsistency();
				return ptr;
			}

			if (blockDataSize < 0)
			{
				locker.SetFree(true);
				ValidateConsistency();
				throw new System.Exception();
			}

			{
				var zone = zonesList[ptr.zoneId];
				var nextBlock = block->next.Ptr(zone);
				var requiredSize = size - blockDataSize;
				// next block is free and its size is enough for current size
				if (nextBlock != null &&
				    nextBlock->state == BLOCK_STATE_FREE &&
				    nextBlock->size - TSize<MemBlock>.size > requiredSize)
				{
					// mark current block as free
					// freePrev is false because it must not collapse block with previous one
					// [!] may be we need to add case, which move data on collapse
					if (!MzFree(zone, (byte*)block + TSize<MemBlock>.size, freePrev: false))
					{
						// Something went wrong
						locker.SetFree(true);
						ValidateConsistency();
						throw new System.Exception();
					}

					// alloc block again
					voidPtr = MzAlloc(zone, block, size + TSize<MemBlock>.size);
#if MEMORY_ALLOCATOR_BOUNDS_CHECK
					{
						var memPtr = GetMemPtr(voidPtr, ptr.zoneId);
						if (memPtr != ptr)
						{
							// Something went wrong
							locker.SetFree(true);
							this.ValidateConsistency();
							throw new System.Exception();
						}
					}
#endif
					locker.SetFree(true);
					ValidateConsistency();

					return ptr;
				}
			}

			locker.SetFree(true);
			ValidateConsistency();

			var newPtr = MemAlloc(size, out voidPtr);
			MemMove(newPtr, 0, ptr, 0, blockDataSize);
			MemFree(ptr);

			return newPtr;
		}

#if BURST
		[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
		[INLINE(256)]
		public MemPtr MemAlloc(long size)
		{
			return MemAlloc(size, out _);
		}

#if BURST
		[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
		[INLINE(256)]
		public MemPtr MemAlloc(int size, out void* ptr)
		{
			if (size == 0)
			{
				ptr = zonesList;
				return MemPtr.CreateZeroSized(allocatorId);
			}

			size = Align(size);

			ValidateConsistency();

			for (int i = 0, cnt = zonesListCount; i < cnt; ++i)
			{
				var zone = zonesList[i];
				if (zone == null)
					continue;

				ptr = MzMalloc(zone, (int)size);
				if (ptr != null)
				{
					var memPtr = GetMemPtr(ptr, i);
#if LOGS_ENABLED
					LogAdd(memPtr, size);
#endif
					ValidateConsistency();

					return memPtr;
				}
			}

			{
				var zone = MzCreateZone((int)FloatMathExt.Max(size, initialSize));
				var zoneIndex = AddZone(zone);
				ptr = MzMalloc(zone, (int)size);
				var memPtr = GetMemPtr(ptr, zoneIndex);
#if LOGS_ENABLED
				LogAdd(memPtr, size);
#endif

				ValidateConsistency();

				return memPtr;
			}
		}

		[INLINE(256)]
		public MemPtr MemAlloc<T>(T data) where T : unmanaged
		{
			var ptr = MemAlloc<T>();
			Ref<T>(ptr) = data;
			return ptr;
		}

		[INLINE(256)]
		public MemPtr MemAlloc<T>(in T data, out T* rawPtr) where T : unmanaged
		{
			var ptr = MemAlloc<T>(out rawPtr);
			Ref<T>(ptr) = data;
			return ptr;
		}

		[INLINE(256)]
		public MemPtr MemAlloc<T>() where T : unmanaged
		{
			var size = TSize<T>.size;
			return MemAlloc(size, out _);
		}

		[INLINE(256)]
		public MemPtr MemAlloc<T>(out T* rawTPtr) where T : unmanaged
		{
			var size = TSize<T>.size;
			var memPtr = MemAlloc(size, out var rawPtr);

			rawTPtr = (T*)rawPtr;
			return memPtr;
		}


#if BURST
		[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
		[INLINE(256)]
		public bool MemFree(in MemPtr ptr)
		{
			if (!ptr.IsValid())
				return false;
			if (ptr.IsZeroSized())
				return true;

			ValidateConsistency();

			locker.SetBusy(true);

			var zoneIndex = ptr.zoneId;

#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			if (zoneIndex >= zonesListCount || zonesList[zoneIndex] == null)
			{
				throw new System.Exception();
			}
#endif

			var zone = zonesList[zoneIndex];

#if LOGS_ENABLED
			if (startLog)
			{
				LogRemove(ptr);
			}
#endif

			var success = false;
			if (zone != null)
			{
				success = MzFree(zone, GetUnsafePtr(ptr));

				if (IsEmptyZone(zone))
				{
					MzFreeZone(zone);
					zonesList[zoneIndex] = null;
				}
			}

			locker.SetFree(true);

			ValidateConsistency();

			return success;
		}

		[INLINE(256)]
		public readonly void MemSwap(in MemPtr a, long aOffset, in MemPtr b, long bOffset, long length)
		{
#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			if (a.IsZeroSized() || b.IsZeroSized())
				throw new System.Exception();

			var aZoneIndex = a.zoneId;
			var bZoneIndex = b.zoneId;
			var aMaxOffset = a.offset + aOffset + length;
			var bMaxOffset = b.offset + bOffset + length;

			if (aZoneIndex >= zonesListCount || bZoneIndex >= zonesListCount)
				throw new System.Exception();

			if (zonesList[aZoneIndex]->size < aMaxOffset || zonesList[bZoneIndex]->size < bMaxOffset)
				throw new System.Exception();
#endif

			var aRawPtr = GetUnsafePtr(a, aOffset);
			var bRawPtr = GetUnsafePtr(b, bOffset);

			MemoryExt.MemSwap(bRawPtr, aRawPtr, length);
		}

		[INLINE(256)]
		public readonly void MemCopy(in MemPtr dest, long destOffset, in MemPtr source, long sourceOffset, long length)
		{
#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			if (dest.IsZeroSized() || source.IsZeroSized())
				throw new System.Exception();

			var destZoneIndex = dest.zoneId;
			var sourceZoneIndex = source.zoneId;
			var destMaxOffset = dest.offset + destOffset + length;
			var sourceMaxOffset = source.offset + sourceOffset + length;

			if (destZoneIndex >= zonesListCount || sourceZoneIndex >= zonesListCount)
				throw new System.Exception();

			if (zonesList[destZoneIndex]->size < destMaxOffset || zonesList[sourceZoneIndex]->size < sourceMaxOffset)
				throw new System.Exception();
#endif

			var sourceRawPtr = GetUnsafePtr(source, sourceOffset);
			var destRawPtr = GetUnsafePtr(dest, destOffset);
			MemoryExt.MemCopy(sourceRawPtr, destRawPtr, length);
		}

		[INLINE(256)]
		public readonly void MemMove(in MemPtr dest, long destOffset, in MemPtr source, long sourceOffset, long length)
		{
#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			if (dest.IsZeroSized() || source.IsZeroSized())
				throw new System.Exception();

			var destZoneIndex = dest.zoneId;
			var sourceZoneIndex = source.zoneId;
			var destMaxOffset = dest.offset + destOffset + length;
			var sourceMaxOffset = source.offset + sourceOffset + length;

			if (destZoneIndex >= zonesListCount || sourceZoneIndex >= zonesListCount)
				throw new System.Exception();

			if (zonesList[destZoneIndex]->size < destMaxOffset || zonesList[sourceZoneIndex]->size < sourceMaxOffset)
				throw new System.Exception();
#endif

			var sourceRawPtr = GetUnsafePtr(source, sourceOffset);
			var destRawPtr = GetUnsafePtr(dest, destOffset);
			MemoryExt.MemMove(sourceRawPtr, destRawPtr, length);
		}

		[INLINE(256)]
		public readonly void MemFill<T>(in MemPtr dest, in T value, long destOffset, int length) where T: unmanaged
		{
#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			if (dest.IsZeroSized())
				throw new System.Exception();

			var zoneIndex = dest.zoneId;

			if (zoneIndex >= zonesListCount || zonesList[zoneIndex]->size < (dest.offset + destOffset + length))
				throw new System.Exception();
#endif

			MemoryExt.MemFill(value, GetUnsafePtr(dest, destOffset), length);
		}

		[INLINE(256)]
		public readonly void MemClear(in MemPtr dest, long destOffset, long length)
		{
#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			if (dest.IsZeroSized())
				throw new System.Exception();

			var zoneIndex = dest.zoneId;

			if (zoneIndex >= zonesListCount || zonesList[zoneIndex]->size < (dest.offset + destOffset + length))
				throw new System.Exception();
#endif

			MemoryExt.MemClear(GetUnsafePtr(dest, destOffset), length);
		}

		[INLINE(256)]
		public void MemPrepare(long size)
		{
			for (var i = 0; i < zonesListCount; i++)
			{
				var zone = zonesList[i];

				if (zone == null)
					continue;

				if (MzHasFreeBlock(zone, (int)size))
				{
					return;
				}
			}

			AddZone(MzCreateZone((int)Math.Max(size, initialSize)));
		}
	}
}
