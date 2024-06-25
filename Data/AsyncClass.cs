using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Sapientia.Data
{
	public class AsyncClass
	{
		private int _millisecondsTimeout;

		private volatile int _threadId;
		private volatile int _count;

		public AsyncClass(int millisecondsTimeout = 1)
		{
			_millisecondsTimeout = millisecondsTimeout;
			_threadId = -1;
			_count = 0;
		}

		public void SetMillisecondsTimeout(int millisecondsTimeout)
		{
			SetBusy();
			_millisecondsTimeout = millisecondsTimeout;
			SetBusy();
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AsyncClassBusyScope GetBusyScope()
		{
			SetBusy();
			return new AsyncClassBusyScope(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AsyncClassAsyncBusyScope GetAsyncBusyScope()
		{
			SetBusy();
			return new AsyncClassAsyncBusyScope(this);
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

		public bool TryGetAsyncBusyScope(out AsyncClassAsyncBusyScope result)
		{
			if (TrySetBusy())
			{
				result = new AsyncClassAsyncBusyScope(this);
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

	public readonly struct AsyncClassAsyncBusyScope : IDisposable
	{
		private readonly AsyncClass _asyncClass;

		public AsyncClassAsyncBusyScope(AsyncClass asyncClass)
		{
			_asyncClass = asyncClass;
		}

		public void Dispose()
		{
			_asyncClass?.SetFree();
		}
	}
}