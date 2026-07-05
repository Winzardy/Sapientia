using Sapientia.Collections;

namespace Sapientia.MemoryAllocator.State
{
	/// <summary>
	/// Компонент, который умеет копировать сам себя в другой мир. Реализацию пишет генератор или
	/// человек (для сложных структур с перекрытием полей).
	/// </summary>
	public interface ICopiable<T> where T : unmanaged, IComponent, ICopiable<T>
	{
		/// <summary>
		/// this - компонент в исходном мире. Положить в <paramref name="entities"/> только свои дочерние
		/// сущности. Ссылки на чужие сущности не класть - их перенастроит <see cref="InnerCopy"/>.
		/// </summary>
		void AppendEntities(WorldState world, ref UnsafeList<Entity> entities);

		/// <summary>
		/// this - старый компонент. <paramref name="component"/> - его копия в новом мире: простые поля
		/// уже верны, поля-сущности держат старые значения, коллекции ссылаются на старую память.
		/// Перенастроить поля-сущности по <paramref name="map"/>, коллекции пересоздать в новом мире по
		/// одному элементу. Если сущности нет в <paramref name="map"/> - поставить <see cref="Entity.EMPTY"/>.
		/// </summary>
		void InnerCopy(WorldState oldWS, WorldState newWS, ref T component, in EntityCopyMap map);
	}
}
