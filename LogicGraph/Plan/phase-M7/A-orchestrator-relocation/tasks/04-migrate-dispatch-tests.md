# T04 — Миграция NodeDispatchTests на Orchestrator

**Статус:** ✅ done

## Цель

Переключить 5 `Run_*` тестов с `ExecutionGraph` + `NodeInvoker.Run` на `Orchestrator`, сохранив **все
ассерты** (доказательство: релокация поведение не изменила).

## Затронутые тесты (`Tests/NodeDispatchTests.cs`)

- `Run_Chain_ExecutesInDependencyOrder` (==20)
- `Run_Diamond_JoinSeesBothBranches` (==23)
- `Run_MultiInstanceSameBlueprint_IndependentMemory` (10/20)
- `Run_PersistenceNode_PersistsAcrossRuns` (два прогона → 2)
- `Run_ResetsCacheEachRun_Deterministic` (детерминизм)

## Шаги (на каждый тест)

1. `var graph = ExecutionGraph.Create();` → `var orch = Orchestrator.Create();`.
2. `graph.Inject(ref compiled, id);` → `orch.Inject(ref compiled, id);`.
3. Убрать `Span<NodeInstanceId> order = stackalloc ...;` (буфер у оркестратора).
4. `NodeInvoker.Run(ref scope, ref compiled, ref graph, order)` → `orch.Run(ref scope, ref compiled)`.
5. `finally`: `graph.Dispose();` → `orch.Dispose();`.
6. Class-doc теста (упоминание `ExecutionGraph.Drain`-порядка / `NodeInvoker.Run`) — обновить на
   `Orchestrator`.

## Done-criteria

- `ExecutionGraphTests.cs` **не изменён**.
- Все 5 тестов зелёные (managed-путь, plain .NET; Burst-таблица под `Assert.Ignore` как в M6-C — проверить,
  что эти тесты на managed forceManaged и не требуют Burst).
- Нет `stackalloc`-буфера в мигрированных тестах.

## Зависимости

T02, T03.

## Notes
