using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
	{
		[INLINE(256)]
		public readonly MemPtr GetArrayElementPtr<T>(in MemPtr ptr, int index) where T : unmanaged
		{
			var size = TSize<T>.size;
			return new MemPtr(ptr.zoneId, ptr.zoneOffset + index * size, allocatorId);
		}

		[INLINE(256)]
		public readonly MemPtr GetArrayElementPtr(in MemPtr ptr, int size, int index)
		{
			return new MemPtr(ptr.zoneId, ptr.zoneOffset + index * size, allocatorId);
		}

		[INLINE(256)]
		public readonly ref T RefArrayElement<T>(in MemPtr ptr, int index) where T : unmanaged
		{
			var size = TSize<T>.size;
			return ref *(T*)GetUnsafePtr(in ptr, index * size);
		}

		[INLINE(256)]
		public MemPtr ReAllocArray<T>(in MemPtr ptr, int newLength) where T : unmanaged
		{
			var size = TSize<T>.size;
			return MemReAlloc(in ptr, size * newLength);
		}

		[INLINE(256)]
		public MemPtr ReAllocArray<T>(in MemPtr memPtr, int newLength, out T* ptr) where T : unmanaged
		{
			var size = TSize<T>.size;
			var newPtr = MemReAlloc(in memPtr, (size * newLength), out var voidPtr);
			ptr = (T*)voidPtr;
			return newPtr;
		}

		[INLINE(256)]
		public MemPtr ReAllocArray(in MemPtr ptr, int elementSizeOf, int newLength)
		{
			return MemReAlloc(ptr, (elementSizeOf * newLength));
		}

		[INLINE(256)]
		public MemPtr ReAllocArray(in MemPtr ptr, int elementSizeOf, int newLength, out void* voidPtr)
		{
			return MemReAlloc(in ptr, (elementSizeOf * newLength), out voidPtr);
		}

		[INLINE(256)]
		public MemPtr AllocArray<T>(int length) where T : struct
		{
			var size = TSize<T>.size;
			return MemAlloc(size * length);
		}

		[INLINE(256)]
		public MemPtr AllocArray(int elementSize, int length)
		{
			return MemAlloc(elementSize * length);
		}

		[INLINE(256)]
		public MemPtr AllocArray(int elementSize, int length, out void* ptr)
		{
			return MemAlloc(elementSize * length, out ptr);
		}

		[INLINE(256)]
		public MemPtr AllocArray<T>(int length, out T* ptr) where T : unmanaged
		{
			var size = TSize<T>.size;
			var memPtr = MemAlloc(size * length, out var voidPtr);
			ptr = (T*)voidPtr;
			return memPtr;
		}
	}
}
