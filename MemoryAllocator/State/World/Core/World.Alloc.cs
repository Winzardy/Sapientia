using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.MemoryAllocator.Core;

namespace Sapientia.MemoryAllocator
{
	public partial class World
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public WPtr MemAlloc(int size)
		{
			return new WPtr(_allocator.MemAlloc(size), worldId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public WPtr MemAlloc(int size, out SafePtr ptr)
		{
			return new WPtr(_allocator.MemAlloc(size, out ptr), worldId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public WPtr MemAlloc<T>() where T: unmanaged
		{
			return new WPtr(_allocator.MemAlloc<T>(), worldId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public WPtr MemAlloc<T>(out SafePtr<T> ptr) where T: unmanaged
		{
			return new WPtr(_allocator.MemAlloc<T>(out ptr), worldId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public WPtr MemAlloc<T>(in T value, out SafePtr<T> ptr) where T: unmanaged
		{
			return new WPtr(_allocator.MemAlloc<T>(value, out ptr), worldId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public WPtr MemReAlloc(WPtr wPtr, int size, out SafePtr ptr)
		{
			return new WPtr(_allocator.MemReAlloc(wPtr, size, out ptr), worldId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemSwap(in WPtr a, int aOffset, in WPtr b, int bOffset, int length)
		{
			_allocator.MemSwap(a, aOffset, b, bOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemCopy(in WPtr source, int sourceOffset, in WPtr dest, int destOffset, int length)
		{
			_allocator.MemCopy(source, sourceOffset, dest, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemCopy<T>(in WPtr source, int sourceIndex, in WPtr dest, int destIndex, int length) where T : unmanaged
		{
			_allocator.MemCopy<T>(source, sourceIndex, dest, destIndex, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemMove(in WPtr source, int sourceOffset, in WPtr dest, int destOffset, int length)
		{
			_allocator.MemMove(source, sourceOffset, dest, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemMove<T>(in WPtr source, int sourceIndex, in WPtr dest, int destIndex, int length) where T : unmanaged
		{
			_allocator.MemMove<T>(source, sourceIndex, dest, destIndex, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemFill<T>(in WPtr ptr, in T value, int index, int length) where T : unmanaged
		{
			_allocator.MemFill(ptr, value, index, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemClear(in WPtr ptr, int index, int size)
		{
			_allocator.MemClear(ptr, index, size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemFree(in WPtr ptr)
		{
			_allocator.MemFree(ptr);
		}
	}
}
