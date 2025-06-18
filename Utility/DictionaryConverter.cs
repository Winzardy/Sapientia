using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sapientia.Reflection;

namespace Sapientia.JsonConverters
{
	public class DictionaryConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			if (!objectType.IsGenericType || objectType.GetGenericTypeDefinition() != typeof(Dictionary<,>))
				return false;

			var keyType = objectType.GetGenericArguments()[0];
			var isSimpleKey = Type.GetTypeCode(keyType) switch
			{
				TypeCode.String => true,
				TypeCode.Int32 => true,
				TypeCode.Int64 => true,
				TypeCode.UInt32 => true,
				TypeCode.Boolean => true,
				TypeCode.Char => true,
				TypeCode.Byte => true,
				TypeCode.SByte => true,
				TypeCode.Int16 => true,
				TypeCode.UInt16 => true,
				_ => keyType.IsEnum
			};

			return !isSimpleKey;
		}

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			var dictionary = (IDictionary) value!;
			var jObject = new JObject();

			foreach (DictionaryEntry entry in dictionary)
			{
				var key = ToString(entry.Key, serializer);
				jObject.Add(key, JToken.FromObject(entry.Value!, serializer));
			}

			jObject.WriteTo(writer);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var keyType = objectType.GetGenericArguments()[0];
			var valueType = objectType.GetGenericArguments()[1];
			var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
			var result = dictType.CreateInstance<IDictionary>();

			var jObject = JObject.Load(reader);
			foreach (var item in jObject)
			{
				var keyReader = new JsonTextReader(new StringReader($"\"{item.Key}\""));
				keyReader.Read();
				var key = serializer.Deserialize(keyReader, keyType)!;

				var value = item.Value!.ToObject(valueType, serializer);
				result.Add(key, value);
			}

			return result;
		}

		internal static string ToString(object key, JsonSerializer serializer)
		{
			var keyWriter = new StringWriter();
			using var jsonWriter = new JsonTextWriter(keyWriter);
			serializer.Serialize(jsonWriter, key);
			var keyJson = keyWriter.ToString();
			return JToken.Parse(keyJson).ToString(Formatting.None).Trim('"');
		}
	}
}
