using Sapientia.Collections;
using Sapientia.Data;
using Submodules.Sapientia.Data;
using Submodules.Sapientia.Memory;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Per-instance <b>Cache</b>: рабочий массив ячеек (<see cref="UnsafeArray{T}"/> из <see cref="CacheLink"/>) + байтовый
	/// массив значений (<see cref="UnsafeArray{T}"/> из <c>byte</c>) + <b>шаблон ячеек</b> (<see cref="_template"/>, копия
	/// <c>CompiledBlueprintHeader.cacheCellsTemplate</c> с забейканными <c>valueOffset</c>). Ячейка хранит тег + офсет
	/// значения / link; значение — в Value-массиве по <see cref="CacheLink.valueOffset"/> (забейкан из шаблона). Адресует
	/// ячейку <see cref="CacheHandler{T}"/> (ordinal/офсет из Static.Map). Транзиентный: сброс — <see cref="Reset"/> каждый
	/// run = <b>копия шаблона</b> в рабочий массив (восстанавливает <c>state=Uninitialized</c> + забейканные офсеты).
	/// <b>Позиционно-независим</b> (массивы хранят сырые указатели на неподвижные блоки), в снапшот мира не идёт.
	/// </summary>
	public struct InstanceCache
	{
		private UnsafeArray<CacheLink> _cells;     // рабочие ячейки (state + valueOffset / link)
		private UnsafeArray<byte> _values;         // значения ячеек (raw bytes)
		private UnsafeArray<CacheLink> _template;  // шаблон (копия из блоба): источник Reset, забейканные valueOffset

		public readonly bool IsValid => _cells.IsCreated;
		public readonly int CellCount => _cells.Length;
		public readonly int ValuesSize => _values.Length;

		/// <summary>
		/// Создаёт Cache: рабочий массив + шаблон (копия <paramref name="template"/> из блоба) + байтовый блок значений
		/// размера <paramref name="valuesSize"/>. Рабочий массив инициализируется копией шаблона. Пустой
		/// (<paramref name="cellCount"/> &lt;= 0) → <c>default</c> (нода/инстанс без Cache-портов).
		/// </summary>
		public static InstanceCache Create(Id<MemoryManager> memoryId, int cellCount, int valuesSize, SafePtr<CacheLink> template)
		{
			if (cellCount <= 0)
				return default;

			var cache = new InstanceCache
			{
				_cells = new UnsafeArray<CacheLink>(memoryId, cellCount),
				_values = valuesSize > 0 ? new UnsafeArray<byte>(memoryId, valuesSize) : default,
				_template = new UnsafeArray<CacheLink>(memoryId, cellCount),
			};
			MemoryExt.MemCopy(template, cache._template.ptr, cellCount); // блоб-шаблон → собственная копия
			cache._cells.CopyFrom(cache._template);                     // шаблон → рабочий массив (стартовое состояние)
			return cache;
		}

		/// <summary>Ячейка по офсету в рабочем массиве.</summary>
		private ref CacheLink Cell(PtrOffset<CacheLink> cellOffset)
		{
			return ref ((SafePtr)_cells.ptr + cellOffset.byteOffset).Cast<CacheLink>().Value();
		}

		/// <summary>Значение по офсету в Value-массиве.</summary>
		private ref T Value<T>(PtrOffset valueOffset) where T : unmanaged
		{
			return ref ((SafePtr)_values.ptr + valueOffset.byteOffset).Cast<T>().Value();
		}

		/// <summary>Сброс кеша перед run'ом: <b>копия шаблона</b> в рабочий массив (state → Uninitialized, valueOffset
		/// восстановлены из шаблона). No-op для пустого Cache.</summary>
		public void Reset()
		{
			if (_cells.IsCreated)
				_cells.CopyFrom(_template);
		}

		/// <summary>
		/// Резолвит passthrough-цепочку <see cref="CacheState.Link"/> до ячейки-значения (или Uninitialized).
		/// <see cref="CacheLink.link"/> — дельта офсетов (целевая − эта); резолв идёт по офсету <b>от базы массива</b>.
		/// Защита от цикла (вкл. self-link): release-safe ограничение по числу ячеек + DEBUG-assert.
		/// </summary>
		public ref CacheLink ResolveLink(PtrOffset<CacheLink> cellOffset)
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

				offset = new PtrOffset<CacheLink>(offset.byteOffset + cell.link.byteOffset);
				cell = ref Cell(offset);
			}
			return ref cell;
		}

		/// <summary>
		/// Записывает значение Out-порта в <b>его</b> ячейку (<see cref="CacheState.Value"/>) и в Value-массив по
		/// <b>забейканному</b> <see cref="CacheLink.valueOffset"/> ячейки (из шаблона; в карте офсета значения нет).
		/// </summary>
		public void Write<T>(CacheHandler<T> handler, in T value) where T : unmanaged
		{
			ref var cell = ref Cell(handler.cell);
			cell.state = CacheState.Value;
			Value<T>(cell.valueOffset) = value;
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
		/// (универсальная passthrough-нода, #13.3). Дельта — разность офсетов ячеек в массиве. Перезатирает
		/// <c>valueOffset</c> ячейки (union по <see cref="CacheLink.state"/>); <see cref="Reset"/> восстановит из шаблона.
		/// </summary>
		public void WriteLink<T>(CacheHandler<T> from, CacheHandler<T> to) where T : unmanaged
		{
			ref var cell = ref Cell(from.cell);
			cell.state = CacheState.Link;
			cell.link = new PtrOffset<CacheLink>(to.cell.byteOffset - from.cell.byteOffset);
		}

		/// <summary>Освобождает блоки Cache. Идемпотентно (no-op для пустого/уже освобождённого).</summary>
		public void Dispose()
		{
			_cells.Dispose();
			_values.Dispose();
			_template.Dispose();
		}
	}
}
