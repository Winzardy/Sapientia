using System;

namespace Sapientia
{
	/// <summary>
	/// Статический доступ к инстансу, смесь паттернов Provider и Strategy.
	/// Используется для инфраструктурных сервисов, минуя ServiceLocator/DI.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Некоторые решения, допустимые на клиенте, могут быть неприемлемы на сервере.
	/// Сервер, как правило, работает в многопоточной среде, где требуется использовать
	/// отдельные инстансы вместо глобальных (статических) объектов.
	/// </para>
	/// <para>
	/// Например, MessageBus может быть реализован через статический провайдер на клиенте — это удобно и работает.
	/// Но на сервере потребуется использовать отдельные экземпляры (MessageHub),
	/// чтобы каждый поток или пользовательский контекст работал с изолированными данными
	/// </para>
	/// <para>
	/// В отличие от этого, Content может использоваться через статический провайдер,
	/// поскольку все его данные по умолчанию являются только для чтения (readonly).
	/// (если не учитывать возможность наличия разных версий контента у пользователей)
	/// </para>
	/// </remarks>
	public abstract class StaticProvider<T> where T : class
	{
		protected static T? _instance;

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
