using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Data;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	[DebuggerTypeProxy(typeof(MemArrayProxy<>))]
	public unsafe struct MemArray : IIsCreated, IListEnumerable
	{
		public static readonly MemArray Empty = new () { ptr = Ptr.Invalid, Length = 0, GrowFactor = 0, };

		public Ptr ptr;
		public int GrowFactor { get; set; }
		public int Length { get; private set; }
		public int ElementSize { get; private set; }

		public int Count
		{
			[INLINE(256)] get => Length;
		}

		public readonly bool IsCreated
		{
			[INLINE(256)] get => ptr.IsValid();
		}

		[INLINE(256)]
		public Allocator* GetAllocatorPtr()
		{
			return ptr.GetAllocatorPtr();
		}

		[INLINE(256)]
		public MemArray(int elementSize, int length, ClearOptions clearOptions = ClearOptions.ClearMemory, int growFactor = 1) : this(AllocatorManager.CurrentAllocatorPtr, elementSize, length, clearOptions, growFactor)
		{

		}

		[INLINE(256)]
		public MemArray(Allocator* allocator, int elementSize, int length, ClearOptions clearOptions = ClearOptions.ClearMemory, int growFactor = 1)
		{
			if (length == 0u)
			{
				this = Empty;
				return;
			}

			this = default;
			ptr = default;
			var memPtr = allocator->AllocArray(elementSize, length, out var tPtr);
			ptr = new Ptr(allocator, tPtr, memPtr);
			ElementSize = elementSize;
			Length = length;
			GrowFactor = growFactor;

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
		public MemArray(Allocator* allocator, in MemArray arr)
		{
			if (arr.Length == 0u)
			{
				this = Empty;
				return;
			}

			ptr = default;
			Length = arr.Length;
			ElementSize = arr.ElementSize;
			GrowFactor = arr.GrowFactor;

			var memPtr = allocator->AllocArray(arr.ElementSize, arr.Length, out var tPtr);
			ptr = new Ptr(allocator, tPtr, memPtr);
			MemArrayExt.CopyNoChecks(allocator, in arr, 0, ref this, 0, arr.Length);
		}

		[INLINE(256)]
		public MemArray(MemPtr memPtr, int elementSize, int length, int growFactor)
		{
			ptr = new Ptr(memPtr);
			Length = length;
			ElementSize = elementSize;
			GrowFactor = growFactor;
			ptr = default;
		}

		[INLINE(256)]
		public void ReplaceWith(Allocator* allocator, in MemArray other)
		{
			if (other.ptr.memPtr == ptr.memPtr)
			{
				return;
			}

			Dispose(allocator);
			this = other;
		}

		[INLINE(256)]
		public void CopyFrom(Allocator* allocator, in MemArray other)
		{
			if (other.ptr.memPtr == ptr.memPtr) return;
			if (!ptr.memPtr.IsValid() && !other.ptr.memPtr.IsValid())
				return;
			if (ptr.memPtr.IsValid() && !other.ptr.memPtr.IsValid())
			{
				Dispose(allocator);
				return;
			}

			if (ptr.memPtr.IsValid() == false)
				this = new MemArray(allocator, other.ElementSize, other.Length, ClearOptions.ClearMemory);

			MemArrayExt.Copy(allocator, in other, ref this);
		}

		[INLINE(256)]
		public void Dispose(Allocator* allocator)
		{
			if (ptr.memPtr.IsValid())
			{
				allocator->Free(ptr.memPtr);
			}

			this = default;
		}

		[INLINE(256)]
		public void* GetPtr(Allocator* allocator)
		{
			return ptr.GetPtr(allocator);
		}

		[INLINE(256)]
		public void* GetPtr()
		{
			return ptr.GetPtr();
		}

		[INLINE(256)]
		public MemPtr GetAllocPtr(Allocator* allocator, int index)
		{
			return allocator->RefArrayPtr(ptr.memPtr, ElementSize, index);
		}

		[INLINE(256)]
		public MemPtr GetAllocPtr(int index)
		{
			var allocator = ptr.memPtr.GetAllocatorPtr();
			return allocator->RefArrayPtr(ptr.memPtr, ElementSize, index);
		}

		[INLINE(256)]
		public ref T GetValue<T>(Allocator* allocator, int index) where T: unmanaged
		{
			return ref *((T*)GetPtr(allocator) + index);
		}

		[INLINE(256)]
		public ref T GetValue<T>(int index) where T: unmanaged
		{
			return ref *((T*)GetPtr() + index);
		}

		[INLINE(256)]
		public T* GetValuePtr<T>() where T: unmanaged
		{
			return (T*)GetPtr();
		}

		[INLINE(256)]
		public T* GetValuePtr<T>(Allocator* allocator) where T: unmanaged
		{
			return (T*)GetPtr(allocator);
		}

		[INLINE(256)]
		public T* GetValuePtr<T>(int index) where T: unmanaged
		{
			return (T*)GetPtr() + index;
		}

		[INLINE(256)]
		public T* GetValuePtr<T>(Allocator* allocator, int index) where T: unmanaged
		{
			return (T*)GetPtr(allocator) + index;
		}

		public void* GetValuePtr()
		{
			return GetPtr();
		}

		[INLINE(256)]
		public void* GetValuePtr(Allocator* allocator)
		{
			return GetPtr(allocator);
		}

		[INLINE(256)]
		public void* GetValuePtr(int index)
		{
			return (byte*)GetPtr() + (index * ElementSize);
		}

		[INLINE(256)]
		public void* GetValuePtr(Allocator* allocator, int index)
		{
			return (byte*)GetPtr(allocator) + (index * ElementSize);
		}

		[INLINE(256)]
		public bool Resize(Allocator* allocator, int newLength,
			ClearOptions options = ClearOptions.ClearMemory)
		{
			return Resize(allocator, newLength, GrowFactor, options);
		}

		[INLINE(256)]
		public bool Resize(Allocator* allocator, int newLength, int growFactor, ClearOptions options = ClearOptions.ClearMemory)
		{
			if (!IsCreated)
			{
				this = new MemArray(allocator, ElementSize, newLength, options, growFactor);
				return true;
			}

			if (newLength <= Length)
			{
				return false;
			}

			newLength *= growFactor;

			var prevLength = Length;
			var arrPtr = allocator->ReAllocArray(this.ptr.memPtr, ElementSize, newLength, out void* ptr);
			this.ptr = new Ptr(allocator, ptr, arrPtr);
			if (options == ClearOptions.ClearMemory)
			{
				Clear(allocator, prevLength, newLength - prevLength);
			}

			Length = newLength;
			return true;
		}

		[INLINE(256)]
		public void Fill<T>(Allocator* allocator, in T value, int from, int to) where T: unmanaged
		{
			allocator->MemFill(ptr.memPtr, value, from, to - from);
		}

		[INLINE(256)]
		public void Clear(Allocator* allocator)
		{
			Clear(allocator, 0, Length);
		}

		[INLINE(256)]
		public void Clear(Allocator* allocator, int index, int length)
		{
			var size = ElementSize;
			allocator->MemClear(ptr.memPtr, index * size, length * size);
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
			allocator->MemClear(ptr.memPtr, index * size, length * size);
		}

		[INLINE(256)]
		public bool Contains<T, TU>(Allocator* allocator, in TU obj) where T: unmanaged where TU : unmanaged, IEquatable<T>
		{
			Debug.Assert(IsCreated);

			var ptr = (T*)GetPtr(allocator);
			for (int i = 0, cnt = Length; i < cnt; ++i)
			{
				if (obj.Equals(*(ptr + i)))
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
		public ListEnumerator<T> GetEnumerator<T>(Allocator* allocator) where T: unmanaged
		{
			return new ListEnumerator<T>(GetValuePtr<T>(allocator), Count);
		}

		[INLINE(256)]
		public ListEnumerator<T> GetEnumerator<T>() where T: unmanaged
		{
			return new ListEnumerator<T>(GetValuePtr<T>(), Count);
		}

		[INLINE(256)]
		public ListPtrEnumerator GetPtrEnumerator(Allocator* allocator)
		{
			return new ListPtrEnumerator((byte*)GetValuePtr(allocator), ElementSize, Count);
		}

		[INLINE(256)]
		public ListPtrEnumerator GetPtrEnumerator()
		{
			return new ListPtrEnumerator((byte*)GetValuePtr(), ElementSize, Count);
		}

		[INLINE(256)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable<T>(Allocator* allocator) where T: unmanaged
		{
			return new (new (GetValuePtr<T>(allocator), Count));
		}

		[INLINE(256)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable<T>() where T: unmanaged
		{
			return new (new (GetValuePtr<T>(), Count));
		}

		[INLINE(256)]
		public Enumerable<IntPtr, ListPtrEnumerator> GetPtrEnumerable(Allocator* allocator)
		{
			return new (new ((byte*)GetValuePtr(allocator), ElementSize, Count));
		}

		[INLINE(256)]
		public Enumerable<IntPtr, ListPtrEnumerator> GetPtrEnumerable()
		{
			return new (new ((byte*)GetValuePtr(), ElementSize, Count));
		}

		[INLINE(256)]
		IEnumerator<IntPtr> IEnumerable<IntPtr>.GetEnumerator()
		{
			return GetPtrEnumerator();
		}

		[INLINE(256)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetPtrEnumerator();
		}
	}
}
