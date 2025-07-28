using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Content
{
	[JsonConverter(typeof(SerializableGuidJsonConverter))]
	[TypeConverter(typeof(SerializableGuidTypeConverter))]
	public partial struct SerializableGuid
	{
	}

	internal class SerializableGuidJsonConverter : JsonConverter<SerializableGuid>
	{
		public override void WriteJson(JsonWriter writer, SerializableGuid value, JsonSerializer serializer)
		{
			writer.WriteValue(value.IsEmpty() ? null : value.ToString());
		}

		public override SerializableGuid ReadJson(JsonReader reader,
			Type _, SerializableGuid __, bool ___,
			JsonSerializer ____)
		{
			var str = reader.Value?.ToString();
			return new SerializableGuid(str);
		}
	}

	public class SerializableGuidTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if (value is string str && Guid.TryParse(str, out var guid))
				return new SerializableGuid(guid);

			return base.ConvertFrom(context, culture, value);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value,
			Type destinationType)
		{
			if (value is SerializableGuid sg && destinationType == typeof(string))
				return sg.guid.ToString();

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
