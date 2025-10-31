using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sapientia.Reflection;

namespace Content.Management
{
	public partial class ContentJsonFormat : Dictionary<string, Dictionary<string, Dictionary<string, object>>>
	{
		internal const string IDS_KEY = "$identifiers";

		private static readonly string SINGLE_KEY = "$" + ContentConstants.DEFAULT_SINGLE_ID
			.ToLower();

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
						e => e.RawValue)
				);

			Add(moduleName, grouped);
		}

		public void Fill(List<IContentEntry> list)
		{
			foreach (var typeToGuidToValue in Values)
			{
				foreach (var (typeKey, keyToValue) in typeToGuidToValue)
				{
					var type = ContentJsonTypeResolver.Resolve(typeKey);

					if (type == null)
						throw ContentDebug.NullException("Type not found: " + typeKey);

					foreach (var (key, rawObject) in keyToValue)
					{
						var value = rawObject;

						if (value == null)
						{
							ContentDebug.LogWarning($"Null value by type [ {type} ] with key [ {key} ]");
						}
						else
						{
							if (value is JToken token)
							{
								value = token.ToObject(type, ContentJsonImporter.defaultSerializer);
								if (value == null)
								{
									ContentDebug.LogError($"Failed to deserialize JObject into [ {type.FullName} ]:\n{token}");
									continue;
								}
							}
							else if (!type.IsAssignableFrom(value.GetType()))
							{
								ContentDebug.LogError(
									$"Invalid value type: expected [ {type.FullName} ], got [ {value.GetType().FullName} ] with key [ {key} ]");
								continue;
							}
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
		}
	}
}
