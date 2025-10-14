using System;
using Sapientia.Deterministic;
using Sapientia.Extensions;
#if CLIENT
using Sirenix.OdinInspector;
#endif

namespace Sapientia.Evaluator.Blackboard.Converter
{
	[Serializable]
#if CLIENT
	[TypeRegistryItem(
		"\u2009Float To Int",
		"Convert",
		SdfIconType.ArrowBarRight,
		darkIconColorR: R, darkIconColorG: G,
		darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G,
		lightIconColorB: B,
		lightIconColorA: A
	)]
#endif
	public class FloatToIntConverter : BlackboardConverter<Fix64, int>
	{
		public ClampType type;

		protected override int Convert(Fix64 value)
			=> type switch
			{
				ClampType.Ceil => FloatMathExt.CeilToInt(value),
				ClampType.Floor => FloatMathExt.FloorToInt(value),
				_ => throw new ArgumentOutOfRangeException()
			};
	}
}
