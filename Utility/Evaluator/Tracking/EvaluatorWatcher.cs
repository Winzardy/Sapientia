#nullable disable
using System;
using System.Collections.Generic;
using Sapientia.Collections;
using Sapientia.Pooling;

namespace Sapientia.Evaluators.Tracking
{
	public interface IEvaluatorWatcher : IDisposable
	{
	}

	public interface IEvaluatorWatcher<TContext> : IEvaluatorWatcher
	{
		IEvaluator BoundEvaluator { get; }

		void Reevaluate(TContext context);
		void Reevaluate(TContext context, bool invoke);
		bool IsMatch(int? hash);

		internal IEvaluatorWatcher<TContext> root { get; }
	}

	public interface IEvaluatorWatcher<TContext, TValue> : IEvaluatorWatcher<TContext>
	{
		void Subscribe(EvaluatorSubscription<TContext, TValue> subscription);
		void Unsubscribe(EvaluatorSubscription<TContext, TValue> subscription);
	}

	public class EvaluatorWatcher<TContext, TValue> : IEvaluatorWatcher<TContext, TValue>
	{
		private bool _active;

		private IEvaluatorTrackingCenter<TContext> _center;
		private IEvaluator<TContext, TValue> _rootEvaluator;

		private TValue _current;

		private HashSet<EvaluatorSubscription<TContext, TValue>> _subscriptions;
		private IEvaluatorTracker<TContext>[] _trackers;
		private EvaluatorWatcherProxy<TContext>[] _proxies;
		private BridgeEvaluatorWatcherProxy<TContext>[] _bridges;

		public IEvaluator BoundEvaluator { get => _rootEvaluator; }
		IEvaluatorWatcher<TContext> IEvaluatorWatcher<TContext>.root { get => this; }

		public EvaluatorWatcher(IEvaluatorTrackingCenter<TContext> center, IEvaluator<TContext, TValue> rootEvaluator)
		{
			_center        = center;
			_rootEvaluator = rootEvaluator;
		}

		public void Initialize(TContext context)
		{
			using (HashSetPool<ITrackableEvaluator>.Get(out var evaluators))
			using (ListPool<IEvaluatorTracker<TContext>>.Get(out var trackers))
			using (ListPool<EvaluatorWatcherProxy<TContext>>.Get(out var proxies))
			using (ListPool<BridgeEvaluatorWatcherProxy<TContext>>.Get(out var bridges))
			{
				CollectTrackableEvaluators(_rootEvaluator, evaluators);

				_active = !evaluators.IsEmpty();

				if (!_active)
					return;

				foreach (var evaluator in evaluators)
				{
					var trackerType = evaluator.TrackerType;

					var tracker = _center.ResolveTracker(trackerType);
					IEvaluatorWatcher<TContext> watcher = this;
					if (evaluator is IBridgeEvaluator<TContext> bridgeEvaluator)
					{
						var bridgeProxy = BridgeEvaluatorWatcherProxy<TContext>.New();
						bridgeProxy.Bind(this, tracker, bridgeEvaluator);
						bridges.Add(bridgeProxy);

						watcher = bridgeProxy;
					}
					else if (evaluator.TrackHash.HasValue)
					{
						var proxy = EvaluatorWatcherProxy<TContext>.New();
						proxy.Bind(this, tracker, evaluator);
						proxies.Add(proxy);

						watcher = proxy;
					}

					if (tracker.Bind(watcher))
						trackers.Add(tracker);
				}

				if (trackers.IsEmpty())
				{
					_active = false;

					foreach (var proxy in proxies)
						EvaluatorWatcherProxy<TContext>.Release(proxy);

					foreach (var bridge in bridges)
						BridgeEvaluatorWatcherProxy<TContext>.Release(bridge);

					return;
				}

				_trackers = trackers.ToArray();
				_proxies  = proxies.ToArray();
				_bridges  = bridges.ToArray();
			}

			_subscriptions = HashSetPool<EvaluatorSubscription<TContext, TValue>>.Get();
			Reevaluate(context, false);
		}

		public void Dispose()
		{
			if (!_active)
				return;

			foreach (var proxy in _proxies)
				EvaluatorWatcherProxy<TContext>.Release(proxy);

			_proxies = null;

			foreach (var bridge in _bridges)
				BridgeEvaluatorWatcherProxy<TContext>.Release(bridge);

			_bridges = null;

			foreach (var tracker in _trackers)
				tracker.Unbind(this);

			_trackers = null!;

			StaticObjectPoolUtility.ReleaseAndSetNull(ref _subscriptions);
		}

		public bool IsMatch(int? hash) => EvaluatorWatcherUtility.IsMatch(hash);

		public void Reevaluate(TContext context)
		{
			Reevaluate(context, true);
		}

		public void Reevaluate(TContext context, bool invoke)
		{
			var value = _rootEvaluator.Evaluate(context);

			if (EqualityComparer<TValue>.Default.Equals(value, _current))
				return;

			_current = value;

			if (!invoke)
				return;

			foreach (var subscription in _subscriptions)
				subscription?.Invoke(value);
		}

		public void Subscribe(EvaluatorSubscription<TContext, TValue> subscription)
		{
			if (!_active)
				return;
			_subscriptions.Add(subscription);
		}

		public void Unsubscribe(EvaluatorSubscription<TContext, TValue> subscription)
		{
			if (!_active)
				return;
			_subscriptions.Remove(subscription);
		}

		private static void CollectTrackableEvaluators(
			IEvaluator evaluator,
			HashSet<ITrackableEvaluator> results)
		{
			using (HashSetPool<IEvaluator>.Get(out var visited))
				CollectTrackableEvaluators(evaluator, results, visited);
		}

		private static void CollectTrackableEvaluators(
			IEvaluator evaluator,
			HashSet<ITrackableEvaluator> results,
			HashSet<IEvaluator> visited)
		{
			if (evaluator == null)
				return;

			if (!visited.Add(evaluator))
				return;

			if (evaluator is ITrackableEvaluator trackable)
				results.Add(trackable);

			foreach (var child in evaluator)
			{
				if (child == null || ReferenceEquals(child, evaluator))
					continue;

				CollectTrackableEvaluators(child, results, visited);
			}
		}
	}

	public static class EvaluatorWatcherUtility
	{
		public static bool IsMatch(int? hash) => !hash.HasValue;
	}
}
