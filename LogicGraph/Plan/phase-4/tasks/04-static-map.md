# Таска 04 — Static.Map + снос старой edge-модели

**Статус: ✅ done (код написан; прогон тестов отложен — batchmode-раннер падает в Google EDM4U, по решению пользователя не гоняем)**

## Цель
Static.Map: In/Out нод как региональные указатели (`RegionPtr`), построенные **из связей блюпринта**
(`inputToOutput` + порты нод). Старая edge-модель (`EdgeToData`/`edgesData`/`InputData`/...) **сносится**.

## Решения (гейт)
- **Map из связей графа** (не из stub-деклараций); порт-модель остаётся минимальной (полная — M9).
- **Регион Out — из типа порта:** `IsPreCalculated` → `Static` (RO, дефолт бейкается при компиляции);
  `IsPersistent` (`NodeStateOutput`) → `Persistance`; иначе → `Cache`.
- **In копирует `RegionPtr` источника** (`inputToOutput[in]`); каждый In обязан иметь источник
  (дефолт — тоже Out-константа).
- **Константы** (precalculated без ноды-владельца, из `Blueprint.outputs`) — в **хвост Static-блока**,
  дефолт бейкается.
- **Out'ы размещаются в голове слайса ноды** своего региона, слоты выровнены (`AlignUp(8)`);
  fit-assert против `DataSizes`.
- **Снос старого:** `EdgeToData`, `edgesData`/`nodeBodies`/`nodesState` + legacy `Compile/SetupBlueprint`,
  `NodeHeader`, `InputData/OutputData/StateData`, `EdgeDataHeader`/`EdgeData`, `NodeBody`/`NodeState`/
  `NodeStateInput`, `Blueprint.outputToIndexMap`, `NodeInvoker`, `AddNode` (sketch), generic-овэрлоды
  `ILogicNode` (остался маркер). Диспатч пересобирается на Map в M6.

## Итог (что реализовано)
- `CompiledBlueprint` = **Static**: Data (слайсы + `nodeTypeIds`) + Map (`nodeMaps` + пул `regionPtrs`);
  lockstep-шаги 1..6; `GetNodeTypeId`/`GetNodeInputs`/`GetNodeOutputs`/`ResolveStatic`.
- `INode` сведён к `NodeTypeId`/`DataSizes`/`GetInputs`/`GetOutputs`; у `NodeOutput` — `IsPersistent` +
  `SetValue(SafePtr)` (бейк дефолта).
- `StubNode` принимает порты; новые `MapTests` (6 тестов: регион из типа порта, стек Out'ов с
  выравниванием, In копирует источник, константа в хвосте + бейк 42, lockstep, пустые порты).
- `NodeInvoker.cs`/`AddNode.cs` нейтрализованы tombstone-комментариями (Bash недоступен — удалить
  файлы+meta+папку `ConcreteNode` при закрытии).

## Self-review (adversarial, 2026-06-09)
- **Реальная находка (исправлено):** `Blueprint.outputs` = реестр всех аутпутов ⇒ node-owned
  precalculated-Out попадает и в `GetOutputs()`, и в `outputs` → `CalculateConstantsSize` резервировал
  хвост повторно (lockstep-рассинхрон). Фикс: исключение node-owned + дедуп повторов (симметрично
  Pass 1/2 `SetupMap`); регрессионный тест `Map_NodeOwnedConstantNotDuplicatedInTail`.
- **Ложные тревоги (проверено по первоисточнику):** `PtrOffset.cs:45` `+-` — это `+ (-b)` внутри
  `operator -` (вычитание корректно; pre-existing стилистическая бородавка в core, вне диффа);
  fit-assert по сырому размеру корректен (конец сырых байт Out ≤ declared ≤ выровненный слот — перекрытий
  нет); `INode<out T>` — pre-existing объявление.
- Teardown-полнота: grep по снесённым символам чист (вне Plan/).

## Пост-ревью (рефактор по запросу пользователя, 2026-06-09)
- **`CompiledBlueprint` → `CompiledBlueprintHeader`** (файл переименован git mv; все ссылки в Storage/
  Instance/тестах обновлены).
- **`allocatorOffset` убран:** всё адресуется self-relative через `BumpArray<T>` (включая Static-блок —
  теперь `BumpArray<byte> staticBlock`); `SetupLayout(ref BumpHeader, ...)` получает аллокатор параметром.
- **Параллельные per-node массивы свёрнуты в `NodeHeader`** (отдельный файл `NodeHeader.cs`):
  `typeId` + `offsets` (NodeLayoutOffsets по 3 регионам) + диапазоны In/Out. Один `BumpArray<NodeHeader> nodes`
  вместо `nodeLayoutOffsets`+`nodeTypeIds`+`nodeMaps`. lockstep → 4 шага.
- **Отклонение от заготовки:** в `NodeHeader.cs` взял `NodeLayoutOffsets offsets` вместо одиночного
  `staticData` — `LayoutTests`/размещение Out'ов требуют офсеты Cache/Persistence ноды; `offsets[Static]` =
  бывший `staticData`.
