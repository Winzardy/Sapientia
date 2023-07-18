using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sapientia.JsonConverters
{
	public class DictionaryConverter<TKey, TValue> : JsonConverter<Dictionary<TKey, TValue>>
	{
		public override Dictionary<TKey, TValue> ReadJson(JsonReader reader, Type objectType, Dictionary<TKey, TValue> existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			// Convert JSON object to Dictionary<TKey, TValue>
			var jObject = JObject.Load(reader);
			var output = new Dictionary<TKey, TValue>();

			foreach (var item in jObject)
			{
				TKey key;
				using (var textReader = new JsonTextReader(new StringReader(item.Key)))
				{
					key = (TKey)serializer.Deserialize(textReader, typeof(TKey))!;
				}
				var value = item.Value!.ToObject<TValue>(serializer);
				output.Add(key, value);
			}

			return output;
		}

		public override void WriteJson(JsonWriter writer, Dictionary<TKey, TValue> value, JsonSerializer serializer)
		{
			// Convert Dictionary<TKey, TValue> to JSON object
			var jObject = new JObject();

			foreach (var item in value)
			{
				var sb = new StringBuilder(256);
				var sw = new StringWriter(sb, CultureInfo.InvariantCulture);
				using (var jsonWriter = new JsonTextWriter(sw))
				{
					jsonWriter.Formatting = serializer.Formatting;
					serializer.Serialize(jsonWriter, item.Key, typeof(TKey));
				}

				var keyJson = sw.ToString();
				jObject.Add(keyJson, JToken.FromObject(item.Value, serializer));
			}

			jObject.WriteTo(writer);
		}
	}
}