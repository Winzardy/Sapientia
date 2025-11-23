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

		public static implicit operator Condition<TContext>(bool value) => value
			? new TrueCondition<TContext>()
			: new FalseCondition<TContext>();
	}

	[Serializable]
	public class TrueCondition<TContext> : Condition<TContext>
	{
		protected override bool OnEvaluate(TContext _) => true;
	}

	[Serializable]
	public class FalseCondition<TContext> : Condition<TContext>
	{
		protected override bool OnEvaluate(TContext _) => false;
	}
}
