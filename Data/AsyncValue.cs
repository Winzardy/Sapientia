using System.Runtime.CompilerServices;
using System.Threading;
using Sapientia.Extensions;

namespace Sapientia.Data
{
	public struct AsyncValue<T> where T: unmanaged
	{
		public T value;

		private int _millisecondsTimeout;
		private int _state;

		public State State => _state.ToEnum<State>();

		public AsyncValue(T value, int millisecondsTimeout = 1)
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
	}

	public static class AsyncValueExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static AsyncValueScope<T> GetScope<T>(this ref AsyncValue<T> asyncValue) where T: unmanaged
		{
			return new AsyncValueScope<T>(ref asyncValue);
		}
	}

	public readonly unsafe ref struct AsyncValueScope<T> where T: unmanaged
	{
		private readonly void* _asyncValue;

		public AsyncValueScope(ref AsyncValue<T> asyncValue)
		{
			asyncValue.SetBusy();
#if UNITY_5_3_OR_NEWER
			_asyncValue = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf(ref asyncValue);
#else
			_asyncValue = Unsafe.AsPointer(ref asyncValue);
#endif
		}

		public void Dispose()
		{
			((AsyncValue<T>*)_asyncValue)->SetFree();
		}
	}

	public enum State
	{
		Free,
		Busy,
	}
}