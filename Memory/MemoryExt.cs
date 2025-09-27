using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia;
using Sapientia.Data;
using Sapientia.Extensions;
using Submodules.Sapientia.Data;
#if !UNITY_5_3_OR_NEWER || FORCE_MARSHAL_ALLOC
using System.Runtime.InteropServices;
#endif

namespace Submodules.Sapientia.Memory
{
	public static unsafe class MemoryExt
	{
		internal static MemoryType ToMemoryType(this Id<MemoryManager> memoryId)
		{
			var intId = (int)memoryId;
			if (intId >= 0)
				return MemoryType.Invalid;

			return (-intId).ToEnum<MemoryType>();
		}

		public static SentinelPtr<T> NullableMemAlloc<T>() where T : unmanaged
		{
			return MemoryManagerController.GetMemoryManager().NullableMemAlloc<T>();
		}

		public static SafePtr<T> MemAlloc<T>() where T : unmanaged
		{
			return MemoryManagerController.GetMemoryManager().MemAlloc<T>();
		}

		public static SafePtr MemAlloc(int size)
		{
			return MemoryManagerController.GetMemoryManager().MemAlloc(size);
		}

		public static SafePtr<T> MakeArray<T>(int length, ClearOptions clearMemory = ClearOptions.ClearMemory) where T : unmanaged
		{
			return MemoryManagerController.GetMemoryManager().MakeArray<T>(length, clearMemory);
		}

		public static void ResizeArray(ref SafePtr arr, ref int length, int newLength, bool powerOfTwo = false, ClearOptions clearMemory = ClearOptions.UninitializedMemory)
		{
			MemoryManagerController.GetMemoryManager().ResizeArray(ref arr, ref length, newLength, powerOfTwo, clearMemory);
		}

		public static void ResizeArray<T>(ref SafePtr<T> arr, ref int length, int newLength, bool powerOfTwo = false, ClearOptions clearMemory = ClearOptions.UninitializedMemory)
			where T : unmanaged
		{
			MemoryManagerController.GetMemoryManager().ResizeArray<T>(ref arr, ref length, newLength, powerOfTwo, clearMemory);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemFree(SentinelPtr memory)
		{
			MemoryManagerController.GetMemoryManager().MemFree(memory);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemFree(SafePtr memory)
		{
			MemoryManagerController.GetMemoryManager().MemFree(memory);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref MemoryManager GetManager(this Id<MemoryManager> id)
		{
			return ref MemoryManagerController.GetMemoryManager(id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemSet(SafePtr destination, byte value, int size)
		{
			destination.AssertValidLength(size);
#if UNITY_5_3_OR_NEWER
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemSet(destination.ptr, value, size);
#else
			var span = new Span<byte>(destination.ptr, size);
			span.Fill(value);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemFill<T>(T source, SafePtr<T> destination, int length) where T: unmanaged
		{
			destination.AssertValidLength(length);
			var sourcePtr = &source;
#if UNITY_5_3_OR_NEWER
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpyReplicate(destination.ptr, sourcePtr, TSize<T>.size, length);
#else
			var span = new Span<T>(destination.ptr, length);
			span.Fill(source);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemFill<T>(SafePtr<T> source, SafePtr<T> destination, int length) where T: unmanaged
		{
			source.AssertValidLength(length);
			destination.AssertValidLength(length);
#if UNITY_5_3_OR_NEWER
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpyReplicate(destination.ptr, source.ptr, TSize<T>.size, length);
#else
			var span = new Span<T>(destination.ptr, length);
			span.Fill(*source.ptr);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemClear<T>(SafePtr<T> destination, int count) where T: unmanaged
		{
			MemClear((SafePtr)destination, TSize<T>.size * count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemClear(SafePtr destination, int size)
		{
			destination.AssertValidLength(size);
#if UNITY_5_3_OR_NEWER
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemClear(destination.ptr, size);
#else
			var span = new Span<byte>(destination.ptr, size);
			span.Clear();
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemCopy<T>(SafePtr<T> source, SafePtr<T> destination, int length) where T: unmanaged
		{
			source.AssertValidLength(length);
			destination.AssertValidLength(length);
			MemCopy<T>(source.ptr, destination.ptr, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemCopy<T>(T* source, T* destination, int length) where T: unmanaged
		{
			var size = length * TSize<T>.size;
#if UNITY_5_3_OR_NEWER
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpy(destination, source, size);
#else
			Buffer.MemoryCopy(source, source, size, size);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemCopy(SafePtr source, SafePtr destination, int size)
		{
			source.AssertValidLength(size);
			destination.AssertValidLength(size);
#if UNITY_5_3_OR_NEWER
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpy(destination.ptr, source.ptr, size);
#else
			Buffer.MemoryCopy(source.ptr, destination.ptr, size, size);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemSwap<T>(SafePtr<T> a, SafePtr<T> b) where T: unmanaged
		{
			MemSwap((SafePtr)a, (SafePtr)b, TSize<T>.size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemSwap(SafePtr a, SafePtr b, int size)
		{
			a.AssertValidLength(size);
			b.AssertValidLength(size);
#if UNITY_5_3_OR_NEWER
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemSwap(a.ptr, b.ptr, size);
#else
			Span<byte> spanA = new Span<byte>(a.ptr, size);
			Span<byte> spanB = new Span<byte>(b.ptr, size);
			Span<byte> temp = stackalloc byte[size];

			spanA.CopyTo(temp);
			spanB.CopyTo(spanA);
			temp.CopyTo(spanB);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemMove<T>(SafePtr<T> source, SafePtr<T> destination, int count) where T: unmanaged
		{
			MemMove((SafePtr)source, (SafePtr)destination, count * TSize<T>.size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemMove(SafePtr source, SafePtr destination, int size)
		{
			source.AssertValidLength(size);
			destination.AssertValidLength(size);
#if UNITY_5_3_OR_NEWER
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemMove(destination.ptr, source.ptr, size);
#else
			var intSize = (int)size;
			var sourceSpan = new Span<byte>(source.ptr, intSize);
			var destinationSpan = new Span<byte>(destination.ptr, intSize);
			Span<byte> temp = stackalloc byte[intSize];

			sourceSpan.CopyTo(temp);
			temp.CopyTo(destinationSpan);
#endif
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Align(int size, int alignmentPowerOfTwo)
		{
			// Copy of Unity CollectionHelper.Align
			if (alignmentPowerOfTwo == 0)
				return size;

			CheckIntPositivePowerOfTwo(alignmentPowerOfTwo);

			return (size + alignmentPowerOfTwo - 1) & ~(alignmentPowerOfTwo - 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Conditional(E.DEBUG)]
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
