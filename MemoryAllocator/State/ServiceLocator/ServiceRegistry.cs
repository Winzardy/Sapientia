namespace Sapientia.MemoryAllocator
{
	public partial struct ServiceRegistry
	{
		public Dictionary<ServiceRegistryContext, IndexedPtr> _typeToPtr;
	}
}
