using System;
using Sapientia.Evaluators;
#if CLIENT
using Sirenix.OdinInspector;
using UnityEngine;
#endif

namespace Sapientia.Conditions.Comparison
{
	[Serializable]
	public abstract class IntCompareCondition<TContext> : Condition<TContext>
	{
#if CLIENT
		public const string SELECTOR_NAME = "\u2009Int Comparison";
		public const string SELECTOR_CATEGORY = "Comparison";
		public const SdfIconType SELECTOR_ICON = SdfIconType.ArrowLeftRight;

		[HorizontalGroup(GROUP)]
		[HorizontalGroup(GROUP + "/group"), HideLabel]
#endif
		[SerializeReference]
		public Evaluator<TContext, int> a;

#if CLIENT
		[HorizontalGroup(GROUP + "/group", OPERATOR_WIDTH), HideLabel]
#endif
		public ComparisonOperator logicOperator;

#if CLIENT
		[HorizontalGroup(GROUP + "/group"), HideLabel]
#endif
		public EvaluatedValue<TContext, int> b;

		protected override bool OnEvaluate(TContext context)
		{
			return logicOperator switch
			{
				ComparisonOperator.GreaterOrEqual => a.Evaluate(context) >= b.Evaluate(context),
				ComparisonOperator.LessOrEqual => a.Evaluate(context) <= b.Evaluate(context),
				ComparisonOperator.Greater => a.Evaluate(context) > b.Evaluate(context),
				ComparisonOperator.Less => a.Evaluate(context) < b.Evaluate(context),
				ComparisonOperator.Equal => a.Evaluate(context) == b.Evaluate(context),
				ComparisonOperator.NotEqual => a.Evaluate(context) != b.Evaluate(context),
				_ => throw new NotImplementedException(),
			};
		}
	}
}
