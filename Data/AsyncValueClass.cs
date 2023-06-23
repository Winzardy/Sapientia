using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Sapientia.Extensions;

namespace Sapientia.Data
{
	public class AsyncValueClass<T>
	{
		public T value;

		private int _millisecondsTimeout;
		private int _state;

		public State State => _state.ToEnum<State>();

		public AsyncValueClass(T value, int millisecondsTimeout = 1)
		{
			this.value = value;
			_millisecondsTimeout = millisecondsTimeout;
			_state = (int)State.Free;
		}

		public void SetMillisecondsTimeout(int millisecondsTimeout)
		{
			SetBusy();
			_millisecondsTimeout = millisecondsTimeout;
			SetFree();
		}

		public T ReadValue()
		{
			SetBusy();
			var result = value;
			SetFree();
			return result;
		}

		public void SetValue(in T newValue)
		{
			SetBusy();
			value = newValue;
			SetFree();
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
		public AsyncValueClassBusyScope<T> GetBusyScope()
		{
			return new AsyncValueClassBusyScope<T>(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AsyncValueClassBusyScopeAsync<T> GetBusyScopeAsync()
		{
			return new AsyncValueClassBusyScopeAsync<T>(this);
		}
	}

	public readonly ref struct AsyncValueClassBusyScope<T>
	{
		private readonly AsyncValueClass<T> _asyncValue;

		public AsyncValueClassBusyScope(AsyncValueClass<T> asyncValue)
		{
			asyncValue.SetBusy();
			_asyncValue = asyncValue;
		}

		public void Dispose()
		{
			_asyncValue.SetFree();
		}
	}

	public readonly struct AsyncValueClassBusyScopeAsync<T> : IDisposable
	{
		private readonly AsyncValueClass<T> _asyncValue;

		public AsyncValueClassBusyScopeAsync(AsyncValueClass<T> asyncValue)
		{
			asyncValue.SetBusy();
			_asyncValue = asyncValue;
		}

		public void Dispose()
		{
			_asyncValue.SetFree();
		}
	}
}