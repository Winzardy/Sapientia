#if UNITY_5_3_OR_NEWER

using System;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Data;
using Unity.Jobs.LowLevel.Unsafe;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(MemArrayThreadCacheLineProxy<>))]
	public unsafe struct MemArrayThreadCacheLine<T> : IIsCreated where T : unmanaged
	{
		private static readonly uint CACHE_LINE_SIZE = Math.Max(JobsUtility.CacheLineSize / TSize<T>.uSize, 1u);

		private readonly MemPtr arrPtr;
		public readonly uint Length => (uint)JobsUtility.ThreadIndexCount;

		public readonly bool IsCreated
		{
			[INLINE(256)] get => arrPtr.IsValid();
		}

		[INLINE(256)]
		public MemArrayThreadCacheLine(ref Allocator allocator, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			this = default;
			var size = TSize<T>.size;
			var memPtr = allocator.Alloc(Length * size * CACHE_LINE_SIZE);

			if (clearOptions == ClearOptions.ClearMemory)
			{
				allocator.MemClear(memPtr, 0u, Length * size * CACHE_LINE_SIZE);
			}

			arrPtr = memPtr;
		}

		[INLINE(256)]
		public void Dispose(ref Allocator allocator)
		{
			E.IS_CREATED(this);

			if (arrPtr.IsValid() == true)
			{
				allocator.Free(arrPtr);
			}

			this = default;
		}

		[INLINE(256)]
		public readonly void* GetUnsafePtr(in Allocator allocator)
		{
			return AllocatorExt.GetUnsafePtr(in allocator, arrPtr);
		}

		public readonly ref T this[in Allocator allocator, int index]
		{
			[INLINE(256)]
			get
			{
				E.RANGE(index, 0, Length);
				return ref *((T*)GetUnsafePtr(in allocator) + index * CACHE_LINE_SIZE);
			}
		}

		public readonly ref T this[in Allocator allocator, uint index]
		{
			[INLINE(256)]
			get
			{
				E.RANGE(index, 0, Length);
				return ref *((T*)GetUnsafePtr(in allocator) + index * CACHE_LINE_SIZE);
			}
		}

		[INLINE(256)]
		public void Clear(ref Allocator allocator)
		{
			var size = TSize<T>.size * CACHE_LINE_SIZE;
			allocator.MemClear(arrPtr, 0L, Length * size);
		}

		[INLINE(256)]
		public void BurstMode(in Allocator allocator, bool state)
		{
		}

		public uint GetReservedSizeInBytes()
		{
			return Length * (uint)sizeof(T) * CACHE_LINE_SIZE;
		}
	}
}

#endif
