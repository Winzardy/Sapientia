using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sapientia.TypeIndexer
{
	public interface IProxy
	{
		ProxyIndex ProxyIndex { get; }

		DelegateIndex FirstDelegateIndex { get; set; }
	}

	[IndexedType]
	public interface IIndexedType {}

	[InterfaceProxy]
	public interface IInterfaceProxyType {}

	public static unsafe class IndexedTypes
	{
		private static Dictionary<Type, TypeIndex> _typeToIndex = new();
		private static Type[] _indexToType = Array.Empty<Type>();

		/// <summary>
		/// При инициализации сюда записываются все методы для всех unmanaged наследников интерфейсов с атрибутом [InterfaceProxyAttribute].
		/// Причём, методы создаются пачками для каждого конкретного наследника интерфейса.
		/// Поэтому зная индекс первого метода, можно получить все остальные методы для конкретного наследника интерфейса.
		/// </summary>
		private static CompiledMethod[] _delegateIndexToCompiledMethod = Array.Empty<CompiledMethod>();
		/// <summary>
		/// Получаем индекс первого метода для интерфейса (ProxyIndex) и его наследника (TypeIndex).
		/// </summary>
		private static Dictionary<(TypeIndex, ProxyIndex), DelegateIndex> _typeToDelegateIndex = new();

		public static void Initialize(
			Dictionary<Type, TypeIndex> typeToIndex,
			Type[] indexToType,
			CompiledMethod[] delegateIndexToCompiledMethod,
			Dictionary<(TypeIndex, ProxyIndex), DelegateIndex> typeToDelegateIndex)
		{
			_typeToIndex = typeToIndex;
			_indexToType = indexToType;
			_delegateIndexToCompiledMethod = delegateIndexToCompiledMethod;
			_typeToDelegateIndex = typeToDelegateIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TProxy GetProxy<TProxy>(TypeIndex executorType) where TProxy: unmanaged, IProxy
		{
			var result = default(TProxy);
			result.FirstDelegateIndex = _typeToDelegateIndex[(executorType, result.ProxyIndex)];
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TProxy GetProxy<T, TProxy>() where TProxy: unmanaged, IProxy
		{
			var result = default(TProxy);
			result.FirstDelegateIndex = _typeToDelegateIndex[(TypeIndex<T>.typeIndex, result.ProxyIndex)];
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CompiledMethod GetCompiledMethod(DelegateIndex delegateIndex)
		{
			return _delegateIndexToCompiledMethod[delegateIndex];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CompiledMethod GetCompiledMethod(int delegateIndex)
		{
			return _delegateIndexToCompiledMethod[delegateIndex];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetIndex(Type type, out TypeIndex index)
		{
			index = -1;
			return _typeToIndex.TryGetValue(type, out index);
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
			return _indexToType[typeIndex.index];
		}
	}
}
