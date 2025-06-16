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

		public void ProxyDispose(void* executorPtr, World world) {}
	}

	[IndexedType]
	public interface IIndexedType {}

	public interface IInterfaceProxyType : IIndexedType
	{
		public virtual void ProxyDispose(World world) {}
	}

	public static class IndexedTypes
	{
		private static System.Collections.Generic.Dictionary<Type, TypeIndex> _typeToIndex = new();
		private static Type[] _types = Array.Empty<Type>();

		/// <summary>
		/// При инициализации сюда записываются все методы для всех unmanaged наследников интерфейсов с атрибутом [InterfaceProxyAttribute].
		/// Причём, методы создаются пачками для каждого конкретного наследника интерфейса.
		/// Поэтому зная индекс первого метода, можно получить все остальные методы для конкретного наследника интерфейса.
		/// </summary>
		private static Delegate[] _delegates;
		/// <summary>
		/// Получаем индекс первого метода для интерфейса (ProxyId) и его наследника (TypeIndex).
		/// </summary>
		private static System.Collections.Generic.Dictionary<(TypeIndex, ProxyId), DelegateIndex> _typeToDelegateIndex;

		public static void Initialize(System.Collections.Generic.Dictionary<Type, TypeIndex> typeToIndex,
			Type[] types,
			Delegate[] delegates, System.Collections.Generic.Dictionary<(TypeIndex, ProxyId), DelegateIndex> typeToDelegateIndex)
		{
			_typeToIndex = typeToIndex;
			_types = types;
			_delegates = delegates;
			_typeToDelegateIndex = typeToDelegateIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TProxy GetProxy<TProxy>(TypeIndex executorType) where TProxy: unmanaged, IProxy
		{
			var result = default(TProxy);
			result.FirstDelegateIndex = _typeToDelegateIndex[(executorType, result.ProxyId)];
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TProxy GetProxy<T, TProxy>() where TProxy: unmanaged, IProxy
		{
			var result = default(TProxy);
			result.FirstDelegateIndex = _typeToDelegateIndex[(TypeIndex<T>.typeIndex, result.ProxyId)];
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Delegate GetDelegate(int delegateIndex)
		{
			return _delegates[delegateIndex];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetIndex(Type type, out TypeIndex index)
		{
			index = -1;
			return _typeToIndex.TryGetValue(type, out index);
		}

#if UNITY_5_3_OR_NEWER
		[Unity.Burst.BurstDiscard]
#endif
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetTypeIndex(Type type, out TypeIndex index)
		{
			index = _typeToIndex[type];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TypeIndex GetTypeIndex(Type type)
		{
			return _typeToIndex[type];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetIndexOrDefault(Type type)
		{
			return _typeToIndex.GetValueOrDefault(type, -1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Type GetType(TypeIndex typeIndex)
		{
			return _types[typeIndex.index];
		}
	}
}
