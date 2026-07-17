namespace Sapientia.MemoryAllocator.State
{
	/// <summary>
	/// Хелперы копирования компонентов между мирами. Сам обход и буферы живут в <see cref="EntityTreeCopier"/>.
	/// </summary>
	public static class WorldStateCopyExtensions
	{
		/// <summary>
		/// Читает компонент <typeparamref name="T"/> из старого мира, делает копию значения и
		/// перенастраивает ссылки через <see cref="ICopiable{T}.InnerCopy"/>. Возвращает копию для записи
		/// в новый мир.
		/// </summary>
		public static T Copy<T>(this WorldState oldWS, Entity entity, WorldState newWS, in EntityCopyMap map)
			where T : unmanaged, IComponent, ICopiable<T>
		{
			var oldComponent = new ComponentSetContext<T>(oldWS).ReadElement(entity);
			var newComponent = oldComponent;
			oldComponent.InnerCopy(oldWS, newWS, ref newComponent, in map);
			return newComponent;
		}

		/// <summary>
		/// Вторая фаза: читает старый компонент, берёт ref на УЖЕ скопированный (фазой 1) компонент нового
		/// мира и даёт ему дозаписать кросс-сущностные данные через <see cref="ILateCopiable{T}.LateInnerCopy"/>.
		/// </summary>
		public static void LateCopy<T>(this WorldState oldWS, Entity oldEntity, WorldState newWS, Entity newEntity, in EntityCopyMap map)
			where T : unmanaged, IComponent, ICopiable<T>, ILateCopiable<T>
		{
			var oldComponent = new ComponentSetContext<T>(oldWS).ReadElement(oldEntity);
			ref var newComponent = ref new ComponentSetContext<T>(newWS).GetElement(newEntity);
			oldComponent.LateInnerCopy(oldWS, newWS, oldEntity, newEntity, ref newComponent, in map);
		}
	}
}
