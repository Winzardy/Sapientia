using Sapientia.Collections;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	/// <summary>
	/// Реализация выбора копира по <see cref="TypeId"/>. Живёт в коде игры (пишется вручную или генератором),
	/// регистрируется в <see cref="GeneratedCopier"/>. Нужна потому, что движок в Sapientia не видит типы игры.
	/// </summary>
	public interface IComponentCopier
	{
		/// <summary>
		/// Есть ли копир для <paramref name="typeId"/>.
		/// </summary>
		bool IsCopiable(TypeId typeId);

		/// <summary>
		/// Помечен ли <paramref name="typeId"/> как намеренно не копируемый (<see cref="SkipCopyAttribute"/>).
		/// Нужно, чтобы отличить ожидаемый пропуск от необработанного ссылочного компонента (молчаливая потеря).
		/// </summary>
		bool IsSkipped(TypeId typeId);

		/// <summary>
		/// Складывает дочерние сущности компонента <paramref name="typeId"/> с <paramref name="entity"/>
		/// в очередь обхода.
		/// </summary>
		void AppendEntities(TypeId typeId, WorldState world, Entity entity, ref UnsafeList<Entity> frontier);

		/// <summary>
		/// Копирует компонент <paramref name="typeId"/> со старой сущности на новую и перенастраивает
		/// ссылки по <paramref name="map"/>.
		/// </summary>
		void CopyComponent(TypeId typeId, WorldState oldWS, WorldState newWS, Entity oldEntity, Entity newEntity, in UnsafeDictionary<Entity, Entity> map);

		/// <summary>
		/// Сообщает о необработанном ссылочном компоненте (без маркера копирования). Реализация в коде игры
		/// пишет ошибку в лог (видно в релизе) и роняет в DEBUG, чтобы потеря при копии не прошла молча.
		/// </summary>
		void ReportUnhandled(TypeId typeId);
	}
}
