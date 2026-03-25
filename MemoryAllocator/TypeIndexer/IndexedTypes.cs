using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.MemoryAllocator;

namespace Sapientia.TypeIndexer
{
	public unsafe interface IProxy
	{
		ProxyId ProxyId { get; }

		DelegateIndex FirstDelegateIndex { get; set; }

		public void ProxyDispose(void* executorPtr, WorldState worldState) {}
	}

	[IndexedType]
	public interface IIndexedType {}

	public interface IInterfaceProxyType : IIndexedType
	{
		public virtual void ProxyDispose(WorldState worldState) {}
	}

	public static class IndexedTypes
	{
		private static System.Collections.Generic.Dictionary<Type, TypeId> _typeToTypeId = new();
		private static Type[] _types = Array.Empty<Type>();

		/// <summary>
		/// При инициализации сюда записываются все методы для всех unmanaged наследников интерфейсов с атрибутом [InterfaceProxyAttribute].
		/// Причём, методы создаются пачками для каждого конкретного наследника интерфейса.
		/// Поэтому зная индекс первого метода, можно получить все остальные методы для конкретного наследника интерфейса.
		/// </summary>
		private static Delegate[] _delegates;
		/// <summary>
		/// Получаем индекс первого метода для интерфейса (ProxyId) и его наследника (TypeId).
		/// </summary>
		private static System.Collections.Generic.Dictionary<(TypeId, ProxyId), DelegateIndex> _typeToDelegateIndex;

		/// <summary>
		/// Количество дочерних типов для каждого контекста (интерфейса). Индексируется по (int)TypeId контекста.
		/// </summary>
		private static int[] _contextCounts = Array.Empty<int>();
		/// <summary>
		/// (Type context, Type type) → последовательный 0-based TypeId внутри контекста.
		/// </summary>
		private static System.Collections.Generic.Dictionary<(Type context, Type type), TypeId> _contextTypeIds = new();
		/// <summary>
		/// Все дочерние TypeId для каждого контекста, упорядоченные по TypeId<TContext>.
		/// </summary>
		private static TypeId[][] _contextChildren = Array.Empty<TypeId[]>();

		public static void Initialize(System.Collections.Generic.Dictionary<Type, TypeId> typeToTypeId,
			Type[] types,
			Delegate[] delegates, System.Collections.Generic.Dictionary<(TypeId, ProxyId), DelegateIndex> typeToDelegateIndex)
		{
			_typeToTypeId = typeToTypeId;
			_types = types;
			_delegates = delegates;
			_typeToDelegateIndex = typeToDelegateIndex;
		}

		public static void InitializeContextTypeIds(
			int[] contextCounts,
			System.Collections.Generic.Dictionary<(Type context, Type type), TypeId> contextTypeIds,
			TypeId[][] contextChildren)
		{
			_contextCounts = contextCounts;
			_contextTypeIds = contextTypeIds;
			_contextChildren = contextChildren;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TProxy GetProxy<TProxy>(TypeId executorType) where TProxy: unmanaged, IProxy
		{
			var result = default(TProxy);
			result.FirstDelegateIndex = _typeToDelegateIndex[(executorType, result.ProxyId)];
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TProxy GetProxy<T, TProxy>() where TProxy: unmanaged, IProxy
		{
			var result = default(TProxy);
			result.FirstDelegateIndex = _typeToDelegateIndex[(TypeIdOf<T>.typeId, result.ProxyId)];
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Delegate GetDelegate(int delegateIndex)
		{
			return _delegates[delegateIndex];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetTypeId(Type type, out TypeId typeId)
		{
			typeId = default;
			return _typeToTypeId.TryGetValue(type, out typeId);
		}

#if UNITY_5_3_OR_NEWER
		[Unity.Burst.BurstDiscard]
#endif
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetTypeId(Type type, out TypeId typeId)
		{
			typeId = _typeToTypeId[type];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TypeId GetTypeId(Type type)
		{
			return _typeToTypeId[type];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetTypeIdOrDefault(Type type)
		{
			return _typeToTypeId.GetValueOrDefault(type, -1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Type GetType(TypeId typeId)
		{
			return _types[(int)typeId];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetContextCount(Type contextType)
		{
			if (!_typeToTypeId.TryGetValue(contextType, out var typeId))
			{
				return 0;
			}
			var index = (int)typeId;
			if (index < 0 || index >= _contextCounts.Length)
			{
				return 0;
			}
			return _contextCounts[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetContextTypeId(Type contextType, Type type, out TypeId typeId)
		{
			if (!_contextTypeIds.TryGetValue((contextType, type), out typeId))
			{
				typeId = default;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TypeId[] GetContextChildren(Type contextType)
		{
			if (!_typeToTypeId.TryGetValue(contextType, out var typeId))
			{
				return Array.Empty<TypeId>();
			}
			var index = (int)typeId;
			if (index < 0 || index >= _contextChildren.Length)
			{
				return Array.Empty<TypeId>();
			}
			return _contextChildren[index] ?? Array.Empty<TypeId>();
		}
	}
}
