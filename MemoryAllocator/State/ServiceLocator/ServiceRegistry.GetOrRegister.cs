using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public partial struct ServiceRegistry
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetOrRegisterServiceIndexedPtr<T>(WorldState worldState, ServiceRegistryContext context) where T: unmanaged
		{
			var result = _typeToPtr.GetValue(worldState, context, out var exist);
			if (!exist)
			{
				result = new IndexedPtr(CachedPtr<T>.Create(worldState), context.typeIndex);
				RegisterService(worldState, context, result);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetOrRegisterServiceIndexedPtr<T>(WorldState worldState, ServiceRegistryContext context, out bool exist) where T: unmanaged
		{
			var result = _typeToPtr.GetValue(worldState, context, out exist);
			if (!exist)
			{
				result = new IndexedPtr(CachedPtr<T>.Create(worldState), context.typeIndex);
				RegisterService(worldState, context, result);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(WorldState worldState, ServiceRegistryContext context) where T: unmanaged
		{
			return ref GetOrRegisterServiceIndexedPtr<T>(worldState, context).GetValue<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(WorldState worldState, ServiceRegistryContext context, out bool exist) where T: unmanaged
		{
			return ref GetOrRegisterServiceIndexedPtr<T>(worldState, context, out exist).GetValue<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(WorldState worldState) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return ref GetOrRegisterService<T>(worldState, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrRegisterServicePtr<T>(WorldState worldState) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return GetOrRegisterServicePtr<T>(worldState, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(WorldState worldState, out bool exist) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return ref GetOrRegisterService<T>(worldState, typeIndex, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(WorldState worldState, ProxyPtr<T> proxyPtr, out bool exist) where T: unmanaged, IProxy
		{
			return ref GetOrRegisterService<T>(worldState, proxyPtr.indexedPtr.typeIndex, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CachedPtr<T> GetOrRegisterServiceCachedPtr<T>(WorldState worldState, ServiceRegistryContext context) where T: unmanaged
		{
			return GetOrRegisterServiceIndexedPtr<T>(worldState, context).GetCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrRegisterServicePtr<T>(WorldState worldState, ServiceRegistryContext context) where T: unmanaged
		{
			return GetOrRegisterServiceIndexedPtr<T>(worldState, context).GetPtr<T>(worldState);
		}
	}
}
