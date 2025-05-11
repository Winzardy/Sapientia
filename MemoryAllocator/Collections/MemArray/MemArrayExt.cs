#if NO_INLINE
using INLINE = NoInlineAttribute;
#else
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;
#endif
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	public static unsafe class MemArrayExt
	{
		[INLINE(256)]
		public static void Copy<T>(World world, MemArray<T> fromArr, ref MemArray<T> arr) where T : unmanaged
		{
			Copy(world, fromArr.As<MemArray<T>, MemArray>(), 0, ref arr.As<MemArray<T>, MemArray>(), 0, fromArr.Length);
		}

		[INLINE(256)]
		public static void Copy(World world, in MemArray fromArr, ref MemArray arr)
		{
			Copy(world, fromArr, 0, ref arr, 0, fromArr.Length);
		}

		[INLINE(256)]
		public static void CopyExact<T>(World world, in MemArray<T> fromArr, ref MemArray<T> arr) where T : unmanaged
		{
			Copy(world, fromArr, 0, ref arr, 0, fromArr.Length, true);
		}

		[INLINE(256)]
		public static void Copy<T>(World world, MemArray<T> fromArr, int sourceIndex, ref MemArray<T> arr, int destIndex, int length, bool copyExact = false) where T : unmanaged
		{
			Copy(world, fromArr.As<MemArray<T>, MemArray>(), sourceIndex, ref arr.As<MemArray<T>, MemArray>(), destIndex, length, copyExact);
		}

		[INLINE(256)]
		public static void Copy(World world, in MemArray fromArr, int sourceIndex, ref MemArray arr, int destIndex, int length, bool copyExact = false)
		{
			switch (fromArr.IsCreated)
			{
				case false when arr.IsCreated == false:
					return;

				case false when arr.IsCreated:
					arr.Dispose(world);
					arr = default;
					return;
			}

			var size = fromArr.ElementSize;
			if (arr.IsCreated == false || (copyExact == false ? arr.Length < fromArr.Length : arr.Length != fromArr.Length))
			{
				if (arr.IsCreated) arr.Dispose(world);
				arr = new MemArray(world, size, fromArr.Length);
			}

			world.MemMove(arr.ptr.memPtr, destIndex * size, fromArr.ptr.memPtr, sourceIndex * size, length * size);
		}

		[INLINE(256)]
		public static void CopyNoChecks<T>(World world, MemArray<T> fromArr, int sourceIndex, ref MemArray<T> arr, int destIndex, int length) where T : unmanaged
		{
			CopyNoChecks(world, fromArr.As<MemArray<T>, MemArray>(), sourceIndex, ref arr.As<MemArray<T>, MemArray>(), destIndex, length);
		}

		[INLINE(256)]
		public static void CopyNoChecks(World world, in MemArray fromArr, int sourceIndex, ref MemArray destArr, int destIndex, int length)
		{
			E.ASSERT(fromArr.ElementSize == destArr.ElementSize);

			var size = fromArr.ElementSize;
			world.MemCopy(fromArr.ptr.memPtr, sourceIndex * size, destArr.ptr.memPtr, destIndex * size, length * size);
		}

		[INLINE(256)]
		public static void SwapElements(World world, in MemArray aArr, int aIndex, in MemArray bArr, int bIndex)
		{
			var size = aArr.ElementSize;
			world.MemSwap(aArr.ptr.memPtr, aIndex * size, bArr.ptr.memPtr, bIndex * size, size);
		}
	}
}
