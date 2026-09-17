using Sapientia.MemoryAllocator.State;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public struct Callback<TCallback>  where TCallback: unmanaged, IProxy
	{
		public Entity callbackReceiver;
		public ProxyPtr<TCallback> callback;
	}
}
