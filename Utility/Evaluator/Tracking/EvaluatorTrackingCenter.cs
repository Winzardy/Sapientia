#nullable disable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Pooling;
using Sapientia.Reflection;

namespace Sapientia.Evaluators.Tracking
{
	public interface IEvaluatorTrackingCenter<TContext>
	{
		ILogger Logger { get; }

		EvaluatorSubscriptionToken Subscribe<TValue>(IEvaluator<TContext, TValue> evaluator, Action<TValue> callback);
		EvaluatorSubscriptionToken Subscribe<TValue>(IEvaluator<TContext, TValue> evaluator, Action callback);

		#region Internal

		internal IEvaluatorTracker<TContext> ResolveTracker(Type type);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ref readonly TContext ResolveContext();

		#endregion
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
			_watchers.DisposeElements();
			StaticObjectPoolUtility.ReleaseAndSetNull(ref _watchers);
			_trackers.DisposeElements();
			StaticObjectPoolUtility.ReleaseAndSetNull(ref _trackers);
		}

		public EvaluatorSubscriptionToken Subscribe<TValue>(IEvaluator<TContext, TValue> evaluator, Action<TValue> callback)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			if (!TryGetOrCreateWatcher(evaluator, out var watcher))
				return EvaluatorSubscriptionToken.Empty;
			var subscription = EvaluatorSubscription<TContext, TValue>.New();
			subscription.Bind(watcher, callback, ReleaseSubscription);
			return subscription;
		}

		public EvaluatorSubscriptionToken Subscribe<TValue>(IEvaluator<TContext, TValue> evaluator, Action callback)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			if (!TryGetOrCreateWatcher(evaluator, out var watcher))
				return EvaluatorSubscriptionToken.Empty;
			var subscription = EvaluatorSubscription<TContext, TValue>.New();
			subscription.Bind(watcher, callback, ReleaseSubscription);
			return subscription;
		}

		private bool TryGetOrCreateWatcher<TValue>(IEvaluator<TContext, TValue> evaluator, out IEvaluatorWatcher<TContext, TValue> watcher)
		{
			if (!_watchers.TryGetValue(evaluator, out var rawWatcher))
			{
				var newWatcher = new EvaluatorWatcher<TContext, TValue>(this, evaluator);
				newWatcher.Initialize();
				_watchers[evaluator] = newWatcher;

				if (!newWatcher.IsTrackable)
				{
					watcher = null;
					return false;
				}

				watcher = newWatcher;
				return true;
			}

			if (!rawWatcher.IsTrackable)
			{
				watcher = null;
				return false;
			}

			watcher = (IEvaluatorWatcher<TContext, TValue>) rawWatcher;
			return true;
		}

		private static void ReleaseSubscription<TValue>(EvaluatorSubscription<TContext, TValue> subscription)
			=> EvaluatorSubscription<TContext, TValue>.Release(subscription);

		IEvaluatorTracker<TContext> IEvaluatorTrackingCenter<TContext>.ResolveTracker(Type type)
		{
			if (!_trackers.TryGetValue(type, out var tracker))
			{
				tracker = _trackers[type] = type.CreateInstance<IEvaluatorTracker<TContext>>();
				tracker.Initialize(this);
			}

			return tracker;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		ref readonly TContext IEvaluatorTrackingCenter<TContext>.ResolveContext() => ref ResolveContext();

		private ref readonly TContext ResolveContext() => ref _context;
	}
}
