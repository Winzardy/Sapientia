using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Sapientia.Data
{
	[StructLayout(LayoutKind.Explicit)]
	public struct AsyncValue
	{
		[FieldOffset(0)]
		private long _data;
		[FieldOffset(0)]
		private volatile int _threadId;
		[FieldOffset(4)]
		private volatile int _count;

		private static AsyncValue Create(int threadId, int count)
		{
			return new AsyncValue()
			{
				_threadId = threadId,
				_count = count,
			};
		}

		public static AsyncValue Create()
		{
			return new AsyncValue()
			{
				_threadId = -1,
				_count = 0,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TrySetBusy(bool ignoreThreadId = false)
		{
			var currentThreadId = Environment.CurrentManagedThreadId;
			if (ignoreThreadId)
			{
				if (_count != 0)
					return false;
			}
			else if (_count != 0 && _threadId != currentThreadId)
				return false;

			return CompareExchange(currentThreadId, 1, 0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool CompareExchange(int threadId, int count, int comparandCount)
		{
			return CompareExchange(threadId, count, _threadId, comparandCount);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool CompareExchange(int threadId, int count, int comparandThreadId, int comparandCount)
		{
			var newAsyncValue = Create(threadId, count);
			var asyncValue = Create(comparandThreadId, comparandCount);
			return Interlocked.CompareExchange(ref _data, newAsyncValue._data, asyncValue._data) == asyncValue._data;
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
			var spin = new SpinWait();

			var currentThreadId = Environment.CurrentManagedThreadId;
			if (ignoreThreadId)
			{
				while (true)
				{
					if (_count == 0 && CompareExchange(currentThreadId, 1, 0))
						break;
					spin.SpinOnce();
				}
			}
			else
			{
				while (true)
				{
					if (_count == 0)
					{
						if (CompareExchange(currentThreadId, 1, 0))
							break;
					}
					else if (_threadId == currentThreadId && CompareExchange(currentThreadId, _count + 1, currentThreadId, _count))
						break;

					spin.SpinOnce();
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetFree(bool ignoreThreadId = false)
		{
			if (!ignoreThreadId)
				E.ASSERT(_threadId == Environment.CurrentManagedThreadId);
			E.ASSERT(_count > 0);
			Interlocked.Decrement(ref _count);
		}
	}

	public struct AsyncValue<T>
	{
		private AsyncValue _asyncValue;
		public T value;

		public AsyncValue(T value)
		{
			_asyncValue = AsyncValue.Create();
			this.value = value;
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
			return _asyncValue.TrySetBusy();
		}

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
	}
}
