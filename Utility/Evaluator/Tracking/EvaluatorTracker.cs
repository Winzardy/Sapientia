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
		internal bool Bind(IEvaluatorWatcher<TContext> watcher);
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
			OnInitialized(_center.GetContext());
		}

		public void Dispose()
		{
			OnDisposed(_center.GetContext());
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

		bool IEvaluatorTracker<TContext>.Bind(IEvaluatorWatcher<TContext> watcher)
		{
			if (OnBind(watcher))
			{
				_watchers.Add(watcher);
				return true;
			}

			return false;
		}

		protected virtual bool OnBind(IEvaluatorWatcher<TContext> watcher)
		{
			return true;
		}

		void IEvaluatorTracker<TContext>.Unbind(IEvaluatorWatcher<TContext> watcher)
		{
			_watchers?.Remove(watcher);
			OnUnbind(watcher);
		}

		protected virtual void OnUnbind(IEvaluatorWatcher<TContext> watcher)
		{
		}

		protected ref readonly TContext GetContext() => ref _center.GetContext();

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
				watcher.Reevaluate(_center.GetContext());
		}
	}
}
