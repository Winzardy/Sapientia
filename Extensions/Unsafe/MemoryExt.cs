using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.Extensions
{
	public enum ClearOptions
	{
		ClearMemory,
		UninitializedMemory,
	}

	public static unsafe class MemoryExt
	{
		[INLINE(256)]
		public static void* MemAlloc(long size)
		{
#if UNITY_5_3_OR_NEWER
			return Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Malloc(size, UnsafeExt.AlignOf<byte>(), Unity.Collections.Allocator.Persistent);
#else
			return (void*)Marshal.AllocHGlobal((int)size);
#endif
		}

		[INLINE(256)]
		public static void* MemAlloc(long size, int align)
		{
#if UNITY_5_3_OR_NEWER
			return Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Malloc(size, align, Unity.Collections.Allocator.Persistent);
#else
			return (void*)Marshal.AllocHGlobal((int)size);
#endif
		}

		[INLINE(256)]
		public static T* MemAlloc<T>(long size) where T : unmanaged
		{
#if UNITY_5_3_OR_NEWER
			return (T*)Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Malloc(size, UnsafeExt.AlignOf<T>(), Unity.Collections.Allocator.Persistent);
#else
			return (T*)Marshal.AllocHGlobal((int)size);
#endif
		}

		[INLINE(256)]
		public static T* MemAlloc<T>() where T : unmanaged
		{
#if UNITY_5_3_OR_NEWER
			return (T*)Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Malloc(TSize<T>.size, UnsafeExt.AlignOf<T>(), Unity.Collections.Allocator.Persistent);
#else
			return (T*)Marshal.AllocHGlobal(TSize<T>.size);
#endif
		}

		[INLINE(256)]
		public static void MemFree(void* memory)
		{
#if UNITY_5_3_OR_NEWER
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Free(memory, Unity.Collections.Allocator.Persistent);
#else
			Marshal.FreeHGlobal((IntPtr)memory);
#endif
		}

		[INLINE(256)]
		public static void MemSet(void* destination, byte value, long size)
		{
#if UNITY_5_3_OR_NEWER
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemSet(destination, value, size);
#else
			var span = new Span<byte>(destination, (int)size);
			span.Fill(value);
#endif
		}

		[INLINE(256)]
		public static void MemFill<T>(T* source, void* destination, int count) where T: unmanaged
		{
#if UNITY_5_3_OR_NEWER
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpyReplicate(source, destination, TSize<T>.size, count);
#else
			var span = new Span<T>(destination, count);
			span.Fill(*source);
#endif
		}

		[INLINE(256)]
		public static void MemClear(void* destination, long size)
		{
#if UNITY_5_3_OR_NEWER
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemSet(destination, 0, size);
#else
			var span = new Span<byte>(destination, (int)size);
			span.Clear();
#endif
		}

		[INLINE(256)]
		public static void MemCopy(void* source, void* destination, long size)
		{
#if UNITY_5_3_OR_NEWER
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpy(destination, source, size);
#else
			Buffer.MemoryCopy(source, destination, size, size);
#endif
		}

		[INLINE(256)]
		public static void MemSwap(void* a, void* b, long size)
		{
#if UNITY_5_3_OR_NEWER
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemSwap(a, b, size);
#else
			Span<byte> spanA = new Span<byte>(a, (int)size);
			Span<byte> spanB = new Span<byte>(b, (int)size);
			Span<byte> temp = stackalloc byte[(int)size];

			spanA.CopyTo(temp);
			spanB.CopyTo(spanA);
			temp.CopyTo(spanB);
#endif
		}

		[INLINE(256)]
		public static void MemMove<T>(T* source, T* destination, int count) where T: unmanaged
		{
			MemMove((void*)source, destination, count * TSize<T>.size);
		}

		[INLINE(256)]
		public static void MemMove(void* source, void* destination, long size)
		{
#if UNITY_5_3_OR_NEWER
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemMove(destination, source, size);
#else
			var intSize = (int)size;
			var sourceSpan = new Span<byte>(source, intSize);
			var destinationSpan = new Span<byte>(destination, intSize);
			Span<byte> temp = stackalloc byte[intSize];

			sourceSpan.CopyTo(temp);
			temp.CopyTo(destinationSpan);
#endif
		}

		[INLINE(256)]
		public static T* MakeArray<T>(in T firstElement, int length, bool clearMemory = false) where T : unmanaged
		{
			var size = TSize<T>.size * length;
			var ptr = MemAlloc(size, TAlign<T>.align);
			if (clearMemory)
				MemClear(ptr, size);

			var tPtr = (T*)ptr;
			*tPtr = firstElement;

			return tPtr;
		}

#if UNITY_5_3_OR_NEWER
		[INLINE(256)]
		public static T* MakeArray<T>(int length, bool clearMemory = true) where T : unmanaged
		{
			var size = TSize<T>.size * length;
			var ptr = MemAlloc(size, TAlign<T>.align);
			if (clearMemory)
				Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemClear(ptr, size);

			return (T*)ptr;
		}
#else
		[INLINE(256)]
		public static T* MakeArray<T>(int length, bool clearMemory = true) where T : unmanaged
		{
			var size = TSize<T>.size * length;
			var ptr = MemAlloc(size, TAlign<T>.align);
			if (clearMemory)
				new Span<byte>(ptr, size).Clear();

			return (T*)ptr;
		}
#endif

		[INLINE(256)]
		public static void ResizeArray<T>(ref T** arr, int length, int newLength) where T : unmanaged
		{
			var ptr = (T**)MemAlloc(newLength * sizeof(T*));
			if (arr != null)
			{
				for (var i = 0; i < length; ++i)
				{
					ptr[i] = arr[i];
				}

				MemFree(arr);
			}

			arr = ptr;
		}

		[INLINE(256)]
		public static void ResizeArray<T>(ref T* arr, ref uint length, uint newLength, bool free = true)
			where T : unmanaged
		{
			if (newLength <= length)
				return;

			var size = newLength * TSize<T>.size;
			var ptr = MemAlloc<T>(size);
			MemClear(ptr, size);

			if (arr != null)
			{
				// MemCopy(arr, ptr, length * TSize<T>.size);
				for (var i = 0; i < length; ++i)
				{
					*(ptr + i) = *(arr + i);
				}

				for (var i = length; i < newLength; ++i)
				{
					*(ptr + i) = default;
				}

				if (free)
					MemFree(arr);
			}

			arr = ptr;
			length = newLength;
		}

		/// <summary>
		/// Returns an allocation size in bytes that factors in alignment.
		/// </summary>
		/// <example><code>
		/// // 55 aligned to 16 is 64.
		/// int size = CollectionHelper.Align(55, 16);
		/// </code></example>
		/// <param name="size">The size to align.</param>
		/// <param name="alignmentPowerOfTwo">A non-zero, positive power of two.</param>
		/// <returns>The smallest integer that is greater than or equal to `size` and is a multiple of `alignmentPowerOfTwo`.</returns>
		/// <exception cref="ArgumentException">Thrown if `alignmentPowerOfTwo` is not a non-zero, positive power of two.</exception>
		public static int Align(int size, int alignmentPowerOfTwo)
		{
			// Copy of Unity CollectionHelper.Align
			if (alignmentPowerOfTwo == 0)
				return size;

			CheckIntPositivePowerOfTwo(alignmentPowerOfTwo);

			return (size + alignmentPowerOfTwo - 1) & ~(alignmentPowerOfTwo - 1);
		}

		[Conditional("UNITY_EDITOR")]
		private static void CheckIntPositivePowerOfTwo(int value)
		{
			// Copy of Unity CollectionHelper.CheckIntPositivePowerOfTwo
			var valid = (value > 0) && value.IsPowerOfTwo();
			if (!valid)
			{
				throw new ArgumentException($"Alignment requested: {value} is not a non-zero, positive power of two.");
			}
		}
	}
}
