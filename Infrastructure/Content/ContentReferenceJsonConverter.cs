#if NEWTONSOFT
using System;
using Newtonsoft.Json;
using Sapientia.Reflection;

namespace Content
{
	public class ContentReferenceJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(ContentReference<>);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var filed = value!.GetType().GetField("guid");
			var rawGuid = filed?.GetValue(value);
			if (rawGuid is SerializableGuid guid)
			{
				writer.WriteValue(guid == SerializableGuid.Empty ? null : guid.ToString());
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var str = reader.Value?.ToString();
			SerializableGuid.TryParse(str, out var guid);
			return objectType.CreateInstance<IContentReference>(guid);
		}
	}
}
#endif
