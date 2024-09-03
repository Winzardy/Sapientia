using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using GameLogic.Extensions;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator;

namespace Sapientia.TypeIndexer
{
	public unsafe class InterfaceProxyGenerator
	{
		private static string MethodImplAttribute = $"[{typeof(System.Runtime.CompilerServices.MethodImplAttribute).FullName}(256)]";
		private static string BurstAttribute = "[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]";

		private static string PrepareDirectory(string generationFolder)
		{
			generationFolder = Path.Combine(generationFolder, nameof(InterfaceProxyGenerator));
			if (Directory.Exists(generationFolder))
				Directory.Delete(generationFolder, true);

			return generationFolder;
		}

		public static void GenerateProxies(string baseGenerationFolder, System.Collections.Generic.Dictionary<string, string> assemblyNameToGenerationFolder)
		{
			var baseFolder = PrepareDirectory(baseGenerationFolder);
			var assemblyNameToFolder = new System.Collections.Generic.Dictionary<string, string>(assemblyNameToGenerationFolder.Count);
			foreach (var (assemblyName, generationFolder) in assemblyNameToGenerationFolder)
			{
				assemblyNameToFolder.Add(assemblyName, PrepareDirectory(generationFolder));
			}

			var proxyTypes = GetProxyTypes();
			for (var i = 0; i < proxyTypes.Count; i++)
			{
				var proxyCode = CreateFile(proxyTypes[i].baseType, i);

				var assemblyName = proxyTypes[i].baseType.Assembly.GetName().Name;
				var path = assemblyNameToFolder.GetValueOrDefault(assemblyName, baseFolder);

				Directory.CreateDirectory(path);
				File.WriteAllText(Path.Combine(path, $"{proxyTypes[i].baseType.Name}Proxy.generated.cs"), proxyCode);
			}
		}

