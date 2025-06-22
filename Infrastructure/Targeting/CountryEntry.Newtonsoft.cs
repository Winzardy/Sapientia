#if NEWTONSOFT
using System;
using Newtonsoft.Json;
using Sapientia.Extensions;

namespace Targeting
{
	// ReSharper disable once StructLacksIEquatable.Global
	[JsonConverter(typeof(CountryEntryJsonConverter))]
	public partial struct CountryEntry
	{
	}

	internal class CountryEntryJsonConverter : JsonConverter<CountryEntry>
	{
		public override void WriteJson(JsonWriter writer, CountryEntry value, JsonSerializer serializer)
		{
			var str = value.code.IsNullOrEmpty() ? CountryEntry.UNKNOWN : CountryEntryUtility.GetLabel(value);
			writer.WriteValue(str);
		}

		public override CountryEntry ReadJson(JsonReader reader, Type objectType, CountryEntry existingValue, bool hasExistingValue,
			JsonSerializer serializer)
		{
			var str = reader.Value?.ToString();
			var (code, name) = CountryEntryUtility.FromLabel(str);
			return new CountryEntry(code, name);
		}
	}
}
#endif
