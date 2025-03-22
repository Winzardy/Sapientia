using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.MemoryAllocator;

namespace Sapientia.TypeIndexer
{
	public unsafe interface IProxy
	{
		ProxyIndex ProxyIndex { get; }

		DelegateIndex FirstDelegateIndex { get; set; }

		public void ProxyDispose(void* executorPtr, Allocator* allocator) {}
	}

	[IndexedType]
	public interface IIndexedType {}

	public unsafe interface IInterfaceProxyType : IIndexedType
	{
		public virtual void ProxyDispose(Allocator* allocator) {}
	}

	public static unsafe class IndexedTypes
	{
		private static System.Collections.Generic.Dictionary<Type, TypeIndex> _typeToIndex = new();
		private static Type[] _indexToType = Array.Empty<Type>();

		/// <summary>
		/// При инициализации сюда записываются все методы для всех unmanaged наследников интерфейсов с атрибутом [InterfaceProxyAttribute].
		/// Причём, методы создаются пачками для каждого конкретного наследника интерфейса.
		/// Поэтому зная индекс первого метода, можно получить все остальные методы для конкретного наследника интерфейса.
		/// </summary>
		private static Delegate[] _delegateIndexToCompiledMethod;
		/// <summary>
		/// Получаем индекс первого метода для интерфейса (ProxyIndex) и его наследника (TypeIndex).
		/// </summary>
		private static System.Collections.Generic.Dictionary<(TypeIndex, ProxyIndex), DelegateIndex> _typeToDelegateIndex;

		public static void Initialize(System.Collections.Generic.Dictionary<Type, TypeIndex> typeToIndex,
			Type[] indexToType,
			Delegate[] delegateIndexToCompiledMethod, System.Collections.Generic.Dictionary<(TypeIndex, ProxyIndex), DelegateIndex> typeToDelegateIndex)
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
		public static Delegate GetDelegate(int delegateIndex)
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
