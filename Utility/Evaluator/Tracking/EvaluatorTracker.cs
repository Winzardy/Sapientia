using System;
using System.Collections.Generic;
using Sapientia.Pooling;

namespace Sapientia.Evaluators.Tracking
{
	public interface IFilteredTrackableEvaluator : ITrackableEvaluator
	{
		int FilterHash { get; }
	}

	public interface ITrackableEvaluator : IEvaluator
	{
		Type TrackerType { get; }
	}

	public interface IEvaluatorTracker<TContext> : IDisposable
	{
		internal void Initialize(IEvaluatorTrackingCenter<TContext> center);
		internal void Bind(IEvaluatorWatcher<TContext> watcher);
		internal void Unbind(IEvaluatorWatcher<TContext> watcher);
	}

	public abstract class EvaluatorTracker<TContext> : IEvaluatorTracker<TContext>
	{
		protected IEvaluatorTrackingCenter<TContext> _center;
		private HashSet<IEvaluatorWatcher<TContext>> _watchers;

		protected ref readonly TContext context { get => ref _center.ResolveContext(); }

		void IEvaluatorTracker<TContext>.Initialize(IEvaluatorTrackingCenter<TContext> center)
		{
			_center   = center;
			_watchers = HashSetPool<IEvaluatorWatcher<TContext>>.Get();
			OnInitialized();
		}

		public void Dispose()
		{
			OnDisposed();
			StaticObjectPoolUtility.ReleaseAndSetNull(ref _watchers);
		}

		protected virtual void OnInitialized()
		{
		}

		protected virtual void OnDisposed()
		{
		}

		void IEvaluatorTracker<TContext>.Bind(IEvaluatorWatcher<TContext> watcher)
		{
			if (_watchers.Add(watcher))
				OnBind(watcher);
		}

		protected virtual void OnBind(IEvaluatorWatcher<TContext> watcher)
		{
		}

		void IEvaluatorTracker<TContext>.Unbind(IEvaluatorWatcher<TContext> watcher)
		{
			if (_watchers.Remove(watcher))
				OnUnbind(watcher);
		}

		protected virtual void OnUnbind(IEvaluatorWatcher<TContext> watcher)
		{
		}

		protected void Reevaluate(int? filterHash = null)
		{
			using (HashSetPool<IEvaluatorWatcher<TContext>>.Get(out var watchers))
			{
				CollectMatchedWatchers(filterHash, watchers);
				Reevaluate(watchers);
			}
		}

		protected void Reevaluate(List<int> filteredHashes)
		{
			using (HashSetPool<IEvaluatorWatcher<TContext>>.Get(out var roots))
			{
				foreach (var filterHash in filteredHashes)
					CollectMatchedWatchers(filterHash, roots);

				Reevaluate(roots);
			}
		}

		private void CollectMatchedWatchers(int? filterHash, HashSet<IEvaluatorWatcher<TContext>> roots)
		{
			foreach (var watcher in _watchers)
			{
				if (!watcher.IsMatch(filterHash))
					continue;

				roots.Add(watcher.parent);
			}
		}

		private void Reevaluate(HashSet<IEvaluatorWatcher<TContext>> watchers)
		{
			foreach (var watcher in watchers)
				watcher.Reevaluate();
		}
	}
}
