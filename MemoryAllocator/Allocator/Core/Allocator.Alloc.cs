using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
	{
		[INLINE(256)]
		public readonly ref T Ref<T>(in MemPtr ptr) where T : unmanaged
		{
			return ref *(T*)GetUnsafePtr(ptr);
		}

		[INLINE(256)]
		public readonly ref T Ref<T>(MemPtr ptr) where T : unmanaged
		{
			return ref *(T*)GetUnsafePtr(ptr);
		}

		[INLINE(256)]
		public readonly byte* GetUnsafePtr(in MemPtr ptr)
		{
#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			if (ptr.zoneId < zonesListCount && zonesList[ptr.zoneId] != null && zonesList[ptr.zoneId]->size < ptr.offset)
			{
				throw new System.Exception();
			}
#endif

			return (byte*)zonesList[ptr.zoneId] + ptr.offset;
		}

		[INLINE(256)]
		private byte* GetUnsafePtr(in MemPtr ptr, uint offset)
		{
			return (byte*)zonesList[ptr.zoneId] + ptr.offset + offset;
		}

		[INLINE(256)]
		private readonly byte* GetUnsafePtr(in MemPtr ptr, long offset)
		{
			return (byte*)zonesList[ptr.zoneId] + ptr.offset + offset;
		}

		[INLINE(256)]
		private readonly MemPtr GetMemPtr(void* ptr, int zoneIndex)
		{
#if MEMORY_ALLOCATOR_BOUNDS_CHECK
			if (zoneIndex >= zonesListCount || zonesList[zoneIndex] == null)
			{
				throw new System.Exception();
			}
#endif
			return new MemPtr(zoneIndex, (int)((byte*)ptr - (byte*)zonesList[zoneIndex]), allocatorId);
		}
	}
}
