#define MEMORY_ALLOCATOR_BOUNDS_CHECK
//#define BURST

using System;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Core;
using Sapientia.MemoryAllocator.Data;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator : IDisposable
	{
		public const int MIN_ZONE_SIZE = MIN_ZONE_SIZE_IN_KB * 1024;
		public const int MIN_ZONE_SIZE_IN_KB = 512; // 128
		private const int MIN_ZONES_LIST_CAPACITY = 20;

		public AsyncValue locker;
		public int threadId;

#if UNITY_5_3_OR_NEWER
		[Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
		public MemZone** zonesList;
		public int zonesListCount;
		internal int zonesListCapacity;
		internal int initialSize;
		internal int maxSize;
		public ServiceLocator serviceLocator;

		public AllocatorId allocatorId;
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
		public Allocator Initialize(AllocatorId allocatorId, int initialSize, int maxSize = -1)
		{
			initialSize = initialSize.Max(MIN_ZONE_SIZE);
			if (maxSize < initialSize)
				maxSize = initialSize;

			this.initialSize = initialSize;
			this.maxSize = maxSize;
			this.allocatorId = allocatorId;

			AddZone(MzCreateZone(initialSize));
			threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
			version = 1;

			serviceLocator = ServiceLocator.Create((Allocator*)this.AsPointer());

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

			this = default;
		}

		[INLINE(256)]
		public void CopyFrom(Allocator* other)
		{
			if (other->zonesList == null && zonesList == null)
			{
			}
			else if (other->zonesList == null && zonesList != null)
			{
				FreeZones();
			}
			else
			{
				if (zonesListCount < other->zonesListCount)
				{
					for (var i = zonesListCount; i < other->zonesListCount; ++i)
					{
						var otherZone = other->zonesList[i];
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

				if (zonesListCount == other->zonesListCount)
				{
					for (var i = 0; i < other->zonesListCount; ++i)
					{
						ref var curZone = ref zonesList[i];
						var otherZone = other->zonesList[i];
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

					for (var i = 0; i < other->zonesListCount; i++)
					{
						var otherZone = other->zonesList[i];

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

			threadId = other->threadId;
			initialSize = other->initialSize;
			maxSize = other->maxSize;
			serviceLocator = other->serviceLocator;

			version = (ushort)(other->version + 1);
		}

		[INLINE(256)]
		public void CopyFromComplete(Allocator* other, int index)
		{
			// We must be sure that source allocator has the same structure and size as current
			// So we must call CopyFromPrepare() first
			var curZone = zonesList[index];
			var otherZone = other->zonesList[index];
			{
				if (curZone == null && otherZone == null) return;
				{
					MemoryExt.MemCopy(otherZone, curZone, otherZone->size);
				}
			}
		}

		[INLINE(256)]
		public void CopyFromPrepare(Allocator* other)
		{
			if (other->zonesList == null && zonesList == null)
			{
			}
			else if (other->zonesList == null && zonesList != null)
			{
				FreeZones();
			}
			else
			{
				if (zonesListCount < other->zonesListCount)
				{
					for (var i = zonesListCount; i < other->zonesListCount; ++i)
					{
						var otherZone = other->zonesList![i];
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

				if (zonesListCount == other->zonesListCount)
				{
					for (var i = 0; i < other->zonesListCount; ++i)
					{
						ref var curZone = ref zonesList[i];
						var otherZone = other->zonesList![i];
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

					for (var i = 0; i < other->zonesListCount; i++)
					{
						var otherZone = other->zonesList[i];
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

			threadId = other->threadId;
			initialSize = other->initialSize;
			maxSize = other->maxSize;
			serviceLocator = other->serviceLocator;

			version = (ushort)(other->version + 1);
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
		internal int AddZone(MemZone* zone, bool lookUpNull = true)
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
		public MemPtr Alloc<T>(in T data, out T* rawPtr) where T : unmanaged
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
		public readonly void MemSwap(in MemPtr a, long aOffset, in MemPtr b, long bOffset, long length)
		{
#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			var aZoneIndex = a.zoneId;
			var bZoneIndex = b.zoneId;
			var aMaxOffset = a.offset + aOffset + length;
			var bMaxOffset = b.offset + bOffset + length;

			if (aZoneIndex >= zonesListCount || bZoneIndex >= zonesListCount)
			{
				throw new System.Exception();
			}

			if (zonesList[aZoneIndex]->size < aMaxOffset || zonesList[bZoneIndex]->size < bMaxOffset)
			{
				throw new System.Exception();
			}
#endif

			var aRawPtr = this.GetUnsafePtr(a, aOffset);
			var bRawPtr = this.GetUnsafePtr(b, bOffset);

			MemoryExt.MemSwap(bRawPtr, aRawPtr, length);
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
		public readonly void MemFill<T>(in MemPtr dest, T value, long destOffset, int length) where T: unmanaged
		{
#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			var zoneIndex = dest.zoneId;

			if (zoneIndex >= zonesListCount || zonesList[zoneIndex]->size < (dest.offset + destOffset + length))
			{
				throw new System.Exception();
			}
#endif

			MemoryExt.MemFill((T*)value.AsPointer(), this.GetUnsafePtr(dest, destOffset), (int)length);
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
		internal readonly MemPtr GetSafePtr(void* ptr, int zoneIndex)
		{
#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			if (zoneIndex >= zonesListCount || zonesList[zoneIndex] == null)
			{
				throw new System.Exception();
			}
#endif
			return new MemPtr(zoneIndex, (int)((byte*)ptr - (byte*)zonesList[zoneIndex]), allocatorId);
		}

		///
		/// Arrays
		///
		[INLINE(256)]
		public readonly MemPtr RefArrayPtr<T>(in MemPtr ptr, int index) where T : unmanaged
		{
			var size = TSize<T>.size;
			return new MemPtr(ptr.zoneId, ptr.offset + index * size, allocatorId);
		}

		[INLINE(256)]
		public readonly MemPtr RefArrayPtr(in MemPtr ptr, int size, int index)
		{
			return new MemPtr(ptr.zoneId, ptr.offset + index * size, allocatorId);
		}

		[INLINE(256)]
		public readonly ref T RefArray<T>(in MemPtr ptr, int index) where T : unmanaged
		{
			var size = TSize<T>.size;
			return ref *(T*)this.GetUnsafePtr(in ptr, index * size);
		}

		[INLINE(256)]
		public MemPtr ReAllocArray<T>(in MemPtr ptr, int newLength) where T : unmanaged
		{
			var size = TSize<T>.size;
			return this.ReAlloc(in ptr, size * newLength);
		}

		[INLINE(256)]
		public MemPtr ReAllocArray<T>(in MemPtr memPtr, int newLength, out T* ptr) where T : unmanaged
		{
			var size = TSize<T>.size;
			var newPtr = this.ReAlloc(in memPtr, (size * newLength), out var voidPtr);
			ptr = (T*)voidPtr;
			return newPtr;
		}

		[INLINE(256)]
		public MemPtr ReAllocArray(in MemPtr ptr, int elementSizeOf, int newLength)
		{
			return this.ReAlloc(ptr, (elementSizeOf * newLength));
		}

		[INLINE(256)]
		public MemPtr ReAllocArray(in MemPtr ptr, int elementSizeOf, int newLength, out void* voidPtr)
		{
			return this.ReAlloc(in ptr, (elementSizeOf * newLength), out voidPtr);
		}

		[INLINE(256)]
		public MemPtr AllocArray<T>(int length) where T : struct
		{
			var size = TSize<T>.size;
			return this.Alloc(size * length);
		}

		[INLINE(256)]
		public MemPtr AllocArray(int sizeOf, int length)
		{
			return this.Alloc(sizeOf * length);
		}

		[INLINE(256)]
		public MemPtr AllocArray(int sizeOf, int length, out void* ptr)
		{
			return this.Alloc(sizeOf * length, out ptr);
		}

		[INLINE(256)]
		public MemPtr AllocArray<T>(int length, out T* ptr) where T : unmanaged
		{
			var size = TSize<T>.size;
			var memPtr = this.Alloc(size * length, out var voidPtr);
			ptr = (T*)voidPtr;
			return memPtr;
		}

		public static Allocator* Deserialize(ref StreamBufferReader stream)
		{
			var allocator = MemoryExt.MemAlloc<Allocator>();

			stream.Read(ref allocator->version);
			stream.Read(ref allocator->maxSize);
			stream.Read(ref allocator->zonesListCount);
			stream.Read(ref allocator->allocatorId);

			allocator->zonesListCapacity = allocator->zonesListCount;
			allocator->zonesList = (MemZone**)MemoryExt.MemAlloc(allocator->zonesListCount * (uint)sizeof(MemZone*), TAlign<IntPtr>.align);

			for (var i = 0; i < allocator->zonesListCount; ++i)
			{
				var length = 0;
				stream.Read(ref length);
				if (length == 0) continue;

				var zone = MzCreateZone(length);

				allocator->zonesList[i] = zone;
				var readSize = length;
				var zn = (byte*)zone;
				stream.Read(ref zn, (uint)readSize);
			}

			return allocator;
		}

		public readonly void Serialize(ref StreamBufferWriter stream)
		{
			stream.Write(version);
			stream.Write(maxSize);
			stream.Write(zonesListCount);
			stream.Write(allocatorId);

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