- **Дрейф спеллинга подхвачен:** enum `MemoryRegion.Persistence`, `NodeTypeId` → `TypeId<INode>`,
  `ILogicNode : IIndexedType` — ссылки в `CompiledBlueprintHeader`/`MapTests` приведены.
- lockstep перепроверен: `BumpHeader.MemAlloc` — чистый bump без выравнивания ⇒ резерв = сумма сырых
  размеров (struct + nodes + staticBlock + regions).

## Пост-ревью №2 (рефактор NodeHeader/static, 2026-06-09)
- **`NodeHeader` — отдельные поля** (вместо `NodeLayoutOffsets offsets`): `typeId`, `staticData`,
  `cache`/`persistence` (PtrOffset), диапазоны In/Out. `NodeLayoutOffsets` удалён (мёртвый).
- **Static — прямая `RelativePtr<byte>`**: `staticBlock`-массив снят; каждый static-слайс ноды и каждая
  константа аллоцируются **отдельно**, заголовок/Map хранят прямую self-relative ссылку. Резолв «на месте»
  через ref (`GetStaticNodeSlice`, `ResolveStaticInput/Output`).
- **Map (`RegionPtr` = `region` + `RelativePtr<byte> data`):** Static — self-relative ссылка
  (`data.GetPtr()`, резолв на месте); Cache/Persistence — `data.byteOffset` = офсет в блоке региона
  (резолв «база + офсет» делает владелец Runtime).
- **`GetNodeOffset`** теперь только для Cache/Persistence; Static — через `GetStaticNodeSlice`. lockstep
  тот же (struct + nodes + static-слайсы/константы + пул).
- Тесты `MapTests`/`LayoutTests` переписаны под новый API (Static-офсеты убраны из layout-проверок;
  Static резолвится через `ResolveStatic*`/`GetStaticNodeSlice`).

## Пост-ревью №3 (вынос маппера, 2026-06-09)
- **In/Out вынесены в отдельную структуру**: `NodePorts` (inputs/outputs одной ноды) + `BlueprintMapper`
  (`BumpArray<NodePorts> nodes` + `GetInputs/GetOutputs/ResolveStaticInput/ResolveStaticOutput`).
- `NodeHeader` — теперь только **Data** (typeId, staticData, cache, persistence); map-диапазоны убраны.
- `CompiledBlueprintHeader` держит `BlueprintMapper mapper` (вместо плоского `regions`-пула); Map-аксессоры
  переехали в маппер. lockstep → 5 шагов (+ NodePorts на ноду + In/Out массивы).
- `NodeId` (мёртвый, заменён `Id<NodeHeader>`) удалён. Тесты переведены на `compiled.mapper.*`.
- Верификация — multi-lens adversarial воркфлоу (раннер недоступен; прогон тестов за пользователем).

## Пост-ревью №4 (откат маппера, 2026-06-09)
- **`BlueprintMapper` удалён**; вынос In/Out откатан к flat-pool-модели.
- **`NodePorts` → `NodePortsHeader`**, перенесён в `NodeHeader` как поле `ports` (только диапазоны
  `inputsStart/Count/outputsStart/Count` — без `BumpArray`/`RegionPtr`: тип и число элементов известны коду).
- Пул `BumpArray<RegionPtr> regions` вернулся на `CompiledBlueprintHeader`; In/Out-аксессоры +
  `ResolveStatic*` снова на заголовке (`nodes.Get(n).ports.X` + срез пула). lockstep → 4 шага
  (NodePortsHeader встроен в `NodeHeader` ⇒ учтён в `TSize<NodeHeader>`; отдельного массива нет).
- Тесты — на header-API. Adversarial-проверка отката: CLEAN (компиляция/lockstep/self-relative/blockSizes).

## Пост-ревью №5 (единый PtrOffset inOut от позиции заголовка, 2026-06-09)
- **`regions`-пул и `NodePortsHeader` убраны.** Map заполняется только на компиляции; обращение — через `NodeHeader`.
- **`NodeHeader.inOut` — единый `PtrOffset`** (не разбит на in/out) на блок In/Out ноды (массив байт = `RegionPtr`'ы,
  In затем Out). Без Count — нода в run'е знает число полей сама. Блок «конвертируется в struct ноды» (конкретные
  структуры пока не делаем).
- **Офсет идёт от позиции `CompiledBlueprintHeader`**: компиляция `header.inOut = blockPtr - headerPtr`
  (`headerPtr = this.AsSafePtr()`); резолв `GetNodeInOut` = `new SafePtr(headerPtr.ptr + inOut.byteOffset)`
  (безразмерный — `Value<T>()` проверяет только `ptr != null`). Round-trip и position-independence — ок.
- Static-указатели внутри блока остаются self-relative (резолв на месте через ref). lockstep тот же (RegionPtr×CountPorts).
- Тесты на `GetNodeInOut` + хелпер `Port(block, i)` (ref на месте). Adversarial-проверка: CLEAN
  (единственная находка — pre-existing `PtrOffset.cs:45` `+-` в core, вне диффа, работает корректно).

## Done-criteria
- Map строится из связей; регионы выводятся; дефолты Static бейкаются; lockstep с Map-блоком; старая
  модель не компилируется нигде. ⚠️ Тесты не прогнаны (раннер лежит) — прогнать в редакторе.
