using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.Data
{
	public unsafe struct ProxyEvent<T> : IEnumerable<SafePtr> where T: unmanaged, IProxy
	{
		private HashSet<ProxyPtr<T>> _proxies;

		public bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _proxies.IsCreated;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<Allocator> GetAllocatorPtr()
		{
			return _proxies.GetAllocatorPtr();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ProxyEvent(SafePtr<Allocator> allocator, int capacity = 8)
		{
			_proxies = new HashSet<ProxyPtr<T>>(allocator, capacity);
		}

		public bool Subscribe(SafePtr<Allocator> allocator, in ProxyPtr<T> proxyPtr)
		{
			return _proxies.Add(allocator, proxyPtr);
		}

		public bool Subscribe(in ProxyPtr<T> proxyPtr)
		{
			return _proxies.Add(proxyPtr);
		}

		public bool UnSubscribe(in ProxyPtr<T> proxyPtr)
		{
			return _proxies.Remove(proxyPtr);
		}

		public bool UnSubscribe(SafePtr<Allocator> allocator, in ProxyPtr<T> proxyPtr)
		{
			return _proxies.Remove(allocator, proxyPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<SafePtr<ProxyPtr<T>>, HashSetPtrEnumerator<ProxyPtr<T>>> GetEnumerable(SafePtr<Allocator> allocator)
		{
			return _proxies.GetPtrEnumerable(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<SafePtr<ProxyPtr<T>>, HashSetPtrEnumerator<ProxyPtr<T>>> GetEnumerable()
		{
			return _proxies.GetPtrEnumerable();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetPtrEnumerator<ProxyPtr<T>> GetEnumerator(SafePtr<Allocator> allocator)
		{
			return _proxies.GetPtrEnumerator(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetPtrEnumerator<ProxyPtr<T>> GetEnumerator()
		{
			return _proxies.GetPtrEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator<SafePtr> IEnumerable<SafePtr>.GetEnumerator()
		{
			return _proxies.GetPtrEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(bool disposeProxies = true)
		{
			Dispose(_proxies.GetAllocatorPtr(), disposeProxies);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(SafePtr<Allocator> allocator, bool disposeProxies = true)
		{
			if (disposeProxies)
			{
				foreach (ProxyPtr<T>* proxy in _proxies.GetPtrEnumerable(allocator))
				{
					proxy->Dispose(allocator);
				}
			}
			_proxies.Dispose(allocator);
		}
	}
}
