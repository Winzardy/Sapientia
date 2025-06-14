#if NEWTONSOFT
using System;
using Newtonsoft.Json;
using Sapientia.Extensions;

namespace InAppPurchasing
{
	// ReSharper disable once StructLacksIEquatable.Global
	[JsonConverter(typeof(IAPPlatformEntryJsonConverter))]
	public partial struct IAPPlatformEntry
	{
	}

	internal class IAPPlatformEntryJsonConverter : JsonConverter<IAPPlatformEntry>
	{
		public override void WriteJson(JsonWriter writer, IAPPlatformEntry value, JsonSerializer serializer)
		{
			var str = value.platform.IsNullOrEmpty() ? IAPPlatformType.UNDEFINED : value.ToString();
			writer.WriteValue(str);
		}

		public override IAPPlatformEntry ReadJson(JsonReader reader, Type objectType, IAPPlatformEntry existingValue, bool hasExistingValue,
			JsonSerializer serializer)
		{
			var str = reader.Value?.ToString();
			return string.IsNullOrEmpty(str)
				? new IAPPlatformEntry(IAPPlatformType.UNDEFINED)
				: new IAPPlatformEntry(str);
		}
	}
}
#endif
