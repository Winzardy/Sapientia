//#define FORCE_MARSHAL_ALLOC
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Data;

#if !UNITY_5_3_OR_NEWER || FORCE_MARSHAL_ALLOC
using System.Runtime.InteropServices;
#endif

namespace Sapientia.Extensions
{
#if UNITY_5_3_OR_NEWER
	using BurstDiscard = Unity.Burst.BurstDiscardAttribute;
#else
	using BurstDiscard = PlaceholderAttribute;
#endif
	public enum ClearOptions
	{
		ClearMemory,
		UninitializedMemory,
	}

	public static unsafe class MemoryExt
	{
#if DEBUG
		private static readonly Dictionary<IntPtr, int> _allocations = new Dictionary<IntPtr, int>(128);
		// Всегда кратно 2, при выделении памяти добавляет в список нижний и верхний предел выделенной памяти
		private static readonly SimpleList<long> _memorySpaces = new SimpleList<long>(128);
#endif

		[BurstDiscard]
		[Conditional(E.DEBUG)]
		private static void DebugMemAlloc(void* ptr, int size)
		{
#if DEBUG
			DebugInsertMemorySpace(ptr, size);
#if UNITY_5_3_OR_NEWER
			UnityEngine.Debug.LogWarning($"MemAlloc: {size}b. Allocations Remains: {_allocations.Count}");
#endif
#endif
		}

		[BurstDiscard]
		[Conditional(E.DEBUG)]
		private static void DebugMemFree(void* ptr)
		{
#if DEBUG
			DebugRemoveMemorySpace(ptr, out var size);
#if UNITY_5_3_OR_NEWER
			UnityEngine.Debug.LogWarning($"MemFree: {size}b. Allocations Remains: {_allocations.Count}");
#endif
#endif
		}

#if DEBUG
		[BurstDiscard]
		private static void DebugRemoveMemorySpace(void* ptr, out int size)
		{
			var intPtr = (IntPtr)ptr;
			E.ASSERT(_allocations.Remove(intPtr, out size));
			var low = intPtr.ToInt64();
			var hi = (intPtr + size).ToInt64() - 1;

			E.ASSERT(DebugIsInBound(low, out var lowIndex));
			E.ASSERT(DebugIsInBound(hi, out var hiIndex));
			E.ASSERT((lowIndex + 1) == hiIndex);

			_memorySpaces.RemoveAt(hiIndex);
			_memorySpaces.RemoveAt(lowIndex);
		}

		[BurstDiscard]
		private static void DebugInsertMemorySpace(void* ptr, int size)
		{
			var intPtr = (IntPtr)ptr;
			var low = intPtr.ToInt64();
			var hi = (intPtr + size).ToInt64() - 1;

			E.ASSERT(!DebugIsInBound(low, out var lowIndex));
			E.ASSERT(!DebugIsInBound(hi, out var hiIndex));
			E.ASSERT(lowIndex == hiIndex);

			_memorySpaces.Insert(hiIndex, hi);
			_memorySpaces.Insert(lowIndex, low);

			_allocations.Add(intPtr, size);
		}

		[BurstDiscard]
		private static bool DebugIsInBound(byte* lowPtr, byte* hiPtr)
		{
			if (!DebugIsInBound(((IntPtr)lowPtr).ToInt64(), out _))
				return false;
			return DebugIsInBound(((IntPtr)(hiPtr - 1)).ToInt64(), out _);
		}

