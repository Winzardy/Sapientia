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

		[SerializeReference]
		public Evaluator<TContext, TValue> b = new ConstantEvaluator<TContext, TValue>(default);

		protected override TValue OnGet(TContext context)
		{
			if (condition.IsFulfilled(context))
				return a.Get(context);

			return b.Get(context);
		}
	}
}
