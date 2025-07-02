using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public partial struct WorldState
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(ServiceRegistryContext context, MemPtr ptr)
		{
			GetServiceRegistry().RegisterService(this, context, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(ServiceRegistryContext context, CachedPtr ptr)
		{
			GetServiceRegistry().RegisterService(this, context, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(ServiceRegistryContext context, IndexedPtr ptr)
		{
			GetServiceRegistry().RegisterService(this, context, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService(ServiceRegistryContext context)
		{
			GetServiceRegistry().RemoveService(this, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(ServiceRegistryContext context) where T: unmanaged
		{
			return ref GetServiceRegistry().GetService<T>(this, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryGetService<T>(ServiceRegistryContext context, out bool exist) where T: unmanaged
		{
			return ref GetServiceRegistry().TryGetService<T>(this, context, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>(ServiceRegistryContext context) where T: unmanaged
		{
			return GetServiceRegistry().GetServiceIndexedPtr<T>(this, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CachedPtr<T> GetServiceCachedPtr<T>(ServiceRegistryContext context) where T: unmanaged
		{
			return GetServiceRegistry().GetServiceCachedPtr<T>(this, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetServicePtr<T>(ServiceRegistryContext context) where T: unmanaged
		{
			return GetServiceRegistry().GetServicePtr<T>(this, context);
		}
	}
}
