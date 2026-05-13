using System;
using Sapientia.Collections;
using Sapientia.Pooling;

namespace Sapientia.Evaluators.Tracking
{
	internal class BridgeEvaluatorTracker<TContext1, TContext2, TValue> : EvaluatorTracker<TContext1>
	{
		private HashMap<IEvaluator, EvaluatorSubscriptionToken<TContext2>> _evaluatorToToken;

		protected override void OnInitialized()
		{
			_evaluatorToToken = HashMapPool<IEvaluator, EvaluatorSubscriptionToken<TContext2>>.Get();
		}

		protected override void OnDisposed()
		{
			_evaluatorToToken._values.DisposeElements();
			StaticObjectPoolUtility.ReleaseAndSetNull(ref _evaluatorToToken);
		}

		protected override bool OnBind(IEvaluatorWatcher<TContext1> watcher)
		{
			var evaluator = watcher.BoundEvaluator;
			if (evaluator is IBridgeEvaluator<TContext1, TContext2, TValue> bridge)
			{
				var context2 = bridge.Convert(GetContext());
				_evaluatorToToken[evaluator] = bridge.evaluator.Subscribe(context2, Callback);
				return true;
			}

			throw _center.Logger.Exception("Invalid root evaluator type!");
			void Callback(TValue _) => watcher.Reevaluate(_center.GetContext());
		}

		protected override void OnUnbind(IEvaluatorWatcher<TContext1> watcher)
		{
			var evaluator = watcher.BoundEvaluator;
			if (!_evaluatorToToken.Contains(evaluator))
				return;

			_evaluatorToToken[evaluator].Dispose();
			_evaluatorToToken.Remove(evaluator);
		}
	}
}
