using System;
using System.Collections.Generic;
using Sapientia.Conditions;
using UnityEngine;

namespace Sapientia.Evaluators
{
	[Serializable]
	public class IfElseEvaluator<TContext, TValue> : Evaluator<TContext, TValue>
	{
		[SerializeReference]
		public Condition<TContext> condition;

		public EvaluatedValue<TContext, TValue> a = default(TValue);
		public EvaluatedValue<TContext, TValue> b = default(TValue);

		protected override TValue OnEvaluate(TContext context)
		{
			if (condition.IsFulfilled(context))
				return a.Evaluate(context);

			return b.Evaluate(context);
		}

		public override IEnumerator<IEvaluator> GetEnumerator()
		{
			yield return this;
			yield return condition;
			if (!a.IsConstant)
				yield return a.evaluator;
			if (!b.IsConstant)
				yield return b.evaluator;
		}
	}
}
