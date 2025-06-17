using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sapientia.Extensions;
using Sapientia.Pooling;
using Sapientia.Reflection;

namespace Content.Management
{
	public partial class ContentJsonFormat : Dictionary<string, Dictionary<string, Dictionary<string, object>>>
	{
		internal const string IDS_KEY = "$identifiers";
		private static readonly string SINGLE_KEY = "$" + ContentConstants.DEFAULT_SINGLE_ID.ToLower();

		[JsonProperty(IDS_KEY)]
		public Dictionary<string, string> guidToId = new();

		public void Add(string moduleName, IEnumerable<IContentEntry> list)
		{
			var grouped = list
			   .GroupBy(e => ContentJsonTypeResolver.ToKey(e.ValueType))
			   .ToDictionary(
					g => g.Key, // key: тип
					g => g.ToDictionary(
						e =>
						{
							if (e is IUniqueContentEntry unique)
							{
								if (unique.Id != unique.Guid.ToString())
									guidToId[unique.Guid.ToString()] = unique.Id;

								return unique.Guid.ToString();
							}

							return SINGLE_KEY;
						},
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
							guidToId.TryGetValue(guid, out var id);
							unique.Setup(new UniqueContentEntryJsonObject(value, guid, id));
							entry = unique;
						}

						list.Add(entry);
					}
				}
			}

			ContentJsonTypeResolver.Clear();
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
#if CLIENT
			TryFillMap(assemblyNameFilter);
			return _keyToType.GetValueOrDefault(fullTypeKey);
#else
			var (typeName, assemblyName) = Split(fullTypeKey);
			return JsonExt.serializationBinder.BindToType(assemblyName, typeName);
#endif
		}

#if !CLIENT
		private static (string typeName, string assemblyName) Split(string fullTypeKey)
		{
			var parts = fullTypeKey.Split(SEPARATOR);
			return (parts[0], parts[1]);
		}
#endif

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
