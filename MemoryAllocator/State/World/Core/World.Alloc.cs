using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public partial struct World
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr MemAlloc(int size)
		{
			return GetAllocator().MemAlloc(size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr MemAlloc(int size, out SafePtr ptr)
		{
			return GetAllocator().MemAlloc(size, out ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr MemAlloc<T>() where T: unmanaged
		{
			return GetAllocator().MemAlloc<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr MemAlloc<T>(out SafePtr<T> ptr) where T: unmanaged
		{
			return GetAllocator().MemAlloc<T>(out ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr MemAlloc<T>(in T value, out SafePtr<T> ptr) where T: unmanaged
		{
			return GetAllocator().MemAlloc<T>(value, out ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr MemReAlloc(MemPtr memPtr, int size, out SafePtr ptr)
		{
			return GetAllocator().MemReAlloc(memPtr, size, out ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemSwap(in MemPtr a, int aOffset, in MemPtr b, int bOffset, int length)
		{
			GetAllocator().MemSwap(a, aOffset, b, bOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemCopy(in MemPtr source, int sourceOffset, in MemPtr dest, int destOffset, int length)
		{
			GetAllocator().MemCopy(source, sourceOffset, dest, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemCopy<T>(in MemPtr source, int sourceIndex, in MemPtr dest, int destIndex, int length) where T : unmanaged
		{
			GetAllocator().MemCopy<T>(source, sourceIndex, dest, destIndex, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemMove(in MemPtr source, int sourceOffset, in MemPtr dest, int destOffset, int length)
		{
			GetAllocator().MemMove(source, sourceOffset, dest, destOffset, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemMove<T>(in MemPtr source, int sourceIndex, in MemPtr dest, int destIndex, int length) where T : unmanaged
		{
			GetAllocator().MemMove<T>(source, sourceIndex, dest, destIndex, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemFill<T>(in MemPtr ptr, in T value, int index, int length) where T : unmanaged
		{
			GetAllocator().MemFill(ptr, value, index, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemClear(in MemPtr ptr, int index, int size)
		{
			GetAllocator().MemClear(ptr, index, size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemFree(in MemPtr ptr)
		{
			GetAllocator().MemFree(ptr);
		}
	}
}
