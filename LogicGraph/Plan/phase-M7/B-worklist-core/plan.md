# M7-B — Work-list core (eager, single-thread, один рантайм)

> **Статус: 🔄 реализовано, self-review пройден, ожидает ревью.** Источник модели — [phase-M7/README.md «Целевая модель исполнения»](../README.md).
> Эта под-фаза — **ядро**: заменяет статический `ExecutionGraph`/`Drain`/батч-DAG на **демандный work-list**
> оркестратора. Single-thread, **один рантайм** (forceManaged), **без lazy/wave/параллели** (это M7-C/D/E).

## Цель

`Orchestrator` исполняет набор входных нод инстанса демандно: тянет (eager) их незакешированные входы, гонит
вперёд по консьюмерам, **пропуская уже посчитанное** (per-port мемоизация). Тела нод **атомарны** (eager —
все входы готовы до запуска; `Get`/yield нет, это M7-D). Снос `ExecutionGraph` и тупиковой entry-node работы.

## Ключевые решения (развилки фазы)

1. **Сущность планирования.** `ExecutionGraph` (батч-DAG/`Inject`/`Drain`) **удаляется**. `Orchestrator`
   владеет **work-list'ом** (`UnsafeList<NodeInstanceId>` или ring) + множеством «в очереди/посчитано».
2. **`Inject` → несколько входов.** `Inject(ReadOnlySpan<NodeInstanceId> entries)` — сидит work-list. (Один
   вход — частный случай.) Накопление по инстансам сохраняется.
3. **Алгоритм Run (демандный, ready-driven):**
   - work-list = entries. Цикл, пока не пуст:
     - взять ноду `N`;
     - **мемоизация:** если выход `N` уже посчитан (все Out-порты `IsCalculated`) → пропустить (не гнать
       повторно), но **push** консьюмеров (они могли ждать `N`);
     - **ready-check:** все ли In-порты `N` посчитаны? (см. решение 4)
       - **нет** → **pull**: поставить незакешированных продюсеров (предшественников из
         `GetNodeRelatives(N).inputs`, чьи Out не `IsCalculated`) в work-list **перед** `N`, `N` вернуть в
         очередь (повторно проверится, когда продюсеры посчитаются);
       - **да** → **исполнить** `N` через `NodeInvoker.Invoke` (тело читает готовые входы, пишет выходы);
         затем **push**: поставить консьюмеров (`GetNodeRelatives(N).outputs`) в work-list.
   - Терминирование: нода исполняется ≤1 раза (после — мемоизирована); pull/push добавляют конечное число;
     «в очереди»-флаг гасит дубли. Гарантия прогресса — DAG (циклы = ошибка графа, как в Drain).
