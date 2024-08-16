using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sapientia.TypeIndexer
{
	public interface IIndexedTypesProvider
	{
		public Dictionary<Type, int> TypeToIndex { get; }
		public Type[] IndexToType { get; }
	}

	public static class IndexedTypes
	{
		private static Dictionary<Type, int> _typeToIndex = new();
		private static Type[] _indexToType = Array.Empty<Type>();

		public static void SetupProvider(IIndexedTypesProvider provider)
		{
			_typeToIndex = provider.TypeToIndex;
			_indexToType = provider.IndexToType;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetIndex(Type type, out int index)
		{
			index = -1;
			return _typeToIndex.TryGetValue(type, out index);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetIndex(Type type)
		{
			return _typeToIndex[type];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetIndexOrDefault(Type type)
		{
			return _typeToIndex.GetValueOrDefault(type, -1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Type GetType(int index)
		{
			return _indexToType[index];
		}
	}
}
