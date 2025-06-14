using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.Collections
{
#if UNITY_5_3_OR_NEWER
	[Unity.Burst.BurstCompile]
#endif
	[DebuggerTypeProxy(typeof(UnsafeArray<>.UnsafeArrayProxy))]
	public struct UnsafeArray<T> : IDisposable
		where T : unmanaged
	{
		public SafePtr<T> rawArray;
		public int length;
#if UNITY_5_3_OR_NEWER
		public readonly Unity.Collections.Allocator allocator;
#endif

		public int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => length;
		}

		public bool IsValid => rawArray != default;

		public UnsafeArray(int length = 8, bool clearMemory = true)
		{
			this.rawArray = MemoryExt.NoCheckMakeArray<T>(length, clearMemory);
			this.length = length;
#if UNITY_5_3_OR_NEWER
			this.allocator = Unity.Collections.Allocator.None;
#endif
		}

#if UNITY_5_3_OR_NEWER
		public UnsafeArray(int length = 8, Unity.Collections.Allocator allocator = Unity.Collections.Allocator.Persistent, bool clearMemory = true)
		{
			this.rawArray = MemoryExt.NoCheckMakeArray<T>(length, allocator, clearMemory);
			this.length = length;
			this.allocator = allocator;
		}
#endif

		public ref T Last
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref rawArray[Length - 1];
		}

		public SafePtr<T> LastPtr
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => rawArray.Slice(Length - 1, 1);
		}

		public ref T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref rawArray[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Fill(T value)
		{
			MemoryExt.MemFill<T>(value, rawArray, Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			MemoryExt.MemClear(rawArray, Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			if (!IsValid)
				return;

#if UNITY_5_3_OR_NEWER
			if (allocator != Unity.Collections.Allocator.None)
			{
				MemoryExt.NoCheckMemFree(rawArray, allocator);
			}
			else
#endif
			{
				MemoryExt.NoCheckMemFree(rawArray);
			}
			this = default;
		}

		public class UnsafeArrayProxy
		{
			private UnsafeList<T> _arr;

			public UnsafeArrayProxy(UnsafeList<T> arr)
			{
				_arr = arr;
			}

			public int Capacity => _arr.capacity;

			public int Count => _arr.count;

			public T[] Items
			{
				get
				{
					var arr = new T[_arr.count];
					for (var i = 0; i < _arr.count; ++i)
					{
						arr[i] = _arr[i].Value();
					}

					return arr;
				}
			}
		}
	}
}
