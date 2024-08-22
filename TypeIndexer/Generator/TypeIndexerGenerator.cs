using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using GameLogic.Extensions;
using Sapientia.Extensions;

namespace Sapientia.TypeIndexer
{
	public static class TypeIndexerGenerator
	{
		public static void GenerateTypeIndexes(string folderPath)
		{
			var typeIndexProvider = CreateTypeIndexProvider();

			if (Directory.Exists(folderPath))
				Directory.Delete(folderPath, true);
			Directory.CreateDirectory(folderPath);

			File.WriteAllText(Path.Combine(folderPath, "IndexedTypesProvider.generated.cs"), typeIndexProvider);
		}

		private static Type[] GetIndexedTypes()
		{
			var typesWithAttribute = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type =>
				{
					if (type.GetCustomAttribute<IndexedTypeAttribute>(true) != null)
						return true;
					if (type.GetCustomAttribute<InterfaceProxyAttribute>(true) != null)
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
						types.Add(child);
					foreach (var child in childrenInterfaces)
						types.Add(child);
				}
			}

			return types.ToArray();
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
			sourceBuilder.AppendLine($"	public static class IndexedTypesInitializer");
			sourceBuilder.AppendLine("	{");
			sourceBuilder.AppendLine("		public static void Initialize()");
			sourceBuilder.AppendLine("		{");

			sourceBuilder.AppendLine($"			var typeToIndex = new Dictionary<Type, {nameof(TypeIndex)}>");
			sourceBuilder.AppendLine("			{");
			sourceBuilder.AppendLine("#if UNITY_EDITOR || (DEBUG && !UNITY_5_3_OR_NEWER)");

			for (var i = 0; i < types.Length; i++)
			{
				sourceBuilder.AppendLine($"				{{ Type.GetType(\"{types[i].FullName}\"), {i} }},");
			}

			sourceBuilder.AppendLine("#else");

			for (var i = 0; i < types.Length; i++)
			{
				sourceBuilder.AppendLine($"				{{ typeof({types[i].FullName}), {i} }},");
			}

			sourceBuilder.AppendLine("#endif");
			sourceBuilder.AppendLine("			};");

			sourceBuilder.AppendLine();
			sourceBuilder.AppendLine("			var indexToType = new Type[]");
			sourceBuilder.AppendLine("			{");
			sourceBuilder.AppendLine("#if UNITY_EDITOR || (DEBUG && !UNITY_5_3_OR_NEWER)");

			for (var i = 0; i < types.Length; i++)
			{
				sourceBuilder.AppendLine($"				Type.GetType(\"{types[i].FullName}\"),");
			}

			sourceBuilder.AppendLine("#else");

			for (var i = 0; i < types.Length; i++)
			{
				sourceBuilder.AppendLine($"				typeof({types[i].FullName}),");
			}

			sourceBuilder.AppendLine("#endif");
			sourceBuilder.AppendLine("			};");
			sourceBuilder.AppendLine();
			sourceBuilder.AppendLine($"			var delegateIndexToCompiledMethod = new {nameof(CompiledMethod)}[]");
			sourceBuilder.AppendLine("			{");

			foreach (var (baseType, children) in proxyTypes)
			{
				var methods = baseType.GetMethods();
				foreach (var methodInfo in methods)
				{
					var genericArguments = methodInfo.GetGenericArguments();
					if (genericArguments.Length > 0) // We don't support generic methods now
						continue;
					var genericParametersString = CodeGenExt.GetGenericParametersString(genericArguments);

					foreach (var child in children)
					{
						sourceBuilder.AppendLine($"			{baseType.Name}<{child.FullName}>.Compile{methodInfo.Name}{genericParametersString}(),");
					}
				}
			}

			sourceBuilder.AppendLine("				");
			sourceBuilder.AppendLine("			};");
			sourceBuilder.AppendLine();
			sourceBuilder.AppendLine($"			var typeToDelegateIndex = new Dictionary<({nameof(TypeIndex)}, {nameof(ProxyIndex)}), {nameof(DelegateIndex)}>");
			sourceBuilder.AppendLine("			{");

			sourceBuilder.AppendLine("#if UNITY_EDITOR || (DEBUG && !UNITY_5_3_OR_NEWER)");
			sourceBuilder.AppendLine(GenerateTypeToDelegateIndexBody(proxyTypes, true));
			sourceBuilder.AppendLine("#else");
			sourceBuilder.AppendLine(GenerateTypeToDelegateIndexBody(proxyTypes, false));
			sourceBuilder.AppendLine("#endif");

			sourceBuilder.AppendLine("			};");
			sourceBuilder.AppendLine();
			sourceBuilder.AppendLine($"			{nameof(IndexedTypes)}.{nameof(IndexedTypes.Initialize)}(typeToIndex, indexToType, delegateIndexToCompiledMethod, typeToDelegateIndex);");
			sourceBuilder.AppendLine("		}");
			sourceBuilder.AppendLine("	}");
			sourceBuilder.AppendLine("}");

			return sourceBuilder.ToString();
		}

		private static string GenerateTypeToDelegateIndexBody(List<(Type baseType, List<Type> children)> proxyTypes, bool isDebug)
		{
			var sourceBuilder = new StringBuilder();
			for (int i = 0, delegateIndex = 0; i < proxyTypes.Count; i++)
			{
				var (baseType, children) = proxyTypes[i];

				var methods = baseType.GetMethods();
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
					if (isDebug)
						sourceBuilder.AppendLine($"			{{ (typeToIndex[Type.GetType(\"{child.FullName}\")], {i}), {delegateIndex}}},");
					else
						sourceBuilder.AppendLine($"			{{ (typeToIndex[typeof({child.FullName})], {i}), {delegateIndex}}},");
					delegateIndex += methodsCount;
				}
			}

			return sourceBuilder.ToString();
		}
	}
}
