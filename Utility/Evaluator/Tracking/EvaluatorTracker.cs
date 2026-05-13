using System;
using System.Collections.Generic;
using Sapientia.Pooling;

namespace Sapientia.Evaluators.Tracking
{
	public interface ITrackableEvaluator : IEvaluator
	{
		Type TrackerType { get; }
		int? TrackHash { get; }
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

		void IEvaluatorTracker<TContext>.Initialize(IEvaluatorTrackingCenter<TContext> center)
		{
			_center   = center;
			_watchers = HashSetPool<IEvaluatorWatcher<TContext>>.Get();
			OnInitialized(_center.ResolveContext());
		}

		public void Dispose()
		{
			OnDisposed(_center.ResolveContext());
			StaticObjectPoolUtility.ReleaseAndSetNull(ref _watchers);
		}

		protected virtual void OnInitialized(TContext context)
		{
			OnInitialized();
		}

		protected virtual void OnInitialized()
		{
		}

		protected virtual void OnDisposed(TContext context)
		{
			OnDisposed();
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

		protected ref readonly TContext GetContext() => ref _center.ResolveContext();

		protected void Reevaluate(int? hash = null)
		{
			using (HashSetPool<IEvaluatorWatcher<TContext>>.Get(out var watchers))
			{
				CollectMatchedWatchers(hash, watchers);
				Reevaluate(watchers);
			}
		}

		protected void Reevaluate(List<int> hashes)
		{
			using (HashSetPool<IEvaluatorWatcher<TContext>>.Get(out var roots))
			{
				foreach (var hash in hashes)
					CollectMatchedWatchers(hash, roots);

				Reevaluate(roots);
			}
		}

		private void CollectMatchedWatchers(int? hash, HashSet<IEvaluatorWatcher<TContext>> roots)
		{
			foreach (var watcher in _watchers)
			{
				if (!watcher.IsMatch(hash))
					continue;

				roots.Add(watcher.root);
			}
		}

		private void Reevaluate(HashSet<IEvaluatorWatcher<TContext>> watchers)
		{
			foreach (var watcher in watchers)
				watcher.Reevaluate(_center.ResolveContext());
		}
	}
}
