using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Sapientia.Extensions.Reflection;

namespace Sapientia.TypeIndexer
{
	public static class TypeIndexerGenerator
	{
		private const string IndexedTypesInitializer = "IndexedTypesInitializer";

		public static void GenerateTypeIndexes(string folderPath)
		{
			folderPath = Path.Combine(folderPath, nameof(TypeIndexerGenerator));

			if (Directory.Exists(folderPath))
				Directory.Delete(folderPath, true);
			Directory.CreateDirectory(folderPath);

			var typeIndexProvider = CreateTypeIndexProvider();
			File.WriteAllText(Path.Combine(folderPath, $"{IndexedTypesInitializer}.generated.cs"), typeIndexProvider);
		}

		private static Type[] GetIndexedTypes()
		{
			var typesWithAttribute = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type =>
				{
					if (type.IsGenericType)
						return false;
					if (type.GetCustomAttribute<IndexedTypeAttribute>(true) != null)
						return true;
					return false;
				});

			var types = new HashSet<Type>();
			foreach (var type in typesWithAttribute)
			{
				if (!types.Add(type))
					continue;
				if (type.IsInterface || (type.IsClass && !type.IsSealed))
				{
					type.GetChildrenTypes(out var childrenTypes, out var childrenInterfaces);
					foreach (var child in childrenTypes)
					{
						if (!child.IsGenericType)
							types.Add(child);
					}
					foreach (var child in childrenInterfaces)
					{
						if (!child.IsGenericType)
							types.Add(child);
					}
				}
			}

			return types.OrderBy(t => t.FullName).ToArray();
		}

		private static string CreateTypeIndexProvider()
		{
			var types = GetIndexedTypes();
			var proxyTypes = InterfaceProxyGenerator.GetProxyTypes();

			var sourceBuilder = new StringBuilder();
			sourceBuilder.AppendLine("using System;");
			sourceBuilder.AppendLine("using System.Collections.Generic;");
			sourceBuilder.AppendLine("");
			sourceBuilder.AppendLine("namespace Sapientia.TypeIndexer");
			sourceBuilder.AppendLine("{");
			sourceBuilder.AppendLine("#if !PROXY_REFACTORING");
			sourceBuilder.AppendLine($"	public static class {IndexedTypesInitializer}");
			sourceBuilder.AppendLine("	{");
			sourceBuilder.AppendLine("		[UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSplashScreen)]");
			sourceBuilder.AppendLine("		public static void Initialize()");
			sourceBuilder.AppendLine("		{");
			sourceBuilder.AppendLine("			var indexToType = new Type[]");
			sourceBuilder.AppendLine("			{");
			for (var i = 0; i < types.Length; i++)
			{
				sourceBuilder.AppendLine($"				typeof({types[i].GetFullName()}),");
			}
			sourceBuilder.AppendLine("			};");
			sourceBuilder.AppendLine();
			sourceBuilder.AppendLine($"			var types = new Dictionary<Type, {nameof(TypeId)}>(indexToType.Length);");
			sourceBuilder.AppendLine("			for (var i = 0; i < indexToType.Length; i++)");
			sourceBuilder.AppendLine("			{");
			sourceBuilder.AppendLine("				types.Add(indexToType[i], i);");
			sourceBuilder.AppendLine("			}");
			sourceBuilder.AppendLine();
			sourceBuilder.AppendLine($"			var delegateIndexToDelegate = new {nameof(Delegate)}[]");
			sourceBuilder.AppendLine("			{");
			sourceBuilder.AppendLine(GenerateDelegateIndexBody(proxyTypes));
			sourceBuilder.AppendLine("			};");
			sourceBuilder.AppendLine();
			sourceBuilder.AppendLine($"			var delegates = new Dictionary<({nameof(TypeId)}, {nameof(ProxyId)}), {nameof(DelegateIndex)}>");
			sourceBuilder.AppendLine("			{");
			sourceBuilder.Append(GenerateDelegatesBody(proxyTypes, false));
			sourceBuilder.AppendLine("			};");
			sourceBuilder.AppendLine();
			sourceBuilder.AppendLine($"			{nameof(IndexedTypes)}.{nameof(IndexedTypes.Initialize)}(types, indexToType, delegateIndexToDelegate, delegates);");
			sourceBuilder.AppendLine();
			var contextMappings = BuildContextMappings(types);
			sourceBuilder.Append(GenerateContextIndicesBody(types, contextMappings));
			sourceBuilder.AppendLine();
			sourceBuilder.Append(GenerateTypeIndexInitialization(types));
			sourceBuilder.AppendLine();
			sourceBuilder.Append(GenerateContextTypeIndexInitialization(types, contextMappings));
			sourceBuilder.AppendLine("		}");
			sourceBuilder.AppendLine("	}");
			sourceBuilder.AppendLine("#endif");
			sourceBuilder.AppendLine("}");
			RuntimeHelpers.RunClassConstructor(typeof(int).TypeHandle);

			return sourceBuilder.ToString();
		}

