using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sapientia.JsonConverters
{
	public class JsonStringConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			writer.WriteRawValue((string)value!);
		}

		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			return serializer.Deserialize<JRaw>(reader)?.ToString();
		}

		public override bool CanRead => true;

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(string);
		}
	}
}