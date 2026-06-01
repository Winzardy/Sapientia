#nullable disable
using System;
using System.Collections.Concurrent;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sapientia.Pooling;

namespace Sapientia.Extensions
{
	public class CustomSerializationBinder : DefaultSerializationBinder
	{
		private static readonly ConcurrentDictionary<string, string> _rawToMatch = new();
		private static readonly ConcurrentDictionary<string, Assembly> _assemblyCache = new();
		private static readonly ConcurrentDictionary<Assembly, string> _assemblyNameCache = new();
		private static readonly ConcurrentDictionary<(string AssemblyName, string TypeName), Type> _typeCache = new();

		public override Type BindToType(string assemblyName, string typeName)
		{
			var key = (assemblyName, typeName);
			if (_typeCache.TryGetValue(key, out var cachedType))
				return cachedType;

			var type = ResolveType(assemblyName, typeName);
			_typeCache[key] = type;
			return type;
		}

		public override void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
		{
			base.BindToName(serializedType, out var rawAssemblyName, out typeName);
			var assembly = serializedType.Assembly;
			var name = GetAssemblyName(assembly);
			CacheAssemblyName(rawAssemblyName, assembly);
			assemblyName = name;
		}

		private Type ResolveType(string assemblyName, string typeName)
		{
			if (_rawToMatch.TryGetValue(assemblyName, out var correctAssemblyName) &&
				TryBindToType(correctAssemblyName, typeName, out var matchedType))
			{
				_typeCache.TryAdd((correctAssemblyName, typeName), matchedType);
				return matchedType;
			}

			try
			{
				var type = base.BindToType(assemblyName, typeName);
				CacheAssemblyName(assemblyName, type.Assembly);
				return type;
			}
			catch (Exception exception)
			{
				if (!TryGetType(assemblyName, typeName, out var type) &&
					!TryResolveGenericType(assemblyName, typeName, out type) &&
					!TryFindType(typeName, out type))
				{
					throw new JsonSerializationException(
						$"Unable to resolve type '{typeName}' from any loaded assembly (original assembly: {assemblyName})",
						exception);
				}

				CacheAssemblyName(assemblyName, type.Assembly);
				_typeCache.TryAdd((GetAssemblyName(type.Assembly), typeName), type);
				return type;
			}
		}

		private bool TryBindToType(string assemblyName, string typeName, out Type type)
		{
			try
			{
				type = base.BindToType(assemblyName, typeName);
				return true;
			}
			catch
			{
				type = null!;
				return false;
			}
		}

		private static bool TryGetType(string assemblyName, string typeName, out Type type)
		{
			type = Type.GetType(
				$"{typeName}, {assemblyName}",
				ResolveAssembly,
				ResolveTypeFromAssembly,
				throwOnError: false)!;

			return type != null;
		}

		private bool TryResolveGenericType(string assemblyName, string typeName, out Type type)
		{
			type = null!;

			var argumentsStartIndex = typeName.IndexOf("[[", StringComparison.Ordinal);
			if (argumentsStartIndex < 0)
				return false;

			var genericTypeDefinitionName = typeName[..argumentsStartIndex];
			if (!TryResolveSimpleType(assemblyName, genericTypeDefinitionName, out var genericTypeDefinition))
				return false;

			var argumentsText = typeName[argumentsStartIndex..];
			if (!TryParseGenericArguments(argumentsText, out var argumentKeys))
				return false;

			var arguments = new Type[argumentKeys.Length];
			for (var i = 0; i < argumentKeys.Length; i++)
			{
				if (!TrySplitAssemblyQualifiedTypeName(argumentKeys[i], out var argumentTypeName, out var argumentAssemblyName))
					return false;

				arguments[i] = BindToType(argumentAssemblyName, argumentTypeName);
			}

			try
			{
				type = genericTypeDefinition.MakeGenericType(arguments);
				return true;
			}
			catch
			{
				type = null!;
				return false;
			}
		}

		private bool TryResolveSimpleType(string assemblyName, string typeName, out Type type)
		{
			if (_rawToMatch.TryGetValue(assemblyName, out var correctAssemblyName) &&
				TryBindToType(correctAssemblyName, typeName, out type))
			{
				_typeCache.TryAdd((correctAssemblyName, typeName), type);
				return true;
			}

			if (TryBindToType(assemblyName, typeName, out type))
			{
				CacheAssemblyName(assemblyName, type.Assembly);
				return true;
			}

			if (TryFindAssembly(assemblyName, out var assembly))
			{
				type = assembly.GetType(typeName, false)!;
				if (type != null)
				{
					CacheAssemblyName(assemblyName, assembly);
					_typeCache.TryAdd((GetAssemblyName(assembly), typeName), type);
					return true;
				}
			}

			if (TryFindType(typeName, out type))
			{
				CacheAssemblyName(assemblyName, type.Assembly);
				_typeCache.TryAdd((GetAssemblyName(type.Assembly), typeName), type);
				return true;
			}

			type = null!;
			return false;
		}

		private static bool TryFindType(string typeName, out Type type)
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (assembly.IsDynamic)
					continue;

				type = assembly.GetType(typeName, throwOnError: false)!;
				if (type != null)
					return true;
			}

