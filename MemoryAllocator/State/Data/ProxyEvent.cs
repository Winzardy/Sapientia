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
		public ProxyEvent(WorldState worldState, int capacity = 8)
		{
			_proxies = new HashSet<ProxyPtr<T>>(worldState, capacity);
		}

		public bool Subscribe(WorldState worldState, in ProxyPtr<T> proxyPtr)
		{
			return _proxies.Add(worldState, proxyPtr);
		}

		public bool UnSubscribe(WorldState worldState, in ProxyPtr<T> proxyPtr)
		{
			return _proxies.Remove(worldState, proxyPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetEnumerator<ProxyPtr<T>> GetEnumerator(WorldState worldState)
		{
			return _proxies.GetEnumerator(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetEnumerable<ProxyPtr<T>> GetEnumerable(WorldState worldState)
		{
			return _proxies.GetEnumerable(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(WorldState worldState, bool disposeProxies = true)
		{
			if (disposeProxies)
			{
				foreach (ref var proxy in _proxies.GetEnumerable(worldState))
				{
					proxy.Dispose(worldState);
				}
			}
			_proxies.Dispose(worldState);
		}
	}
}
