# Фаза 4 — Static/Runtime 3-region model (бывш. `ExecutionScope`)

**Статус фазы: ◐ переосмыслена. Static-модель + instance identity — done; каркас runtime/cache/execution
заложен (компилируется, поведения нет) → доделка в M6/M7. `ExecutionScope` отложен (проектируется
последним). Зафиксировано коммитом 2026-06-14 (см. [Пост-ревью №5](#пост-ревью-5-2026-06-14--снимок-перед-коммитом)).**

> ⚠️ **Снимок как-построено — [../STATE.md](../STATE.md)** (приоритетный источник). Этот документ —
> **живая история фазы**: ранние «Пост-ревью» (№1–№3, `ExecutionScope`/`MemorySource`/5-scope) —
> **исторические**, отражают промежуточные итерации до перехода на 3-региона/off-allocator. Актуальное
> состояние — в №4 (доменная модель) и №5 (снимок).
>
> Источник цели — [../../PLAN.md → Phase 4](../../PLAN.md). Дизайн-навигатор — [../../CLAUDE.md](../../CLAUDE.md)
> (его §4 — до-рефакторная 5-scope модель, устарел; см. STATE.md).

## Цель

**Домен исполнения поверх БД (Фазы 3).** `ExecutionScope` владеет **per-usage-site** областями
`static cache` (off-allocator) и `static persistent` (в worldState/снапшоте), создаваемыми **лениво**, и
**управляет жизненным циклом инстансов** внутри домена. Static-блоб читается из
`CompiledBlueprintStorage` по `(Id<Blueprint>, version)` — scope его не владеет и не компилирует.
**Множество scope сосуществуют**, каждый со своими `static *`.

## Утверждённый дизайн (предложение на гейт)

- **Usage-site = `(Id<Blueprint>, version)` внутри scope** (на эту фазу). Вложенность/композиция блюпринтов
  (`bp1→bp2`) — это M9, поэтому полный per-usage-site ключ (3 буфера для `bp1→bp2`/`bp3→bp2`/standalone)
  откладывается. Сейчас: каждая отдельная `(id,version)`, инстанцированная в scope, лениво получает **один**
  `static cache` + **один** `static persistent`, общий для всех инстансов этой версии в этом scope; между
  scope — независимы.
- **worldState-agnostic (без хранения).** Scope не хранит `WorldState`. Persistent-области (идут в снапшот)
  аллоцируются/освобождаются через `WorldState`, **передаваемый в методы** (паттерн `BlueprintInstance`).
  Off-allocator-области (static cache) — через хранимый `Id<MemoryManager>`. *(«via factory» из PLAN
  трактуется как «источник памяти задаётся снаружи»; полноценная фабрика источника откладывается.)*
- **Ссылка на storage не хранится** — `ref CompiledBlueprintStorage storage` передаётся в `CreateInstance`.
  Scope читает БД, но не владеет ей (развязка ответственности).
- **Scope трекает живые инстансы** (`_instances`), чтобы `Dispose` освободил их все. Блоки самого инстанса
  (`instance cache`/`instance persistent`) по-прежнему заводит `BlueprintInstance` (Фаза 2).
- **Static\*-области — плоские блоки** (без `BumpHeader`): слайсы нод уже адресуются через офсеты раскладки
  `CompiledBlueprint` (Фаза 2). `static cache` — off-allocator `SafePtr` (база неподвижна ⇒ кешировать
  безопасно), `static persistent` — world `CachedPtr`.
- **Scope→state инвариант (2026-06-08):** `static cache` off-allocator (✓), `static persistent` в
  worldState (✓). `instance cache` off-allocator — **отложенный follow-up** (см. Non-goals).

## Публичный API (эскиз)

```csharp
public struct ExecutionScope : IDisposable
{
    public bool IsCreated { get; }
    public int InstanceCount { get; }

    public static ExecutionScope Create(Id<MemoryManager> memoryId = default, int instanceCapacity = 8);

    // Резолвит static из storage по (id,version); лениво заводит site (static cache/persistent);
    // аллоцирует блоки инстанса; трекает инстанс; возвращает scope-локальный id.
    public Id<BlueprintInstance> CreateInstance(WorldState ws, ref CompiledBlueprintStorage storage,
                                                Id<Blueprint> id, Id version);

    public bool TryGetInstance(Id<BlueprintInstance> id, out CachedPtr<BlueprintInstance> instance);
    public void DisposeInstance(WorldState ws, Id<BlueprintInstance> id);   // блоки инстанса; site не трогает

    // Доступ к областям site (реальные аксессоры для M7; используются и тестами).
    public SafePtr GetStaticCache(Id<Blueprint> id, Id version);
    public SafePtr GetStaticPersistent(WorldState ws, Id<Blueprint> id, Id version);
    public bool HasSite(Id<Blueprint> id, Id version);

    public void ResetStaticCache();        // обнуляет static cache всех site (на старте run)

    public void Dispose(WorldState ws);    // освобождает все site (cache off-alloc + persistent world) + инстансы
}
```

### Раскладка данных

```csharp
struct StaticSite {                 // per (id,version)
    Id<Blueprint> blueprintId;
    Id           version;
    SafePtr      staticCache;        // off-allocator плоский блок (стабильный); reset каждый run
    int          staticCacheSize;
    CachedPtr    staticPersistent;   // world плоский блок; переживает run
}
struct InstanceSlot {
    Id<BlueprintInstance>        instanceId;
    CachedPtr<BlueprintInstance> instance;   // сам BlueprintInstance в world
}
struct ExecutionScope {
    Id<MemoryManager>        _memoryId;   // off-allocator источник (static cache)
    UnsafeList<StaticSite>   _sites;      // лениво, per (id,version)
    UnsafeList<InstanceSlot> _instances;  // живые инстансы
    int                      _nextInstanceId;
}
```

## Шаги исполнения

1. Типы/раскладка: `StaticSite`, `InstanceSlot`, поля `ExecutionScope`; `Create`/`IsCreated`/`InstanceCount`.
2. Site-слой: `EnsureSite(ws, ref storage, id, version)` (ленивый), `HasSite`, `GetStaticCache`,
   `GetStaticPersistent`, `ResetStaticCache`. Off-alloc + world аллокации, zero-size чисто.
3. Инстанс-слой: `CreateInstance` (ensure site → `BlueprintInstance.Create` → трек), `TryGetInstance`,
   `DisposeInstance`.
4. `Dispose` (инстансы → site: persistent world + cache off-alloc), идемпотентность.
5. Тесты (рядом с кодом).
6. Self-review (Step 4): тесты зелёные + чек-лист + adversarial-сабагент.

## Задачи

| # | Таска | Что | Статус |
|---|---|---|---|
| 01 | [scope + static sites](tasks/01-scope-static-sites.md) | `ExecutionScope` Create/Dispose; ленивые per-(id,version) `static cache` (off-alloc) + `static persistent` (world); `Get*`/`HasSite`/`ResetStaticCache`; zero-size чисто | ✅ done |
| 02 | [instance lifecycle](tasks/02-instance-lifecycle.md) | `CreateInstance` (ensure site + `BlueprintInstance.Create` + трек), `TryGetInstance`, `DisposeInstance`; `Dispose` освобождает всё | ✅ done |
| 03 | [tests](tasks/03-tests.md) | `ExecutionScopeTests`: site shared/isolated, версии, reset, dispose-instance-vs-site, dispose-all, zero-size, +persistent re-resolve после снапшота | ✅ done |

## Тест-лист (model-proving, stub-ноды)

- **Site shared across instances**: 2 инстанса одной `(id,version)` в одном scope → **один** site
  (один `static cache`/`static persistent`); адреса совпадают.
- **Multi-scope isolation**: `bp2` в scopeA и scopeB → независимые `static *` (разные адреса); `Dispose`
  scopeA не трогает scopeB.
- **Distinct versions → distinct sites**: `(1,1)` и `(1,2)` в одном scope → 2 независимых site.
- **Reset static cache**: записали в `static cache` и `static persistent`; `ResetStaticCache` → cache
  обнулён, persistent цел.
- **Dispose instance ≠ dispose site**: 2 инстанса одного bp; `DisposeInstance` одного → site жив, второй
  инстанс жив, `InstanceCount` уменьшился.
- **Dispose all**: инстансы по нескольким site; `Dispose(ws)` → без падений/двойного free; `IsCreated`=false.
- **Zero-size static scopes**: bp с нулевыми `static cache`/`static persistent` → site ничего не аллоцирует;
  reset/dispose чисты.

## Non-goals (отложено)

- **Полный per-usage-site ключ для вложенности** (`bp1→bp2`/`bp3→bp2`/standalone = 3 буфера) — нужна
  композиция блюпринтов (**M9**). Сейчас site = `(id,version)`.
- **Ambient context registry на scope** — по реордеру передаётся в методы исполнения (**M7**), на scope не
  хранится.
- **`instance cache` off-allocator** — `BlueprintInstance.instanceCache` пока в worldState (Фаза 2
  follow-up). **Предлагается отложить** отдельным мелким шагом (ортогонально ядру Фазы 4). — *на гейт.*
- **Исполнение/dispatch/reset edges/`BeginRun`** — **M6/M7**.
- **Staleness-enforcement** — scope доверяет вызывающему/`storage.Has`; явная проверка `(id,version)` —
  позже.
- **Save/load `static persistent`** — **Фаза 5**.

## Пост-ревью №3 (на гейте) — полная развязка инстанса от памяти/стейта

> Требование пользователя: `BlueprintInstance` и `BlueprintInstanceStorage` **ничего не знают** о
> `WorldState`, Mem-сущностях (`MemPtr`/`SafePtr`/аллокаторе) и **источнике аллокаций**. Выбран
> (на вопросе) вариант: инстанс ссылается на память **абстрактными офсетами `PtrOffset`**; владелец
> (`ExecutionScope`) даёт базу любого источника и резолвит. Плюс FIX'ы: storage → `SparseSet`, `Create` → конструктор.

**`BlueprintInstance`** (pure runtime, без Mem/WorldState):
```csharp
struct BlueprintInstance {
    Id version; Id<Blueprint> blueprintId; Id<BlueprintInstance> instanceId;
    PtrOffset instanceCache;       // офсет слайса в cache-store владельца
    PtrOffset instancePersistent;  // офсет слайса в persistent-store владельца
    static BlueprintInstance Create(in CompiledBlueprint, PtrOffset cache, PtrOffset persistent);
}
```
Ни одного метода/поля, касающегося памяти напрямую. `ResetCache` уезжает в scope (инстанс не имеет базы).

**`BlueprintInstanceStorage`** (off-alloc `UnsafeIndexAllocSparseSet`, без Mem/WorldState):
```csharp
struct BlueprintInstanceStorage {
    UnsafeIndexAllocSparseSet<BlueprintInstance> _instances;
    public BlueprintInstanceStorage(int capacity = 8);     // конструктор вместо Create (FIX)
    Id<BlueprintInstance> Add(BlueprintInstance);            // AllocateId → проставить id → set
    bool Has(id); ref BlueprintInstance Get(id); bool TryGet(id, out);
    void Remove(id);                                         // ReleaseId (id переиспользуется)
    Span<BlueprintInstance> Values { get; }                 // итерация владельцем
    int Count; bool IsCreated; void Dispose();
}
```

**`ExecutionScope`** (владелец памяти, граница `WorldState`) — два растущих store на регион:
- `UnsafeList<byte> _instanceCacheStore` (off-alloc) — append-only слайсы instance cache;
- `MemList<byte> _instancePersistentStore` (world, едет в снапшот) — append-only слайсы instance persistent;
- `CreateInstance`: EnsureSite → зарезервировать слайс в обоих store (offset = текущий rover, `SetCount`) →
  `BlueprintInstance.Create(compiled, cacheOffset, persistentOffset)` → `_instances.Add`;
- `GetInstanceCache(id)` = `cacheStore.base + offset`; `GetInstancePersistent(ws, id)` = `persistentStore.base(ws) + offset`;
- `ResetInstanceCache(ref CompiledBlueprintStorage, id)` — MemClear слайса (размер из compiled);
- `DisposeInstance(id)` — **только** untrack (`_instances.Remove`); слайс не возвращается;
- `Dispose(ws)` — целиком освободить оба store + sites + sparse set.

**Компромиссы (подтвердить на гейте):**
1. **Per-instance free отменён** — память слайсов возвращается только на `Dispose` scope (общий store).
   Долгоживущий scope с churn'ом инстансов растёт неограниченно. **Follow-up:** slot-recycling free-list.
   *(Вы выбрали `PtrOffset`, зная этот компромисс — он был в описании опции.)*
2. **`ResetCache` уезжает в scope** (инстанс без базы).
3. **id инстансов переиспользуются** (`SparseSet`) — прежняя «монотонность, без reuse» теряется;
   generation-staleness (#11/#12) — **follow-up**.
4. Тесты instance-памяти переписываются под offset-модель; snapshot-тест persistent сохраняется
   (persistent-store — world, едет в снапшот).

**Файлы:** `BlueprintInstance.cs` (M), `BlueprintInstanceStorage.cs` (M), `ExecutionScope.cs` (M), 3 теста (M).

## Пост-ревью №4 (реализовано) — финальная instance-модель

- **`BlueprintInstance`** = identity + `PtrOffset instanceCache/instancePersistent`. Ноль Mem/WorldState.
- **`BlueprintInstanceId{index, generation}`** — хендл со staleness-защитой (Remove → generation++).
- **`BlueprintInstanceStorage`** — `UnsafeIndexAllocSparseSet<BlueprintInstance>` + параллельный generation-массив;
  **конструктор** (не `Create`). *Важно:* для struct `new T()` вызывает zero-init (capacity 0 → DivByZero в
  sparse set) — конструировать только `new BlueprintInstanceStorage(capacity)` (scope так и делает; тесты — `(8)`).
- **`ExecutionScope`** — владелец памяти: per-site **slot-recycling slab'ы**. cache — off-alloc `UnsafeList<byte>`;
  persistent — растущий вручную **`MemPtr`-блок** (резолв `GetSafePtr`, snapshot-safe; `MemList`/`MemArray`
  не годятся — `AssertWorldState` по worldId падает после `DeserializeWorld`). Слот выделяется только если у
  версии есть instance-память; offsets = `slot * slotSize`; free → `freeSlots`; рост store = realloc + MemCopy.
- **`VersionedId<T>{id, version}`** (дженерик рядом с `Id<T>`, `Submodules.Sapientia.Data`) — пара (id, version)
  `CompiledBlueprint` объединена в один ключ `VersionedId<Blueprint> key`.
- Тесты: generation-staleness + slot-reuse, **store-growth >InitialSlots** (realloc/переезд базы), instance-cache
  off-alloc + persistent snapshot-resolve.

**Follow-up'ы (зафиксировать на Step 8):** slab-store не шринкается (память слотов возвращается на `Dispose`
scope; free-list переиспользует слоты, но ёмкость не падает); generation — `int` (wrap на ~2^31, не практично).

## Пост-ревью №4 — доменная модель Static/Runtime (3 региона); `ExecutionScope` — последним

**Решение пользователя:** проектируем сущности снизу вверх, `ExecutionScope` (коннектор) — **в самом конце**.
Доменная модель (фокус — Static и Runtime; State — позже):

- **Static** (RO, единый экземпляр на приложение, дедуп по `VersionedId<Blueprint>`): **Data** (тела нод +
  индекс метода), **Map** (In/Out как `RegionPtr` — офсет + регион), **ContextType** (`TypeId[]`).
- **Runtime** (per-instance): **Cache** (сброс каждый run), **Persistance** (постоянные), **Map** (копия
  Static.Map + флаги существования In/Out), **Context** (доступные ноде контексты).
- **State** (позже): `Persistance` + `Static` → восстановление `Runtime`.
- **`StaticCache`/`StaticPersistent` удалены.** Память **Runtime — вся off-allocator** (персистентность через
  State, не через снапшот мира) ⇒ World/`CachedPtr`/`MemPtr`-механизм не нужен, источник один (raw).

**Инкремент 1 (сделан, тесты не прогнаны — см. ниже):**
- `DataLayout` (5) → **`MemoryRegion`** (3: `Static`/`Cache`/`Persistance`); `DataSizes`/`NodeLayoutOffsets`
  → 3 региона; файл `DataLayout.cs` → `MemoryRegion.cs`.
- Новый примитив **`RegionPtr`** (`{ MemoryRegion region; PtrOffset offset; }`) — указатель Map (будет связан
  в инкременте Static.Map).
- `CompiledBlueprint`: раскладка на 3 региона (`GetBlockSize/GetNodeOffset(MemoryRegion)`); реально
  аллоцируется только Static-блок.
- `StubNode(staticSize, cacheSize, persistanceSize)`; `LayoutTests`/инстанс-тесты переписаны на 3 региона.
- **`ExecutionScope` + `ExecutionScopeTests` запаркованы (удалены)** — коннектор проектируем последним.

> **Прогон тестов (2026-06-09):** компиляция чистая (нет `error CS`), но локальный batchmode PlayMode-раннер
> **3 раза подряд падает нативным segfault редактора** в Google EDM4U (`Google.RunOnMainThread` →
> `mono_jit_compile_method_inner`; в логе «Access token is unavailable», adb-демон недоступен) — это
> окружение, не LogicGraph. **Тесты инкремента 1 не прогнаны** (риск): прогнать в редакторе
> (Test Runner ▸ PlayMode ▸ Run All) либо повторить batchmode, когда редактор/лицензия восстановятся.

**Следующие инкременты:** Static.Map (In/Out → `RegionPtr`) → ContextType → `Runtime` (Cache/Persistance/
Map/Context) → `ExecutionScope` (коннектор).

**Инкремент 2 — Static.Map + снос старой edge-модели (сделан, см. [tasks/04-static-map.md](tasks/04-static-map.md)):**
Map строится **из связей** (`inputToOutput` + порты); регион Out — из типа порта (precalculated → Static с
бейком дефолта, persistent → Persistance, иначе Cache); In копирует указатель источника; константы — в
хвосте Static-блока. Снесено: legacy `Compile/SetupBlueprint`, `EdgeToData`/`edgesData`/`NodeHeader`/
`InputData`/`OutputData`/`StateData`/`EdgeDataHeader`/`EdgeData`, `NodeBody`/`NodeState`/`NodeStateInput`,
`outputToIndexMap`, `NodeInvoker`, `AddNode`; `ILogicNode` → маркер (диспатч → M6 на Map). Новые
`MapTests` (6). ⚠️ Прогон тестов отложен (раннер падает в EDM4U; по решению пользователя не гоняем);
`NodeInvoker.cs`/`AddNode.cs` — tombstone, удалить файлы+meta при закрытии.

## Пост-ревью №3 — `ExecutionScope` без WorldState/Mem (на гейт)

**Директива:** `ExecutionScope` не должен ничего знать о Mem-коллекциях и WorldState. **Решения (гейт):**
persistent — через **инъекцию источника памяти** (раз снапшот сохраняется); `MemoryManager` scope тоже
не держит — он внутри источника.

**Новый seam — `MemorySource` (tagged unmanaged struct), `LogicGraph/Memory/MemorySource.cs`:**
```csharp
public enum MemorySourceKind : byte { Raw, World }

public struct MemoryBlock { public MemPtr world; public SafePtr raw; public int size; public bool IsValid => size > 0; }

public struct MemorySource              // Raw | World — инкапсулирует MemoryManager/WorldState
{
    public static MemorySource Raw(Id<MemoryManager> memoryId);
    public static MemorySource World(WorldState worldState);
    public void Bind(WorldState worldState);                 // refresh после снапшота (World)

    public MemoryBlock Alloc(int size, bool clear = true);
    public SafePtr Resolve(in MemoryBlock block);            // Raw → стабильный base; World → GetSafePtr(memPtr)
    public void Grow(ref MemoryBlock block, int newSize, bool clearNew = true); // realloc + copy
    public void Free(ref MemoryBlock block);
}
```
- **Raw-бэкенд** — `Id<MemoryManager>`, base неподвижен (static cache, instance cache slab).
- **World-бэкенд** — `WorldState` + `MemPtr`-хендлы, едет в снапшот (static persistent, instance persistent slab).
  После снапшота владелец зовёт `Bind(restored)` (MemPtr стабилен — данные переживают).

**`ExecutionScope` переписывается:** держит **два инъектируемых источника** (`_cache`, `_persistent`) +
`_sites` (`UnsafeList`, off-alloc, не Mem) + `BlueprintInstanceStorage`. Все 4 store-области site
(`staticCache`/`staticPersistent`/`instanceCacheStore`/`instancePersistentBlock`) становятся `MemoryBlock`
через соответствующий источник. **Из публичных сигнатур исчезает `WorldState`** (и `CachedPtr`/`MemPtr`):
```csharp
static ExecutionScope Create(MemorySource cache, MemorySource persistent, int instanceCapacity = 8);
void RebindPersistent(MemorySource persistent);             // после снапшота
BlueprintInstanceId CreateInstance(ref CompiledBlueprintStorage storage, VersionedId<Blueprint> bp);
SafePtr GetInstanceCache(BlueprintInstanceId id);
SafePtr GetInstancePersistent(BlueprintInstanceId id);      // без WorldState — резолв внутри источника
void ResetInstanceCache(BlueprintInstanceId id);
void DisposeInstance(BlueprintInstanceId id);
SafePtr GetStaticCache(VersionedId<Blueprint> bp);
SafePtr GetStaticPersistent(VersionedId<Blueprint> bp);
void ResetStaticCache();  bool HasSite(VersionedId<Blueprint> bp);  void Dispose();
```

**Тесты:** конструируют `MemorySource.Raw(default)` + `MemorySource.World(worldState)`, зовут
`Create(cache, persistent)`, убирают `worldState` из вызовов scope; снапшот-тест после restore зовёт
`RebindPersistent(MemorySource.World(restored))`.

**Non-goals/заметки:** `MemorySource` кладём в LogicGraph (ссылается на WorldState/MemoryManager, которые
LogicGraph уже использует); вынос в core — позже. `WorldState`, хранимый в World-источнике, обновляется
через `Bind` (он же стареет на снапшоте — это контракт владельца).

## Открытые вопросы (на гейт)

1. **Имя** — `ExecutionScope` (PLAN) vs `NodesScope` (CLAUDE.md working name)? Предлагаю `ExecutionScope`.
2. **`instance cache` off-allocator** в этой фазе или отдельным follow-up? Предлагаю **отложить**.
3. **Usage-site = `(id,version)`** на эту фазу (вложенность → M9)? Предлагаю да.

## Отклонения / находки

- Per-usage-site тест сведён к per-`(id,version)` + multi-scope (вложенность отложена — по реордеру).
- «worldState-agnostic via factory» трактуется как «worldState в методах, off-alloc источник снаружи»;
  полноценная фабрика источника памяти отложена.
- **Пост-ревью №1 (по запросу пользователя):** управление инстансами вынесено в **отдельный
  `BlueprintInstanceStorage`** (`Logic/BlueprintInstanceStorage.cs`) — аналог `CompiledBlueprintStorage`, но
  для мутабельного per-instance стейта. `ExecutionScope` его **композирует**; `static *` site остаются на scope.
- **Пост-ревью №2 (по запросу пользователя):** `BlueprintInstance` и `BlueprintInstanceStorage` сделаны
  **WorldState-free** — память инстанса выделяется **снаружи**:
  - `BlueprintInstance` хранит `MemPtr instancePersistent` (стабильный хендл) + `SafePtr instanceCache`
    (off-allocator); `Create(in compiled, MemPtr, SafePtr)` только связывает поля, `ResetCache(in compiled)`
    чистит off-allocator cache без `WorldState`. Никаких `Create(ws…)`/`Dispose(ws)`/внутренней аллокации.
  - `BlueprintInstanceStorage` — pure bookkeeping (`Add(BlueprintInstance)` присваивает монотонный id;
    `TryGet`/`this[int]`/`Remove(id)`/`Dispose()`), памяти не касается.
  - `ExecutionScope` — **владелец памяти инстансов** и граница `WorldState`: `CreateInstance` выделяет
    `instance cache` off-allocator + `instance persistent` в worldState (`MemPtr`), собирает инстанс,
    кладёт в сторедж; `DisposeInstance`/`Dispose` освобождают; добавлены `GetInstanceCache`/`GetInstancePersistent`.
  - **Побочно закрыт follow-up Фазы 2:** `instance cache` уехал off-allocator (соответствует инварианту
    Scope→state). Тесты `InstanceScopeTests`/`BlueprintInstanceStorageTests` переписаны как WorldState-free;
    добавлены `Scope_InstanceMemory_*` и `Scope_InstancePersistent_ReResolvesAfterWorldSnapshot`.

## Пост-ревью №5 (2026-06-14) — снимок перед коммитом

> Полная карта как-построено — [../STATE.md](../STATE.md). Здесь — что зафиксировано и что осталось.

**Зафиксировано коммитом** (submodule `Sapientia`, ветка `rnd/nodes_graph`, push выполнен):

- ✅ **Static-модель** (`CompiledBlueprintHeader` + `NodeHeader` + `RegionPtr` + `MemoryRegion`/`DataSizes`):
  3-региона раскладка, per-node static-слайсы, Map (In/Out → `RegionPtr`, вывод региона из типа порта),
  константы, lockstep. БД блобов (`CompiledBlueprintStorage`, Фаза 3). Покрыто `LayoutTests`/`MapTests`/
  `CompiledBlueprintStorageTests`.
- ✅ **Instance identity/lifecycle** (`BlueprintInstanceHeader`/`Storage`/`Id` + `NodeInstanceId`):
  `UnsafeIndexAllocSparseSet` + generation-staleness. Покрыто `BlueprintInstanceStorageTests`/`InstanceScopeTests`.
- 🟡 **Каркас runtime/cache/execution** (компилируется, поведения нет): `DataCache` (раскладка union,
  ответ на `// QUESTION` выравнивания — тег@0/payload@8), `CacheHeader`/`CacheHandler`/`NodeIn`/`NodeOut`
  (shell), `ExecutionGraph`/`ExecutionBatch`/`ExecutionIteration`/`RuntimeType` (двигают курсор),
  `NodeMapHeader`/`NodeRelativesHeader` (поля + TODO).
- Снесено: edge-модель, `CompiledBlueprint`, `NodeInvoker`, `AddNode`, `DataLayout`, `NodeTypeId`,
  старый `BlueprintInstance` (+ tombstone-файлы удалены).

**Применённые однозначные правки** (компиляция/корректность, без дизайн-решений): 3 ошибки `header.cache`
в `CompiledBlueprintHeader` (Cache-офсет считается локально, `GetNodeOffset`→`GetNodePersistenceOffset`);
синтаксический разрыв `ExecutionGraph` (`iteration.Run`+`RemoveAt`+курсор); layout-баг `DataCache`
(`CacheState : byte`, payload@8); `NodeState`→`[Flags]`; `LayoutTests`/`MapTests` подстроены.

**Не сделано — заблокировано 8 развилками** (нужны решения пользователя; полный список с рекомендациями —
[../STATE.md §4](../STATE.md)): CacheData↔RegionPtr; `AsyncValue`→`int+Interlocked`; аллокация/кратность
`NodeMapHeader` (+поле `nodesMap` не аллоцируется, ломает lockstep); место методов Build/Inject;
`IterationTo`/`iterationsToSchedule`; семантика `NodeState`; wiring `runtimeType` (нужен `INode.RuntimeType`).

**Следующий чат:** см. [../STATE.md §5](../STATE.md) + handoff-промпт `Plan/phase-4/HANDOFF.md`. Тесты не
прогнаны (раннер падает в EDM4U) — прогнать в редакторе, когда окружение восстановится.
</content>
</invoke>
