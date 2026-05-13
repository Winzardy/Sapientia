using System;

namespace Sapientia.Evaluators.Tracking
{
	public static class EvaluatorTrackingUtility
	{
		public static EvaluatorSubscriptionToken<TContext> Subscribe<TContext, TValue>(this IEvaluator<TContext, TValue> evaluator, TContext context, Action<TValue> callback, bool invokeOnSubscribe = false)
		{
			var resolver = EvaluatorTrackerResolverRegistry.GetResolverByContext<TContext>();
			var tracker = resolver.ResolveCenter(context);
			if (invokeOnSubscribe)
				callback.Invoke(evaluator.Evaluate(context));
			return tracker.Subscribe(evaluator, callback);
		}

		public static EvaluatorSubscriptionToken<TContext> Subscribe<TContext, TValue>(this EvaluatedValue<TContext, TValue> value, TContext context, Action<TValue> callback, bool invokeOnSubscribe = false)
		{
			if (value.IsConstant)
				return EvaluatorSubscriptionToken<TContext>.Empty;

			return value.evaluator.Subscribe(context, callback,  invokeOnSubscribe);
		}
	}
}
