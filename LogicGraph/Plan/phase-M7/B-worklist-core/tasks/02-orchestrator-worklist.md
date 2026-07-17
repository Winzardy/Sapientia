# T02 — Orchestrator work-list

**Статус:** ✅ done

## Цель
Демандный ready-driven прогон вместо батч-DAG.

## Шаги
1. Поля: `_queue` (UnsafeList<NodeInstanceId>, FIFO cursor), `_queued` bitset (NodesCount на инстанс,
   per-instance transient — место в кеш-пространстве), `_memoryId`.
2. `Inject(ReadOnlySpan<NodeInstanceId> entries)` — enqueue (с дедупом).
3. `Run(ref scope, ref compiled)`: пока queue не пуст → взять N → если выход посчитан: push консьюмеров,
   continue → ready-check: нет → pull (незакеш. продюсеры в хвост) + ре-энкью N → да → `NodeInvoker.Invoke`,
   push консьюмеров. Сброс `_queued`/cursor в начале.
4. `Dispose`.

## Done-criteria
- Цепочка/diamond/мульти-инстанс/persistence/детерминизм/пустой — как в M7-A-тестах.
- Нода исполняется ≤1 раза (мемоизация).
