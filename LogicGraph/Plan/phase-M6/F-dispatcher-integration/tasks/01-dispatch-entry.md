# 01 — `NodeInvoker.Invoke` (per-node seam) + `Run` (Drain-driver)

**Статус: ✅ done (на ревью)**

## Цель

Ввести seam, замыкающий run-путь: per-node `Invoke` (сборка `NodeContext` из памяти инстанса + dispatch) и
driver `Run` (прогон `ExecutionGraph.Drain`-порядка). Оба — в `Logic/RuntimeData/Execution/NodeInvoker.cs`.

## Шаги

1. `NodeInvoker.Invoke(ref ExecutionScope scope, ref CompiledBlueprintHeader compiled, NodeInstanceId node)`:
   - `ref` память инстанса: `cachePtr = scope.GetInstanceCachePtr(node.blueprintId)`,
     `persistencePtr = scope.GetInstancePersistencePtr(node.blueprintId)`.
   - `ctx = new NodeContext { compiled = compiled.AsSafePtr(), cache = cachePtr, persistence = persistencePtr, nodeId = node.nodeId }`.
   - `ordinal = compiled.GetNodeTypeId(node.nodeId)` (неявный → `int`, без явного каста — память
     `no-explicit-id-int-cast`); `rt = compiled.GetNodeRuntimeType(node.nodeId)`.
   - `scope.Registry.Invoke(ordinal, rt, ref ctx)`.
2. `NodeInvoker.Run(ref ExecutionScope scope, ref CompiledBlueprintHeader compiled, ref ExecutionGraph graph, Span<NodeInstanceId> orderBuffer) → int`:
   - run-prologue: `scope.ResetAllCache(); graph.ResetDeps();`
   - `var n = graph.Drain(orderBuffer);`
   - `for (i in 0..n) Invoke(ref scope, ref compiled, orderBuffer[i]);`
   - `return n;`
3. Оба метода **managed** (не `[BurstCompile]`); `Execute<T>`/`Compile<T>` не трогаем. XML-doc на русском
   (как в файле): seam managed, Burst-горячий путь — внутри `registry.Invoke`; wave/job — M7.

## Done-criteria

- Компилируется в обе среды (Unity/.NET); `compiled` берётся **через `ref`** (резолв self-relative — не на копии).
- `Invoke` не аллоцирует; `Run` использует переданный `orderBuffer` (не аллоцирует список порядка).

## Зависимости

- Task 02 (аксессоры `Registry`/`GetInstance*Ptr`/`GetNodeRuntimeType`) — строить параллельно; `Invoke` их зовёт.

## Notes / findings

_(заполняется по ходу)_
