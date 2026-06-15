# Runtime/Cache/Execution — доделка каркаса (продолжение Фазы 4)

> **Контекст.** Static-модель (`CompiledBlueprintHeader`/`NodeHeader`/`RegionPtr`/Map) и instance
> identity (`BlueprintInstanceHeader`/`Storage`/`Id`) — done (см. [../../STATE.md](../../STATE.md)).
> Осталось доделать **runtime/cache/execution** слой: он компилируется, но поведения нет, и был
> заблокирован 8 развилками ([STATE.md §4](../../STATE.md)). Этот файл — **решения по развилкам** + разбивка
> доделки на под-фазы (каждая = одно ревью-сидение, компилируется, без полу-состояний).
>
> Источник правды — код, затем [STATE.md](../../STATE.md). Порядок под-фаз = [STATE.md §5](../../STATE.md).

## Решения по 8 развилкам (на гейт)

| # | Развилка | Решение | Где лендится |
|---|---|---|---|
| 1 | **CacheData ↔ RegionPtr** | Cache-регион инстанса = массив ячеек `DataCache<T>` (мемоизация Is-Calculated + passthrough-link). `RegionPtr` остаётся **static-проводкой** (регион + офсет); для Cache-портов офсет = офсет ячейки `DataCache` в Cache-блоке. Резолв `база Cache + офсет → ref DataCache<T>` делает `CacheHeader`/`CacheHandler`. **Следствие:** Cache-слайсы размечаются в байтах ячеек (`TSize<DataCache<T>>`), не сырого `T` — кодоген/нода объявляет `DataSizes.Cache` уже в терминах ячеек. | 4C (CacheHeader) |
| 2 | **`ExecutionBatch.previousBatchesCount`** | `AsyncValue<int>` (lock с thread-id, не счётчик) → **синхронный `int remainingDeps`** (без `Interlocked`) + `int inDegree` для реинициализации перед каждым run. **Потоки делятся относительно стартовых батчей** (один start-батч + его wave = один поток), поэтому редактирование `remainingDeps` остаётся **в рамках одного потока** — атомарность не нужна. Детерминизм цел (результат от порядка не зависит). | 4B (ExecutionGraph) |
| 3 | **`NodeMapHeader` — аллокация и кратность** | Топология **бейкается в Static-блоб** (инстанс-агностична, детерминирована из `inputToOutput`, дедуп вместе с блобом): `NodeMapHeader{ relatives[], startNodes[] }`, per-node `NodeRelativesHeader{ inputs[], outputs[] }` (id предшественников/потомков, дедуп по ноде). Аллоцируется в `SetupLayout`, добавлено в `CalculateLayoutSizeToReserve` (lockstep через общий хелпер `BuildAdjacency`). Поле `nodesMap` в `CompiledBlueprintHeader` теперь реально аллоцируется. | **4A (эта под-фаза)** |
| 4 | **Методы `NodeMapHeader` (Build/Inject)** | `BuildAdjacency`/`BuildNodeMap` — **на этапе компиляции, Static** (читает связи, бейкает relatives+startNodes — инстанс-агностично). `Inject(BlueprintInstanceId)` — **на runtime `ExecutionGraph`**: инстанцирует батч-DAG из Static-топологии под конкретный инстанс (`NodeInstanceId` + атомарный счётчик). | 4A (Build) + 4B (Inject) |
| 5 | **`IterationTo`** | **Удалить** — мёртвый дубль полей `ExecutionIteration` (`batches`+`startBatches`). Шаблон теперь — Static-топология (`NodeMapHeader`). | 4B |
| 6 | **`iterationsToSchedule`** | **Оставить как буфер следующего wave** (НЕ удалять). По wave-модели (ниже): во время wave новые батчи добавляются в обработку; если батч **того же `RuntimeType`**, что исполняется сейчас — модифицируем связи и продолжаем текущий wave; если **другого `RuntimeType`** — паркуем в этот буфер для **следующего wave** (чередование Burst↔Managed). Точную форму спроектировать на гейте 4B. | 4B |
| 7 | **Семантика `NodeState`** | Подтверждаю: `HasCache` = нода имеет хотя бы один Out в Cache-регионе → её результат мемоизируется (гейт Is-Calculated, M8); выставляется в `SetupLayout` по региону Out'ов. `Multiple` = Out ноды читают ≥2 потребителя (fan-out) → ячейка живёт, пока не прочитают все; выставляется по `relatives.outputs.Length > 1`. | 4D (wiring) |
| 8 | **Wiring `runtimeType`** | Добавить `INode.RuntimeType => RuntimeType.Unmanaged` (default); `SetupLayout` пишет `header.runtimeType = node.RuntimeType`. Тривиально. | 4D (wiring) |

## Разбивка на под-фазы (порядок [STATE.md §5](../../STATE.md))

