# Под-фаза 4B — ExecutionGraph (батч-DAG + детерминированный ордеринг)

**Статус: 🔄 in progress (план, гейт).** Развилки: 2 (`AsyncValue`→синхронный `int`), 4 (Inject — runtime),
5 (снос `IterationTo`), 6 (`iterationsToSchedule`). Обзор — [../README.md](../README.md).

## Цель

Из Static-топологии (`NodeMapHeader`, 4A) инстанцировать **per-instance батч-DAG** и прогнать его в
**детерминированном порядке зависимостей** (single-thread, без реального исполнения тел нод) + `Dispose`.
Это substrate под оркестратор: джоб-параллелизм, чередование Burst/non-Burst (wave-модель) и реальный
диспатч — **M6/M7**.

**Батч = линейная цепочка нод** (директива): идут последовательно, не ждут, не ветвятся. Цепочка `P→N`
продолжается ⟺ `outDeg(P)==1 && inDeg(N)==1`; иначе `N` начинает новый батч. Свойство цепочки даёт чистую
сборку DAG: межбатчевые рёбра выходят **только из хвоста** батча, а в голову join-батча входят рёбра из
**различных** батчей ⇒ `previousBatchesCount(B) == inDegree(head B)`.

## Решения на гейт (рекомендации)

1. **Батч-DAG строится в runtime (`Inject`) из Static relatives**, не бейкается в Static. Почему: батч несёт
   **мутабельный per-instance** стейт (`remainingDeps`, `NodeInstanceId`) — он рантаймовый; Static остаётся
   минимальным, без lockstep-стоимости переменных батч-массивов; форк 4 («Inject — runtime»). Форма цепочек
   одинакова для всех инстансов версии — **кеширование шаблона = M7** (сейчас пересчёт на каждый `Inject`).
2. **`ExecutionGraph` пересобирается вокруг батч-DAG + drain.** Сносится мёртвый каркас: `IterationTo` (форк 5),
   курсорный `TryRun`/`currentIteration`/`runtimes: UnsafeArray<ExecutionRuntime>`/`ExecutionIteration`,
   `iterationsToSchedule`, `AsyncValue`. **Wave-модель (RuntimeType-бакетинг батчей + буфер следующего wave +
   чередование Burst↔Managed) переносится в 4D/M7** — она требует `NodeHeader.runtimeType` (форк 8, ещё не
   проставляется: все ноды сейчас `Unmanaged` ⇒ один wave). `enum RuntimeType` оставляем. Интент wave —
   задокументирован в [../README.md](../README.md).
3. **Правило цепочки** (выше) — подтвердить.

## Файлы

- **M** `Logic/RuntimeData/Execution/ExecutionGraph.cs` — пересобрать (см. раскладку); снести
  `IterationTo`/`ExecutionIteration`/`ExecutionRuntime`/курсор; `ExecutionBatch` под синхронный счётчик.
- **A** `Tests/ExecutionGraphTests.cs` — chain-coalescing, diamond, parallel, multi-instance, reset, dispose.

## Раскладка данных

```csharp
public struct ExecutionGraph : IDisposable
{
    Id<MemoryManager> _memoryId;
    UnsafeList<ExecutionBatch> _batches;  // батч-DAG (накопительно по Inject); индекс = глобальный id батча
    UnsafeList<int>            _startBatches; // индексы батчей с inDegree 0 (старты)

    public bool IsCreated { get; }
    public int  BatchCount { get; }

    public static ExecutionGraph Create(Id<MemoryManager> memoryId = default);

    // Инстанцирует батч-DAG блюпринта под инстанс: chain-декомпозиция Static-топологии → батчи с
    // NodeInstanceId(instance, nodeId). Возвращает индекс первого добавленного батча (смещение).
    public int Inject(ref CompiledBlueprintHeader compiled, BlueprintInstanceId instance);

    public void ResetDeps();  // remainingDeps = inDegree для всех (перед каждым прогоном)

    // Детерминированный обход (ready-queue от startBatches, FIFO; декремент remainingDeps).
    // Пишет порядок нод в orderOut, возвращает их число. Тела нод НЕ исполняются (M6).
    public int Drain(Span<NodeInstanceId> orderOut);

    public void Dispose();
}

public struct ExecutionBatch
{
    public int inDegree;       // исходное число батчей-предшественников (для ResetDeps)
    public int remainingDeps;  // синхронный счётчик (форк 2; без Interlocked — потоки делятся по startBatches)
    public UnsafeList<int>            nextBatches; // индексы зависимых батчей (могут идти параллельно — M7)
    public UnsafeList<NodeInstanceId> nodesOrder;  // ноды цепочки последовательно
}

public enum RuntimeType : byte { Unmanaged, Managed }  // оставлен для 4D/M7
```

