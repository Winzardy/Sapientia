using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public partial struct ServiceRegistry
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetOrRegisterServiceIndexedPtr<T>(World world, ServiceRegistryContext context) where T: unmanaged
		{
			var result = _typeToPtr.GetValue(world, context, out var exist);
			if (!exist)
			{
				result = new IndexedPtr(CachedPtr<T>.Create(world), context.typeIndex);
				RegisterService(world, context, result);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetOrRegisterServiceIndexedPtr<T>(World world, ServiceRegistryContext context, out bool exist) where T: unmanaged
		{
			var result = _typeToPtr.GetValue(world, context, out exist);
			if (!exist)
			{
				result = new IndexedPtr(CachedPtr<T>.Create(world), context.typeIndex);
				RegisterService(world, context, result);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(World world, ServiceRegistryContext context) where T: unmanaged
		{
			return ref GetOrRegisterServiceIndexedPtr<T>(world, context).GetValue<T>(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(World world, ServiceRegistryContext context, out bool exist) where T: unmanaged
		{
			return ref GetOrRegisterServiceIndexedPtr<T>(world, context, out exist).GetValue<T>(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(World world) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return ref GetOrRegisterService<T>(world, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrRegisterServicePtr<T>(World world) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return GetOrRegisterServicePtr<T>(world, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(World world, out bool exist) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return ref GetOrRegisterService<T>(world, typeIndex, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(World world, ProxyPtr<T> proxyPtr, out bool exist) where T: unmanaged, IProxy
		{
			return ref GetOrRegisterService<T>(world, proxyPtr.indexedPtr.typeIndex, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CachedPtr<T> GetOrRegisterServiceCachedPtr<T>(World world, ServiceRegistryContext context) where T: unmanaged
		{
			return GetOrRegisterServiceIndexedPtr<T>(world, context).GetCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrRegisterServicePtr<T>(World world, ServiceRegistryContext context) where T: unmanaged
		{
			return GetOrRegisterServiceIndexedPtr<T>(world, context).GetPtr<T>(world);
		}
	}
}
