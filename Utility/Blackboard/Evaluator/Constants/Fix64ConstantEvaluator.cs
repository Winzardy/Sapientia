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
		"\u2009Constant",
		"/",
		SdfIconType.DiamondFill,
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
	public class Fix64ConstantEvaluator : BlackboardConstantEvaluator<Fix64>
	{
		public Fix64ConstantEvaluator() : this(Fix64.Zero)
		{
		}

		public Fix64ConstantEvaluator(Fix64 value) : base(value)
		{
		}
	}
}
