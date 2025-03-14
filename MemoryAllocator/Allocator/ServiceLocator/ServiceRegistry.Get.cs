using System.Diagnostics;
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
			ref var result = ref _typeToPtr.GetValue(allocator, context, out var success).GetValue<T>(allocator);
			E.ASSERT(success, $"Service not found: {typeof(T).Name}");
			return ref result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(Allocator* allocator) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return ref GetService<T>(allocator, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();
			return ref GetService<T>(allocator, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(Allocator* allocator, ServiceRegistryContext context, out bool isExist) where T: unmanaged
		{
			ref var ptr = ref _typeToPtr.GetValue(allocator, context, out isExist);
			if (isExist)
				return ref ptr.GetValue<T>(allocator);
			return ref TDefaultValue<T>.value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(Allocator* allocator, out bool isExist) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();

			ref var ptr = ref _typeToPtr.GetValue(allocator, typeIndex, out isExist);
			if (isExist)
				return ref ptr.GetValue<T>(allocator);
			return ref TDefaultValue<T>.value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(out bool isExist) where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();

			ref var ptr = ref _typeToPtr.GetValue(allocator, typeIndex, out isExist);
			if (isExist)
				return ref ptr.GetValue<T>(allocator);
			return ref TDefaultValue<T>.value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(Allocator* allocator, ProxyPtr<T> proxyPtr) where T: unmanaged, IProxy
		{
			ref var result = ref _typeToPtr.GetValue(allocator, proxyPtr.indexedPtr.typeIndex, out var success).GetValue<T>(allocator);
			E.ASSERT(success, $"Service not found: {typeof(T).Name}");
			return ref result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(ProxyPtr<T> proxyPtr) where T: unmanaged, IProxy
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			return ref _typeToPtr.GetValue(allocator, proxyPtr.indexedPtr.typeIndex).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(Allocator* allocator, ProxyPtr<T> proxyPtr, out bool isExist) where T: unmanaged, IProxy
		{
			ref var ptr = ref _typeToPtr.GetValue(allocator, proxyPtr.indexedPtr.typeIndex, out isExist);
			if (isExist)
				return ref ptr.GetValue<T>(allocator);
			return ref TDefaultValue<T>.value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(ProxyPtr<T> proxyPtr, out bool isExist) where T: unmanaged, IProxy
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			ref var ptr = ref _typeToPtr.GetValue(allocator, proxyPtr.indexedPtr.typeIndex, out isExist);
			if (isExist)
				return ref ptr.GetValue<T>(allocator);
			return ref TDefaultValue<T>.value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetServiceAs<TBase, T>(Allocator* allocator) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var typeIndex = TypeIndex.Create<TBase>();
			ref var result = ref _typeToPtr.GetValue(allocator, typeIndex, out var success).GetValue<T>(allocator);
			E.ASSERT(success, $"Service not found: {typeof(T).Name}");

			return ref result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetServiceAs<TBase, T>() where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			return ref GetServiceAs<TBase, T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>(Allocator* allocator, ServiceRegistryContext context) where T: unmanaged
		{
			var result = _typeToPtr.GetValue(allocator, context, out var success);
			E.ASSERT(success, $"Service not found: {typeof(T).Name}");
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>(Allocator* allocator) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return GetServiceIndexedPtr<T>(allocator, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();
			return GetServiceIndexedPtr<T>(allocator, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Ptr<T> GetServiceCachedPtr<T>(Allocator* allocator, ServiceRegistryContext context) where T: unmanaged
		{
			var result = _typeToPtr.GetValue(allocator, context, out var success).GetCachedPtr<T>();
			E.ASSERT(success, $"Service not found: {typeof(T).Name}");
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Ptr<T> GetServiceCachedPtr<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();
			return GetServiceCachedPtr<T>(allocator, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Ptr<T> GetServiceCachedPtr<T>(Allocator* allocator) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return GetServiceCachedPtr<T>(allocator, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetServicePtr<T>(Allocator* allocator, ServiceRegistryContext context) where T: unmanaged
		{
			var result = _typeToPtr.GetValue(allocator, context, out var success).GetPtr<T>(allocator);
			E.ASSERT(success, $"Service not found: {typeof(T).Name}");

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetServicePtr<T>(Allocator* allocator) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return GetServicePtr<T>(allocator, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetServicePtr<T>() where T: unmanaged, IIndexedType
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			var typeIndex = TypeIndex.Create<T>();
			return GetServicePtr<T>(allocator, typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetServicePtr<T>(Allocator* allocator, out T* ptr) where T: unmanaged, IIndexedType
		{
			ptr = null;
			var typeIndex = TypeIndex.Create<T>();
			ref var refPtr = ref _typeToPtr.GetValue(allocator, typeIndex, out var success);
			if (success)
				ptr = refPtr.GetPtr<T>(allocator);
			return success;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetServiceAsPtr<TBase, T>(Allocator* allocator) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var typeIndex = TypeIndex.Create<TBase>();
			var result = _typeToPtr.GetValue(allocator, typeIndex, out var success).GetPtr<T>(allocator);
			E.ASSERT(success, $"Service not found: {typeof(T).Name}");

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetServiceAsPtr<TBase, T>() where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var allocator = _typeToPtr.GetAllocatorPtr();
			return GetServiceAsPtr<TBase, T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasService<T>(Allocator* allocator) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			return _typeToPtr.ContainsKey(allocator, typeIndex);
		}
	}
}
