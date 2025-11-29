using System;
using Sapientia.Conditions;
using UnityEngine;

namespace Sapientia.Evaluators
{
	[Serializable]
	public class IfElseCondition<TContext> : Condition<TContext>
	{
		[SerializeReference]
		public Condition<TContext> condition;

		[SerializeReference]
		public Condition<TContext> a;

		[SerializeReference]
		public Condition<TContext> b = true;

		protected override bool OnEvaluate(TContext context)
		{
			if (condition.IsFulfilled(context))
				return a.Evaluate(context);

			return b.Evaluate(context);
		}
	}
}
