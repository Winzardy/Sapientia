#if NEWTONSOFT
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sapientia.JsonConverters;
using Sapientia.Reflection;

namespace Sapientia.Collections
{
	[JsonConverter(typeof(HashMapFactoryConverter))]
	public sealed partial class HashMap<TKey, TValue>
	{
	}

	internal class HashMapFactoryConverter : JsonConverter
	{
		public override bool CanConvert(Type _) => true;

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			var objectType = value!.GetType();

			var keyType = objectType.GetGenericArguments()[0];
			var valueType = objectType.GetGenericArguments()[1];

			var converterType = typeof(HashMapConverter<,>).MakeGenericType(keyType, valueType);
			var converter = converterType.CreateInstance<JsonConverter>();
			converter.WriteJson(writer, value, serializer);
		}

		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var keyType = objectType.GetGenericArguments()[0];
			var valueType = objectType.GetGenericArguments()[1];

			var converterType = typeof(HashMapConverter<,>).MakeGenericType(keyType, valueType);
			var converter = converterType.CreateInstance<JsonConverter>();
			return converter.ReadJson(reader, objectType, existingValue, serializer);
		}
	}

	internal class HashMapConverter<TKey, TValue> : JsonConverter<HashMap<TKey, TValue>>
		where TKey : notnull
		where TValue : struct
	{
		public override void WriteJson(JsonWriter writer, HashMap<TKey, TValue>? value, JsonSerializer serializer)
		{
			writer.WriteStartObject();

			if (value != null)
			{
				foreach (var (key, index) in value._keyToIndex)
				{
					var keyString = DictionaryConverter.ToString(key, serializer);
					writer.WritePropertyName(keyString);
					serializer.Serialize(writer, value._values[index]);
				}
			}

			writer.WriteEndObject();
		}


		public override HashMap<TKey, TValue> ReadJson(JsonReader reader, Type objectType, HashMap<TKey, TValue>? existingValue,
			bool hasExistingValue, JsonSerializer serializer)
		{
			var map = new HashMap<TKey, TValue>();
			var dict = serializer.Deserialize<Dictionary<TKey, TValue>>(reader);

			if (dict != null)
				foreach (var kvp in dict)
					map.Add(kvp.Key, kvp.Value);

			return map;
		}
	}
}
#endif
