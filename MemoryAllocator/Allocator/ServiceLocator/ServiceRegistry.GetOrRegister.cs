using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct ServiceRegistry
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetOrRegisterServiceIndexedPtr<T>(SafePtr<Allocator> allocator, ServiceRegistryContext context) where T: unmanaged
		{
			ref var result = ref _typeToPtr.GetValue(allocator, context, out var exist);
			if (!exist)
			{
				result = new IndexedPtr(Ptr<T>.Create(allocator), context.typeIndex);
				RegisterService(allocator, context, result);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetOrRegisterServiceIndexedPtr<T>(SafePtr<Allocator> allocator, ServiceRegistryContext context, out bool exist) where T: unmanaged
		{
			ref var result = ref _typeToPtr.GetValue(allocator, context, out exist);
			if (!exist)
			{
				result = new IndexedPtr(Ptr<T>.Create(allocator), context.typeIndex);
				RegisterService(allocator, context, result);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetOrRegisterServiceIndexedPtr<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();

			return GetOrRegisterServiceIndexedPtr<T>(allocator, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(SafePtr<Allocator> allocator, ServiceRegistryContext context) where T: unmanaged
		{
			return ref GetOrRegisterServiceIndexedPtr<T>(allocator, context).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(SafePtr<Allocator> allocator, ServiceRegistryContext context, out bool exist) where T: unmanaged
		{
			return ref GetOrRegisterServiceIndexedPtr<T>(allocator, context, out exist).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(SafePtr<Allocator> allocator) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return ref GetOrRegisterService<T>(allocator, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrRegisterServicePtr<T>(SafePtr<Allocator> allocator) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return GetOrRegisterServicePtr<T>(allocator, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(SafePtr<Allocator> allocator, out bool exist) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return ref GetOrRegisterService<T>(allocator, typeIndex, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			return ref GetOrRegisterService<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(out bool exist) where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			return ref GetOrRegisterService<T>(allocator, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(ProxyPtr<T> proxyPtr) where T: unmanaged, IProxy
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			return ref GetOrRegisterService<T>(allocator, proxyPtr.indexedPtr.typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(ProxyPtr<T> proxyPtr, out bool exist) where T: unmanaged, IProxy
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			return ref GetOrRegisterService<T>(allocator, proxyPtr.indexedPtr.typeIndex, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterService<T>(SafePtr<Allocator> allocator, ProxyPtr<T> proxyPtr, out bool exist) where T: unmanaged, IProxy
		{
			return ref GetOrRegisterService<T>(allocator, proxyPtr.indexedPtr.typeIndex, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrRegisterServiceAs<TBase, T>() where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<TBase>();

			return ref GetOrRegisterService<T>(allocator, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Ptr<T> GetOrRegisterServiceCachedPtr<T>(SafePtr<Allocator> allocator, ServiceRegistryContext context) where T: unmanaged
		{
			return GetOrRegisterServiceIndexedPtr<T>(allocator, context).GetCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Ptr<T> GetOrRegisterServiceCachedPtr<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();

			return GetOrRegisterServiceIndexedPtr<T>(allocator, typeIndex).GetCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrRegisterServicePtr<T>(SafePtr<Allocator> allocator, ServiceRegistryContext context) where T: unmanaged
		{
			return GetOrRegisterServiceIndexedPtr<T>(allocator, context).GetPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrRegisterServicePtr<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();

			return GetOrRegisterServiceIndexedPtr<T>(allocator, typeIndex).GetPtr<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrRegisterServiceAsPtr<TBase, T>() where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<TBase>();

			return GetOrRegisterServiceIndexedPtr<T>(allocator, typeIndex).GetPtr<T>();
		}
	}
}
