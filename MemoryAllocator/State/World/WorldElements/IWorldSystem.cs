using Sapientia.Collections;
using Sapientia.MemoryAllocator.State;

namespace Sapientia.MemoryAllocator
{
	public interface IWorldSystem : IWorldElement
	{
		public virtual void BeforeUpdate(WorldState worldState, IndexedPtr self) {}
		public virtual void Update(WorldState worldState, IndexedPtr self, float deltaTime) {}
		public virtual void AfterUpdate(WorldState worldState, IndexedPtr self) {}

		public virtual void BeforeLateUpdate(WorldState worldState, IndexedPtr self) {}
		public virtual void LateUpdate(WorldState worldState, IndexedPtr self) {}
		public virtual void AfterLateUpdate(WorldState worldState, IndexedPtr self) {}

		/// <summary>
		/// Положить в <paramref name="roots"/> свои корневые сущности для копирования их поддеревьев в новый
		/// мир - аналог <see cref="IWorldStatePart.AppendEntities"/> для системы. Ссылки на чужие
		/// сущности не класть, их перенастроит <see cref="InnerCopy"/>.
		/// </summary>
		public virtual void AppendEntities(WorldState world, ref UnsafeList<Entity> roots) {}

		/// <summary>
		/// Перенос данных между мирами силами системы: в отличие от <see cref="IWorldStatePart.InnerCopy"/>,
		/// здесь можно звать любые *Logic напрямую. Компоненты сущностей уже скопированы своими копирами
		/// к моменту вызова - это шаг для кросс-сущностной и кросс-компонентной работы.
		/// </summary>
		public virtual void InnerCopy(WorldState oldWorld, WorldState newWorld, in EntityCopyMap map) {}
	}
}
