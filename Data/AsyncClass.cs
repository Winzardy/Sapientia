using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Sapientia.Data
{
	public class AsyncClass
	{
		private volatile int _threadId = -1;
		private volatile int _count = 0;

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
		///			new Task(Test2).Start(); // No lock
		///			new Thread(Test2).Start(); // Lock, but not deadlock
		///		}
		///		async.SetFree();
		/// }
		/// </param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetBusy(bool ignoreThreadId = false)
		{
			var currentThreadId = Environment.CurrentManagedThreadId;
			var spin = new SpinWait();

			if (ignoreThreadId)
			{
				var threadId = _threadId;
				while (_count > 0 || Interlocked.CompareExchange(ref _threadId, currentThreadId, threadId) != threadId)
				{
					spin.SpinOnce();
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

					spin.SpinOnce();
				}
			}

			Interlocked.Increment(ref _count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetFree(bool ignoreThreadId = false)
		{
			if (!ignoreThreadId)
				E.ASSERT(_threadId == Environment.CurrentManagedThreadId);
			E.ASSERT(_count > 0);
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