| Под-фаза | Концепт | Развилки | Статус |
|---|---|---|---|
| **[4A — NodeMapHeader](A-nodemap/plan.md)** | Топология (relatives + startNodes) в Static-блобе + `BuildAdjacency` (lockstep) + тесты; выделен `BlueprintCompiler` | 3, 4 (Build) | ✅ одобрено (не закоммичено) |
| **[4B — ExecutionGraph](B-execution/plan.md)** | `Inject` (инстанцирование батч-DAG из Static), детерминированный батч-ордеринг + `Dispose`; снос `IterationTo`/курсора/`iterationsToSchedule`/`AsyncValue`; синхронный `int`. **Батч = линейная цепочка** нод | 2, 4 (Inject), 5, 6 | ✅ одобрено |
| **[4C — CacheHeader](C-cache/plan.md)** | alloc/read/write Cache-блока (ячейки `DataCache`), Is-Calculated-мемоизация, резолв link (passthrough); wiring `NodeIn`/`NodeOut` | 1 | ✅ одобрено |
| **4D — runtimeType/NodeState** | `INode.RuntimeType` (default `Unmanaged`) → `NodeHeader.runtimeType`; флаги `NodeState` (`HasCache`/`Multiple`) битовой маской `ByteEnumMask<NodeState>` в `SetupNodeFlags` (после `BuildNodeMap`) | 7, 8 | ✅ (закоммичено, не запушено) |
| **4E — ContextType** | Ноды объявляют `TypeId<INodeContext>[]`; компилятор бейкает дедуп-union в `CompiledBlueprintHeader.contextTypes` (`BumpArray`, сортирован по id, span-аксессор `GetContextTypes`); на ноде не хранится. Маркер категории `INodeContext : IIndexedType`. Runtime-реестр-владелец — 4F | — | ✅ (закоммичено, не запушено) |
| **4F-1 — ExecutionScope (память+lifecycle)** | Владелец per-instance памяти + трекер инстансов: `CreateInstance`/`Dispose`/`Reset`/`Get*` поверх `BlueprintInstanceStorage`. `InstanceCache`/`InstancePersistence` — обёртки над `UnsafeArray<byte>`/`UnsafeArray<DataCache>` (off-allocator). Кеш переделан: `DataCache` = метаданные (state+valueOffset+link, union), значения отдельно | — | ✅ (закоммичено, не запушено) |
| **[4F-2 — Context-реестр](F-context/plan.md)** | `ContextRegistry<TContext>` на `ExecutionScope` (generic по категории, scope: `INodeContext`): `UnsafeArray<SafePtr>` размера `TypeId<TContext>.Count`, **владеет** памятью контекстов (lazy `MemAlloc(TSize<T>)` при set). API generic-only `SetContext<T>`/`GetContext<T>`/`HasContext<T>` (без id/count). Round-trip тесты отложены (`IndexedTypes` не init в EditMode) | — | ✅ (закоммичено, не запушено) |
| **4F-3 — Кеш-долг 4F-1** | Свести дуальную кеш-раскладку компилятора (старая per-node `DataSizes.Cache`/`CacheCellSize`/Cache-`RegionPtr` ↔ новая `cacheCellCount`/`cacheValuesSize`): **развилка** — как Cache-порт получает два офсета (cell+value); чистка dead-code `BumpHeader.Reset/Size`/`RawBumpAllocator.Size`; сверка `MapTests`/`LayoutTests` | — | ☐ todo (свой гейт) |

## Wave-модель исполнения (директива пользователя — для 4B/M7)

- **Батч** = группа нод, идущих **последовательно**, никого **не ждут** и **не ветвятся** (линейная цепочка).
  Цепочка рвётся на ветвлении (>1 потомок), join'е (>1 предшественник) или смене `RuntimeType`.
- **Потоки делятся относительно стартовых батчей** — один start-батч и его wave исполняются одним потоком,
  поэтому `previousBatchesCount` (`remainingDeps`) редактируется в рамках одного потока (форк 2: синхронно).
- **Wave:** исполняется текущий `RuntimeType`. Нода/батч может породить новые батчи:
  - **тот же `RuntimeType`** → модифицируем связи и продолжаем **текущий** wave;
  - **другой `RuntimeType`** → паркуем в буфер (`iterationsToSchedule`) на **следующий** wave (Burst↔Managed
    чередование). Форк 6.

> **Не входит (M6/M7/M8):** реальный диспатч нод (Burst fn-pointer registry by index + managed-путь —
> M6); джоб-параллелизм + чередование Burst/non-Burst pass (M7); pull-based Is-Calculated gating в run'е
> (M8). Под-фазы 4A–4F строят **substrate** под это (детерминированный single-thread ордеринг, без
> исполнения тел нод).
