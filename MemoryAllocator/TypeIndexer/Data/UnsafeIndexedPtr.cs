using System;
using Sapientia.Data;
using Sapientia.TypeIndexer;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	public struct UnsafeIndexedPtr : IEquatable<UnsafeIndexedPtr>
	{
		public readonly TypeIndex typeIndex;
		private SafePtr _ptr;

		public readonly bool IsCreated
		{
			[INLINE(256)] get => _ptr.IsValid;
		}

		[INLINE(256)]
		public UnsafeIndexedPtr(SafePtr ptr, TypeIndex typeIndex)
		{
			_ptr = ptr;
			this.typeIndex = typeIndex;
		}

		[INLINE(256)]
		public static UnsafeIndexedPtr Create<T>(SafePtr<T> ptr) where T : unmanaged
		{
			return new UnsafeIndexedPtr(ptr, TypeIndex<T>.typeIndex);
		}

		[INLINE(256)]
		public ref T GetValue<T>() where T : unmanaged
		{
			E.ASSERT(IsCreated);
			return ref _ptr.Value<T>();
		}

		[INLINE(256)]
		public SafePtr GetPtr()
		{
			E.ASSERT(IsCreated);
			return _ptr;
		}

		[INLINE(256)]
		public SafePtr<T> GetPtr<T>() where T: unmanaged
		{
			E.ASSERT(IsCreated);
			return _ptr;
		}


		public static bool operator ==(UnsafeIndexedPtr a, UnsafeIndexedPtr b)
		{
			return a.typeIndex == b.typeIndex && a._ptr == b._ptr;
		}

		public static bool operator !=(UnsafeIndexedPtr a, UnsafeIndexedPtr b)
		{
			return a.typeIndex != b.typeIndex || a._ptr != b._ptr;
		}

		public bool Equals(UnsafeIndexedPtr other)
		{
			return this == other;
		}

		public override bool Equals(object obj)
		{
			return obj is UnsafeIndexedPtr other && this == other;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(_ptr, typeIndex);
		}
	}
}
