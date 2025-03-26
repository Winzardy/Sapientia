using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct ServiceRegistryContext : IEquatable<ServiceRegistryContext>
	{
		public TypeIndex typeIndex;
		public TypeIndex contextTypeIndex;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ServiceRegistryContext Create<T>() where T: unmanaged, IIndexedType
		{
			return new ServiceRegistryContext
			{
				typeIndex = TypeIndex<T>.typeIndex,
				contextTypeIndex = TypeIndex.Empty,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ServiceRegistryContext Create<T, TContext>() where T: unmanaged, IIndexedType where TContext: IIndexedType
		{
			return new ServiceRegistryContext
			{
				typeIndex = TypeIndex<T>.typeIndex,
				contextTypeIndex = TypeIndex<TContext>.typeIndex,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(ServiceRegistryContext other)
		{
			return other.typeIndex == typeIndex && other.contextTypeIndex == contextTypeIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return typeIndex.index ^ contextTypeIndex.index;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ServiceRegistryContext(TypeIndex typeIndex)
		{
			return new ServiceRegistryContext { typeIndex = typeIndex, contextTypeIndex = -1, };
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(MemPtr ptr)
		{
			AllocatorManager.CurrentAllocatorPtr->serviceRegistry.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(Ptr ptr)
		{
			AllocatorManager.CurrentAllocatorPtr->serviceRegistry.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(IndexedPtr indexedPtr)
		{
			AllocatorManager.CurrentAllocatorPtr->serviceRegistry.RegisterService(this, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService()
		{
			AllocatorManager.CurrentAllocatorPtr->serviceRegistry.RemoveService(this);
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
			allocator->serviceRegistry.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(Allocator* allocator, Ptr ptr)
		{
			allocator->serviceRegistry.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(Allocator* allocator, Ptr<T> ptr) where T: unmanaged
		{
			allocator->serviceRegistry.RegisterService(this, (Ptr)ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(Allocator* allocator, IndexedPtr indexedPtr)
		{
			allocator->serviceRegistry.RegisterService(this, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService(Allocator* allocator)
		{
			allocator->serviceRegistry.RemoveService(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(Allocator* allocator) where T: unmanaged
		{
			return ref allocator->serviceRegistry.GetService<T>(allocator, this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(Allocator* allocator, out bool exist) where T: unmanaged
		{
			return ref allocator->serviceRegistry.GetService<T>(allocator, this, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>(Allocator* allocator) where T: unmanaged
		{
			return allocator->serviceRegistry.GetServiceIndexedPtr<T>(allocator, this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Ptr<T> GetServiceCachedPtr<T>(Allocator* allocator) where T: unmanaged
		{
			return allocator->serviceRegistry.GetServiceCachedPtr<T>(allocator, this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetServicePtr<T>(Allocator* allocator) where T: unmanaged
		{
			return allocator->serviceRegistry.GetServicePtr<T>(allocator, this);
		}
	}
}
