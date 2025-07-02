using System.Runtime.CompilerServices;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public partial struct ServiceRegistry
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(WorldState worldState, ServiceRegistryContext context, MemPtr ptr)
		{
			_typeToPtr.Add(worldState, context, new IndexedPtr(ptr, context.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(WorldState worldState, ServiceRegistryContext context, CachedPtr ptr)
		{
			_typeToPtr.Add(worldState, context, new IndexedPtr(ptr, context.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(WorldState worldState, ServiceRegistryContext context, IndexedPtr indexedPtr)
		{
			_typeToPtr.Add(worldState, context, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(WorldState worldState, MemPtr ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = ServiceRegistryContext.Create<T>();
			_typeToPtr.Add(worldState, serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(WorldState worldState, CachedPtr<T> ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = ServiceRegistryContext.Create<T>();
			_typeToPtr.Add(worldState, serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(WorldState worldState, CachedPtr ptr) where T: unmanaged, IIndexedType
		{
			var serviceContext = ServiceRegistryContext.Create<T>();
			_typeToPtr.Add(worldState, serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(WorldState worldState, IndexedPtr indexedPtr)
		{
			_typeToPtr.Add(worldState, indexedPtr.typeIndex, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterServiceAs<T, TBase>(WorldState worldState, CachedPtr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var serviceContext = ServiceRegistryContext.Create<TBase>();
			_typeToPtr.Add(worldState, serviceContext, new IndexedPtr(ptr, serviceContext.typeIndex));
		}
	}
}
