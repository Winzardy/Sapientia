# 01 — Компилятор + блоб: flat-ordinal cell-офсет + таблица value-офсетов

**Статус: ✅ done**

> **Финал (после re-gate'ов):** `RegionPtr` — **union 16 байт** (`staticData`/`cacheData: Id<CacheLink>` ordinal/
> `instanceData`). Value-офсет забейкан в **`cacheCellsTemplate: BumpArray<CacheLink>`** (по ordinal), не в карте.
> Компилятор: `cacheData`=ordinal + `cacheCellsTemplate[ordinal].valueOffset`; `WriteSlot` по региону; reserve += шаблон;
> `cacheNodeOffset` снесён; null-Out skip. `DataCache`→`CacheLink`. `InstanceCache` копирует шаблон (Reset=copy).

## Цель

Свести Cache-раскладку `SetupMap` на плоскую ordinal-модель (по `InstanceCache._cells`/`_values`) и забейкать
value-офсеты, чтобы Cache-порт нёс оба офсета (cell в `RegionPtr`, value в новой таблице блоба).

## Шаги

1. `CompiledBlueprintHeader`: добавить поле `cacheValueOffsets : BumpArray<PtrOffset>` + аксессор
   `GetCacheValueOffset(int cellOrdinal)` + doc (индекс = ordinal ячейки; value-офсет в `_values`).
2. `SetupMap` pass 1:
   - снести `cacheNodeOffset` и его инкремент (`+= DataSizes.GetAligned(Cache)`);
   - Cache-Out: `cellOffset = cacheCells × TSize<DataCache>` (плоский), `valueOffset = cacheValuesBytes`;
     `outTarget[output] = Runtime(Cache, cellOffset)`; `cacheValueOffsets.Get(cacheCells) = new PtrOffset(valueOffset)`;
     затем `cacheCells++`, `cacheValuesBytes += output.DataSize.AlignUp(8)`;
   - Persistence-Out оставить как есть (per-node `header.persistence.byteOffset + intra`);
   - разнести Cache/Persistence-ветки (сейчас общий `else` с ternary `nodeRegionOffset`);
   - сохранить budget-ASSERT (ячейки ноды ≤ declared `cacheSize`) — для Cache считать через локальный per-node счётчик.
3. `SetupMap`: до pass 1 — `if (cacheCells_total > 0) compiled.cacheValueOffsets.Alloc(ref allocator, cacheCells_total)`
   (через `CountCacheCells`); пустой — не аллоцировать (симметрия с `contextTypes`).
4. `CalculateLayoutSizeToReserve`: новый шаг (lockstep) `size += TSize<PtrOffset>.size × CountCacheCells(blueprint)`;
   хелпер `CountCacheCells(blueprint)` (walk нод, считать Out'ы с `GetOutputRegion == Cache`).

## Done-criteria

- Cache-`RegionPtr.data.byteOffset` == `cellOrdinal × 16` (плоский, не зависит от `DataSizes.Cache`/slack).
- `cacheValueOffsets[ordinal]` == префикс-сумма выровненных `DataSize` Cache-Out'ов.
- Lockstep цел (`CalculateLayoutSizeToReserve` == bump) на графах с Cache-Out'ами (MapTests).
- `cacheNodeOffset` отсутствует в коде.
- Компилируется; `blockSizes[Cache]`/`DataSizes.Cache` не тронуты.

## Зависимости

Нет (входная точка фазы).

## Notes / findings

_(заполняется по ходу)_
