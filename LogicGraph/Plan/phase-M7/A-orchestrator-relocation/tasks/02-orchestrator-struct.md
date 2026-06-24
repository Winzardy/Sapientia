# T02 — Orchestrator struct

**Статус:** ✅ done

## Цель

Новая сущность-оркестратор, владеющая `ExecutionGraph` + буфером порядка; переносит run-прогон из
`NodeInvoker.Run` без изменения поведения.

## Шаги

1. `Logic/RuntimeData/Execution/Orchestrator.cs`, namespace `Sapientia.LogicGraph`,
   `struct Orchestrator : IDisposable`.
2. Поля: `_memoryId`, `ExecutionGraph _graph`, `UnsafeArray<NodeInstanceId> _order`.
3. `Create(Id<MemoryManager> memoryId = default)` — `_graph = ExecutionGraph.Create(memoryId)`; `_order`
   ленивый (не создаём здесь).
4. `Inject(ref CompiledBlueprintHeader compiled, BlueprintInstanceId instance)` → `_graph.Inject(...)`,
   вернуть результат.
5. `Run(ref ExecutionScope scope, ref CompiledBlueprintHeader compiled)`:
   - `EnsureOrder(_graph.NodeCount)` — если `!_order.IsCreated || _order.Length < need` → Dispose+realloc
     `new UnsafeArray<NodeInstanceId>(_memoryId, need)`.
   - тело как в текущем `NodeInvoker.Run`: `scope.ResetAllCache(compiled.GetCacheCellsTemplate())`,
     `_graph.ResetDeps()`, `var count = _graph.Drain(_order.GetSpan())`, цикл `NodeInvoker.Invoke(ref
     scope, ref compiled, _order[i])`, `return count`.
6. `IsCreated => _graph.IsCreated`; `Dispose()` — `_order.Dispose()` (если создан) + `_graph.Dispose()` +
   `this = default`.
7. Комментарии — русские, в стиле `ExecutionGraph`/`NodeInvoker`; отметить, что single-thread/один
   блюпринт, wave/параллелизм/мульти-блюпринт — M7-B/C/D.

## Done-criteria

- Прогон побайтово эквивалентен текущему `NodeInvoker.Run` (тот же prologue → Drain → Invoke).
- Буфер переиспользуется между прогонами; растёт только при росте `NodeCount`; освобождается в `Dispose`
  (нет утечки off-allocator памяти).
- Компилируется под Unity и .NET.

## Зависимости

T01 (`NodeCount`).

## Notes

- **Self-review (adversarial-сабагент).** Найден латентный дефект: `Run` без `Inject` (`NodeCount == 0`)
  вёл к `EnsureOrder(0)` → `MakeArray(0)`, который роняет DEBUG-assert (`MemoryManager.cs:204`) —
  ужесточение контракта против старого `stackalloc[0]`. **Фикс:** `EnsureOrder` рано выходит при
  `need == 0`, `Run` подаёт `Span<NodeInstanceId>.Empty` в `Drain` (0 нод, без аллокации). Добавлен
  регрессионный тест `Run_NoInject_ReturnsZero`.
- Попутно: поправлен dangling cref `NodeInvoker.Run` → `Orchestrator.Run` в `ExecutionScope.cs`.
- Убран спекулятивный `BatchCount` (YAGNI).