			type = null!;
			return false;
		}

		private static bool TryParseGenericArguments(string argumentsText, out string[] arguments)
		{
			using (ListPool<string>.Get(out var list))
			{
				if (argumentsText.Length < 4 ||
					argumentsText[0] != '[' ||
					argumentsText[1] != '[' ||
					argumentsText[^1] != ']')
				{
					arguments = null!;
					return false;
				}

				var index = 1;
				var argumentsEndIndex = argumentsText.Length - 1;

				while (index < argumentsEndIndex)
				{
					SkipWhitespace(argumentsText, ref index, argumentsEndIndex);
					if (argumentsText[index] != '[')
					{
						arguments = null!;
						return false;
					}

					index++;
					var startIndex = index;
					var depth = 0;

					while (index < argumentsEndIndex)
					{
						var c = argumentsText[index];
						if (c == '[')
							depth++;
						else if (c == ']')
						{
							if (depth == 0)
								break;

							depth--;
						}

						index++;
					}

					if (index >= argumentsEndIndex)
					{
						arguments = null!;
						return false;
					}

					list.Add(argumentsText.Substring(startIndex, index - startIndex));
					index++;

					SkipWhitespace(argumentsText, ref index, argumentsEndIndex);
					if (index == argumentsEndIndex)
					{
						arguments = list.ToArray();
						return true;
					}

					if (argumentsText[index] != ',')
					{
						arguments = null!;
						return false;
					}

					index++;
					SkipWhitespace(argumentsText, ref index, argumentsEndIndex);
				}

				arguments = list.ToArray();
				return list.Count > 0;
			}
		}

		private static void SkipWhitespace(string value, ref int index, int endIndex)
		{
			while (index < endIndex && char.IsWhiteSpace(value[index]))
				index++;
		}

		private static bool TrySplitAssemblyQualifiedTypeName(string value, out string typeName, out string assemblyName)
		{
			var separatorIndex = IndexOfTopLevelComma(value);
			if (separatorIndex < 0)
			{
				typeName = null!;
				assemblyName = null!;
				return false;
			}

			typeName = value[..separatorIndex].Trim();
			assemblyName = value[(separatorIndex + 1)..].Trim();
			return typeName.Length > 0 && assemblyName.Length > 0;
		}

		private static int IndexOfTopLevelComma(string value)
		{
			var depth = 0;
			for (var i = 0; i < value.Length; i++)
			{
				var c = value[i];
				if (c == '[')
					depth++;
				else if (c == ']')
					depth--;
				else if (c == ',' && depth == 0)
					return i;
			}

			return -1;
		}

		private static Assembly? ResolveAssembly(AssemblyName assemblyName)
		{
			var fullName = assemblyName.FullName;
			if (!fullName.IsNullOrEmpty() &&
				_rawToMatch.TryGetValue(fullName, out var correctAssemblyName) &&
				TryFindAssembly(correctAssemblyName, out var assembly))
				return assembly;

			var name = assemblyName.Name;
			if (name != null &&
				_rawToMatch.TryGetValue(name, out correctAssemblyName) &&
				TryFindAssembly(correctAssemblyName, out assembly))
				return assembly;

			if (name != null && TryFindAssembly(name, out assembly))
				return assembly;

			return !fullName.IsNullOrEmpty()
				&& TryFindAssembly(fullName, out assembly)
					? assembly
					: null;
		}

		private static Type? ResolveTypeFromAssembly(Assembly? assembly, string typeName, bool ignoreCase)
		{
			if (assembly != null)
				return assembly.GetType(typeName, false, ignoreCase);

			return TryFindType(typeName, out var type) ? type : null;
		}

		private static bool TryFindAssembly(string assemblyName, out Assembly assembly)
		{
			if (_assemblyCache.TryGetValue(assemblyName, out assembly))
				return true;

			var simpleAssemblyName = GetSimpleAssemblyName(assemblyName);
			if (simpleAssemblyName == "mscorlib")
			{
				assembly = typeof(object).Assembly;
				CacheAssembly(assemblyName, assembly);
				return true;
			}

			foreach (var candidate in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (candidate.IsDynamic)
					continue;

				var candidateName = GetAssemblyName(candidate);
				var candidateFullName = candidate.FullName;
				if (assemblyName != candidateName &&
					assemblyName != candidateFullName &&
					simpleAssemblyName != candidateName)
					continue;

				assembly = candidate;
				CacheAssembly(assemblyName, candidate);
				return true;
			}

			try
			{
				assembly = Assembly.Load(new AssemblyName(assemblyName));
				CacheAssembly(assemblyName, assembly);
				return true;
			}
			catch
			{
				// fail...
			}

			assembly = null!;
			return false;
		}

		private static void CacheAssemblyName(string? rawAssemblyName, Assembly assembly)
		{
			if (rawAssemblyName == null)
				return;

			var name = GetAssemblyName(assembly);
			_rawToMatch[rawAssemblyName] = name;
			CacheAssembly(rawAssemblyName, assembly);
		}

		private static void CacheAssembly(string assemblyName, Assembly assembly)
		{
			_assemblyCache.TryAdd(assemblyName, assembly);
			_assemblyCache.TryAdd(GetSimpleAssemblyName(assemblyName), assembly);
			_assemblyCache.TryAdd(GetAssemblyName(assembly), assembly);
			var fullName = assembly.FullName;
			if (fullName != null)
				_assemblyCache.TryAdd(fullName, assembly);
		}

		private static string GetSimpleAssemblyName(string assemblyName)
		{
			try
			{
				return new AssemblyName(assemblyName).Name ?? assemblyName;
			}
			catch
			{
				var index = assemblyName.IndexOf(',');
				return index < 0 ? assemblyName : assemblyName[..index].Trim();
			}
		}

		private static string GetAssemblyName(Assembly assembly) =>
			_assemblyNameCache.GetOrAdd(assembly, x => x.GetName().Name!);
	}
}
