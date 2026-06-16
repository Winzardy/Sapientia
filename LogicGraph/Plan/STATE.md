# LogicGraph — текущее состояние (снимок 2026-06-14)

> **Что это.** Единый источник правды о том, что **уже собрано в коде** (а не задумано). Дополняет
> [PLAN.md](../PLAN.md) (роадмап) и [CLAUDE.md](../CLAUDE.md) (дизайн-навигатор). Если расходятся —
> **прав код**, затем этот файл, затем PLAN.md, затем CLAUDE.md (его §3/§4/§7 описывают **до-рефакторную**
> модель: 5 scope, `CompiledBlueprint`, edge-модель, `NodeInvoker` — всё это **снесено**, см. ниже).
>
> **Ветка/коммит.** Submodule `Sapientia`, ветка `rnd/nodes_graph`. Этот снимок зафиксирован коммитом
> «LogicGraph: Static/Runtime 3-region model + runtime/cache/execution scaffold».
>
> **Тесты.** Не прогонялись: локальный Unity batchmode-раннер падает нативным segfault в Google EDM4U
> (окружение, не LogicGraph) — по решению пользователя не гоняем. Верификация — компиляция + инспекция +
> adversarial-сабагенты. Код **компилируется** (ссылок на снесённые символы нет, все API реальны — проверено).

---

## 1. Доменная модель (как спроектировано)

Три региона памяти ноды/блюпринта — `MemoryRegion { Static, Cache, Persistence }`
([MemoryRegion.cs](../Blueprint/MemoryRegion.cs)). Старая 5-scope `DataLayout` снесена;
`StaticCache`/`StaticPersistent` **не существуют**.

- **Static** — read-only, единственный экземпляр на приложение (дедуп по `VersionedId<Blueprint>`),
  живёт в собственной bump-арене. Состав: **Data** (на ноду: индекс метода + прямая self-relative ссылка
  на static-слайс) + **Map** (на ноду блок In/Out как массив `RegionPtr`) + **ContextType**
  (`BumpArray<TypeId<INodeContext>>` — дедуп-union по нодам, сортирован по id; **4E done**). Компиляция —
  managed-путь (editor/server), не Burst.
- **Runtime** — per-instance, **вся память off-allocator** (персистентность — через будущий слой **State**,
  не через снапшот мира ⇒ `World`/`CachedPtr`/`MemPtr` для рантайма не нужны). Состав: **Cache** (сброс
  каждый run), **Persistence** (постоянные данные нод), **Map** (топология для шедулинга), **Context**.
- **State** (позже) — `Persistence` + `Static` → восстановление `Runtime`. Сейчас не проектируем.

---

## 2. Карта сущностей (что в коде)

### Static — read-only блоб (✅ собран, протестирован инспекцией)

