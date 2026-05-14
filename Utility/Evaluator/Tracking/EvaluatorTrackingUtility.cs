#nullable disable
using System;

namespace Sapientia.Evaluators.Tracking
{
	public static class EvaluatorTrackingUtility
	{
		public static EvaluatorSubscriptionToken Subscribe<TContext, TValue>(this IEvaluator<TContext, TValue> evaluator, TContext context, Action<TValue> callback, bool invokeOnSubscribe = false)
		{
			if (evaluator == null)
				return EvaluatorSubscriptionToken.Empty;

			var resolver = EvaluatorTrackerResolverRegistry.GetResolverByContext<TContext>();
			var tracker = resolver.ResolveCenter(context);
			if (invokeOnSubscribe)
				callback.Invoke(evaluator.Evaluate(context));
			return tracker.Subscribe(evaluator, callback);
		}

		public static EvaluatorSubscriptionToken Subscribe<TContext, TValue>(this IEvaluator<TContext, TValue> evaluator, TContext context, Action callback, bool invokeOnSubscribe = false)
		{
			if (evaluator == null)
				return EvaluatorSubscriptionToken.Empty;

			var resolver = EvaluatorTrackerResolverRegistry.GetResolverByContext<TContext>();
			var tracker = resolver.ResolveCenter(context);
			if (invokeOnSubscribe)
				callback.Invoke();
			return tracker.Subscribe(evaluator, callback);
		}

		public static EvaluatorSubscriptionToken Subscribe<TContext, TValue>(this EvaluatedValue<TContext, TValue> value, TContext context, Action<TValue> callback, bool invokeOnSubscribe = false)
		{
			if (value.IsConstant)
				return EvaluatorSubscriptionToken.Empty;

			return value.evaluator.Subscribe(context, callback, invokeOnSubscribe);
		}

		public static EvaluatorSubscriptionToken Subscribe<TContext, TValue>(this EvaluatedValue<TContext, TValue> value, TContext context, Action callback, bool invokeOnSubscribe = false)
		{
			if (value.IsConstant)
				return EvaluatorSubscriptionToken.Empty;

			return value.evaluator.Subscribe(context, callback, invokeOnSubscribe);
		}
	}
}
