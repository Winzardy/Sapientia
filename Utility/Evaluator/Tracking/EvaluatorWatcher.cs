#nullable disable
using System;
using System.Collections.Generic;
using Sapientia.Collections;
using Sapientia.Pooling;
using UnityEngine;

namespace Sapientia.Evaluators.Tracking
{
	public interface IEvaluatorWatcher : IDisposable
	{
	}

	public interface IEvaluatorWatcher<TContext> : IEvaluatorWatcher
	{
		IEvaluatorWatcher<TContext> Root { get; }
		void Reevaluate(TContext context, bool invoke = true);
		bool IsMatch(int? hash);
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

		public IEvaluatorWatcher<TContext> Root { get => this; }

		public EvaluatorWatcher(IEvaluatorTrackingCenter<TContext> center, IEvaluator<TContext, TValue> rootEvaluator)
		{
			_center        = center;
			_rootEvaluator = rootEvaluator;

			using (HashSetPool<ITrackableEvaluator>.Get(out var evaluators))
			using (ListPool<IEvaluatorTracker<TContext>>.Get(out var trackers))
			using (ListPool<EvaluatorWatcherProxy<TContext>>.Get(out var proxies))
			{
				CollectTrackableEvaluators(_rootEvaluator, evaluators);

				_active = !evaluators.IsEmpty();

				if (!_active)
					return;

				foreach (var evaluator in evaluators)
				{
					var trackerType = evaluator.TrackerType;

					var tracker = center.ResolveTracker(trackerType);
					IEvaluatorWatcher<TContext> watcher = this;
					if (evaluator.TrackHash.HasValue)
					{
						var proxy = EvaluatorWatcherProxy<TContext>.New();
						proxy.Bind(this, tracker, evaluator);
						proxies.Add(proxy);

						watcher = proxy;
					}

					tracker.Bind(watcher);

					trackers.Add(tracker);
				}

				_trackers = trackers.ToArray();
				_proxies  = proxies.ToArray();
			}

			_subscriptions = HashSetPool<EvaluatorSubscription<TContext, TValue>>.Get();
		}

		public void Dispose()
		{
			if (!_active)
				return;

			foreach (var proxy in _proxies)
				EvaluatorWatcherProxy<TContext>.Release(proxy);

			_proxies = null;

			foreach (var tracker in _trackers)
				tracker.Unbind(this);

			_trackers = null!;

			StaticObjectPoolUtility.ReleaseAndSetNull(ref _subscriptions);
		}

		public bool IsMatch(int? hash) => !hash.HasValue;

		public void Initialize(TContext context)
		{
			Reevaluate(context, false);
		}

		public void Reevaluate(TContext context, bool invoke = true)
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
			Pool<EvaluatorSubscription<TContext, TValue>>.Release(subscription);
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
}
