using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.Data
{
	public unsafe struct ProxyEvent<T> : IEnumerable<IntPtr> where T: unmanaged, IProxy
	{
		private HashSet<ProxyPtr<T>> _proxies;

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
			_proxies = new HashSet<ProxyPtr<T>>(allocator, capacity);
		}

		public bool Subscribe(Allocator* allocator, in ProxyPtr<T> proxyPtr)
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

		public bool UnSubscribe(Allocator* allocator, in ProxyPtr<T> proxyPtr)
		{
			return _proxies.Remove(allocator, proxyPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<IntPtr, HashSetPtrEnumerator<ProxyPtr<T>>> GetEnumerable(Allocator* allocator)
		{
			return _proxies.GetPtrEnumerable(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<IntPtr, HashSetPtrEnumerator<ProxyPtr<T>>> GetEnumerable()
		{
			return _proxies.GetPtrEnumerable();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetPtrEnumerator<ProxyPtr<T>> GetEnumerator(Allocator* allocator)
		{
			return _proxies.GetPtrEnumerator(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetPtrEnumerator<ProxyPtr<T>> GetEnumerator()
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
