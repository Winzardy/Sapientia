#nullable disable
using Sapientia.Pooling;

namespace Sapientia.Evaluators.Tracking
{
	public class BridgeEvaluatorWatcherProxy<TContext> : IEvaluatorWatcher<TContext>, IPoolable
	{
		private IEvaluatorWatcher<TContext> _watcher;
		private IEvaluatorTracker<TContext> _tracker;
		private IBridgeEvaluator<TContext> _bridge;

		public IEvaluator BoundEvaluator { get => _bridge; }

		IEvaluatorWatcher<TContext> IEvaluatorWatcher<TContext>.root { get => _watcher; }

		public void Bind(IEvaluatorWatcher<TContext> watcher, IEvaluatorTracker<TContext> tracker, IBridgeEvaluator<TContext> bridge)
		{
			_tracker = tracker;
			_watcher = watcher;
			_bridge  = bridge;
		}

		public void Dispose() => Release();

		public void Release()
		{
			_tracker.Unbind(this);

			_tracker = null;
			_watcher = null;
			_bridge  = null;
		}

		public bool IsMatch(int? hash) => EvaluatorWatcherUtility.IsMatch(hash);
		public void Reevaluate(TContext context) => _watcher.Reevaluate(context);
		public void Reevaluate(TContext context, bool invoke) => _watcher.Reevaluate(context, invoke);
		public static BridgeEvaluatorWatcherProxy<TContext> New() => Pool<BridgeEvaluatorWatcherProxy<TContext>>.Get();
		public static void Release(BridgeEvaluatorWatcherProxy<TContext> proxy) => Pool<BridgeEvaluatorWatcherProxy<TContext>>.Release(proxy);
	}
}
