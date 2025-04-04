using System;

namespace Sapientia
{
	/// <summary>
	/// Статический сервис, cмесь двух паттернов Provider и Strategy. Используется для инфраструктурных сервисов, минуя ServiceLocator/DI
	/// </summary>
	public abstract class StaticWrapper<T> where T : class
	{
		protected static T _instance;

		public static void Initialize(T service)
		{
			Terminate();

			_instance = service;
		}

		public static void Terminate()
		{
			switch (_instance)
			{
				case null:
					return;
				case IDisposable disposable:
					disposable.Dispose();
					break;
			}

			_instance = null;
		}
	}
}
