# Под-фаза 4F-3 — Свод кеш-раскладки компилятора (долг 4F-1)

**Статус: ✅ done (закоммичено, не запушено).** Обзор слоя — [../README.md](../README.md); снимок — [../../../STATE.md](../../../STATE.md).

## Цель

`SetupMap` считал Cache-офсет в `RegionPtr` по старой per-node модели (`cacheNodeOffset += DataSizes.GetAligned(Cache)`),
что расходилось с фактической раскладкой `InstanceCache` при Cache-slack. Привести Cache к плоскому ordinal и убрать
дублирование value-офсета в карте.

## Финальное решение (после ряда re-gate'ов пользователя)

- **`RegionPtr`** → `[StructLayout(Explicit)]`, **union 16 байт** (порт всегда одного региона ⇒ один слот @8):
  `staticData: RelativePtr<byte>` (Static, self-rel) / `cacheData: Id<CacheLink>` (Cache, **ordinal ячейки**) /
  `instanceData: PtrOffset` (InstancePersistence, офсет слайса).
- **Value-офсет в карте не лежит** (нет дублирования по портам). Он забейкан **один раз** в
  **`CompiledBlueprintHeader.cacheCellsTemplate: BumpArray<CacheLink>`** (по ordinal): шаблон ячеек с `valueOffset` и
  `state = Uninitialized`.
- **`InstanceCache`** владеет **копией** шаблона (`_template`, скопирована из блоба на `Create`). `Reset` = `_cells.CopyFrom(_template)`
  (восстанавливает `state` + забейканные `valueOffset`). `Write` пишет по `cell.valueOffset` (без `handler.value`).
- **`CacheHandler<T>`** = `{ PtrOffset<CacheLink> cell }` (value-офсет в самой ячейке).
- **Rename** `DataCache` → `CacheLink`. **`cacheNodeOffset`** снесён. **`link` не бейкаем** (runtime `WriteLink`, переустановка
  каждый run; `Reset`-copy шаблона восстановит ячейку — при будущем бейке link'а шаблон подхватит без правок).
- **`BumpHeader.Size`/`Reset` + `RawBumpAllocator.Size`** — оставлены (re-gate; не dead-code).

### Почему `InstanceCache` владеет копией шаблона, а не указателем в blob

Блоб стабилен (своя off-allocator `RawBumpAllocator`-арена), но position-independent и `ExecutionScope` намеренно не кеширует
blob-ref. Владеемая копия делает `Reset()` самодостаточным (без аргумента, без staleness) ценой дублирования шаблона на инстанс
(транзиентный кеш — приемлемо).

## Файлы

| Действие | Путь | Что |
|---|---|---|
| **rename** | `Logic/CacheData/CacheLink.cs` | `DataCache` → `CacheLink` (структура без изменений). |
| **~** | `Blueprint/RegionPtr.cs` | `[Explicit]` union 16 байт: `staticData`/`cacheData: Id<CacheLink>`/`instanceData`. |
| **~** | `Logic/CacheData/CacheHandler.cs` | Убран `value`; только `cell`. |
| **~** | `Logic/CacheData/InstanceCache.cs` | + `_template`; `Create(…, SafePtr<CacheLink> template)` копирует шаблон; `Reset` = `CopyFrom`; `Write` по `cell.valueOffset`. |
| **~** | `Logic/StaticData/CompiledBlueprintHeader.cs` | `cacheValueOffsets` → `cacheCellsTemplate: BumpArray<CacheLink>` + `GetCacheCellsTemplate()`. |
| **~** | `Logic/StaticData/BlueprintCompiler.cs` | Cache → `cacheData`=ordinal; бейк `cacheCellsTemplate[ordinal].valueOffset`; Persistence → `instanceData`; Static → `staticData`; reserve += шаблон; `CountCacheCells`; null-Out skip. |
| **~** | `Logic/ExecutionScope.cs` | `Create(…, compiled.GetCacheCellsTemplate())`. |
| **~** | `Blueprint/INode.cs` | rename + cref-фиксы. |
| **~** | `Tests/MapTests.cs` | cache через `cacheData` (ordinal), value через `cacheCellsTemplate.Get(i).valueOffset`, persistence через `instanceData`. |
| **~** | `Tests/CacheTests.cs` | `Create` строит шаблон; `H` без `value`. |

## Tasks (индекс)

| # | Задача | Статус |
|---|---|---|
| 01 | [Компилятор + RegionPtr + блоб](tasks/01-compiler-cache-offsets.md) | ✅ done |
| 02 | [BumpHeader/RawBumpAllocator](tasks/02-dead-code.md) — `Size`/`Reset` оставлены (re-gate) | ✅ done |
| 03 | [Тесты](tasks/03-tests.md) | ✅ done |

## Test list

- `Map_CacheCellsFlatOrdinalAcrossNodes` — slack ⇒ ordinal'ы `0,1,2`; value-офсеты шаблона `0,8,16`; `cacheCellCount==3`, `cacheValuesSize==24`.
- `Map_CacheValueOffsetsBakedByOrdinal` — long+int ⇒ ordinal'ы `0,1`; value-офсеты `0,8`.
- `Map_InCopiesSourceCacheOrdinal` / `Map_InPointsAtSourceOut` — In копирует ordinal источника.
- `Cache_*` (CacheTests) — write/read/IsCalculated/reset(copy шаблона)/link под забейканный `valueOffset`.
- `Map_LockstepWithPorts` / `LayoutTests` — зелёные (`RegionPtr` 16 байт + шаблон учтены в резерве; `blockSizes` не тронут).

## Self-review (итог)

- **Компиляция** — сверены символы/usings/cref'ы (грепы чистые: нет `cacheValueOffsets`/`handler.value`/`DataCache`/старого `.data`).
  Раннер не гоняли (EDM4U) — инспекция + ручная арифметика тестов + adversarial-сабагенты на промежуточных итерациях.
- **Lockstep** — `RegionPtr` 16 байт (union) + `cacheCellsTemplate` (`TSize<CacheLink>×CountCacheCells`) учтены и в резерве, и в bump.
- **Allocator-safety** — `InstanceCache` (`_cells`/`_values`/`_template`) освобождает все три в `Dispose`; шаблон — владеемая копия (без blob-ptr staleness).

## Non-goals (отложено)

- Сборка/диспатч нод по `cacheData` (ordinal → ячейка) — M6/M7.
- Бейк `link` в шаблон (passthrough) — позже; сейчас `WriteLink` runtime.
- Резолв ambient-контекста — M7.

## Deviations / История

Серия re-gate'ов: Вариант A (таблица `PtrOffset`) → inline-value (откатано) → split + 2 офсета в `RegionPtr` → **union 16 байт +
`cacheCellsTemplate` (CacheLink-шаблон) + Reset=copy** (финал, по правкам пользователя). Цель свода (плоский ordinal, slack не влияет)
достигнута; value-офсет хранится один раз (в шаблоне), не дублируется в карте.
