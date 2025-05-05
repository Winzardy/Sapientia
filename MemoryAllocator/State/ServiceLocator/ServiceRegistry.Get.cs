using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public partial struct ServiceRegistry
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(World world, ServiceRegistryContext context) where T: unmanaged
		{
			ref var result = ref _typeToPtr.GetValue(world, context, out var success).GetValue<T>(world);
			E.ASSERT(success);
			return ref result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(World world) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return ref GetService<T>(world, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryGetService<T>(World world, ServiceRegistryContext context, out bool isExist) where T: unmanaged
		{
			ref var ptr = ref _typeToPtr.GetValue(world, context, out isExist);
			if (isExist)
				return ref ptr.GetValue<T>(world);
			return ref TDefaultValue<T>.value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(World world, out bool isExist) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();

			ref var ptr = ref _typeToPtr.GetValue(world, typeIndex, out isExist);
			if (isExist)
				return ref ptr.GetValue<T>(world);
			return ref TDefaultValue<T>.value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(World world, ProxyPtr<T> proxyPtr) where T: unmanaged, IProxy
		{
			ref var result = ref _typeToPtr.GetValue(world, proxyPtr.indexedPtr.typeIndex, out var success).GetValue<T>(world);
			E.ASSERT(success);
			return ref result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(World world, ProxyPtr<T> proxyPtr, out bool isExist) where T: unmanaged, IProxy
		{
			ref var ptr = ref _typeToPtr.GetValue(world, proxyPtr.indexedPtr.typeIndex, out isExist);
			if (isExist)
				return ref ptr.GetValue<T>(world);
			return ref TDefaultValue<T>.value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetServiceAs<TBase, T>(World world) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var typeIndex = TypeIndex.Create<TBase>();
			ref var result = ref _typeToPtr.GetValue(world, typeIndex, out var success).GetValue<T>(world);
			E.ASSERT(success);

			return ref result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>(World world, ServiceRegistryContext context) where T: unmanaged
		{
			var result = _typeToPtr.GetValue(world, context, out var success);
			E.ASSERT(success);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>(World world) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return GetServiceIndexedPtr<T>(world, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CWPtr<T> GetServiceCachedPtr<T>(World world, ServiceRegistryContext context) where T: unmanaged
		{
			var result = _typeToPtr.GetValue(world, context, out var success).GetCachedPtr<T>();
			E.ASSERT(success);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CWPtr<T> GetServiceCachedPtr<T>(World world) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return GetServiceCachedPtr<T>(world, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetServicePtr<T>(World world, ServiceRegistryContext context) where T: unmanaged
		{
			var result = _typeToPtr.GetValue(world, context, out var success).GetPtr<T>(world);
			E.ASSERT(success);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetServicePtr<T>(World world) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return GetServicePtr<T>(world, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetServicePtr<T>(World world, out SafePtr<T> ptr) where T: unmanaged, IIndexedType
		{
			ptr = default;
			var typeIndex = TypeIndex.Create<T>();
			ref var refPtr = ref _typeToPtr.GetValue(world, typeIndex, out var success);
			if (success)
				ptr = refPtr.GetPtr<T>(world);
			return success;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetServiceAsPtr<TBase, T>(World world) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var typeIndex = TypeIndex.Create<TBase>();
			var result = _typeToPtr.GetValue(world, typeIndex, out var success).GetPtr<T>(world);
			E.ASSERT(success);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasService<T>(World world) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return _typeToPtr.ContainsKey(world, typeIndex);
		}
	}
}
