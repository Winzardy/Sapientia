# 02 — drain + reset + dispose + cleanup

**Статус: ☐ todo** · Под-фаза [4B](../plan.md) · Развилки 5, 6.

## Цель

Детерминированный обход батч-DAG + жизненный цикл; снос мёртвого каркаса `ExecutionGraph`.

## Шаги

1. `ResetDeps()` — `remainingDeps = inDegree` по всем батчам.
2. `Drain(Span<NodeInstanceId> orderOut) → int` — ready-queue (`UnsafeList<int>` или индекс-курсор) от
   `_startBatches` (в порядке), FIFO: pop → дописать `nodesOrder` в `orderOut` → по `nextBatches` декремент
   `remainingDeps`, при 0 enqueue. Возврат — число записанных нод. Тела нод не исполняются (M6).
3. `Dispose()` — освободить вложенные `nextBatches`/`nodesOrder` каждого батча, затем `_batches`/`_startBatches`;
   идемпотентно (`this = default`).
4. **Cleanup:** удалить `IterationTo`, `ExecutionIteration`, `ExecutionRuntime`, курсорный `TryRun`,
   `currentIteration`, `iterationsToSchedule`, `runtimes`, использование `AsyncValue`. `enum RuntimeType` оставить.

## Done-criteria

- `Drain` уважает зависимости (ромб); `ResetDeps`→повторный `Drain` воспроизводим; нет утечек/двойного free;
  мёртвый каркас удалён, компилируется.

## Зависимости

- Таска 01.

## Заметки

- `Drain` детерминирован: `_startBatches` в порядке вставки + FIFO-очередь. Без Random/wall-clock.
- Реентерабельность буфера обхода: либо локальная temp-очередь (off-alloc, освободить), либо переиспользуемое
  поле — решить при реализации (temp проще, без скрытого стейта).
