using System.Diagnostics;
using Sapientia.TypeIndexer;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator.Data
{
	public unsafe struct ValueRef : IIsCreated
	{
		private Ptr _ptr;
		public readonly TypeIndex typeIndex;

		public readonly bool IsCreated
		{
			[INLINE(256)] get => _ptr.memPtr.IsValid();
		}

		[INLINE(256)]
		public ValueRef(MemPtr memPtr, TypeIndex typeIndex)
		{
			_ptr = new (memPtr);
			this.typeIndex = typeIndex;
		}

		[INLINE(256)]
		public ValueRef(Ptr ptr, TypeIndex typeIndex)
		{
			_ptr = ptr;
			this.typeIndex = typeIndex;
		}

		[INLINE(256)]
		public ValueRef(in Allocator allocator, void* cachedPtr, MemPtr memPtr, TypeIndex typeIndex)
		{
			_ptr = new Ptr(allocator, cachedPtr, memPtr);
			this.typeIndex = typeIndex;
		}

		[INLINE(256)]
		public static ValueRef Create<T>(Ptr<T> ptr) where T : unmanaged
		{
			return new ValueRef(ptr, TypeIndex<T>.typeIndex);
		}

		[INLINE(256)]
		public static ValueRef Create<T>(Ptr ptr) where T : unmanaged
		{
			return new ValueRef(ptr, TypeIndex<T>.typeIndex);
		}

		[INLINE(256)]
		public static ValueRef Create<T>(ref Allocator allocator) where T : unmanaged
		{
			var memPtr = allocator.Alloc<T>(out var rawPtr);
			return new ValueRef(allocator, rawPtr, memPtr, TypeIndex<T>.typeIndex);
		}

		[INLINE(256)]
		public static ValueRef Create<T>(ref Allocator allocator, in T value) where T : unmanaged
		{
			var memPtr = allocator.Alloc<T>(value, out var rawPtr);
			return new ValueRef(allocator, rawPtr, memPtr, TypeIndex<T>.typeIndex);
		}

		[INLINE(256)]
		public ref T GetValue<T>() where T : unmanaged
		{
			Debug.Assert(IsCreated);
			return ref _ptr.Get<T>();
		}

		[INLINE(256)]
		public ref T GetValue<T>(in Allocator allocator) where T : unmanaged
		{
			Debug.Assert(IsCreated);
			return ref _ptr.Get<T>(allocator);
		}

		[INLINE(256)]
		public void* GetPtr()
		{
			Debug.Assert(IsCreated);
			return _ptr.GetPtr();
		}

		[INLINE(256)]
		public void* GetPtr(in Allocator allocator)
		{
			Debug.Assert(IsCreated);
			return _ptr.GetPtr(allocator);
		}

		[INLINE(256)]
		public readonly MemPtr GetMemPtr()
		{
			Debug.Assert(IsCreated);
			return _ptr.memPtr;
		}

		[INLINE(256)]
		public readonly Ptr GetCachedPtr()
		{
			Debug.Assert(IsCreated);
			return _ptr;
		}

		[INLINE(256)]
		public void Dispose(ref Allocator allocator)
		{
			allocator.Free(_ptr.memPtr);
			_ptr = Ptr.Invalid;
		}

		public static bool operator ==(ValueRef a, ValueRef b)
		{
			return a.typeIndex == b.typeIndex && a._ptr == b._ptr;
		}

		public static bool operator !=(ValueRef a, ValueRef b)
		{
			return a.typeIndex != b.typeIndex || a._ptr != b._ptr;
		}
	}
}
