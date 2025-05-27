using System;
using System.Runtime.CompilerServices;

namespace Sapientia.Data
{
	public class AsyncValueClass<T> : AsyncClass
	{
		public T value;

		public AsyncValueClass(T value)
		{
			this.value = value;
		}

		public T ReadValue(bool ignoreThreadId = false)
		{
			SetBusy(ignoreThreadId);
			var result = value;
			SetFree();
			return result;
		}

		public void SetValue(in T newValue, bool ignoreThreadId = false)
		{
			SetBusy(ignoreThreadId);
			value = newValue;
			SetFree();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AsyncValueClassBusyScope<T> GetValueBusyScope(bool ignoreThreadId = false, bool isDisposed = false)
		{
			return new AsyncValueClassBusyScope<T>(this, ignoreThreadId, isDisposed);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AsyncValueClassBusyScope<T> GetValueBusyScope(out T result, bool ignoreThreadId = false, bool isDisposed = false)
		{
			var scope = new AsyncValueClassBusyScope<T>(this, ignoreThreadId, isDisposed);
			result = value;
			return scope;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AsyncValueClassAsyncBusyScope<T> GetAsyncValueBusyScope(bool ignoreThreadId = false, bool isDisposed = false)
		{
			return new AsyncValueClassAsyncBusyScope<T>(this, ignoreThreadId, isDisposed);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AsyncValueClassAsyncBusyScope<T> GetAsyncValueBusyScope(out T result, bool ignoreThreadId = false, bool isDisposed = false)
		{
			var scope = new AsyncValueClassAsyncBusyScope<T>(this, ignoreThreadId, isDisposed);
			result = value;
			return scope;
		}

		public bool TryGetBusyScope(out AsyncValueClassBusyScope<T> result, bool ignoreThreadId = false, bool isDisposed = false)
		{
			if (TrySetBusy())
			{
				result = new AsyncValueClassBusyScope<T>(this, ignoreThreadId, isDisposed);
				return true;
			}

			result = default!;
			return false;
		}

		public bool TryGetBusyScopeAsync(out AsyncValueClassAsyncBusyScope<T> result, bool ignoreThreadId = false, bool isDisposed = false)
		{
			if (TrySetBusy())
			{
				result = new AsyncValueClassAsyncBusyScope<T>(this, ignoreThreadId, isDisposed);
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

		public AsyncValueClassBusyScope(AsyncValueClass<T> asyncValue, bool ignoreThreadId = false, bool isDisposed = false)
		{
			if (!isDisposed)
				asyncValue.SetBusy(ignoreThreadId);
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

		public AsyncValueClassAsyncBusyScope(AsyncValueClass<T> asyncValue, bool ignoreThreadId = false, bool isDisposed = false)
		{
			if (!isDisposed)
				asyncValue.SetBusy(ignoreThreadId);
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
