# M6-D — Managed (.NET) backend + selection

> **Статус: ✅ done (одобрено 2026-06-17).** Под-фаза вехи M6 ([../README.md](../README.md), развилки 4, 5).
> Источник правды — код, затем [../README.md](../README.md) / [../../STATE.md](../../STATE.md).
> Предыдущее: M6-A (dispatch-index), M6-B (контракт `NodeContext`/`Execute`), M6-C (`NodeFunctionRegistry` —
> function-table по ordinal). M6-D даёт **выбор бэкенда** (по `runtimeType` + глобальный managed-форс) и
> **реальное исполнение managed-путём** (без Burst). Прогон через `Drain` в scope — M6-F; version gate — M6-E.

## Цель

M6-C построил **две таблицы** (Burst `UnsafeArray<FunctionPointer>` под Unity + managed `ExecuteFn[]`), но
никто из них **не выбирает** и не исполняет в run'е. M6-D вводит **selection seam**: по `NodeHeader.runtimeType`
(per-node, забейкан на компиляции) + флагу глобального managed-форса диспетчер решает, по какой таблице
исполнять ноду, и **исполняет её** (одна нода за вызов — прогон порядка `Drain` через scope остаётся на M6-F).

Два следствия развилки 5, которые закрывает M6-D:
1. **`Managed`-нода = строго NoBurst.** Тело managed-ноды может лезть в managed-код ⇒ `CompileFunctionPointer`
   на нём **упал бы**. Реестр на сборке (`Build`) **пропускает** Burst-компиляцию для таких типов и держит
   только managed-делегат. (M6-C компилил Burst для **всех** — это был осознанный интерим, managed-тел не было.)
2. **Выбор в run'е.** `Unmanaged` под Unity → Burst-fn-ptr; `Managed` (или форс, или чистый .NET) → managed.
   В чистом .NET Burst недоступен (`#if` вырезает Burst-таблицу) ⇒ **всегда** managed.

«Managed-ность» — это свойство **logic-типа** (может ли тело быть Burst-компилировано), а не отдельной ноды.
Поэтому она поднимается на `ILogicNode` (capability), а `NodeHeader.runtimeType` (per-node в блобе) обязан с ней
совпадать — это обеспечивается тем, что `INode<TLogicNode>.RuntimeType` **выводится из logic-типа**.

## Решения по развилкам (на гейт)

| # | Развилка | Решение |
|---|---|---|
| 5 | **Выбор бэкенда** | Per-node по `NodeHeader.runtimeType` в **selection seam** на реестре: `UseManaged(RuntimeType)` (предикат) + `Invoke(ordinal, runtimeType, ref ctx)` (исполнение по выбранной таблице). Под Unity: `_forceManaged ‖ rt==Managed` → managed, иначе Burst. В чистом .NET (`#else`): всегда managed. `Build` **пропускает** Burst-компиляцию для logic-типов с `RuntimeType.Managed`, но **всегда** строит для них managed-делегат (единственный путь). |
| 4 | **Хранилище реестра (хвост)** | Managed-таблица аллоцируется при `count>0` **всегда** (а не только при `buildManaged`), чтобы у `Managed`-нод был managed-слот даже когда `buildManaged:false` под Unity. Семантика `buildManaged` уточняется: «заполнять managed-делегат и для `Unmanaged`-нод» (для `Managed` — заполняется безусловно). Глобальный managed-форс — поле `_forceManaged` в инстансе (ставится в `Build`/`Create`); шарится копией вместе с таблицей. |

## «Managed-ность» как свойство logic-типа (ключевое решение — на гейт)

Чтобы `Build` мог **пропустить** Burst-компиляцию для managed-тела, реестру нужно знать `RuntimeType`
**по logic-типу** (`TypeId<ILogicNode>` ordinal), а не по ноде блоба. Поэтому:

- **`ILogicNode` получает `RuntimeType RuntimeType { get; }`** (default interface member, дефолт `Unmanaged`).
  Существующие реализации (stub-ноды) компилируются без правок (берут дефолт). Managed-нода объявляет
  `public RuntimeType RuntimeType => RuntimeType.Managed;`.
- **`INode<TLogicNode>.RuntimeType` выводится из logic-типа** (`((ILogicNode)default(TLogicNode)).RuntimeType`),
  так что забейканный `NodeHeader.runtimeType` (из `INode.RuntimeType`) **гарантированно совпадает** с тем,
  по чему реестр решал Burst-skip. Это **новое** (M6-D) — без него codegen'утая `INode<T>`-нода молча
  дефолтилась бы в `Unmanaged`, и диспатч ушёл бы в **дефолтный (пустой) `FunctionPointer` → краш**.
- Чтение `RuntimeType` — **только managed-путь** (editor/compile/registry-build), никогда не на Burst-горячем
  пути ⇒ boxing default-инстанса в `ILogicNode` (раз/тип на сборке, раз/нода на компиляции) допустим.

