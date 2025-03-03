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
		public bool TrySetBusy(bool ignoreThreadId = false)
		{
			if (_count > 0)
			{
				if (ignoreThreadId)
					return false;
				var currentThreadId = Environment.CurrentManagedThreadId;
				if (_threadId != currentThreadId)
					return false;
			}
			else
			{
				var currentThreadId = Environment.CurrentManagedThreadId;
				var threadId = _threadId;
				if (Interlocked.CompareExchange(ref _threadId, currentThreadId, threadId) != threadId)
					return false;
			}

			Interlocked.Increment(ref _count);
			return true;
		}

		/// <param name="ignoreThreadId">
		/// If `true` -> recursive call will lock thread.
		/// Example:
		/// void Test1()
		/// {
		///		async.SetBusy(true);
		///		if (something)
		///			Test1(); // Deadlock
		///		if (somethingElse)
		///		{
		///			new Task(Test1).Start(); // Lock, but not deadlock
		///			new Thread(Test1).Start(); // Lock, but not deadlock
		///		}
		///		async.SetFree();
		/// }
		/// void Test2()
		/// {
		///		async.SetBusy(false);
		///		if (something)
		///			Test2(); // No lock
		///		if (somethingElse)
		///		{
		///			new Task(Test1).Start(); // No lock
		///			new Thread(Test1).Start(); // Lock, but not deadlock
		///		}
		///		async.SetFree();
		/// }
		/// </param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetBusy(bool ignoreThreadId = false)
		{
			var currentThreadId = Environment.CurrentManagedThreadId;
			if (ignoreThreadId)
			{
				var threadId = _threadId;
				while (_count > 0 || Interlocked.CompareExchange(ref _threadId, currentThreadId, threadId) != threadId)
				{
					Thread.Sleep(_millisecondsTimeout);
					threadId = _threadId;
				}
			}
			else
			{
				while (true)
				{
					if (_count == 0)
					{
						var threadId = _threadId;
						if (Interlocked.CompareExchange(ref _threadId, currentThreadId, threadId) == threadId)
							break;
					}
					else if (_threadId == currentThreadId)
						break;

					Thread.Sleep(_millisecondsTimeout);
				}
			}

			Interlocked.Increment(ref _count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetFree()
		{
			Debug.Assert(_threadId == Environment.CurrentManagedThreadId);
			Debug.Assert(_count > 0);
			Interlocked.Decrement(ref _count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AsyncClassBusyScope GetBusyScope(bool ignoreThreadId = false)
		{
			SetBusy(ignoreThreadId);
			return new AsyncClassBusyScope(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AsyncClassAsyncBusyScope GetAsyncBusyScope(bool ignoreThreadId = false)
		{
			SetBusy(ignoreThreadId);
			return new AsyncClassAsyncBusyScope(this);
		}

		public bool TryGetBusyScope(out AsyncClassBusyScope result, bool ignoreThreadId = false)
		{
			if (TrySetBusy(ignoreThreadId))
			{
				result = new AsyncClassBusyScope(this);
				return true;
			}

			result = default!;
			return false;
		}

		public bool TryGetAsyncBusyScope(out AsyncClassAsyncBusyScope result, bool ignoreThreadId = false)
		{
			if (TrySetBusy(ignoreThreadId))
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
