using System;

namespace Sapientia
{
	/// <summary>
	/// Статический доступ к инстансу
	/// Используется для инфраструктурных сервисов, минуя ServiceLocator/DI.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Некоторые решения, допустимые на клиенте, могут быть неприемлемы на сервере.
	/// Сервер, как правило, работает в многопоточной среде, где требуется использовать
	/// отдельные инстансы вместо глобальных (статических) объектов.
	/// </para>
	/// <para>
	/// Например, MessageBus может быть реализован через статический доступ на клиенте — это удобно и работает.
	/// Но на сервере потребуется использовать отдельные экземпляры (MessageHub),
	/// чтобы каждый поток или пользовательский контекст работал с изолированными данными
	/// </para>
	/// <para>
	/// В отличие от этого, Content может использоваться через статический провайдер,
	/// поскольку все его данные по умолчанию являются только для чтения (readonly).
	/// (если не учитывать возможность наличия разных версий контента у пользователей)
	/// </para>
	/// </remarks>
	public abstract class StaticAccessor<T> where T : class
	{
		protected static T? _instance;

		public static void Set(T instance, bool disposePrevious = true)
		{
			Clear(disposePrevious);

			_instance = instance;
		}

		public static void Clear(bool dispose = true)
		{
			switch (_instance)
			{
				case null:
					return;
				case IDisposable disposable:
					if (dispose)
						disposable.Dispose();
					break;
			}

			_instance = null;
		}
	}
}
