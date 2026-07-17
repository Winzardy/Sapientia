# M6-F — Dispatcher integration + reconcile

> **Статус: ✅ done (закоммичено, 2026-06-17) — веха M6 закрыта.** Финальная под-фаза вехи M6 ([../README.md](../README.md)).
> Источник правды — код, затем [../README.md](../README.md) / [../../STATE.md](../../STATE.md).
>
> **⚠️ Пост-рефактор-свод (после реализации, тем же сидением — эскиз API ниже частично устарел):** диспатч-вызов
> вынесен из реестра в `NodeInvoker.InvokeBurst(in UnsafeArray<FunctionPointer<ExecuteFn>>, Id<ExecuteFn>, ref NodeContext)`
> (`[BurstCompile]`, только blittable) / `InvokeManaged(ExecuteFn[], …)`; `NodeFunctionRegistry.Invoke(ordinal, runtimeType, ref ctx)`
> **удалён** (реестр отдаёт `BurstTable`/`ManagedTable`/`UseManaged`, выбор бэкенда — раз/ноду в `NodeInvoker.Invoke`).
> `CompiledBlueprintHeader.GetNodeRuntimeType`/`GetNodeTypeId` **заменены одним** `GetNode → ref readonly NodeHeader`
> (читать `.typeId`/`.runtimeType`). `InstanceCache` шаблон не хранит — `Reset(SafePtr<CacheLink> template)` из статики;
> `ExecutionScope.ResetInstanceCache(id, template)`/`ResetAllCache(template)`. Эскизы «F5/Публичный API» ниже читать с этой поправкой.
> Предыдущее: M6-A (dispatch-id = `TypeId<ILogicNode>` ordinal), M6-B (контракт `NodeContext`/`Execute`),
> M6-C (`NodeFunctionRegistry` — function-table по ordinal), M6-D (выбор бэкенда + managed-исполнение),
> M6-E (version gate). **M6-F связывает всё это в реальный прогон**: `ExecutionGraph.Drain`-порядок →
> резолв памяти инстанса через `ExecutionScope` → dispatch ноды через реестр.

## Цель

До M6-F все куски диспатча существуют, но **никто не гоняет порядок нод через них**:
`ExecutionGraph.Drain` отдаёт детерминированный список `NodeInstanceId`, `NodeFunctionRegistry` умеет
исполнить **одну** ноду по ordinal'у над готовым `NodeContext`, но **seam, который соберёт `NodeContext` из
памяти инстанса (scope) и прогонит весь Drain-порядок**, отсутствует. M6-F вводит этот seam и замыкает
run-путь end-to-end (single-thread; джоб-параллелизм/wave — M7).

Конкретно (PLAN.md / README развилка M6-F): `NodeInvoker.Invoke(ref scope, ref compiled, NodeInstanceId)` —
резолв памяти + dispatch через реестр; прогон `ExecutionGraph.Drain`-порядка через него. Плюс **reconcile**:
синхронизация STATE.md / CLAUDE.md status-map (M6 закрыт).

## Решения по развилкам (на гейт)

M6-F — интеграционная под-фаза, своих fork'ов в README нет; ниже — дизайн-решения seam'а (предлагаемые):

