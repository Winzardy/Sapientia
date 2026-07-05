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
	}
}
