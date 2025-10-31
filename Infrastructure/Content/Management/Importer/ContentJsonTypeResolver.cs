using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sapientia.Extensions;
using Sapientia.Pooling;

namespace Content.Management
{
	public static class ContentJsonTypeResolver
	{
		private const char SEPARATOR = ',';

		// ReSharper disable once InconsistentNaming
		private static ISerializationBinder _serializationBinder => NewtonsoftJsonUtility.JSON_SETTINGS_DEFAULT.SerializationBinder;

		private static readonly Regex _regex = new(
			@",\s*Version=\d+(?:\.\d+){1,3}\s*,\s*Culture=[^,\]]+\s*,\s*PublicKeyToken=[^,\]]+(?:,\s*Retargetable=\w+)?",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		public static string ToKey(Type type)
		{
			_serializationBinder.BindToName(type, out var assemblyName, out var typeName);
			typeName = _regex.Replace(typeName!, string.Empty);
			return $"{typeName}{SEPARATOR} {assemblyName}";
		}

		public static Type Resolve(string key)
		{
			var (typeName, assemblyName) = Split(key);
			return _serializationBinder.BindToType(assemblyName, typeName);
		}

		private static (string typeName, string assemblyName) Split(string fullTypeKey)
		{
			var parts = SmartSplitAllBrackets(fullTypeKey, SEPARATOR);
			return (parts[0], parts[1]);
		}

		private static string[] SmartSplitAllBrackets(string input, char separator)
		{
			using (ListPool<string>.Get(out var result))
			{
				if (string.IsNullOrEmpty(input))
					return Array.Empty<string>();

				using (StackPool<char>.Get(out var stack))
				{
					var startIndex = 0;

					for (var i = 0; i < input.Length; i++)
					{
						var c = input[i];

						if (c is '[' or '<' or '(')
							stack.Push(c);
						else if ((c == ']' && stack.TryPeek(out var o1) && o1 == '[') ||
							(c == '>' && stack.TryPeek(out var o2) && o2 == '<') ||
							(c == ')' && stack.TryPeek(out var o3) && o3 == '('))
							stack.Pop();
						else if (c == separator && stack.Count == 0)
						{
							result.Add(input.Substring(startIndex, i - startIndex).Trim());
							startIndex = i + 1;
						}
					}

					if (startIndex < input.Length)
						result.Add(input[startIndex..].Trim());
				}

				return result.ToArray();
			}
		}
	}
}
