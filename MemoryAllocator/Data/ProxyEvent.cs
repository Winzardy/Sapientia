using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.Data
{
	public unsafe struct ProxyEvent<T> : IEnumerable<IntPtr> where T: unmanaged, IProxy
	{
		private HashSet<ProxyRef<T>> _proxies;

		public bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _proxies.IsCreated;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Allocator* GetAllocatorPtr()
		{
			return _proxies.GetAllocatorPtr();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ProxyEvent(Allocator* allocator, int capacity)
		{
			_proxies = new HashSet<ProxyRef<T>>(allocator, capacity);
		}

		public bool Subscribe(in ProxyRef<T> proxyRef)
		{
			return _proxies.Add(proxyRef);
		}

		public bool UnSubscribe(in ProxyRef<T> proxyRef)
		{
			return _proxies.Remove(proxyRef);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<IntPtr, HashSetPtrEnumerator<ProxyRef<T>>> GetEnumerable(Allocator* allocator)
		{
			return _proxies.GetPtrEnumerable(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<IntPtr, HashSetPtrEnumerator<ProxyRef<T>>> GetEnumerable()
		{
			return _proxies.GetPtrEnumerable();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetPtrEnumerator<ProxyRef<T>> GetEnumerator(Allocator* allocator)
		{
			return _proxies.GetPtrEnumerator(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetPtrEnumerator<ProxyRef<T>> GetEnumerator()
		{
			return _proxies.GetPtrEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator<IntPtr> IEnumerable<IntPtr>.GetEnumerator()
		{
			return _proxies.GetPtrEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			_proxies.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(Allocator* allocator)
		{
			_proxies.Dispose(allocator);
		}
	}
}