| Файл | Что | Статус |
|---|---|---|
| [CompiledBlueprintHeader.cs](../Logic/StaticData/CompiledBlueprintHeader.cs) | Static-блоб — **чистая рантайм-структура** (о authoring-`Blueprint` не знает): `nodes: BumpArray<NodeHeader>`, `blockSizes: DataSizes`, `blueprintKey: VersionedId<Blueprint>` (тег идентичности), `nodesMap`, `contextTypes: BumpArray<TypeId<INodeContext>>` (4E); аксессоры `GetStaticNodeSlice`/`GetNodeInOut`/`GetNodePersistenceOffset`/`GetBlockSize`/`GetNodeRelatives`/`GetNodeInDegree`/`StartNode*`/`GetContextTypes` (span) | ✅ |
| [INodeContext.cs](../Blueprint/INodeContext.cs) | Маркер категории ambient-контекста: `interface INodeContext : IIndexedType`. Нода объявляет нужные контексты как `TypeId<INodeContext>[]` (`INode.GetContextTypes`); компилятор бейкает дедуп-union в блоб | ✅ **(4E)** |
| [BlueprintCompiler.cs](../Logic/StaticData/BlueprintCompiler.cs) | **Вся компиляция** `Blueprint → CompiledBlueprintHeader` (единственное место, знающее об authoring): `CalculateLayoutSizeToReserve`⟷`SetupLayout`/`SetupMap`/`BuildNodeMap` (lockstep), `BuildAdjacency`. Выделен из заголовка (ревью 4A) | ✅ |
| [NodeHeader.cs](../Logic/StaticData/NodeHeader.cs) | На ноду: `typeId`, `runtimeType` (форк 8: из `INode.RuntimeType`), `state: ByteEnumMask<NodeState>` (форк 7), `staticData: RelativePtr`, `persistence: PtrOffset`, `inOut: PtrOffset`; `enum NodeState : byte {HasCache=0, Multiple=1}` (члены — индексы бит, не `[Flags]`) | ✅ **(4D)** |
| [RegionPtr.cs](../Blueprint/RegionPtr.cs) | Указатель Map одного In/Out: `{ MemoryRegion region; RelativePtr<byte> data }`. Static → self-relative (резолв на месте), Cache/Persistence → `data.byteOffset` = офсет в блоке региона | ✅ |
| [MemoryRegion.cs](../Blueprint/MemoryRegion.cs) | `MemoryRegion{Static,Cache,Persistence}` + `DataSizes` (3-региона fixed-буфер, `Alignment=8`) | ✅ |
| [CompiledBlueprintStorage.cs](../Logic/StaticData/CompiledBlueprintStorage.cs) | «БД» скомпилированных блобов: `Add(arena, offsets)`, дедуп + сосуществование версий по `(id,version)`, jump-by-id, never-remove (Dispose-only) | ✅ (Фаза 3) |
| [NodeMapHeader.cs](../Logic/StaticData/NodeMapHeader.cs) | Топология (граф связей) для шедулинга: `relatives: BumpArray<NodeRelativesHeader>` (per-node), `startNodes: BumpArray<Id<NodeHeader>>`; `NodeRelativesHeader{inputs/outputs: BumpArray<Id<NodeHeader>>}` + `InDegree` | ✅ **(4A)**: бейкается в блоб из связей (дедуп по ноде, precalc/висячий/самопетля рёбер не дают), lockstep; покрыто `NodeMapTests`. Build — компиляция (форки 3,4 решены); Inject — 4B |

**Map строится из связей** (`Blueprint.inputToOutput` + порты): регион Out выводится из типа порта
(`IsPreCalculated`→Static с бейком дефолта, `IsPersistent`→Persistence, иначе Cache); In копирует указатель
своего источника; константы — отдельными аллокациями в хвосте Static. `NodeHeader.inOut` — `PtrOffset` от
позиции `CompiledBlueprintHeader` на блок байт `RegionPtr` (сначала In'ы, затем Out'ы). Покрыто
`LayoutTests`/`MapTests`.

### Runtime — per-instance (🟡 каркас, поведение не реализовано)

