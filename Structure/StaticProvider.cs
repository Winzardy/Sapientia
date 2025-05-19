using System;

namespace Sapientia
{
	/// <summary>
	/// Статический доступ к инстансу, cмесь двух паттернов Provider и Strategy. Используется для инфраструктурных сервисов, минуя ServiceLocator/DI
	/// </summary>
	public abstract class StaticProvider<T> where T : class
	{
		protected static T _instance;

		public static void Initialize(T instance)
		{
			Terminate();

			_instance = instance;
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
