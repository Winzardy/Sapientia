using System;
using Sapientia.Extensions;

namespace Sapientia
{
	[Serializable]
	public class FloatToIntConverter : BlackboardConverter<float, int>
	{
		public ClampType type;

		protected override int Convert(float value)
			=> type switch
			{
				ClampType.Ceil => FloatMathExt.CeilToInt(value),
				ClampType.Floor => FloatMathExt.FloorToInt(value),
				_ => throw new ArgumentOutOfRangeException()
			};
	}
}