using System;
using Sapientia.Conditions;
using UnityEngine;

namespace Sapientia.Evaluators
{
	[Serializable]
	public class IfElseEvaluator<TContext, TValue> : Evaluator<TContext, TValue>
	{
		[SerializeReference]
		public Condition<TContext> condition;

		[SerializeReference]
		public Evaluator<TContext, TValue> a;

		public EvaluatedValue<TContext, TValue> b = default(TValue);

		protected override TValue OnGet(TContext context)
		{
			if (condition.IsFulfilled(context))
				return a.Get(context);

			return b.Get(context);
		}
	}
}
