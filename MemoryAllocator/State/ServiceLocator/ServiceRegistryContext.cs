using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public struct ServiceRegistryContext : IEquatable<ServiceRegistryContext>
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
			WorldManager.CurrentWorld.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(CachedPtr ptr)
		{
			WorldManager.CurrentWorld.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(IndexedPtr indexedPtr)
		{
			WorldManager.CurrentWorld.RegisterService(this, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService()
		{
			WorldManager.CurrentWorld.RemoveService(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>() where T: unmanaged
		{
			return ref GetService<T>(WorldManager.CurrentWorld);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(out bool exist) where T: unmanaged
		{
			return ref TryGetService<T>(WorldManager.CurrentWorld, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>() where T: unmanaged
		{
			return GetServiceIndexedPtr<T>(WorldManager.CurrentWorld);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CachedPtr<T> GetServiceCachedPtr<T>() where T: unmanaged
		{
			return GetServiceCachedPtr<T>(WorldManager.CurrentWorld);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetServicePtr<T>() where T: unmanaged
		{
			return GetServicePtr<T>(WorldManager.CurrentWorld);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(World world, MemPtr ptr)
		{
			world.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(World world, CachedPtr ptr)
		{
			world.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(World world, CachedPtr<T> ptr) where T: unmanaged
		{
			world.RegisterService(this, (CachedPtr)ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(World world, IndexedPtr indexedPtr)
		{
			world.RegisterService(this, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService(World world)
		{
			world.RemoveService(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(World world) where T: unmanaged
		{
			return ref world.GetService<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryGetService<T>(World world, out bool exist) where T: unmanaged
		{
			return ref world.TryGetService<T>(this, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>(World world) where T: unmanaged
		{
			return world.GetServiceIndexedPtr<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CachedPtr<T> GetServiceCachedPtr<T>(World world) where T: unmanaged
		{
			return world.GetServiceCachedPtr<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetServicePtr<T>(World world) where T: unmanaged
		{
			return world.GetServicePtr<T>(this);
		}
	}
}
