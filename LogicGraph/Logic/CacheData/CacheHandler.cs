using Sapientia.Data;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Адрес ячейки кеша порта (из Static.Map): <see cref="cell"/> — офсет метаданных в массиве <see cref="DataCache"/>;
	/// <see cref="value"/> — офсет значения в Value-массиве инстанса. <c>T</c> — тип значения для типизированного
	/// чтения/записи. На Write ячейка запоминает свой <see cref="DataCache.valueOffset"/> (= <see cref="value"/>),
	/// чтобы резолв passthrough-цепочки нашёл значение целевой ячейки.
	/// </summary>
	public struct CacheHandler<T> where T : unmanaged
	{
		public PtrOffset<DataCache> cell;
		public PtrOffset value;
	}
}
