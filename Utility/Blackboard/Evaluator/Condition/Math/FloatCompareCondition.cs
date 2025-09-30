using System;
using Sapientia.BlackboardEvaluator;
using Sapientia.Deterministic;

#if CLIENT
using Sirenix.OdinInspector;
using UnityEngine;
#endif

namespace Sapientia.Conditions.Common
{
	[Serializable]
#if CLIENT
	[TypeRegistryItem(
		"\u2009Float Compare",
		"Common/Math",
		SdfIconType.ArrowLeftRight,
		darkIconColorR: R, darkIconColorG: G, darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G, lightIconColorB: B,
		lightIconColorA: A
	)]
#endif
	public class FloatCompareCondition : Condition
	{
		[SerializeReference]
		public IBlackboardEvaluator<Fix64> a;

		public ComparisonOperator logicOperator;

		[SerializeReference]
		public IBlackboardEvaluator<Fix64> b;

		protected override bool OnEvaluate(Blackboard context)
		{
			return logicOperator switch
			{
				ComparisonOperator.GreaterOrEqual => a.Evaluate(context) >= b.Evaluate(context),
				ComparisonOperator.LessOrEqual => a.Evaluate(context) <= b.Evaluate(context),
				ComparisonOperator.Greater => a.Evaluate(context) > b.Evaluate(context),
				ComparisonOperator.Less => a.Evaluate(context) < b.Evaluate(context),
				ComparisonOperator.Equal => a.Evaluate(context) == b.Evaluate(context),
				ComparisonOperator.NotEqual => a.Evaluate(context) != b.Evaluate(context),
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
}
