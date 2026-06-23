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
		/// Складывает дочерние сущности компонента <paramref name="typeId"/> с <paramref name="entity"/>
		/// в очередь обхода.
		/// </summary>
		void AppendEntities(TypeId typeId, WorldState world, Entity entity, ref UnsafeList<Entity> frontier);

		/// <summary>
		/// Копирует компонент <paramref name="typeId"/> со старой сущности на новую и перенастраивает
		/// ссылки по <paramref name="map"/>.
		/// </summary>
		void CopyComponent(TypeId typeId, WorldState oldWS, WorldState newWS, Entity oldEntity, Entity newEntity, in UnsafeDictionary<Entity, Entity> map);
	}
}
