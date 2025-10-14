using System;
using Sapientia.Evaluator.Blackboard;
using Sapientia.Deterministic;
#if CLIENT
using Sirenix.OdinInspector;
#endif

namespace Sapientia.Evaluator.Blackboard
{
#if CLIENT
	[TypeRegistryItem(
		"\u2009Range",
		"/",
		SdfIconType.DiamondHalf,
		darkIconColorR: R, darkIconColorG: G,
		darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G,
		lightIconColorB: B,
		lightIconColorA: A,
		priority: 99
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
}
