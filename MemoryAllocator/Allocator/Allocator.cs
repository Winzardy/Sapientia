#define MEMORY_ALLOCATOR_BOUNDS_CHECK
#define LOGS_ENABLED
//#define BURST
#define ALLOCATOR_VALIDATION

using System;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Core;
using Sapientia.MemoryAllocator.Data;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	public enum ClearOptions
	{
		ClearMemory,
		UninitializedMemory,
	}

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

			voidPtr = GetUnsafePtr(in allocator, ptr);
			var block = (Allocator.MemBlock*)((byte*)voidPtr - TSize<Allocator.MemBlock>.size);
			var blockSize = block->size;
			var blockDataSize = blockSize - TSize<Allocator.MemBlock>.size;
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
				    nextBlock->size - TSize<Allocator.MemBlock>.size > requiredSize)
				{
					// mark current block as free
					// freePrev is false because it must not collapse block with previous one
					// [!] may be we need to add case, which move data on collapse
					if (!Allocator.MzFree(zone, (byte*)block + TSize<Allocator.MemBlock>.size, freePrev: false))
					{
						// Something went wrong
						allocator.locker.SetFreeIgnoreThread();
						ValidateConsistency(ref allocator);
						throw new System.Exception();
					}

					// alloc block again
					var newPtr = Allocator.MzAlloc(zone, block, size + TSize<Allocator.MemBlock>.size);
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
					UnityEngine.Debug.LogError($"zone {i}, block {blockIndex}, index {index}, thread {Unity.Jobs.LowLevel.Unsafe.JobsUtility.ThreadIndex}");
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

			for (uint i = 0u, cnt = allocator.zonesListCount; i < cnt; ++i)
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
				success = Allocator.MzFree(zone, GetUnsafePtr(in allocator, ptr));

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
	}

	public unsafe partial struct Allocator : IDisposable
	{
		public AsyncValue locker;
		public int threadId;

#if LOGS_ENABLED && UNITY_EDITOR
		[Unity.Burst.BurstDiscardAttribute]
		public static void LogAdd(in MemPtr memPtr, long size)
		{
			if (startLog)
			{
				var str = "ALLOC: " + memPtr + ", SIZE: " + size;
				logList.Add(memPtr, str + "\n" + UnityEngine.StackTraceUtility.ExtractStackTrace());
			}
		}

		[Unity.Burst.BurstDiscardAttribute]
		public static void LogRemove(in MemPtr memPtr)
		{
			logList.Remove(memPtr);
		}

		public static bool startLog;

		public static System.Collections.Generic.Dictionary<MemPtr, string> logList = new ();

		[UnityEditor.MenuItem("ME.ECS/Debug/Allocator: Start Log")]
		public static void StartLog()
		{
			startLog = true;
		}

		[UnityEditor.MenuItem("ME.ECS/Debug/Allocator: End Log")]
		public static void EndLog()
		{
			startLog = false;
			logList.Clear();
		}

		[UnityEditor.MenuItem("ME.ECS/Debug/Allocator: Print Log")]
		public static void PrintLog()
		{
			foreach (var item in logList)
			{
				UnityEngine.Debug.Log(item.Key + "\n" + item.Value);
			}
		}
#endif

		public const long OFFSET_MASK = 0xFFFFFFFF;
		public const long MIN_ZONE_SIZE = 512 * 1024; //128 * 1024;
		public const int MIN_ZONE_SIZE_IN_KB = (int)(MIN_ZONE_SIZE / 1024);
		private const int MIN_ZONES_LIST_CAPACITY = 20;

#if UNITY_5_3_OR_NEWER
		[Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
		public MemZone** zonesList;
		public uint zonesListCount;
		internal uint zonesListCapacity;
		internal long maxSize;
		internal int initialSize;
		public ushort version;

		public bool IsValid => zonesList != null;

		public uint GetSize(in MemPtr ptr)
		{
			var block = (MemBlock*)((byte*)zonesList[ptr.zoneId] + ptr.offset - sizeof(MemBlock));
			return (uint)block->size;
		}

		[INLINE(256)]
		public readonly void GetSize(out int reservedSize, out int usedSize, out int freeSize)
		{
			usedSize = 0;
			reservedSize = 0;
			for (var i = 0; i < zonesListCount; i++)
			{
				var zone = zonesList[i];
				if (zone != null)
				{
					reservedSize += zone->size;
					usedSize = reservedSize;
					usedSize -= GetMzFreeMemory(zone);
				}
			}

			freeSize = reservedSize - usedSize;
		}

		[INLINE(256)]
		public readonly int GetReservedSize()
		{
			var size = 0;
			for (var i = 0; i < zonesListCount; i++)
			{
				var zone = zonesList[i];
				if (zone != null)
				{
					size += zone->size;
				}
			}

			return size;
		}

		[INLINE(256)]
		public readonly int GetUsedSize()
		{
			var size = 0;
			for (var i = 0; i < zonesListCount; i++)
			{
				var zone = zonesList[i];
				if (zone != null)
				{
					size += zone->size;
					size -= GetMzFreeMemory(zone);
				}
			}

			return size;
		}

		[INLINE(256)]
		public readonly int GetFreeSize()
		{
			var size = 0;
			for (var i = 0; i < zonesListCount; i++)
			{
				var zone = zonesList[i];
				if (zone != null)
				{
					size += GetMzFreeMemory(zone);
				}
			}

			return size;
		}

		///
		/// Constructors
		///
		[INLINE(256)]
		public Allocator Initialize(long initialSize, long maxSize = -1L)
		{
			if (maxSize < initialSize)
				maxSize = initialSize;

			this.initialSize = (int)Math.Max(initialSize, MIN_ZONE_SIZE);
			AddZone(MzCreateZone(this.initialSize));
			this.maxSize = maxSize;
			threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
			version = 1;

			return this;
		}

		[INLINE(256)]
		public void Dispose()
		{
			FreeZones();

			if (zonesList != null)
			{
				MemoryExt.MemFree(zonesList);
				zonesList = null;
			}

			zonesListCapacity = 0;
			maxSize = default;
		}

		[INLINE(256)]
		public void CopyFrom(in Allocator other)
		{
			if (other.zonesList == null && zonesList == null)
			{
			}
			else if (other.zonesList == null && zonesList != null)
			{
				FreeZones();
			}
			else
			{
				if (zonesListCount < other.zonesListCount)
				{
					for (var i = zonesListCount; i < other.zonesListCount; ++i)
					{
						var otherZone = other.zonesList![i];
						if (otherZone == null)
						{
							AddZone(null, false);
						}
						else
						{
							var zone = MzCreateZone(otherZone->size);
							AddZone(zone, false);
						}
					}
				}

				if (zonesListCount == other.zonesListCount)
				{
					for (var i = 0; i < other.zonesListCount; ++i)
					{
						ref var curZone = ref zonesList[i];
						var otherZone = other.zonesList![i];
						{
							if (curZone == null && otherZone == null) continue;

							if (curZone == null)
							{
								curZone = MzCreateZone(otherZone->size);
								MemoryExt.MemCopy(otherZone, curZone, otherZone->size);
							}
							else if (otherZone == null)
							{
								MzFreeZone(curZone);
								curZone = null;
							}
							else
							{
								// resize zone
								curZone = MzReallocZone(curZone, otherZone->size);
								MemoryExt.MemCopy(otherZone, curZone, otherZone->size);
							}
						}
					}
				}
				else
				{
					FreeZones();

					for (var i = 0; i < other.zonesListCount; i++)
					{
						var otherZone = other.zonesList![i];

						if (otherZone != null)
						{
							var zone = MzCreateZone(otherZone->size);
							MemoryExt.MemCopy(otherZone, zone, otherZone->size);
							AddZone(zone, false);
						}
						else
						{
							AddZone(null, false);
						}
					}
				}
			}

			version = other.version;
			++version;
			threadId = other.threadId;
			maxSize = other.maxSize;
			initialSize = other.initialSize;
		}

		[INLINE(256)]
		public void CopyFromComplete(in Allocator other, int index)
		{
			// We must be sure that source allocator has the same structure and size as current
			// So we must call CopyFromPrepare() first
			var curZone = zonesList[index];
			var otherZone = other.zonesList[index];
			{
				if (curZone == null && otherZone == null) return;
				{
					MemoryExt.MemCopy(otherZone, curZone, otherZone->size);
				}
			}
		}

		[INLINE(256)]
		public void CopyFromPrepare(in Allocator other)
		{
			if (other.zonesList == null && zonesList == null)
			{
			}
			else if (other.zonesList == null && zonesList != null)
			{
				FreeZones();
			}
			else
			{
				if (zonesListCount < other.zonesListCount)
				{
					for (var i = zonesListCount; i < other.zonesListCount; ++i)
					{
						var otherZone = other.zonesList![i];
						if (otherZone == null)
						{
							AddZone(null, false);
						}
						else
						{
							var zone = MzCreateZone(otherZone->size);
							AddZone(zone, false);
						}
					}
				}

				if (zonesListCount == other.zonesListCount)
				{
					for (var i = 0; i < other.zonesListCount; ++i)
					{
						ref var curZone = ref zonesList[i];
						var otherZone = other.zonesList![i];
						{
							if (curZone == null && otherZone == null) continue;

							if (curZone == null)
							{
								curZone = MzCreateZone(otherZone->size);
							}
							else if (otherZone == null)
							{
								MzFreeZone(curZone);
								curZone = null;
							}
							else
							{
								// resize zone
								curZone = MzReallocZone(curZone, otherZone->size);
							}
						}
					}
				}
				else
				{
					FreeZones();

					for (var i = 0; i < other.zonesListCount; i++)
					{
						var otherZone = other.zonesList![i];
						if (otherZone != null)
						{
							var zone = MzCreateZoneEmpty(otherZone->size);
							AddZone(zone, false);
						}
						else
						{
							AddZone(null, false);
						}
					}
				}
			}

			version = other.version;
			++version;
			threadId = other.threadId;
			maxSize = other.maxSize;
			initialSize = other.initialSize;
		}

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
		internal uint AddZone(MemZone* zone, bool lookUpNull = true)
		{
			if (lookUpNull)
			{
				for (var i = 0u; i < zonesListCount; ++i)
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

			return zonesListCount - 1u;
		}

		///
		/// Base
		///
		[INLINE(256)]
		public readonly ref T Ref<T>(in MemPtr ptr) where T : unmanaged
		{
			return ref *(T*)this.GetUnsafePtr(ptr);
		}

		[INLINE(256)]
		public readonly ref T Ref<T>(MemPtr ptr) where T : unmanaged
		{
			return ref *(T*)this.GetUnsafePtr(ptr);
		}

		[INLINE(256)]
		public MemPtr Alloc<T>(T data) where T : unmanaged
		{
			var ptr = Alloc<T>();
			Ref<T>(ptr) = data;
			return ptr;
		}

		[INLINE(256)]
		public MemPtr Alloc<T>(T data, out T* rawPtr) where T : unmanaged
		{
			var ptr = Alloc<T>(out rawPtr);
			Ref<T>(ptr) = data;
			return ptr;
		}

		[INLINE(256)]
		public MemPtr Alloc<T>() where T : unmanaged
		{
			var size = TSize<T>.size;
			var alignOf = TAlign<T>.align;
			return Alloc(size + alignOf);
		}

		[INLINE(256)]
		public MemPtr Alloc<T>(out T* rawTPtr) where T : unmanaged
		{
			var size = TSize<T>.size;
			var alignOf = TAlign<T>.align;
			var memPtr = Alloc(size + alignOf, out var rawPtr);

			rawTPtr = (T*)rawPtr;
			return memPtr;
		}

		[INLINE(256)]
		public readonly void MemCopy(in MemPtr dest, long destOffset, in MemPtr source, long sourceOffset, long length)
		{
#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			var destZoneIndex = dest.zoneId;
			var sourceZoneIndex = source.zoneId;
			var destMaxOffset = dest.offset + destOffset + length;
			var sourceMaxOffset = source.offset + sourceOffset + length;

			if (destZoneIndex >= zonesListCount || sourceZoneIndex >= zonesListCount)
			{
				throw new System.Exception();
			}

			if (zonesList[destZoneIndex]->size < destMaxOffset || zonesList[sourceZoneIndex]->size < sourceMaxOffset)
			{
				throw new System.Exception();
			}
#endif

			var sourceRawPtr = this.GetUnsafePtr(source, sourceOffset);
			var destRawPtr = this.GetUnsafePtr(dest, destOffset);
			MemoryExt.MemCopy(sourceRawPtr, destRawPtr, length);
		}

		[INLINE(256)]
		public readonly void MemMove(in MemPtr dest, long destOffset, in MemPtr source, long sourceOffset, long length)
		{
#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			var destZoneIndex = dest.zoneId;
			var sourceZoneIndex = source.zoneId;
			var destMaxOffset = dest.offset + destOffset + length;
			var sourceMaxOffset = source.offset + sourceOffset + length;

			if (destZoneIndex >= zonesListCount || sourceZoneIndex >= zonesListCount)
			{
				throw new System.Exception();
			}

			if (zonesList[destZoneIndex]->size < destMaxOffset || zonesList[sourceZoneIndex]->size < sourceMaxOffset)
			{
				throw new System.Exception();
			}
#endif

			var sourceRawPtr = this.GetUnsafePtr(source, sourceOffset);
			var destRawPtr = this.GetUnsafePtr(dest, destOffset);
			MemoryExt.MemMove(sourceRawPtr, destRawPtr, length);
		}

		[INLINE(256)]
		public readonly void MemClear(in MemPtr dest, long destOffset, long length)
		{
#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			var zoneIndex = dest.zoneId;

			if (zoneIndex >= zonesListCount || zonesList[zoneIndex]->size < (dest.offset + destOffset + length))
			{
				throw new System.Exception();
			}
#endif

			MemoryExt.MemClear(this.GetUnsafePtr(dest, destOffset), length);
		}

		[INLINE(256)]
		public void Prepare(long size)
		{
			for (var i = 0; i < zonesListCount; i++)
			{
				var zone = zonesList[i];

				if (zone == null) continue;

				if (MzHasFreeBlock(zone, (int)size))
				{
					return;
				}
			}

			AddZone(MzCreateZone((int)Math.Max(size, initialSize)));
		}

		[INLINE(256)]
		internal readonly MemPtr GetSafePtr(void* ptr, uint zoneIndex)
		{
#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			if (zoneIndex >= zonesListCount || zonesList[zoneIndex] == null)
			{
				throw new System.Exception();
			}
#endif
			return new MemPtr(zoneIndex, (uint)((byte*)ptr - (byte*)zonesList[zoneIndex]));
		}

		///
		/// Arrays
		///
		[INLINE(256)]
		public readonly MemPtr RefArrayPtr<T>(in MemPtr ptr, int index) where T : unmanaged
		{
			var size = TSize<T>.uSize;
			return new MemPtr(ptr.zoneId, ptr.offset + (uint)index * size);
		}

		[INLINE(256)]
		public readonly MemPtr RefArrayPtr<T>(in MemPtr ptr, uint index) where T : unmanaged
		{
			var size = TSize<T>.uSize;
			return new MemPtr(ptr.zoneId, ptr.offset + index * size);
		}

		[INLINE(256)]
		public readonly ref T RefArray<T>(in MemPtr ptr, int index) where T : unmanaged
		{
			var size = TSize<T>.size;
			return ref *(T*)this.GetUnsafePtr(in ptr, index * size);
		}

		[INLINE(256)]
		public readonly ref T RefArray<T>(MemPtr ptr, int index) where T : unmanaged
		{
			var size = TSize<T>.size;
			return ref *(T*)this.GetUnsafePtr(in ptr, index * size);
		}

		[INLINE(256)]
		public readonly ref T RefArray<T>(in MemPtr ptr, uint index) where T : unmanaged
		{
			var size = TSize<T>.uSize;
			return ref *(T*)this.GetUnsafePtr(in ptr, index * size);
		}

		[INLINE(256)]
		public readonly ref T RefArray<T>(MemPtr ptr, uint index) where T : unmanaged
		{
			var size = TSize<T>.uSize;
			return ref *(T*)this.GetUnsafePtr(in ptr, index * size);
		}

		[INLINE(256)]
		public MemPtr ReAllocArray<T>(in MemPtr ptr, int newLength) where T : unmanaged
		{
			var size = TSize<T>.size;
			return this.ReAlloc(in ptr, size * newLength);
		}

		[INLINE(256)]
		public MemPtr ReAllocArray<T>(in MemPtr ptr, uint newLength) where T : unmanaged
		{
			var size = TSize<T>.uSize;
			return this.ReAlloc(in ptr, (int)(size * newLength));
		}

		[INLINE(256)]
		public MemPtr ReAllocArray<T>(in MemPtr memPtr, uint newLength, out T* ptr) where T : unmanaged
		{
			var size = TSize<T>.size;
			var newPtr = this.ReAlloc(in memPtr, (int)(size * newLength), out var voidPtr);
			ptr = (T*)voidPtr;
			return newPtr;
		}

		[INLINE(256)]
		public MemPtr ReAllocArray(uint elementSizeOf, in MemPtr ptr, uint newLength)
		{
			return this.ReAlloc(ptr, (int)(elementSizeOf * newLength));
		}

		[INLINE(256)]
		public MemPtr ReAllocArray(uint elementSizeOf, in MemPtr ptr, uint newLength, out void* voidPtr)
		{
			return this.ReAlloc(in ptr, (int)(elementSizeOf * newLength), out voidPtr);
		}

		[INLINE(256)]
		public MemPtr AllocArray<T>(int length) where T : struct
		{
			var size = TSize<T>.size;
			return this.Alloc(size * length);
		}

		[INLINE(256)]
		public MemPtr AllocArray<T>(uint length) where T : struct
		{
			var size = TSize<T>.size;
			return this.Alloc(size * length);
		}

		[INLINE(256)]
		public MemPtr AllocArray(int length, int sizeOf)
		{
			return this.Alloc(sizeOf * length);
		}

		[INLINE(256)]
		public MemPtr AllocArray(uint length, uint sizeOf)
		{
			return this.Alloc(sizeOf * length);
		}

		[INLINE(256)]
		public MemPtr AllocArray<T>(uint length, out T* ptr) where T : unmanaged
		{
			var size = TSize<T>.size;
			var memPtr = this.Alloc(size * length, out var voidPtr);
			ptr = (T*)voidPtr;
			return memPtr;
		}

		public void Deserialize(ref StreamBufferReader stream)
		{
			var allocator = new Allocator();
			stream.Read(ref allocator.version);
			stream.Read(ref allocator.maxSize);
			stream.Read(ref allocator.zonesListCount);

			allocator.zonesListCapacity = allocator.zonesListCount;
			allocator.zonesList = (MemZone**)MemoryExt.MemAlloc(allocator.zonesListCount * (uint)sizeof(MemZone*), TAlign<IntPtr>.align);

			for (var i = 0; i < allocator.zonesListCount; ++i)
			{
				var length = 0;
				stream.Read(ref length);
				if (length == 0) continue;

				var zone = MzCreateZone(length);

				allocator.zonesList[i] = zone;
				var readSize = length;
				var zn = (byte*)zone;
				stream.Read(ref zn, (uint)readSize);
			}

			this = allocator;
		}

		public readonly void Serialize(ref StreamBufferWriter stream)
		{
			stream.Write(version);
			stream.Write(maxSize);
			stream.Write(zonesListCount);

			for (var i = 0; i < zonesListCount; ++i)
			{
				var zone = zonesList[i];

				stream.Write(zone->size);

				if (zone->size == 0) continue;

				var writeSize = zone->size;
				stream.Write((byte*)zone, (uint)writeSize);
			}
		}
	}
}
