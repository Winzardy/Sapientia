using Sapientia.Extensions;
#if NO_INLINE
using INLINE = NoInlineAttribute;
#else
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;
#endif

namespace Sapientia.MemoryAllocator
{
	public static unsafe class NativeArrayUtils
	{
		[INLINE(256)]
		public static void Copy<T>(ref Allocator allocator,
			in MemArray<T> fromArr,
			ref MemArray<T> arr) where T : unmanaged
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

		[System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static void Copy<T>(ref Allocator allocator,
			in MemArray<T> fromArr,
			uint sourceIndex,
			ref MemArray<T> arr,
			uint destIndex,
			uint length,
			bool copyExact = false) where T : unmanaged
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

			if (arr.IsCreated == false ||
			    (copyExact == false ? arr.Length < fromArr.Length : arr.Length != fromArr.Length))
			{
				if (arr.IsCreated) arr.Dispose(ref allocator);
				arr = new MemArray<T>(ref allocator, fromArr.Length);
			}

			var size = TSize<T>.size;
			allocator.MemMove(arr.cachedPtr.memPtr, destIndex * size, fromArr.cachedPtr.memPtr, sourceIndex * size, length * size);
		}


		[INLINE(256)]
		public static void CopyNoChecks<T>(ref Allocator allocator,
			in MemArray<T> fromArr,
			uint sourceIndex,
			ref MemArray<T> arr,
			uint destIndex,
			uint length) where T : unmanaged
		{
			var size = sizeof(T);
			allocator.MemCopy(arr.cachedPtr.memPtr, destIndex * size, fromArr.cachedPtr.memPtr, sourceIndex * size, length * size);
		}
	}
}