> **Это деривация на гейт.** Поднятие `RuntimeType` на `ILogicNode` + деривация в `INode<T>` — расширение
> относительно дословной разбивки (там M6-D = «параллельная managed-таблица + выбор»). Обоснование: без
> per-logic-type managed-ности **невозможен** Burst-skip развилки 5 (реестр индексирован по logic-типу), а без
> деривации в `INode<T>` блоб и реестр **рассогласуются**. Если не одобрено — fallback: оставить `RuntimeType`
> только на `INode`, Build читать managed-ность **нечем** ⇒ Burst-skip переносится (риск краша на первой
> managed-ноде остаётся открытым). Рекомендация — одобрить деривацию.

## Файлы

| Файл | Что |
|---|---|
| `Logic/ILogicNode.cs` | **+** `RuntimeType RuntimeType { get; }` (DIM, дефолт `Unmanaged`) — capability logic-типа. |
| `Blueprint/INode.cs` | `INode<TLogicNode>.RuntimeType` → вывод из logic-типа (`((ILogicNode)default(TLogicNode)).RuntimeType`). |
| `Logic/RuntimeData/Execution/NodeFunctionRegistry.cs` | `Build`: skip Burst для `Managed`-типов, managed-делегат для них безусловно, managed-таблица аллоцируется при `count>0`; `_forceManaged` (поле, параметр `Build`/`Create`); **selection seam** `UseManaged(RuntimeType)` + `Invoke(int ordinal, RuntimeType, ref NodeContext)`. |
| `Tests/BackendSelectionTests.cs` | **new** — selection (`UseManaged`), реальное managed-исполнение через `Invoke`, форс, детерминизм, деривация `RuntimeType`, Build-skip (под `Assert.Ignore`). |

## Публичный API (эскиз)

```csharp
// ILogicNode.cs
public interface ILogicNode : IIndexedType
{
    void Execute(ref NodeContext ctx);
    RuntimeType RuntimeType => RuntimeType.Unmanaged;   // capability: может ли тело быть Burst-компилировано
}

// INode.cs — деривация в generic-обёртке (как уже сделано для NodeTypeId)
public interface INode<out TLogicNode> : INode where TLogicNode : unmanaged, ILogicNode
{
    TypeId<ILogicNode> INode.NodeTypeId => TypeIdOf<ILogicNode, TLogicNode>.typeId;
    RuntimeType INode.RuntimeType => ((ILogicNode)default(TLogicNode)).RuntimeType;
}

// NodeFunctionRegistry.cs
public static NodeFunctionRegistry Build(bool buildManaged = true, bool forceManaged = false);
public static NodeFunctionRegistry Create(ExecuteFn[] managed
#if UNITY_5_3_OR_NEWER
    , FunctionPointer<ExecuteFn>[] burst = null
#endif
    , bool forceManaged = false);

// Решение бэкенда (per-node). В .NET всегда true (Burst недоступен).
public readonly bool UseManaged(RuntimeType runtimeType);

// Исполнение одной ноды по выбранной таблице (managed реально / Burst под Unity). Прогон Drain — M6-F.
public readonly void Invoke(int ordinal, RuntimeType runtimeType, ref NodeContext ctx);
```

## Логика `Build` (уточнение M6-C)

```
children = IndexedTypes.GetContextChildren(typeof(ILogicNode))      // TypeId[], index == ordinal
count = children.Length
_forceManaged = forceManaged
_managed = count>0 ? new ExecuteFn[count] : Array.Empty             // всегда при count>0 (слоты для Managed-нод)
#if UNITY  _burst = count>0 ? new UnsafeArray<…>(count) : default  #endif
for ordinal in 0..count:
    t  = IndexedTypes.GetType(children[ordinal])
    rt = ((ILogicNode)Activator.CreateInstance(t)).RuntimeType      // managed-путь сборки; boxing раз/тип
#if UNITY
    if (rt == Unmanaged)   _burst[ordinal] = Compile<t>()           // Managed → НЕ компилим (упал бы)
#endif
    if (buildManaged || rt == Managed)  _managed[ordinal] = GetManaged<t>()  // Managed → всегда
```

## Selection seam

```
UseManaged(rt):
#if UNITY   return _forceManaged || rt == Managed;
#else       return true;          // чистый .NET: Burst вырезан #if
#endif

Invoke(ordinal, rt, ref ctx):
#if UNITY   if (!UseManaged(rt)) { _burst[ordinal].Invoke(ref ctx); return; }   #endif
            _managed[ordinal].Invoke(ref ctx);
```

## Тесты

