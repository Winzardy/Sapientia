using System;
using System.Diagnostics;
using Sapientia.Data;
using Sapientia.TypeIndexer;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator.Data
{
	public unsafe struct IndexedPtr : IEquatable<IndexedPtr>
	{
		public readonly TypeIndex typeIndex;
		private Ptr _ptr;

		public readonly bool IsCreated
		{
			[INLINE(256)] get => _ptr.memPtr.IsCreated();
		}

		[INLINE(256)]
		public IndexedPtr(MemPtr memPtr, TypeIndex typeIndex)
		{
			_ptr = new (memPtr);
			this.typeIndex = typeIndex;
		}

		[INLINE(256)]
		public IndexedPtr(Ptr ptr, TypeIndex typeIndex)
		{
			_ptr = ptr;
			this.typeIndex = typeIndex;
		}

		[INLINE(256)]
		public IndexedPtr(Allocator allocator, SafePtr cachedPtr, MemPtr memPtr, TypeIndex typeIndex)
		{
			_ptr = new Ptr(allocator, cachedPtr, memPtr);
			this.typeIndex = typeIndex;
		}

		[INLINE(256)]
		public static IndexedPtr Create<T>(Ptr<T> ptr) where T : unmanaged
		{
			return new IndexedPtr(ptr, TypeIndex<T>.typeIndex);
		}

		[INLINE(256)]
		public static IndexedPtr Create<T>(Ptr ptr) where T : unmanaged
		{
			return new IndexedPtr(ptr, TypeIndex<T>.typeIndex);
		}

		[INLINE(256)]
		public static IndexedPtr Create<T>(Allocator allocator) where T : unmanaged
		{
			var memPtr = allocator.MemAlloc<T>(out var rawPtr);
			return new IndexedPtr(allocator, rawPtr, memPtr, TypeIndex<T>.typeIndex);
		}

		[INLINE(256)]
		public static IndexedPtr Create<T>(Allocator allocator, in T value) where T : unmanaged
		{
			var memPtr = allocator.MemAlloc<T>(value, out var rawPtr);
			return new IndexedPtr(allocator, rawPtr, memPtr, TypeIndex<T>.typeIndex);
		}

		[INLINE(256)]
		public static IndexedPtr Create<T>(in T value) where T : unmanaged
		{
			var allocator = AllocatorManager.CurrentAllocator;
			var memPtr = allocator.MemAlloc<T>(value, out var rawPtr);
			return new IndexedPtr(allocator, rawPtr, memPtr, TypeIndex<T>.typeIndex);
		}

		[INLINE(256)]
		public ref T GetValue<T>() where T : unmanaged
		{
			E.ASSERT(IsCreated);
			return ref _ptr.Get<T>();
		}

		[INLINE(256)]
		public ref T GetValue<T>(Allocator allocator) where T : unmanaged
		{
			E.ASSERT(IsCreated);
			return ref _ptr.Get<T>(allocator);
		}

		[INLINE(256)]
		public SafePtr GetPtr()
		{
			E.ASSERT(IsCreated);
			return _ptr.GetPtr();
		}

		[INLINE(256)]
		public SafePtr GetPtr(Allocator allocator)
		{
			E.ASSERT(IsCreated);
			return _ptr.GetPtr(allocator);
		}

		[INLINE(256)]
		public SafePtr<T> GetPtr<T>() where T: unmanaged
		{
			E.ASSERT(IsCreated);
			return _ptr.GetPtr();
		}

		[INLINE(256)]
		public SafePtr<T> GetPtr<T>(Allocator allocator) where T: unmanaged
		{
			E.ASSERT(IsCreated);
			return _ptr.GetPtr(allocator);
		}

		[INLINE(256)]
		public readonly MemPtr GetMemPtr()
		{
			E.ASSERT(IsCreated);
			return _ptr.memPtr;
		}

		[INLINE(256)]
		public readonly Ptr GetCachedPtr()
		{
			E.ASSERT(IsCreated);
			return _ptr;
		}

		[INLINE(256)]
		public readonly Ptr<T> GetCachedPtr<T>() where T : unmanaged
		{
			E.ASSERT(IsCreated);
			return _ptr;
		}

		[INLINE(256)]
		public Allocator GetAllocator()
		{
			return _ptr.GetAllocator();
		}

		[INLINE(256)]
		public void Dispose(Allocator allocator)
		{
			_ptr.Dispose(allocator);
			this = default;
		}

		[INLINE(256)]
		public void Dispose()
		{
			_ptr.Dispose();
			this = default;
		}

		[INLINE(256)]
		public IndexedPtr CopyTo(Allocator srsAllocator, Allocator dstAllocator)
		{
			return new IndexedPtr(_ptr.CopyTo(srsAllocator, dstAllocator), typeIndex);
		}

		public static bool operator ==(IndexedPtr a, IndexedPtr b)
		{
			return a.typeIndex == b.typeIndex && a._ptr == b._ptr;
		}

		public static bool operator !=(IndexedPtr a, IndexedPtr b)
		{
			return a.typeIndex != b.typeIndex || a._ptr != b._ptr;
		}

		public bool Equals(IndexedPtr other)
		{
			return this == other;
		}

		public override bool Equals(object obj)
		{
			return obj is IndexedPtr other && this == other;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(_ptr, typeIndex);
		}
	}
}
