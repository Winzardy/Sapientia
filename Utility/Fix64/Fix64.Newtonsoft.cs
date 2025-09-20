#if NEWTONSOFT
using System;
using Newtonsoft.Json;

namespace Sapientia.Deterministic
{
	/// <summary>
	/// Represents a Q31.32 fixed-point number.
	/// </summary>
	[JsonConverter(typeof(Fix64JsonConverter))]
	public partial struct Fix64
	{
	}

	internal class Fix64JsonConverter : JsonConverter<Fix64>
	{
		public override void WriteJson(JsonWriter writer, Fix64 value, JsonSerializer serializer)
		{
			writer.WriteValue((double) value);
		}

		public override Fix64 ReadJson(JsonReader reader, Type objectType, Fix64 existingValue, bool hasExistingValue,
			JsonSerializer serializer)
		{
			return (double) reader.Value;
		}
	}
}
#endif
