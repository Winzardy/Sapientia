# T01 — InstanceCache.IsCalculated(cell) + ready-helper

**Статус:** ✅ done

## Цель
Тип-агностичная проверка готовности входов ноды для оркестратора.

## Шаги
1. `InstanceCache`: + не-generic `bool IsCalculated(Id<CacheLink> cell)` — читает `state` ячейки (следуя
   `CacheState.Link`), без `T`. Рефактор: типизированный `IsCalculated<T>` делегирует в неё.
2. Ready-helper (на `CompiledBlueprintHeader` или в `Orchestrator`): `AllInputsCalculated(ref compiled, ref
   cache, nodeId)` — по In-портам ноды (`GetNodeInOut` → `RegionPtr.cacheData`) проверить все `IsCalculated`.

## Done-criteria
- Не-generic `IsCalculated(cell)` == типизированному по семантике (тест).
- Ready-helper корректен на цепочке/diamond.
