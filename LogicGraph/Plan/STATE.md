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
  на static-слайс) + **Map** (на ноду блок In/Out как массив `RegionPtr`) + **ContextType** (`TypeId[]` —
  ещё не сделано). Компиляция — managed-путь (editor/server), не Burst.
- **Runtime** — per-instance, **вся память off-allocator** (персистентность — через будущий слой **State**,
  не через снапшот мира ⇒ `World`/`CachedPtr`/`MemPtr` для рантайма не нужны). Состав: **Cache** (сброс
  каждый run), **Persistence** (постоянные данные нод), **Map** (топология для шедулинга), **Context**.
- **State** (позже) — `Persistence` + `Static` → восстановление `Runtime`. Сейчас не проектируем.

---

## 2. Карта сущностей (что в коде)

### Static — read-only блоб (✅ собран, протестирован инспекцией)

| Файл | Что | Статус |
|---|---|---|
| [CompiledBlueprintHeader.cs](../Logic/StaticData/CompiledBlueprintHeader.cs) | Static-блоб: `CalculateLayoutSizeToReserve`⟷`SetupLayout`/`SetupMap` (lockstep), `nodes: BumpArray<NodeHeader>`, `blockSizes: DataSizes`, `blueprintKey: VersionedId<Blueprint>`; аксессоры `GetStaticNodeSlice`/`GetNodeInOut`/`GetNodePersistenceOffset`/`GetBlockSize` | ✅ |
| [NodeHeader.cs](../Logic/StaticData/NodeHeader.cs) | На ноду: `typeId`, `runtimeType`, `staticData: RelativePtr`, `persistence: PtrOffset`, `inOut: PtrOffset`; `enum NodeState : [Flags] byte {None, HasCache, Multiple}` | ✅ (см. форк 7, 8) |
| [RegionPtr.cs](../Blueprint/RegionPtr.cs) | Указатель Map одного In/Out: `{ MemoryRegion region; RelativePtr<byte> data }`. Static → self-relative (резолв на месте), Cache/Persistence → `data.byteOffset` = офсет в блоке региона | ✅ |
| [MemoryRegion.cs](../Blueprint/MemoryRegion.cs) | `MemoryRegion{Static,Cache,Persistence}` + `DataSizes` (3-региона fixed-буфер, `Alignment=8`) | ✅ |
| [CompiledBlueprintStorage.cs](../Logic/StaticData/CompiledBlueprintStorage.cs) | «БД» скомпилированных блобов: `Add(arena, offsets)`, дедуп + сосуществование версий по `(id,version)`, jump-by-id, never-remove (Dispose-only) | ✅ (Фаза 3) |
| [NodeMapHeader.cs](../Logic/StaticData/NodeMapHeader.cs) | Граф связей для шедулинга: `relatives: RelativePtr<NodeRelativesHeader>`, `NodeRelativesHeader{inputs/outputs: BumpArray<Id<NodeHeader>>}` | 🟡 **stub**: только поля + комментарий; методы Build/Inject не написаны; поле `nodesMap` в `CompiledBlueprintHeader` **не аллоцируется** (см. форк 3) |

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
| [LogicGraph.cs](../Logic/LogicGraph.cs) | `{CompiledBlueprintStorage storage; Id<Blueprint> entryBlueprintId}` | 🟡 поля |
| [CompiledGraph.cs](../Logic/StaticData/CompiledGraph.cs) | Поля-заглушка | 🟡 stub |

### Cache — per-instance кеш In/Out (🟡 каркас, alloc/read/write/link не реализованы)

| Файл | Что | Статус |
|---|---|---|
| [DataCache.cs](../Logic/CacheData/DataCache.cs) | Ячейка кеша порта: `[Explicit]` union `{state: CacheState @0; value: T / link: PtrOffset<DataCache<T>> @8}`; `CacheState{Uninitialized,Value,Link}`. Тег 1 байт @0, payload @8 (выравнивание под любой `T`, совпадает с `DataSizes.Alignment=8`) | ✅ раскладка |
| [CacheHeader.cs](../Logic/CacheData/CacheHeader.cs) | `dataCache: RelativePtr`; `GetCachePtr/GetCache<T>(CacheHandler<T>)` | 🟡 **shell**: нет Alloc/записи/резолва link/мемоизации |
| [CacheHandler.cs](../Logic/CacheData/CacheHandler.cs) | `{PtrOffset<DataCache<T>> offset}` | 🟡 поле |
| [NodeIn.cs](../Logic/CacheData/NodeIn.cs) / [NodeOut.cs](../Logic/CacheData/NodeOut.cs) | `{CacheHandler<T>}` — типизированные обёртки порта | 🟡 поля, без поведения |

### Execution — оркестратор (🟡 каркас, шедулинг не реализован)

| Файл | Что | Статус |
|---|---|---|
| [ExecutionGraph.cs](../Logic/RuntimeData/Execution/ExecutionGraph.cs) | DAG-оркестратор: `runtimes: UnsafeArray<ExecutionRuntime>` (Unmanaged/Managed), `currentIteration`, `TryRun(runtimeType, parallelCount)` — **сейчас только продвигает курсор**. Типы `ExecutionIteration` (`Run` пуст), `ExecutionBatch` (`previousBatchesCount: AsyncValue<int>`, `nextBatches`, `nodesOrder: NodeInstanceId[]`), `RuntimeType{Unmanaged,Managed}` | 🟡 **stub**: `IterationTo` мёртв, `iterationsToSchedule` не используется (см. форки 2,5,6) |

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
- 🟡 **Runtime/Cache/Execution** — **только каркас (компилируется, поведения нет)**: `DataCache` раскладка
  есть, но `CacheHeader` не аллоцирует/не пишет/не резолвит link; `ExecutionGraph` не шедулит (двигает
  курсор); `NodeMapHeader` без методов и не аллоцируется в блобе; `runtimeType`/`NodeState` не заполняются.
- ⬜ **ExecutionScope** (коннектор Static↔Runtime↔Context) — **спроектировать последним**, когда runtime-слой
  устаканится (директива пользователя). Прошлые наброски `ExecutionScope`/`MemorySource` в plan.md —
  **исторические**, под 5-scope модель; пересобрать под 3-региона/off-allocator.

---

## 4. Открытые развилки (нужны решения пользователя)

Доделка runtime/cache/execution заблокирована этими решениями (в скобках — моя рекомендация):

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

1. `NodeMapHeader`: аллокация в блобе (форк 3) + `BuildBatches` из `inputToOutput` (топосортировка в батчи).
2. `ExecutionGraph`: реальный шедулинг (`inDegree`/декремент, `startBatches`, джоб-порог), `Dispose`,
   чистка `IterationTo`/`iterationsToSchedule` (форки 2,5,6).
3. `CacheHeader`: alloc per-instance Cache-блока, read/write/`Is-Calculated`-мемоизация, резолв link
   (passthrough) — форк 1.
4. Wiring `runtimeType`/`NodeState` (форки 7,8) + `INode.RuntimeType`.
5. `ContextType` на Static → Runtime `Context`.
6. **`ExecutionScope`** (коннектор) — в самом конце.
7. M6 (диспатч нод по Map: Burst fn-pointer registry by index + managed-путь + version gate).

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