## Алгоритм `Inject` (chain-декомпозиция, unmanaged)

1. `baseBatch = _batches.count`. `n = compiled.NodesCount`. Temp `UnsafeArray<int> batchOf` (init −1).
2. **Heads:** нода `h` — голова батча ⟺ `inDeg(h)!=1` ИЛИ (`inDeg(h)==1` И `outDeg(pred)!=1`),
   где `pred = relatives(h).inputs[0]`. (`inDeg`=`GetNodeInDegree`, `outDeg`=`relatives.outputs.Length`.)
3. **Цепочки:** для каждой головы `h` создать батч `b`; `cur=h`; цикл: `batchOf[cur]=b`,
   `nodesOrder.Add(NodeInstanceId(instance, cur))`; если `outDeg(cur)==1` и у единственного потомка `s`
   `inDeg(s)==1` → `cur=s`; иначе стоп.
4. **Рёбра/счётчики:** для каждого батча `b` голова `head` → `inDegree = GetNodeInDegree(head)`
   (== число батчей-предшественников, по свойству цепочки); хвост `tail` → для каждого потомка `s` из
   `relatives(tail).outputs` добавить `nextBatches.Add(batchOf[s])` (потомки различны ⇒ без дедупа).
5. `remainingDeps = inDegree`; если `inDegree==0` → `_startBatches.Add(b)`. Освободить temp.

> Корректность опирается на свойство цепочки (рёбра — только из хвоста; предшественники головы — из разных
> батчей). Тесты проверяют это на ромбе/цепочке/параллели.

## Шаги исполнения

1. Пересобрать `ExecutionGraph.cs`: `ExecutionBatch` (синхронный счётчик), `Create`/`IsCreated`/`BatchCount`.
2. `Inject` (алгоритм выше) + temp-аллокации (off-alloc, освобождать).
3. `ResetDeps`, `Drain` (ready-queue), `Dispose` (вложенные `UnsafeList` + внешние).
4. Снос `IterationTo`/`ExecutionIteration`/`ExecutionRuntime`/`TryRun`/`iterationsToSchedule`/`AsyncValue`.
5. `ExecutionGraphTests` (рядом).
6. Self-review (Step 4): чек-лист + adversarial-сабагент. Прогон отложен (EDM4U).

## Задачи

| # | Таска | Статус |
|---|---|---|
| 01 | [batch-DAG + Inject](tasks/01-inject.md) — типы, chain-декомпозиция, рёбра/счётчики | ☐ |
| 02 | [drain + reset + dispose + cleanup](tasks/02-drain-dispose.md) — ready-queue обход; снос мёртвого каркаса | ☐ |
| 03 | [tests](tasks/03-tests.md) — chain/diamond/parallel/multi-instance/reset/dispose | ☐ |

## Тест-лист (stub-ноды, синтетический `BlueprintInstanceId`)

- **Chain coalescing**: `A→B→C` → **1 батч**, `nodesOrder=[A,B,C]`, `Drain` = [A,B,C].
- **Diamond**: `A→B,A→C,B→D,C→D` → 4 батча; `inDegree`: A=0,B=1,C=1,D=2; start=[A]; `Drain` уважает
  зависимости (A первый, D последний).
- **Parallel**: 3 несвязанные → 3 батча, все в `startBatches`.
- **Multi-instance**: два `Inject` одного bp с разными `BlueprintInstanceId` → батчи накапливаются, индексы
  потомков смещены корректно; `Drain` обходит оба под-DAG'а; `NodeInstanceId` несёт верный инстанс.
- **ResetDeps**: после `Drain` (`remainingDeps`=0) → `ResetDeps` восстанавливает `inDegree`; повторный `Drain`
  даёт тот же порядок.
- **Dispose**: вложенные `UnsafeList` освобождены, без двойного free; `IsCreated`=false.

## Non-goals (отложено)

- **Wave-модель / RuntimeType-бакетинг / буфер следующего wave / чередование Burst↔Managed** — **4D/M7**
  (нужен `runtimeType`, форк 8). Сейчас один wave, все ноды `Unmanaged`.
- **Джоб-параллелизм** (`nextBatches` параллельно, порог) — **M7**. Здесь `Drain` строго single-thread.
- **Реальный диспатч/исполнение тел нод** — **M6**. `Drain` только упорядочивает.
- **Кеш шаблона батчей по версии** — **M7** (сейчас пересчёт на `Inject`).
- **Источник `BlueprintInstanceId`** (`ExecutionScope`/`Storage`) — **4F**; в тестах — синтетический id.

## Отклонения

- Сносится существующий каркас `ExecutionGraph` (runtimes/iterations/курсор/`AsyncValue`) — он был
  заглушкой («двигал курсор»), поведения не нёс; wave-машинерия пересоберётся в 4D/M7 с реальной семантикой.
