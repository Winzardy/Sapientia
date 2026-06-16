# 03 — Тесты: MapTests (flat-ordinal, value-офсеты, lockstep)

**Статус: ✅ done**

> **Финал:** MapTests — cache через `cacheData` (ordinal), value через `cacheCellsTemplate.Get(i).valueOffset`, persistence
> через `instanceData`: `Map_CacheCellsFlatOrdinalAcrossNodes` (ordinal `0,1,2` / value `0,8,16`),
> `Map_CacheValueOffsetsBakedByOrdinal` (long+int), `Map_InCopiesSourceCacheOrdinal`. CacheTests — `Create` строит шаблон, `H` без `value`.

## Цель

Зафиксировать новую Cache-раскладку: cell-офсеты плоские по ordinal (slack не влияет), value-офсеты бейкаются,
lockstep с таблицей цел; существующие Cache-ожидания остаются зелёными.

## Новые тесты (`MapTests.cs`)

- `Map_CacheCellsFlatOrdinalAcrossNodes` — три Cache-Out-ноды; средняя declares slack (`cacheSize` > 1 ячейки).
  Ассертить: cell-офсеты Out'ов = `0,16,32` (плоские, НЕ `0,16,48`); `GetCacheValueOffset(0/1/2)` = `0,8,16`.
- `Map_CacheValueOffsetsVaryingSizes` — одна нода, Cache-Out'ы `NodeOutput<long>`(8) + `NodeOutput<int>`(4).
  Ассертить: cell-офсеты `0,16`; `GetCacheValueOffset(0)==0`, `GetCacheValueOffset(1)==8` (int выровнен до 8).
- `Map_LockstepWithCacheTable` — граф с ≥2 Cache-Out'ами; `AssertLockstep` (резерв == bump) — проверяет учёт таблицы
  в `CalculateLayoutSizeToReserve`.

## Сверка существующих (должны остаться зелёными)

- `Map_OutRegionDerivedFromPortType` — node0 cache-out ordinal 0 ⇒ offset 0 ✓.
- `Map_MultipleOutsStackAlignedWithinSlice` — две cache-out одной ноды ⇒ ordinal 0,1 ⇒ `0,16` ✓.
- `Map_InPointsAtSourceOut` — In копирует cell-офсет источника ✓.
- `Map_LockstepWithPorts` — обновится автоматически (резерв учитывает таблицу) ✓.
- `LayoutTests.Layout_PerNodeSizesSumToBlockSizes` — `blockSizes[Cache]` не тронут ✓.

## Done-criteria

- Новые тесты написаны; компилируются; логика офсетов сверена инспекцией (раннер не гоняем — EDM4U).
- Существующие Cache/Layout-ожидания не сломаны (сверка вручную в self-review).

## Зависимости

Зависит от 01 (новый аксессор + поведение).

## Notes / findings

_(заполняется по ходу)_
