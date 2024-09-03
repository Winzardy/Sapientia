using System;
using System.Runtime.CompilerServices;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.Data
{
	public unsafe struct ProxyRef<T> : IEquatable<ProxyRef<T>> where T: unmanaged, IProxy
	{
		public ValueRef valueRef;
		public readonly T proxy;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ProxyRef(ValueRef valueRef)
		{
			proxy = IndexedTypes.GetProxy<T>(valueRef.typeIndex);
			this.valueRef = valueRef;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ProxyRef<T1> ToProxy<T1>() where T1: unmanaged, IProxy
		{
			return new ProxyRef<T1>(valueRef);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void* GetPtr(Allocator* allocator)
		{
			return valueRef.GetPtr(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void* GetPtr()
		{
			return valueRef.GetPtr();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ProxyRef<T>(ValueRef value)
		{
			return new ProxyRef<T>(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ValueRef(ProxyRef<T> value)
		{
			return value.valueRef;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(ProxyRef<T> other)
		{
			return valueRef == other.valueRef && proxy.FirstDelegateIndex.index == other.proxy.FirstDelegateIndex.index;
		}
	}
}
