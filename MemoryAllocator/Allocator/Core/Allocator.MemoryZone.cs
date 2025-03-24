//#define MEMORY_ALLOCATOR_BOUNDS_CHECK
//#define BURST

using System;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(AllocatorProxy))]
#if BURST
	[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
	public unsafe partial struct Allocator
	{
		[INLINE(256)]
		private void FreeZones()
		{
			if (zonesListCount > 0 && zonesList != null)
			{
				for (var i = 0; i < zonesListCount; i++)
				{
					var zone = zonesList[i];
					if (zone != null)
					{
						MzFreeZone(zone);
					}
				}
			}

			zonesListCount = 0;
		}

		[INLINE(256)]
		private int AddZone(MemZone* zone, bool lookUpNull = true)
		{
			if (lookUpNull)
			{
				for (var i = 0; i < zonesListCount; ++i)
				{
					if (zonesList[i] == null)
					{
						zonesList[i] = zone;
						return i;
					}
				}
			}

			if (zonesListCapacity <= zonesListCount)
			{
				var capacity = Math.Max(MIN_ZONES_LIST_CAPACITY, zonesListCapacity * 2);
				var list = (MemZone**)MemoryExt.MemAlloc(capacity * (uint)sizeof(MemZone*), TAlign<IntPtr>.align);

				if (zonesList != null)
				{
					MemoryExt.MemCopy(zonesList, list, (uint)sizeof(MemZone*) * zonesListCount);
					MemoryExt.MemFree(zonesList);
				}

				zonesList = list;
				zonesListCapacity = capacity;
			}

			zonesList[zonesListCount++] = zone;
			return zonesListCount - 1;
		}

#if BURST
		[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
		[INLINE(256)]
		private static void MzClearZone(MemZone* zone)
		{
			var block = (MemBlock*)((byte*)zone + sizeof(MemZone));
			var blockOffset = new MemBlockOffset(block, zone);

			// set the entire zone to one free block
			zone->blocklist.next = zone->blocklist.prev = blockOffset;

			zone->blocklist.state = BLOCK_STATE_USED;
			zone->rover = blockOffset;

			block->prev = block->next = new MemBlockOffset(&zone->blocklist, zone);
			block->state = BLOCK_STATE_FREE;
			block->size = zone->size - TSize<MemZone>.size;
		}

#if BURST
		[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
		[INLINE(256)]
		private static MemZone* MzCreateZoneEmpty(int size)
		{
			size = MzGetMemBlockSize(size) + TSize<MemZone>.size;
			var zone = (MemZone*)MemoryExt.MemAlloc(size);
			return zone;
		}

#if BURST
		[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
		[INLINE(256)]
		private static MemZone* MzCreateZone(int size)
		{
			size = MzGetMemBlockSize(size) + TSize<MemZone>.size;
			var zone = (MemZone*)MemoryExt.MemAlloc(size);
			zone->size = size;
			MzClearZone(zone);

			return zone;
		}

#if BURST
		[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
		[INLINE(256)]
		private static MemZone* MzReallocZone(MemZone* zone, int newSize)
		{
			if (zone->size >= newSize)
				return zone;

			var newZone = MzCreateZone(newSize);
			var extra = newZone->size - zone->size;

			MemoryExt.MemCopy(zone, newZone, zone->size);

			newZone->size = zone->size + extra;

			var top = newZone->rover.Ptr(newZone);

			for (var block = newZone->blocklist.next.Ptr(newZone); block != &newZone->blocklist; block = block->next.Ptr(newZone))
			{
				if (block > top)
				{
					top = block;
				}
			}

			if (top->state == BLOCK_STATE_FREE)
			{
				top->size += extra;
			}
			else
			{
				var newBlock = (MemBlock*)((byte*)top + top->size);
				var newBlockOffset = new MemBlockOffset(newBlock, newZone);
				newBlock->size = extra;

				newBlock->state = BLOCK_STATE_FREE;
#if MEMORY_ALLOCATOR_BOUNDS_CHECK
				newBlock->id = ZONE_ID;
#endif
				newBlock->prev = new MemBlockOffset(top, newZone);
				newBlock->next = top->next;
				newBlock->next.Ptr(newZone)->prev = newBlockOffset;

				top->next = newBlockOffset;
				newZone->rover = newBlockOffset;
			}

			MzFreeZone(zone);

			return newZone;
		}

#if BURST
		[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
		[INLINE(256)]
		internal static void MzFreeZone(MemZone* zone)
		{
			MemoryExt.MemFree(zone);
		}

		[System.Diagnostics.ConditionalAttribute("MEMORY_ALLOCATOR_BOUNDS_CHECK")]
		private static void CHECK_PTR(void* ptr)
		{
			if (ptr == null)
			{
				throw new ArgumentException("CHECK_PTR failed");
			}
		}

		[System.Diagnostics.ConditionalAttribute("MEMORY_ALLOCATOR_BOUNDS_CHECK")]
		public static void CHECK_ZONE_ID(int id)
		{
			if (id != ZONE_ID)
			{
				throw new ArgumentException("MzFree: freed a pointer without ZONEID");
			}
		}

#if BURST
		[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
		[INLINE(256)]
		public static bool MzFree(MemZone* zone, void* ptr, bool freePrev = true)
		{
			CHECK_PTR(ptr);

			var block = (MemBlock*)((byte*)ptr - TSize<MemBlock>.size);
			var blockOffset = new MemBlockOffset(block, zone);

#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			CHECK_ZONE_ID(block->id);
#endif

			if (block->state == BLOCK_STATE_FREE)
			{
				throw new System.Exception("Seems like ptr is free already");
			}

			// mark as free
			block->state = BLOCK_STATE_FREE;
#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			block->id = 0;
#endif

			MemBlock* other;
			MemBlockOffset otherOffset;
			if (freePrev)
			{
				other = block->prev.Ptr(zone);
				otherOffset = block->prev;
				if (other->state == BLOCK_STATE_FREE)
				{
					// merge with previous free block
					other->size += block->size;
					other->next = block->next;
					other->next.Ptr(zone)->prev = otherOffset;

					if (blockOffset == zone->rover)
						zone->rover = otherOffset;

					block = other;
					blockOffset = otherOffset;
				}
			}

			{
				other = block->next.Ptr(zone);
				otherOffset = block->next;
				if (other->state == BLOCK_STATE_FREE)
				{
					// merge the next free block onto the end
					block->size += other->size;
					block->next = other->next;
					block->next.Ptr(zone)->prev = blockOffset;

					if (otherOffset == zone->rover)
						zone->rover = blockOffset;
				}
			}

			return true;
		}

#if BURST
		[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
		[INLINE(256)]
		private static int MzGetMemBlockSize(int size)
		{
			return size + TSize<MemBlock>.size;
		}

		[INLINE(256)]
		internal static int Align(int size) => ((size + 3) & ~3);

#if BURST
		[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
		[INLINE(256)]
		public static void* MzMalloc(MemZone* zone, int size)
		{
			size = MzGetMemBlockSize(size);

			// scan through the block list,
			// looking for the first free block
			// of sufficient size,
			// throwing out any purgable blocks along the way.

			// if there is a free block behind the rover,
			//  back up over them
			var baseBlock = zone->rover.Ptr(zone);

			if (baseBlock->prev.Ptr(zone)->state != BLOCK_STATE_FREE)
				baseBlock = baseBlock->prev.Ptr(zone);

			var rover = baseBlock;
			var start = baseBlock->prev.Ptr(zone);

			do
			{
				if (rover == start)
				{
					// scanned all the way around the list
					return null;
					//throw new System.OutOfMemoryException($"Malloc: failed on allocation of {size} bytes");
				}

				if (rover->state != BLOCK_STATE_FREE)
				{
					// hit a block that can't be purged,
					// so move base past it
					baseBlock = rover = rover->next.Ptr(zone);
				}
				else
				{
					rover = rover->next.Ptr(zone);
				}
			} while (baseBlock->state != BLOCK_STATE_FREE || baseBlock->size < size);

			// found a block big enough
			var extra = baseBlock->size - size;
			if (extra > MIN_FRAGMENT)
			{
				// there will be a free fragment after the allocated block
				var newBlock = (MemBlock*)((byte*)baseBlock + size);
				var newBlockOffset = new MemBlockOffset(newBlock, zone);
				newBlock->size = extra;

				// NULL indicates free block.
				newBlock->state = BLOCK_STATE_FREE;
				newBlock->prev = new MemBlockOffset(baseBlock, zone);
				newBlock->next = baseBlock->next;
				newBlock->next.Ptr(zone)->prev = newBlockOffset;

				baseBlock->next = newBlockOffset;
				baseBlock->size = size;
			}

#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			baseBlock->id = ZONE_ID;
#endif

			baseBlock->state = BLOCK_STATE_USED;
			// next allocation will start looking here
			zone->rover = baseBlock->next;

			return (void*)((byte*)baseBlock + TSize<MemBlock>.size);
		}

		[INLINE(256)]
		public static void* MzAlloc(MemZone* zone, MemBlock* baseBlock, int size)
		{
			// found a block big enough
			var extra = baseBlock->size - size;
			if (extra > MIN_FRAGMENT)
			{
				// there will be a free fragment after the allocated block
				var newBlock = (MemBlock*)((byte*)baseBlock + size);
				var newBlockOffset = new MemBlockOffset(newBlock, zone);
				newBlock->size = extra;

				// NULL indicates free block.
				newBlock->state = BLOCK_STATE_FREE;
				newBlock->prev = new MemBlockOffset(baseBlock, zone);
				newBlock->next = baseBlock->next;
				newBlock->next.Ptr(zone)->prev = newBlockOffset;

				baseBlock->next = newBlockOffset;
				baseBlock->size = size;
			}

#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			baseBlock->id = ZONE_ID;
#endif

			baseBlock->state = BLOCK_STATE_USED;
			// next allocation will start looking here
			zone->rover = baseBlock->next;

			return (byte*)baseBlock + TSize<MemBlock>.size;
		}

#if BURST
		[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
		[INLINE(256)]
		public static bool IsEmptyZone(MemZone* zone)
		{
			var block = zone->blocklist.next.Ptr(zone);
			while (block != &zone->blocklist)
			{
				if (block->state != BLOCK_STATE_FREE)
					return false;
				E.ASSERT(block->next.value != 0);
				block = block->next.Ptr(zone);
			}

			return true;
		}

		[INLINE(256)]
		public static void MzDumpHeap(MemZone* zone, System.Collections.Generic.List<string> results)
		{
			results.Add($"zone size: {zone->size}; location: {new IntPtr(zone)}; rover block offset: {zone->rover.value}");

			for (var block = zone->blocklist.next.Ptr(zone);; block = block->next.Ptr(zone))
			{
				results.Add($"block offset: {(byte*)block - (byte*)@zone}; size: {block->size}; state: {block->state}");

				if (block->next.Ptr(zone) == &zone->blocklist) break;

				MzCheckBlock(zone, block, results);
			}
		}

		[INLINE(256)]
		public static void MzCheckHeap(MemZone* zone, System.Collections.Generic.List<string> results)
		{
			var block = zone->blocklist.next.Ptr(zone);
			while (block->next.Ptr(zone) != &zone->blocklist)
			{
				MzCheckBlock(zone, block, results);
				block = block->next.Ptr(zone);
			}
		}

		[INLINE(256)]
		private static void MzCheckBlock(MemZone* zone, MemBlock* block,
			System.Collections.Generic.List<string> results)
		{
			var next = (byte*)block->next.Ptr(zone);
			if (next == null)
			{
				results.Add("CheckHeap: next block is null\n");
				return;
			}

			if ((byte*)block + block->size != (byte*)block->next.Ptr(zone))
			{
				results.Add("CheckHeap: block size does not touch the next block\n");
			}

			if (block->next.Ptr(zone)->prev.Ptr(zone) != block)
			{
				results.Add("CheckHeap: next block doesn't have proper back link\n");
			}

			if (block->state == BLOCK_STATE_FREE && block->next.Ptr(zone)->state == BLOCK_STATE_FREE)
			{
				results.Add("CheckHeap: two consecutive free blocks\n");
			}
		}

		[INLINE(256)]
		public static bool MzCheckHeap(MemZone* zone, out int blockIndex, out int index)
		{
			blockIndex = -1;
			index = -1;
			for (var block = zone->blocklist.next.Ptr(zone);; block = block->next.Ptr(zone))
			{
				if (block->next.Ptr(zone) == &zone->blocklist)
				{
					// all blocks have been hit
					break;
				}

				++blockIndex;
				if (!MzCheckBlock(zone, block, out index))
					return false;
			}

			return true;
		}

		[INLINE(256)]
		private static bool MzCheckBlock(MemZone* zone, MemBlock* block, out int index)
		{
			index = -1;
			var next = (byte*)block->next.Ptr(zone);
			if (next == null)
			{
				index = 0;
				return false;
			}

			if ((byte*)block + block->size != next)
			{
				index = 1;
				return false;
			}

			if (block->next.Ptr(zone)->prev.Ptr(zone) != block)
			{
				index = 2;
				return false;
			}

			if (block->state == BLOCK_STATE_FREE && block->next.Ptr(zone)->state == BLOCK_STATE_FREE)
			{
				index = 3;
				return false;
			}

			return true;
		}

#if BURST
		[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
		[INLINE(256)]
		public static int GetMzFreeMemory(MemZone* zone)
		{
			var free = 0;

			for (var block = zone->blocklist.next.Ptr(zone); block != &zone->blocklist; block = block->next.Ptr(zone))
			{
				if (block->state == BLOCK_STATE_FREE)
					free += block->size;
			}

			return free;
		}

#if BURST
		[Unity.Burst.BurstCompileAttribute.BURST(CompileSynchronously = true)]
#endif
		[INLINE(256)]
		public static bool MzHasFreeBlock(MemZone* zone, int size)
		{
			size = MzGetMemBlockSize(size);

			for (var block = zone->blocklist.next.Ptr(zone); block != &zone->blocklist; block = block->next.Ptr(zone))
			{
				if (block->state == BLOCK_STATE_FREE && block->size > size)
				{
					return true;
				}
			}

			return false;
		}
	}
}
