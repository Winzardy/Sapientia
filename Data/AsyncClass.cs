using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Sapientia.Extensions;

namespace Sapientia.Data
{
	public class AsyncClass
	{
		private int _millisecondsTimeout;
		private int _state;

		public State State => _state.ToEnum<State>();

		public AsyncClass(int millisecondsTimeout = 1)
		{
			_millisecondsTimeout = millisecondsTimeout;
			_state = (int)State.Free;
		}

		public void SetMillisecondsTimeout(int millisecondsTimeout)
		{
			using (GetBusyScope())
			{
				_millisecondsTimeout = millisecondsTimeout;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TrySetBusy()
		{
			return _state.Interlocked_CompareExchangeIntEnum(State.Busy, State.Free) == State.Free;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetBusy()
		{
			while (_state.Interlocked_CompareExchangeIntEnum(State.Busy, State.Free) == State.Busy)
			{
				Thread.Sleep(_millisecondsTimeout);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetFree()
		{
			Interlocked.Exchange(ref _state, (int)State.Free);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AsyncClassBusyScope GetBusyScope()
		{
			SetBusy();
			return new AsyncClassBusyScope(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AsyncClassBusyScopeAsync GetBusyScopeAsync()
		{
			SetBusy();
			return new AsyncClassBusyScopeAsync(this);
		}

		public bool TryGetBusyScope(out AsyncClassBusyScope result)
		{
			if (TrySetBusy())
			{
				result = new AsyncClassBusyScope(this);
				return true;
			}

			result = default!;
			return false;
		}

		public bool TryGetBusyScopeAsync(out AsyncClassBusyScopeAsync result)
		{
			if (TrySetBusy())
			{
				result = new AsyncClassBusyScopeAsync(this);
				return true;
			}

			result = default!;
			return false;
		}
	}

	public readonly ref struct AsyncClassBusyScope
	{
		private readonly AsyncClass _asyncClass;

		public AsyncClassBusyScope(AsyncClass asyncClass)
		{
			_asyncClass = asyncClass;
		}

		public void Dispose()
		{
			_asyncClass?.SetFree();
		}
	}

	public readonly struct AsyncClassBusyScopeAsync : IDisposable
	{
		private readonly AsyncClass _asyncClass;

		public AsyncClassBusyScopeAsync(AsyncClass asyncClass)
		{
			_asyncClass = asyncClass;
		}

		public void Dispose()
		{
			_asyncClass?.SetFree();
		}
	}
}