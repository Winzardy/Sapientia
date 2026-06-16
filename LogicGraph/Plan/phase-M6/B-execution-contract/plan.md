# M6-B — Node execution contract

> **Статус: ✅ done (одобрено 2026-06-16).** Под-фаза вехи M6 ([../README.md](../README.md), развилки 2, 6).
> Источник правды — код, затем [../README.md](../README.md) / [../../STATE.md](../../STATE.md).

## Цель

Дать ноде **контракт исполнения**: что́ функция ноды получает на вход и как добирается до своей памяти.
Это seam, который в M6-C компилируется в `FunctionPointer`, а в M6-F прогоняется по `Drain`-порядку.
**Дисптача/реестра/параллелизма ещё нет** — только сигнатура + резолв памяти, проверенный managed-вызовом.

## Модель (по итогам разбора резолва)

- **Static-слайс ноды = её logic-body `T`** (`ILogicNode`-struct: статическая конфигурация + — когда появится
  wiring — забейканные port-handle'ы). `NodeHeader.staticData` → `GetStaticNodeSlice(nodeId)`; нода с телом
  объявляет `DataSizes.Static >= TSize<T>`.
- **Порты** ноды — блок `RegionPtr` (`GetNodeInOut(nodeId)`): In указывает на данные источника, Out — на свою
  ячейку. Для Cache-портов `RegionPtr.cacheData` = ordinal ячейки `CacheLink` ⇒ `CacheHandler<T>` ⇒
  `InstanceCache.Read/Write`.
- **Cache I/O** — через `ref InstanceCache` (есть `Read`/`Write`/`IsCalculated`/`ResolveLink`).
- **Persistence** — слайс `GetNodePersistenceOffset(nodeId)` поверх базы `InstancePersistence.GetPtr()`.

## Решения (одобрено + уточнения 2026-06-16)

- **Специализировано под ноды.** Node-agnostic абстракция (`IExecutable<TContext>`/`IExecutionContext`/
  generic `Executor`) **убрана** — контракт узловой: `NodeContext` + `ILogicNode.Execute`.
- **FunctionPointer + адаптация под методы (как изначально, `NodeInvoker`).** Дисптач — на
  `FunctionPointer<ExecuteFn>`: generic-обёртка `[BurstCompile] NodeInvoker.Execute<T>` — единая точка адаптации
  типа ноды в указатель на функцию (`Compile<T>()` → `CompileFunctionPointer<ExecuteFn>(Execute<T>)`), как
  исторический `CompileDoNode<T>/DoBurst<T>`. Тело ноды — данные в static-слайсе (`NodeContext.Body<T>()`);
  логика — `ILogicNode.Execute(ref NodeContext)`.
- **Без виртуальных методов в рантайме.** Диспатч — **по индексу через function pointer** (`TypeId<ILogicNode>`),
  без vtable; `body.Execute` внутри монолитной `Execute<T>` на конкретном `T` — constrained call, девиртуализуется
  Burst'ом. Static-abstract interface members в Unity 6 (runtime/Burst) недоступны ⇒ это единственная форма
  FunctionPointer + reflection-регистрации без кодогена.
- `NodeContext` несёт **In/Out + persistence** (+ static-слайс/Body). **Ambient-context Burst-резолв — M7**.
- **Кросс-среда.** `NodeContext`/`ExecuteFn`/`ILogicNode`/`Execute<T>` компилируются и в Unity, и в чистом .NET.
  `Unity.Burst` (`[BurstCompile]`/`FunctionPointer`/`CompileFunctionPointer`/`Compile<T>`) — **строго под**
  `#if UNITY_5_3_OR_NEWER`; в .NET путь через managed-делегат `GetManaged<T>()` (= `Execute<T>`). Гранулярность
  Burst/NoBurst — по ноде (`RuntimeType`); сборка таблицы по индексу (реестр) — M6-C/D.

## Публичный API (эскиз) — узловой, FunctionPointer + `NodeInvoker`

```csharp
// Burst-совместимый seam: только указатели + nodeId, без managed-ссылок.
public struct NodeContext
{
    public SafePtr<CompiledBlueprintHeader> compiled;   // блоб (static-слайс + Map + топология)
    public SafePtr<InstanceCache>           cache;      // Cache-регион инстанса
    public SafePtr<InstancePersistence>     persistence;// Persistence-регион инстанса
    public Id<NodeHeader>                    nodeId;

    public ref CompiledBlueprintHeader Compiled();      // ref в блоб (self-relative — через ref!)
    public ref InstanceCache           Cache();
    public SafePtr StaticSlice();                       // Compiled().GetStaticNodeSlice(nodeId)
    public ref T   Body<T>() where T : unmanaged;       // тело ноды = StaticSlice().Cast<T>().Value()
    public SafePtr InOut();                             // Compiled().GetNodeInOut(nodeId) — блок RegionPtr
    public SafePtr PersistenceSlice();                  // persistence.GetPtr() + GetNodePersistenceOffset(nodeId)
}

// Ячейка function-table; под Burst — FunctionPointer<ExecuteFn> по индексу (M6-C).
public delegate void ExecuteFn(ref NodeContext ctx);

// Тело ноды: данные (в static-слайсе) + логика Execute. Диспатч — не виртуальный (по fn-pointer-индексу).
public interface ILogicNode : IIndexedType
{
    void Execute(ref NodeContext ctx);
}

// Адаптация типа ноды в указатель на функцию (как исторический NodeInvoker.CompileDoNode/DoBurst).
[BurstCompile] // под #if UNITY_5_3_OR_NEWER
public static class NodeInvoker
{
    [BurstCompile] // под #if; монолитна под конкретный T → constrained-вызов тела, без vtable
    public static void Execute<T>(ref NodeContext ctx) where T : unmanaged, ILogicNode
    {
        ref var body = ref ctx.Body<T>();
        body.Execute(ref ctx);
    }

#if UNITY_5_3_OR_NEWER
    public static FunctionPointer<ExecuteFn> Compile<T>() where T : unmanaged, ILogicNode
        => BurstCompiler.CompileFunctionPointer<ExecuteFn>(Execute<T>);
#endif

    public static ExecuteFn GetManaged<T>() where T : unmanaged, ILogicNode => Execute<T>; // .NET-путь / fallback
}
```

## Файлы

| Файл | Что |
|---|---|
| `Logic/RuntimeData/Execution/NodeContext.cs` | **new** — узловой seam `NodeContext` + аксессоры (`Body<T>`/`Cache`/`StaticSlice`/`InOut`/`PersistenceSlice`) |
| `Logic/RuntimeData/Execution/NodeInvoker.cs` | **new** — `delegate ExecuteFn` + `NodeInvoker` (`Execute<T>` + Burst `Compile<T>` под `#if` + `GetManaged<T>`) |
| `Logic/ILogicNode.cs` | `ILogicNode : IIndexedType { void Execute(ref NodeContext ctx); }` |
| `Tests/DispatchIndexTests.cs` | `StubLogicA/B` реализуют пустой `Execute(ref NodeContext)` |
| `Tests/NodeExecutionTests.cs` | **new** — managed round-trip seam'а: `NodeInvoker.Execute<StubAdd>(ref ctx)` |

## Тесты

| Тест | Что проверяет |
|---|---|
| `Exec_NodeBodyExecutesThroughSeam` | вручную собранный узел (static-слайс = stub-body с `CacheHandler`, `InstanceCache` из шаблона) → `NodeInvoker.Execute<StubAdd>(ref ctx)` (managed-путь адаптера; под Burst — тот же код в FunctionPointer) читает In из Cache, пишет Out → `cache.Read(out)` == ожидаемое. **Managed, без Burst/IndexedTypes** — реальный. |
| `Exec_StaticSliceResolves` | `ctx.StaticSlice()` отдаёт ref на тело в блобе (через ref, не копию); правка тела видна вызывающему |
| `Exec_PersistenceSliceResolves` | `ctx.PersistenceSlice()` = база + офсет ноды; запись/чтение переживает (в пределах теста) |

> Burst-компиляция `Execute<T>` и реестр — **M6-C** (тесты под `Assert.Ignore`, как 4E/4F-2). Здесь —
> только managed-путь seam'а (исполняется в plain .NET, даёт реальное покрытие).

## Задачи

| # | Задача | Статус |
|---|---|---|
| [01](tasks/01-node-context.md) | `IExecutable`/`IExecutionContext` + `NodeContext` (seam + аксессоры) | ✅ done |
| [02](tasks/02-execute-contract.md) | `Executor.Execute<T,TContext>` + `ExecuteFn` + `ILogicNode : IExecutable<NodeContext>` + апдейт stub-logic | ✅ done |
| [03](tasks/03-tests.md) | managed round-trip тесты seam'а | ✅ done |

## Non-goals (последующие под-фазы / вехи)

- **Авто-бейк port-handle'ов в тело** из `INode.GetInputs/GetOutputs` + Map — **отложено** (зависит от
  нерешённой authoring-поверхности портов, M9). В M6-B тело/handle'ы собираются вручную в тестах.
- Реестр + Burst-компиляция `Execute<T>` (M6-C); managed-vs-Burst выбор (M6-D); version gate (M6-E);
  прогон `Drain` через диспетчер (M6-F); ambient-context Burst-резолв (M7); Is-Calculated гейт в run'е (M8).

## Решено (форма дисптача)

- **Узловой, FunctionPointer + `NodeInvoker` (как изначально)** (директивы 2026-06-16). Node-agnostic вариант
  (`IExecutable`/`Executor`) откатан. Дисптач восстановлен на `FunctionPointer<ExecuteFn>` + generic-адаптер
  `NodeInvoker.Execute<T>`/`Compile<T>`. «Без виртуальных» = диспатч по fn-pointer-индексу (без vtable);
  `body.Execute` в монолитной `Execute<T>` — constrained, девиртуализуется Burst'ом. Static-abstract в Unity 6
  недоступен.

## Верификация

Компиляция-инспекция + adversarial-сабагент + (при необходимости) изолированный `dotnet build`. Batchmode не гоняем.
