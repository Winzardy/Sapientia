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
		public override void WriteJson(JsonWriter writer, ContentJsonFormat value, JsonSerializer serializer)
		{
			writer.WriteStartObject();

			foreach (var (moduleName, typeMap) in value)
			{
				writer.WritePropertyName(moduleName);
				serializer.Serialize(writer, typeMap);
			}

			if (value.guidToId is {Count: > 0})
			{
				writer.WritePropertyName(ContentJsonFormat.IDS_KEY);
				serializer.Serialize(writer, value.guidToId);
			}

			writer.WriteEndObject();
		}

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
