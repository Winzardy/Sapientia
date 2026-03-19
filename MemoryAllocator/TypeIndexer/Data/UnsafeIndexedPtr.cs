using System;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public struct UnsafeIndexedPtr : IEquatable<UnsafeIndexedPtr>
	{
		public readonly TypeId typeId;
		private SafePtr _ptr;

		public readonly bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _ptr.IsValid;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UnsafeIndexedPtr(SafePtr ptr, TypeId typeId)
		{
			_ptr = ptr;
			this.typeId = typeId;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnsafeIndexedPtr Create<T>(SafePtr<T> ptr) where T : unmanaged
		{
			return new UnsafeIndexedPtr(ptr, TypeId<T>.typeId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetValue<T>() where T : unmanaged
		{
			E.ASSERT(IsCreated);
			return ref _ptr.Value<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetPtr()
		{
			E.ASSERT(IsCreated);
			return _ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetPtr<T>() where T: unmanaged
		{
			E.ASSERT(IsCreated);
			return _ptr;
		}


		public static bool operator ==(UnsafeIndexedPtr a, UnsafeIndexedPtr b)
		{
			return a.typeId == b.typeId && a._ptr == b._ptr;
		}

		public static bool operator !=(UnsafeIndexedPtr a, UnsafeIndexedPtr b)
		{
			return a.typeId != b.typeId || a._ptr != b._ptr;
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
			return HashCode.Combine(_ptr, typeId);
		}
	}
}
