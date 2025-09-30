using System;
using Sapientia.Extensions;
#if CLIENT
using Sirenix.OdinInspector;
#endif

namespace Sapientia.BlackboardEvaluator.Converter
{
	[Serializable]
#if CLIENT
	[TypeRegistryItem(
		"\u2009Float To Int",
		"Converter/Math",
		SdfIconType.ArrowLeftRight,
		darkIconColorR: R, darkIconColorG: G,
		darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G,
		lightIconColorB: B,
		lightIconColorA: A
	)]
#endif
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
