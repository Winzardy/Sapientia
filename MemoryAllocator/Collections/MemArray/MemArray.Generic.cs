using System.Diagnostics;
using System.Runtime.InteropServices;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Data;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(MemArrayProxy<>))]
	public unsafe struct MemArray<T> : IIsCreated where T : unmanaged
	{
		public static readonly MemArray<T> Empty = new () { innerArray = MemArray.Empty };

		public MemArray innerArray;

		public readonly uint Length
		{
			[INLINE(256)] get => innerArray.Length;
		}

		public readonly bool IsCreated
		{
			[INLINE(256)] get => innerArray.IsCreated;
		}

		[INLINE(256)]
		public ref Allocator GetAllocator()
		{
			return ref innerArray.GetAllocator();
		}

		[INLINE(256)]
		public MemArray(uint length, ClearOptions clearOptions = ClearOptions.ClearMemory, ushort growFactor = 1) : this(ref AllocatorManager.CurrentAllocator, length, clearOptions, growFactor)
		{

		}

		[INLINE(256)]
		public MemArray(ref Allocator allocator, uint length, ClearOptions clearOptions = ClearOptions.ClearMemory, ushort growFactor = 1)
		{
			innerArray = new MemArray(ref allocator, TSize<T>.uSize, length, clearOptions, growFactor);
		}

		[INLINE(256)]
		public MemArray(uint elementSize, uint length, ClearOptions clearOptions, ushort growFactor = 1) : this(ref AllocatorManager.CurrentAllocator, elementSize, length, clearOptions, growFactor)
		{

		}

		[INLINE(256)]
		public MemArray(ref Allocator allocator, uint elementSize, uint length, ClearOptions clearOptions, ushort growFactor = 1)
		{
			innerArray = new MemArray(ref allocator, elementSize, length, clearOptions, growFactor);
		}

		[INLINE(256)]
		public MemArray(in MemArray<T> arr) : this(ref AllocatorManager.CurrentAllocator, arr)
		{
		}

		[INLINE(256)]
		public MemArray(ref Allocator allocator, MemArray<T> arr)
		{
			innerArray = new MemArray(ref allocator, arr.As<MemArray<T>, MemArray>());
		}

		[INLINE(256)]
		public MemArray(MemPtr memPtr, uint length, ushort growFactor)
		{
			innerArray = new MemArray(memPtr, TSize<T>.uSize, length, growFactor);
		}

		[INLINE(256)]
		public ref U As<U>(in Allocator allocator, uint index) where U : unmanaged
		{
			return ref innerArray.GetValue<U>(allocator, index);
		}

		[INLINE(256)]
		public void ReplaceWith(ref Allocator allocator, MemArray<T> other)
		{
			innerArray.ReplaceWith(ref allocator, other.As<MemArray<T>, MemArray>());
		}

		[INLINE(256)]
		public void CopyFrom(ref Allocator allocator, MemArray<T> other)
		{
			innerArray.CopyFrom(ref allocator, other.As<MemArray<T>, MemArray>());;
		}

		[INLINE(256)]
		public void Dispose(ref Allocator allocator)
		{
			innerArray.Dispose(ref allocator);
		}

		[INLINE(256)]
		public void Dispose(Allocator* allocator)
		{
			innerArray.Dispose(allocator);
		}

		[INLINE(256)]
		public void BurstMode(in Allocator allocator, bool state)
		{
			innerArray.BurstMode(allocator, state);
		}

		[INLINE(256)]
		public void* GetPtr(in Allocator allocator)
		{
			return innerArray.GetPtr(in allocator);
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
		public MemPtr GetAllocPtr(in Allocator allocator, uint index)
		{
			return innerArray.GetAllocPtr(allocator, index);
		}

		public ref T this[int index]
		{
			[INLINE(256)]
			get => ref innerArray.GetValue<T>(index);
		}

		public ref T this[uint index]
		{
			[INLINE(256)]
			get => ref innerArray.GetValue<T>(index);
		}

		[INLINE(256)]
		public MemPtr GetAllocPtr(uint index)
		{
			return innerArray.GetAllocPtr(index);
		}

		public ref T this[in Allocator allocator, int index]
		{
			[INLINE(256)]
			get => ref innerArray.GetValue<T>(allocator, index);
		}

		public ref T this[in Allocator allocator, uint index]
		{
			[INLINE(256)]
			get => ref innerArray.GetValue<T>(allocator, index);
		}

		public ref T this[Allocator* allocator, int index]
		{
			[INLINE(256)]
			get => ref innerArray.GetValue<T>(allocator, index);
		}

		public ref T this[Allocator* allocator, uint index]
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
		public T* GetValuePtr(in Allocator allocator)
		{
			return innerArray.GetValuePtr<T>(allocator);
		}

		[INLINE(256)]
		public T* GetValuePtr(uint index)
		{
			return innerArray.GetValuePtr<T>(index);
		}

		[INLINE(256)]
		public T* GetValuePtr(in Allocator allocator, uint index)
		{
			return innerArray.GetValuePtr<T>(allocator, index);
		}

		[INLINE(256)]
		public bool Resize(ref Allocator allocator, uint newLength,
			ClearOptions options = ClearOptions.ClearMemory)
		{
			return innerArray.Resize(ref allocator, newLength, options);
		}

		[INLINE(256)]
		public bool Resize(ref Allocator allocator, uint newLength, ushort growFactor, ClearOptions options = ClearOptions.ClearMemory)
		{
			return innerArray.Resize(ref allocator, newLength, growFactor, options);
		}

		[INLINE(256)]
		public void Fill(in Allocator allocator, in T value, uint from, uint to)
		{
			innerArray.Fill(allocator, value, from, to);
		}

		[INLINE(256)]
		public void Clear(in Allocator allocator)
		{
			innerArray.Clear(allocator);
		}

		[INLINE(256)]
		public void Clear(in Allocator allocator, uint index, uint length)
		{
			innerArray.Clear(allocator, index, length);
		}

		[INLINE(256)]
		public void Clear()
		{
			innerArray.Clear();
		}

		[INLINE(256)]
		public void Clear(uint index, uint length)
		{
			innerArray.Clear(index, length);
		}

		[INLINE(256)]
		public bool Contains<U>(in Allocator allocator, in U obj) where U : unmanaged, System.IEquatable<T>
		{
			return innerArray.Contains<T, U>(allocator, obj);
		}

		[INLINE(256)]
		public uint GetReservedSizeInBytes()
		{
			return innerArray.GetReservedSizeInBytes();
		}
	}
}
