using System;
using System.Collections.Generic;
using Sapientia.Pooling;

namespace Sapientia.Evaluators.Tracking
{
	public interface ITrackableEvaluator
	{
		Type TrackerType { get; }
		int? TrackHash { get; }
	}

	public interface IEvaluatorTracker<TContext> : IDisposable
	{
		internal void Initialize(TContext context);
		internal void Bind(IEvaluatorWatcher<TContext> watcher);
		internal void Unbind(IEvaluatorWatcher<TContext> watcher);
	}

	public abstract class EvaluatorTracker<TContext> : IEvaluatorTracker<TContext>
	{
		private TContext _context;
		private HashSet<IEvaluatorWatcher<TContext>> _watchers;

		void IEvaluatorTracker<TContext>.Initialize(TContext context)
		{
			_context  = context;
			_watchers = HashSetPool<IEvaluatorWatcher<TContext>>.Get();
			OnInitialized(context);
		}

		public void Dispose()
		{
			OnDisposed(_context);
			StaticObjectPoolUtility.ReleaseAndSetNull(ref _watchers);
		}

		protected abstract void OnInitialized(TContext context);

		protected virtual void OnDisposed(TContext context)
		{
			OnDisposed();
		}

		protected virtual void OnDisposed()
		{
		}

		void IEvaluatorTracker<TContext>.Bind(IEvaluatorWatcher<TContext> watcher) => _watchers.Add(watcher);
		void IEvaluatorTracker<TContext>.Unbind(IEvaluatorWatcher<TContext> watcher) => _watchers?.Remove(watcher);

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

				roots.Add(watcher.Root);
			}
		}

		private void Reevaluate(HashSet<IEvaluatorWatcher<TContext>> watchers)
		{
			foreach (var watcher in watchers)
				watcher.Reevaluate(_context);
		}
	}
}
