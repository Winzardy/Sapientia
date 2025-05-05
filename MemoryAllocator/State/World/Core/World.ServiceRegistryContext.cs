using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public partial class World
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(ServiceRegistryContext context, MemPtr ptr)
		{
			_serviceRegistry.RegisterService(this, context, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(ServiceRegistryContext context, CachedPtr ptr)
		{
			_serviceRegistry.RegisterService(this, context, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(ServiceRegistryContext context, IndexedPtr ptr)
		{
			_serviceRegistry.RegisterService(this, context, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService(ServiceRegistryContext context)
		{
			_serviceRegistry.RemoveService(this, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(ServiceRegistryContext context) where T: unmanaged
		{
			return ref _serviceRegistry.GetService<T>(this, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryGetService<T>(ServiceRegistryContext context, out bool exist) where T: unmanaged
		{
			return ref _serviceRegistry.TryGetService<T>(this, context, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>(ServiceRegistryContext context) where T: unmanaged
		{
			return _serviceRegistry.GetServiceIndexedPtr<T>(this, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CachedPtr<T> GetServiceCachedPtr<T>(ServiceRegistryContext context) where T: unmanaged
		{
			return _serviceRegistry.GetServiceCachedPtr<T>(this, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetServicePtr<T>(ServiceRegistryContext context) where T: unmanaged
		{
			return _serviceRegistry.GetServicePtr<T>(this, context);
		}
	}
}