		public static System.Collections.Generic.List<(Type baseType, System.Collections.Generic.List<Type> children)> GetProxyTypes()
		{
			var result = new System.Collections.Generic.List<(Type baseType, System.Collections.Generic.List<Type> children)>();

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
				var children = new System.Collections.Generic.List<Type>();

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

					var newChildren = new System.Collections.Generic.List<Type>();
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

				parametersString = parametersString.Replace("(", "(void* __executorPtr" + (parameters.Length > 0 ? ", " : string.Empty));
				parametersWithoutTypeString = parametersWithoutTypeString.Replace("(", "(__executorPtr" + (parameters.Length > 0 ? ", " : string.Empty));

				sourceBuilder.AppendLine($"		internal delegate {returnTypeString} {methodInfo.Name}Delegate{genericParametersString}{parametersString};");
				sourceBuilder.AppendLine($"		{MethodImplAttribute}");
				sourceBuilder.AppendLine($"		public readonly {returnTypeString} {methodInfo.Name}{genericParametersString}{parametersString}");
				sourceBuilder.AppendLine($"		{{");
				sourceBuilder.AppendLine($"			var __compiledMethod = {nameof(IndexedTypes)}.{nameof(IndexedTypes.GetCompiledMethod)}(this._firstDelegateIndex + {delegateIndex});");
				sourceBuilder.AppendLine($"			var __method = {typeof(Marshal).FullName}.{nameof(Marshal.GetDelegateForFunctionPointer)}<{methodInfo.Name}Delegate{genericParametersString}>(__compiledMethod.functionPointer);");
				if (returnType.IsVoid())
					sourceBuilder.AppendLine($"			__method.Invoke{genericParametersString}{parametersWithoutTypeString};");
				else
					sourceBuilder.AppendLine($"			return __method.Invoke{genericParametersString}{parametersWithoutTypeString};");
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
				{
					var proxyRefParametersString = parametersString.Replace("(", $"(this ProxyRef<{baseType.Name}Proxy> __proxyRef" + (parameters.Length > 0 ? ", " : string.Empty));
					var proxyRefParametersWithoutTypeString = parametersWithoutTypeString.Replace("(", "(__proxyRef.GetPtr()" + (parameters.Length > 0 ? ", " : string.Empty));

					sourceBuilder.AppendLine($"		{MethodImplAttribute}");
					sourceBuilder.AppendLine($"		public static {returnTypeString} {methodInfo.Name}{genericParametersString}{proxyRefParametersString}");
					sourceBuilder.AppendLine($"		{{");
					if (returnType.IsVoid())
						sourceBuilder.AppendLine($"			__proxyRef.proxy.{methodInfo.Name}{genericParametersString}{proxyRefParametersWithoutTypeString};");
					else
						sourceBuilder.AppendLine($"			return __proxyRef.proxy.{methodInfo.Name}{genericParametersString}{proxyRefParametersWithoutTypeString};");
					sourceBuilder.AppendLine($"		}}");
					sourceBuilder.AppendLine();
				}

				var eventParametersString = parametersString.Replace("(", $"(this ref ProxyEvent<{baseType.Name}Proxy> __proxyEvent" + (parameters.Length > 0 ? ", " : string.Empty));
				var eventParametersWithoutTypeString = parametersWithoutTypeString.Replace("(", "(__proxyRef->GetPtr()" + (parameters.Length > 0 ? ", " : string.Empty));
				{
					sourceBuilder.AppendLine($"		{MethodImplAttribute}");
					sourceBuilder.AppendLine($"		public static {returnTypeString} {methodInfo.Name}{genericParametersString}{eventParametersString}");
					sourceBuilder.AppendLine($"		{{");
					if (!returnType.IsVoid())
						sourceBuilder.AppendLine($"			{returnTypeString} __result = default;");
					sourceBuilder.AppendLine($"			foreach (ProxyRef<{baseType.Name}Proxy>* __proxyRef in __proxyEvent.GetEnumerable())");
					sourceBuilder.AppendLine($"			{{");
					if (returnType.IsVoid())
						sourceBuilder.AppendLine($"				__proxyRef->proxy.{methodInfo.Name}{genericParametersString}{eventParametersWithoutTypeString};");
					else
						sourceBuilder.AppendLine($"				__result = __proxyRef->proxy.{methodInfo.Name}{genericParametersString}{eventParametersWithoutTypeString};");
					sourceBuilder.AppendLine($"			}}");
					if (!returnType.IsVoid())
						sourceBuilder.AppendLine($"			return __result;");
					sourceBuilder.AppendLine($"		}}");
					sourceBuilder.AppendLine();
				}

				var eventAllocatorParametersString = parametersString.Replace("(", $"(this ref ProxyEvent<{baseType.Name}Proxy> __proxyEvent, {typeof(Allocator).FullName}* __allocator" + (parameters.Length > 0 ? ", " : string.Empty));
				{
					sourceBuilder.AppendLine($"		{MethodImplAttribute}");
					sourceBuilder.AppendLine($"		public static {returnTypeString} {methodInfo.Name}{genericParametersString}{eventAllocatorParametersString}");
					sourceBuilder.AppendLine($"		{{");
					if (!returnType.IsVoid())
						sourceBuilder.AppendLine($"			{returnTypeString} __result = default;");
					sourceBuilder.AppendLine($"			foreach (ProxyRef<{baseType.Name}Proxy>* __proxyRef in __proxyEvent.GetEnumerable(__allocator))");
					sourceBuilder.AppendLine($"			{{");
					if (returnType.IsVoid())
						sourceBuilder.AppendLine($"				__proxyRef->proxy.{methodInfo.Name}{genericParametersString}{eventParametersWithoutTypeString};");
					else
						sourceBuilder.AppendLine($"				__result = __proxyRef->proxy.{methodInfo.Name}{genericParametersString}{eventParametersWithoutTypeString};");
					sourceBuilder.AppendLine($"			}}");
					if (!returnType.IsVoid())
						sourceBuilder.AppendLine($"			return __result;");
					sourceBuilder.AppendLine($"		}}");
					sourceBuilder.AppendLine();
				}
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
				sourceBuilder.AppendLine($"			ref var __source = ref {typeof(UnsafeExt).FullName}.AsRef<TSource>(executorPtr);");
				if (returnType.IsVoid())
					sourceBuilder.AppendLine($"			__source.{methodInfo.Name}{genericParametersString}{methodParametersWithoutTypeString};");
				else
					sourceBuilder.AppendLine($"			return __source.{methodInfo.Name}{genericParametersString}{methodParametersWithoutTypeString};");
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
