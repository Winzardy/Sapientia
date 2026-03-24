using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
#if UNITY_5_3_OR_NEWER
	[Unity.Burst.BurstCompile]
#endif
	public struct ServiceRegistryContext : IEquatable<ServiceRegistryContext>
	{
		public TypeId typeId;
		public TypeId contextTypeId;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ServiceRegistryContext Create<T>() where T: unmanaged, IIndexedType
		{
			return new ServiceRegistryContext
			{
				typeId = TypeIdOf<T>.typeId,
				contextTypeId = TypeId.Empty,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ServiceRegistryContext Create<T, TContext>() where T: unmanaged, IIndexedType where TContext: IIndexedType
		{
			return new ServiceRegistryContext
			{
				typeId = TypeIdOf<T>.typeId,
				contextTypeId = TypeIdOf<TContext>.typeId,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(ServiceRegistryContext other)
		{
			return other.typeId == typeId && other.contextTypeId == contextTypeId;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return (int)typeId ^ (int)contextTypeId;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ServiceRegistryContext(TypeId typeId)
		{
			return new ServiceRegistryContext { typeId = typeId, contextTypeId = -1, };
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(MemPtr ptr)
		{
			WorldManager.CurrentWorldState.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(CachedPtr ptr)
		{
			WorldManager.CurrentWorldState.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(IndexedPtr indexedPtr)
		{
			WorldManager.CurrentWorldState.RegisterService(this, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService()
		{
			WorldManager.CurrentWorldState.RemoveService(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>() where T: unmanaged
		{
			return ref GetService<T>(WorldManager.CurrentWorldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(out bool exist) where T: unmanaged
		{
			return ref TryGetService<T>(WorldManager.CurrentWorldState, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>() where T: unmanaged
		{
			return GetServiceIndexedPtr<T>(WorldManager.CurrentWorldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CachedPtr<T> GetServiceCachedPtr<T>() where T: unmanaged
		{
			return GetServiceCachedPtr<T>(WorldManager.CurrentWorldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetServicePtr<T>() where T: unmanaged
		{
			return GetServicePtr<T>(WorldManager.CurrentWorldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(WorldState worldState, MemPtr ptr)
		{
			worldState.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(WorldState worldState, CachedPtr ptr)
		{
			worldState.RegisterService(this, ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService<T>(WorldState worldState, CachedPtr<T> ptr) where T: unmanaged
		{
			worldState.RegisterService(this, (CachedPtr)ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterService(WorldState worldState, IndexedPtr indexedPtr)
		{
			worldState.RegisterService(this, indexedPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveService(WorldState worldState)
		{
			worldState.RemoveService(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetService<T>(WorldState worldState) where T: unmanaged
		{
			return ref worldState.GetService<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TryGetService<T>(WorldState worldState, out bool exist) where T: unmanaged
		{
			return ref worldState.TryGetService<T>(this, out exist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IndexedPtr GetServiceIndexedPtr<T>(WorldState worldState) where T: unmanaged
		{
			return worldState.GetServiceIndexedPtr<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CachedPtr<T> GetServiceCachedPtr<T>(WorldState worldState) where T: unmanaged
		{
			return worldState.GetServiceCachedPtr<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetServicePtr<T>(WorldState worldState) where T: unmanaged
		{
			return worldState.GetServicePtr<T>(this);
		}
	}
}
