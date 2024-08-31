using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.Data
{
	public struct ProxyEvent<T> : IEnumerable<IntPtr> where T: unmanaged, IProxy
	{
		public HashSet<ProxyRef<T>> proxies;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref Allocator GetAllocator()
		{
			return ref proxies.GetAllocator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ProxyEvent(ref Allocator allocator, uint capacity)
		{
			proxies = new HashSet<ProxyRef<T>>(ref allocator, capacity);
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
		public HashSet<ProxyRef<T>>.IntPtrEnumerable GetEnumerable(in Allocator allocator)
		{
			return proxies.GetIntPtrEnumerable(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSet<ProxyRef<T>>.IntPtrEnumerator GetEnumerator(in Allocator allocator)
		{
			return proxies.GetIntPtrEnumerator(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSet<ProxyRef<T>>.IntPtrEnumerator GetEnumerator()
		{
			return proxies.GetIntPtrEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator<IntPtr> IEnumerable<IntPtr>.GetEnumerator()
		{
			return proxies.GetIntPtrEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Dispose()
		{
			proxies.Dispose();
		}
	}
}
