using System;
using Sapientia.Collections;

namespace Sapientia.MemoryAllocator.State
{
	/// <summary>
	/// Точки входа копирования сущностей между мирами. Сам обход и буферы живут в <see cref="EntityTreeCopier"/>.
	/// </summary>
	public static class WorldStateCopyExtensions
	{
		/// <summary>
		/// Копирует <paramref name="root"/> с дочерним поддеревом из srcWorld в dstWorld, возвращает новый
		/// корень. Тонкая обёртка над <see cref="EntityTreeCopier"/> на один корень. Условие: dstWorld построен
		/// (наборы компонентов зарегистрированы), <paramref name="root"/> не помечен <see cref="IgnoreEntityCopy"/>.
		/// </summary>
		public static Entity CopyEntityTree(this WorldState srcWorld, Entity root, WorldState dstWorld)
		{
			var copier = new EntityTreeCopier(srcWorld, dstWorld);
			try
			{
				copier.AddRoot(root);
				copier.CollectAll();
				copier.CreateCopies();
				copier.CopyValues();
				return copier.GetCopy(root);
			}
			finally
			{
				copier.Dispose();
			}
		}

		/// <summary>
		/// Копирует поддеревья всех <paramref name="roots"/> за один батч: общая таблица копий, поэтому ссылки
		/// между поддеревьями перенастраиваются корректно. <paramref name="newRoots"/> заполняется копиями корней
		/// в том же порядке. Условие: каждый корень построен и не помечен <see cref="IgnoreEntityCopy"/>.
		/// </summary>
		public static void CopyEntityTreeBatch(this WorldState srcWorld, ReadOnlySpan<Entity> roots, Span<Entity> newRoots, WorldState dstWorld)
		{
			E.ASSERT(newRoots.Length >= roots.Length, "CopyEntityTreeBatch: newRoots короче roots - некуда писать копии корней.");

			var copier = new EntityTreeCopier(srcWorld, dstWorld);
			try
			{
				foreach (var root in roots)
				{
					copier.AddRoot(root);
				}
				copier.CollectAll();
				copier.CreateCopies();
				copier.CopyValues();
				for (var i = 0; i < roots.Length; i++)
				{
					newRoots[i] = copier.GetCopy(roots[i]);
				}
			}
			finally
			{
				copier.Dispose();
			}
		}

		/// <summary>
		/// Переводит старую сущность в новую по таблице копирования. Если сущности нет в таблице (ссылка
		/// на чужую сущность, которую не копировали) - возвращает <see cref="Entity.EMPTY"/>.
		/// </summary>
		public static Entity Remap(this in UnsafeDictionary<Entity, Entity> map, Entity entity)
		{
			return map.TryGetValue(entity, out var mapped) ? mapped : Entity.EMPTY;
		}

		/// <summary>
		/// Читает компонент <typeparamref name="T"/> из старого мира, делает копию значения и
		/// перенастраивает ссылки через <see cref="ICopiable{T}.InnerCopy"/>. Возвращает копию для записи
		/// в новый мир.
		/// </summary>
		public static T Copy<T>(this WorldState oldWS, Entity entity, WorldState newWS, in UnsafeDictionary<Entity, Entity> map)
			where T : unmanaged, IComponent, ICopiable<T>
		{
			var oldComponent = new ComponentSetContext<T>(oldWS).ReadElement(entity);
			var newComponent = oldComponent;
			oldComponent.InnerCopy(oldWS, newWS, ref newComponent, in map);
			return newComponent;
		}
	}
}
