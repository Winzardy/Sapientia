using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Sapientia.Data
{
	public struct AsyncValue
	{
		private int _millisecondsTimeout;

		private volatile int _threadId;
		private volatile int _count;

		public AsyncValue(int millisecondsTimeout = 1)
		{
			_millisecondsTimeout = millisecondsTimeout;
			_threadId = -1;
			_count = 0;
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
			var currentThreadId = Environment.CurrentManagedThreadId;
			if (_count == 0 || _threadId == currentThreadId)
			{
				Interlocked.Increment(ref _count);
				Interlocked.Exchange(ref _threadId, currentThreadId);
				return true;
			}
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetBusy()
		{
			var currentThreadId = Environment.CurrentManagedThreadId;
			while (_count > 0 && _threadId != currentThreadId)
			{
				Thread.Sleep(_millisecondsTimeout);
			}
			Interlocked.Increment(ref _count);
			Interlocked.Exchange(ref _threadId, currentThreadId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetFree()
		{
			Debug.Assert(_threadId == Environment.CurrentManagedThreadId);
			Debug.Assert(_count > 0);
			Interlocked.Decrement(ref _count);
		}
	}

	public struct AsyncValue<T>
	{
		public T value;

		private int _millisecondsTimeout;

		private volatile int _threadId;
		private volatile int _count;

		public AsyncValue(T value, int millisecondsTimeout = 1)
		{
			this.value = value;

			_millisecondsTimeout = millisecondsTimeout;
			_threadId = -1;
			_count = 0;
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
			var currentThreadId = Environment.CurrentManagedThreadId;
			if (_count == 0 || _threadId == currentThreadId)
			{
				Interlocked.Increment(ref _count);
				Interlocked.Exchange(ref _threadId, currentThreadId);
				return true;
			}
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetBusy()
		{
			var currentThreadId = Environment.CurrentManagedThreadId;
			while (_count > 0 && _threadId != currentThreadId)
			{
				Thread.Sleep(_millisecondsTimeout);
			}
			Interlocked.Increment(ref _count);
			Interlocked.Exchange(ref _threadId, currentThreadId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetFree()
		{
			Debug.Assert(_threadId == Environment.CurrentManagedThreadId);
			Debug.Assert(_count > 0);
			Interlocked.Decrement(ref _count);
		}
	}
}