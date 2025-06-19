#if NEWTONSOFT
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Content.Management
{
	[JsonConverter(typeof(ContentJsonFormatJsonConverter))]
	public partial class ContentJsonFormat
	{
	}

	internal class ContentJsonFormatJsonConverter : JsonConverter<ContentJsonFormat>
	{
		private const string TYPE_KEY = "$type";

		public override void WriteJson(JsonWriter writer, ContentJsonFormat value, JsonSerializer serializer)
		{
			writer.WriteStartObject();

			foreach (var (moduleName, typeToEntries) in value)
			{
				writer.WritePropertyName(moduleName);
				writer.WriteStartObject();

				foreach (var (typeName, entries) in typeToEntries)
				{
					writer.WritePropertyName(typeName);
					writer.WriteStartObject();

					var declaredType = ContentJsonTypeResolver.Resolve(typeName);

					foreach (var (key, rawValue) in entries)
					{
						writer.WritePropertyName(key);

						if (rawValue == null)
						{
							writer.WriteNull();
							continue;
						}

						var jToken = JToken.FromObject(rawValue, serializer);

						if (jToken is JObject obj)
						{
							var actualType = rawValue.GetType();

							if (declaredType.IsAbstract || declaredType.IsInterface)
							{
								var reordered = new JObject
								{
									[TYPE_KEY] = ContentJsonTypeResolver.ToKey(actualType)
								};

								foreach (var prop in obj.Properties())
									reordered[prop.Name] = prop.Value;

								jToken = reordered;
							}
							else
							{
								obj.Remove(TYPE_KEY);
								jToken = obj;
							}
						}

						jToken.WriteTo(writer);
					}

					writer.WriteEndObject();
				}

				writer.WriteEndObject();
			}

			if (value.guidToId is {Count: > 0})
			{
				writer.WritePropertyName(ContentJsonFormat.IDS_KEY);
				serializer.Serialize(writer, value.guidToId);
			}

			writer.WriteEndObject();
		}

		// public JObject ToJObject(JsonSerializer serializer)
		// {
		// 	foreach (var (typeName, entries) in typeMap)
		// 	{
		// 		var typeObject = new JObject();
		//
		// 		foreach (var (key, rawValue) in entries)
		// 		{
		// 			if (rawValue != null)
		// 			{
		// 				var jToken = JToken.FromObject(rawValue, serializer);
		//
		// 				if (jToken is JObject originalObject)
		// 				{
		// 					var type = ContentJsonTypeResolver.Resolve(typeName);
		// 					if (type.IsAbstract)
		// 					{
		// 						var valueType = rawValue.GetType();
		// 						var reorderedJObject = new JObject
		// 						{
		// 							[TYPE_KEY] = ContentJsonTypeResolver.ToKey(valueType)
		// 						};
		//
		// 						foreach (var property in originalObject.Properties())
		// 							reorderedJObject[property.Name] = property.Value;
		//
		// 						jToken = reorderedJObject;
		// 					}
		// 				}
		//
		// 				typeObject[key] = jToken;
		// 			}
		// 			else
		// 				typeObject[key] = null;
		// 		}
		// 	}
		// }

		public override ContentJsonFormat ReadJson(JsonReader reader, Type objectType, ContentJsonFormat existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			var jObject = JObject.Load(reader);
			var result = new ContentJsonFormat();

			foreach (var prop in jObject.Properties())
			{
				if (prop.Name == ContentJsonFormat.IDS_KEY)
				{
					result.guidToId = prop.Value.ToObject<Dictionary<string, string>>(serializer);
				}
				else
				{
					var module = prop.Value.ToObject<Dictionary<string, Dictionary<string, object>>>(serializer);
					result[prop.Name] = module;
				}
			}

			return result;
		}
	}
}
#endif
