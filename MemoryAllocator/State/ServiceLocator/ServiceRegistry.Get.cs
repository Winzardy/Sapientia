using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public partial struct ServiceRegistry
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(WorldState worldState, ServiceRegistryContext context) where T: unmanaged
		{
			var ptr = _typeToPtr.GetValue(worldState, context, out var success);
			E.ASSERT(success);
			ref var result = ref ptr.GetValue<T>(worldState);
			return ref result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(WorldState worldState) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return ref GetService<T>(worldState, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryGetService<T>(WorldState worldState, ServiceRegistryContext context, out bool isExist) where T: unmanaged
		{
			ref var ptr = ref _typeToPtr.GetValue(worldState, context, out isExist);
			if (isExist)
				return ref ptr.GetValue<T>(worldState);
			return ref UnsafeExt.DefaultRef<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(WorldState worldState, out bool isExist) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();

			ref var ptr = ref _typeToPtr.GetValue(worldState, typeIndex, out isExist);
			if (isExist)
				return ref ptr.GetValue<T>(worldState);
			return ref UnsafeExt.DefaultRef<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(WorldState worldState, ProxyPtr<T> proxyPtr) where T: unmanaged, IProxy
		{
			var ptr = _typeToPtr.GetValue(worldState, proxyPtr.indexedPtr.typeIndex, out var success);
			E.ASSERT(success);
			return ref ptr.GetValue<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(WorldState worldState, ProxyPtr<T> proxyPtr, out bool isExist) where T: unmanaged, IProxy
		{
			ref var ptr = ref _typeToPtr.GetValue(worldState, proxyPtr.indexedPtr.typeIndex, out isExist);
			if (isExist)
				return ref ptr.GetValue<T>(worldState);
			return ref UnsafeExt.DefaultRef<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetServiceAs<TBase, T>(WorldState worldState) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var typeIndex = TypeIndex.Create<TBase>();
			var ptr = _typeToPtr.GetValue(worldState, typeIndex, out var success);
			E.ASSERT(success);

			return ref ptr.GetValue<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>(WorldState worldState, ServiceRegistryContext context) where T: unmanaged
		{
			var result = _typeToPtr.GetValue(worldState, context, out var success);
			E.ASSERT(success);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>(WorldState worldState) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return GetServiceIndexedPtr<T>(worldState, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CachedPtr<T> GetServiceCachedPtr<T>(WorldState worldState, ServiceRegistryContext context) where T: unmanaged
		{
			var result = _typeToPtr.GetValue(worldState, context, out var success);
			E.ASSERT(success);
			return result.GetCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CachedPtr<T> GetServiceCachedPtr<T>(WorldState worldState) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return GetServiceCachedPtr<T>(worldState, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetServicePtr<T>(WorldState worldState, ServiceRegistryContext context) where T: unmanaged
		{
			var result = _typeToPtr.GetValue(worldState, context, out var success);
			E.ASSERT(success);

			return result.GetPtr<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetServicePtr<T>(WorldState worldState) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return GetServicePtr<T>(worldState, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetServicePtr<T>(WorldState worldState, out SafePtr<T> ptr) where T: unmanaged, IIndexedType
		{
			ptr = default;
			var typeIndex = TypeIndex.Create<T>();
			ref var refPtr = ref _typeToPtr.GetValue(worldState, typeIndex, out var success);
			if (success)
				ptr = refPtr.GetPtr<T>(worldState);
			return success;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetServiceAsPtr<TBase, T>(WorldState worldState) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var typeIndex = TypeIndex.Create<TBase>();
			var result = _typeToPtr.GetValue(worldState, typeIndex, out var success);
			E.ASSERT(success);

			return result.GetPtr<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasService<T>(WorldState worldState) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return _typeToPtr.ContainsKey(worldState, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasService(WorldState worldState, ServiceRegistryContext context)
		{
			return _typeToPtr.ContainsKey(worldState, context);
		}
	}
}
