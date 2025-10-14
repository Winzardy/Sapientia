using System;
using Sapientia.Evaluators;
#if CLIENT
using Sirenix.OdinInspector;
using UnityEngine;
#endif

namespace Sapientia.Conditions.Comparison
{
	[Serializable]
	public abstract class IntInRangeCondition<TContext> : Condition<TContext>
	{
#if CLIENT
		public const string SELECTOR_NAME = "\u2009Int In Range";
		public const string SELECTOR_CATEGORY = "Comparison";
		public const SdfIconType SELECTOR_ICON = SdfIconType.ArrowsCollapse;
		public const int SELECTOR_PRIORITY = -1;

		[HorizontalGroup(GROUP)]
		[PropertyOrder(1)]
		[HorizontalGroup(GROUP + "/group"), HideLabel]
#endif
		[SerializeReference]
		public Evaluator<TContext, int> min = 0;

#if CLIENT
		[PropertyOrder(2)]
		[HorizontalGroup(GROUP + "/group", 16), HideLabel]
#endif
		public bool exclusiveMin;

#if CLIENT
		[PropertyOrder(3)]
		[DisplayAsString]
		[ShowInInspector]
		[HorizontalGroup(GROUP + "/group", 10), HideLabel]
#endif
		public string minSuffix => exclusiveMin ? "\u2264" : "<";
#if CLIENT
		[PropertyOrder(4)]
		[HorizontalGroup(GROUP + "/group"), HideLabel]
#endif
		[SerializeReference]
		public Evaluator<TContext, int> value;

#if CLIENT
		[PropertyOrder(5)]
		[DisplayAsString]
		[ShowInInspector]
		[HorizontalGroup(GROUP + "/group", 10), HideLabel]
#endif
		public string maxSuffix => exclusiveMax ? "\u2264" : "<";

#if CLIENT
		[PropertyOrder(6)]
		[HorizontalGroup(GROUP + "/group", 16), HideLabel, Tooltip("Включительно")]
#endif
		public bool exclusiveMax;

#if CLIENT
		[PropertyOrder(7)]
		[HorizontalGroup(GROUP + "/group"), HideLabel]
#endif
		[SerializeReference]
		public Evaluator<TContext, int> max = 100;

		protected override bool OnEvaluate(TContext context)
		{
			var v = value.Get(context);
			var a = min.Get(context);
			var b = max.Get(context);
			return (exclusiveMin
					? v >= a
					: v > a)
				&&
				(exclusiveMax
					? v <= b
					: v < b);
		}
	}
}
