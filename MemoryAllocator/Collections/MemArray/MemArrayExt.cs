#if NO_INLINE
using INLINE = NoInlineAttribute;
#else
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;
#endif
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	public static unsafe class MemArrayExt
	{
		[INLINE(256)]
		public static void Copy<T>(Allocator* allocator,
			MemArray<T> fromArr,
			ref MemArray<T> arr) where T : unmanaged
		{
			Copy(allocator, fromArr.As<MemArray<T>, MemArray>(), 0, ref arr.As<MemArray<T>, MemArray>(), 0, fromArr.Length);
		}

		[INLINE(256)]
		public static void Copy(Allocator* allocator,
			in MemArray fromArr,
			ref MemArray arr)
		{
			Copy(allocator, fromArr, 0, ref arr, 0, fromArr.Length);
		}

		[INLINE(256)]
		public static void CopyExact<T>(Allocator* allocator,
			in MemArray<T> fromArr,
			ref MemArray<T> arr) where T : unmanaged
		{
			Copy(allocator, fromArr, 0, ref arr, 0, fromArr.Length, true);
		}

		[INLINE(256)]
		public static void Copy<T>(Allocator* allocator,
			MemArray<T> fromArr,
			int sourceIndex,
			ref MemArray<T> arr,
			int destIndex,
			int length,
			bool copyExact = false) where T : unmanaged
		{
			Copy(allocator, fromArr.As<MemArray<T>, MemArray>(), sourceIndex, ref arr.As<MemArray<T>, MemArray>(), destIndex, length, copyExact);
		}

		[INLINE(256)]
		public static void Copy(Allocator* allocator,
			in MemArray fromArr,
			int sourceIndex,
			ref MemArray arr,
			int destIndex,
			int length,
			bool copyExact = false)
		{
			switch (fromArr.IsCreated)
			{
				case false when arr.IsCreated == false:
					return;

				case false when arr.IsCreated:
					arr.Dispose(allocator);
					arr = default;
					return;
			}

			var size = fromArr.ElementSize;
			if (arr.IsCreated == false ||
			    (copyExact == false ? arr.Length < fromArr.Length : arr.Length != fromArr.Length))
			{
				if (arr.IsCreated) arr.Dispose(allocator);
				arr = new MemArray(allocator, size, fromArr.Length);
			}

			allocator->MemMove(arr.ptr.memPtr, destIndex * size, fromArr.ptr.memPtr, sourceIndex * size, length * size);
		}

		[INLINE(256)]
		public static void CopyNoChecks<T>(Allocator* allocator,
			MemArray<T> fromArr,
			int sourceIndex,
			ref MemArray<T> arr,
			int destIndex,
			int length) where T : unmanaged
		{
			CopyNoChecks(allocator, fromArr.As<MemArray<T>, MemArray>(), sourceIndex, ref arr.As<MemArray<T>, MemArray>(), destIndex, length);
		}

		[INLINE(256)]
		public static void CopyNoChecks(Allocator* allocator,
			in MemArray fromArr,
			int sourceIndex,
			ref MemArray arr,
			int destIndex,
			int length)
		{
			var size = fromArr.ElementSize;
			allocator->MemCopy(arr.ptr.memPtr, destIndex * size, fromArr.ptr.memPtr, sourceIndex * size, length * size);
		}

		[INLINE(256)]
		public static void SwapElements(Allocator* allocator,
			in MemArray aArr,
			int aIndex,
			in MemArray bArr,
			int bIndex)
		{
			var size = aArr.ElementSize;
			allocator->MemSwap(aArr.ptr.memPtr, aIndex * size, bArr.ptr.memPtr, bIndex * size, size);
		}
	}
}
