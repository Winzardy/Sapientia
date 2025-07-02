using System;
using System.Runtime.CompilerServices;

namespace Sapientia.Data
{
	public class AsyncClass
	{
		private AsyncValue _asyncValue = AsyncValue.Create();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TrySetBusy(bool ignoreThreadId = false)
		{
			return _asyncValue.TrySetBusy(ignoreThreadId);
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
			_asyncValue.SetBusy(ignoreThreadId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetFree(bool ignoreThreadId = false)
		{
			_asyncValue.SetFree(ignoreThreadId);
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