| Тест | Что проверяет | Среда |
|---|---|---|
| `Select_ManagedRuntimeType_UsesManaged` | `UseManaged(Managed) == true` | managed |
| `Select_UnmanagedRuntimeType_PerEnv` | под Unity `UseManaged(Unmanaged)==false` (Burst), в .NET `==true` (`#if`-разветвлённый assert) | оба |
| `Select_ForceManaged_AlwaysManaged` | `Create(…, forceManaged:true)` ⇒ `UseManaged(Unmanaged)==true` и `UseManaged(Managed)==true` | оба |
| `Invoke_ManagedNode_ExecutesManaged` | `Invoke(0, Managed, ctx)` реально исполняет тело (Add: 5+100=105) | managed (реально) |
| `Invoke_ForceManaged_RunsUnmanagedNodeManaged` | форс ⇒ `Invoke(0, Unmanaged, ctx)` идёт managed-путём и исполняет тело | managed (реально) |
| `Invoke_DeterministicAcrossRuns` | нода с float-математикой: повтор `Invoke` + два инстанса дают **побитово равный** результат | managed (реально) |
| `RuntimeType_LogicTypeCapability` | `((ILogicNode)default(StubManaged)).RuntimeType==Managed`; дефолт-stub → `Unmanaged` | managed |
| `RuntimeType_DerivedByGenericNode` | `((INode)new GenNode<StubManaged>()).RuntimeType==Managed` (деривация `INode<T>`) | managed |
| `Build_SkipsBurstForManaged` | `Build` не компилит Burst для `Managed`-типа, но даёт managed-делегат | **`Assert.Ignore`** (нужен init `IndexedTypes`) |

> Burst-`FunctionPointer` и `IndexedTypes`-init в EditMode недоступны ⇒ фактический **Burst-прогон** и
> reflection-`Build` — под `Assert.Ignore`/инспекцией (как M6-A/C, 4E/4F). **Managed-путь исполняется
> по-настоящему** (plain .NET-семантика даже в EditMode) ⇒ детерминизм и selection покрыты реально.
> «Детерминизм Burst↔.NET» в части Burst — **конструктивный** аргумент (единый исходник `Execute<T>`
> компилируется в обе таблицы), а не runtime-assert (Burst в EditMode не гоняется).

## Задачи

| # | Задача | Статус |
|---|---|---|
| [01](tasks/01-runtime-type-capability.md) | `ILogicNode.RuntimeType` (DIM) + деривация `INode<T>.RuntimeType` из logic-типа | ✅ done |
| [02](tasks/02-registry-selection.md) | `NodeFunctionRegistry`: Burst-skip для `Managed`, `_forceManaged`, `UseManaged`/`Invoke` | ✅ done |
| [03](tasks/03-tests.md) | `BackendSelectionTests`: selection + managed-исполнение + форс + детерминизм + деривация | ✅ done |

## Non-goals (последующие под-фазы / вехи)

- **Прогон `ExecutionGraph.Drain` через диспетчер** + интеграция в `ExecutionScope`
  (`NodeInvoker.Invoke(ref scope, ref compiled, NodeInstanceId)`) — **M6-F**.
- **Version gate** (хеш контракта нод-функций, reject несовместимого блоба) — **M6-E**.
- **Чередование Burst↔Managed pass** (wave-модель, бакетинг батчей по `runtimeType`, параллелизм) — **M7**.
- **Кодоген initializer'а** (генерируемая сборка таблицы вместо reflection) — **M10**.
- Ambient-context Burst-резолв в run'е — **M7**.

## Риски

- **`INode<T>`-деривация через boxing.** `((ILogicNode)default(TLogicNode)).RuntimeType` боксит — но только на
  **компиляции** (раз/нода), не на горячем пути. Приемлемо; кодоген M10 уберёт reflection/boxing.
- **`Build` Burst-skip требует init `IndexedTypes`** ⇒ в EditMode тест-skip под `Assert.Ignore`. Selection и
  managed-исполнение покрыты реально (через `Create`), так что половин-состояния в покрытии нет.
- **Согласованность `NodeHeader.runtimeType` ↔ logic-тип.** Для `INode<T>` гарантируется деривацией; для
  ручной не-generic `INode` (тест/спец-нода) автор держит оба в синхроне — зафиксировано комментарием-инвариантом.
- **`forceManaged` требует населённой managed-таблицы.** При `forceManaged` `Build` де-факто должен заполнить
  managed для всех (иначе `Invoke` на `Unmanaged`-ноде упрётся в `null`-делегат). Документируем; в .NET managed
  населяется всегда.

## Верификация

Компиляция-инспекция + adversarial-сабагент по working-tree diff + (при необходимости) изолированный
`dotnet build` минимального репро (кросс-среда). Batchmode-раннер не гоняем (segfault в EDM4U — окружение).
**Особенность M6-D:** managed-путь исполняется в plain managed-семантике ⇒ детерминизм/selection — реальные
unit-тесты даже без Burst/Unity.
