using Sapientia.Collections;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	/// <summary>
	/// Выбор копира компонента по <see cref="TypeId"/>. Фасад: сам диспатч живёт в коде игры
	/// (<see cref="IComponentCopier"/>) и регистрируется через <see cref="SetCopier"/>. Пока копир не
	/// зарегистрирован, копируемых типов нет.
	/// </summary>
	public static class GeneratedCopier
	{
		private static IComponentCopier? _copier;

		/// <summary>
		/// Регистрирует диспатч из кода игры. Вызывается один раз при старте.
		/// </summary>
		public static void SetCopier(IComponentCopier copier)
		{
			_copier = copier;
		}

		/// <summary>
		/// Есть ли копир для <paramref name="typeId"/>. false, если диспатч не зарегистрирован.
		/// </summary>
		public static bool IsCopiable(TypeId typeId)
		{
			return _copier != null && _copier.IsCopiable(typeId);
		}

		/// <summary>
		/// Складывает дочерние сущности компонента <paramref name="typeId"/> с <paramref name="entity"/>
		/// в <paramref name="frontier"/>. Вызывается только когда <see cref="IsCopiable"/> вернул true.
		/// </summary>
		public static void AppendEntities(TypeId typeId, WorldState world, Entity entity, ref UnsafeList<Entity> frontier)
		{
			_copier!.AppendEntities(typeId, world, entity, ref frontier);
		}

		/// <summary>
		/// Копирует компонент <paramref name="typeId"/> со старой сущности на новую и перенастраивает
		/// ссылки по <paramref name="map"/>. Вызывается только когда <see cref="IsCopiable"/> вернул true.
		/// </summary>
		public static void CopyComponent(TypeId typeId, WorldState oldWS, WorldState newWS, Entity oldEntity, Entity newEntity, in UnsafeDictionary<Entity, Entity> map)
		{
			_copier!.CopyComponent(typeId, oldWS, newWS, oldEntity, newEntity, in map);
		}
	}
}
