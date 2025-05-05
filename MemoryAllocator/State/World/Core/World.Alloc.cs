using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.MemoryAllocator.Core;

namespace Sapientia.MemoryAllocator
{
	public partial class World
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr MemAlloc(int size)
		{
			return _allocator.MemAlloc(size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr MemAlloc(int size, out SafePtr ptr)
		{
			return _allocator.MemAlloc(size, out ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr MemAlloc<T>() where T: unmanaged
		{
			return _allocator.MemAlloc<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr MemAlloc<T>(out SafePtr<T> ptr) where T: unmanaged
		{
			return _allocator.MemAlloc<T>(out ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr MemAlloc<T>(in T value, out SafePtr<T> ptr) where T: unmanaged
		{
			return _allocator.MemAlloc<T>(value, out ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr MemReAlloc(MemPtr memPtr, int size, out SafePtr ptr)
		{
			return _allocator.MemReAlloc(memPtr, size, out ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemSwap(in MemPtr a, int aOffset, in MemPtr b, int bOffset, int length)
		{
			_allocator.MemSwap(a, aOffset, b, bOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemCopy(in MemPtr source, int sourceOffset, in MemPtr dest, int destOffset, int length)
		{
			_allocator.MemCopy(source, sourceOffset, dest, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemCopy<T>(in MemPtr source, int sourceIndex, in MemPtr dest, int destIndex, int length) where T : unmanaged
		{
			_allocator.MemCopy<T>(source, sourceIndex, dest, destIndex, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemMove(in MemPtr source, int sourceOffset, in MemPtr dest, int destOffset, int length)
		{
			_allocator.MemMove(source, sourceOffset, dest, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemMove<T>(in MemPtr source, int sourceIndex, in MemPtr dest, int destIndex, int length) where T : unmanaged
		{
			_allocator.MemMove<T>(source, sourceIndex, dest, destIndex, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemFill<T>(in MemPtr ptr, in T value, int index, int length) where T : unmanaged
		{
			_allocator.MemFill(ptr, value, index, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemClear(in MemPtr ptr, int index, int size)
		{
			_allocator.MemClear(ptr, index, size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemFree(in MemPtr ptr)
		{
			_allocator.MemFree(ptr);
		}
	}
}
