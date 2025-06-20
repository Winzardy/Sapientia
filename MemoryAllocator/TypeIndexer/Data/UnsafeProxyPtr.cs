using System;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public struct UnsafeProxyPtr<T> : IEquatable<UnsafeProxyPtr<T>> where T: unmanaged, IProxy
	{
		public UnsafeIndexedPtr indexedPtr;
		public readonly T proxy;

		public bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => indexedPtr.IsCreated;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UnsafeProxyPtr(UnsafeIndexedPtr indexedPtr)
		{
			proxy = IndexedTypes.GetProxy<T>(indexedPtr.typeIndex);
			this.indexedPtr = indexedPtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UnsafeProxyPtr(SafePtr ptr, TypeIndex typeIndex)
		{
			proxy = IndexedTypes.GetProxy<T>(typeIndex);
			indexedPtr = new UnsafeIndexedPtr(ptr, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnsafeProxyPtr<T> Create<TInstance>(SafePtr<TInstance> value) where TInstance: unmanaged
		{
			return new UnsafeProxyPtr<T>(UnsafeIndexedPtr.Create<TInstance>(value));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UnsafeProxyPtr<T1> ToProxy<T1>() where T1: unmanaged, IProxy
		{
			return new UnsafeProxyPtr<T1>(indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T1 GetValue<T1>() where T1: unmanaged
		{
			return ref indexedPtr.GetValue<T1>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetPtr<T1>() where T1: unmanaged
		{
			return indexedPtr.GetPtr<T1>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetPtr()
		{
			return indexedPtr.GetPtr();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator UnsafeProxyPtr<T>(UnsafeIndexedPtr value)
		{
			return new UnsafeProxyPtr<T>(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator UnsafeIndexedPtr(UnsafeProxyPtr<T> value)
		{
			return value.indexedPtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(UnsafeProxyPtr<T> other)
		{
			return indexedPtr == other.indexedPtr && proxy.FirstDelegateIndex.index == other.proxy.FirstDelegateIndex.index;
		}

		public override int GetHashCode()
		{
			return indexedPtr.GetHashCode();
		}
	}
}
