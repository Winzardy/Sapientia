using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public unsafe struct ProxyEvent<T> where T: unmanaged, IProxy
	{
		private HashSet<ProxyPtr<T>> _proxies;

		public bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _proxies.IsCreated;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ProxyEvent(World world, int capacity = 8)
		{
			_proxies = new HashSet<ProxyPtr<T>>(world, capacity);
		}

		public bool Subscribe(World world, in ProxyPtr<T> proxyPtr)
		{
			return _proxies.Add(world, proxyPtr);
		}

		public bool UnSubscribe(World world, in ProxyPtr<T> proxyPtr)
		{
			return _proxies.Remove(world, proxyPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<SafePtr<ProxyPtr<T>>, HashSetPtrEnumerator<ProxyPtr<T>>> GetEnumerable(World world)
		{
			return _proxies.GetPtrEnumerable(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetPtrEnumerator<ProxyPtr<T>> GetEnumerator(World world)
		{
			return _proxies.GetPtrEnumerator(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(World world, bool disposeProxies = true)
		{
			if (disposeProxies)
			{
				foreach (ProxyPtr<T>* proxy in _proxies.GetPtrEnumerable(world))
				{
					proxy->Dispose(world);
				}
			}
			_proxies.Dispose(world);
		}
	}
}
