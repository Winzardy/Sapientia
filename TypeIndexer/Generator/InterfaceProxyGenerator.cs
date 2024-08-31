using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using GameLogic.Extensions;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.TypeIndexer
{
	public unsafe class InterfaceProxyGenerator
	{
		private static string MethodImplAttribute = $"[{typeof(System.Runtime.CompilerServices.MethodImplAttribute).FullName}(256)]";
		private static string BurstAttribute = "[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]";

		public static void GenerateProxies(string folderPath, string internalLibraryFolderPath)
		{
			if (Directory.Exists(folderPath))
				Directory.Delete(folderPath, true);
			Directory.CreateDirectory(folderPath);
			if (Directory.Exists(internalLibraryFolderPath))
				Directory.Delete(internalLibraryFolderPath, true);
			Directory.CreateDirectory(internalLibraryFolderPath);

			var proxyTypes = GetProxyTypes();
			for (var i = 0; i < proxyTypes.Count; i++)
			{
				var path = folderPath;
				var fullName = proxyTypes[i].baseType.FullName;
				if (!string.IsNullOrEmpty(fullName) && fullName.StartsWith(nameof(Sapientia)))
					path = internalLibraryFolderPath;

				var proxyCode = CreateFile(proxyTypes[i].baseType, i);
				File.WriteAllText(Path.Combine(path, $"{proxyTypes[i].baseType.Name}Proxy.generated.cs"), proxyCode);
			}
		}

		public static List<(Type baseType, List<Type> children)> GetProxyTypes()
		{
			var result = new List<(Type baseType, List<Type> children)>();

			var typesWithAttribute = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type =>
				{
					if (type.IsInterface && !type.IsGenericType && type.HasAttribute<InterfaceProxyAttribute>())
						return true;
					return false;
				});

			foreach (var baseType in typesWithAttribute)
			{
				var children = new List<Type>();

				baseType.GetChildrenTypes(out var childrenTypes, out var interfaceTypes);
				foreach (var child in childrenTypes)
				{
					if (child.IsGenericType || !child.IsBlittable())
						continue;
					children.Add(child);
				}
				result.Add((baseType, children));

				foreach (var newBaseType in interfaceTypes)
				{
					if (newBaseType.IsGenericType)
						continue;

					var newChildren = new List<Type>();
					newBaseType.GetChildrenTypes(out childrenTypes, out _);
					foreach (var child in childrenTypes)
					{
						if (child.IsGenericType || !child.IsBlittable())
							continue;
						children.Add(child);
					}

					result.Add((newBaseType, newChildren));
				}
			}

			return result;
		}

		private static string CreateFile(Type baseType, int proxyIndex)
		{
			var methods = baseType.GetAllInstanceMethods();

			var sourceBuilder = new StringBuilder();
			sourceBuilder.AppendLine("using System;");
			sourceBuilder.AppendLine("using System.Collections.Generic;");
			sourceBuilder.AppendLine("using Sapientia.MemoryAllocator.Data;");
			sourceBuilder.AppendLine("");
			sourceBuilder.AppendLine("namespace Sapientia.TypeIndexer");
			sourceBuilder.AppendLine("{");
			sourceBuilder.Append(CreateBaseProxy(baseType, methods, proxyIndex));
			sourceBuilder.AppendLine();
			sourceBuilder.Append(CreateBaseProxyExt(baseType, methods));
			sourceBuilder.AppendLine();
			sourceBuilder.Append(CreateGenericProxy(baseType, methods));

			sourceBuilder.AppendLine("}");

			return sourceBuilder.ToString();
		}

		private static string CreateBaseProxy(Type baseType, IEnumerable<MethodInfo> methods, int proxyIndex)
		{
			var sourceBuilder = new StringBuilder();
			sourceBuilder.AppendLine($"	public unsafe struct {baseType.Name}Proxy : {nameof(IProxy)}");
			sourceBuilder.AppendLine("	{");
			sourceBuilder.AppendLine($"		public static readonly {nameof(ProxyIndex)} ProxyIndex = {proxyIndex};");
			sourceBuilder.AppendLine($"		{nameof(ProxyIndex)} {nameof(IProxy)}.{nameof(IProxy.ProxyIndex)}");
			sourceBuilder.AppendLine($"		{{");
			sourceBuilder.AppendLine($"			{MethodImplAttribute}");
			sourceBuilder.AppendLine($"			get => ProxyIndex;");
			sourceBuilder.AppendLine($"		}}");
			sourceBuilder.AppendLine();
			sourceBuilder.AppendLine($"		private {nameof(DelegateIndex)} _firstDelegateIndex;");
			sourceBuilder.AppendLine($"		{nameof(DelegateIndex)} {nameof(IProxy)}.{nameof(IProxy.FirstDelegateIndex)}");
			sourceBuilder.AppendLine($"		{{");
			sourceBuilder.AppendLine($"			{MethodImplAttribute}");
			sourceBuilder.AppendLine($"			get => _firstDelegateIndex;");
			sourceBuilder.AppendLine($"			{MethodImplAttribute}");
			sourceBuilder.AppendLine($"			set => _firstDelegateIndex = value;");
			sourceBuilder.AppendLine($"		}}");
			sourceBuilder.AppendLine();

			var delegateIndex = 0;
			foreach (var methodInfo in methods)
			{
				var returnType = methodInfo.ReturnType;
				var genericArguments = methodInfo.GetGenericArguments();
				if (genericArguments.Length > 0) // We don't support generic methods now
					continue;
				var parameters = methodInfo.GetParameters();

				var returnTypeString = CodeGenExt.GetTypeString(returnType);
				var genericParametersString = CodeGenExt.GetGenericParametersString(genericArguments);
				var parametersString = CodeGenExt.GetParametersString(parameters, false);
				var parametersWithoutTypeString = CodeGenExt.GetParametersString(parameters, true);

				parametersString = parametersString.Replace("(", "(void* executorPtr" + (parameters.Length > 0 ? ", " : string.Empty));
				parametersWithoutTypeString = parametersWithoutTypeString.Replace("(", "(executorPtr" + (parameters.Length > 0 ? ", " : string.Empty));

				sourceBuilder.AppendLine($"		internal delegate {returnTypeString} {methodInfo.Name}Delegate{genericParametersString}{parametersString};");
				sourceBuilder.AppendLine($"		{MethodImplAttribute}");
				sourceBuilder.AppendLine($"		public readonly {returnTypeString} {methodInfo.Name}{genericParametersString}{parametersString}");
				sourceBuilder.AppendLine($"		{{");
				sourceBuilder.AppendLine($"			var compiledMethod = {nameof(IndexedTypes)}.{nameof(IndexedTypes.GetCompiledMethod)}(_firstDelegateIndex + {delegateIndex});");
				sourceBuilder.AppendLine($"			var method = {typeof(Marshal).FullName}.{nameof(Marshal.GetDelegateForFunctionPointer)}<{methodInfo.Name}Delegate{genericParametersString}>(compiledMethod.functionPointer);");
				if (returnType.IsVoid())
					sourceBuilder.AppendLine($"			method.Invoke{genericParametersString}{parametersWithoutTypeString};");
				else
					sourceBuilder.AppendLine($"			return method.Invoke{genericParametersString}{parametersWithoutTypeString};");
				sourceBuilder.AppendLine($"		}}");
				sourceBuilder.AppendLine();

				delegateIndex++;
			}

			sourceBuilder.AppendLine("	}");

			return sourceBuilder.ToString();
		}

		private static string CreateBaseProxyExt(Type baseType, IEnumerable<MethodInfo> methods)
		{
			var sourceBuilder = new StringBuilder();
			sourceBuilder.AppendLine($"	public static unsafe class {baseType.Name}ProxyExt");
			sourceBuilder.AppendLine("	{");

			foreach (var methodInfo in methods)
			{
				var returnType = methodInfo.ReturnType;
				var genericArguments = methodInfo.GetGenericArguments();
				if (genericArguments.Length > 0) // We don't support generic methods now
					continue;
				var parameters = methodInfo.GetParameters();

				var returnTypeString = CodeGenExt.GetTypeString(returnType);
				var genericParametersString = CodeGenExt.GetGenericParametersString(genericArguments);
				var parametersString = CodeGenExt.GetParametersString(parameters, false);
				var parametersWithoutTypeString = CodeGenExt.GetParametersString(parameters, true);

				var proxyRefParametersString = parametersString.Replace("(", $"(this ProxyRef<{baseType.Name}Proxy> proxyRef" + (parameters.Length > 0 ? ", " : string.Empty));
				var proxyRefParametersWithoutTypeString = parametersWithoutTypeString.Replace("(", "(proxyRef.GetPtr()" + (parameters.Length > 0 ? ", " : string.Empty));

				sourceBuilder.AppendLine($"		{MethodImplAttribute}");
				sourceBuilder.AppendLine($"		public static {returnTypeString} {methodInfo.Name}{genericParametersString}{proxyRefParametersString}");
				sourceBuilder.AppendLine($"		{{");
				if (returnType.IsVoid())
					sourceBuilder.AppendLine($"			proxyRef.proxy.{methodInfo.Name}{genericParametersString}{proxyRefParametersWithoutTypeString};");
				else
					sourceBuilder.AppendLine($"			return proxyRef.proxy.{methodInfo.Name}{genericParametersString}{proxyRefParametersWithoutTypeString};");
				sourceBuilder.AppendLine($"		}}");
				sourceBuilder.AppendLine();

				var eventParametersString = parametersString.Replace("(", $"(this ref ProxyEvent<{baseType.Name}Proxy> proxyEvent" + (parameters.Length > 0 ? ", " : string.Empty));
				var eventParametersWithoutTypeString = parametersWithoutTypeString.Replace("(", "(proxyRef->GetPtr()" + (parameters.Length > 0 ? ", " : string.Empty));

				sourceBuilder.AppendLine($"		{MethodImplAttribute}");
				sourceBuilder.AppendLine($"		public static {returnTypeString} {methodInfo.Name}{genericParametersString}{eventParametersString}");
				sourceBuilder.AppendLine($"		{{");
				sourceBuilder.AppendLine($"			ref var allocator = ref proxyEvent.GetAllocator();");
				if (!returnType.IsVoid())
					sourceBuilder.AppendLine($"			{returnTypeString} result = default;");
				sourceBuilder.AppendLine($"			foreach (ProxyRef<{baseType.Name}Proxy>* proxyRef in proxyEvent.GetEnumerable(allocator))");
				sourceBuilder.AppendLine($"			{{");
				if (returnType.IsVoid())
					sourceBuilder.AppendLine($"				proxyRef->proxy.{methodInfo.Name}{genericParametersString}{eventParametersWithoutTypeString};");
				else
					sourceBuilder.AppendLine($"				result = proxyRef->proxy.{methodInfo.Name}{genericParametersString}{eventParametersWithoutTypeString};");
				sourceBuilder.AppendLine($"			}}");
				if (!returnType.IsVoid())
					sourceBuilder.AppendLine($"			return result;");
				sourceBuilder.AppendLine($"		}}");
				sourceBuilder.AppendLine();
			}

			sourceBuilder.AppendLine("	}");

			return sourceBuilder.ToString();
		}

		private static string CreateGenericProxy(Type baseType, IEnumerable<MethodInfo> methods)
		{
			var sourceBuilder = new StringBuilder();
			sourceBuilder.AppendLine($"	public unsafe struct {baseType.Name}Proxy<TSource> where TSource: struct, {baseType.FullName}");
			sourceBuilder.AppendLine("	{");

			foreach (var methodInfo in methods)
			{
				var returnType = methodInfo.ReturnType;
				var genericArguments = methodInfo.GetGenericArguments();
				if (genericArguments.Length > 0) // We don't support generic methods now
					continue;
				var parameters = methodInfo.GetParameters();

				var returnTypeString = CodeGenExt.GetTypeString(returnType);
				var genericParametersString = CodeGenExt.GetGenericParametersString(genericArguments);
				var parametersString = CodeGenExt.GetParametersString(parameters, false);
				var methodParametersWithoutTypeString = CodeGenExt.GetParametersString(parameters, true);

				parametersString = parametersString.Replace("(", "(void* executorPtr" + (parameters.Length > 0 ? ", " : string.Empty));

				sourceBuilder.AppendLine("#if UNITY_5_3_OR_NEWER");
				sourceBuilder.AppendLine("		[UnityEngine.Scripting.Preserve]");
				sourceBuilder.AppendLine("#if BURST");
				sourceBuilder.AppendLine($"		{BurstAttribute}");
				sourceBuilder.AppendLine("#endif");
				sourceBuilder.AppendLine("#endif");
				sourceBuilder.AppendLine($"		[{typeof(AOT.MonoPInvokeCallbackAttribute).FullName}(typeof({baseType.Name}Proxy.{methodInfo.Name}Delegate))]");
				sourceBuilder.AppendLine($"		private static {returnTypeString} {methodInfo.Name}{genericParametersString}{parametersString}");
				sourceBuilder.AppendLine("		{");
				sourceBuilder.AppendLine($"			ref var @source = ref {typeof(UnsafeExt).FullName}.AsRef<TSource>(executorPtr);");
				if (returnType.IsVoid())
					sourceBuilder.AppendLine($"			@source.{methodInfo.Name}{genericParametersString}{methodParametersWithoutTypeString};");
				else
					sourceBuilder.AppendLine($"			return @source.{methodInfo.Name}{genericParametersString}{methodParametersWithoutTypeString};");
				sourceBuilder.AppendLine("		}");
				sourceBuilder.AppendLine();
				sourceBuilder.AppendLine("#if UNITY_5_3_OR_NEWER");
				sourceBuilder.AppendLine("		[UnityEngine.Scripting.Preserve]");
				sourceBuilder.AppendLine("#endif");
				sourceBuilder.AppendLine($"		{MethodImplAttribute}");
				sourceBuilder.AppendLine($"		public static {nameof(CompiledMethod)} Compile{methodInfo.Name}{genericParametersString}()");
				sourceBuilder.AppendLine($"		{{");
				sourceBuilder.AppendLine($"			return {nameof(CompiledMethod)}.Create<{baseType.Name}Proxy.{methodInfo.Name}Delegate>({methodInfo.Name}{genericParametersString});");
				sourceBuilder.AppendLine($"		}}");
			}

			sourceBuilder.AppendLine("	}");
			return sourceBuilder.ToString();
		}
	}
}
