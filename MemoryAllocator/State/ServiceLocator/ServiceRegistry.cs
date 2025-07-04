namespace Sapientia.MemoryAllocator
{
	public partial struct ServiceRegistry
	{
		public MemDictionary<ServiceRegistryContext, IndexedPtr> _typeToPtr;
	}
}
