#nullable disable
using System;
using Sapientia.Evaluators;

namespace Sapientia.Conditions
{
	[Serializable]
	public abstract partial class Condition<TContext> : Evaluator<TContext, bool>, ICondition<TContext>
	{
		bool IEvaluator<TContext, bool>.Evaluate(TContext context)
			=> EvaluateInternal(context);

		bool ICondition<TContext>.IsFulfilled(TContext context)
			=> EvaluateInternal(context);

		protected virtual bool EvaluateInternal(TContext context) => OnEvaluate(context);

		public static implicit operator Condition<TContext>(bool value) => value
			? null
			: new FalseCondition<TContext>();
	}

	/// <summary>
	/// Используется в основном для инспектора, так как <c>condition == null</c> и так <c>true</c>
	/// </summary>
	public class TrueCondition<TContext> : Condition<TContext>
	{
		protected override bool OnEvaluate(TContext _) => true;
	}

	[Serializable]
	public class FalseCondition<TContext> : Condition<TContext>
	{
		protected override bool OnEvaluate(TContext _) => false;
	}

	public static class ConditionUtility
	{
		public static bool IsFulfilled<TContext>(this ICondition<TContext> condition, TContext context)
			=> condition == null || condition.Evaluate(context);
	}
}
