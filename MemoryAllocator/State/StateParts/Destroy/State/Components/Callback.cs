using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public struct Callback<TCallback>  where TCallback: unmanaged, IProxy
	{
		public Entity callbackReceiver;
		public ProxyPtr<TCallback> callback;
	}
}