		private static string GenerateDelegateIndexBody(List<(Type baseType, HashSet<Type> children)> proxyTypes)
		{
			var duplicates = new HashSet<string>();
			var sourceBuilder = new StringBuilder();

			foreach (var (baseType, children) in proxyTypes)
			{
				var methods = baseType.GetAllInstanceMethods();

				foreach (var child in children)
				{
					foreach (var methodInfo in methods)
					{
						var genericArguments = methodInfo.GetGenericArguments();
						if (genericArguments.Length > 0) // We don't support generic methods now
							continue;
						var genericParametersString = CodeGenExt.GetGenericParametersString(genericArguments);

						var body = $"				{baseType.Name}Proxy<{child.GetFullName()}>.Create{methodInfo.Name}Delegate{genericParametersString}(),";
						E.ASSERT(duplicates.Add(body));
						sourceBuilder.AppendLine(body);
					}
				}
			}

			return sourceBuilder.ToString();
		}

		private static string GenerateDelegatesBody(List<(Type baseType, HashSet<Type> children)> proxyTypes, bool isDebug)
		{
			var duplicates = new HashSet<string>();
			var sourceBuilder = new StringBuilder();
			for (int i = 0, delegateIndex = 0; i < proxyTypes.Count; i++)
			{
				var (baseType, children) = proxyTypes[i];

				var methods = baseType.GetAllInstanceMethods();
				var methodsCount = 0;
				foreach (var methodInfo in methods)
				{
					var genericArguments = methodInfo.GetGenericArguments();
					if (genericArguments.Length > 0) // We don't support generic methods now
						continue;
					methodsCount++;
				}

				foreach (var child in children)
				{
					var body = isDebug
						? $"				{{ (types[Type.GetType(\"{child.AssemblyQualifiedName}\")], {i}), {delegateIndex}}},"
						: $"				{{ (types[typeof({child.GetFullName()})], {i}), {delegateIndex}}},";

					E.ASSERT(duplicates.Add(body));
					sourceBuilder.AppendLine(body);
					delegateIndex += methodsCount;
				}
			}

			return sourceBuilder.ToString();
		}

		private static string GenerateTypeIndexInitialization(Type[] types)
		{
			var sourceBuilder = new StringBuilder();
			foreach (var type in types)
			{
				sourceBuilder.AppendLine($"			_ = TypeId<{type.GetFullName()}>.typeId;");
			}

			return sourceBuilder.ToString();
		}

		private static Dictionary<int, List<int>> BuildContextMappings(Type[] types)
		{
			var contextToChildren = new Dictionary<int, List<int>>();

			for (var contextIndex = 0; contextIndex < types.Length; contextIndex++)
			{
				var contextType = types[contextIndex];
				if (!contextType.IsInterface)
					continue;

				var children = new List<int>();
				for (var childIndex = 0; childIndex < types.Length; childIndex++)
				{
					if (childIndex == contextIndex)
						continue;
					var childType = types[childIndex];
					if (childType.IsInterface || childType.IsAbstract)
						continue;
					if (contextType.IsAssignableFrom(childType))
					{
						children.Add(childIndex);
					}
				}

				if (children.Count > 0)
				{
					contextToChildren[contextIndex] = children;
				}
			}

			return contextToChildren;
		}

		private static string GenerateContextIndicesBody(Type[] types, Dictionary<int, List<int>> contextMappings)
		{
			var sourceBuilder = new StringBuilder();
			sourceBuilder.AppendLine($"			var contextCounts = new int[indexToType.Length];");
			sourceBuilder.AppendLine($"			var contextTypeIndices = new Dictionary<(Type, Type), int>();");
			sourceBuilder.AppendLine($"			var contextChildren = new {nameof(TypeId)}[indexToType.Length][];");
			sourceBuilder.AppendLine();

			foreach (var (contextIndex, children) in contextMappings)
			{
				sourceBuilder.AppendLine($"			contextCounts[{contextIndex}] = {children.Count};");
				sourceBuilder.Append($"			contextChildren[{contextIndex}] = new {nameof(TypeId)}[] {{ ");
				sourceBuilder.Append(string.Join(", ", children));
				sourceBuilder.AppendLine(" };");
				for (var i = 0; i < children.Count; i++)
				{
					sourceBuilder.AppendLine($"			contextTypeIndices.Add((indexToType[{contextIndex}], indexToType[{children[i]}]), {i});");
				}
				sourceBuilder.AppendLine();
			}

			sourceBuilder.AppendLine($"			{nameof(IndexedTypes)}.{nameof(IndexedTypes.InitializeContextIndices)}(contextCounts, contextTypeIndices, contextChildren);");

			return sourceBuilder.ToString();
		}

		private static string GenerateContextTypeIndexInitialization(Type[] types, Dictionary<int, List<int>> contextMappings)
		{
			var sourceBuilder = new StringBuilder();
			foreach (var (contextIndex, children) in contextMappings)
			{
				var contextTypeName = types[contextIndex].GetFullName();
				foreach (var childIndex in children)
				{
					var childTypeName = types[childIndex].GetFullName();
					sourceBuilder.AppendLine($"			_ = TypeIndex<{contextTypeName}, {childTypeName}>.typeIndex;");
				}
			}

			return sourceBuilder.ToString();
		}
	}
}
