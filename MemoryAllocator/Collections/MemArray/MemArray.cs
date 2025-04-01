using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Data;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	[DebuggerTypeProxy(typeof(MemArrayProxy<>))]
	public unsafe struct MemArray
	{
		public static readonly MemArray Empty = new () { ptr = Ptr.Invalid, Length = 0, };

		public Ptr ptr;
		public int Length { get; private set; }
		public int ElementSize { get; private set; }

		public int Count
		{
			[INLINE(256)] get => Length;
		}

		public readonly bool IsCreated
		{
			[INLINE(256)] get => ptr.IsCreated();
		}

		[INLINE(256)]
		public SafePtr<Allocator> GetAllocatorPtr()
		{
			return ptr.GetAllocatorPtr();
		}

		[INLINE(256)]
		public MemArray(int elementSize, int length, ClearOptions clearOptions = ClearOptions.ClearMemory) : this(AllocatorManager.CurrentAllocatorPtr, elementSize, length, clearOptions)
		{

		}

		[INLINE(256)]
		public MemArray(SafePtr<Allocator> allocator, int elementSize, int length, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			this = default;
			ptr = default;
			var memPtr = allocator.Value().MemAlloc(elementSize * length, out var tPtr);
			ptr = new Ptr(allocator, tPtr, memPtr);
			ElementSize = elementSize;
			Length = length;

			if (clearOptions == ClearOptions.ClearMemory)
			{
				Clear(allocator);
			}
		}

		[INLINE(256)]
		public MemArray(in MemArray arr) : this(AllocatorManager.CurrentAllocatorPtr, arr)
		{
		}

		[INLINE(256)]
		public MemArray(SafePtr<Allocator> allocator, in MemArray arr)
		{
			if (arr.Length == 0u)
			{
				this = Empty;
				return;
			}

			ptr = default;
			Length = arr.Length;
			ElementSize = arr.ElementSize;

			var memPtr = allocator.Value().MemAlloc(arr.ElementSize * arr.Length, out var tPtr);
			ptr = new Ptr(allocator, tPtr, memPtr);
			MemArrayExt.CopyNoChecks(allocator, in arr, 0, ref this, 0, arr.Length);
		}

		[INLINE(256)]
		public MemArray(MemPtr memPtr, int elementSize, int length)
		{
			ptr = new Ptr(memPtr);
			Length = length;
			ElementSize = elementSize;
			ptr = default;
		}

		[INLINE(256)]
		public void ReplaceWith(SafePtr<Allocator> allocator, in MemArray other)
		{
			if (other.ptr.memPtr == ptr.memPtr)
			{
				return;
			}

			Dispose(allocator);
			this = other;
		}

		[INLINE(256)]
		public void CopyFrom(SafePtr<Allocator> allocator, in MemArray other)
		{
			if (other.ptr.memPtr == ptr.memPtr) return;
			if (!ptr.memPtr.IsCreated() && !other.ptr.memPtr.IsCreated())
				return;
			if (ptr.memPtr.IsCreated() && !other.ptr.memPtr.IsCreated())
			{
				Dispose(allocator);
				return;
			}

			if (ptr.memPtr.IsCreated() == false)
				this = new MemArray(allocator, other.ElementSize, other.Length, ClearOptions.ClearMemory);

			MemArrayExt.Copy(allocator, in other, ref this);
		}

		[INLINE(256)]
		public void Dispose(SafePtr<Allocator> allocator)
		{
			if (ptr.memPtr.IsCreated())
			{
				allocator.Value().MemFree(ptr.memPtr);
			}

			this = default;
		}

		[INLINE(256)]
		public SafePtr GetPtr(SafePtr<Allocator> allocator)
		{
			return ptr.GetPtr(allocator);
		}

		[INLINE(256)]
		public SafePtr GetPtr()
		{
			return ptr.GetPtr();
		}

		[INLINE(256)]
		public MemPtr GetAllocPtr(int index)
		{
			return ptr.memPtr.GetArrayElement(ElementSize, index);
		}

		[INLINE(256)]
		public ref T GetValue<T>(SafePtr<Allocator> allocator, int index) where T: unmanaged
		{
			return ref (GetPtr(allocator).Cast<T>() + index).Value();
		}

		[INLINE(256)]
		public ref T GetValue<T>(int index) where T: unmanaged
		{
			return ref (GetPtr().Cast<T>() + index).Value();
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr<T>() where T: unmanaged
		{
			return GetPtr();
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr<T>(SafePtr<Allocator> allocator) where T: unmanaged
		{
			return GetPtr(allocator);
		}

		public Span<T> GetSpan<T>(SafePtr<Allocator> allocator) where T: unmanaged
		{
			return new Span<T>(GetValuePtr<T>(allocator).ptr, Length);
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr<T>(int index) where T: unmanaged
		{
			return GetPtr().Cast<T>() + index;
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr<T>(SafePtr<Allocator> allocator, int index) where T: unmanaged
		{
			return GetPtr(allocator).Cast<T>() + index;
		}

		public SafePtr GetValuePtr()
		{
			return GetPtr();
		}

		[INLINE(256)]
		public SafePtr GetValuePtr(SafePtr<Allocator> allocator)
		{
			return GetPtr(allocator);
		}

		[INLINE(256)]
		public SafePtr GetValuePtr(int index)
		{
			return GetPtr() + (index * ElementSize);
		}

		[INLINE(256)]
		public SafePtr GetValuePtr(SafePtr<Allocator> allocator, int index)
		{
			return GetPtr(allocator) + (index * ElementSize);
		}

		[INLINE(256)]
		public bool Resize(SafePtr<Allocator> allocator, int newLength, int elementSize, ClearOptions options = ClearOptions.ClearMemory)
		{
			if (!IsCreated)
			{
				this = new MemArray(allocator, elementSize, newLength, options);
				return true;
			}

			if (newLength <= Length)
			{
				return false;
			}

			E.ASSERT(elementSize == ElementSize);

			var prevLength = Length;
			var arrPtr = allocator.Value().MemReAlloc(ptr.memPtr, elementSize * newLength, out var rawPtr);
			ptr = new Ptr(allocator, rawPtr, arrPtr);

			if (options == ClearOptions.ClearMemory)
			{
				Clear(allocator, prevLength, newLength - prevLength);
			}

			Length = newLength;
			return true;
		}

		[INLINE(256)]
		public bool Resize<T>(SafePtr<Allocator> allocator, int newLength, ClearOptions options = ClearOptions.ClearMemory)
			where T : unmanaged
		{
			var elementSize = TSize<T>.size;
			return Resize(allocator, newLength, elementSize, options);
		}

		[INLINE(256)]
		public void Fill<T>(SafePtr<Allocator> allocator, in T value, int fromIndex, int count) where T: unmanaged
		{
			allocator.Value().MemFill<T>(ptr.memPtr, value, fromIndex, count);
		}

		[INLINE(256)]
		public void Clear(SafePtr<Allocator> allocator)
		{
			Clear(allocator, 0, Length);
		}

		[INLINE(256)]
		public void Clear(SafePtr<Allocator> allocator, int index, int length)
		{
			var size = ElementSize;
			allocator.Value().MemClear(ptr.memPtr, index * size, length * size);
		}

		[INLINE(256)]
		public void Clear()
		{
			Clear(0, Length);
		}

		[INLINE(256)]
		public void Clear(int index, int length)
		{
			var size = ElementSize;
			var allocator = ptr.memPtr.GetAllocatorPtr();
			allocator.Value().MemClear(ptr.memPtr, index * size, length * size);
		}

		[INLINE(256)]
		public bool Contains<T, TU>(SafePtr<Allocator> allocator, in TU obj) where T: unmanaged where TU : unmanaged, IEquatable<T>
		{
			Debug.Assert(IsCreated);

			var safePtr = GetPtr(allocator).Cast<T>();
			for (int i = 0, cnt = Length; i < cnt; ++i)
			{
				if (obj.Equals((safePtr + i).Value()))
				{
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public int GetReservedSizeInBytes()
		{
			return Length * ElementSize;
		}

		[INLINE(256)]
		public ListEnumerator<T> GetEnumerator<T>(SafePtr<Allocator> allocator) where T: unmanaged
		{
			return new ListEnumerator<T>(GetValuePtr<T>(allocator), Count);
		}

		[INLINE(256)]
		public ListEnumerator<T> GetEnumerator<T>() where T: unmanaged
		{
			return new ListEnumerator<T>(GetValuePtr<T>(), Count);
		}

		[INLINE(256)]
		public ListPtrEnumerator<T> GetPtrEnumerator<T>(SafePtr<Allocator> allocator) where T: unmanaged
		{
			return new ListPtrEnumerator<T>(GetValuePtr(allocator), ElementSize, Count);
		}

		[INLINE(256)]
		public ListPtrEnumerator<T> GetPtrEnumerator<T>() where T: unmanaged
		{
			return new ListPtrEnumerator<T>(GetValuePtr(), ElementSize, Count);
		}

		[INLINE(256)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable<T>(SafePtr<Allocator> allocator) where T: unmanaged
		{
			return new (new (GetValuePtr<T>(allocator), Count));
		}

		[INLINE(256)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable<T>() where T: unmanaged
		{
			return new (new (GetValuePtr<T>(), Count));
		}

		[INLINE(256)]
		public Enumerable<SafePtr<T>, ListPtrEnumerator<T>> GetPtrEnumerable<T>(SafePtr<Allocator> allocator) where T: unmanaged
		{
			return new (new (GetValuePtr(allocator), ElementSize, Count));
		}

		[INLINE(256)]
		public Enumerable<SafePtr<T>, ListPtrEnumerator<T>> GetPtrEnumerable<T>() where T: unmanaged
		{
			return new (new (GetValuePtr(), ElementSize, Count));
		}
	}
}
