using System;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	/// <summary>
	/// IndexedPtr — это обёртка над указателем,
	/// которая хранит также индекс типа через <see cref="TypeIndex"/>.
	///
	/// Используется как универсальный указатель с информацией о типе.
	/// </summary>
	public struct IndexedPtr : IEquatable<IndexedPtr>
	{
		public readonly TypeIndex typeIndex;
		private CachedPtr _ptr;

		public readonly bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _ptr.memPtr.IsValid();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr(MemPtr memPtr, TypeIndex typeIndex)
		{
			_ptr = new (memPtr);
			this.typeIndex = typeIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr(CachedPtr ptr, TypeIndex typeIndex)
		{
			_ptr = ptr;
			this.typeIndex = typeIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr(WorldState worldState, SafePtr cachedPtr, MemPtr memPtr, TypeIndex typeIndex)
		{
			_ptr = new CachedPtr(worldState, cachedPtr, memPtr);
			this.typeIndex = typeIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IndexedPtr Create<T>(CachedPtr<T> ptr) where T : unmanaged
		{
			return new IndexedPtr(ptr, TypeIndex<T>.typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IndexedPtr Create<T>(CachedPtr ptr) where T : unmanaged
		{
			return new IndexedPtr(ptr, TypeIndex<T>.typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IndexedPtr Create<T>(WorldState worldState) where T : unmanaged
		{
			var memPtr = worldState.MemAlloc<T>(out var rawPtr);
			return new IndexedPtr(worldState, rawPtr, memPtr, TypeIndex<T>.typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IndexedPtr Create<T>(WorldState worldState, in T value) where T : unmanaged
		{
			var memPtr = worldState.MemAlloc<T>(value, out var rawPtr);
			return new IndexedPtr(worldState, rawPtr, memPtr, TypeIndex<T>.typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IndexedPtr Create<T>(in T value) where T : unmanaged
		{
			var worldState = WorldManager.CurrentWorldState;
			var memPtr = worldState.MemAlloc<T>(value, out var rawPtr);
			return new IndexedPtr(worldState, rawPtr, memPtr, TypeIndex<T>.typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetValue<T>(WorldState worldState) where T : unmanaged
		{
			E.ASSERT(IsCreated);
			return ref _ptr.Get<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetPtr(WorldState worldState)
		{
			E.ASSERT(IsCreated);
			return _ptr.GetPtr(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetPtr<T>(WorldState worldState) where T: unmanaged
		{
			E.ASSERT(IsCreated);
			return _ptr.GetPtr(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly MemPtr GetMemPtr()
		{
			E.ASSERT(IsCreated);
			return _ptr.memPtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly CachedPtr GetCachedPtr()
		{
			E.ASSERT(IsCreated);
			return _ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly CachedPtr<T> GetCachedPtr<T>() where T : unmanaged
		{
			E.ASSERT(IsCreated);
			return _ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(WorldState worldState)
		{
			_ptr.Dispose(worldState);
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
