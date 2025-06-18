#if NEWTONSOFT
using System;
using Newtonsoft.Json;
using Sapientia.Extensions;

namespace Targeting
{
	// ReSharper disable once StructLacksIEquatable.Global
	[JsonConverter(typeof(StorePlatformEntryJsonConverter))]
	public partial struct DistributionEntry
	{
	}

	internal class StorePlatformEntryJsonConverter : JsonConverter<DistributionEntry>
	{
		public override void WriteJson(JsonWriter writer, DistributionEntry value, JsonSerializer serializer)
		{
			var str = value.name.IsNullOrEmpty() ? DistributionType.UNDEFINED : value.ToString();
			writer.WriteValue(str);
		}

		public override DistributionEntry ReadJson(JsonReader reader, Type objectType, DistributionEntry existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			var str = reader.Value?.ToString();
			return string.IsNullOrEmpty(str)
				? new DistributionEntry(DistributionType.UNDEFINED)
				: new DistributionEntry(str);
		}
	}
}
#endif
