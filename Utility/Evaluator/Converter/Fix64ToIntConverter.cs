using System;
using Sapientia.Deterministic;
using Sapientia.Extensions;

namespace Sapientia.Evaluators.Converter
{
	[Serializable]
	public class Fix64ToIntConverter<TContext> : Converter<TContext, Fix64, int>
	{
		public ClampType clampType;

		protected override int Convert(Fix64 value)
			=> clampType switch
			{
				ClampType.Ceil => FloatMathExt.CeilToInt(value),
				ClampType.Floor => FloatMathExt.FloorToInt(value),
				_ => throw new ArgumentOutOfRangeException()
			};
	}
}
