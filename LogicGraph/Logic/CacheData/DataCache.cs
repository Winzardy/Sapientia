using System.Runtime.InteropServices;
using Sapientia.Data;

namespace Sapientia.LogicGraph
{
	public enum CacheState : byte
	{
		Uninitialized,
		Value,
		Link,
	}

	/// <summary>
	/// Метаданные ячейки кеша одного In/Out-порта инстанса — <b>без значения и без <c>T</c></b>: тег
	/// <see cref="state"/> + объединение <see cref="valueOffset"/> (офсет значения в Value-массиве инстанса,
	/// при <see cref="CacheState.Value"/>) / <see cref="link"/> (passthrough-ссылка на другую ячейку — дельта
	/// офсетов в массиве ячеек, при <see cref="CacheState.Link"/>). <see cref="valueOffset"/> и <see cref="link"/>
	/// взаимоисключающи (гейтятся <see cref="state"/>), поэтому лежат в union. Сами значения хранятся отдельно
	/// (<c>InstanceCache</c> держит массив ячеек <see cref="DataCache"/> и байтовый массив значений раздельно).
	/// </summary>
	/// <remarks>
	/// Выравнивание: тег = 1 байт на офсете 0, payload (union) на офсете 8 (7 байт паддинга — цена за корректное
	/// выравнивание под Burst/IL2CPP/ARM и совпадение с <c>DataSizes.Alignment = 8</c>). Без этого тег перекрывал бы
	/// payload на нечётном офсете.
	/// </remarks>
	[StructLayout(LayoutKind.Explicit)]
	public struct DataCache
	{
		[FieldOffset(0)]
		public CacheState state;

		[FieldOffset(8)]
		public PtrOffset valueOffset;
		[FieldOffset(8)]
		public PtrOffset<DataCache> link;
	}
}
