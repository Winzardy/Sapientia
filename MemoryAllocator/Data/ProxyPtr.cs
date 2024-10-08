using System;
using System.Runtime.CompilerServices;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.Data
{
	public unsafe struct ProxyPtr<T> : IEquatable<ProxyPtr<T>> where T: unmanaged, IProxy
	{
		public IndexedPtr indexedPtr;
		public readonly T proxy;

		public bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => indexedPtr.IsCreated;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ProxyPtr(IndexedPtr indexedPtr)
		{
			proxy = IndexedTypes.GetProxy<T>(indexedPtr.typeIndex);
			this.indexedPtr = indexedPtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ProxyPtr<T> Create<TInstance>(Allocator* allocator) where TInstance: unmanaged
		{
			return new ProxyPtr<T>(IndexedPtr.Create<TInstance>(allocator));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ProxyPtr<T> Create<TInstance>(Allocator* allocator, in TInstance value) where TInstance: unmanaged
		{
			return new ProxyPtr<T>(IndexedPtr.Create(allocator, value));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ProxyPtr<T> Create<TInstance>(in TInstance value) where TInstance: unmanaged
		{
			return new ProxyPtr<T>(IndexedPtr.Create(value));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ProxyPtr<T1> ToProxy<T1>() where T1: unmanaged, IProxy
		{
			return new ProxyPtr<T1>(indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void* GetPtr(Allocator* allocator)
		{
			return indexedPtr.GetPtr(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void* GetPtr()
		{
			return indexedPtr.GetPtr();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			indexedPtr.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(Allocator* allocator)
		{
			indexedPtr.Dispose(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ProxyPtr<T>(IndexedPtr value)
		{
			return new ProxyPtr<T>(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator IndexedPtr(ProxyPtr<T> value)
		{
			return value.indexedPtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(ProxyPtr<T> other)
		{
			return indexedPtr == other.indexedPtr && proxy.FirstDelegateIndex.index == other.proxy.FirstDelegateIndex.index;
		}
	}
}
