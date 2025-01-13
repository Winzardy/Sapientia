using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(MemArrayProxy<>))]
	public unsafe struct MemArray<T> : IIsCreated, IListEnumerable<T> where T : unmanaged
	{
		public static readonly MemArray<T> Empty = new () { innerArray = MemArray.Empty };

		public MemArray innerArray;

		public readonly int Length
		{
			[INLINE(256)] get => innerArray.Length;
		}

		public readonly bool IsCreated
		{
			[INLINE(256)] get => innerArray.IsCreated;
		}

		public readonly int Count
		{
			[INLINE(256)] get => innerArray.Length;
		}

		public int ElementSize
		{
			[INLINE(256)] get => innerArray.ElementSize;
		}

		[INLINE(256)]
		public Allocator* GetAllocatorPtr()
		{
			return innerArray.GetAllocatorPtr();
		}

		[INLINE(256)]
		public MemArray(int length, ClearOptions clearOptions = ClearOptions.ClearMemory, int growFactor = 1) : this(AllocatorManager.CurrentAllocatorPtr, length, clearOptions, growFactor)
		{

		}

		[INLINE(256)]
		public MemArray(Allocator* allocator, int length, ClearOptions clearOptions = ClearOptions.ClearMemory, int growFactor = 1)
		{
			innerArray = new MemArray(allocator, TSize<T>.size, length, clearOptions, growFactor);
		}

		[INLINE(256)]
		public MemArray(int elementSize, int length, ClearOptions clearOptions, int growFactor = 1) : this(AllocatorManager.CurrentAllocatorPtr, elementSize, length, clearOptions, growFactor)
		{

		}

		[INLINE(256)]
		public MemArray(Allocator* allocator, int elementSize, int length, ClearOptions clearOptions, int growFactor = 1)
		{
			innerArray = new MemArray(allocator, elementSize, length, clearOptions, growFactor);
		}

		[INLINE(256)]
		public MemArray(in MemArray<T> arr) : this(AllocatorManager.CurrentAllocatorPtr, arr)
		{
		}

		[INLINE(256)]
		public MemArray(Allocator* allocator, MemArray<T> arr)
		{
			innerArray = new MemArray(allocator, arr.As<MemArray<T>, MemArray>());
		}

		[INLINE(256)]
		public MemArray(MemPtr memPtr, int length, int growFactor)
		{
			innerArray = new MemArray(memPtr, TSize<T>.size, length, growFactor);
		}

		[INLINE(256)]
		public ref TU As<TU>(Allocator* allocator, int index) where TU : unmanaged
		{
			return ref innerArray.GetValue<TU>(allocator, index);
		}

		[INLINE(256)]
		public void ReplaceWith(Allocator* allocator, MemArray<T> other)
		{
			innerArray.ReplaceWith(allocator, other.As<MemArray<T>, MemArray>());
		}

		[INLINE(256)]
		public void CopyFrom(Allocator* allocator, MemArray<T> other)
		{
			innerArray.CopyFrom(allocator, other.As<MemArray<T>, MemArray>());;
		}

		[INLINE(256)]
		public void Dispose(Allocator* allocator)
		{
			innerArray.Dispose(allocator);
		}

		[INLINE(256)]
		public void* GetPtr(Allocator* allocator)
		{
			return innerArray.GetPtr(allocator);
		}

		[INLINE(256)]
		public void* GetPtr()
		{
			return innerArray.GetPtr();
		}

		[INLINE(256)]
		public MemPtr GetAllocPtr(Allocator* allocator, int index)
		{
			return innerArray.GetAllocPtr(allocator, index);
		}

		public ref T this[int index]
		{
			[INLINE(256)]
			get => ref innerArray.GetValue<T>(index);
		}

		[INLINE(256)]
		public MemPtr GetAllocPtr(int index)
		{
			return innerArray.GetAllocPtr(index);
		}

		public ref T this[Allocator* allocator, int index]
		{
			[INLINE(256)]
			get => ref innerArray.GetValue<T>(allocator, index);
		}

		[INLINE(256)]
		public T* GetValuePtr()
		{
			return innerArray.GetValuePtr<T>();
		}

		[INLINE(256)]
		public readonly T* GetValuePtr(Allocator* allocator)
		{
			return innerArray.GetValuePtr<T>(allocator);
		}

		[INLINE(256)]
		public T* GetValuePtr(int index)
		{
			return innerArray.GetValuePtr<T>(index);
		}

		[INLINE(256)]
		public T* GetValuePtr(Allocator* allocator, int index)
		{
			return innerArray.GetValuePtr<T>(allocator, index);
		}

		[INLINE(256)]
		public bool Resize(Allocator* allocator, int newLength,
			ClearOptions options = ClearOptions.ClearMemory)
		{
			return innerArray.Resize(allocator, newLength, options);
		}

		[INLINE(256)]
		public bool Resize(Allocator* allocator, int newLength, int growFactor, ClearOptions options = ClearOptions.ClearMemory)
		{
			return innerArray.Resize(allocator, newLength, growFactor, options);
		}

		[INLINE(256)]
		public void Fill(Allocator* allocator, in T value, int from, int to)
		{
			innerArray.Fill(allocator, value, from, to);
		}

		[INLINE(256)]
		public void Clear(Allocator* allocator)
		{
			innerArray.Clear(allocator);
		}

		[INLINE(256)]
		public void Clear(Allocator* allocator, int index, int length)
		{
			innerArray.Clear(allocator, index, length);
		}

		[INLINE(256)]
		public void Clear()
		{
			innerArray.Clear();
		}

		[INLINE(256)]
		public void Clear(int index, int length)
		{
			innerArray.Clear(index, length);
		}

		[INLINE(256)]
		public bool Contains<TU>(Allocator* allocator, in TU obj) where TU : unmanaged, System.IEquatable<T>
		{
			return innerArray.Contains<T, TU>(allocator, obj);
		}

		[INLINE(256)]
		public int GetReservedSizeInBytes()
		{
			return innerArray.GetReservedSizeInBytes();
		}

		[INLINE(256)]
		public ListEnumerator<T> GetEnumerator(Allocator* allocator)
		{
			return new ListEnumerator<T>(GetValuePtr(allocator), Count);
		}

		[INLINE(256)]
		public new ListEnumerator<T> GetEnumerator()
		{
			return new ListEnumerator<T>(GetValuePtr(), Count);
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
		public Enumerable<T, ListEnumerator<T>> GetEnumerable(Allocator* allocator)
		{
			return new (new (GetValuePtr(allocator), Count));
		}

		[INLINE(256)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable()
		{
			return new (new (GetValuePtr(), Count));
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
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return GetEnumerator();
		}

		[INLINE(256)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
