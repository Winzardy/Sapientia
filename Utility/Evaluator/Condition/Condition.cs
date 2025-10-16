using System;
using Sapientia.Evaluators;

namespace Sapientia.Conditions
{
	[Serializable]
	public abstract partial class Condition<TContext> : Evaluator<TContext, bool>, ICondition<TContext>
	{
		bool IEvaluator<TContext, bool>.Evaluate(TContext context)
			=> IsFulfilled(context);

		public virtual bool IsFulfilled(TContext context) =>
			OnEvaluate(context);
	}
}
