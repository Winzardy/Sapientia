# M7 — Orchestrator (разбивка вехи на под-фазы)

> **Статус: разбивка согласована (2026-06-18), под-фазы не начаты.** Это milestone-expansion (как
> [phase-M6/README.md](../phase-M6/README.md) для M6 и [phase-4/runtime/README.md](../phase-4/runtime/README.md)
> для Фазы 4): сначала разбить M7 на под-фазы и согласовать решения, затем исполнять по одной (каждая =
> своё ревью-сидение, свой план-гейт через `/logicgraph-phase`, компилируется, без полу-состояний).
>
> Источник правды — код, затем [STATE.md](../STATE.md). Цель вехи (PLAN.md): **dependency scheduling +
> Burst/non-Burst wave-passes + параллелизм + мульти-блюпринтовый прогон**. Здесь же `Run` переезжает из
> `NodeInvoker` и планирование вызовов переделывается.

## Контекст как-построено (что substrate уже даёт)

- **Батч-DAG.** `ExecutionGraph` (`Logic/RuntimeData/Execution/ExecutionGraph.cs`): `Inject(ref compiled,
  BlueprintInstanceId)` — chain-декомпозиция Static-топологии (батч = линейная цепочка), **накапливает
  батчи по инстансам** (multi-instance одного блюпринта уже работает; индексы потомков — глобальные).
  `Drain(Span<NodeInstanceId>)` — детерминированный single-thread обход (ready-queue, FIFO от
  `_startBatches`, декремент `remainingDeps`). `ResetDeps`/`Dispose`. `ExecutionBatch{ inDegree,
  remainingDeps (синхронный int), nextBatches, nodesOrder }`. `enum RuntimeType{Unmanaged,Managed}`.
- **Диспатч ноды.** `NodeInvoker` (`Logic/RuntimeData/Execution/NodeInvoker.cs`): `Invoke(ref scope, ref
  compiled, NodeInstanceId)` — сборка `NodeContext` из памяти инстанса + выбор бэкенда раз/ноду + вызов
  `InvokeBurst`(blittable function-table `UnsafeArray<FunctionPointer<ExecuteFn>>`, `[BurstCompile]`)/
  `InvokeManaged`(`ExecuteFn[]`). **`Run(ref scope, ref compiled, ref graph, Span<NodeInstanceId>)`** —
  run-prologue (`ResetAllCache`+`ResetDeps`) → `Drain` → per-node `Invoke`. **`Run` — временный seam M6-F,
  переезжает в M7** (память `logicgraph-run-temporary`).
- **Память инстанса.** `ExecutionScope` (`Logic/ExecutionScope.cs`): владелец `InstanceCache`/
  `InstancePersistence` (off-allocator) + `ContextRegistry<INodeContext>` + shared-копия
  `NodeFunctionRegistry _registry` (managed-поле ⇒ scope **не Burst-блиттабл**). `GetInstanceCachePtr`/
  `GetInstancePersistencePtr` отдают **blittable** `SafePtr<…>` (валидны, пока store не ресайзится).
- **Function-table.** `NodeFunctionRegistry` (`Logic/RuntimeData/Execution/NodeFunctionRegistry.cs`):
  `BurstTable` (`UnsafeArray<FunctionPointer<ExecuteFn>>`, off-allocator, **blittable**, под `#if UNITY`) +
  `ManagedTable` (`ExecuteFn[]`, managed). `UseManaged(runtimeType)` — выбор бэкенда. Индекс — ordinal
  `TypeId<ILogicNode>` (== `NodeHeader.typeId`).
- **Параллелизм-задел.** `ExecutionBatch.nextBatches` уже несёт зависимости для параллелизма;
  `remainingDeps` — синхронный счётчик; phase-4 wave-модель (директива) описывает чередование Burst↔Managed
  и parked-буфер. Форк 6 phase-4 (`iterationsToSchedule`) был **отложен «до гейта M7»** — здесь решается.
- **Кросс-среда.** Весь диспатч M6 компилируется и под Unity (Burst), и в чистом .NET (память
  `logicgraph-dispatch-cross-env`): Burst-зависимости строго под `#if UNITY_5_3_OR_NEWER`. M7 это
  наследует (threading seam — два impl).

## Решения по развилкам