| Файл | Что | Статус |
|---|---|---|
| [BlueprintInstanceHeader.cs](../Logic/BlueprintInstanceHeader.cs) | Чистая рантайм-сущность: `blueprintId: VersionedId<Blueprint>` + `instanceCache`/`instancePersistent: PtrOffset` (абстрактные офсеты, без Mem/WorldState); `Create(in compiled, …)` | ✅ |
| [BlueprintInstanceStorage.cs](../Logic/BlueprintInstanceStorage.cs) | Хранилище живых инстансов: `UnsafeIndexAllocSparseSet` + per-slot **generation** (staleness); `Add/Has/TryGet/Remove/Values/Dispose` | ✅ |
| [BlueprintInstanceId.cs](../Logic/BlueprintInstanceId.cs) | Хендл `{Id id; int generation; long _raw}` (explicit layout, `IEquatable`); `generation==0` — невалид | ✅ |
| [NodeInstanceId.cs](../Logic/NodeInstanceId.cs) | `{BlueprintInstanceId blueprintId; Id<NodeHeader> nodeId}` — адрес ноды конкретного инстанса | ✅ |
| [ExecutionScope.cs](../Logic/ExecutionScope.cs) | **Коннектор/владелец per-instance памяти** (4F-1): `_instances: BlueprintInstanceStorage` + `_cache: UnsafeList<InstanceCache>` + `_memory: UnsafeList<InstancePersistence>` (по id) + `_contexts: ContextRegistry<INodeContext>` (4F-2). `Create`/`CreateInstance(ref storage, bp)`/`Has`/`GetInstanceCache(ref)`/`GetInstancePersistent(ref)`/`ResetInstanceCache`/`ResetAllCache`/`DisposeInstance`/`Dispose` + generic `SetContext<T>`/`GetContext<T>`/`HasContext<T>`. Читает блоб из `CompiledBlueprintStorage` (передаётся, не хранит) | ✅ **(4F-1, 4F-2)** |
| [InstancePersistence.cs](../Logic/RuntimeData/InstancePersistence.cs) | Per-instance Persistence (стейт) поверх `UnsafeArray<byte>`: `Create(memoryId, size)`/`GetPtr`/`Dispose`/`IsValid`/`Size`. Фикс-размер, не растёт (динамика — через контекст) | ✅ **(4F-1)** |
| [ContextRegistry.cs](../Logic/RuntimeData/ContextRegistry.cs) | **Ambient-context-реестр scope'а** (4F-2), generic по категории: `ContextRegistry<TContext> where TContext : IIndexedType` (scope: `INodeContext`). `UnsafeArray<SafePtr>` размера `TypeId<TContext>.Count`, **владеет** памятью контекстов (lazy `MemAlloc(TSize<T>)` при `SetContext<T>`). API generic-only: `SetContext<T>(in T)`/`GetContext<T>()→readonly ref T`/`HasContext<T>()`/`Dispose`. Без id-based/count. Резолв нодой в run'е — M7 | ✅ **(4F-2)** |
| [LogicGraph.cs](../Logic/LogicGraph.cs) | `{CompiledBlueprintStorage storage; Id<Blueprint> entryBlueprintId}` | 🟡 поля |
| [CompiledGraph.cs](../Logic/StaticData/CompiledGraph.cs) | Поля-заглушка | 🟡 stub |

### Cache — per-instance кеш In/Out (✅ 4C поведение; ✅ 4F-3: union-`RegionPtr` + шаблон ячеек, Reset = copy)

| Файл | Что | Статус |
|---|---|---|
| [CacheLink.cs](../Logic/CacheData/CacheLink.cs) | Ячейка кеша (бывш. `DataCache`): `[Explicit]` 16 байт, union `{state @0; valueOffset / link @8}` (взаимоисключающи по `state`) | ✅ **(4F-3 rename)** |
| [InstanceCache.cs](../Logic/CacheData/InstanceCache.cs) | Per-instance Cache = `_cells` (рабочие) + `_values` (значения) + **`_template`** (своя копия `cacheCellsTemplate` из блоба, забейканные `valueOffset`). `Create(memoryId, cellCount, valuesSize, SafePtr<CacheLink> template)` (копирует шаблон); **`Reset` = `_cells.CopyFrom(_template)`**; `Write` пишет по `cell.valueOffset` (из шаблона). Позиционно-независим | ✅ **(4F-3)** |
| [CacheHandler.cs](../Logic/CacheData/CacheHandler.cs) | `{PtrOffset<CacheLink> cell}` — **один** офсет ячейки (value-офсет забейкан в самой ячейке) | ✅ **(4F-3)** |
| [RegionPtr.cs](../Blueprint/RegionPtr.cs) | `[Explicit]` **16 байт, union @8**: `staticData` (Static self-rel) / `cacheData: Id<CacheLink>` (Cache — **ordinal ячейки**) / `instanceData: PtrOffset` (Persistence — офсет слайса). Порт одного региона ⇒ один слот | ✅ **(4F-3)** |
| [NodeIn.cs](../Logic/CacheData/NodeIn.cs) / [NodeOut.cs](../Logic/CacheData/NodeOut.cs) | `Read`/`Write`/`IsCalculated(ref InstanceCache)` через `InstanceCache` | ✅ |
| Cache-layout (`BlueprintCompiler`) | На компиляции: `cacheCellCount`/`cacheValuesSize` + **`cacheCellsTemplate: BumpArray<CacheLink>`** (по ordinal, забейкан `valueOffset`=префикс-сумма; slack `DataSizes.Cache` не влияет). Карта несёт `cacheData`=ordinal. `ExecutionScope` создаёт `InstanceCache` копией шаблона. Старый per-node `cacheNodeOffset` снесён | ✅ **(4F-3)** |

