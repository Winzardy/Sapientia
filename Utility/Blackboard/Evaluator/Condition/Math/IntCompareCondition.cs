using System;
using Sapientia.BlackboardEvaluator;

#if CLIENT
using Sirenix.OdinInspector;
using UnityEngine;
#endif

namespace Sapientia.Conditions.Common
{
	[Serializable]
#if CLIENT
	[TypeRegistryItem(
		"\u2009Int Compare",
		"Common/Math",
		SdfIconType.ArrowLeftRight,
		darkIconColorR: R, darkIconColorG: G, darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G, lightIconColorB: B,
		lightIconColorA: A
	)]
#endif
	public class IntCompareCondition : Condition
	{
		[SerializeReference]
#if CLIENT
		[HorizontalGroup, HideLabel]
#endif
		public IBlackboardEvaluator<int> a;

#if CLIENT
		[HorizontalGroup(120), HideLabel]
#endif
		public ComparisonOperator logicOperator;

		[SerializeReference]
#if CLIENT
		[HorizontalGroup, HideLabel]
#endif
		public IBlackboardEvaluator<int> b;

		protected override bool OnEvaluate(Blackboard blackboard)
		{
			return logicOperator switch
			{
				ComparisonOperator.GreaterOrEqual => a.Evaluate(blackboard) >= b.Evaluate(blackboard),
				ComparisonOperator.LessOrEqual => a.Evaluate(blackboard) <= b.Evaluate(blackboard),
				ComparisonOperator.Greater => a.Evaluate(blackboard) > b.Evaluate(blackboard),
				ComparisonOperator.Less => a.Evaluate(blackboard) < b.Evaluate(blackboard),
				ComparisonOperator.Equal => a.Evaluate(blackboard) == b.Evaluate(blackboard),
				ComparisonOperator.NotEqual => a.Evaluate(blackboard) != b.Evaluate(blackboard),
				_ => throw new NotImplementedException(),
			};
		}
	}
}
