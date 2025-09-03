using System;
using System.Collections.Generic;
using System.Linq;

namespace Sapientia.Extensions
{
	public static class EnumTypeExt
	{
		public enum EmptyEnum {}

		public struct EnumCollection
		{
			public readonly Dictionary<int, string> enumNames;
			public readonly Dictionary<string, int> enumValues;

			public EnumCollection(string[] enumNames, Array values)
			{
				this.enumNames = new Dictionary<int, string>(enumNames.Length);
				enumValues = new Dictionary<string, int>(enumNames.Length);
				for (var i = 0; i < enumNames.Length; i++)
				{
					var value = (int)values.GetValue(i);
					this.enumNames.Add(value, enumNames[i]);
					enumValues.Add(enumNames[i], value);
				}
			}

			public string GetName(int value)
			{
				enumNames.TryGetValue(value, out var name);
				return name;
			}
		}

		private static readonly Dictionary<(Type, Type), EnumCollection> _enumCollections = new Dictionary<(Type, Type), EnumCollection>();

		public static EnumCollection GetEnumCollectionFromContext(Type contextType, Type enumContainer)
		{
			var key = (contextType, enumContainer);
			if (!_enumCollections.TryGetValue(key, out var names))
			{
				var enumType = GetEnumTypeFromContext(contextType, enumContainer);
				names = new EnumCollection(Enum.GetNames(enumType), Enum.GetValues(enumType));
				_enumCollections.Add(key, names);
			}
			return names;
		}

		private static Type GetEnumTypeFromContext(Type contextType, Type enumContainer)
		{
			if (!enumContainer.IsGenericType || !enumContainer.IsAssignableFrom(contextType))
				throw new ArgumentException($"Тип {contextType} не является наследником {enumContainer}");

			if (enumContainer.IsInterface)
			{
				// Проверяем интерфейсы
				var evaluatorInterface = contextType.GetInterfaces()
					.FirstOrDefault(i => i.IsGenericType && enumContainer.IsAssignableFrom(i) && i.Name.Remove(i.Name.IndexOf('`')) == enumContainer.Name);

				if (evaluatorInterface != null)
				{
					var arguments = evaluatorInterface.GetGenericArguments();
					foreach (var argument in arguments)
					{
						if (argument.IsEnum)
							return argument;
					}
				}
			}

			// Проверяем базовый класс
			while (contextType != null! && enumContainer.IsAssignableFrom(contextType))
			{
				if (contextType.IsGenericType && contextType.Name.Remove(contextType.Name.IndexOf('`')) == enumContainer.Name)
				{
					var arguments = contextType.GetGenericArguments();
					foreach (var argument in arguments)
					{
						if (argument.IsEnum)
							return argument;
					}
					break;
				}
				contextType = contextType.BaseType!;
			}

			return typeof(EmptyEnum); // Если интерфейс не найден
		}
	}
}
