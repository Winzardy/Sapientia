using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Sapientia.Extensions.Reflection;

namespace Sapientia.TypeIndexer
{
	public static class TypeIndexerGenerator
	{
		public static void GenerateTypeIndexes(string folderPath)
		{
			folderPath = Path.Combine(folderPath, nameof(TypeIndexerGenerator));

			if (Directory.Exists(folderPath))
				Directory.Delete(folderPath, true);
			Directory.CreateDirectory(folderPath);

			var typeIndexProvider = CreateTypeIndexProvider();
			File.WriteAllText(Path.Combine(folderPath, "IndexedTypesProvider.generated.cs"), typeIndexProvider);
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
			sourceBuilder.AppendLine("	public static class IndexedTypesInitializer");
			sourceBuilder.AppendLine("	{");
			sourceBuilder.AppendLine("		[UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSplashScreen)]");
			sourceBuilder.AppendLine("		public static void Initialize()");
			sourceBuilder.AppendLine("		{");
			sourceBuilder.AppendLine("			var indexToType = new Type[]");
			sourceBuilder.AppendLine("			{");
			sourceBuilder.AppendLine("#if UNITY_EDITOR || (DEBUG && !UNITY_5_3_OR_NEWER)");

			for (var i = 0; i < types.Length; i++)
			{
				sourceBuilder.AppendLine($"				Type.GetType(\"{types[i].AssemblyQualifiedName}\"),");
			}

			sourceBuilder.AppendLine("#else");

			for (var i = 0; i < types.Length; i++)
			{
				sourceBuilder.AppendLine($"				typeof({types[i].GetFullName()}),");
			}

			sourceBuilder.AppendLine("#endif");
			sourceBuilder.AppendLine("			};");
			sourceBuilder.AppendLine();
			sourceBuilder.AppendLine($"			var types = new Dictionary<Type, {nameof(TypeIndex)}>(indexToType.Length);");
			sourceBuilder.AppendLine("			for (var i = 0; i < indexToType.Length; i++)");
			sourceBuilder.AppendLine("			{");
			sourceBuilder.AppendLine("				types.Add(indexToType[i], i);");
			sourceBuilder.AppendLine("			}");
			sourceBuilder.AppendLine();
			sourceBuilder.AppendLine($"			var delegateIndexToDelegate = new {nameof(Delegate)}[]");
			sourceBuilder.AppendLine("			{");
			sourceBuilder.AppendLine("#if PROXY_REFACTORING");
			sourceBuilder.AppendLine("#else");
			sourceBuilder.AppendLine(GenerateDelegateIndexBody(proxyTypes));
			sourceBuilder.AppendLine("#endif");
			sourceBuilder.AppendLine("			};");
			sourceBuilder.AppendLine();
			sourceBuilder.AppendLine($"			var delegates = new Dictionary<({nameof(TypeIndex)}, {nameof(ProxyId)}), {nameof(DelegateIndex)}>");
			sourceBuilder.AppendLine("			{");

			sourceBuilder.AppendLine("#if UNITY_EDITOR || (DEBUG && !UNITY_5_3_OR_NEWER)");
			sourceBuilder.Append(GenerateDelegatesBody(proxyTypes, true));
			sourceBuilder.AppendLine("#else");
			sourceBuilder.Append(GenerateDelegatesBody(proxyTypes, false));
			sourceBuilder.AppendLine("#endif");

			sourceBuilder.AppendLine("			};");
			sourceBuilder.AppendLine();
			sourceBuilder.AppendLine($"			{nameof(IndexedTypes)}.{nameof(IndexedTypes.Initialize)}(types, indexToType, delegateIndexToDelegate, delegates);");
			sourceBuilder.AppendLine("		}");
			sourceBuilder.AppendLine("	}");
			sourceBuilder.AppendLine("}");

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
						Debug.Assert(duplicates.Add(body));
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

					Debug.Assert(duplicates.Add(body));
					sourceBuilder.AppendLine(body);
					delegateIndex += methodsCount;
				}
			}

			return sourceBuilder.ToString();
		}
	}
}
