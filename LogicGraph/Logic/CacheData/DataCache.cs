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
	/// Ячейка кеша одного In/Out-порта инстанса: тег <see cref="state"/> + объединение <see cref="value"/>
	/// (мемоизированное значение) / <see cref="link"/> (passthrough-ссылка на другую ячейку).
	/// </summary>
	/// <remarks>
	/// QUESTION (выравнивание) — решено: тег = 1 байт на офсете 0, payload на офсете 8 (выровнен под любой
	/// unmanaged <typeparamref name="T"/>; 7 байт паддинга на ячейку — цена за корректное выравнивание под
	/// Burst/IL2CPP/ARM и совпадение с <c>DataSizes.Alignment = 8</c>). Без этого 4-байтный enum-тег перекрывал бы
	/// <see cref="value"/> на нечётном офсете 1.
	/// </remarks>
	[StructLayout(LayoutKind.Explicit)]
	public struct DataCache<T> where T : unmanaged
	{
		[FieldOffset(0)]
		public CacheState state;

		[FieldOffset(8)]
		public T value;
		[FieldOffset(8)]
		public PtrOffset<DataCache<T>> link; // self-relative дельта от этой ячейки до целевой (passthrough)
	}
}
