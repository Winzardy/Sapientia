using System;
using Sapientia.Deterministic;
using Sapientia.Evaluators;
#if CLIENT
using Sirenix.OdinInspector;
using UnityEngine;
#endif

namespace Sapientia.Conditions.Comparison
{
	[Serializable]
	public abstract class Fix64CompareCondition<TContext> : Condition<TContext>
	{
#if CLIENT
		[HorizontalGroup(GROUP)]
		[HorizontalGroup(GROUP + "/group"), HideLabel]
#endif
		[SerializeReference]
		public Evaluator<TContext, Fix64> a;

#if CLIENT
		[HorizontalGroup(GROUP + "/group", OPERATOR_WIDTH), HideLabel]
#endif
		public ComparisonOperator logicOperator;

#if CLIENT
		[HorizontalGroup(GROUP + "/group"), HideLabel]
#endif
		[SerializeReference]
		public Evaluator<TContext, Fix64> b;

		protected override bool OnEvaluate(TContext context)
		{
			return logicOperator switch
			{
				ComparisonOperator.GreaterOrEqual => a.Get(context) >= b.Get(context),
				ComparisonOperator.LessOrEqual => a.Get(context) <= b.Get(context),
				ComparisonOperator.Greater => a.Get(context) > b.Get(context),
				ComparisonOperator.Less => a.Get(context) < b.Get(context),
				ComparisonOperator.Equal => a.Get(context) == b.Get(context),
				ComparisonOperator.NotEqual => a.Get(context) != b.Get(context),
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
}