| # | Развилка | Решение | Где лендится |
|---|---|---|---|
| 1 | **Сущность оркестратора + где живёт `Run`** | Новый struct (`Orchestrator`/`GraphRunner`), владеющий `ExecutionGraph` + order-буфером + next-wave-буфером; `Run(ref scope, ref storage, …)` принимает scope/storage по ref. `NodeInvoker` откатывается до **чистого диспатча** (`Invoke`/`InvokeBurst`/`InvokeManaged` остаются, `Run` уходит). Закрывает долг «`Run` временный». | **M7-A** |
| 2 | **Threading primitive (кросс-среда Unity+.NET)** | ✅ **Узкий seam «parallel-for по готовым батчам wave» с двумя impl**: `IJobParallelFor` под `#if UNITY_5_3_OR_NEWER` / `Parallel.For` (или пул) в чистом .NET. Зеркало dual-backend диспатча M6-C/D. | M7-D |
| 3 | **Бухгалтерия `remainingDeps` под параллелизмом** | ✅ **Wave-barrier**: параллелизм *внутри* wave, декремент зависимостей и набор next-ready — **сериально между wave'ами** (планировщик собирает завершения, декрементит в один поток, запускает следующую параллельную пачку). Без атомиков, детерминизм гарантирован конструктивно. **Отменяет** заложенное phase-4 (форк 2) «поток = стартовый батч + его wave». | M7-D |
| 4 | **Wave-чередование + parked-буфер (`iterationsToSchedule`, форк 6 phase-4)** | Готовые батчи бакетятся в две очереди по `RuntimeType`; исполняем очередь текущего типа wave, новые ready капаем в нужный бакет; тип wave флипается, когда текущий бакет опустел (Burst↔Managed чередование). Точную форму буфера финализировать на гейте M7-B. | M7-B |
| 5 | **Scope-в-Burst-job (blittable payload)** | `ExecutionScope` несёт managed-поле ⇒ job его не видит. Managed-glue **пре-собирает payload wave'а** (off-allocator буфер `NodeContext` + ordinals — всё blittable), Burst-job итерирует `InvokeBurst`. **Managed-wave** идёт обычным путём (без job). Чистое разделение: Burst-wave джобится, Managed-wave — нет. | M7-D |
| 6 | **Мульти-блюпринт: резолв `compiled` per-node** | Оркестратор резолвит per-node: `NodeInstanceId.blueprintId → BlueprintInstanceHeader.blueprintId (VersionedId) → storage.Get(key)`; кешировать compiled-ptr на инстанс (не лукапить на каждую ноду). Группа = **независимые** блюпринты; вложенность/ExecRef → M9. | M7-C |
| 7 | **Контракт детерминизма под параллелизмом** | Записи в cache-ячейки **непересекающиеся** (нода — в одном батче, deps упорядочены; fan-out — read-only конкурентно). M7 не трогает тела нод ⇒ детерминизм = «результат не зависит от порядка завершения». Верификация — **A/B тест** parallel-vs-serial + forceManaged-vs-Burst, сверка стейта инстансов. | спина M7-D |

## Разбивка на под-фазы

| Под-фаза | Концепт | Развилки | Статус |
|---|---|---|---|
| **M7-A — Orchestrator entity + Run relocation** | Вынести `Run` из `NodeInvoker` в новый `Orchestrator`/`GraphRunner`; владение `ExecutionGraph` + буферами; `NodeInvoker` → чистый диспатч. Single-thread, один блюпринт — **поведение не меняется**, только расположение/ownership. | 1 | ☐ не начата |
| **M7-B — RuntimeType wave-passes** | RuntimeType-aware планирование: бакетинг ready-батчей по типу, parked-буфер следующего wave, чередование Burst↔Managed. **Single-thread внутри wave** (изоляция wave-структуры от потоков). | 4 | ☐ не начата |
| **M7-C — Multi-blueprint group run** | Прогон группы *независимых* блюпринтов: per-node резолв `compiled` через scope+storage; `Inject` уже накапливает. Single-thread. | 6 | ☐ не начата |
| **M7-D — Parallelism + scope-in-job** | Параллель внутри wave: cross-env parallel-for seam (два impl); wave-barrier бухгалтерия deps; blittable payload для Burst-job; A/B детерминизм-тесты. Капстоун — параллельный мульти-блюпринт с wave-чередованием. | 2, 3, 5, 7 | ☐ не начата |

Порядок **A→B→C→D** (B↔C независимы, но D хочет оба на месте).

## Не входит (M8/M9/M10)

- **Pull-based Is-Calculated мемоизация** в run'е (гейт `NodeState.HasCache`) — **M8**.
- **ExecRef / blueprint-as-node / вложенный вызов / typed I/O** — **M9** (M7 = только *независимая* группа).
- **Кодоген** (генерируемый initializer таблицы, partial `Execute`) — **M10**; в M7 reflection-startup как в M6.

## Решения пользователя (гейт разбивки, 2026-06-18)

1. **Бухгалтерия deps (развилка 3).** ✅ **Wave-barrier** — параллель внутри wave, dep-учёт сериально
   между wave'ами. Отменяет phase-4 «поток на стартовый батч».
2. **Threading primitive (развилка 2).** ✅ **Seam с двумя impl** (Unity Jobs под `#if` / .NET parallel) —
   зеркало dual-backend.
3. **Порядок под-фаз.** ✅ **A→B→C→D** (сначала wave-структура single-thread, потом multi-blueprint, потом
   параллелизм-капстоун).

> Развилки 1, 4, 5, 6, 7 — приняты по рекомендации; точная форма каждой финализируется на план-гейте
> своей под-фазы (`/logicgraph-phase`), как было в 4A–4F / M6-A…F.

## Верификация (как в 4A–4F / M6)

Batchmode-раннер не гоняем (segfault в EDM4U — окружение). Верификация — компиляция-инспекция +
adversarial-сабагент по working-tree diff + изолированный `dotnet build` минимального репро для нетривиальной
компиляции. **Особенность M7-D:** managed-путь исполняется в plain .NET → даёт реальные A/B
детерминизм-тесты (parallel-vs-serial, forceManaged-vs-Burst) даже без Burst/Unity (Burst-таблица и
`IndexedTypes`-инициализация в EditMode — под `Assert.Ignore`, как в 4E/4F-2/M6-C).
