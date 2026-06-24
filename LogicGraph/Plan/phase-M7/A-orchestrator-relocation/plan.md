# M7-A — Orchestrator entity + Run relocation

> **Статус: 🔄 in progress** (правки ревью применены, ожидает ревью пользователя). Источник правды — код, затем [STATE.md](../../STATE.md) и
> [разбивка M7](../README.md). Развилка фазы — №1 из [phase-M7/README.md](../README.md#решения-по-развилкам).

> **Правки по ходу ревью (2026-06-18), сверх исходной «релокации»:**
> 1. **`Run`-сигнатура** → `ref compiled` (гейт; `ref storage` per-node → M7-C).
> 2. **Учёт нод** перенесён из `ExecutionGraph.NodeCount` в `Orchestrator._nodeCount` (ревью: размер
>    буфера — ответственность владельца буфера; `ExecutionGraph` не трогаем).
> 3. **Inject → модель входной ноды** (ревью пользователя): `Inject(ref compiled, NodeInstanceId entry)`
>    строит подграф, **достижимый вперёд от entry** (а не от всех голов блюпринта); `inDegree` — только по
>    рёбрам внутри подграфа. `Inject` возвращает |R| (число инжектнутых нод). Затрагивает `ExecutionGraph`,
>    `Orchestrator`, оба теста (`Entry`-хелпер; multi-root → вход на корень). Это «начать исполнение с
>    ноды» (#12–13, ExecRef/input-node). **Кросс-входной дедуп общих нод — не делается** (позже).
> 4. **Burst-совместимость диспатч-пути** (отдельная забота, не M7-A; всплывает при реальной Burst-AOT по
>    одной): **BC1047** — `VersionedId<T>` (`Data/`) generic + `[StructLayout(Explicit)]` запрещён → снят
>    explicit layout (sequential, байт-совместимо); **BC1064** — `NodeInvoker.InvokeBurst`/`InvokeManaged`
>    параметр `ordinal` был `Id<ExecuteFn>` по значению (структура по значению в `[BurstCompile]` entry запрещена) → `in Id<ExecuteFn>` (по readonly-ref; тип сохранён, ABI — указатель).
>    Это M6-долги, не M7-A; **отдельный коммит**. Возможны ещё (фикшу итеративно по мере Burst-прогона).
> 5. **Adversarial-находка (Blocker, исправлено):** в `IsChainHead` был by-value-копий self-relative
>    `BumpArray` (`var inputs = …`) → wild pointer; доступ переведён на `ref` (инвариант «не копировать
>    BumpHeader по значению»).

## Цель

Вынести временный seam `NodeInvoker.Run` (M6-F) в **новую сущность-оркестратор**, владеющую
`ExecutionGraph` и буфером порядка обхода. `NodeInvoker` откатывается до **чистого диспатча**
(`Invoke`/`InvokeBurst`/`InvokeManaged`/`Execute`/`Compile`/`GetManaged` остаются). **Поведение не
меняется**: single-thread, один блюпринт, тот же Drain→Invoke прогон. Это чистая релокация + установка
ownership — задел под wave-passes (M7-B), мульти-блюпринт (M7-C), параллелизм (M7-D). Закрывает долг
памяти `logicgraph-run-temporary`.

## Развилка (№1) и решение

**Где живёт `Run` и его сигнатура.** Новый `struct Orchestrator` (`Logic/RuntimeData/Execution/
Orchestrator.cs`, namespace `Sapientia.LogicGraph`) — **владеет** `ExecutionGraph _graph` + off-allocator
буфером порядка `UnsafeArray<NodeInstanceId> _order`. API:

- `Orchestrator Create(Id<MemoryManager> memoryId = default)` — создаёт owned `ExecutionGraph`; буфер
  ленивый (растёт в `Run` под `graph.NodeCount`).
- `int Inject(ref CompiledBlueprintHeader compiled, BlueprintInstanceId instance)` — форвард в
  `_graph.Inject`; накапливает батчи (мульти-инстанс).
- `int Run(ref ExecutionScope scope, ref CompiledBlueprintHeader compiled)` — тот же run-prologue
  (`ResetAllCache` + `ResetDeps`) → `Drain(_order)` → per-node `NodeInvoker.Invoke`. Возвращает число
  исполненных нод.
- `void Dispose()` — освобождает `_graph` + `_order`.

> **Открытый вопрос гейта (нужно ACK).** Решение разбивки M7 (№1) записало сигнатуру как `Run(ref scope,
> ref storage, …)`. Здесь предлагается **отступление**: M7-A берёт `ref CompiledBlueprintHeader compiled`
> (как сейчас), а резолв `compiled` per-node через `storage` — это работа **M7-C** (форк 6,
> мульти-блюпринт). Причина: M7-A = «поведение не меняется, только релокация»; для одного блюпринта
> `ref compiled` поведенчески идентичен, а `ref storage,key` тянул бы за собой `BlueprintKey`-резолв,
> который по-настоящему нужен только в группе разных блюпринтов. Так публичная сигнатура `Run` в M7-C
> поменяется один раз (внутренности: per-node resolve), а не дважды. **Альтернатива:** взять `Run(ref
> scope, ref CompiledBlueprintStorage storage, BlueprintKey key)` уже сейчас (один `storage.Get(key)` на
> входе) — буквальнее следует решению №1, но добавляет параметр-ключ без функциональной нужды в M7-A.
> Рекомендация — **`ref compiled`** (минимальная релокация). Жду решения.

## Файлы

| Файл | Изменение |
|---|---|
| `Logic/RuntimeData/Execution/Orchestrator.cs` | **новый** — `struct Orchestrator : IDisposable` (см. API выше); сам считает `_nodeCount` в `Inject` (`compiled.NodesCount`). |
| `Logic/RuntimeData/Execution/ExecutionGraph.cs` | **без изменений** (ревью: учёт нод — ответственность владельца буфера, т.е. оркестратора, а не `ExecutionGraph`). |
| `Logic/RuntimeData/Execution/NodeInvoker.cs` | − `Run(...)` (переехал); правка class-doc (убрать «Drain-driver»); `Invoke`/`InvokeBurst`/`InvokeManaged`/`Execute`/`Compile`/`GetManaged` без изменений. |
| `Tests/NodeDispatchTests.cs` | 5 `Run_*` тестов: `ExecutionGraph.Create()`+`NodeInvoker.Run(...,ref graph, order)` → `Orchestrator.Create()`+`orch.Inject`+`orch.Run`; убрать `stackalloc order` (буфер у оркестратора). |
| `Tests/ExecutionGraphTests.cs` | **без изменений** — тестирует `ExecutionGraph` напрямую (топология/Drain), струк остаётся standalone. |

## Раскладка данных

```
struct Orchestrator : IDisposable
    Id<MemoryManager>            _memoryId
    ExecutionGraph               _graph    // owned: Create/Inject/Dispose через оркестратор
    UnsafeArray<NodeInstanceId>  _order     // owned, off-allocator; ленивый, растёт под _nodeCount
    int                          _nodeCount // += compiled.NodesCount в Inject (== числу записей Drain)
    bool IsCreated => _graph.IsCreated
```

`ExecutionGraph` **не меняется**: размер буфера считает сам оркестратор (`_nodeCount` в `Inject`).

## Шаги исполнения

1. **T01** — учёт `_nodeCount` в оркестраторе (`Inject` читает `compiled.NodesCount`); `ExecutionGraph` нетронут.
2. **T02** — `Orchestrator` struct (owns graph+buffer; Create/Inject/Run/Dispose; ленивый рост буфера).
3. **T03** — `NodeInvoker`: удалить `Run`, поправить class-doc; диспатч-точки нетронуты.
4. **T04** — миграция `NodeDispatchTests` на `Orchestrator`; `ExecutionGraphTests` не трогаем.

## Тесты (доказательная база — те же инварианты, новый владелец)

Все 5 `Run_*` из `NodeDispatchTests` сохраняют **те же ассерты** (порядок цепочки = 20; diamond join = 23;
мульти-инстанс изоляция 10/20; Persistence переживает 2 прогона = 2; reset кеша детерминирован). Меняется
только обвязка (Orchestrator вместо ExecutionGraph+NodeInvoker.Run). Зелёные = релокация не сломала
поведение. Дополнительно проверяется неявно: владение буфером (переиспользование между двумя `Run` в
`Persistence`/`Deterministic` тестах) и рост под `NodeCount` (мульти-инстанс — 2 ноды).

## Не входит (позже)

- **`ref storage` per-node resolve** компайла → **M7-C** (форк 6).
- **RuntimeType wave-passes / бакетинг / parked-буфер** → **M7-B**.
- **Параллелизм, scope-in-job, next-wave-буфер, A/B детерминизм-тесты** → **M7-D**.
- Переименование/реворк `ExecutionGraph` внутренностей — не нужно; struct остаётся как есть.

## Индекс задач

| Задача | Статус |
|---|---|
| [01 — ExecutionGraph.NodeCount](tasks/01-execution-graph-nodecount.md) | ✅ done |
| [02 — Orchestrator struct](tasks/02-orchestrator-struct.md) | ✅ done |
| [03 — NodeInvoker → чистый диспатч](tasks/03-nodeinvoker-pure-dispatch.md) | ✅ done |
| [04 — Миграция NodeDispatchTests](tasks/04-migrate-dispatch-tests.md) | ✅ done |

> Гейт: сигнатура `Run` — выбрано **`ref compiled`** (минимальная релокация; `ref storage` per-node → M7-C).
