using System;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
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
		public static ProxyPtr<T> Create<TInstance>(World world) where TInstance: unmanaged
		{
			return new ProxyPtr<T>(IndexedPtr.Create<TInstance>(world));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ProxyPtr<T> Create<TInstance>(World world, in TInstance value) where TInstance: unmanaged
		{
			return new ProxyPtr<T>(IndexedPtr.Create(world, value));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ProxyPtr<T> Create<TInstance>(in CachedPtr<TInstance> value) where TInstance: unmanaged
		{
			return new ProxyPtr<T>(value);
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
		public ref T1 GetValue<T1>(World world) where T1: unmanaged
		{
			return ref indexedPtr.GetValue<T1>(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CachedPtr<T1> ToPtr<T1>() where T1: unmanaged
		{
			return new CachedPtr<T1>(indexedPtr.GetMemPtr());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetPtr(World world)
		{
			return indexedPtr.GetPtr(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(World world)
		{
			if (IsCreated)
			{
				proxy.ProxyDispose(GetPtr(world).ptr, world);
				indexedPtr.Dispose(world);
			}
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

		public override int GetHashCode()
		{
			return indexedPtr.GetHashCode();
		}
	}
}