### Execution — оркестратор (✅ 4B: батч-DAG + детерминированный обход; параллелизм/диспатч — M6/M7)

| Файл | Что | Статус |
|---|---|---|
| [ExecutionGraph.cs](../Logic/RuntimeData/Execution/ExecutionGraph.cs) | Батч-DAG: `Inject(ref compiled, BlueprintInstanceId)` — chain-декомпозиция Static-топологии (батч = линейная цепочка); `Drain(Span<NodeInstanceId>)` — детерминированный обход (ready-queue, single-thread); `ResetDeps`/`Dispose`. `ExecutionBatch{ inDegree, remainingDeps (синхронный int), nextBatches, nodesOrder }`. `enum RuntimeType{Unmanaged,Managed}` (для 4D/M7). Снесён мёртвый каркас (`IterationTo`/курсор/`AsyncValue`/`iterationsToSchedule`) | ✅ **(4B)** (тела нод не исполняются — M6) |

### Снесено (по ходу рефактора)

`DataLayout`, `Data/NodeTypeId`, `BlueprintInstance` (старый), `ConcreteNode/AddNode`, `NodeInvoker`,
`CompiledBlueprint` (+ edge-модель: `EdgeToData`/`EdgeDataHeader`/`EdgeData`/`InputData`/`OutputData`/
`StateData`/`NodeBody`/`NodeState`/`NodeStateInput`/`outputToIndexMap`). `ILogicNode` → пустой маркер
(диспатч пересобирается на Static.Map в M6).

---

## 3. Что готово vs что заглушка

- ✅ **Static-модель целиком**: компиляция блюпринта в 3-региональный блоб, per-node static-слайсы, Map
  (In/Out → `RegionPtr` с выводом региона), константы, lockstep `reserve == bump`. БД блобов
  (дедуп/версии). Покрыто `LayoutTests`/`MapTests`/`CompiledBlueprintStorageTests`.
- ✅ **Instance identity/lifecycle**: `BlueprintInstanceHeader`/`Storage`/`Id` с generation-staleness.
  Покрыто `BlueprintInstanceStorageTests`/`InstanceScopeTests`.
- 🟢 **Runtime/Cache/Execution** — доделывается по под-фазам, см. [phase-4/runtime/README.md](phase-4/runtime/README.md)
  (8 развилок решены, разбивка 4A–4F, wave-модель). **Готово: 4A** (`NodeMapHeader` топология + `BlueprintCompiler`),
  **4B** (`ExecutionGraph` батч-DAG + детерминированный обход), **4C** (`CacheHeader` ячейки + read/write/link),
  **4D** (`runtimeType`/`NodeState` wiring: `INode.RuntimeType` → `NodeHeader.runtimeType`; `ByteEnumMask<NodeState>`
  флаги `HasCache`/`Multiple` в `SetupNodeFlags`), **4E** (ContextType: ноды → `TypeId<INodeContext>[]`, дедуп-union
  бейкается в `CompiledBlueprintHeader.contextTypes`), **4F-1** (`ExecutionScope` — владелец per-instance памяти:
  `InstanceCache`/`InstancePersistence` поверх `UnsafeArray`; `CreateInstance`/`Dispose`/`Reset`/`Get*`; кеш-раскладка
  переделана), **4F-2** (`ContextRegistry<TContext>` — ambient-context-реестр
  на scope: владеет памятью контекстов, generic-only `SetContext<T>`/`GetContext<T>`→`readonly ref T`/`HasContext<T>`),
  **4F-3** (кеш-раскладка сведена). Покрыто `NodeMapTests`/`ExecutionGraphTests`/`CacheTests`
  (+обновлённые `MapTests`/`LayoutTests`); context — `ContextRegistryTests`/`ExecutionScopeTests` (round-trip под
  `Assert.Ignore`: `IndexedTypes` не init в EditMode). **4F-3 (свод кеша):** `RegionPtr` — **union 16 байт**
  (`staticData`/`cacheData: Id<CacheLink>` ordinal/`instanceData`); value-офсет забейкан в **`cacheCellsTemplate`**
  (`BumpArray<CacheLink>` по ordinal). `InstanceCache` владеет копией шаблона, `Reset` = `_cells.CopyFrom(_template)`.
  `DataCache`→`CacheLink`; `cacheNodeOffset` снесён; `link` не бейкаем. `BumpHeader.Reset/Size`/`RawBumpAllocator.Size` — оставлены.

