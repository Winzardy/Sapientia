using System;
using Newtonsoft.Json;
using Sapientia.Extensions;
using Sapientia.Reflection;

namespace Content.Management
{
	public class ContentEntryJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type type) => type.IsGenericType &&
			type.GetGenericTypeDefinition() == typeof(ContentEntry<>);

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value is not IUniqueContentEntry entry)
			{
				writer.WriteNull();
				return;
			}

			writer.WriteValue(entry.Guid.ToString());
		}

		public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer _)
		{
			if (reader.TokenType == JsonToken.Null)
				return null;

			if (reader.TokenType != JsonToken.String)
				throw new JsonSerializationException($"Expected string token, got {reader.TokenType}");

			var str = (string) reader.Value;
			if (str.IsNullOrEmpty())
				return null;

			if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(ContentEntry<>))
				throw new JsonSerializationException($"Unsupported type: {type}");

			var guid = SerializableGuid.Parse(str);
			var entry = type.CreateInstance<IUniqueContentEntry>();
			entry.Setup(new UniqueContentEntryJsonObject(guid));
			return entry;
		}
	}
}
