using System;
using Sapientia.Comparison;
using Sapientia.Deterministic;
using Sapientia.Evaluators;

#if CLIENT
using UnityEngine.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;
#endif

namespace Sapientia.Conditions.Comparison
{
	[Serializable]
	public abstract class Fix64CompareCondition<TContext> : Condition<TContext>
	{
#if CLIENT
		public const string SELECTOR_NAME = "\u2009Float Comparison";
		public const string SELECTOR_CATEGORY = "Comparison";
		public const SdfIconType SELECTOR_ICON = SdfIconType.ArrowLeftRight;

		[HorizontalGroup(GROUP)]
		[HorizontalGroup(GROUP + "/group"), HideLabel]
#endif
		[SerializeReference]
		public Evaluator<TContext, Fix64> a;

#if CLIENT
		[FormerlySerializedAs("logicOperator")]
		[HorizontalGroup(GROUP + "/group", OPERATOR_WIDTH), HideLabel]
#endif
		public ComparisonOperator @operator;

#if CLIENT
		[HorizontalGroup(GROUP + "/group"), HideLabel]
#endif
		public EvaluatedValue<TContext, Fix64> b;

		protected override bool OnEvaluate(TContext context)
		{
			return a.Evaluate(context)
				.Compare(@operator, b.Evaluate(context));
		}
	}
}
