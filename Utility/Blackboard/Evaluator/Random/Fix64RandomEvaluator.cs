using System;
using Sapientia.BlackboardEvaluator;
using Sapientia.Deterministic;
#if CLIENT
using Sirenix.OdinInspector;
#endif

#if CLIENT
[TypeRegistryItem(
	"\u2009Range",
	"",
	SdfIconType.DiamondHalf,
	darkIconColorR: R, darkIconColorG: G,
	darkIconColorB: B,
	darkIconColorA: A,
	lightIconColorR: R, lightIconColorG: G,
	lightIconColorB: B,
	lightIconColorA: A,
	priority: 100
)]
#endif
[Serializable]
public class Fix64RandomEvaluator : BlackboardRandomEvaluator<Fix64>
{
	public Fix64RandomEvaluator() : this(Fix64.Zero, Fix64.One)
	{
	}

	public Fix64RandomEvaluator(Fix64 min, Fix64 max) : base(min, max)
	{
	}
}