---

## 4. Открытые развилки

> **РЕШЕНЫ (2026-06-15).** Все 8 развилок закрыты пользователем; решения + разбивка на под-фазы —
> [phase-4/runtime/README.md](phase-4/runtime/README.md). Кратко: 1 Cache=`DataCache`-ячейки (4C);
> 2 синхронный `int`+inDegree (потоки по startBatches); 3,4 топология в блобе, Build=компиляция/Inject=runtime
> (4A done); 5 `IterationTo` снести; 6 `iterationsToSchedule`=буфер следующего wave (4D/M7); 7 `NodeState`
> HasCache/Multiple; 8 `INode.RuntimeType` (4D). Ниже — исходная формулировка (история).

Доделка runtime/cache/execution была заблокирована этими решениями (в скобках — рекомендация, теперь принятая):

1. **CacheData ↔ RegionPtr.** `DataCache<T>` — это per-instance value-слой Cache-региона (мемоизация
   `Is-Calculated` + passthrough-link), а `RegionPtr` — статическая разводка Map. → *inOut Cache-портов
   несёт `CacheHandler`, указывающий в per-instance `CacheHeader`; RegionPtr остаётся static-проводкой.*
2. **`ExecutionBatch.previousBatchesCount`.** `AsyncValue<int>` (lock, без атомарного декремента) →
   *`int` + `Interlocked.Decrement`, плюс исходный `inDegree` для переинициализации каждый run.*
3. **`NodeMapHeader` — аллокация и кратность.** Поле `CompiledBlueprintHeader.nodesMap` **не
   аллоцируется** и ломает lockstep, если включить. → *либо `BumpArray<NodeRelativesHeader>` per-node
   (аллоцировать в `SetupLayout`, добавить в `CalculateLayoutSizeToReserve`), либо убрать поле, если Map
   строится в рантайме off-blob.*
4. **Методы `NodeMapHeader`.** Где `BuildBatches(root)` (instance-agnostic, на Static) и
   `Inject(BlueprintInstanceId, index)` (на runtime `ExecutionGraph`)? Аргументы из комментария:
   `Id<NodeHeader>` (корень цепочки) + `BlueprintInstanceId` + индекс инъекции.
5. **`IterationTo`** — удалить (мёртв) или сделать шаблоном batch-DAG (template из Static, инстанцируется в
   `ExecutionIteration`)?
6. **`iterationsToSchedule`** — роль? (буфер планирования на `parallelCount * runtimesCount`).
7. **Семантика `NodeState`** — `HasCache` = нода производит кешируемый Out (гейт мемоизации)? `Multiple` =
   мульти-вызов / параллельные батчи? Подтвердить.
8. **Wiring `runtimeType`** — нужен `INode.RuntimeType` (default `Unmanaged`), чтобы `SetupLayout`
   заполнял `NodeHeader.runtimeType` (сейчас TODO, всегда default).

---

## 5. Следующие шаги (после решений по §4)

1. ✅ **(4A)** `NodeMapHeader`: топология (relatives+startNodes) в блобе из `inputToOutput`; выделен `BlueprintCompiler`.
2. ✅ **(4B)** `ExecutionGraph`: батч-DAG (батч=линейная цепочка) + детерминированный обход + `Dispose`;
   синхронный `remainingDeps`/`inDegree`; снос `IterationTo`/курсора/`iterationsToSchedule`/`AsyncValue` (форки 2,5,6).
