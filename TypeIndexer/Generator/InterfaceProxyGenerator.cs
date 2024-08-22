using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using GameLogic.Extensions;
using Sapientia.Extensions;

namespace Sapientia.TypeIndexer
{
	public unsafe class InterfaceProxyGenerator
	{
		private static void ParameterSource(void* executorPtr)
		{
		}

		public static void GenerateProxies(string folderPath)
		{
			if (Directory.Exists(folderPath))
				Directory.Delete(folderPath, true);
			Directory.CreateDirectory(folderPath);

			var proxyTypes = GetProxyTypes();
			for (var i = 0; i < proxyTypes.Count; i++)
			{
				var proxyCode = CreateProxy(proxyTypes[i].baseType, i);
				File.WriteAllText(Path.Combine(folderPath, $"{proxyTypes[i].baseType.Name}Proxy.generated.cs"),
					proxyCode);
			}
		}

		public static List<(Type baseType, List<Type> children)> GetProxyTypes()
		{
			var result = new List<(Type baseType, List<Type> children)>();

			var typesWithAttribute = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type =>
				{
					if (type.IsInterface && !type.IsGenericType && type.GetCustomAttribute<InterfaceProxyAttribute>(true) != null)
						return true;
					return false;
				});

			foreach (var baseType in typesWithAttribute)
			{
				var children = new List<Type>();

				baseType.GetChildrenTypes(out var childrenTypes, out _);
				foreach (var child in childrenTypes)
				{
					if (child.IsGenericType || !child.IsBlittable())
						continue;
					children.Add(child);
				}

				if (children.Count > 0)
					result.Add((baseType, children));
			}

			return result;
		}

		private static string CreateProxy(Type baseType, int proxyIndex)
		{
			var methImplAttribute = $"[{typeof(System.Runtime.CompilerServices.MethodImplAttribute).FullName}(256)]";
			var burstAttribute =
				"[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]";
			var executorPtrParameter =
				typeof(InterfaceProxyGenerator).GetMethod(nameof(ParameterSource))!.GetParameters();
			var methods = baseType.GetMethods();

			var sourceBuilder = new StringBuilder();
			sourceBuilder.AppendLine("using System;");
			sourceBuilder.AppendLine("using System.Collections.Generic;");
			sourceBuilder.AppendLine("");
			sourceBuilder.AppendLine("namespace Sapientia.TypeIndexer");
			sourceBuilder.AppendLine("{");
			sourceBuilder.AppendLine($"	public unsafe struct {baseType.Name}Proxy : {nameof(IInterfaceProxy)}");
			sourceBuilder.AppendLine("	{");
			sourceBuilder.AppendLine($"		public static readonly {nameof(ProxyIndex)} ProxyIndex = {proxyIndex};");
			sourceBuilder.AppendLine(
				$"		{nameof(ProxyIndex)} {nameof(IInterfaceProxy)}.{nameof(IInterfaceProxy.ProxyIndex)}");
			sourceBuilder.AppendLine($"		{{");
			sourceBuilder.AppendLine($"			{methImplAttribute}");
			sourceBuilder.AppendLine($"			get => ProxyIndex;");
			sourceBuilder.AppendLine($"		}}");
			sourceBuilder.AppendLine();
			sourceBuilder.AppendLine($"		private {nameof(DelegateIndex)} _firstDelegateIndex;");
			sourceBuilder.AppendLine(
				$"		{nameof(DelegateIndex)} {nameof(IInterfaceProxy)}.{nameof(IInterfaceProxy.DelegateIndex)}");
			sourceBuilder.AppendLine($"		{{");
			sourceBuilder.AppendLine($"			{methImplAttribute}");
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
				var parameters = executorPtrParameter;
				ArrayExt.AddRange(ref parameters, methodInfo.GetParameters());

				var returnTypeString = CodeGenExt.GetTypeString(returnType);
				var genericParametersString = CodeGenExt.GetGenericParametersString(genericArguments);
				var parametersString = CodeGenExt.GetParametersString(parameters, false);
				var parametersWithoutTypeString = CodeGenExt.GetParametersString(parameters, true);

				sourceBuilder.AppendLine($"		internal delegate {returnTypeString} {methodInfo.Name}Delegate{genericParametersString}{parametersString};");
				sourceBuilder.AppendLine($"		{methImplAttribute}");
				sourceBuilder.AppendLine($"		public {returnTypeString} {methodInfo.Name}{genericParametersString}{parametersString}");
				sourceBuilder.AppendLine($"		{{");
				sourceBuilder.AppendLine($"			var compiledMethod = {nameof(IndexedTypes)}.{nameof(IndexedTypes.GetCompiledMethod)}(_firstDelegateIndex + {delegateIndex});");
				sourceBuilder.AppendLine($"			var method = {typeof(Marshal).FullName}.{nameof(Marshal.GetDelegateForFunctionPointer)}<{methodInfo.Name}Delegate{genericParametersString}>(compiledMethod.functionPointer)");
				if (returnType.IsVoid())
					sourceBuilder.AppendLine($"			method.Invoke{genericParametersString}{parametersWithoutTypeString};");
				else
					sourceBuilder.AppendLine($"			return method.Invoke{genericParametersString}{parametersWithoutTypeString};");
				sourceBuilder.AppendLine($"		}}");

				delegateIndex++;
			}

