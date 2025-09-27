using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	public partial struct WorldState
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool TryGetUnmanagedLocalServicePtr<T>(out SafePtr<T> result) where T: unmanaged, IIndexedType
		{
			return GetLocalServiceRegistry().TryGetPtr<T>(out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly SafePtr<T> GetUnmanagedLocalServicePtr<T>() where T: unmanaged, IIndexedType
		{
			return GetLocalServiceRegistry().GetPtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T GetUnmanagedLocalService<T>() where T: unmanaged, IIndexedType
		{
			return ref GetLocalServiceRegistry().Get<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T GetOrCreateUnmanagedLocalService<T>() where T: unmanaged, IIndexedType
		{
			return ref GetLocalServiceRegistry().GetOrCreate<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T GetOrCreateUnmanagedLocalService<T>(out ServiceRegistryContext context) where T: unmanaged, IIndexedType
		{
			return ref GetLocalServiceRegistry().GetOrCreate<T>(out context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly SafePtr<T> GetOrCreateUnmanagedLocalServicePtr<T>() where T: unmanaged, IIndexedType
		{
			return GetLocalServiceRegistry().GetOrCreatePtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly SafePtr<T> GetOrCreateUnmanagedLocalServicePtr<T>(out bool isCreated) where T: unmanaged, IIndexedType
		{
			return GetLocalServiceRegistry().GetOrCreatePtr<T>(out isCreated);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool RemoveUnmanagedLocalService<T>() where T: unmanaged, IIndexedType
		{
			return GetLocalServiceRegistry().Remove<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool RemoveUnmanagedLocalService<T>(out T service) where T: unmanaged, IIndexedType
		{
			return GetLocalServiceRegistry().Remove<T>(out service);
		}
	}
}
