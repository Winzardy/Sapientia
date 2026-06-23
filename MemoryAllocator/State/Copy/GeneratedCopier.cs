using Sapientia.Collections;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	/// <summary>
	/// Выбор копира компонента по <see cref="TypeId"/>. Связывает движок в Sapientia с кодом игры,
	/// который этот выбор заполняет. Пока пусто: копируемых типов нет, поэтому <see cref="AppendEntities"/>
	/// и <see cref="CopyComponent"/> не вызываются (защищены проверкой <see cref="IsCopiable"/>).
	/// </summary>
	public static class GeneratedCopier
	{
		/// <summary>
		/// Есть ли копир для <paramref name="typeId"/>. Пока всегда false.
		/// </summary>
		public static bool IsCopiable(TypeId typeId)
		{
			return false;
		}

		/// <summary>
		/// Складывает дочерние сущности компонента <paramref name="typeId"/> с <paramref name="entity"/>
		/// в очередь обхода. Вызывается только когда <see cref="IsCopiable"/> вернул true.
		/// </summary>
		public static void AppendEntities(TypeId typeId, WorldState world, Entity entity, ref UnsafeList<Entity> frontier)
		{
		}

		/// <summary>
		/// Копирует компонент <paramref name="typeId"/> со старой сущности на новую и перенастраивает
		/// ссылки по <paramref name="map"/>. Вызывается только когда <see cref="IsCopiable"/> вернул true.
		/// </summary>
		public static void CopyComponent(TypeId typeId, WorldState oldWS, WorldState newWS, Entity oldEntity, Entity newEntity, in UnsafeDictionary<Entity, Entity> map)
		{
		}
	}
}
