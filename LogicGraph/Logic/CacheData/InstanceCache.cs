using Sapientia.Collections;
using Sapientia.Data;
using Submodules.Sapientia.Data;
using Submodules.Sapientia.Memory;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Per-instance <b>Cache</b>: два раздельных off-allocator-блока — массив метаданных ячеек
	/// (<see cref="UnsafeArray{T}"/> из <see cref="DataCache"/>) и байтовый массив значений
	/// (<see cref="UnsafeArray{T}"/> из <c>byte</c>). Ячейка хранит тег/офсет значения/link (без <c>T</c> и без
	/// инлайн-значения); значение лежит в Value-массиве по <see cref="DataCache.valueOffset"/>. Адресует ячейку и
	/// значение <see cref="CacheHandler{T}"/> (офсеты из Static.Map). Транзиентный: сброс — <see cref="Reset"/> каждый
	/// run. <b>Позиционно-независим</b> (массивы хранят сырые указатели на неподвижные блоки), безопасно лежит в
	/// массиве/копируется (в снапшот мира не идёт).
	/// </summary>
	public struct InstanceCache
	{
		private UnsafeArray<DataCache> _cells;  // метаданные ячеек (state + valueOffset + link)
		private UnsafeArray<byte> _values;      // значения ячеек (raw bytes)

		public readonly bool IsValid => _cells.IsCreated;
		public readonly int CellCount => _cells.Length;
		public readonly int ValuesSize => _values.Length;

		/// <summary>
		/// Создаёт Cache: массив из <paramref name="cellCount"/> ячеек метаданных + байтовый блок значений размера
		/// <paramref name="valuesSize"/> (оба обнулены). Пустой (<paramref name="cellCount"/> &lt;= 0) → <c>default</c>
		/// (нода/инстанс без Cache-портов).
		/// </summary>
		public static InstanceCache Create(Id<MemoryManager> memoryId, int cellCount, int valuesSize)
		{
			if (cellCount <= 0)
				return default;

			return new InstanceCache
			{
				_cells = new UnsafeArray<DataCache>(memoryId, cellCount),
				_values = valuesSize > 0 ? new UnsafeArray<byte>(memoryId, valuesSize) : default,
			};
		}

		/// <summary>Ячейка метаданных по офсету в массиве ячеек.</summary>
		private ref DataCache Cell(PtrOffset<DataCache> cellOffset)
		{
			return ref ((SafePtr)_cells.ptr + cellOffset.byteOffset).Cast<DataCache>().Value();
		}

		/// <summary>Значение по офсету в Value-массиве.</summary>
		private ref T Value<T>(PtrOffset valueOffset) where T : unmanaged
		{
			return ref ((SafePtr)_values.ptr + valueOffset.byteOffset).Cast<T>().Value();
		}

		/// <summary>Сброс кеша перед run'ом: все ячейки → <see cref="CacheState.Uninitialized"/> (0). No-op для пустого Cache.</summary>
		public void Reset()
		{
			if (_cells.IsCreated)
				_cells.Clear();
		}

		/// <summary>
		/// Резолвит passthrough-цепочку <see cref="CacheState.Link"/> до ячейки-значения (или Uninitialized).
		/// <see cref="DataCache.link"/> — дельта офсетов (целевая − эта); резолв идёт по офсету <b>от базы массива</b>.
		/// Защита от цикла (вкл. self-link): release-safe ограничение по числу ячеек + DEBUG-assert.
		/// </summary>
		public ref DataCache ResolveLink(PtrOffset<DataCache> cellOffset)
		{
			var maxHops = _cells.Length;
			var offset = cellOffset;
			ref var cell = ref Cell(offset);

			var hops = 0;
			while (cell.state == CacheState.Link)
			{
				E.ASSERT(hops < maxHops, "[InstanceCache] Цикл passthrough-link'ов (некорректный граф).");
				if (hops++ >= maxHops)
					break; // release-safe: не зацикливаемся на некорректном (циклическом) графе

				offset = new PtrOffset<DataCache>(offset.byteOffset + cell.link.byteOffset);
				cell = ref Cell(offset);
			}
			return ref cell;
		}

		/// <summary>
		/// Записывает значение Out-порта в <b>его</b> ячейку (<see cref="CacheState.Value"/>) и в Value-массив по
		/// <see cref="CacheHandler{T}.value"/>; ячейка запоминает <see cref="DataCache.valueOffset"/> (для резолва link'ов).
		/// </summary>
		public void Write<T>(CacheHandler<T> handler, in T value) where T : unmanaged
		{
			ref var cell = ref Cell(handler.cell);
			cell.state = CacheState.Value;
			cell.valueOffset = handler.value;
			Value<T>(handler.value) = value;
		}

		/// <summary>Читает значение порта (следуя link'ам). <c>true</c> — посчитано (Value); <c>false</c> — Uninitialized.</summary>
		public bool Read<T>(CacheHandler<T> handler, out T value) where T : unmanaged
		{
			ref var cell = ref ResolveLink(handler.cell);
			if (cell.state == CacheState.Value)
			{
				value = Value<T>(cell.valueOffset);
				return true;
			}
			value = default;
			return false;
		}

		/// <summary>Посчитан ли порт (гейт мемоизации Is-Calculated; читается в M8).</summary>
		public bool IsCalculated<T>(CacheHandler<T> handler) where T : unmanaged
		{
			return ResolveLink(handler.cell).state == CacheState.Value;
		}

		/// <summary>
		/// Делает ячейку <paramref name="from"/> passthrough-ссылкой на <paramref name="to"/> — zero-cost jump
		/// (универсальная passthrough-нода, #13.3). Дельта — разность офсетов ячеек в массиве.
		/// </summary>
		public void WriteLink<T>(CacheHandler<T> from, CacheHandler<T> to) where T : unmanaged
		{
			ref var cell = ref Cell(from.cell);
			cell.state = CacheState.Link;
			cell.link = new PtrOffset<DataCache>(to.cell.byteOffset - from.cell.byteOffset);
		}

		/// <summary>Освобождает оба блока Cache. Идемпотентно (no-op для пустого/уже освобождённого).</summary>
		public void Dispose()
		{
			_cells.Dispose();
			_values.Dispose();
		}
	}
}
