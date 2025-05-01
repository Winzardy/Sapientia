using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	[DebuggerTypeProxy(typeof(MemArrayProxy<>))]
	public unsafe struct MemArray
	{
		public static readonly MemArray Empty = new () { ptr = CWPtr.Invalid, Length = 0, };

		public CWPtr ptr;
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
		public World GetAllocator()
		{
			return ptr.GetAllocator();
		}

		[INLINE(256)]
		public MemArray(int elementSize, int length, ClearOptions clearOptions = ClearOptions.ClearMemory) : this(WorldManager.CurrentWorld, elementSize, length, clearOptions)
		{

		}

		[INLINE(256)]
		public MemArray(World world, int elementSize, int length, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			this = default;
			ptr = default;
			var memPtr = world.MemAlloc(elementSize * length, out var tPtr);
			ptr = new CWPtr(world, tPtr, memPtr);
			ElementSize = elementSize;
			Length = length;

			if (clearOptions == ClearOptions.ClearMemory)
			{
				Clear(world);
			}
		}

		[INLINE(256)]
		public MemArray(in MemArray arr) : this(WorldManager.CurrentWorld, arr)
		{
		}

		[INLINE(256)]
		public MemArray(World world, in MemArray arr)
		{
			if (arr.Length == 0u)
			{
				this = Empty;
				return;
			}

			ptr = default;
			Length = arr.Length;
			ElementSize = arr.ElementSize;

			var memPtr = world.MemAlloc(arr.ElementSize * arr.Length, out var tPtr);
			ptr = new CWPtr(world, tPtr, memPtr);
			MemArrayExt.CopyNoChecks(world, in arr, 0, ref this, 0, arr.Length);
		}

		[INLINE(256)]
		public MemArray(WPtr wPtr, int elementSize, int length)
		{
			ptr = new CWPtr(wPtr);
			Length = length;
			ElementSize = elementSize;
			ptr = default;
		}

		[INLINE(256)]
		public void ReplaceWith(World world, in MemArray other)
		{
			if (other.ptr.wPtr == ptr.wPtr)
			{
				return;
			}

			Dispose(world);
			this = other;
		}

		[INLINE(256)]
		public void CopyFrom(World world, in MemArray other)
		{
			if (other.ptr.wPtr == ptr.wPtr) return;
			if (!ptr.wPtr.IsCreated() && !other.ptr.wPtr.IsCreated())
				return;
			if (ptr.wPtr.IsCreated() && !other.ptr.wPtr.IsCreated())
			{
				Dispose(world);
				return;
			}

			if (ptr.wPtr.IsCreated() == false)
				this = new MemArray(world, other.ElementSize, other.Length, ClearOptions.ClearMemory);

			MemArrayExt.Copy(world, in other, ref this);
		}

		[INLINE(256)]
		public void Dispose(World world)
		{
			if (ptr.wPtr.IsCreated())
			{
				world.MemFree(ptr.wPtr);
			}

			this = default;
		}

		[INLINE(256)]
		public SafePtr GetPtr(World world)
		{
			return ptr.GetPtr(world);
		}

		[INLINE(256)]
		public SafePtr GetPtr()
		{
			return ptr.GetPtr();
		}

		[INLINE(256)]
		public WPtr GetAllocPtr(int index)
		{
			return ptr.wPtr.GetArrayElement(ElementSize, index);
		}

		[INLINE(256)]
		public ref T GetValue<T>(World world, int index) where T: unmanaged
		{
			return ref (GetPtr(world).Cast<T>() + index).Value();
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
		public SafePtr<T> GetValuePtr<T>(World world) where T: unmanaged
		{
			return GetPtr(world);
		}

		public Span<T> GetSpan<T>(World world) where T: unmanaged
		{
			return new Span<T>(GetValuePtr<T>(world).ptr, Length);
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr<T>(int index) where T: unmanaged
		{
			return GetPtr().Cast<T>() + index;
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr<T>(World world, int index) where T: unmanaged
		{
			return GetPtr(world).Cast<T>() + index;
		}

		public SafePtr GetValuePtr()
		{
			return GetPtr();
		}

		[INLINE(256)]
		public SafePtr GetValuePtr(World world)
		{
			return GetPtr(world);
		}

		[INLINE(256)]
		public SafePtr GetValuePtr(int index)
		{
			return GetPtr() + (index * ElementSize);
		}

		[INLINE(256)]
		public SafePtr GetValuePtr(World world, int index)
		{
			return GetPtr(world) + (index * ElementSize);
		}

		[INLINE(256)]
		public bool Resize(World world, int newLength, int elementSize, ClearOptions options = ClearOptions.ClearMemory)
		{
			if (!IsCreated)
			{
				this = new MemArray(world, elementSize, newLength, options);
				return true;
			}

			if (newLength <= Length)
			{
				return false;
			}

			E.ASSERT(elementSize == ElementSize);

			var prevLength = Length;
			var arrPtr = world.MemReAlloc(ptr.wPtr, elementSize * newLength, out var rawPtr);
			ptr = new CWPtr(world, rawPtr, arrPtr);

			if (options == ClearOptions.ClearMemory)
			{
				Clear(world, prevLength, newLength - prevLength);
			}

			Length = newLength;
			return true;
		}

		[INLINE(256)]
		public bool Resize<T>(World world, int newLength, ClearOptions options = ClearOptions.ClearMemory)
			where T : unmanaged
		{
			var elementSize = TSize<T>.size;
			return Resize(world, newLength, elementSize, options);
		}

		[INLINE(256)]
		public void Fill<T>(World world, in T value, int fromIndex, int count) where T: unmanaged
		{
			world.MemFill<T>(ptr.wPtr, value, fromIndex, count);
		}

		[INLINE(256)]
		public void Clear(World world)
		{
			Clear(world, 0, Length);
		}

		[INLINE(256)]
		public void Clear(World world, int index, int length)
		{
			var size = ElementSize;
			world.MemClear(ptr.wPtr, index * size, length * size);
		}

		[INLINE(256)]
		public void Clear()
		{
			Clear(0, Length);
		}

		[INLINE(256)]
		public void Clear(int index, int length)
		{
			if (!IsCreated)
				return;
			var size = ElementSize;
			var allocator = ptr.wPtr.GetWorld();
			allocator.MemClear(ptr.wPtr, index * size, length * size);
		}

		[INLINE(256)]
		public bool Contains<T, TU>(World world, in TU obj) where T: unmanaged where TU : unmanaged, IEquatable<T>
		{
			E.ASSERT(IsCreated);

			var safePtr = GetPtr(world).Cast<T>();
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
		public ListEnumerator<T> GetEnumerator<T>(World world) where T: unmanaged
		{
			return new ListEnumerator<T>(GetValuePtr<T>(world), Count);
		}

		[INLINE(256)]
		public ListEnumerator<T> GetEnumerator<T>() where T: unmanaged
		{
			return new ListEnumerator<T>(GetValuePtr<T>(), Count);
		}

		[INLINE(256)]
		public ListPtrEnumerator<T> GetPtrEnumerator<T>(World world) where T: unmanaged
		{
			return new ListPtrEnumerator<T>(GetValuePtr(world), ElementSize, Count);
		}

		[INLINE(256)]
		public ListPtrEnumerator<T> GetPtrEnumerator<T>() where T: unmanaged
		{
			return new ListPtrEnumerator<T>(GetValuePtr(), ElementSize, Count);
		}

		[INLINE(256)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable<T>(World world) where T: unmanaged
		{
			return new (new (GetValuePtr<T>(world), Count));
		}

		[INLINE(256)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable<T>() where T: unmanaged
		{
			return new (new (GetValuePtr<T>(), Count));
		}

		[INLINE(256)]
		public Enumerable<SafePtr<T>, ListPtrEnumerator<T>> GetPtrEnumerable<T>(World world) where T: unmanaged
		{
			return new (new (GetValuePtr(world), ElementSize, Count));
		}

		[INLINE(256)]
		public Enumerable<SafePtr<T>, ListPtrEnumerator<T>> GetPtrEnumerable<T>() where T: unmanaged
		{
			return new (new (GetValuePtr(), ElementSize, Count));
		}
	}
}
