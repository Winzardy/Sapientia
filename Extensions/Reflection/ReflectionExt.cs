using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Sapientia.Extensions.Reflection
{
	/// <summary>
	/// https://www.notion.so/Extension-b985410501c742dabb3a08ca171a319c?pvs=4#242e037edaf64d27ba8bfd71d602528a
	/// </summary>
	public static partial class ReflectionExt
	{
		public const BindingFlags FIELD_BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField;

		public const BindingFlags INTERNAL_FIELD_BINDING_FLAGS = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField;

		public const BindingFlags METHOD_BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		public const BindingFlags OVERRIDEN_METHOD_BINDING_FLAGS = BindingFlags.Instance | BindingFlags.DeclaredOnly;
		public const BindingFlags PRIVATE_METHOD_BINDING_FLAGS = BindingFlags.NonPublic | BindingFlags.Instance;

		public const char PATH_SPLIT_CHAR = '.';
		public const char ARRAY_DATA_TERMINATOR = ']';
		public const string ARRAY_DATA_BEGINNER = "data[";

		private static readonly Dictionary<(Type baseType, bool insertNull, bool includeInterfaces, bool interfacesOnly), Type[]> TYPES = new ();
		private static readonly Dictionary<Type[], Dictionary<string, Type>> NAMES_TO_TYPES = new ();

		public static string GetTypeName(this Type type)
		{
			var name = type.Name;
			var index = name.IndexOf('`');
			return index == -1 ? name : name.Substring(0, index);
		}

		public static bool HasAttribute<T>(this Type type) where T: Attribute
		{
			return type.GetCustomAttribute<T>(true) != null;
		}

		public static bool IsProperty(this MethodInfo methodInfo)
		{
			return methodInfo.IsSpecialName || methodInfo.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length > 0;
		}

		public static bool IsBlittable(this Type type)
		{
			if (!type.IsValueType)
				return false;
			try
			{
				var instance = FormatterServices.GetUninitializedObject(type);
				GCHandle.Alloc(instance, GCHandleType.Pinned).Free();

				return true;
			}
			catch
			{
				return false;
			}
		}

		public static bool IsVoid(this Type type)
		{
			return type == typeof(void);
		}

		public static bool IsList(this Type type)
		{
			if (!type.IsGenericType)
				return false;

			var genericTypeDefinition = type.GetGenericTypeDefinition();
			return genericTypeDefinition == typeof(List<>);
		}

		public static bool InheritsFrom(this Type type, Type baseType)
		{
			return baseType.IsAssignableFrom(type);
		}

		public static Type[] GetInheritorTypes(this Type[] baseTypes, bool insertNull = false, bool includeInterfaces = false, bool interfacesOnly = false)
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			var typeList = new List<Type>();

			for (var a = 0; a < assemblies.Length; a++)
			{
				var types = assemblies[a].GetTypes();
				for (var t = 0; t < types.Length; t++)
				{
					foreach (var baseType in baseTypes)
					{
						if (baseType == types[t])
							goto skip;
						if (!baseType.IsAssignableFrom(types[t]))
							goto skip;
					}

					if ((interfacesOnly && types[t].IsInterface) ||
					    (!interfacesOnly && (includeInterfaces || !types[t].IsInterface) && !types[t].IsAbstract &&
					     !types[t].IsGenericType))
					{
						typeList.Add(types[t]);
					}

					skip: ;
				}
			}

			var typeArray = typeList.ToArray();

			Type[] inheritorTypes;
			if (insertNull)
			{
				inheritorTypes = new Type[typeArray.Length + 1];
				Array.ConstrainedCopy(typeArray, 0, inheritorTypes, 1, typeArray.Length);
			}
			else
			{
				inheritorTypes = typeArray;
			}

			return inheritorTypes;
		}

		public static Dictionary<string, Type> GetNameToInheritorTypes(this Type baseType, bool includeInterfaces = false, bool interfacesOnly = false)
		{
			var types = baseType.GetInheritorTypes(false, includeInterfaces, interfacesOnly);

			if (NAMES_TO_TYPES.TryGetValue(types, out var nameToType))
				return nameToType;
			nameToType = new();
			foreach (var type in types)
			{
				nameToType.Add(type.Name, type);
			}

			NAMES_TO_TYPES[types] = nameToType;
			return nameToType;
		}

		public static Type[] GetInheritorTypes(this Type baseType, bool insertNull = false, bool includeInterfaces = false, bool interfacesOnly = false)
		{
			var key = (baseType, insertNull, includeInterfaces, interfacesOnly);

			if (TYPES.TryGetValue(key, out var inheritorTypes))
				return inheritorTypes;

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			var typeList = new List<Type>();

			for (var a = 0; a < assemblies.Length; a++)
			{
				var types = assemblies[a].GetTypes();
				for (var t = 0; t < types.Length; t++)
				{
					if (baseType == types[t])
						continue;
					if (!baseType.IsAssignableFrom(types[t]))
						continue;

					if ((interfacesOnly && types[t].IsInterface) ||
					    (!interfacesOnly && (includeInterfaces || !types[t].IsInterface) && !types[t].IsAbstract &&
					     !types[t].IsGenericType))
					{
						typeList.Add(types[t]);
					}
				}
			}

			var typeArray = typeList.ToArray();

			if (insertNull)
			{
				inheritorTypes = new Type[typeArray.Length + 1];
				Array.ConstrainedCopy(typeArray, 0, inheritorTypes, 1, typeArray.Length);
			}
			else
			{
				inheritorTypes = typeArray;
			}

			TYPES.Add(key, inheritorTypes);

			return inheritorTypes;
		}

		public static void GetChildrenInterfacesInType(this Type type, Type targetInterface, out List<Type> interfaceList)
		{
			interfaceList = new List<Type>();
			if (type == targetInterface)
				return;
			if (type.GetInterface(targetInterface.Name) == null)
				return;

			foreach (var typeInterface in type.GetInterfaces())
			{
				if (typeInterface == targetInterface)
					continue;
				if (typeInterface.GetInterface(targetInterface.Name) != null)
					interfaceList.Add(typeInterface);
			}
		}

		public static void GetChildrenInterfaces(this Type targetInterface, out List<Type> interfaceList)
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			interfaceList = new List<Type>();

			foreach (var assembly in assemblies)
			{
				var types = assembly.GetTypes();
				foreach (var type in types)
				{
					if (!type.IsInterface)
						continue;
					if (type == targetInterface)
						continue;
					if (type.GetInterface(targetInterface.Name) == null)
						continue;

					foreach (var typeInterface in type.GetInterfaces())
					{
						if (typeInterface == targetInterface)
							continue;
						if (typeInterface.GetInterface(targetInterface.Name) == null)
							goto skip;
					}

					interfaceList.Add(type);
					skip: ;
				}
			}
		}

		public static void GetChildrenTypes(this Type targetInterface, out List<Type> typeList, out List<Type> interfaceList)
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			typeList = new List<Type>();
			interfaceList = new List<Type>();

			foreach (var assembly in assemblies)
			{
				var types = assembly.GetTypes();
				foreach (var type in types)
				{
					if (type == targetInterface)
						continue;
					if (!type.InheritsFrom(targetInterface))
						continue;

					if (type.IsInterface)
						interfaceList.Add(type);
					else
						typeList.Add(type);
				}
			}
		}

		public static List<Type> GetTypesWithInterfacesBetween(this Type fromInterface, Type toInterface)
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			var typeList = new List<Type>();

			foreach (var assembly in assemblies)
			{
				var types = assembly.GetTypes();
				foreach (var type in types)
				{
					if (type.IsInterface)
						continue;

					if (type.GetInterface(fromInterface.Name) == null)
						continue;

					foreach (var typeInterface in type.GetInterfaces())
					{
						if (typeInterface == toInterface)
							continue;
						if (typeInterface.GetInterface(fromInterface.Name) == null)
							continue;
						if (toInterface.GetInterface(typeInterface.Name) == null)
							goto skip;
					}

					typeList.Add(type);

					skip: ;
				}
			}

			return typeList;
		}

		public static void SetObjectByLocalPath(object source, string objectPath, object value)
		{
			var target = source;
			if (string.IsNullOrEmpty(objectPath))
				return;

			var pathComponents = objectPath.Split(PATH_SPLIT_CHAR);

			for (var p = 0; p < pathComponents.Length; p++)
			{
				var pathComponent = pathComponents[p];
				if (target is Array array)
				{
					if (p < pathComponents.Length - 1 && pathComponents[p + 1].StartsWith(ARRAY_DATA_BEGINNER))
					{
						var index = int.Parse(pathComponents[++p].Replace(ARRAY_DATA_BEGINNER, "")
							.Replace($"{ARRAY_DATA_TERMINATOR}", ""));

						if (p + 1 == pathComponents.Length)
						{
							array.SetValue(value, index);
							return;
						}

						target = array.GetValue(index);
					}
				}
				else
				{
					var field = GetAnyField(target.GetType(), pathComponent);

					if (p + 1 == pathComponents.Length)
					{
						field.SetValue(target, value);
						return;
					}

					target = field.GetValue(target);
				}
			}
		}

		public static object GetObjectByLocalPath(object source, string objectPath)
		{
			var target = source;
			if (string.IsNullOrEmpty(objectPath))
				return target;

			var pathComponents = objectPath.Split(PATH_SPLIT_CHAR);

			for (var p = 0; p < pathComponents.Length; p++)
			{
				var pathComponent = pathComponents[p];
				if (target is Array array)
				{
					if (p < pathComponents.Length - 1 && pathComponents[p + 1].StartsWith(ARRAY_DATA_BEGINNER))
					{
						var index = int.Parse(pathComponents[++p].Replace(ARRAY_DATA_BEGINNER, "")
							.Replace($"{ARRAY_DATA_TERMINATOR}", ""));
						target = array.GetValue(index);
					}
				}
				else
				{
					var field = GetAnyField(target.GetType(), pathComponent);
					target = field.GetValue(target);
				}
			}

			return target;
		}

		public static Type GetTypeByLocalPath(object source, string propertyPath)
		{
			return GetObjectByLocalPath(source, propertyPath).GetType();
		}

		public static string GetParentPath(string propertyPath)
		{
			return GetParentPath(propertyPath, out _);
		}

		public static string GetParentPath(string propertyPath, out string localPath)
		{
			var removeIndex = propertyPath.LastIndexOf(PATH_SPLIT_CHAR);
			if (removeIndex >= 0)
			{
				localPath = propertyPath.Remove(0, removeIndex + 1);
				propertyPath = propertyPath.Remove(removeIndex, propertyPath.Length - removeIndex);

				if (localPath[localPath.Length - 1] != ARRAY_DATA_TERMINATOR)
					return propertyPath;

				// Remove "{field name}.Array"
				removeIndex = propertyPath.LastIndexOf(PATH_SPLIT_CHAR);
				localPath = propertyPath.Remove(0, removeIndex + 1) + localPath;
				propertyPath = propertyPath.Remove(removeIndex, propertyPath.Length - removeIndex);

				removeIndex = propertyPath.LastIndexOf(PATH_SPLIT_CHAR);
				if (removeIndex < 0)
					return "";

				localPath = propertyPath.Remove(0, removeIndex + 1) + localPath;
				propertyPath = propertyPath.Remove(removeIndex, propertyPath.Length - removeIndex);

				return propertyPath;
			}

			localPath = propertyPath;
			return "";
		}

		public static FieldInfo GetAnyField(this Type type, string fieldName)
		{
			var field = type.GetField(fieldName, FIELD_BINDING_FLAGS);
			while (field == null)
			{
				type = type.BaseType;
				field = type.GetField(fieldName, INTERNAL_FIELD_BINDING_FLAGS);
			}

			return field;
		}

		public static MethodInfo GetAnyMethod_WithoutArguments(this Type type, string methodName)
		{
			var methodInfo = type.GetMethod(methodName, METHOD_BINDING_FLAGS, null, new Type[] { }, null);
			while (methodInfo == null)
			{
				type = type.BaseType;
				methodInfo = type.GetMethod(methodName, PRIVATE_METHOD_BINDING_FLAGS, null, new Type[] { }, null);
			}

			return methodInfo;
		}

		public static void InvokeMethodByLocalPath(object source, string methodPath)
		{
			var targetPath = "";
			var methodName = methodPath;

			var removeIndex = methodPath.LastIndexOf(PATH_SPLIT_CHAR);
			if (removeIndex >= 0)
			{
				targetPath = methodPath.Remove(removeIndex, methodPath.Length - removeIndex);
				methodName = methodPath.Remove(0, removeIndex + 1);
			}

			var target = GetObjectByLocalPath(source, targetPath);
			var methodInfo = target.GetType().GetAnyMethod_WithoutArguments(methodName);

			methodInfo.Invoke(target, null);
		}

		public static string AppendPath(this string sourcePath, string additionalPath)
		{
			if (string.IsNullOrEmpty(sourcePath))
				return additionalPath;
			if (string.IsNullOrEmpty(additionalPath))
				return sourcePath;

			return sourcePath + PATH_SPLIT_CHAR + additionalPath;
		}

		public static HashSet<MethodInfo> GetAllInstanceMethods(this Type type)
		{
			var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			var allMethods = new HashSet<MethodInfo>();

			foreach (var methodInfo in methods)
			{
				if (methodInfo.IsProperty())
					continue;
				allMethods.Add(methodInfo);
			}

			var interfaces = type.GetInterfaces();
			foreach (var interfaceType in interfaces)
			{
				foreach (var methodInfo in interfaceType.GetMethods())
				{
					if (methodInfo.IsProperty())
						continue;
					allMethods.Add(methodInfo);
				}
			}

			return allMethods;
		}
	}
}
