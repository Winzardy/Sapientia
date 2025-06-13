using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sapientia.Pooling;
using Sapientia.Reflection;

namespace Content.Management
{
	public class ContentJsonFormat : Dictionary<string, Dictionary<string, Dictionary<string, object>>>
	{
		private const string TYPE_KEY = "$type";
		private static readonly string SINGLE_KEY = "$" + ContentConstants.DEFAULT_SINGLE_ID.ToLower();

		public void Add(string moduleName, IEnumerable<IContentEntry> list)
		{
			var grouped = list
			   .GroupBy(e => ContentJsonTypeResolver.ToKey(e.ValueType))
			   .ToDictionary(
					g => g.Key, // key: тип
					g => g.ToDictionary(
						e => e is IUniqueContentEntry unique
							? unique.Guid.ToString()
							: SINGLE_KEY,
						e => e.RawValue
					)
				);

			Add(moduleName, grouped);
		}

		public void Fill(List<IContentEntry> list)
		{
			using var _ = HashSetPool<string>.Get(out var assmeblyFilter);
			foreach (var typeToGuidToValue in Values)
			{
				foreach (var typeKey in typeToGuidToValue.Keys)
				{
					var assemblyName = ContentJsonTypeResolver.ToAssemblyName(typeKey);
					assmeblyFilter.Add(assemblyName);
				}
			}

			foreach (var typeToGuidToValue in Values)
			{
				foreach (var (typeKey, keyToValue) in typeToGuidToValue)
				{
					var type = ContentJsonTypeResolver.Resolve(typeKey, assmeblyFilter);

					if (type == null)
						throw ContentDebug.NullException("Type not found: " + typeKey);

					foreach (var (key, rawObject) in keyToValue)
					{
						var value = rawObject;

						if (value == null)
						{
							ContentDebug.LogError($"Can't to parse value by type [ {type} ]");
							continue;
						}

						if (value is JObject jObject)
						{
							value = jObject.ToObject(type, ContentJsonImporter.defaultSerializer);
							if (value == null)
							{
								ContentDebug.LogError($"Failed to deserialize JObject into [ {type.FullName} ]:\n{jObject}");
								continue;
							}
						}
						else if (!type.IsAssignableFrom(value.GetType()))
						{
							ContentDebug.LogError($"Invalid value type: expected [ {type.FullName} ], got [ {value.GetType().FullName} ]");
							continue;
						}

						IContentEntry entry;

						if (key == SINGLE_KEY)
						{
							var wrapperType = typeof(SingleContentEntry<>).MakeGenericType(type);
							var single = wrapperType.CreateInstance<ISingleContentEntry>();
							single.Setup(new ContentEntryJsonObject(value));
							entry = single;
						}
						else
						{
							var wrapperType = typeof(UniqueContentEntry<>).MakeGenericType(type);
							var guid = SerializableGuid.Parse(key);
							var unique = wrapperType.CreateInstance<IUniqueContentEntry>();
							unique.Setup(new UniqueContentEntryJsonObject(value, guid));
							entry = unique;
						}

						list.Add(entry);
					}
				}
			}

			ContentJsonTypeResolver.Clear();
		}

		public JObject ToJObject(JsonSerializer serializer)
		{
			var root = new JObject();
			foreach (var (moduleName, typeMap) in this)
			{
				var moduleObject = new JObject();

				foreach (var (typeName, entries) in typeMap)
				{
					var typeObject = new JObject();

					foreach (var (key, rawValue) in entries)
					{
						if (rawValue != null)
						{
							var jToken = JToken.FromObject(rawValue, serializer);

							if (jToken is JObject originalObject)
							{
								var type = ContentJsonTypeResolver.Resolve(typeName);
								if (type.IsAbstract)
								{
									var valueType = rawValue.GetType();
									var reorderedJObject = new JObject
									{
										[TYPE_KEY] = ContentJsonTypeResolver.ToKey(valueType)
									};

									foreach (var property in originalObject.Properties())
										reorderedJObject[property.Name] = property.Value;

									jToken = reorderedJObject;
								}
							}

							typeObject[key] = jToken;
						}
						else
							typeObject[key] = null;
					}

					moduleObject[typeName] = typeObject;
				}

				root[moduleName] = moduleObject;
			}

			return root;
		}
	}

	public static class ContentJsonTypeResolver
	{
		private const string SEPARATOR = ", ";

		private static Dictionary<string, Type> _keyToType;

		public static string ToKey(Type type) => $"{type.FullName}{SEPARATOR}{ToAssemblyName(type)}";

		public static string ToAssemblyName(string key) => key.Split(SEPARATOR)[1];
		public static string ToAssemblyName(Type type) => ToAssemblyName(type.Assembly);
		public static string ToAssemblyName(Assembly assembly) => assembly.GetName().Name;

		public static Type Resolve(string fullTypeKey, IEnumerable<string> assemblyNameFilter = null)
		{
			TryFillMap(assemblyNameFilter);
			return _keyToType.GetValueOrDefault(fullTypeKey);
		}

		public static void Clear()
		{
			_keyToType?.Clear();
			_keyToType = null;
		}

		private static void TryFillMap(IEnumerable<string> assemblyNameFilter = null)
		{
			if (_keyToType != null)
				return;

			var filterSet = assemblyNameFilter != null
				? new HashSet<string>(assemblyNameFilter)
				: null;

			_keyToType = AppDomain.CurrentDomain
			   .GetAssemblies()
			   .Where(a => filterSet == null || filterSet.Contains(a.GetName().Name))
			   .SelectMany(a =>
				{
					try
					{
						return a.GetTypes();
					}
					catch (ReflectionTypeLoadException e)
					{
						return e.Types.Where(t => t != null)!;
					}
				})
			   .Where(t => t?.FullName != null)
			   .ToDictionary(
					ToKey,
					t => t!);
		}
	}
}
