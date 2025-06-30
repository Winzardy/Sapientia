using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Sapientia.Data
{
	public struct AsyncValue
	{
		private volatile int _threadId;
		private volatile int _count;

		public static AsyncValue Create()
		{
			return new AsyncValue()
			{
				_threadId = -1,
				_count = 0,
			};
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
		public bool TrySetBusyIgnoreThread()
		{
			if (_count == 0)
			{
				Interlocked.Increment(ref _count);
				return true;
			}
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetBusy(bool ignoreThreadId = false)
		{
			var spin = new SpinWait();
			var currentThreadId = Environment.CurrentManagedThreadId;
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
	}

	public struct AsyncValue<T>
	{
		public T value;

		private volatile int _threadId;
		private volatile int _count;

		public AsyncValue(T value)
		{
			this.value = value;

			_threadId = -1;
			_count = 0;
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
			var spin = new SpinWait();
			var currentThreadId = Environment.CurrentManagedThreadId;
			while (_count > 0 && _threadId != currentThreadId)
			{
				spin.SpinOnce();
			}
			Interlocked.Increment(ref _count);
			Interlocked.Exchange(ref _threadId, currentThreadId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetFree()
		{
			E.ASSERT(_threadId == Environment.CurrentManagedThreadId);
			E.ASSERT(_count > 0);
			Interlocked.Decrement(ref _count);
		}
	}
}
