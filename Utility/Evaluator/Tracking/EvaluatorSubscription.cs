#nullable disable
using System;
using System.Runtime.CompilerServices;
using Sapientia.Pooling;
using UnityEngine;

namespace Sapientia.Evaluators.Tracking
{
	public interface IEvaluatorSubscription : ISubscriptionToken
	{
		Type ContextType { get; }
		void Release(bool solo = true);
		void Reevaluate();
	}

	public class EvaluatorSubscription<TContext, TValue> : IEvaluatorSubscription, IPoolable
	{
		private int _generation;

		private IEvaluatorWatcher<TContext, TValue> _watcher;

		private Action _callback;
		private Action<TValue> _callbackWithValue;

		private Action<EvaluatorSubscription<TContext, TValue>> _onRelease;

		int ISubscriptionToken.Generation { get => _generation; }
		public Type ContextType { get => typeof(TContext); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Invoke(TValue value)
		{
			_callbackWithValue?.Invoke(value);
			_callback?.Invoke();
		}

		internal void Bind(IEvaluatorWatcher<TContext, TValue> watcher, Action<TValue> callback, Action<EvaluatorSubscription<TContext, TValue>> onRelease)
		{
			_callbackWithValue = callback;
			Bind(watcher, onRelease);
		}

		internal void Bind(IEvaluatorWatcher<TContext, TValue> watcher, Action callback, Action<EvaluatorSubscription<TContext, TValue>> onRelease)
		{
			_callback = callback;
			Bind(watcher, onRelease);
		}

		private void Bind(IEvaluatorWatcher<TContext, TValue> watcher, Action<EvaluatorSubscription<TContext, TValue>> onRelease)
		{
			_watcher   = watcher;
			_onRelease = onRelease;

			watcher.Subscribe(this);
		}

		public void Dispose() => Release();

		public void Release(bool solo = true)
		{
			if (solo)
			{
				if (_watcher == null)
					throw new InvalidOperationException($"{typeof(TValue).Name} token already released");

				_watcher.Unsubscribe(this);
			}

			_onRelease.Invoke(this);
		}

		public void Reevaluate() => _watcher.Reevaluate(false);

		void IPoolable.Release()
		{
			_watcher           = null;
			_callback          = null;
			_callbackWithValue = null;
			_onRelease         = null;

			_generation++;
		}

		public static implicit operator EvaluatorSubscriptionToken(EvaluatorSubscription<TContext, TValue> token) => new(token, token._generation);

		internal static EvaluatorSubscription<TContext, TValue> New() => Pool<EvaluatorSubscription<TContext, TValue>>.Get();
		internal static void Release(EvaluatorSubscription<TContext, TValue> subscription) => Pool<EvaluatorSubscription<TContext, TValue>>.Release(subscription);
	}

	public readonly struct EvaluatorSubscriptionToken : IDisposable
	{
		private readonly bool _empty;

		private readonly IEvaluatorSubscription _subscription;
		private readonly int _generation;

		public bool IsValid { get => _subscription != null && _subscription.Generation == _generation; }
		public bool IsEmpty { get => _empty; }

		internal IEvaluatorSubscription Subscription { get => _subscription; }

		public EvaluatorSubscriptionToken(IEvaluatorSubscription subscription, int generation)
		{
			_subscription = subscription;
			_generation   = generation;

			_empty        = false;
		}

		private EvaluatorSubscriptionToken(bool empty)
		{
			_empty        = empty;

			_generation   = 0;
			_subscription = null;
		}

		public void Reevaluate()
		{
			if (_empty)
				return;

			_subscription.Reevaluate();
		}

		public void Dispose()
		{
			if (_empty)
				return;

			Release();
		}

		public void Release()
		{
			if (_empty)
				return;

			if (!IsValid)
				throw new InvalidOperationException($"[EvaluatorSubscriptionToken] Invalid token (token gen:{_subscription?.Generation ?? -1} != gen: {_generation})");

			_subscription.Release();
		}

		public void ReleaseSafe()
		{
			if (!IsValid)
				return;

			Release();
		}

		#region Static

		public static EvaluatorSubscriptionToken Empty = new EvaluatorSubscriptionToken(true);

		public static void ReleaseAndSetNull(ref EvaluatorSubscriptionToken? token)
		{
			token?.Release();
			token = null;
		}

		#endregion
	}

	public static class EvaluatorSubscriptionTokenExtensions
	{
		public static void Reevaluate(this EvaluatorSubscriptionToken? tokenOrNull)
		{
			if (!tokenOrNull.HasValue)
				return;

			tokenOrNull.Value.Reevaluate();
		}

		public static void Release(this ref EvaluatorSubscriptionToken? tokenOrNull)
		{
			if (!tokenOrNull.HasValue)
				return;

			EvaluatorSubscriptionToken.ReleaseAndSetNull(ref tokenOrNull);
		}
	}
}