		[BurstDiscard]
		private static bool DebugIsInBound(long ptr, out int index)
		{
			index = _memorySpaces.BinarySearch(ptr);
			if (index >= 0)
				return true;
			index = ~index;

			return index % 2 == 1;
		}
#endif

#if UNITY_5_4_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr MemAlloc(int size, int align, Unity.Collections.Allocator allocator, bool safetyCheck = true)
		{
			var ptr = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Malloc(size, align, allocator);
#if DEBUG
			if (safetyCheck)
				DebugInsertMemorySpace(ptr, size);
#endif

			return new SafePtr(ptr, size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr MemAlloc(int size, Unity.Collections.Allocator allocator)
		{
			return MemAlloc(size, TAlign<byte>.align, allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> MemAlloc<T>(Unity.Collections.Allocator allocator) where T : unmanaged
		{
			return (SafePtr<T>)MemAlloc(TSize<T>.size, TAlign<T>.align, allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> MemAllocAndClear<T>(Unity.Collections.Allocator allocator) where T : unmanaged
		{
			var size = TSize<T>.size;
			var safePtr = MemAlloc(size, TAlign<T>.align, allocator);
			MemClear(safePtr, size);

			return (SafePtr<T>)safePtr;
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr MemAlloc(int size, int align, bool safetyCheck = true)
		{
#if UNITY_5_3_OR_NEWER && !FORCE_MARSHAL_ALLOC
			var ptr = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Malloc(size, align, Unity.Collections.Allocator.Persistent);
#else
			var ptr = (void*)Marshal.AllocHGlobal(size);
#endif
#if DEBUG
			if (safetyCheck)
				DebugMemAlloc(ptr, size);
#endif

			return new SafePtr(ptr, size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr MemAlloc(int size, bool safetyCheck = true)
		{
			return MemAlloc(size, TAlign<byte>.align, safetyCheck);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SentinelPtr<T> NullableMemAlloc<T>() where T : unmanaged
		{
			var ptr = MemAlloc(TSize<T>.size, TAlign<T>.align);
			return SentinelPtr<T>.Create(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> MemAlloc<T>() where T : unmanaged
		{
			return MemAlloc(TSize<T>.size, TAlign<T>.align);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> MemAllocAndClear<T>() where T : unmanaged
		{
			var size = TSize<T>.size;
			var safePtr = MemAlloc(size, TAlign<T>.align);
			MemClear(safePtr, size);

			return (SafePtr<T>)safePtr;
		}

#if UNITY_5_3_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemFree(SafePtr memory, Unity.Collections.Allocator allocator, bool safetyCheck = true)
		{
#if DEBUG
			if (safetyCheck)
				DebugRemoveMemorySpace(memory.ptr, out var size);
#endif
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Free(memory.ptr, allocator);
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemFree(SentinelPtr memory, bool safetyCheck = true)
		{
			MemFree(memory.GetPtr(), safetyCheck);
			memory.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemFree(SafePtr memory, bool safetyCheck = true)
		{
#if DEBUG
			if (safetyCheck)
				DebugMemFree(memory.ptr);
#endif
#if UNITY_5_3_OR_NEWER && !FORCE_MARSHAL_ALLOC
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Free(memory.ptr, Unity.Collections.Allocator.Persistent);
#else
			Marshal.FreeHGlobal((IntPtr)memory.ptr);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemSet(SafePtr destination, byte value, int size)
		{
#if DEBUG
			E.ASSERT(destination.IsValidLength(size));
#endif
#if UNITY_5_3_OR_NEWER
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemSet(destination.ptr, value, size);
#else
			var span = new Span<byte>(destination.ptr, size);
			span.Fill(value);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemFill<T>(T source, SafePtr<T> destination, int count) where T: unmanaged
		{
#if DEBUG
			E.ASSERT(destination.IsLengthInRange(count));
#endif
			var sourcePtr = &source;
#if UNITY_5_3_OR_NEWER
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpyReplicate(destination.ptr, sourcePtr, TSize<T>.size, count);
#else
			var span = new Span<T>(destination.ptr, count);
			span.Fill(source);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemFill<T>(SafePtr<T> source, SafePtr<T> destination, int count) where T: unmanaged
		{
#if DEBUG
			E.ASSERT(source.IsLengthInRange(count));
			E.ASSERT(destination.IsLengthInRange(count));
#endif
#if UNITY_5_3_OR_NEWER
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpyReplicate(destination.ptr, source.ptr, TSize<T>.size, count);
#else
			var span = new Span<T>(destination.ptr, count);
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
#if DEBUG
			E.ASSERT(destination.IsValidLength(size));
#endif
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
#if DEBUG
			E.ASSERT(source.IsLengthInRange(length));
			E.ASSERT(destination.IsLengthInRange(length));
#endif
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
#if DEBUG
			E.ASSERT(source.IsValidLength(size));
			E.ASSERT(destination.IsValidLength(size));
#endif
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
#if DEBUG
			E.ASSERT(a.IsValidLength(size));
			E.ASSERT(b.IsValidLength(size));
#endif
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
#if DEBUG
			E.ASSERT(source.IsValidLength(size));
			E.ASSERT(destination.IsValidLength(size));
#endif
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

#if UNITY_5_4_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> MakeArray<T>(int length, Unity.Collections.Allocator allocator, bool clearMemory = true, bool safetyCheck = true) where T : unmanaged
		{
			var size = TSize<T>.size * length;
			var ptr = MemAlloc(size, TAlign<T>.align, allocator, safetyCheck);
			if (clearMemory)
				MemClear(ptr, size);

			return (SafePtr<T>)ptr;
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> MakeArray<T>(int length, bool clearMemory = true, bool safetyCheck = true) where T : unmanaged
		{
			var size = TSize<T>.size * length;
			var ptr = MemAlloc(size, TAlign<T>.align, safetyCheck);
			if (clearMemory)
				MemClear(ptr, size);

			return (SafePtr<T>)ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ResizeArray(ref SafePtr arr, ref int length, int newLength, bool powerOfTwo = false, bool clearMemory = false, bool safetyCheck = true)
		{
			if (newLength <= length)
				return;
			if (powerOfTwo)
				newLength = newLength.NextPowerOfTwo().Max(8);

			var size = newLength;
			var ptr = MemAlloc(size, safetyCheck);
			if (clearMemory)
				MemClear(ptr, size);

			if (arr != default)
			{
				MemCopy(arr, ptr, length);
				MemFree(arr, safetyCheck);
			}

			arr = ptr;
			length = newLength;
		}

#if UNITY_5_3_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ResizeArray<T>(ref SafePtr<T> arr, ref int length, int newLength, Unity.Collections.Allocator allocator, bool powerOfTwo = false, bool clearMemory = false, bool safetyCheck = true)
			where T : unmanaged
		{
			if (newLength <= length)
				return;
			if (powerOfTwo)
				newLength = newLength.NextPowerOfTwo().Max(8);

			var size = newLength * TSize<T>.size;
			var ptr = MemAlloc(size, TAlign<T>.align, allocator, safetyCheck);
			if (clearMemory)
				MemClear(ptr, size);

			if (arr != default)
			{
				MemCopy<T>(arr, (SafePtr<T>)ptr, length);
				MemFree(arr, allocator, safetyCheck);
			}

			arr = (SafePtr<T>)ptr;
			length = newLength;
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ResizeArray<T>(ref SafePtr<T> arr, ref int length, int newLength, bool powerOfTwo = false, bool clearMemory = false, bool safetyCheck = true)
			where T : unmanaged
		{
			if (newLength <= length)
				return;
			if (powerOfTwo)
				newLength = newLength.NextPowerOfTwo().Max(8);

			var size = newLength * TSize<T>.size;
			var ptr = MemAlloc(size, TAlign<T>.align, safetyCheck);
			if (clearMemory)
				MemClear(ptr, size);

			if (arr != default)
			{
				MemCopy<T>(arr, (SafePtr<T>)ptr, length);
				MemFree(arr, safetyCheck);
			}

			arr = (SafePtr<T>)ptr;
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
