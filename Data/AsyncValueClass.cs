using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Sapientia.Extensions;

namespace Sapientia.Data
{
	public class AsyncValueClass<T>
	{
		public T value;

		private int _millisecondsTimeout;

		private volatile int _threadId;
		private volatile int _count;

		public AsyncValueClass(T value, int millisecondsTimeout = 1)
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AsyncValueClassBusyScope<T> GetBusyScope(bool isDisposed = false)
		{
			return new AsyncValueClassBusyScope<T>(this, isDisposed);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AsyncValueClassBusyScope<T> GetBusyScope(out T value, bool isDisposed = false)
		{
			var scope = new AsyncValueClassBusyScope<T>(this, isDisposed);
			value = this.value;
			return scope;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AsyncValueClassAsyncBusyScope<T> GetAsyncBusyScope(bool isDisposed = false)
		{
			return new AsyncValueClassAsyncBusyScope<T>(this, isDisposed);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AsyncValueClassAsyncBusyScope<T> GetAsyncBusyScope(out T value, bool isDisposed = false)
		{
			var scope = new AsyncValueClassAsyncBusyScope<T>(this, isDisposed);
			value = this.value;
			return scope;
		}

		public bool TryGetBusyScope(out AsyncValueClassBusyScope<T> result, bool isDisposed = false)
		{
			if (TrySetBusy())
			{
				result = new AsyncValueClassBusyScope<T>(this, isDisposed);
				return true;
			}

			result = default!;
			return false;
		}

		public bool TryGetBusyScopeAsync(out AsyncValueClassAsyncBusyScope<T> result, bool isDisposed = false)
		{
			if (TrySetBusy())
			{
				result = new AsyncValueClassAsyncBusyScope<T>(this, isDisposed);
				return true;
			}

			result = default!;
			return false;
		}
	}

	public ref struct AsyncValueClassBusyScope<T>
	{
		private bool _isDisposed;
		private readonly AsyncValueClass<T> _asyncValue;

		public T Value => _asyncValue.value;

		public AsyncValueClassBusyScope(AsyncValueClass<T> asyncValue, bool isDisposed = false)
		{
			if (!isDisposed)
				asyncValue.SetBusy();
			_asyncValue = asyncValue;
			_isDisposed = isDisposed;
		}

		public void Dispose()
		{
			if (!_isDisposed)
			{
				_asyncValue.SetFree();
				_isDisposed = true;
			}
		}
	}

	public struct AsyncValueClassAsyncBusyScope<T> : IDisposable
	{
		private bool _isDisposed;
		private readonly AsyncValueClass<T> _asyncValue;

		public T Value => _asyncValue.value;

		public AsyncValueClassAsyncBusyScope(AsyncValueClass<T> asyncValue, bool isDisposed = false)
		{
			if (!isDisposed)
				asyncValue.SetBusy();
			_asyncValue = asyncValue;
			_isDisposed = isDisposed;
		}

		public void Dispose()
		{
			if (!_isDisposed)
			{
				_asyncValue.SetFree();
				_isDisposed = true;
			}
		}
	}
}