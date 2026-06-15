using Sapientia.Data;
using Sapientia.Extensions;
using Submodules.Sapientia.Memory;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Per-instance <b>Cache</b>: массив ячеек <see cref="DataCache{T}"/> (мемоизация Is-Calculated + passthrough-link)
	/// для In/Out Cache-портов нод. <see cref="dataCache"/> — self-relative указатель на блок ячеек (его выделяет
	/// владелец инстанса, 4F; <see cref="Setup"/> связывает + чистит). Ячейку адресует <see cref="CacheHandler{T}"/>
	/// (офсет в блоке) из Static.Map (<see cref="RegionPtr"/> Cache-порта). Сброс — <see cref="Reset"/> каждый run.
	/// </summary>
	/// <remarks>Self-relative: методы вызывать только через ref/стабильный адрес — на копии по значению
	/// <see cref="dataCache"/> сломается. Runtime off-allocator, транзиентно (в снапшот мира не идёт).</remarks>
	public struct CacheHeader
	{
		/// <summary>Self-relative → блок ячеек <see cref="DataCache{T}"/>.</summary>
		public RelativePtr dataCache;
		/// <summary>Размер блока ячеек в байтах (для <see cref="Reset"/>).</summary>
		public int cellsSize;

		/// <summary>
		/// Связывает с блоком ячеек и обнуляет его (все ячейки → <see cref="CacheState.Uninitialized"/>).
		/// Зовёт владелец инстанса после аллокации блока (размер — <c>compiled.GetBlockSize(Cache)</c>).
		/// </summary>
		public void Setup(SafePtr cellsBlock, int cellsBlockSize)
		{
			dataCache.SetPtr(cellsBlock);
			cellsSize = cellsBlockSize;
			MemoryExt.MemClear(cellsBlock, cellsBlockSize);
		}

		/// <summary>Сброс кеша перед run'ом: все ячейки → <see cref="CacheState.Uninitialized"/> (0).</summary>
		public void Reset()
		{
			MemoryExt.MemClear(dataCache.GetPtr(), cellsSize);
		}

		public SafePtr<DataCache<T>> GetCachePtr<T>(CacheHandler<T> handler) where T : unmanaged
		{
			return dataCache.GetPtr(handler.offset);
		}

		public ref DataCache<T> GetCache<T>(CacheHandler<T> handler) where T : unmanaged
		{
			return ref GetCachePtr(handler).Value();
		}

		/// <summary>
		/// Резолвит passthrough-цепочку <see cref="CacheState.Link"/> до ячейки-значения (или Uninitialized).
		/// <see cref="DataCache{T}.link"/> — дельта (в байтах) от ячейки до целевой. Резолв идёт по офсету
		/// <b>от базы блока</b> (а не «адрес ячейки ± дельта» — иначе ломаются bounds одной ячейки).
		/// Защита от цикла (вкл. self-link): release-safe ограничение по числу ячеек + DEBUG-assert.
		/// </summary>
		public ref DataCache<T> ResolveLink<T>(CacheHandler<T> handler) where T : unmanaged
		{
			// Валидная цепочка не длиннее числа ячеек в блоке.
			var maxHops = cellsSize / TSize<DataCache<T>>.size;
			var offset = handler.offset;
			ref var cell = ref dataCache.GetPtr(offset).Value();

			var hops = 0;
			while (cell.state == CacheState.Link)
			{
				E.ASSERT(hops < maxHops, "[CacheHeader] Цикл passthrough-link'ов (некорректный граф).");
				if (hops++ >= maxHops)
					break; // release-safe: не зацикливаемся на некорректном (циклическом) графе

				offset = new PtrOffset<DataCache<T>>(offset.byteOffset + cell.link.byteOffset);
				cell = ref dataCache.GetPtr(offset).Value();
			}
			return ref cell;
		}

		/// <summary>
		/// Записывает мемоизированное значение Out-порта в <b>его собственную</b> ячейку (<see cref="CacheState.Value"/>).
		/// Инвариант: ячейка Out — не <see cref="CacheState.Link"/> (link'и ставит только passthrough через
		/// <see cref="WriteLink{T}"/> на отдельной ячейке); Write такую ячейку перетёр бы в Value.
		/// </summary>
		public void Write<T>(CacheHandler<T> handler, in T value) where T : unmanaged
		{
			ref var cell = ref GetCache(handler);
			cell.state = CacheState.Value;
			cell.value = value;
		}

		/// <summary>Читает значение порта (следуя link'ам). <c>true</c> — посчитано (Value); <c>false</c> — Uninitialized.</summary>
		public bool Read<T>(CacheHandler<T> handler, out T value) where T : unmanaged
		{
			ref var cell = ref ResolveLink(handler);
			if (cell.state == CacheState.Value)
			{
				value = cell.value;
				return true;
			}
			value = default;
			return false;
		}

		/// <summary>Посчитан ли порт (гейт мемоизации Is-Calculated; читается в M8).</summary>
		public bool IsCalculated<T>(CacheHandler<T> handler) where T : unmanaged
		{
			return ResolveLink(handler).state == CacheState.Value;
		}

		/// <summary>
		/// Делает ячейку <paramref name="from"/> passthrough-ссылкой на <paramref name="to"/> — zero-cost jump
		/// (универсальная passthrough-нода, #13.3). Дельта — разность офсетов ячеек в общем блоке.
		/// </summary>
		public void WriteLink<T>(CacheHandler<T> from, CacheHandler<T> to) where T : unmanaged
		{
			ref var src = ref GetCache(from);
			src.state = CacheState.Link;
			src.link = new PtrOffset<DataCache<T>>(to.offset.byteOffset - from.offset.byteOffset);
		}
	}
}
