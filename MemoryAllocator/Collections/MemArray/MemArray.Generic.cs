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
	public unsafe struct MemArray<T> : IListEnumerable<T> where T : unmanaged
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
		public SafePtr<Allocator> GetAllocatorPtr()
		{
			return innerArray.GetAllocatorPtr();
		}

		[INLINE(256)]
		public MemArray(int length, ClearOptions clearOptions = ClearOptions.ClearMemory) :
			this(AllocatorManager.CurrentAllocatorPtr, length, clearOptions)
		{

		}

		[INLINE(256)]
		public MemArray(SafePtr<Allocator> allocator, int length, T defaultValue)
		{
			innerArray = new MemArray(allocator, TSize<T>.size, length, ClearOptions.UninitializedMemory);
			innerArray.Fill(allocator, defaultValue, 0, length);
		}

		[INLINE(256)]
		public MemArray(SafePtr<Allocator> allocator, int length, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			innerArray = new MemArray(allocator, TSize<T>.size, length, clearOptions);
		}

		[INLINE(256)]
		public MemArray(int elementSize, int length, ClearOptions clearOptions) :
			this(AllocatorManager.CurrentAllocatorPtr, elementSize, length, clearOptions)
		{

		}

		[INLINE(256)]
		public MemArray(SafePtr<Allocator> allocator, int elementSize, int length, ClearOptions clearOptions)
		{
			innerArray = new MemArray(allocator, elementSize, length, clearOptions);
		}

		[INLINE(256)]
		public MemArray(in MemArray<T> arr) : this(AllocatorManager.CurrentAllocatorPtr, arr)
		{
		}

		[INLINE(256)]
		public MemArray(SafePtr<Allocator> allocator, MemArray<T> arr)
		{
			innerArray = new MemArray(allocator, arr.As<MemArray<T>, MemArray>());
		}

		[INLINE(256)]
		public MemArray(SafePtr<Allocator> allocator, ReadOnlySpan<T> arr)
		{
			innerArray = new MemArray(allocator, TSize<T>.size, arr.Length, ClearOptions.UninitializedMemory);
			arr.CopyTo(new Span<T>(innerArray.GetValuePtr().ptr, arr.Length));
		}

		[INLINE(256)]
		public MemArray(MemPtr memPtr, int length)
		{
			innerArray = new MemArray(memPtr, TSize<T>.size, length);
		}

		[INLINE(256)]
		public ref TU As<TU>(SafePtr<Allocator> allocator, int index) where TU : unmanaged
		{
			return ref innerArray.GetValue<TU>(allocator, index);
		}

		[INLINE(256)]
		public void ReplaceWith(SafePtr<Allocator> allocator, MemArray<T> other)
		{
			innerArray.ReplaceWith(allocator, other.As<MemArray<T>, MemArray>());
		}

		[INLINE(256)]
		public void CopyFrom(SafePtr<Allocator> allocator, MemArray<T> other)
		{
			innerArray.CopyFrom(allocator, other.As<MemArray<T>, MemArray>());;
		}

		[INLINE(256)]
		public void Dispose(SafePtr<Allocator> allocator)
		{
			innerArray.Dispose(allocator);
		}

		[INLINE(256)]
		public SafePtr GetPtr(SafePtr<Allocator> allocator)
		{
			return innerArray.GetPtr(allocator);
		}

		[INLINE(256)]
		public SafePtr GetPtr()
		{
			return innerArray.GetPtr();
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

		public ref T this[SafePtr<Allocator> allocator, int index]
		{
			[INLINE(256)]
			get
			{
				E.IS_CREATED(this);
				E.RANGE(index, 0, this.Length);
				return ref innerArray.GetValue<T> (allocator, index);
			}
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr()
		{
			return innerArray.GetValuePtr<T>();
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr(SafePtr<Allocator> allocator)
		{
			E.IS_CREATED(this);
			return innerArray.GetValuePtr<T>(allocator);
		}

		[INLINE(256)]
		public Span<T> GetSpan(SafePtr<Allocator> allocator)
		{
			return innerArray.GetSpan<T>(allocator);
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr(int index)
		{
			return innerArray.GetValuePtr<T>(index);
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr(SafePtr<Allocator> allocator, int index)
		{
			return innerArray.GetValuePtr<T>(allocator, index);
		}

		[INLINE(256)]
		public bool Resize(int newLength, ClearOptions options = ClearOptions.ClearMemory)
		{
			return innerArray.Resize<T>(GetAllocatorPtr(), newLength, options);
		}

		[INLINE(256)]
		public bool Resize(SafePtr<Allocator> allocator, int newLength, ClearOptions options = ClearOptions.ClearMemory)
		{
			return innerArray.Resize<T>(allocator, newLength, options);
		}

		[INLINE(256)]
		public void Fill(SafePtr<Allocator> allocator, in T value, int fromIndex, int count)
		{
			innerArray.Fill(allocator, value, fromIndex, count);
		}

		[INLINE(256)]
		public void Clear(SafePtr<Allocator> allocator)
		{
			innerArray.Clear(allocator);
		}

		[INLINE(256)]
		public void Clear(SafePtr<Allocator> allocator, int index, int length)
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
		public bool Contains<TU>(SafePtr<Allocator> allocator, in TU obj) where TU : unmanaged, System.IEquatable<T>
		{
			return innerArray.Contains<T, TU>(allocator, obj);
		}

		[INLINE(256)]
		public int GetReservedSizeInBytes()
		{
			return innerArray.GetReservedSizeInBytes();
		}

		[INLINE(256)]
		public ListEnumerator<T> GetEnumerator(SafePtr<Allocator> allocator)
		{
			return new ListEnumerator<T>(GetValuePtr(allocator), Count);
		}

		[INLINE(256)]
		public new ListEnumerator<T> GetEnumerator()
		{
			return new ListEnumerator<T>(GetValuePtr(), Count);
		}

		[INLINE(256)]
		public ListPtrEnumerator<T> GetPtrEnumerator(SafePtr<Allocator> allocator)
		{
			return new ListPtrEnumerator<T>(GetValuePtr(allocator), ElementSize, Count);
		}

		[INLINE(256)]
		public ListPtrEnumerator<T> GetPtrEnumerator()
		{
			return new ListPtrEnumerator<T>(GetValuePtr(), ElementSize, Count);
		}

		[INLINE(256)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable(SafePtr<Allocator> allocator)
		{
			return new (new (GetValuePtr(allocator), Count));
		}

		[INLINE(256)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable()
		{
			return new (new (GetValuePtr(), Count));
		}

		[INLINE(256)]
		public Enumerable<SafePtr<T>, ListPtrEnumerator<T>> GetPtrEnumerable(SafePtr<Allocator> allocator)
		{
			return new (new (GetValuePtr(allocator), ElementSize, Count));
		}

		[INLINE(256)]
		public Enumerable<SafePtr<T>, ListPtrEnumerator<T>> GetPtrEnumerable()
		{
			return new (new (GetValuePtr(), ElementSize, Count));
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
