#if NEWTONSOFT
using System;
using Newtonsoft.Json;

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
			var guidCtor = objectType.GetConstructor(new[] {typeof(SerializableGuid), typeof(int)});

			var guid = string.IsNullOrEmpty(str)
				? SerializableGuid.Empty
				: new SerializableGuid(str);

			return guidCtor?.Invoke(new object[] {guid, -1});
		}
	}
}
#endif
