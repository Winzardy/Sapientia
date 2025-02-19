using System.Runtime.CompilerServices;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct ServiceRegistry
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(Allocator* allocator, ServiceRegistryContext context) where T: unmanaged
		{
			return ref _typeToPtr.GetValue(allocator, context).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(Allocator* allocator) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return ref _typeToPtr.GetValue(allocator, typeIndex).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();

			return ref _typeToPtr.GetValue(allocator, typeIndex).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryGetService<T>(Allocator* allocator, ServiceRegistryContext context, out bool isExist) where T: unmanaged
		{
			ref var ptr = ref _typeToPtr.TryGetValue(allocator, context, out isExist);
			if (isExist)
				return ref ptr.GetValue<T>(allocator);
			return ref TDefaultValue<T>.value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryGetService<T>(Allocator* allocator, out bool isExist) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();

			ref var ptr = ref _typeToPtr.TryGetValue(allocator, typeIndex, out isExist);
			if (isExist)
				return ref ptr.GetValue<T>(allocator);
			return ref TDefaultValue<T>.value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryGetService<T>(out bool isExist) where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();

			ref var ptr = ref _typeToPtr.TryGetValue(allocator, typeIndex, out isExist);
			if (isExist)
				return ref ptr.GetValue<T>(allocator);
			return ref TDefaultValue<T>.value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(Allocator* allocator, ProxyPtr<T> proxyPtr) where T: unmanaged, IProxy
		{
			return ref _typeToPtr.GetValue(allocator, proxyPtr.indexedPtr.typeIndex).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(ProxyPtr<T> proxyPtr) where T: unmanaged, IProxy
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			return ref _typeToPtr.GetValue(allocator, proxyPtr.indexedPtr.typeIndex).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryGetService<T>(Allocator* allocator, ProxyPtr<T> proxyPtr, out bool isExist) where T: unmanaged, IProxy
		{
			ref var ptr = ref _typeToPtr.TryGetValue(allocator, proxyPtr.indexedPtr.typeIndex, out isExist);
			if (isExist)
				return ref ptr.GetValue<T>(allocator);
			return ref TDefaultValue<T>.value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryGetService<T>(ProxyPtr<T> proxyPtr, out bool isExist) where T: unmanaged, IProxy
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			ref var ptr = ref _typeToPtr.TryGetValue(allocator, proxyPtr.indexedPtr.typeIndex, out isExist);
			if (isExist)
				return ref ptr.GetValue<T>(allocator);
			return ref TDefaultValue<T>.value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetServiceAs<TBase, T>(Allocator* allocator) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var typeIndex = TypeIndex.Create<TBase>();

			return ref _typeToPtr.GetValue(allocator, typeIndex).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetServiceAs<TBase, T>() where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<TBase>();

			return ref _typeToPtr.GetValue(allocator, typeIndex).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>(Allocator* allocator, ServiceRegistryContext context) where T: unmanaged
		{
			return _typeToPtr.GetValue(allocator, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>(Allocator* allocator) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();

			return _typeToPtr.GetValue(allocator, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();

			return _typeToPtr.GetValue(allocator, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Ptr<T> GetServiceCachedPtr<T>(Allocator* allocator, ServiceRegistryContext context) where T: unmanaged
		{
			return _typeToPtr.GetValue(allocator, context).GetCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Ptr<T> GetServiceCachedPtr<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();

			return _typeToPtr.GetValue(allocator, typeIndex).GetCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Ptr<T> GetServiceCachedPtr<T>(Allocator* allocator) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();

			return _typeToPtr.GetValue(allocator, typeIndex).GetCachedPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetServicePtr<T>(Allocator* allocator, ServiceRegistryContext context) where T: unmanaged
		{
			return _typeToPtr.GetValue(allocator, context).GetPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetServicePtr<T>(Allocator* allocator) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();

			return _typeToPtr.GetValue(allocator, typeIndex).GetPtr<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetServicePtr<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();

			return _typeToPtr.GetValue(allocator, typeIndex).GetPtr<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* TryGetServicePtr<T>(Allocator* allocator, out bool isExist) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			ref var ptr = ref _typeToPtr.TryGetValue(allocator, typeIndex, out isExist);
			if (isExist)
				return ptr.GetPtr<T>(allocator);
			return null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetServiceAsPtr<TBase, T>(Allocator* allocator) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var typeIndex = TypeIndex.Create<TBase>();

			return _typeToPtr.GetValue(allocator, typeIndex).GetPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetServiceAsPtr<TBase, T>() where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<TBase>();

			return _typeToPtr.GetValue(allocator, typeIndex).GetPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasService<T>(Allocator* allocator) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return _typeToPtr.ContainsKey(allocator, typeIndex);
		}
	}
}
