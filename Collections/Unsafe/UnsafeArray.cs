using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Submodules.Sapientia.Data;
using Submodules.Sapientia.Memory;

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

		public readonly Id<MemoryManager> memoryId;

		public int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => length;
		}

		public bool IsCreated => ptr != default;

		public UnsafeArray(int length = 8, ClearOptions clearMemory = ClearOptions.ClearMemory) : this(default, length, clearMemory)
		{
		}

		public UnsafeArray(Id<MemoryManager> memoryId, int length = 8, ClearOptions clearMemory = ClearOptions.ClearMemory)
		{
			this.ptr = memoryId.GetManager().MakeArray<T>(length, clearMemory);
			this.length = length;
			this.memoryId = memoryId;
		}

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

			var newArray = new UnsafeArray<T>(memoryId, newLength, ClearOptions.UninitializedMemory);
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

			memoryId.GetManager().MemFree(ptr);
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
