using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Submodules.Sapientia.Memory;

namespace Submodules.Sapientia.Safety
{
	internal class DisposeSentinelManager
	{
		// Формирует `Id` для каждого типа, к которому происходит обращение
		// Разделение на `Id` нужно для уменьшения потенциального рейс кондишена
		// И унификации `SafetyHandle` без привязки к типу, чтобы уменьшить риск ошибки
		private static class DisposeSentinelTypeId<T>
		{
#if UNITY_5_3_OR_NEWER
			private static readonly Unity.Burst.SharedStatic<int> _typeId;
#else
			private static int _typeId;
#endif
			public static ref int TypeId
			{
#if UNITY_5_3_OR_NEWER
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => ref _typeId.Data;
#else
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => ref _typeId;
#endif
			}

			static DisposeSentinelTypeId()
			{
#if UNITY_5_3_OR_NEWER
				_typeId = Unity.Burst.SharedStatic<int>.GetOrCreate<T, DisposeSentinelManager>();
#endif
				TypeId = -1;
			}
		}

#if UNITY_5_3_OR_NEWER
		private static readonly Unity.Burst.SharedStatic<UnsafeIndexAllocSparseSet<DisposeSentinelAllocator>> _disposeSentinelAllocators = Unity.Burst.SharedStatic<UnsafeIndexAllocSparseSet<DisposeSentinelAllocator>>.GetOrCreate<DisposeSentinelAllocator>();
#else
		private static UnsafeIndexAllocSparseSet<DisposeSentinelAllocator> _disposeSentinelAllocators;
#endif

		// Нельзя инициализировать `_safetyHandleAllocators` из конструктора, `BURST` не поддерживает
		private static ref UnsafeIndexAllocSparseSet<DisposeSentinelAllocator> GetDisposeSentinelAllocators()
		{
#if UNITY_5_3_OR_NEWER
			ref var result = ref _disposeSentinelAllocators.Data;
#else
			ref var result = ref _disposeSentinelAllocators;
#endif
			if (!result.IsCreated)
				result = new UnsafeIndexAllocSparseSet<DisposeSentinelAllocator>(MemoryManager.NoTrackMemoryId, 64);

			return ref result;
		}

		private static int GetTypeId<T>()
		{
			if (DisposeSentinelTypeId<T>.TypeId < 0)
				DisposeSentinelTypeId<T>.TypeId = AllocateTypeId();
			return DisposeSentinelTypeId<T>.TypeId;
		}

		/// <summary>
		/// Для каждого типа аллоцируем свой `DisposeSentinelAllocator`
		/// </summary>
		private static int AllocateTypeId()
		{
			ref var disposeSentinelAllocators = ref GetDisposeSentinelAllocators();

			var typeId = disposeSentinelAllocators.AllocateId();
			disposeSentinelAllocators.Get(typeId) = DisposeSentinelAllocator.Create(typeId);

			return typeId;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static DisposeSentinel AllocateDisposeSentinel<T>()
		{
			var typeId = GetTypeId<T>();
			ref var safetyHandleAllocator = ref GetDisposeSentinelAllocators().Get(typeId);
			return safetyHandleAllocator.AllocateDisposeSentinel();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CheckDisposeSentinel(DisposeSentinel handle)
		{
			ref var safetyHandleAllocator = ref GetDisposeSentinelAllocators().Get(handle.typeId);
			return safetyHandleAllocator.CheckDisposeSentinel(handle);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ReleaseDisposeSentinel(DisposeSentinel handle)
		{
			ref var safetyHandleAllocator = ref GetDisposeSentinelAllocators().Get(handle.typeId);
			safetyHandleAllocator.ReleaseDisposeSentinel(handle);
		}
	}
}