4. **Ready-check по УЗЛОВОЙ топологии** (РЕШЕНО (B), 2026-06-18 — меняет гейт #3). Перечислить In-**порты**
   ноды оркестратор не может (число портов в компиляте не лежит — «знает сама нода»). Поэтому ready-check —
   по **нодам-предшественникам** (`GetNodeRelatives(N).inputs`, `InDegree`) + per-node бит `_computed`:
   `Ready(N) ⟺ все предшественники computed`. Константные входы не в `inputs` → не блокируют. Для eager
   (нода производит **все** выходы за раз) per-node ≡ per-port. Не-generic `IsCalculated(cell)` **выпадает из
   M7-B** (вернётся в M7-D/M9 для lazy/частичного/инкрементального skip).
   **Хранилище бит'ов:** два бита на ноду (`queued`/`computed`) — в **per-instance transient-массиве**
   (`UnsafeArray<byte>` размера `NodesCount`), живёт рядом с `InstanceCache` в `ExecutionScope`, сбрасывается
   оркестратором в начале `Run` (решение гейта #1 — «место в кеш-пространстве»). Мульти-инстанс — естественно
   (у каждого инстанса свой массив).
5. **Eager — границы.** В M7-B тело **не тянет** входы (`Get`/`TryGet`/yield — M7-D). Контракт тела:
   `Read` готовых In + `Write` Out (как сейчас в стаб-нодах). `NodeContext` без `Get`/yield.
6. **Один рантайм.** Cross-runtime (Burst↔managed граница, wave) — **M7-C**. В M7-B всё forceManaged ⇒
   `Invoke` всегда успешен синхронно (managed-делегат), ready-driven цикл без wave.
7. **Push-критерий.** В M7-B (кеш сброшен caller'ом до прогона, ничего не посчитано) push = «нода посчитана →
   её консьюмеры могут стать ready» — просто кладём всех консьюмеров в очередь; ready-check отсеет неготовых.
   (Точечный «изменился ли аутпут» для инкрементального пересчёта — M9, тут не нужен.)

## Файлы

| Файл | Изменение |
|---|---|
| `Logic/RuntimeData/Execution/ExecutionGraph.cs` (+тесты `ExecutionGraphTests.cs`) | **удалить** (батч-DAG/`Drain` уходят). `ExecutionBatch`/`RuntimeType` — `RuntimeType` переехать (он нужен), `ExecutionBatch` удалить. |
| `Logic/RuntimeData/Execution/Orchestrator.cs` | переписать: work-list вместо `_graph`; `Inject(span)`; ready-driven `Run`; pull/push/memoize. |
| `Logic/CacheData/InstanceCache.cs` | + не-generic `IsCalculated(Id<CacheLink> cell)` (ready-check). |
| `Logic/RuntimeData/Execution/NodeContext.cs` | без изменений по сути (eager Read/Write уже есть); убрать намёки на Get (их нет). |
| `Tests/NodeDispatchTests.cs` | переписать `Run_*` под work-list `Inject(span)`; ассерты сохранить (цепочка/diamond/мульти-инстанс/persistence/детерминизм/пустой). Мемоизация — новый тест (нода не считается дважды). |
| `Tests/ExecutionGraphTests.cs` | **удалить** (тестировал батч-DAG). Топология теперь косвенно через work-list-тесты. |

## API-скетч

```
struct Orchestrator : IDisposable
    Id<MemoryManager>            _memoryId
    UnsafeList<NodeInstanceId>   _queue      // work-list (FIFO ring/cursor)
    UnsafeList<bool>/bitset      _queued     // дедуп «уже в очереди» (по слоту инстанса? см. open q)
    Create(memoryId)
    Inject(ReadOnlySpan<NodeInstanceId> entries)        // сидит _queue
    int Run(ref ExecutionScope scope, ref CompiledBlueprintHeader compiled)  // прогон до опустошения
    Dispose()
```

## Шаги / задачи

| Задача | Концепт | Статус |
|---|---|---|
| **T01** — ready-check по узловой топологии (`relatives`) + per-node bitset'ы (вместо `IsCalculated(cell)` — реш. (B)) | ✅ (свёрнут в T02) |
| **T02** — `Orchestrator` work-list: `Inject(span)` + ready-driven `Run`; per-instance `_schedule` (queued/computed); pull + push-реактивация + memoize | ✅ |
| **T03** — снос `ExecutionGraph`/`ExecutionBatch`/`Drain` + `ExecutionGraphTests`; `RuntimeType` → отдельный файл | ✅ |
| **T04** — `NodeDispatchTests` под `Inject(span)`; + `Run_SharedAncestor_RunsOnce` (мемоизация) | ✅ |

> **Self-review (adversarial):** Blocker/Major нет. Исправлены 2 Minor — (1) убран self-re-enqueue → push-
> реактивация (терминирует на циклах, как старый Drain); (2) DEBUG-assert на reuse schedule-слота. Nit (stale
> `Drain` doc-комментарии) — поправлены. **Компиляция/тесты под Unity здесь не прогонялись** (нет Unity).

## Тесты (доказательная база)

Те же сценарии, новый двигатель: цепочка A→B→C (=20), diamond (=23, join ждёт оба), мульти-инстанс (10/20),
persistence ×2, детерминизм (caller-reset перед каждой итерацией), пустой вход. **+ мемоизация:** граф, где
нода-источник — общий предок двух потребителей; источник исполняется **ровно один раз** (счётчик исполнений в
стаб-ноде через Persistence). Single-runtime forceManaged ⇒ реальное исполнение в plain .NET.

## Не входит (позже)

- **Lazy-ноды** (`TryGet`/yield, re-execution) — **M7-D**.
- **Runtime-wave** (Burst↔managed граница) — **M7-C**.
- **Мульти-блюпринт** (разные `compiled` за прогон) + **command-buffer** — **M7-E**.
- **Параллелизм** — вне M7.
- **Push с «изменился аутпут»** / инкрементальный пересчёт / ExecRef — **M9**.

## Решения гейта (2026-06-18)

1. ✅ **Дедуп «уже в очереди»** — bitset размера `NodesCount` на инстанс, чистится перед прогоном.
   **Можно выделить под него пространство в кеше** (per-instance transient-регион), а не отдельным буфером.
2. ✅ **Pull-порядок — FIFO** (продюсеры в хвост, ноду ре-энкью; ready-check отсеет; детерминизм от `inputs`).
   `Inject` принимает **список инпутов** (span) на обработку.
3. ✅ **ready-check** — не-generic `InstanceCache.IsCalculated(cell)` (читает `state`, тип не нужен) — см.
   разъяснение ниже (T01).

> **Задел под M7-D (НЕ ломать в M7-B).** Кодогенное решение для вызова managed/unmanaged ноды **посреди тела**
> исполнения (lazy `Get`-yield + re-execution/стейт-машина) приходит в M7-D. M7-B должен оставить
> архитектуру **открытой** под него: `Inject(span)` + work-list + per-node диспатч — общий субстрат; lazy
> добавит yield-путь + возобновление поверх, **без переделки ядра**. `NodeContext` в M7-B — eager (Read/Write),
> но его расширение `TryGet`/yield в M7-D не должно требовать смены сигнатур work-list/`Inject`/`Run`.

### Разъяснение T01 — зачем не-generic `IsCalculated`

Оркестратору надо ответить «можно ли запускать ноду N?» = «посчитаны ли **все** её входы?». Для каждого входа
он смотрит ячейку кеша, которую вход читает: она `Calculated` или ещё `Uninitialized`? Существующий метод —
`IsCalculated<T>(CacheHandler<T>)`, **generic по типу значения** `T` (long/float/…). Но оркестратор бежит по
In-портам ноды **по ordinal'у** (из Map) и типов портов **не знает**. А проверка «посчитано ли» читает только
**байт `state` ячейки** (Calculated/Uninitialized/Link, следуя link'ам) — тип `T` для этого **не нужен** (он
нужен лишь чтобы прочитать само значение). ⇒ добавляем перегрузку `IsCalculated(cell)` без `T`; типизированные
`IsCalculated<T>`/`Read<T>` остаются для тел нод (они свой `T` знают).
