using Sapientia.Collections;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	/// <summary>
	/// Диспатч копира по локальному индексу компонента (<see cref="TypeId{IComponent}"/>). Реализация
	/// (<see cref="ComponentCopier"/>) запекается генератором и регистрируется в <see cref="GeneratedCopier"/>.
	/// Лог о необработанном компоненте делегируется в код игры, т.к. движок в Sapientia не видит WLog.
	/// </summary>
	public interface IComponentCopier
	{
		/// <summary>
		/// Есть ли копир для <paramref name="typeId"/>.
		/// </summary>
		bool IsCopiable(TypeId<IComponent> typeId);

		/// <summary>
		/// Помечен ли <paramref name="typeId"/> как намеренно не копируемый (<see cref="SkipCopyAttribute"/>).
		/// Нужно, чтобы отличить ожидаемый пропуск от необработанного ссылочного компонента (молчаливая потеря).
		/// </summary>
		bool IsSkipped(TypeId<IComponent> typeId);

		/// <summary>
		/// Складывает дочерние сущности компонента <paramref name="typeId"/> с <paramref name="entity"/>
		/// в очередь обхода.
		/// </summary>
		void AppendEntities(TypeId<IComponent> typeId, WorldState world, Entity entity, ref UnsafeList<Entity> frontier);

		/// <summary>
		/// Копирует компонент <paramref name="typeId"/> со старой сущности на новую и перенастраивает
		/// ссылки по <paramref name="map"/>.
		/// </summary>
		void CopyComponent(TypeId<IComponent> typeId, WorldState oldWS, WorldState newWS, Entity oldEntity, Entity newEntity, in UnsafeDictionary<Entity, Entity> map);

		/// <summary>
		/// Сообщает о необработанном ссылочном компоненте (без маркера копирования). Реализация в коде игры
		/// пишет ошибку в лог (видно в релизе) и роняет в DEBUG, чтобы потеря при копии не прошла молча.
		/// </summary>
		void ReportUnhandled(TypeId<IComponent> typeId);
	}
}
