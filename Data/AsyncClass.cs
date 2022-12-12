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
			_state = (int)State.Free;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AsyncClassBusyScope GetBusyScope()
		{
			return new AsyncClassBusyScope(this);
		}
	}

	public readonly ref struct AsyncClassBusyScope
	{
		private readonly AsyncClass _asyncClass;

		public AsyncClassBusyScope(AsyncClass asyncClass)
		{
			asyncClass.SetBusy();
			_asyncClass = asyncClass;
		}

		public void Dispose()
		{
			_asyncClass.SetFree();
		}
	}
}