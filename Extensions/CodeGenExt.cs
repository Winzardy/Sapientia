using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sapientia.Extensions
{
	public static class CodeGenExt
	{
		private const string VoidFullName = "System.Void";

		public static string GetMethodString(MethodInfo methodInfo)
		{
			var returnType = methodInfo.ReturnType;
			var genericArguments = methodInfo.GetGenericArguments();
			var delegateName = methodInfo.Name;
			var parameters = methodInfo.GetParameters();

			return GetMethodString(returnType, genericArguments, delegateName, parameters);
		}

		public static string GetMethodString(MethodInfo methodInfo, string delegateName, params ParameterInfo[] parameters)
		{
			var returnType = methodInfo.ReturnType;
			var genericArguments = methodInfo.GetGenericArguments();

			return GetMethodString(returnType, genericArguments, delegateName, parameters);
		}

		public static string GetMethodString(Type returnType, Type[] genericArguments, string delegateName, ParameterInfo[] parameters)
		{
			var genericParametersString = GetGenericParametersString(genericArguments);
			var parametersString = GetParametersString(parameters);

			return $"{GetTypeString(returnType)} {delegateName}{genericParametersString}{parametersString}";
		}

		public static string GetGenericParametersString(Type[] genericArguments)
		{
			return genericArguments.Length > 0
				? "<" + string.Join(", ", genericArguments.Select(ga => ga.Name)) + ">"
				: "";
		}

		public static string GetParametersString(ParameterInfo[] parameters, bool withoutType = false)
		{
			var delegateBuilder = new StringBuilder();

			delegateBuilder.Append("(");
			// Добавляем параметры метода в определение делегата
			for (var i = 0; i < parameters.Length; i++)
			{
				var param = parameters[i];
				if (param.IsOut)
					delegateBuilder.Append("out ");
				else if (param.ParameterType.IsByRef)
					delegateBuilder.Append("ref ");
				else if (param.GetCustomAttribute(typeof(System.Runtime.InteropServices.InAttribute)) != null)
					delegateBuilder.Append("in ");

				if (!withoutType)
				{
					var parameterType = param.ParameterType.IsByRef ?
						param.ParameterType.GetElementType() :
						param.ParameterType;
					delegateBuilder.Append(GetTypeString(parameterType));
					delegateBuilder.Append(" ");
				}
				delegateBuilder.Append(param.Name);

				if (i < parameters.Length - 1)
					delegateBuilder.Append(", ");
			}

			delegateBuilder.Append(")");

			return delegateBuilder.ToString();
		}

		public static string GetTypeString(Type type)
		{
			if (type.IsGenericType)
			{
				var typeName = new StringBuilder();
				typeName.Append(type.Namespace + ".");
				typeName.Append(type.Name.Substring(0, type.Name.IndexOf('`')));
				typeName.Append("<");
				var genericArguments = type.GetGenericArguments();
				for (var i = 0; i < genericArguments.Length; i++)
				{
					typeName.Append(GetTypeString(genericArguments[i]));
					if (i < genericArguments.Length - 1)
						typeName.Append(", ");
				}

				typeName.Append(">");
				return typeName.ToString().Replace('+', '.'); // Замена для вложенных типов
			}
			else
			{
				var fullName = type.FullName;
				if (fullName == null)
					return type.Name;
				if (fullName.StartsWith(VoidFullName))
					fullName = fullName.Replace(VoidFullName, "void");
				return fullName.Replace('+', '.');
			}
		}
	}
}