#if NEWTONSOFT
using System;
using Newtonsoft.Json;
using Sapientia.Extensions;

namespace InAppPurchasing
{
	// ReSharper disable once StructLacksIEquatable.Global
	[JsonConverter(typeof(IAPPlatformEntryJsonConverter))]
	public partial struct IAPBillingEntry
	{
	}

	internal class IAPPlatformEntryJsonConverter : JsonConverter<IAPBillingEntry>
	{
		public override void WriteJson(JsonWriter writer, IAPBillingEntry value, JsonSerializer serializer)
		{
			var str = value.platform.IsNullOrEmpty() ? IAPBillingType.UNDEFINED : value.ToString();
			writer.WriteValue(str);
		}

		public override IAPBillingEntry ReadJson(JsonReader reader, Type objectType, IAPBillingEntry existingValue, bool hasExistingValue,
			JsonSerializer serializer)
		{
			var str = reader.Value?.ToString();
			return string.IsNullOrEmpty(str)
				? new IAPBillingEntry(IAPBillingType.UNDEFINED)
				: new IAPBillingEntry(str);
		}
	}
}
#endif
