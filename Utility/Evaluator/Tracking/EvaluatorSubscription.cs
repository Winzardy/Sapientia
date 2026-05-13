#nullable disable
using System;
using System.Runtime.CompilerServices;
using Sapientia.Pooling;

namespace Sapientia.Evaluators.Tracking
{
	public interface IEvaluatorSubscription<TContext> : ISubscriptionToken
	{
		void Release(bool solo = true);
		void Reevaluate(TContext context);
	}

	public class EvaluatorSubscription<TContext, TValue> : IEvaluatorSubscription<TContext>, IPoolable
	{
		private int _generation;

		private Action<TValue> _callback;
		private IEvaluatorWatcher<TContext, TValue> _watcher;
		private Action<EvaluatorSubscription<TContext, TValue>> _onRelease;

		int ISubscriptionToken.Generation { get => _generation; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Invoke(TValue value) => _callback?.Invoke(value);

		public void Bind(IEvaluatorWatcher<TContext, TValue> watcher, Action<TValue> callback, Action<EvaluatorSubscription<TContext, TValue>> onRelease)
		{
			_watcher  = watcher;
			_callback = callback;

			_onRelease = onRelease;

			watcher.Subscribe(this);
		}

		public void Dispose() => Release();

		public void Release(bool solo = true)
		{
			if (solo)
			{
				if (_watcher == null)
					throw new InvalidOperationException($"{typeof(TValue).Name} Token already released");

				_watcher.Unsubscribe(this);
			}

			_onRelease.Invoke(this);
		}

		public void Reevaluate(TContext context) => _watcher.Reevaluate(context, false);

		void IPoolable.Release()
		{
			_watcher  = null;
			_callback = null;

			_generation++;
		}

		public static implicit operator EvaluatorSubscriptionToken<TContext>(EvaluatorSubscription<TContext, TValue> token) => new(token, token._generation);

		internal static EvaluatorSubscription<TContext, TValue> New() => Pool<EvaluatorSubscription<TContext, TValue>>.Get();
		internal static void Release(EvaluatorSubscription<TContext, TValue> subscription) => Pool<EvaluatorSubscription<TContext, TValue>>.Release(subscription);
	}

	public readonly struct EvaluatorSubscriptionToken<TContext> : IDisposable
	{
		private readonly bool _empty;

		public static EvaluatorSubscriptionToken<TContext> Empty = new EvaluatorSubscriptionToken<TContext>(true);

		private readonly IEvaluatorSubscription<TContext> _token;
		private readonly int _generation;

		internal IEvaluatorSubscription<TContext> Token { get => _token; }
		public bool IsValid { get => _token != null && _token.Generation == _generation; }
		public bool IsEmpty { get => _empty; }

		public EvaluatorSubscriptionToken(IEvaluatorSubscription<TContext> token, int generation)
		{
			_token      = token;
			_generation = generation;
			_empty      = false;
		}

		private EvaluatorSubscriptionToken(bool empty)
		{
			_empty = empty;
			_generation = 0;
			_token = null;
		}

		public void Reevaluate(TContext context)
		{
			if (!_empty)
				_token.Reevaluate(context);
		}

		public void Dispose()
		{
			if (!_empty)
				Release();
		}

		public void Release()
		{
			if (!IsValid)
				throw new InvalidOperationException($"[EvaluatorSubscriptionToken<{typeof(TContext).Name}>] Invalid token (token gen:{_token?.Generation ?? -1} != gen: {_generation})");

			_token.Release();
		}

		public void ReleaseSafe()
		{
			if (!IsValid)
				return;

			Release();
		}

		public static void ReleaseAndSetNull(ref EvaluatorSubscriptionToken<TContext>? token)
		{
			token?.Release();
			token = null;
		}
	}
}
