//#define MEMORY_ALLOCATOR_BOUNDS_CHECK
//#define BURST

using System;
using System.Runtime.InteropServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe partial struct Allocator : IDisposable
	{
		public AsyncValue locker;
		public int threadId;

#if UNITY_5_3_OR_NEWER
		[Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
		public MemZone** zonesList;
		public int zonesListCount;
		internal int zonesListCapacity;
		internal int initialSize;

		public ServiceRegistry serviceRegistry;
		public AllocatorId allocatorId;
		public ushort version;

		public bool IsValid => zonesList != null;
	}
}
