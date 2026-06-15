# 03 — ExecutionGraphTests

**Статус: ✅ done** · Под-фаза [4B](../plan.md).

## Цель

`Tests/ExecutionGraphTests.cs` — доказать chain-декомпозицию, батч-DAG и детерминированный обход на
stub-графах. Инстанс — синтетический `BlueprintInstanceId` (без `ExecutionScope`, он 4F).

## Тесты

- `Execution_ChainCoalesced` — `A→B→C` → 1 батч, `nodesOrder=[A,B,C]`, `Drain`=[A,B,C].
- `Execution_Diamond` — 4 батча; `inDegree` A0/B1/C1/D2; start=[A]; `Drain` уважает зависимости.
- `Execution_ParallelIndependent` — 3 батча, все старты; `Drain` = все 3 ноды.
- `Execution_MultiInstance` — два `Inject` (разные id) → накопление; `Drain` обходит оба; `NodeInstanceId.blueprintId` верный.
- `Execution_ResetDepsReproducible` — `Drain` → `ResetDeps` → `Drain` даёт тот же порядок.
- `Execution_DisposeFreesNested` — после `Dispose` `IsCreated`=false; повторный `Dispose` — no-op.

## Done-criteria

- Тесты написаны, компилируются. Прогон отложен (EDM4U) — риск в пакете.

## Зависимости

- Таски 01, 02; `StubNode`/`StubBlueprint` (есть), `BlueprintCompiler.CompileLayout`.

## Заметки

- Синтетический id: `new BlueprintInstanceId { id = 1, generation = 1 }` (или через `BlueprintInstanceStorage.Add`).
- Сравнение порядка: собрать `Drain` в массив `NodeInstanceId`, проверить позиции/индексы нод.
