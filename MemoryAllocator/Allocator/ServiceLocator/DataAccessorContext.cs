using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct DataAccessorContext : IEquatable<DataAccessorContext>
	{
		public TypeIndex typeIndex;
		public TypeIndex contextTypeIndex;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static DataAccessorContext Create<T>() where T: unmanaged, IIndexedType
		{
			return new DataAccessorContext
			{
				typeIndex = TypeIndex<T>.typeIndex,
				contextTypeIndex = TypeIndex.Empty,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static DataAccessorContext Create<T, TContext>() where T: unmanaged, IIndexedType where TContext: IIndexedType
		{
			return new DataAccessorContext
			{
				typeIndex = TypeIndex<T>.typeIndex,
				contextTypeIndex = TypeIndex<TContext>.typeIndex,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(DataAccessorContext other)
		{
			return other.typeIndex == typeIndex && other.contextTypeIndex == contextTypeIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return typeIndex.index ^ contextTypeIndex.index;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator DataAccessorContext(TypeIndex typeIndex)
		{
			return new DataAccessorContext { typeIndex = typeIndex, contextTypeIndex = -1, };
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(MemPtr ptr)
		{
			AllocatorManager.CurrentAllocator.dataAccessor.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(Ptr ptr)
		{
			AllocatorManager.CurrentAllocator.dataAccessor.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(IndexedPtr indexedPtr)
		{
			AllocatorManager.CurrentAllocator.dataAccessor.RegisterService(this, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService()
		{
			AllocatorManager.CurrentAllocator.dataAccessor.RemoveService(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>() where T: unmanaged
		{
			return ref GetService<T>(AllocatorManager.CurrentAllocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(out bool exist) where T: unmanaged
		{
			return ref GetService<T>(AllocatorManager.CurrentAllocator, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>() where T: unmanaged
		{
			return GetServiceIndexedPtr<T>(AllocatorManager.CurrentAllocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Ptr<T> GetServiceCachedPtr<T>() where T: unmanaged
		{
			return GetServiceCachedPtr<T>(AllocatorManager.CurrentAllocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetServicePtr<T>() where T: unmanaged
		{
			return GetServicePtr<T>(AllocatorManager.CurrentAllocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(Allocator allocator, MemPtr ptr)
		{
			allocator.dataAccessor.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(Allocator allocator, Ptr ptr)
		{
			allocator.dataAccessor.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(Allocator allocator, Ptr<T> ptr) where T: unmanaged
		{
			allocator.dataAccessor.RegisterService(this, (Ptr)ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(Allocator allocator, IndexedPtr indexedPtr)
		{
			allocator.dataAccessor.RegisterService(this, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService(Allocator allocator)
		{
			allocator.dataAccessor.RemoveService(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(Allocator allocator) where T: unmanaged
		{
			return ref allocator.dataAccessor.GetService<T>(allocator, this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(Allocator allocator, out bool exist) where T: unmanaged
		{
			return ref allocator.dataAccessor.GetService<T>(allocator, this, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>(Allocator allocator) where T: unmanaged
		{
			return allocator.dataAccessor.GetServiceIndexedPtr<T>(allocator, this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Ptr<T> GetServiceCachedPtr<T>(Allocator allocator) where T: unmanaged
		{
			return allocator.dataAccessor.GetServiceCachedPtr<T>(allocator, this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetServicePtr<T>(Allocator allocator) where T: unmanaged
		{
			return allocator.dataAccessor.GetServicePtr<T>(allocator, this);
		}
	}
}
