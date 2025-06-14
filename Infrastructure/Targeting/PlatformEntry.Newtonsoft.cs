#if NEWTONSOFT
using System;
using Newtonsoft.Json;
using Sapientia.Extensions;

namespace Targeting
{
	// ReSharper disable once StructLacksIEquatable.Global
	[JsonConverter(typeof(PlatformEntryJsonConverter))]
	public partial struct PlatformEntry
	{
	}

	internal class PlatformEntryJsonConverter : JsonConverter<PlatformEntry>
	{
		public override void WriteJson(JsonWriter writer, PlatformEntry value, JsonSerializer serializer)
		{
			var str = value.platform.IsNullOrEmpty() ? PlatformType.UNDEFINED : value.ToString();
			writer.WriteValue(str);
		}

		public override PlatformEntry ReadJson(JsonReader reader, Type objectType, PlatformEntry existingValue, bool hasExistingValue,
			JsonSerializer serializer)
		{
			var str = reader.Value?.ToString();
			return string.IsNullOrEmpty(str)
				? new PlatformEntry(PlatformType.UNDEFINED)
				: new PlatformEntry(str);
		}
	}
}
#endif