3. ✅ **(4C)** `CacheHeader`: per-instance Cache-блок (ячейки `DataCache`), read/write/`Is-Calculated`-мемоизация,
   резолв link (passthrough); Cache-layout по cell-size; wiring `NodeIn`/`NodeOut` (форк 1).
4. ✅ **(4D)** Wiring `runtimeType`/`NodeState` (форки 7,8): `INode.RuntimeType` (default `Unmanaged`) →
   `NodeHeader.runtimeType`; флаги `NodeState` (`HasCache` по региону Out'ов, `Multiple` по fan-out `relatives.outputs`)
   битовой маской `ByteEnumMask<NodeState>` в `SetupNodeFlags` (после `BuildNodeMap`, вне lockstep).
5. ✅ **(4E)** `ContextType`: маркер `INodeContext : IIndexedType`; ноды объявляют `TypeId<INodeContext>[]`
   (`INode.GetContextTypes`); компилятор бейкает дедуп-union (сорт. по id) в `CompiledBlueprintHeader.contextTypes`
   (`BumpArray`, span-аксессор; lockstep шаг 7). На ноде не хранится; runtime-реестр-владелец — 4F.
6. ✅ **(4F-1)** `ExecutionScope` — владелец per-instance памяти + трекер инстансов: `CreateInstance(ref storage, bp)`
   заводит `InstanceCache` + `InstancePersistence` (оба поверх `UnsafeArray<byte>`/`UnsafeArray<DataCache>`, off-allocator),
   трекает в `BlueprintInstanceStorage`; `Get*`/`Reset*`/`DisposeInstance`/`Dispose`. Кеш — split: `DataCache`
   метаданные (state + valueOffset + link, union) + значения отдельным `UnsafeArray<byte>` (сохранён в 4F-3).
7. ✅ **(4F-2)** Context-реестр на `ExecutionScope`: `ContextRegistry<TContext>` (generic по категории; scope:
   `INodeContext`) — `UnsafeArray<SafePtr>` размера `TypeId<TContext>.Count`, **владеет** памятью контекстов (lazy
   `MemAlloc(TSize<T>)` при set). Generic-only API: `SetContext<T>(in T)`/`GetContext<T>()`→`readonly ref T`/`HasContext<T>()`.
   Без id-based/count (решение пользователя). Round-trip тесты под `Assert.Ignore` (`IndexedTypes` не init в EditMode).
8. ✅ **(4F-3)** Свод кеш-раскладки: `RegionPtr` — **union 16 байт** (`staticData`/`cacheData: Id<CacheLink>` ordinal/
   `instanceData`); value-офсет в карте не лежит — забейкан в **`cacheCellsTemplate: BumpArray<CacheLink>`** (по ordinal).
   `InstanceCache` владеет копией шаблона, `Reset` = `_cells.CopyFrom(_template)`. `DataCache`→`CacheLink` (rename); старый
   `cacheNodeOffset` снесён; `link` не бейкаем (runtime `WriteLink`). `BumpHeader.Reset/Size`/`RawBumpAllocator.Size` оставлены.
9. M6 (диспатч нод по Map: Burst fn-pointer registry by index + managed-путь + version gate).

---

## 6. Долги/риски

- **Тесты не прогнаны** (раннер падает в EDM4U). Прогнать в редакторе (Test Runner ▸ PlayMode ▸ Run All),
  когда окружение восстановится.
- **CLAUDE.md устарел** (§3/§4/§7/§8 — до-рефакторная 5-scope/edge модель). Нужна сверка/перепись —
  отдельной задачей; этот STATE.md имеет приоритет.
- `BlueprintInstanceStorage`: `new BlueprintInstanceStorage(capacity)` обязателен (struct `new T()` zero-init
  → capacity 0 → DivByZero в sparse set). Тесты/scope конструируют с ёмкостью.
- `generation` — `int` (wrap на ~2^31, не практично). Slab-store не шринкается (память на `Dispose`).
- Self-relative (`RelativePtr`/`BumpArray`/`inOut`-офсет) **нельзя** трогать на копии по значению (в т.ч.
  defensive-copy от `in`) — только через ref/арена-указатель.
