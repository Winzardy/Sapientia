using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public partial struct WorldState
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdateSafePtr<T>(MemPtr memPtr, ref SafePtr<T> safePtr, ref ushort version)
			where T : unmanaged
		{
			ref var worldStateData = ref WorldStateData;
			if (worldStateData.version == version)
				return;

			safePtr = worldStateData.allocator.GetSafePtr<T>(memPtr);
			version = worldStateData.version;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdateSafePtr(MemPtr memPtr, ref SafePtr safePtr, ref ushort version)
		{
			ref var worldStateData = ref WorldStateData;
			if (worldStateData.version == version)
				return;

			safePtr = worldStateData.allocator.GetSafePtr(memPtr);
			version = worldStateData.version;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetSafePtr(MemPtr memPtr)
		{
			return GetAllocator().GetSafePtr(memPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetSafePtr<T>(MemPtr memPtr) where T: unmanaged
		{
			return GetAllocator().GetSafePtr<T>(memPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetValue<T>(MemPtr memPtr) where T: unmanaged
		{
			return ref GetAllocator().GetRef<T>(memPtr);
		}

		/// <summary>
		/// Мы должны быть уверены, что результат не будет использован!
		/// Иначе может повредиться память стейта.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetZeroRef<T>() where T: unmanaged
		{
			return ref GetAllocator().GetZeroRef<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr CopyPtrTo(WorldState dstWorldState, in MemPtr srsPtr)
		{
			return GetAllocator().CopyPtrTo(ref dstWorldState.GetAllocator(), srsPtr);
		}
	}
}
