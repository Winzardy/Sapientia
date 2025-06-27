using System.Runtime.CompilerServices;
using Sapientia.Collections;

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
				_typeId = Unity.Burst.SharedStatic<int>.GetOrCreate<T>();
#endif
				TypeId = -1;
			}
		}

#if UNITY_5_3_OR_NEWER
		private static readonly Unity.Burst.SharedStatic<UnsafeIndexAllocSparseSet<DisposeSentinelAllocator>> _safetyHandleAllocators = Unity.Burst.SharedStatic<UnsafeIndexAllocSparseSet<DisposeSentinelAllocator>>.GetOrCreate<DisposeSentinelAllocator>();
#else
		private static UnsafeIndexAllocSparseSet<DisposeSentinelAllocator> _safetyHandleAllocators;
#endif

		// Нельзя инициализировать `_safetyHandleAllocators` из конструктора, `BURST` не поддерживает
		private static ref UnsafeIndexAllocSparseSet<DisposeSentinelAllocator> GetSafetyHandleAllocators()
		{
#if UNITY_5_3_OR_NEWER
			ref var result = ref _safetyHandleAllocators.Data;
#else
			ref var result = ref _safetyHandleAllocators;
#endif
			if (!result.IsCreated)
				result = new UnsafeIndexAllocSparseSet<DisposeSentinelAllocator>(64);

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
			var typeId = _safetyHandleAllocators.Data.AllocateId();
			GetSafetyHandleAllocators().Get(typeId) = DisposeSentinelAllocator.Create(typeId);

			return typeId;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static DisposeSentinel AllocateSafetyHandle<T>()
		{
			var typeId = GetTypeId<T>();
			ref var safetyHandleAllocator = ref GetSafetyHandleAllocators().Get(typeId);
			return safetyHandleAllocator.AllocateSafetyHandle();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CheckSafetyHandle(DisposeSentinel handle)
		{
			ref var safetyHandleAllocator = ref GetSafetyHandleAllocators().Get(handle.typeId);
			return safetyHandleAllocator.CheckSafetyHandle(handle);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ReleaseSafetyHandle(DisposeSentinel handle)
		{
			ref var safetyHandleAllocator = ref GetSafetyHandleAllocators().Get(handle.typeId);
			safetyHandleAllocator.ReleaseSafetyHandle(handle);
		}
	}
}
