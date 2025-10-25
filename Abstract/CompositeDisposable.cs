using System;
using System.Collections.Generic;
using Sapientia.Collections;
using Sapientia.Pooling;

#nullable enable

namespace Sapientia.Extensions
{
	public class CompositeDisposable : IDisposable
	{
		private List<IDisposable>? _disposables;

		public virtual void Dispose()
		{
			OnDisposeInternal();
			if (_disposables?.IsNullOrEmpty() ?? true)
				return;

			foreach (var disposable in _disposables)
				disposable.Dispose();

			_disposables.ReleaseToStaticPool();
		}

		protected virtual void OnDisposeInternal() => OnDispose();

		protected virtual void OnDispose()
		{
		}

		public void AddDisposable(IDisposable disposable)
		{
			_disposables ??= ListPool<IDisposable>.Get();
			_disposables.Add(disposable);
		}

		public void RemoveDisposable(IDisposable disposable)
		{
			_disposables?.Remove(disposable);
		}

		protected void DisposeAndSetNullSafe<T>(ref T obj) where T : class, IDisposable
		{
			if (obj == null!)
				return;

			DisposeAndSetNull(ref obj);
		}

		protected void DisposeAndSetNull<T>(ref T obj) where T : class, IDisposable
			=> DisposeUtility.DisposeAndSetNull(ref obj);

		/// <summary>
		/// Создать (через конструктор) и добавить в disposable список
		/// </summary>
		protected T Create<T>(out T instance, bool addToAutoDispose = false)
			where T : IDisposable, new()
		{
			instance = Create<T>(addToAutoDispose);
			return instance;
		}

		/// <summary>
		/// Создать (через Activator) и добавить в disposable список
		/// </summary>
		protected T Create<T>(out T instance, bool addToAutoDispose = false, params object[] args)
			where T : IDisposable
		{
			instance = Create<T>(addToAutoDispose, args);
			return instance;
		}

		/// <summary>
		/// Создать (через фабричный метод) и добавить в disposable список
		/// </summary>
		protected T Create<T>(out T instance, Func<T> factory, bool addToAutoDispose = false)
			where T : IDisposable
		{
			instance = Create(factory, addToAutoDispose);
			return instance;
		}

		/// <summary>
		/// Создать (через фабричный метод с аргументом) и добавить в disposable список
		/// </summary>
		protected T Create<T, T2>(out T instance, Func<T2, T> factory, T2 argument, bool autoDispose = false)
			where T : IDisposable
		{
			instance = Create(factory, argument, autoDispose);
			return instance;
		}

		/// <summary>
		/// Создать (через конструктор) и добавить в disposable список
		/// </summary>
		protected T Create<T>(bool autoDispose = false)
			where T : IDisposable, new()
		{
			var instance = new T();
			if (autoDispose)
				AddDisposable(instance);
			return instance;
		}

		/// <summary>
		/// Создать (через Activator) и добавить в disposable список
		/// </summary>
		protected T Create<T>(bool autoDispose = false, params object[] args)
			where T : IDisposable
		{
			var instance = (T) Activator.CreateInstance(typeof(T), args);
			if (autoDispose)
				AddDisposable(instance);
			return instance;
		}

		/// <summary>
		/// Создать (через фабричный метод) и добавить в disposable список
		/// </summary>
		protected T Create<T>(Func<T> factory, bool addToAutoDispose = false)
			where T : IDisposable
		{
			var instance = factory();
			if (addToAutoDispose)
				AddDisposable(instance);
			return instance;
		}

		/// <summary>
		/// Создать (через фабричный метод с аргументом) и добавить в disposable список
		/// </summary>
		protected T Create<T, T2>(Func<T2, T> factory, T2 argument, bool addToAutoDispose = false)
			where T : IDisposable
		{
			var instance = factory(argument);
			if (addToAutoDispose)
				AddDisposable(instance);
			return instance;
		}

		/// <summary>
		/// Проверяет наличие поля и возвращает его, либо создает новое и возвращает
		/// </summary>
		protected T TryCreate<T>(ref T field, bool addToAutoDispose = false)
			where T : IDisposable, new()
			=> field ?? Create(out field, addToAutoDispose);
	}

	public static class DisposeUtility
	{
		public static void DisposeAndSetNull<T>(ref T obj) where T : class, IDisposable
		{
			obj.Dispose();
			obj = null!;
		}
	}
}
