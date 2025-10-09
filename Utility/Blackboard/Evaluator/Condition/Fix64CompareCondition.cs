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
		"\u2009Float Comparison",
		"Common",
		SdfIconType.ArrowLeftRight,
		darkIconColorR: R, darkIconColorG: G, darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G, lightIconColorB: B,
		lightIconColorA: A
	)]
#endif
	public class Fix64CompareCondition : Condition
	{
#if CLIENT
		[HorizontalGroup(GROUP)]
		[HorizontalGroup(GROUP+"/group"), HideLabel]
#endif
		[SerializeReference]
		public BlackboardEvaluator<Fix64> a;

#if CLIENT
		[HorizontalGroup(GROUP+"/group", 120), HideLabel]
#endif
		public ComparisonOperator logicOperator;

#if CLIENT
		[HorizontalGroup(GROUP+"/group"), HideLabel]
#endif
		[SerializeReference]
		public BlackboardEvaluator<Fix64> b;

		protected override bool OnEvaluate(Blackboard blackboard)
		{
			return logicOperator switch
			{
				ComparisonOperator.GreaterOrEqual => a.Get(blackboard) >= b.Get(blackboard),
				ComparisonOperator.LessOrEqual => a.Get(blackboard) <= b.Get(blackboard),
				ComparisonOperator.Greater => a.Get(blackboard) > b.Get(blackboard),
				ComparisonOperator.Less => a.Get(blackboard) < b.Get(blackboard),
				ComparisonOperator.Equal => a.Get(blackboard) == b.Get(blackboard),
				ComparisonOperator.NotEqual => a.Get(blackboard) != b.Get(blackboard),
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
}
