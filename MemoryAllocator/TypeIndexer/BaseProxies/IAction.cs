using Sapientia.Data;
using Sapientia.MemoryAllocator;

namespace Sapientia.TypeIndexer.BaseProxies
{
	public unsafe interface IAction : IInterfaceProxyType
	{
		public void Invoke(SafePtr<Allocator> allocator);
	}
}
