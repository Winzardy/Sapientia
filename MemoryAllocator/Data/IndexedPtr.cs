using System.Diagnostics;
using Sapientia.TypeIndexer;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator.Data
{
	public unsafe struct IndexedPtr : IIsCreated
	{
		private Ptr _ptr;
		public readonly TypeIndex typeIndex;

		public readonly bool IsCreated
		{
			[INLINE(256)] get => _ptr.memPtr.IsValid();
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
		public IndexedPtr(Allocator* allocator, void* cachedPtr, MemPtr memPtr, TypeIndex typeIndex)
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
		public static IndexedPtr Create<T>(Allocator* allocator) where T : unmanaged
		{
			var memPtr = allocator->Alloc<T>(out var rawPtr);
			return new IndexedPtr(allocator, rawPtr, memPtr, TypeIndex<T>.typeIndex);
		}

		[INLINE(256)]
		public static IndexedPtr Create<T>(Allocator* allocator, in T value) where T : unmanaged
		{
			var memPtr = allocator->Alloc<T>(value, out var rawPtr);
			return new IndexedPtr(allocator, rawPtr, memPtr, TypeIndex<T>.typeIndex);
		}

		[INLINE(256)]
		public static IndexedPtr Create<T>(in T value) where T : unmanaged
		{
			var allocator = AllocatorManager.CurrentAllocatorPtr;
			var memPtr = allocator->Alloc<T>(value, out var rawPtr);
			return new IndexedPtr(allocator, rawPtr, memPtr, TypeIndex<T>.typeIndex);
		}

		[INLINE(256)]
		public ref T GetValue<T>() where T : unmanaged
		{
			Debug.Assert(IsCreated);
			return ref _ptr.Get<T>();
		}

		[INLINE(256)]
		public ref T GetValue<T>(Allocator* allocator) where T : unmanaged
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
		public void* GetPtr(Allocator* allocator)
		{
			Debug.Assert(IsCreated);
			return _ptr.GetPtr(allocator);
		}

		[INLINE(256)]
		public T* GetPtr<T>() where T: unmanaged
		{
			Debug.Assert(IsCreated);
			return (T*)_ptr.GetPtr();
		}

		[INLINE(256)]
		public T* GetPtr<T>(Allocator* allocator) where T: unmanaged
		{
			Debug.Assert(IsCreated);
			return (T*)_ptr.GetPtr(allocator);
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
		public void Dispose(Allocator* allocator)
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

		public static bool operator ==(IndexedPtr a, IndexedPtr b)
		{
			return a.typeIndex == b.typeIndex && a._ptr == b._ptr;
		}

		public static bool operator !=(IndexedPtr a, IndexedPtr b)
		{
			return a.typeIndex != b.typeIndex || a._ptr != b._ptr;
		}
	}
}
