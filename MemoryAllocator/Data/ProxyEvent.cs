using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.Data
{
	public unsafe struct ProxyEvent<T> : IEnumerable<IntPtr> where T: unmanaged, IProxy
	{
		public HashSet<ProxyRef<T>> proxies;

		public bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => proxies.IsCreated;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Allocator* GetAllocatorPtr()
		{
			return proxies.GetAllocatorPtr();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ProxyEvent(Allocator* allocator, int capacity)
		{
			proxies = new HashSet<ProxyRef<T>>(allocator, capacity);
		}

		public bool Subscribe(in ProxyRef<T> proxyRef)
		{
			return proxies.Add(proxyRef);
		}

		public bool UnSubscribe(in ProxyRef<T> proxyRef)
		{
			return proxies.Remove(proxyRef);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<IntPtr, HashSetPtrEnumerator<ProxyRef<T>>> GetEnumerable(Allocator* allocator)
		{
			return proxies.GetPtrEnumerable(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<IntPtr, HashSetPtrEnumerator<ProxyRef<T>>> GetEnumerable()
		{
			return proxies.GetPtrEnumerable();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetPtrEnumerator<ProxyRef<T>> GetEnumerator(Allocator* allocator)
		{
			return proxies.GetPtrEnumerator(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetPtrEnumerator<ProxyRef<T>> GetEnumerator()
		{
			return proxies.GetPtrEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator<IntPtr> IEnumerable<IntPtr>.GetEnumerator()
		{
			return proxies.GetPtrEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			proxies.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(Allocator* allocator)
		{
			proxies.Dispose(allocator);
		}
	}
}
