using Sapientia.Data;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Адрес ячейки кеша порта (из Static.Map): <see cref="cell"/> — офсет ячейки <see cref="CacheLink"/> в массиве
	/// <c>InstanceCache._cells</c>. Офсет значения отдельно <b>не нужен</b> — он забейкан в самой ячейке
	/// (<see cref="CacheLink.valueOffset"/>, копируется из шаблона при создании/сбросе инстанса). <c>T</c> — тип
	/// значения для типизированного чтения/записи.
	/// </summary>
	public struct CacheHandler<T> where T : unmanaged
	{
		public PtrOffset<CacheLink> cell;
	}
}
