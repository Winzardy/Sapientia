using System;
using System.Collections.Generic;
using Sapientia.Pooling;

namespace Sapientia.Extensions
{
	public class CompositeDisposable : IDisposable
	{
		private HashSet<IDisposable> _disposables;

		public virtual void Dispose()
		{
			if (_disposables.IsNullOrEmpty())
				return;

			foreach (var disposable in _disposables)
				disposable.Dispose();

			_disposables.ReleaseToStaticPool();
		}

		protected void AddDisposable(IDisposable disposable)
		{
			_disposables ??= HashSetPool<IDisposable>.Get();
			_disposables.Add(disposable);
		}

		protected void RemoveDisposable(IDisposable disposable)
		{
			_disposables.Remove(disposable);
		}

		/// <summary>
		/// Создать и добавить в disposable список
		/// </summary>
		protected T Create<T>(out T instance, bool autoDispose = true)
			where T : IDisposable, new()
		{
			instance = Create<T>(autoDispose);
			return instance;
		}

		/// <summary>
		/// Создать и добавить в disposable список
		/// </summary>
		protected T Create<T>(out T instance, bool autoDispose = true, params object[] args)
			where T : IDisposable
		{
			instance = Create<T>(autoDispose, args);
			return instance;
		}

		/// <summary>
		/// Создать и добавить в disposable список
		/// </summary>
		protected T Create<T>(bool autoDispose = true)
			where T : IDisposable, new()
		{
			var instance = new T();
			if (autoDispose)
				AddDisposable(instance);
			return instance;
		}

		/// <summary>
		/// Создать и добавить в disposable список
		/// </summary>
		protected T Create<T>(bool autoDispose = true, params object[] args)
			where T : IDisposable
		{
			var instance = (T) Activator.CreateInstance(typeof(T), args);
			if (autoDispose)
				AddDisposable(instance);
			return instance;
		}
	}
}
