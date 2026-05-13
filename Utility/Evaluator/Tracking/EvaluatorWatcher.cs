#nullable disable
using System;
using System.Collections.Generic;
using Sapientia.Collections;
using Sapientia.Pooling;

namespace Sapientia.Evaluators.Tracking
{
	public interface IEvaluatorWatcher : IDisposable
	{
		bool IsTrackable { get; }
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
		private IEvaluatorTrackingCenter<TContext> _center;
		private IEvaluator<TContext, TValue> _rootEvaluator;

		private bool _hasTrackableEvaluators;
		private bool _trackingEnable;

		private TValue _current;

		private HashSet<EvaluatorSubscription<TContext, TValue>> _subscriptions;
		private IEvaluatorTracker<TContext>[] _trackers;
		private EvaluatorWatcherProxy<TContext>[] _proxies;
		private BridgeEvaluatorWatcherProxy<TContext>[] _bridges;

		public IEvaluator BoundEvaluator { get => _rootEvaluator; }
		public bool IsTrackable { get => _hasTrackableEvaluators; }

		private bool _invoking;
		private HashSet<EvaluatorSubscription<TContext, TValue>> _subscribePending;
		private HashSet<EvaluatorSubscription<TContext, TValue>> _unsubscribePending;

		IEvaluatorWatcher<TContext> IEvaluatorWatcher<TContext>.root { get => this; }

		public EvaluatorWatcher(IEvaluatorTrackingCenter<TContext> center, IEvaluator<TContext, TValue> rootEvaluator)
		{
			_center        = center;
			_rootEvaluator = rootEvaluator;
		}

		public void Initialize()
		{
			using (HashSetPool<ITrackableEvaluator>.Get(out var evaluators))
			using (HashSetPool<IEvaluatorTracker<TContext>>.Get(out var trackers))
			using (ListPool<EvaluatorWatcherProxy<TContext>>.Get(out var proxies))
			using (ListPool<BridgeEvaluatorWatcherProxy<TContext>>.Get(out var bridges))
			{
				CollectTrackableEvaluators(_rootEvaluator, evaluators);

				_hasTrackableEvaluators = !evaluators.IsEmpty();

				if (!_hasTrackableEvaluators)
					return;

				foreach (var evaluator in evaluators)
				{
					var trackerType = evaluator.TrackerType;

					var tracker = _center.ResolveTracker(trackerType);
					if (evaluator is IBridgeEvaluator<TContext> bridgeEvaluator)
					{
						var bridgeProxy = BridgeEvaluatorWatcherProxy<TContext>.New();
						bridgeProxy.Bind(this, tracker, bridgeEvaluator);
						bridges.Add(bridgeProxy);
					}
					else if (evaluator.TrackHash.HasValue)
					{
						var proxy = EvaluatorWatcherProxy<TContext>.New();
						proxy.Bind(this, tracker, evaluator);
						proxies.Add(proxy);
					}
					else
					{
						trackers.Add(tracker);
					}
				}

				_trackers = trackers.ToArray();
				_proxies  = proxies.ToArray();
				_bridges  = bridges.ToArray();
			}

			_subscriptions = HashSetPool<EvaluatorSubscription<TContext, TValue>>.Get();
		}

		public void Dispose()
		{
			if (!_hasTrackableEvaluators)
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

			foreach (var subscription in _subscriptions)
				subscription.Release(false);
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

			EnableInvoking();
			{
				foreach (var subscription in _subscriptions)
					subscription?.Invoke(value);
			}
			DisableInvoking();
		}

		public void Subscribe(EvaluatorSubscription<TContext, TValue> subscription)
		{
			if (!_hasTrackableEvaluators)
				return;

			if (_invoking)
			{
				_unsubscribePending?.Remove(subscription);

				_subscribePending ??= HashSetPool<EvaluatorSubscription<TContext, TValue>>.Get();
				_subscribePending.Add(subscription);
				return;
			}

			_subscriptions.Add(subscription);

			if (_subscriptions.Count == 1)
			{
				var context = _center.ResolveContext();
				Reevaluate(context, false);
				EnableTracking();
			}
		}

		public void Unsubscribe(EvaluatorSubscription<TContext, TValue> subscription)
		{
			if (!_hasTrackableEvaluators)
				return;

			if (_invoking)
			{
				_subscribePending?.Remove(subscription);

				_unsubscribePending ??= HashSetPool<EvaluatorSubscription<TContext, TValue>>.Get();
				_unsubscribePending.Add(subscription);
				return;
			}

			_subscriptions.Remove(subscription);

			if (_subscriptions.Count == 0)
				DisableTracking();
		}

		private void EnableTracking()
		{
			if (_trackingEnable)
				return;

			foreach (var tracker in _trackers)
				tracker.Bind(this);

			foreach (var bridge in _bridges)
				bridge.EnableTracking();

			foreach (var proxy in _proxies)
				proxy.EnableTracking();

			_trackingEnable = true;
		}

		private void DisableTracking()
		{
			if (!_trackingEnable)
				return;

			foreach (var tracker in _trackers)
				tracker.Unbind(this);

			foreach (var bridge in _bridges)
				bridge.DisableTracking();

			foreach (var proxy in _proxies)
				proxy.DisableTracking();

			_trackingEnable = false;
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

		private void EnableInvoking()
		{
			_invoking = true;
		}

		private void DisableInvoking()
		{
			_invoking = false;

			if (_subscribePending != null)
			{
				foreach (var subscription in _subscribePending)
					Subscribe(subscription);

				StaticObjectPoolUtility.ReleaseAndSetNull(ref _subscribePending);
			}

			if (_unsubscribePending != null)
			{
				foreach (var subscription in _unsubscribePending)
					Unsubscribe(subscription);

				StaticObjectPoolUtility.ReleaseAndSetNull(ref _unsubscribePending);
			}
		}
	}

	public static class EvaluatorWatcherUtility
	{
		public static bool IsMatch(int? hash) => !hash.HasValue;
	}
}
