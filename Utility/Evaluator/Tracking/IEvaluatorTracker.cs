using System;

namespace Sapientia.Evaluators.Tracking
{
	public static class EvaluatorTrackingExtensions
	{
		public static EvaluatorSubscriptionToken Subscribe<TContext, TValue>(this IEvaluator<TContext, TValue> evaluator, TContext context, Action callback)
		{
			var resolver = EvaluatorTrackerResolverRegistry.GetResolverByContext<TContext>();
			var tracker = resolver.GetTracker(context);
			return tracker.Subscribe(evaluator, context, callback);
		}
	}

	public interface IEvaluatorTracker<TContext>
	{
		EvaluatorSubscriptionToken Subscribe<TValue>(IEvaluator<TContext, TValue> evaluator, TContext context, Action callback);
	}

	public readonly struct EvaluatorSubscriptionToken : IDisposable
	{
		private readonly IEvaluatorSubscriptionToken _token;
		private readonly int _generation;

		internal IEvaluatorSubscriptionToken Token { get => _token; }
		public bool IsValid { get => _token != null && _token.Generation == _generation; }

		internal EvaluatorSubscriptionToken(IEvaluatorSubscriptionToken token, int generation)
		{
			_token      = token;
			_generation = generation;
		}

		public void Dispose() => Release();

		public void Release()
		{
			if (!IsValid)
				throw new InvalidOperationException($"[{nameof(EvaluatorSubscriptionToken)}] Invalid token (token gen:{_token?.Generation ?? -1} != gen: {_generation})");

			_token.Release();
		}

		public void ReleaseSafe()
		{
			if (!IsValid)
				return;

			Release();
		}

		public static void ReleaseAndSetNull(ref BlackboardToken? token)
		{
			token?.Release();
			token = null;
		}
	}

	internal interface IEvaluatorSubscriptionToken : ISubscriptionToken
	{
		void Release();
	}
}
