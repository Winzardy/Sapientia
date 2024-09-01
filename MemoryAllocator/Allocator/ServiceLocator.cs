using System.Runtime.CompilerServices;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public unsafe struct ServiceLocator
	{
		private Dictionary<TypeIndex, ValueRef> _typeToPtr;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ServiceLocator Create(uint capacity = 128u)
		{
			return Create(ref AllocatorManager.CurrentAllocator, capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ServiceLocator Create(ref Allocator allocator, uint capacity = 128u)
		{
			return new ServiceLocator
			{
				_typeToPtr = new Dictionary<TypeIndex, ValueRef>(ref allocator, capacity),
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(MemPtr ptr) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			_typeToPtr.Add(typeIndex, new ValueRef(ptr, typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			_typeToPtr.Add(typeIndex, new ValueRef(ptr, typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(Ptr ptr) where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			_typeToPtr.Add(typeIndex, new ValueRef(ptr, typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(ValueRef valueRef)
		{
			_typeToPtr.Add(valueRef.typeIndex, valueRef);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterServiceAs<T, TBase>(Ptr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			var typeIndex = TypeIndex.Create<TBase>();
			_typeToPtr.Add(typeIndex, new ValueRef(ptr, typeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService<T>() where T: unmanaged, IIndexedType
		{
			var typeIndex = TypeIndex.Create<T>();
			_typeToPtr.Remove(typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService(ValueRef valueRef)
		{
			_typeToPtr.Remove(valueRef.typeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>() where T: unmanaged, IIndexedType
		{
			ref var allocator = ref _typeToPtr.GetAllocator();
			var typeIndex = TypeIndex.Create<T>();

			return ref _typeToPtr.GetValue(ref allocator, typeIndex).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(out bool exist) where T: unmanaged, IIndexedType
		{
			ref var allocator = ref _typeToPtr.GetAllocator();
			var typeIndex = TypeIndex.Create<T>();

			return ref _typeToPtr.GetValue(ref allocator, typeIndex, out exist).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetServicePtr<T>() where T: unmanaged, IIndexedType
		{
			ref var allocator = ref _typeToPtr.GetAllocator();
			var typeIndex = TypeIndex.Create<T>();

			return _typeToPtr.GetValue(ref allocator, typeIndex).GetPtr<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(ProxyRef<T> proxyRef) where T: unmanaged, IProxy
		{
			ref var allocator = ref _typeToPtr.GetAllocator();
			return ref _typeToPtr.GetValue(ref allocator, proxyRef.valueRef.typeIndex).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(ProxyRef<T> proxyRef, out bool exist) where T: unmanaged, IProxy
		{
			ref var allocator = ref _typeToPtr.GetAllocator();
			return ref _typeToPtr.GetValue(ref allocator, proxyRef.valueRef.typeIndex, out exist).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetServiceAs<TBase, T>() where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			ref var allocator = ref _typeToPtr.GetAllocator();
			var typeIndex = TypeIndex.Create<TBase>();

			return ref _typeToPtr.GetValue(ref allocator, typeIndex).GetValue<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetServiceAsPtr<TBase, T>() where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			ref var allocator = ref _typeToPtr.GetAllocator();
			var typeIndex = TypeIndex.Create<TBase>();

			return _typeToPtr.GetValue(ref allocator, typeIndex).GetPtr<T>();
		}
	}

	public static unsafe class ServiceLocatorExt
	{
		#region Allocator

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref Allocator allocator, MemPtr ptr) where T: unmanaged, IIndexedType
		{
			allocator.serviceLocator.RegisterService<T>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref Allocator allocator, Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			allocator.serviceLocator.RegisterService(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref Allocator allocator, Ptr ptr) where T: unmanaged, IIndexedType
		{
			allocator.serviceLocator.RegisterService<T>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService(this ref Allocator allocator, ValueRef valueRef)
		{
			allocator.serviceLocator.RegisterService(valueRef);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveService<T>(this ref Allocator allocator) where T: unmanaged, IIndexedType
		{
			allocator.serviceLocator.RemoveService<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterServiceAs<T, TBase>(this ref Allocator allocator, Ptr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			allocator.serviceLocator.RegisterServiceAs<T, TBase>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveService(this ref Allocator allocator, ValueRef valueRef)
		{
			allocator.serviceLocator.RemoveService(valueRef);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref Allocator allocator) where T: unmanaged, IIndexedType
		{
			return ref allocator.serviceLocator.GetService<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T* GetServicePtr<T>(this ref Allocator allocator) where T: unmanaged, IIndexedType
		{
			return allocator.serviceLocator.GetServicePtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref Allocator allocator, out bool exist) where T: unmanaged, IIndexedType
		{
			return ref allocator.serviceLocator.GetService<T>(out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref Allocator allocator, ProxyRef<T> proxyRef) where T: unmanaged, IProxy
		{
			return ref allocator.serviceLocator.GetService(proxyRef);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref Allocator allocator, ProxyRef<T> proxyRef, out bool exist) where T: unmanaged, IProxy
		{
			return ref allocator.serviceLocator.GetService(proxyRef, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetServiceAs<TBase, T>(this ref Allocator allocator) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return ref allocator.serviceLocator.GetServiceAs<TBase, T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T* GetServiceAsPtr<TBase, T>(this ref Allocator allocator) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return allocator.serviceLocator.GetServiceAsPtr<TBase, T>();
		}

		#endregion

		#region AllocatorId

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref AllocatorId allocatorId, MemPtr ptr) where T: unmanaged, IIndexedType
		{
			allocatorId.GetAllocator().serviceLocator.RegisterService<T>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref AllocatorId allocatorId, Ptr<T> ptr) where T: unmanaged, IIndexedType
		{
			allocatorId.GetAllocator().serviceLocator.RegisterService(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService<T>(this ref AllocatorId allocatorId, Ptr ptr) where T: unmanaged, IIndexedType
		{
			allocatorId.GetAllocator().serviceLocator.RegisterService<T>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterService(this ref AllocatorId allocatorId, ValueRef valueRef)
		{
			allocatorId.GetAllocator().serviceLocator.RegisterService(valueRef);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveService<T>(this ref AllocatorId allocatorId) where T: unmanaged, IIndexedType
		{
			allocatorId.GetAllocator().serviceLocator.RemoveService<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RegisterServiceAs<T, TBase>(this ref AllocatorId allocatorId, Ptr<T> ptr) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			allocatorId.GetAllocator().serviceLocator.RegisterServiceAs<T, TBase>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveService(this ref AllocatorId allocatorId, ValueRef valueRef)
		{
			allocatorId.GetAllocator().serviceLocator.RemoveService(valueRef);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref AllocatorId allocatorId) where T: unmanaged, IIndexedType
		{
			return ref allocatorId.GetAllocator().serviceLocator.GetService<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T* GetServicePtr<T>(this ref AllocatorId allocatorId) where T: unmanaged, IIndexedType
		{
			return allocatorId.GetAllocator().serviceLocator.GetServicePtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref AllocatorId allocatorId, out bool exist) where T: unmanaged, IIndexedType
		{
			return ref allocatorId.GetAllocator().serviceLocator.GetService<T>(out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref AllocatorId allocatorId, ProxyRef<T> proxyRef) where T: unmanaged, IProxy
		{
			return ref allocatorId.GetAllocator().serviceLocator.GetService(proxyRef);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetService<T>(this ref AllocatorId allocatorId, ProxyRef<T> proxyRef, out bool exist) where T: unmanaged, IProxy
		{
			return ref allocatorId.GetAllocator().serviceLocator.GetService(proxyRef, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T GetServiceAs<TBase, T>(this ref AllocatorId allocatorId) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return ref allocatorId.GetAllocator().serviceLocator.GetServiceAs<TBase, T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T* GetServiceAsPtr<TBase, T>(this ref AllocatorId allocatorId) where TBase: unmanaged, IIndexedType where T: unmanaged
		{
			return allocatorId.GetAllocator().serviceLocator.GetServiceAsPtr<TBase, T>();
		}

		#endregion
	}
}
