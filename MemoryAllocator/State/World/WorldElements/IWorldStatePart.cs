using Sapientia.Collections;
using Sapientia.MemoryAllocator.State;

namespace Sapientia.MemoryAllocator
{
	public interface IWorldStatePart : IWorldElement
	{
		/// <summary>
		/// Положить в <paramref name="roots"/> свои корневые сущности для копирования их поддеревьев в новый
		/// мир (аналог <see cref="State.ICopiable{T}.AppendEntities"/>, но для стейт-парта). Ссылки на чужие
		/// сущности не класть - их перенастроит <see cref="InnerCopy"/>.
		/// </summary>
		public virtual void AppendEntities(WorldState world, ref UnsafeList<Entity> roots) {}

		/// <summary>
		/// Перенести СВОИ поля из старого стейт-парта (this) в новый (тот же тип в <paramref name="newWorld"/>):
		/// поля-сущности перенастроить по <paramref name="map"/>, коллекции пересоздать по одному элементу.
		/// Компоненты сущностей НЕ трогать - это ответственность их собственных копиров.
		/// </summary>
		public virtual void InnerCopy(WorldState oldWorld, WorldState newWorld, in EntityCopyMap map) {}
	}
}
