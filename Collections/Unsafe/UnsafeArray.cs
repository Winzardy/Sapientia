using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.Collections
{
	public enum ResizeSettings
	{
		CopyOldValues,
		ClearMemory,
		UninitializedMemory
	}

#if UNITY_5_3_OR_NEWER
	[Unity.Burst.BurstCompile]
#endif
	[DebuggerTypeProxy(typeof(UnsafeArray<>.UnsafeArrayProxy))]
	public struct UnsafeArray<T> : IDisposable
		where T : unmanaged
	{
		public SafePtr<T> ptr;
		public int length;
#if UNITY_5_3_OR_NEWER
		private Unity.Collections.Allocator _allocator;
#endif

		public int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => length;
		}

		public bool IsCreated => ptr != default;

		public UnsafeArray(int length = 8, bool clearMemory = true)
		{
			this.ptr = MemoryExt.MakeArray<T>(length, clearMemory, false);
			this.length = length;
#if UNITY_5_3_OR_NEWER
			_allocator = Unity.Collections.Allocator.None;
#endif
		}

#if UNITY_5_3_OR_NEWER
		public UnsafeArray(int length, Unity.Collections.Allocator allocator, bool clearMemory = true)
		{
			this.ptr = MemoryExt.MakeArray<T>(length, allocator, clearMemory, false);
			this.length = length;
			_allocator = allocator;
		}
#endif

		public ref T Last
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref ptr[Length - 1];
		}

		public SafePtr<T> LastPtr
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ptr.Slice(Length - 1, 1);
		}

		public ref T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref ptr[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Fill(T value)
		{
			MemoryExt.MemFill<T>(value, ptr, Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Fill(T value, int index, int count)
		{
			MemoryExt.MemFill<T>(value, ptr + index, count);
		}

		public void CopyFrom(UnsafeArray<T> other)
		{
			Resize(other.length, ResizeSettings.UninitializedMemory);
			MemoryExt.MemCopy(other.ptr, ptr, Length);
		}

		public void Resize(int newLength, ResizeSettings settings = ResizeSettings.CopyOldValues)
		{
			if (length == newLength)
			{
				if (settings == ResizeSettings.ClearMemory)
					Clear();
				return;
			}

#if UNITY_5_3_OR_NEWER
			var newArray = _allocator == Unity.Collections.Allocator.None ?
				new UnsafeArray<T>(newLength, false) :
				new UnsafeArray<T>(newLength, _allocator, false);
#else
			var newArray = new UnsafeArray<T>(newLength, false);
#endif
			switch (settings)
			{
				case ResizeSettings.CopyOldValues:
					MemoryExt.MemCopy<T>(ptr, newArray.ptr, Length.Min(newLength));
					break;
				case ResizeSettings.ClearMemory:
					newArray.Clear();
					break;
			}
			Dispose();

			this = newArray;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			MemoryExt.MemClear(ptr, Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(int startIndex, int count)
		{
			MemoryExt.MemClear(ptr + startIndex, count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			if (!IsCreated)
				return;

#if UNITY_5_3_OR_NEWER
			if (_allocator != Unity.Collections.Allocator.None)
				MemoryExt.MemFree(ptr, _allocator, false);
			else
#endif
			{
				MemoryExt.MemFree(ptr, false);
			}
			this = default;
		}

		public class UnsafeArrayProxy
		{
			private UnsafeArray<T> _arr;

			public UnsafeArrayProxy(UnsafeArray<T> arr)
			{
				_arr = arr;
			}

			public int Length => _arr.length;

			public T[] Items
			{
				get
				{
					var arr = new T[_arr.length];
					for (var i = 0; i < _arr.length; ++i)
					{
						arr[i] = _arr[i];
					}

					return arr;
				}
			}
		}
	}
}
