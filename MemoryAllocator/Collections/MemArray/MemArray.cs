using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct MemArray
	{
		public static readonly MemArray Empty = new () { ptr = CachedPtr.Invalid, Length = 0, };

		public CachedPtr ptr;
		public int Length { get; private set; }
		public int ElementSize { get; private set; }

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => Length;
		}

		public readonly bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => ptr.IsValid();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemArray(int elementSize, int length, ClearOptions clearOptions = ClearOptions.ClearMemory) : this(WorldManager.CurrentWorldState, elementSize, length, clearOptions)
		{

		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemArray(WorldState worldState, int elementSize, int length, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			this = default;
			ptr = default;
			var memPtr = worldState.MemAlloc(elementSize * length, out var tPtr);
			ptr = new CachedPtr(worldState, tPtr, memPtr);
			ElementSize = elementSize;
			Length = length;

			if (clearOptions == ClearOptions.ClearMemory)
			{
				Clear(worldState);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemArray(in MemArray arr) : this(WorldManager.CurrentWorldState, arr)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemArray(WorldState worldState, in MemArray arr)
		{
			if (arr.Length == 0u)
			{
				this = Empty;
				return;
			}

			ptr = default;
			Length = arr.Length;
			ElementSize = arr.ElementSize;

			var memPtr = worldState.MemAlloc(arr.ElementSize * arr.Length, out var tPtr);
			ptr = new CachedPtr(worldState, tPtr, memPtr);
			MemArrayExt.CopyNoChecks(worldState, in arr, 0, ref this, 0, arr.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReplaceWith(WorldState worldState, in MemArray other)
		{
			if (other.ptr.memPtr == ptr.memPtr)
			{
				return;
			}

			Dispose(worldState);
			this = other;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(WorldState worldState, in MemArray other)
		{
			if (other.ptr.memPtr == ptr.memPtr) return;
			if (!ptr.memPtr.IsValid() && !other.ptr.memPtr.IsValid())
				return;
			if (ptr.memPtr.IsValid() && !other.ptr.memPtr.IsValid())
			{
				Dispose(worldState);
				return;
			}

			if (!ptr.memPtr.IsValid())
				this = new MemArray(worldState, other.ElementSize, other.Length, ClearOptions.ClearMemory);

			MemArrayExt.Copy(worldState, in other, ref this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(WorldState worldState)
		{
			if (ptr.memPtr.IsValid())
			{
				worldState.MemFree(ptr.memPtr);
			}

			this = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetPtr(WorldState worldState)
		{
			return ptr.GetPtr(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr GetWPtr(int index)
		{
			return ptr.memPtr.GetArrayElement(ElementSize, index);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetValue<T>(WorldState worldState, int index) where T: unmanaged
		{
			return ref (GetPtr(worldState).Cast<T>() + index).Value();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr<T>(WorldState worldState) where T: unmanaged
		{
			return GetPtr(worldState).Cast<T>();
		}

		public Span<T> GetSpan<T>(WorldState worldState) where T: unmanaged
		{
			return new Span<T>(GetValuePtr<T>(worldState).ptr, Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr<T>(WorldState worldState, int index) where T: unmanaged
		{
			return GetPtr(worldState).Cast<T>() + index;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetValuePtr(WorldState worldState)
		{
			return GetPtr(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetValuePtr(WorldState worldState, int index)
		{
			return GetPtr(worldState) + (index * ElementSize);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Resize(WorldState worldState, int newLength, int elementSize, ClearOptions options = ClearOptions.ClearMemory)
		{
			if (!IsCreated)
			{
				this = new MemArray(worldState, elementSize, newLength, options);
				return true;
			}

			if (newLength <= Length)
			{
				return false;
			}

			E.ASSERT(elementSize == ElementSize);

			var prevLength = Length;
			var arrPtr = worldState.MemReAlloc(ptr.memPtr, elementSize * newLength, out var rawPtr);
			ptr = new CachedPtr(worldState, rawPtr, arrPtr);

			if (options == ClearOptions.ClearMemory)
			{
				Clear(worldState, prevLength, newLength - prevLength);
			}

			Length = newLength;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Resize<T>(WorldState worldState, int newLength, ClearOptions options = ClearOptions.ClearMemory)
			where T : unmanaged
		{
			var elementSize = TSize<T>.size;
			return Resize(worldState, newLength, elementSize, options);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Fill<T>(WorldState worldState, in T value, int fromIndex, int count) where T: unmanaged
		{
			worldState.MemFill<T>(ptr.memPtr, value, fromIndex, count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(WorldState worldState)
		{
			Clear(worldState, 0, Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(WorldState worldState, int index, int length)
		{
			var size = ElementSize;
			worldState.MemClear(ptr.memPtr, index * size, length * size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains<T, TU>(WorldState worldState, in TU obj) where T: unmanaged where TU : unmanaged, IEquatable<T>
		{
			E.ASSERT(IsCreated);

			var safePtr = GetPtr(worldState).Cast<T>();
			for (int i = 0, cnt = Length; i < cnt; ++i)
			{
				if (obj.Equals((safePtr + i).Value()))
				{
					return true;
				}
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetReservedSizeInBytes()
		{
			return Length * ElementSize;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemListEnumerator<T> GetEnumerator<T>(WorldState worldState) where T: unmanaged
		{
			return new MemListEnumerator<T>(GetValuePtr<T>(worldState), Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemListEnumerable<T> GetEnumerable<T>(WorldState worldState) where T: unmanaged
		{
			return new (GetEnumerator<T>(worldState));
		}
	}
}
