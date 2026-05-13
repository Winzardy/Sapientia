#nullable disable
using Sapientia.Pooling;

namespace Sapientia.Evaluators.Tracking
{
	public class EvaluatorWatcherProxy<TContext> : IEvaluatorWatcher<TContext>, IPoolable
	{
		private IEvaluatorWatcher<TContext> _watcher;
		private ITrackableEvaluator _evaluator;
		private IEvaluatorTracker<TContext> _tracker;

		public IEvaluatorWatcher<TContext> Root { get => _watcher; }

		public void Bind(IEvaluatorWatcher<TContext> watcher, IEvaluatorTracker<TContext> tracker, ITrackableEvaluator evaluator)
		{
			_tracker   = tracker;
			_evaluator = evaluator;
			_watcher   = watcher;
		}

		public void Dispose() => Release();

		public void Release()
		{
			_tracker.Unbind(this);

			_tracker   = null;
			_watcher   = null;
			_evaluator = null;
		}

		public bool IsMatch(int? hashOrNull)
		{
			if (!hashOrNull.TryGetValue(out var hash))
				return false;

			if (hash != _evaluator.TrackHash)
				return false;

			return true;
		}

		public void Reevaluate(TContext context, bool invoke = true) => _watcher.Reevaluate(context, invoke);

		public static EvaluatorWatcherProxy<TContext> New() => Pool<EvaluatorWatcherProxy<TContext>>.Get();
		public static void Release(EvaluatorWatcherProxy<TContext> proxy) => Pool<EvaluatorWatcherProxy<TContext>>.Release(proxy);
	}
}
