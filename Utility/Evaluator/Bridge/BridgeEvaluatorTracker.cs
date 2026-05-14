using Sapientia.Collections;
using Sapientia.Pooling;

namespace Sapientia.Evaluators.Tracking
{
	internal class BridgeEvaluatorTracker<TContext1, TContext2, TValue> : EvaluatorTracker<TContext1>
	{
		private HashMap<IEvaluatorWatcher, EvaluatorSubscriptionToken> _evaluatorToToken;

		protected override void OnInitialized()
		{
			_evaluatorToToken = HashMapPool<IEvaluatorWatcher, EvaluatorSubscriptionToken>.Get();
		}

		protected override void OnDisposed()
		{
			_evaluatorToToken._values.DisposeElements();
			StaticObjectPoolUtility.ReleaseAndSetNull(ref _evaluatorToToken);
		}

		protected override void OnBind(IEvaluatorWatcher<TContext1> rawWatcher)
		{
			if (rawWatcher is BridgeEvaluatorWatcher<TContext1> {Bridge: IBridgeEvaluator<TContext1, TContext2, TValue> bridge})
			{
				var context2 = bridge.Convert(context);
				var token = bridge.evaluator.Subscribe(context2, rawWatcher.Reevaluate);

				if (token.IsEmpty)
					return;

				_evaluatorToToken[rawWatcher] = token;
				return;
			}

			throw _center.Logger.Exception($"Invalid watcher type [ {rawWatcher.GetType().Name} ] for bridge evaluator subscription...");
		}

		protected override void OnUnbind(IEvaluatorWatcher<TContext1> watcher)
		{
			if (!_evaluatorToToken.Contains(watcher))
				return;

			_evaluatorToToken[watcher].Dispose();
			_evaluatorToToken.Remove(watcher);
		}
	}
}
