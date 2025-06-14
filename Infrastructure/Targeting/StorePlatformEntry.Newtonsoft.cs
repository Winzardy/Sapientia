#if NEWTONSOFT
using System;
using Newtonsoft.Json;
using Sapientia.Extensions;

namespace Targeting
{
	// ReSharper disable once StructLacksIEquatable.Global
	[JsonConverter(typeof(StorePlatformEntryJsonConverter))]
	public partial struct StorePlatformEntry
	{
	}

	internal class StorePlatformEntryJsonConverter : JsonConverter<StorePlatformEntry>
	{
		public override void WriteJson(JsonWriter writer, StorePlatformEntry value, JsonSerializer serializer)
		{
			var str = value.store.IsNullOrEmpty() ? StorePlatformType.UNDEFINED : value.ToString();
			writer.WriteValue(str);
		}

		public override StorePlatformEntry ReadJson(JsonReader reader, Type objectType, StorePlatformEntry existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			var str = reader.Value?.ToString();
			return string.IsNullOrEmpty(str)
				? new StorePlatformEntry(StorePlatformType.UNDEFINED)
				: new StorePlatformEntry(str);
		}
	}
}
#endif
