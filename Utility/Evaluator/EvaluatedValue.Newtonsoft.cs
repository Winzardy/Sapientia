#if NEWTONSOFT
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sapientia.Evaluators
{
	[JsonConverter(typeof(EvaluatedValueJsonConverter))]
	public partial struct EvaluatedValue<TContext, TValue>
	{
	}

	public class EvaluatedValueJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType) =>
			objectType.IsGenericType &&
			objectType.GetGenericTypeDefinition().Name
				.StartsWith("EvaluatedValue");

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var type = value.GetType();
			var evaluatorField = type.GetField("evaluator");
			var constantField = type.GetField("value");

			var evaluator = evaluatorField?.GetValue(value);
			var constant = constantField?.GetValue(value);

			if (evaluator == null)
				serializer.Serialize(writer, constant, constantField!.FieldType);
			else
				serializer.Serialize(writer, evaluator, evaluatorField!.FieldType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var token = JToken.Load(reader);

			var instance = Activator.CreateInstance(objectType);

			if (token.Type == JTokenType.Null)
				return instance;

			var constantField = objectType
				.GetField("value");
			var evaluatorField = objectType
				.GetField("evaluator");

			if (token is JObject jObject && jObject.ContainsKey("$type"))
			{
				var evalObj = token.ToObject(evaluatorField.FieldType, serializer);
				evaluatorField.SetValue(instance, evalObj);
				return instance;
			}

			var constant = token.ToObject(constantField.FieldType, serializer);
			constantField.SetValue(instance, constant);
			return instance;
		}
	}
}
#endif
