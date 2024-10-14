using System;
using System.Runtime.CompilerServices;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public unsafe struct ServiceContext : IEquatable<ServiceContext>
	{
		public TypeIndex typeIndex;
		public TypeIndex contextTypeIndex;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ServiceContext Create<T>() where T: unmanaged, IIndexedType
		{
			return new ServiceContext
			{
				typeIndex = TypeIndex<T>.typeIndex,
				contextTypeIndex = -1,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ServiceContext Create<T, TContext>() where T: unmanaged, IIndexedType where TContext: IIndexedType
		{
			return new ServiceContext
			{
				typeIndex = TypeIndex<T>.typeIndex,
				contextTypeIndex = TypeIndex<TContext>.typeIndex,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(ServiceContext other)
		{
			return other.typeIndex == typeIndex && other.contextTypeIndex == contextTypeIndex;
		}

		public static implicit operator ServiceContext(TypeIndex typeIndex)
		{
			return new ServiceContext { typeIndex = typeIndex, contextTypeIndex = -1, };
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(MemPtr ptr)
		{
			AllocatorManager.CurrentAllocatorPtr->serviceLocator.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(Ptr ptr)
		{
			AllocatorManager.CurrentAllocatorPtr->serviceLocator.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(IndexedPtr indexedPtr)
		{
			AllocatorManager.CurrentAllocatorPtr->serviceLocator.RegisterService(this, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService()
		{
			AllocatorManager.CurrentAllocatorPtr->serviceLocator.RemoveService(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>() where T: unmanaged
		{
			return ref GetService<T>(AllocatorManager.CurrentAllocatorPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(out bool exist) where T: unmanaged
		{
			return ref GetService<T>(AllocatorManager.CurrentAllocatorPtr, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>() where T: unmanaged
		{
			return GetServiceIndexedPtr<T>(AllocatorManager.CurrentAllocatorPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Ptr<T> GetServiceCachedPtr<T>() where T: unmanaged
		{
			return GetServiceCachedPtr<T>(AllocatorManager.CurrentAllocatorPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetServicePtr<T>() where T: unmanaged
		{
			return GetServicePtr<T>(AllocatorManager.CurrentAllocatorPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(Allocator* allocator, MemPtr ptr)
		{
			allocator->serviceLocator.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(Allocator* allocator, Ptr ptr)
		{
			allocator->serviceLocator.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(Allocator* allocator, Ptr<T> ptr) where T: unmanaged
		{
			allocator->serviceLocator.RegisterService(this, (Ptr)ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(Allocator* allocator, IndexedPtr indexedPtr)
		{
			allocator->serviceLocator.RegisterService(this, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService(Allocator* allocator)
		{
			allocator->serviceLocator.RemoveService(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(Allocator* allocator) where T: unmanaged
		{
			return ref allocator->serviceLocator.GetService<T>(allocator, this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(Allocator* allocator, out bool exist) where T: unmanaged
		{
			return ref allocator->serviceLocator.GetService<T>(allocator, this, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>(Allocator* allocator) where T: unmanaged
		{
			return allocator->serviceLocator.GetServiceIndexedPtr<T>(allocator, this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Ptr<T> GetServiceCachedPtr<T>(Allocator* allocator) where T: unmanaged
		{
			return allocator->serviceLocator.GetServiceCachedPtr<T>(allocator, this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetServicePtr<T>(Allocator* allocator) where T: unmanaged
		{
			return allocator->serviceLocator.GetServicePtr<T>(allocator, this);
		}
	}
}
