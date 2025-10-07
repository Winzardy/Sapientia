using System;
using System.Runtime.CompilerServices;
#if !UNITY_5_3_OR_NEWER || FORCE_MARSHAL_ALLOC
using System.Runtime.InteropServices;
#endif
using Sapientia;
using Sapientia.Data;
using Sapientia.Extensions;
using Submodules.Sapientia.Data;

namespace Submodules.Sapientia.Memory
{
	public enum ClearOptions
	{
		ClearMemory,
		UninitializedMemory,
	}

	internal enum MemoryType
	{
		Invalid = 0,
		Default = 1,
		Temp = 2,
		NoTrack = 3,
		NoTrackTemp = 4,
		Inner = 5,
	}

	public unsafe struct MemoryManager : IDisposable
	{
		internal static readonly Id<MemoryManager> InnerMemoryId = -(int)MemoryType.Inner;

		/// <summary>
		/// Используется в случаях, где нужно параллельно обращаться к памяти из разных потоков.
		/// Сейчас полноценный трекинг не поддерживается в параллельной обработке.
		/// Использует Allocator.Temp в Unity.
		/// </summary>
		public static readonly Id<MemoryManager> NoTrackTempMemoryId = -(int)MemoryType.NoTrackTemp;
		/// <summary>
		/// Используется в случаях, где нужно параллельно обращаться к памяти из разных потоков.
		/// Сейчас полноценный трекинг не поддерживается в параллельной обработке.
		/// Использует Allocator.Persistent в Unity.
		/// </summary>
		public static readonly Id<MemoryManager> NoTrackMemoryId = -(int)MemoryType.NoTrack;
		/// <summary>
		/// Использует Allocator.Temp в Unity.
		/// </summary>
		public static readonly Id<MemoryManager> TempMemoryId = -(int)MemoryType.Temp;
		/// <summary>
		/// Использует Allocator.Persistent в Unity.
		/// Используется по умолчанию везде, где не требуется что-то специфичное.
		/// </summary>
		public static readonly Id<MemoryManager> DefaultMemoryId = -(int)MemoryType.Default;

		/// <summary>
		/// Нужно, как минимум, для дебага
		/// </summary>
		private Id<MemoryManager> _id;
		private MemoryTracker _tracker;
#if UNITY_5_3_OR_NEWER
		private Unity.Collections.Allocator _unityAllocator;
#endif

#if UNITY_5_3_OR_NEWER
		public MemoryManager(Id<MemoryManager> id, Unity.Collections.Allocator unityAllocator = Unity.Collections.Allocator.Persistent)
		{
			_id = id;
			_tracker = MemoryTracker.Invalid;
			_unityAllocator = unityAllocator;
		}
#else
		public MemoryManager(Id<MemoryManager> id)
		{
			_id = id;
			_tracker = MemoryTracker.Invalid;
		}
#endif

		public void SetTracker(TrackingType trackingType = TrackingType.DeepTracking)
		{
			_tracker = new MemoryTracker(trackingType);
		}

		void IDisposable.Dispose()
		{
			Dispose();
		}

		internal void Dispose()
		{
			_tracker.StartDisposeTrackingType();
			var allocations = _tracker.GetAllocations();
			foreach (var entry in allocations)
			{
				var ptr = new SafePtr((void*)entry.key);
				MemFree(ptr);
			}

			_tracker.Dispose();
			this = default;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr MemAlloc(int size, int align)
		{
#if UNITY_5_3_OR_NEWER && !FORCE_MARSHAL_ALLOC
			var ptr = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Malloc(size, align, _unityAllocator);
#else
			var ptr = (void*)Marshal.AllocHGlobal(size);
#endif
			_tracker.Track(ptr, size);

			return new SafePtr(ptr, size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr MemAlloc(int size, ClearOptions clearMemory)
		{
			var ptr = MemAlloc(size, TAlign<byte>.align);
			if (clearMemory == ClearOptions.ClearMemory)
				MemoryExt.MemClear(ptr, size);
			return ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> MemAlloc<T>(ClearOptions clearMemory)
			where T : unmanaged
		{
			var ptr = MemAlloc<T>();
			if (clearMemory == ClearOptions.ClearMemory)
				MemoryExt.MemClear(ptr, TSize<T>.size);
			return ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr MemAlloc(int size)
		{
			return MemAlloc(size, TAlign<byte>.align);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SentinelPtr<T> NullableMemAlloc<T>() where T : unmanaged
		{
			var ptr = MemAlloc(TSize<T>.size, TAlign<T>.align);
			return SentinelPtr<T>.Create(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> MemAlloc<T>() where T : unmanaged
		{
			return MemAlloc(TSize<T>.size, TAlign<T>.align);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> MemAllocAndClear<T>() where T : unmanaged
		{
			var size = TSize<T>.size;
			var safePtr = MemAlloc(size, TAlign<T>.align);
			MemoryExt.MemClear(safePtr, size);

			return (SafePtr<T>)safePtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemFree(SentinelPtr memory)
		{
			MemFree(memory.GetPtr());
			memory.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MemFree(SafePtr memory)
		{
			_tracker.Untrack(memory.ptr);

#if UNITY_5_3_OR_NEWER && !FORCE_MARSHAL_ALLOC
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Free(memory.ptr, _unityAllocator);
#else
			Marshal.FreeHGlobal((IntPtr)memory.ptr);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> MakeArray<T>(int length, ClearOptions clearMemory = ClearOptions.ClearMemory) where T : unmanaged
		{
			E.ASSERT(length > 0, "Попытка аллокации массива нулевой или отрицательной длины");

			var size = TSize<T>.size * length;
			var ptr = MemAlloc(size, TAlign<T>.align);
			if (clearMemory == ClearOptions.ClearMemory)
				MemoryExt.MemClear(ptr, size);

			return (SafePtr<T>)ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ResizeArray(ref SafePtr arr, ref int length, int newLength, bool powerOfTwo = false, ClearOptions clearMemory = ClearOptions.UninitializedMemory)
		{
			if (newLength <= length)
				return;
			if (powerOfTwo)
				newLength = newLength.NextPowerOfTwo().Max(8);

			var size = newLength;
			var ptr = MemAlloc(size);
			if (clearMemory == ClearOptions.ClearMemory)
				MemoryExt.MemClear(ptr, size);

			if (arr != default)
			{
				MemoryExt.MemCopy(arr, ptr, length);
				MemFree(arr);
			}

			arr = ptr;
			length = newLength;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ResizeArray<T>(ref SafePtr<T> arr, ref int length, int newLength, bool powerOfTwo = false, ClearOptions clearMemory = ClearOptions.UninitializedMemory)
			where T : unmanaged
		{
			if (newLength <= length)
				return;
			if (powerOfTwo)
				newLength = newLength.NextPowerOfTwo().Max(8);

			var size = newLength * TSize<T>.size;
			var ptr = MemAlloc(size, TAlign<T>.align);
			if (clearMemory == ClearOptions.ClearMemory)
				MemoryExt.MemClear(ptr, size);

			if (arr != default)
			{
				MemoryExt.MemCopy<T>(arr, (SafePtr<T>)ptr, length);
				MemFree(arr);
			}

			arr = (SafePtr<T>)ptr;
			length = newLength;
		}
	}
}
