# 01 — batch-DAG + Inject

**Статус: ✅ done** · Под-фаза [4B](../plan.md) · Развилки 2, 4 (Inject).

## Цель

Типы `ExecutionGraph`/`ExecutionBatch` (синхронный счётчик) + `Inject`: chain-декомпозиция Static-топологии
(`NodeMapHeader`, 4A) в per-instance батч-DAG с `NodeInstanceId`.

## Шаги

1. `ExecutionBatch{ inDegree, remainingDeps, nextBatches: UnsafeList<int>, nodesOrder: UnsafeList<NodeInstanceId> }`.
2. `ExecutionGraph{ _memoryId, _batches, _startBatches }` + `Create`/`IsCreated`/`BatchCount`.
3. `Inject(ref CompiledBlueprintHeader, BlueprintInstanceId)` — алгоритм heads → chains → рёбра/счётчики
   (см. [plan.md](../plan.md)); temp `UnsafeArray<int> batchOf` (off-alloc, освободить); смещение индексов
   на `baseBatch = _batches.count` для multi-instance.

## Done-criteria

- Компилируется; батч-DAG корректен на цепочке/ромбе/параллели (тесты 03); `previousBatchesCount`/start верны.

## Зависимости

- 4A (`GetNodeRelatives`/`GetNodeInDegree`/`NodesCount`); `NodeInstanceId`/`BlueprintInstanceId`.

## Заметки

- `nextBatches` хранит **глобальные** индексы (с `baseBatch`-смещением) — для multi-instance накопления.
