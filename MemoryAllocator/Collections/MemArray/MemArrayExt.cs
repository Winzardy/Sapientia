using Sapientia.Extensions;
#if NO_INLINE
using INLINE = NoInlineAttribute;
#else
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;
#endif

namespace Sapientia.MemoryAllocator
{
	public static unsafe class MemArrayExt
	{
		[INLINE(256)]
		public static void Copy<T>(ref Allocator allocator,
			MemArray<T> fromArr,
			ref MemArray<T> arr) where T : unmanaged
		{
			Copy(ref allocator, fromArr.As<MemArray<T>, MemArray>(), 0, ref arr.As<MemArray<T>, MemArray>(), 0, fromArr.Length);
		}

		[INLINE(256)]
		public static void Copy(ref Allocator allocator,
			in MemArray fromArr,
			ref MemArray arr)
		{
			Copy(ref allocator, fromArr, 0, ref arr, 0, fromArr.Length);
		}

		[INLINE(256)]
		public static void CopyExact<T>(ref Allocator allocator,
			in MemArray<T> fromArr,
			ref MemArray<T> arr) where T : unmanaged
		{
			Copy(ref allocator, fromArr, 0, ref arr, 0, fromArr.Length, true);
		}

		[INLINE(256)]
		public static void Copy<T>(ref Allocator allocator,
			MemArray<T> fromArr,
			uint sourceIndex,
			ref MemArray<T> arr,
			uint destIndex,
			uint length,
			bool copyExact = false) where T : unmanaged
		{
			Copy(ref allocator, fromArr.As<MemArray<T>, MemArray>(), sourceIndex, ref arr.As<MemArray<T>, MemArray>(), destIndex, length, copyExact);
		}

		[INLINE(256)]
		public static void Copy(ref Allocator allocator,
			in MemArray fromArr,
			uint sourceIndex,
			ref MemArray arr,
			uint destIndex,
			uint length,
			bool copyExact = false)
		{
			switch (fromArr.IsCreated)
			{
				case false when arr.IsCreated == false:
					return;

				case false when arr.IsCreated:
					arr.Dispose(ref allocator);
					arr = default;
					return;
			}

			var size = fromArr.ElementSize;
			if (arr.IsCreated == false ||
			    (copyExact == false ? arr.Length < fromArr.Length : arr.Length != fromArr.Length))
			{
				if (arr.IsCreated) arr.Dispose(ref allocator);
				arr = new MemArray(ref allocator, size, fromArr.Length);
			}

			allocator.MemMove(arr.ptr.memPtr, destIndex * size, fromArr.ptr.memPtr, sourceIndex * size, length * size);
		}

		[INLINE(256)]
		public static void CopyNoChecks<T>(ref Allocator allocator,
			MemArray<T> fromArr,
			uint sourceIndex,
			ref MemArray<T> arr,
			uint destIndex,
			uint length) where T : unmanaged
		{
			CopyNoChecks(ref allocator, fromArr.As<MemArray<T>, MemArray>(), sourceIndex, ref arr.As<MemArray<T>, MemArray>(), destIndex, length);
		}

		[INLINE(256)]
		public static void CopyNoChecks(ref Allocator allocator,
			in MemArray fromArr,
			uint sourceIndex,
			ref MemArray arr,
			uint destIndex,
			uint length)
		{
			var size = fromArr.ElementSize;
			allocator.MemCopy(arr.ptr.memPtr, destIndex * size, fromArr.ptr.memPtr, sourceIndex * size, length * size);
		}

		[INLINE(256)]
		public static void SwapElements(in Allocator allocator,
			in MemArray aArr,
			uint aIndex,
			in MemArray bArr,
			uint bIndex)
		{
			var size = aArr.ElementSize;
			allocator.MemSwap(aArr.ptr.memPtr, aIndex * size, bArr.ptr.memPtr, bIndex * size, size);
		}
	}
}
