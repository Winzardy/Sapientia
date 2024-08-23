using Sapientia.TypeIndexer;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator.Data
{
	public unsafe struct ValueRef : IIsCreated
	{
		private CachedPtr _cachedPtr;
		public readonly TypeIndex typeIndex;

		public readonly bool IsCreated
		{
			[INLINE(256)] get => _cachedPtr.memPtr.IsValid();
		}

		[INLINE(256)]
		public ValueRef(MemPtr memPtr, TypeIndex typeIndex)
		{
			_cachedPtr = new (memPtr);
			this.typeIndex = typeIndex;
		}

		[INLINE(256)]
		public ValueRef(CachedPtr cachedPtr, TypeIndex typeIndex)
		{
			_cachedPtr = cachedPtr;
			this.typeIndex = typeIndex;
		}

		[INLINE(256)]
		public ValueRef(in Allocator allocator, void* cachedPtr, MemPtr memPtr, TypeIndex typeIndex)
		{
			_cachedPtr = new CachedPtr(allocator, cachedPtr, memPtr);
			this.typeIndex = typeIndex;
		}

		[INLINE(256)]
		public static ValueRef Create<T>(CachedPtr cachedPtr) where T : unmanaged
		{
			return new ValueRef(cachedPtr, IndexedTypes.GetIndex<T>());
		}

		[INLINE(256)]
		public static ValueRef Create<T>(ref Allocator allocator) where T : unmanaged
		{
			var memPtr = allocator.Alloc<T>(out var rawPtr);
			return new ValueRef(allocator, rawPtr, memPtr, IndexedTypes.GetIndex<T>());
		}

		[INLINE(256)]
		public static ValueRef Create<T>(ref Allocator allocator, in T value) where T : unmanaged
		{
			var memPtr = allocator.Alloc<T>(value, out var rawPtr);
			return new ValueRef(allocator, rawPtr, memPtr, IndexedTypes.GetIndex<T>());
		}

		[INLINE(256)]
		public ref T GetValue<T>(in Allocator allocator) where T : unmanaged
		{
			E.IS_CREATED(this);
			return ref _cachedPtr.Read<T>(allocator);
		}

		[INLINE(256)]
		public readonly MemPtr GetMemPtr()
		{
			E.IS_CREATED(this);
			return _cachedPtr.memPtr;
		}

		[INLINE(256)]
		public readonly CachedPtr GetCachedPtr()
		{
			E.IS_CREATED(this);
			return _cachedPtr;
		}

		[INLINE(256)]
		public void Dispose(ref Allocator allocator)
		{
			allocator.Free(_cachedPtr.memPtr);
			_cachedPtr = CachedPtr.Invalid;
		}
	}

	public unsafe struct ValueRef<T> : IIsCreated where T: unmanaged
	{
		private CachedPtr<T> _cachedPtr;

		public readonly bool IsCreated
		{
			[INLINE(256)] get => _cachedPtr.memPtr.IsValid();
		}

		[INLINE(256)]
		public ValueRef(CachedPtr cachedPtr)
		{
			_cachedPtr = cachedPtr;
		}

		[INLINE(256)]
		public ValueRef(in Allocator allocator, T* cachedPtr, MemPtr memPtr)
		{
			_cachedPtr = new CachedPtr(allocator, cachedPtr, memPtr);
		}

		[INLINE(256)]
		public static ValueRef<T> Create(CachedPtr cachedPtr)
		{
			return new ValueRef<T>(cachedPtr);
		}

		[INLINE(256)]
		public static ValueRef<T> Create(ref Allocator allocator)
		{
			var memPtr = allocator.Alloc<T>(out var rawPtr);
			return new ValueRef<T>(allocator, rawPtr, memPtr);
		}

		[INLINE(256)]
		public static ValueRef<T> Create(ref Allocator allocator, in T value)
		{
			var memPtr = allocator.Alloc<T>(value, out var rawPtr);
			return new ValueRef<T>(allocator, rawPtr, memPtr);
		}

		[INLINE(256)]
		public ref T GetValue(in Allocator allocator)
		{
			E.IS_CREATED(this);
			return ref _cachedPtr.Read(allocator);
		}

		[INLINE(256)]
		public readonly MemPtr GetMemPtr()
		{
			E.IS_CREATED(this);
			return _cachedPtr.memPtr;
		}

		[INLINE(256)]
		public readonly CachedPtr GetCachedPtr()
		{
			E.IS_CREATED(this);
			return _cachedPtr;
		}

		[INLINE(256)]
		public void Dispose(ref Allocator allocator)
		{
			allocator.Free(_cachedPtr.memPtr);
			_cachedPtr = CachedPtr.Invalid;
		}

		[INLINE(256)]
		public static implicit operator ValueRef(ValueRef<T> value)
		{
			return ValueRef.Create<T>(value._cachedPtr);
		}

		[INLINE(256)]
		public static implicit operator ValueRef<T>(ValueRef value)
		{
			return Create(value.GetCachedPtr());
		}
	}
}
