using System;
using Sapientia.Data;
using Sapientia.TypeIndexer;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	public struct IndexedPtr : IEquatable<IndexedPtr>
	{
		public readonly TypeIndex typeIndex;
		public CachedPtr _ptr;

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
		public IndexedPtr(CachedPtr ptr, TypeIndex typeIndex)
		{
			_ptr = ptr;
			this.typeIndex = typeIndex;
		}

		[INLINE(256)]
		public IndexedPtr(World world, SafePtr cachedPtr, MemPtr memPtr, TypeIndex typeIndex)
		{
			_ptr = new CachedPtr(world, cachedPtr, memPtr);
			this.typeIndex = typeIndex;
		}

		[INLINE(256)]
		public static IndexedPtr Create<T>(CachedPtr<T> ptr) where T : unmanaged
		{
			return new IndexedPtr(ptr, TypeIndex<T>.typeIndex);
		}

		[INLINE(256)]
		public static IndexedPtr Create<T>(CachedPtr ptr) where T : unmanaged
		{
			return new IndexedPtr(ptr, TypeIndex<T>.typeIndex);
		}

		[INLINE(256)]
		public static IndexedPtr Create<T>(World world) where T : unmanaged
		{
			var memPtr = world.MemAlloc<T>(out var rawPtr);
			return new IndexedPtr(world, rawPtr, memPtr, TypeIndex<T>.typeIndex);
		}

		[INLINE(256)]
		public static IndexedPtr Create<T>(World world, in T value) where T : unmanaged
		{
			var memPtr = world.MemAlloc<T>(value, out var rawPtr);
			return new IndexedPtr(world, rawPtr, memPtr, TypeIndex<T>.typeIndex);
		}

		[INLINE(256)]
		public static IndexedPtr Create<T>(in T value) where T : unmanaged
		{
			var world = WorldManager.CurrentWorld;
			var memPtr = world.MemAlloc<T>(value, out var rawPtr);
			return new IndexedPtr(world, rawPtr, memPtr, TypeIndex<T>.typeIndex);
		}

		[INLINE(256)]
		public ref T GetValue<T>(World world) where T : unmanaged
		{
			E.ASSERT(IsCreated);
			return ref _ptr.Get<T>(world);
		}

		[INLINE(256)]
		public SafePtr GetPtr(World world)
		{
			E.ASSERT(IsCreated);
			return _ptr.GetPtr(world);
		}

		[INLINE(256)]
		public SafePtr<T> GetPtr<T>(World world) where T: unmanaged
		{
			E.ASSERT(IsCreated);
			return _ptr.GetPtr(world);
		}

		[INLINE(256)]
		public readonly MemPtr GetMemPtr()
		{
			E.ASSERT(IsCreated);
			return _ptr.memPtr;
		}

		[INLINE(256)]
		public readonly CachedPtr GetCachedPtr()
		{
			E.ASSERT(IsCreated);
			return _ptr;
		}

		[INLINE(256)]
		public readonly CachedPtr<T> GetCachedPtr<T>() where T : unmanaged
		{
			E.ASSERT(IsCreated);
			return _ptr;
		}

		[INLINE(256)]
		public void Dispose(World world)
		{
			_ptr.Dispose(world);
			this = default;
		}

		[INLINE(256)]
		public IndexedPtr CopyTo(World srsWorld, World dstWorld)
		{
			return new IndexedPtr(_ptr.CopyTo(srsWorld, dstWorld), typeIndex);
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
