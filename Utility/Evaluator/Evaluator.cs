using System;

namespace Sapientia.Evaluators
{
	[Serializable]
	public abstract partial class Evaluator<TContext, TValue> : IEvaluator<TContext, TValue>
	{
		TValue IEvaluator<TContext, TValue>.Evaluate(TContext context) => Get(context);

		public TValue Get(TContext context) => OnGet(context);

		protected abstract TValue OnGet(TContext context);

		public static implicit operator Evaluator<TContext, TValue>(TValue value)
			=> new ConstantEvaluator<TContext, TValue>(value);
	}
}
