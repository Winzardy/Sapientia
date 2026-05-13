#nullable disable
using System;
using System.Collections.Generic;
using Sapientia.Collections;
using Sapientia.Pooling;
using Sapientia.Reflection;

namespace Sapientia.Evaluators.Tracking
{
	public interface IEvaluatorTrackingCenter<TContext>
	{
		ILogger Logger { get; }
		EvaluatorSubscriptionToken<TContext> Subscribe<TValue>(IEvaluator<TContext, TValue> evaluator, Action<TValue> callback);
		internal IEvaluatorTracker<TContext> ResolveTracker(Type type);
	}

	public class EvaluatorTrackingCenter<TContext> : IEvaluatorTrackingCenter<TContext>, IDisposable
	{
		private TContext _context;
		private ILogger _logger;

		private Dictionary<IEvaluator, IEvaluatorWatcher> _watchers;
		private Dictionary<Type, IEvaluatorTracker<TContext>> _trackers;

		public ILogger Logger { get => _logger; }

		public EvaluatorTrackingCenter(TContext context, ILogger logger = null)
		{
			_logger  = logger;
			_context = context;

			_watchers = DictionaryPool<IEvaluator, IEvaluatorWatcher>.Get();
			_trackers = DictionaryPool<Type, IEvaluatorTracker<TContext>>.Get();
		}

		public void Dispose()
		{
			_trackers.DisposeElements();
			StaticObjectPoolUtility.ReleaseAndSetNull(ref _trackers);
			_watchers.DisposeElements();
			StaticObjectPoolUtility.ReleaseAndSetNull(ref _watchers);
		}

		IEvaluatorTracker<TContext> IEvaluatorTrackingCenter<TContext>.ResolveTracker(Type type)
		{
			if (!_trackers.TryGetValue(type, out var tracker))
			{
				tracker = _trackers[type] = type.CreateInstance<IEvaluatorTracker<TContext>>();
				tracker.Initialize(_context);
			}

			return tracker;
		}

		EvaluatorSubscriptionToken<TContext> IEvaluatorTrackingCenter<TContext>.Subscribe<TValue>(IEvaluator<TContext, TValue> evaluator, Action<TValue> callback)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			var subscription = Pool<EvaluatorSubscription<TContext, TValue>>.Get();
			if (!_watchers.TryGetValue(evaluator, out var rawWatcher))
			{
				var watcher = new EvaluatorWatcher<TContext, TValue>(this, evaluator);
				watcher.Initialize(_context);
				_watchers[evaluator] = watcher;

				subscription.Bind(watcher, callback);
			}
			else
			{
				var watcher = (IEvaluatorWatcher<TContext, TValue>) rawWatcher;
				subscription.Bind( watcher, callback);
			}

			return subscription;
		}
	}
}
