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
	public abstract class Fix64InRangeCondition<TContext> : Condition<TContext>
	{
#if CLIENT
		public const string SELECTOR_NAME = "\u2009Float In Range";
		public const string SELECTOR_CATEGORY = "Comparison";
		public const SdfIconType SELECTOR_ICON = SdfIconType.ArrowsCollapse;
		public const int SELECTOR_PRIORITY = -1;

		[HorizontalGroup(GROUP)]
		[PropertyOrder(1)]
		[HorizontalGroup(GROUP + "/group"), HideLabel]
#endif
		public EvaluatedValue<TContext, Fix64> min = Fix64.Zero;

#if CLIENT
		[PropertyOrder(3)]
		[DisplayAsString]
		[ShowInInspector]
		[HorizontalGroup(GROUP + "/group", 10), HideLabel]
#endif
		public string minSuffix => "<";
#if CLIENT
		[PropertyOrder(4)]
		[HorizontalGroup(GROUP + "/group"), HideLabel]
#endif
		[SerializeReference]
		public Evaluator<TContext, Fix64> value;

#if CLIENT
		[PropertyOrder(5)]
		[DisplayAsString]
		[ShowInInspector]
		[HorizontalGroup(GROUP + "/group", 10), HideLabel]
#endif
		public string maxSuffix => "<";

#if CLIENT
		[PropertyOrder(7)]
		[HorizontalGroup(GROUP + "/group"), HideLabel]
#endif
		public EvaluatedValue<TContext, Fix64> max = Fix64.One;

		protected override bool OnEvaluate(TContext context)
		{
			var v = value.Evaluate(context);
			var a = min.Evaluate(context);
			var b = max.Evaluate(context);
			return v > a && v < b;
		}
	}
}
