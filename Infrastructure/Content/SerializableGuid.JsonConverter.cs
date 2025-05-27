#if NEWTONSOFT
using System;
using Newtonsoft.Json;

namespace Content
{
	public class SerializableGuidJsonConverter : JsonConverter<SerializableGuid>
	{
		public override void WriteJson(JsonWriter writer, SerializableGuid value, JsonSerializer serializer)
		{
			writer.WriteValue(value.guid == SerializableGuid.Empty ? null : value.ToString());
		}

		public override SerializableGuid ReadJson(JsonReader reader,
			Type _, SerializableGuid __, bool ___,
			JsonSerializer ____)
		{
			var str = reader.Value?.ToString();
			return string.IsNullOrEmpty(str) ? SerializableGuid.Empty : new SerializableGuid(str);
		}
	}
}
#endif
