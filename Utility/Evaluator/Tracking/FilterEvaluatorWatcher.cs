#nullable disable
using Sapientia.Pooling;

namespace Sapientia.Evaluators.Tracking
{
	public class FilterEvaluatorWatcher<TContext> : IEvaluatorWatcher<TContext>, IPoolable
	{
		private IEvaluatorWatcher<TContext> _watcher;
		private IFilteredTrackableEvaluator _evaluator;
		private IEvaluatorTracker<TContext> _tracker;

		bool IEvaluatorWatcher.IsTrackable { get => _watcher.IsTrackable; }
		IEvaluatorWatcher<TContext> IEvaluatorWatcher<TContext>.parent { get => _watcher; }

		public void Bind(IEvaluatorWatcher<TContext> watcher, IEvaluatorTracker<TContext> tracker, IFilteredTrackableEvaluator evaluator)
		{
			_tracker   = tracker;
			_evaluator = evaluator;
			_watcher   = watcher;
		}

		public void EnableTracking() => _tracker.Bind(this);
		public void DisableTracking() => _tracker.Unbind(this);

		public void Dispose() => Release();

		public void Release()
		{
			_tracker.Unbind(this);

			_tracker   = null;
			_watcher   = null;
			_evaluator = null;
		}

		public bool IsMatch(int? filterHash)
		{
			if (!filterHash.TryGetValue(out var hash))
				return false;

			if (hash != _evaluator.FilterHash)
				return false;

			return true;
		}

		public void Reevaluate() => _watcher.Reevaluate();
		public void Reevaluate(bool invoke) => _watcher.Reevaluate(invoke);

		public static FilterEvaluatorWatcher<TContext> New() => Pool<FilterEvaluatorWatcher<TContext>>.Get();
		public static void Release(FilterEvaluatorWatcher<TContext> proxy) => Pool<FilterEvaluatorWatcher<TContext>>.Release(proxy);
	}
}
