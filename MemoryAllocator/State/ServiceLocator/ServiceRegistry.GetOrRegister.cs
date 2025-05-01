using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct ServiceRegistry
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetOrRegisterServiceIndexedPtr<T>(World world, ServiceRegistryContext context) where T: unmanaged
		{
			ref var result = ref _typeToPtr.GetValue(world, context, out var exist);
			if (!exist)
			{
				result = new IndexedPtr(CWPtr<T>.Create(world), context.typeIndex);
				RegisterService(world, context, result);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetOrRegisterServiceIndexedPtr<T>(World world, ServiceRegistryContext context, out bool exist) where T: unmanaged
		{
			ref var result = ref _typeToPtr.GetValue(world, context, out exist);
			if (!exist)
			{
				result = new IndexedPtr(CWPtr<T>.Create(world), context.typeIndex);
				RegisterService(world, context, result);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetOrRegisterServiceIndexedPtr<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocator();
			var typeIndex = TypeIndex.Create<T>();

			return GetOrRegisterServiceIndexedPtr<T>(allocator, typeIndex);
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
		public ref T GetOrRegisterService<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocator();
			return ref GetOrRegisterService<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(out bool exist) where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocator();
			return ref GetOrRegisterService<T>(allocator, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(ProxyPtr<T> proxyPtr) where T: unmanaged, IProxy
		{
			var allocator = _typeToPtr.GetAllocator();
			return ref GetOrRegisterService<T>(allocator, proxyPtr.indexedPtr.typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(ProxyPtr<T> proxyPtr, out bool exist) where T: unmanaged, IProxy
		{
			var allocator = _typeToPtr.GetAllocator();
			return ref GetOrRegisterService<T>(allocator, proxyPtr.indexedPtr.typeIndex, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(World world, ProxyPtr<T> proxyPtr, out bool exist) where T: unmanaged, IProxy
		{
			return ref GetOrRegisterService<T>(world, proxyPtr.indexedPtr.typeIndex, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterServiceAs<TBase, T>() where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var allocator = _typeToPtr.GetAllocator();
			var typeIndex = TypeIndex.Create<TBase>();

			return ref GetOrRegisterService<T>(allocator, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CWPtr<T> GetOrRegisterServiceCachedPtr<T>(World world, ServiceRegistryContext context) where T: unmanaged
		{
			return GetOrRegisterServiceIndexedPtr<T>(world, context).GetCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CWPtr<T> GetOrRegisterServiceCachedPtr<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocator();
			var typeIndex = TypeIndex.Create<T>();

			return GetOrRegisterServiceIndexedPtr<T>(allocator, typeIndex).GetCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrRegisterServicePtr<T>(World world, ServiceRegistryContext context) where T: unmanaged
		{
			return GetOrRegisterServiceIndexedPtr<T>(world, context).GetPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrRegisterServicePtr<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocator();
			var typeIndex = TypeIndex.Create<T>();

			return GetOrRegisterServiceIndexedPtr<T>(allocator, typeIndex).GetPtr<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrRegisterServiceAsPtr<TBase, T>() where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var allocator = _typeToPtr.GetAllocator();
			var typeIndex = TypeIndex.Create<TBase>();

			return GetOrRegisterServiceIndexedPtr<T>(allocator, typeIndex).GetPtr<T>();
		}
	}
}
