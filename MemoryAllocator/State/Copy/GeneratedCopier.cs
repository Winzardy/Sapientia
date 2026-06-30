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
		public static bool IsCopiable(TypeId<IComponent> typeId)
		{
			return _copier != null && _copier.IsCopiable(typeId);
		}

		/// <summary>
		/// Помечен ли <paramref name="typeId"/> как намеренно не копируемый. Если диспатч не зарегистрирован -
		/// считаем пропущенным (копировать всё равно нечем, ложную диагностику не поднимаем).
		/// </summary>
		public static bool IsSkipped(TypeId<IComponent> typeId)
		{
			return _copier == null || _copier.IsSkipped(typeId);
		}

		/// <summary>
		/// Складывает дочерние сущности компонента <paramref name="typeId"/> с <paramref name="entity"/>
		/// в <paramref name="frontier"/>. Вызывается только когда <see cref="IsCopiable"/> вернул true.
		/// </summary>
		public static void AppendEntities(TypeId<IComponent> typeId, WorldState world, Entity entity, ref UnsafeList<Entity> frontier)
		{
			_copier!.AppendEntities(typeId, world, entity, ref frontier);
		}

		/// <summary>
		/// Копирует компонент <paramref name="typeId"/> со старой сущности на новую и перенастраивает
		/// ссылки по <paramref name="map"/>. Вызывается только когда <see cref="IsCopiable"/> вернул true.
		/// </summary>
		public static void CopyComponent(TypeId<IComponent> typeId, WorldState oldWS, WorldState newWS, Entity oldEntity, Entity newEntity, in UnsafeDictionary<Entity, Entity> map)
		{
			_copier!.CopyComponent(typeId, oldWS, newWS, oldEntity, newEntity, in map);
		}

		/// <summary>
		/// Сообщает о необработанном ссылочном компоненте через диспатч в код игры. Если диспатч не
		/// зарегистрирован - сообщать нечем и незачем (копировать всё равно нечем).
		/// </summary>
		public static void ReportUnhandled(TypeId<IComponent> typeId)
		{
			_copier?.ReportUnhandled(typeId);
		}
	}
}
