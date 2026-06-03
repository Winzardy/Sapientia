#nullable disable
using Sapientia.Pooling;

namespace Sapientia.Evaluators.Tracking
{
	public class BridgeEvaluatorWatcher<TContext> : IEvaluatorWatcher<TContext>, IPoolable
	{
		private IEvaluatorWatcher<TContext> _watcher;
		private IEvaluatorTracker<TContext> _tracker;
		private IBridgeEvaluator<TContext> _bridge;

		internal IBridgeEvaluator<TContext> Bridge {get  => _bridge;}

		bool IEvaluatorWatcher.IsTrackable { get => _watcher.IsTrackable; }
		IEvaluatorWatcher<TContext> IEvaluatorWatcher<TContext>.parent { get => _watcher; }


		public void Bind(IEvaluatorWatcher<TContext> watcher, IEvaluatorTracker<TContext> tracker, IBridgeEvaluator<TContext> bridge)
		{
			_tracker = tracker;
			_watcher = watcher;
			_bridge  = bridge;
		}

		public void EnableTracking() => _tracker.Bind(this);
		public void DisableTracking() => _tracker.Unbind(this);

		public void Dispose() => Release();

		public void Release()
		{
			_tracker.Unbind(this);

			_tracker = null;
			_watcher = null;
			_bridge  = null;
		}

		public bool IsMatch(int? filterHash) => EvaluatorWatcherUtility.IsMatch(filterHash);
		public void Reevaluate() => _watcher.Reevaluate();
		public void Reevaluate(bool invoke) => _watcher.Reevaluate(invoke);
		public static BridgeEvaluatorWatcher<TContext> New() => Pool<BridgeEvaluatorWatcher<TContext>>.Get();
		public static void Release(BridgeEvaluatorWatcher<TContext> proxy) => Pool<BridgeEvaluatorWatcher<TContext>>.Release(proxy);
	}
}