| # | Развилка | Предлагаемое решение | Обоснование |
|---|---|---|---|
| F1 | **Где живёт реестр в run-пути** | Реестр **прокидывается в `ExecutionScope`** (поле `_registry`), как и обещано в доке `NodeFunctionRegistry` («в `ExecutionScope` прокидывается на M6-F»). Scope держит **shared-копию** (таблицы общие по указателю), **не владеет** ей и **не диспозит** — владелец (тот, кто строил `Build`/`Create`) диспозит сам. Прокидывается опц. параметром `Create(..., NodeFunctionRegistry registry = default)` (**в хвост** — call-site'ы существующих тестов без диспатча не ломаются). | README именует `NodeInvoker.Invoke(ref scope, ref compiled, NodeInstanceId)` **без** параметра реестра ⇒ реестр обязан быть в scope. Контракт sharing/owner-disposes — из доки реестра (копия по значению разделяет таблицы). |
| F2 | **Дом seam'а диспатча** | Per-node `NodeInvoker.Invoke(ref ExecutionScope, ref CompiledBlueprintHeader, NodeInstanceId)` + driver `NodeInvoker.Run(ref scope, ref compiled, ref ExecutionGraph, Span<NodeInstanceId>)`. Оба **managed** (single-thread glue); под `[BurstCompile]` остаются только `Execute<T>`/`Compile<T>`. | README кладёт per-node `Invoke` в `NodeInvoker` (исторический «execution dispatch primitive»). `ExecutionScope` несёт managed-поле (`NodeFunctionRegistry._managed` — managed-массив) ⇒ сам по себе **не** Burst-блиттабл; диспетчер-glue managed, а Burst-горячий путь — внутри `registry.Invoke` через `FunctionPointer`. Wave/job — M7. |
| F3 | **Границы прогона** | M6-F гоняет **один compiled-блоб за Run** (мульти-инстанс **одного** блюпринта — ОК: `ExecutionGraph` это уже умеет, общий блоб + разные хендлы инстансов). Группа **разных** блюпринтов за один прогон → **M7** (там же бакетинг по `runtimeType` + wave). | Держит диф «в одно ревью-сидение», не тащит резолв «инстанс → `VersionedId` → `storage.Get`» (мульти-блоб) в M6-F. `Run` принимает `ref compiled` (один блоб); per-инстансная память различается по `NodeInstanceId.blueprintId`. |
| F4 | **Run-prologue (reset)** | `Run` сам делает run-prologue: `scope.ResetAllCache()` + `graph.ResetDeps()` **до** `Drain` (Drain расходует счётчики; кеш транзиентен). | Корректная семантика прогона в одном месте; вызывающему не надо помнить порядок. Для single-bp scope `ResetAllCache` корректен (все инстансы — этого блюпринта). |
| F5 | **Новые аксессоры** | `CompiledBlueprintHeader.GetNodeRuntimeType(nodeId)` (читает `NodeHeader.runtimeType` — нужен для выбора бэкенда); `ExecutionScope.GetInstanceCachePtr`/`GetInstancePersistencePtr` (`SafePtr<...>` для сборки `NodeContext`) + `Registry`. | Минимальные публичные добавки, без смены раскладки блоба. `NodeContext.cache/persistence` — `SafePtr<T>`; берём адрес элемента store'а (`_cache[id].AsSafePtr()`, стабилен off-allocator). |

## Файлы

| Файл | Что |
|---|---|
| `Logic/RuntimeData/Execution/NodeInvoker.cs` | **+** `Invoke(ref ExecutionScope, ref CompiledBlueprintHeader, NodeInstanceId)` (per-node seam: сборка `NodeContext` из scope + dispatch через `scope.Registry`) **+** `Run(ref scope, ref compiled, ref ExecutionGraph, Span<NodeInstanceId>)` (run-prologue + Drain-driver). Managed (не `[BurstCompile]`). |
| `Logic/ExecutionScope.cs` | **+** поле `_registry` (shared, не диспозится) + `Create(..., registry = default)` + `Registry` + `GetInstanceCachePtr`/`GetInstancePersistencePtr`. |
| `Logic/StaticData/CompiledBlueprintHeader.cs` | **+** `GetNodeRuntimeType(Id<NodeHeader>)` (чтение `NodeHeader.runtimeType`). |
| `Tests/NodeDispatchTests.cs` | **new** — интеграционные тесты прогона (real, managed-путь, `forceManaged`): single-node seam, chain, diamond/join, мульти-инстанс, persistence, reset/детерминизм. |

> **Не трогаются:** `NodeFunctionRegistry` (M6-D уже даёт `Invoke`/`UseManaged`), `ExecutionGraph`
> (`Inject`/`Drain`/`ResetDeps` готовы, 4B), `BlueprintCompiler`/раскладка блоба, version gate (M6-E).

## Публичный API (эскиз)

```csharp
// NodeInvoker.cs (managed seam; Execute<T>/Compile<T> остаются Burst)
public static class NodeInvoker
{
    // per-node: собирает NodeContext из памяти инстанса (scope) + диспатчит через scope.Registry
    public static void Invoke(ref ExecutionScope scope, ref CompiledBlueprintHeader compiled, NodeInstanceId node);
    // driver: run-prologue (ResetAllCache + ResetDeps) → Drain → per-node Invoke по порядку; возвращает число нод
    public static int Run(ref ExecutionScope scope, ref CompiledBlueprintHeader compiled, ref ExecutionGraph graph, Span<NodeInstanceId> orderBuffer);
}

// ExecutionScope.cs
public static ExecutionScope Create(Id<MemoryManager> memoryId = default, int instanceCapacity = 8, NodeFunctionRegistry registry = default);
public readonly NodeFunctionRegistry Registry { get; }                  // shared-копия; scope НЕ диспозит
public SafePtr<InstanceCache> GetInstanceCachePtr(BlueprintInstanceId id);
public SafePtr<InstancePersistence> GetInstancePersistencePtr(BlueprintInstanceId id);

// CompiledBlueprintHeader.cs
public RuntimeType GetNodeRuntimeType(Id<NodeHeader> nodeId);           // == nodes.Get(nodeId).runtimeType
```

`Invoke` тело (эскиз): резолв `node.blueprintId` → `cachePtr`/`persistencePtr` из scope; `compiled.AsSafePtr()`
→ `ctx.compiled`; `ordinal = compiled.GetNodeTypeId(node.nodeId)`, `rt = compiled.GetNodeRuntimeType(node.nodeId)`;
`scope.Registry.Invoke(ordinal, rt, ref ctx)`.

## Execution steps

1. **F5-аксессоры** — `CompiledBlueprintHeader.GetNodeRuntimeType` + `ExecutionScope` (поле `_registry`,
   `Create`-параметр, `Registry`, `GetInstanceCachePtr`/`GetInstancePersistencePtr`; `Dispose` **не** трогает реестр).
2. **Seam диспатча** — `NodeInvoker.Invoke` (per-node) + `NodeInvoker.Run` (driver). Компилируется.
3. **Тесты** — `NodeDispatchTests` (real, managed, `forceManaged`); handle'ы Cache читаются из Map блоба.
4. **Self-review (Step 4)** — чек-лист (аллокатор/детерминизм/lockstep/конвенции) + adversarial-сабагент по diff.
5. **Reconcile** — STATE.md §5 п.9 (M6-F done) + §2 таблицы; CLAUDE.md status-map (orchestrator/dispatch строки);
   README M6-F → ✅; root PLAN.md M6 → ✅ (веха закрыта).

## Task index

| # | Задача | Статус |
|---|---|---|
| [01](tasks/01-dispatch-entry.md) | `NodeInvoker.Invoke` (per-node seam) + `Run` (Drain-driver) | ✅ done (закоммичено) |
| [02](tasks/02-scope-wiring.md) | `ExecutionScope` реестр+ptr-аксессоры + (пост-рефактор) `CompiledBlueprintHeader.GetNode` | ✅ done (закоммичено) |
| [03](tasks/03-tests.md) | `NodeDispatchTests` (интеграционный прогон, real/managed) | ✅ done (закоммичено) |
| [04](tasks/04-reconcile-docs.md) | Reconcile STATE.md / CLAUDE.md status-map / README / PLAN.md | ✅ done (закоммичено) |

## Тесты

| Тест | Что проверяет | Среда |
|---|---|---|
| `Invoke_SingleNode_ResolvesMemoryAndDispatches` | per-node seam: `NodeContext` собран из scope (cache/persistence) + dispatch через `scope.Registry`; Out посчитан | real (managed) |
| `Run_Chain_ExecutesInDependencyOrder` | A→B→C: каждое тело читает In-кеш, +k, пишет Out-кеш; после `Run` C-Out = накопленное ⇒ порядок зависимостей + проброс памяти | real |
| `Run_Diamond_JoinSeesBothBranches` | A→B,C→D: D читает оба входа и пишет сумму ⇒ Drain-порядок соблюдён (D после B,C) | real |
| `Run_MultiInstanceSameBlueprint_IndependentMemory` | два инстанса одного блюпринта в одном scope/graph; один `Run` гоняет оба; кеши независимы, оба посчитаны (резолв по `NodeInstanceId.blueprintId`) | real |
| `Run_PersistenceNode_PersistsAcrossRuns` | нода пишет/читает `ctx.PersistenceSlice()` (инкремент); два `Run` ⇒ counter==2 (persistence проброшен и переживает reset кеша) | real |
| `Run_ResetsCacheEachRun_Deterministic` | два `Run` дают побитово равный результат (reset кеша + детерминизм; нет Random/wall-clock) | real |

> Все тесты — **реальные** (managed-путь исполняется в EditMode/.NET по-настоящему; реестр строим через
> `NodeFunctionRegistry.Create(managed[], forceManaged: true)` ⇒ Burst/`IndexedTypes` не нужны). Под
> `Assert.Ignore` — ничего (как M6-D/E). `StubNode(typeId: …)` даёт явный ordinal ⇒ слот реестра под ноду
> контролируется. Cache-handle'ы тел берутся из Map блоба (`GetNodeInOut` → `RegionPtr.cacheData`), без
> хардкода ordinal'ов.

## Non-goals (M7+)

- **Джоб-параллелизм + wave-модель** (чередование Burst↔Managed pass, бакетинг батчей по `runtimeType`) — **M7**.
- **Мульти-блюпринтовый прогон за один `Run`** (группа разных блюпринтов; резолв «инстанс → `VersionedId` →
  `storage.Get`») — **M7** (M6-F: один блоб за Run).
- **Реальный Burst-прогон** через `FunctionPointer` в тестах (нужен init `IndexedTypes` + Burst) — конструктивный
  аргумент (единый исходник `Execute<T>` в обе таблицы), не runtime-assert; покрытие managed-путём.
- **Ambient-context Burst-side proxy-резолв** нодой в run'е — **M7** (`NodeContext` его пока не несёт).
- **Pull-based Is-Calculated мемоизация** (гейт `NodeState.HasCache` в run'е) — **M8**.
- **Авто-бейк port-handle'ов в тело ноды из Map** — **M9** (в тестах handle'ы ставим вручную).

## Риски

- **`ExecutionScope` несёт managed-поле** (`NodeFunctionRegistry._managed`) ⇒ scope **не** blittable под Burst.
  Для M6-F это ок (диспетчер managed; Burst — внутри `registry.Invoke`). Если scope позже понадобится в Burst-job
  (M7) — реестр придётся передавать в job отдельно (Burst-таблица — off-allocator `UnsafeArray`, читается из Burst),
  а managed-glue оставить снаружи. Зафиксировать комментарием.
- **Реестр не диспозится scope'ом** (shared) — двойной dispose был бы багом. Документируем владение; `Dispose`
  scope'а реестр **не** трогает.
- **Ptr на элемент store'а** (`_cache[id].AsSafePtr()`) валиден, пока store не ресайзится. В `Run` инстансы не
  добавляются ⇒ стабильно; собирать `NodeContext` **внутри** прогона, не кешировать между `Run`.
- **`Run` для одного блоба** — если в scope инстансы **разных** блюпринтов, `ResetAllCache`/Drain одного графа их
  не охватит корректно; M6-F такой сценарий не поддерживает (бросовый misuse — M7). Документируем границу.
- **IL2CPP/AOT** (как M6-C/D/E) — `NodeFunctionRegistry.Build` через reflection; в тестах используем `Create`
  (без reflection) ⇒ не блокер. Генерируемый initializer — M10.

## Верификация

Как M6-A…E: компиляция-инспекция + adversarial-сабагент по working-tree diff + (при необходимости) изолированный
`dotnet`-репро. Batchmode-раннер не гоняем (segfault в EDM4U — окружение). **Особенность M6-F:** весь прогон
исполняется managed-путём ⇒ покрыт **реальными** unit-тестами без Burst/Unity, без `Assert.Ignore`.
