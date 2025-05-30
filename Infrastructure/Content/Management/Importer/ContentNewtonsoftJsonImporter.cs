#if NEWTONSOFT
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fusumity.Collections;
using Newtonsoft.Json;
using Sapientia.Extensions;
using Sapientia.Reflection;

namespace Content.Management
{
	public class ContentNewtonsoftJsonImporter : IContentImporter
	{
		public Task<IList<IContentEntry>> ImportAsync(CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public static JsonSerializerSettings Settings =
			new()
			{
				Converters = new List<JsonConverter>
				{
					new ContentReferenceJsonConverter(),
					new SerializableGuidJsonConverter(),
					new SerializableDictionaryJsonConverter(),
					new ContentEntryConverter()
				},
				DefaultValueHandling = DefaultValueHandling.Ignore,
				TypeNameHandling = TypeNameHandling.Auto,
				NullValueHandling = NullValueHandling.Ignore
			};
	}

	public class ContentEntryConverter : JsonConverter
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

			var guid = (string) reader.Value;

			if (guid.IsNullOrEmpty())
				return null;

			if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(ContentEntry<>))
				throw new JsonSerializationException($"Unsupported type: {type}");

			// Создаём экземпляр ContentEntry<T> через рефлексию
			var instance = type.CreateInstance<IUniqueContentEntry>(
				null,
				new SerializableGuid(guid));
			return instance;
		}
	}
}
#endif
