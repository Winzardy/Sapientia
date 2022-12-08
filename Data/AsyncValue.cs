using System.Threading;
using Sapientia.Extensions;

namespace Sapentia.Data
{
	public struct AsyncValue<T>
	{
		public T value;

		private int _millisecondsTimeout;
		private int _state;

		public State State => (State)_state;

		public AsyncValue(T value, int millisecondsTimeout = 1)
		{
			this.value = value;
			_millisecondsTimeout = millisecondsTimeout;
			_state = (int)State.Free;
		}

		public bool TrySetBusy()
		{
			return _state.Interlocked_CompareExchangeIntEnum(State.Busy, State.Free) == State.Free;
		}

		public void SetBusy()
		{
			while (_state.Interlocked_CompareExchangeIntEnum(State.Busy, State.Free) == State.Busy)
			{
				Thread.Sleep(_millisecondsTimeout);
			}
		}

		public void SetFree()
		{
			_state = (int)State.Free;
		}
	}

	public enum State
	{
		Free,
		Busy,
	}
}