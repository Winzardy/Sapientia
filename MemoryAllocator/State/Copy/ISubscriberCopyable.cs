using Sapientia.Collections;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	/// <summary>
	/// Общий предок callback-подписчиков (IKillSubscriber и т.п.) - даёт Copy/AppendEntities, чтобы
	/// Callback&lt;TProxy&gt;-подписки переживали перенос между мирами так же, как обычные компоненты.
	/// </summary>
	public interface ISubscriberCopyable : IInterfaceProxyType
	{
		/// <summary>
		/// Копирует себя из oldWS в newWS и перенастраивает свои Entity-поля через map. Возвращает IndexedPtr -
		/// вызывающий код оборачивает его в ProxyPtr (implicit).
		/// </summary>
		IndexedPtr Copy(WorldState oldWS, WorldState newWS, in EntityCopyMap map);

		/// <summary>Кладёт свои Entity-поля (если есть) в обход копира - аналог ICopiable.AppendEntities.</summary>
		void AppendEntities(WorldState world, ref UnsafeList<Entity> entities);
	}
}
