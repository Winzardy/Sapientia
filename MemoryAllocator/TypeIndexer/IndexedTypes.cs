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
		private static System.Collections.Generic.Dictionary<Type, TypeId> _typeToIndex = new();
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
		/// Количество дочерних типов для каждого контекста (интерфейса). Индексируется по TypeId.id контекста.
		/// </summary>
		private static int[] _contextCounts = Array.Empty<int>();
		/// <summary>
		/// (Type context, Type type) → последовательный 0-based индекс внутри контекста.
		/// </summary>
		private static System.Collections.Generic.Dictionary<(Type, Type), int> _contextTypeIndices = new();
		/// <summary>
		/// Все дочерние TypeId для каждого контекста, упорядоченные по TypeIndex<TContext>.
		/// </summary>
		private static TypeId[][] _contextChildren = Array.Empty<TypeId[]>();

		public static void Initialize(System.Collections.Generic.Dictionary<Type, TypeId> typeToIndex,
			Type[] types,
			Delegate[] delegates, System.Collections.Generic.Dictionary<(TypeId, ProxyId), DelegateIndex> typeToDelegateIndex)
		{
			_typeToIndex = typeToIndex;
			_types = types;
			_delegates = delegates;
			_typeToDelegateIndex = typeToDelegateIndex;
		}

		public static void InitializeContextIndices(
			int[] contextCounts,
			System.Collections.Generic.Dictionary<(Type, Type), int> contextTypeIndices,
			TypeId[][] contextChildren)
		{
			_contextCounts = contextCounts;
			_contextTypeIndices = contextTypeIndices;
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
			result.FirstDelegateIndex = _typeToDelegateIndex[(TypeId<T>.typeId, result.ProxyId)];
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Delegate GetDelegate(int delegateIndex)
		{
			return _delegates[delegateIndex];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetIndex(Type type, out TypeId typeId)
		{
			typeId = -1;
			return _typeToIndex.TryGetValue(type, out typeId);
		}

#if UNITY_5_3_OR_NEWER
		[Unity.Burst.BurstDiscard]
#endif
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetTypeIndex(Type type, out TypeId typeId)
		{
			typeId = _typeToIndex[type];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TypeId GetTypeIndex(Type type)
		{
			return _typeToIndex[type];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetIndexOrDefault(Type type)
		{
			return _typeToIndex.GetValueOrDefault(type, -1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Type GetType(TypeId typeId)
		{
			return _types[typeId.id];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetContextCount(Type contextType)
		{
			if (!_typeToIndex.TryGetValue(contextType, out var typeId))
			{
				return 0;
			}
			var index = typeId.id;
			if (index < 0 || index >= _contextCounts.Length)
			{
				return 0;
			}
			return _contextCounts[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetContextTypeIndex(Type contextType, Type type, out int index)
		{
			if (!_contextTypeIndices.TryGetValue((contextType, type), out index))
			{
				index = -1;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TypeId[] GetContextChildren(Type contextType)
		{
			if (!_typeToIndex.TryGetValue(contextType, out var typeId))
			{
				return Array.Empty<TypeId>();
			}
			var index = typeId.id;
			if (index < 0 || index >= _contextChildren.Length)
			{
				return Array.Empty<TypeId>();
			}
			return _contextChildren[index] ?? Array.Empty<TypeId>();
		}
	}
}