			sourceBuilder.AppendLine("	}");
			sourceBuilder.AppendLine();
			sourceBuilder.AppendLine(
				$"	public unsafe struct {baseType.Name}Proxy<TSource> where TSource: struct, {baseType.FullName}");
			sourceBuilder.AppendLine("	{");

			foreach (var methodInfo in methods)
			{
				var returnType = methodInfo.ReturnType;
				var genericArguments = methodInfo.GetGenericArguments();
				if (genericArguments.Length > 0) // We don't support generic methods now
					continue;
				var parameters = executorPtrParameter;
				ArrayExt.AddRange(ref parameters, methodInfo.GetParameters());

				var returnTypeString = CodeGenExt.GetTypeString(returnType);
				var genericParametersString = CodeGenExt.GetGenericParametersString(genericArguments);
				var parametersString = CodeGenExt.GetParametersString(parameters, false);
				var parametersWithoutTypeString = CodeGenExt.GetParametersString(parameters, true);

				sourceBuilder.AppendLine("#if UNITY_5_3_OR_NEWER");
				sourceBuilder.AppendLine("		[UnityEngine.Scripting.Preserve]");
				sourceBuilder.AppendLine("#if BURST");
				sourceBuilder.AppendLine($"		{burstAttribute}");
				sourceBuilder.AppendLine("#endif");
				sourceBuilder.AppendLine("#endif");
				sourceBuilder.AppendLine($"		[{typeof(AOT.MonoPInvokeCallbackAttribute).FullName}(typeof({baseType.Name}Proxy.{methodInfo.Name}Delegate))]");
				sourceBuilder.AppendLine($"		private static {returnTypeString} {methodInfo.Name}{genericParametersString}{parametersString}");
				sourceBuilder.AppendLine("		{");
				sourceBuilder.AppendLine($"			ref var value = ref {typeof(UnsafeExt).FullName}.AsRef<TSource>(executorPtr);");
				if (returnType.IsVoid())
					sourceBuilder.AppendLine($"			value.{methodInfo.Name}{genericParametersString}{parametersWithoutTypeString};");
				else
					sourceBuilder.AppendLine($"			return value.{methodInfo.Name}{genericParametersString}{parametersWithoutTypeString};");
				sourceBuilder.AppendLine("		}");
				sourceBuilder.AppendLine();
				sourceBuilder.AppendLine("#if UNITY_5_3_OR_NEWER");
				sourceBuilder.AppendLine("		[UnityEngine.Scripting.Preserve]");
				sourceBuilder.AppendLine("#endif");
				sourceBuilder.AppendLine($"		{methImplAttribute}");
				sourceBuilder.AppendLine($"		internal static {nameof(CompiledMethod)} Compile{methodInfo.Name}{genericParametersString}()");
				sourceBuilder.AppendLine($"		{{");
				sourceBuilder.AppendLine($"			return {nameof(CompiledMethod)}.Create<{baseType.Name}Proxy.{methodInfo.Name}Delegate>({methodInfo.Name}{genericParametersString});");
				sourceBuilder.AppendLine($"		}}");
			}

			sourceBuilder.AppendLine("	}");

			return sourceBuilder.ToString();
		}
	}
}
