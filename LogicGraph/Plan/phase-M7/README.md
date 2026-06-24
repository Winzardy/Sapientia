# M7 — Orchestrator (разбивка вехи на под-фазы)

> **🛑 МОДЕЛЬ ПЕРЕСМОТРЕНА (2026-06-18).** Читать первым раздел **«Целевая модель исполнения (СОГЛАСОВАНО)»**
> ниже — он авторитетен. Статический батч-DAG/`Drain` подход и старая разбивка «wave-passes + parallelism»
> (секции «Контекст как-построено», «Решения по развилкам», частично — ниже) **отменены/переосмыслены**:
> расписание теперь **демандное (work-list оркестратора)**, не предвычисленный батч-DAG.
>
> **Статус: разбивка согласована (2026-06-18), под-фазы не начаты.** Это milestone-expansion (как
> [phase-M6/README.md](../phase-M6/README.md) для M6 и [phase-4/runtime/README.md](../phase-4/runtime/README.md)
> для Фазы 4): сначала разбить M7 на под-фазы и согласовать решения, затем исполнять по одной (каждая =
> своё ревью-сидение, свой план-гейт через `/logicgraph-phase`, компилируется, без полу-состояний).
>
> Источник правды — код, затем [STATE.md](../STATE.md). Цель вехи (PLAN.md): **dependency scheduling +
> Burst/non-Burst wave-passes + параллелизм + мульти-блюпринтовый прогон**. Здесь же `Run` переезжает из
> `NodeInvoker` и планирование вызовов переделывается.

## Контекст как-построено (что substrate уже даёт)

- **Батч-DAG.** `ExecutionGraph` (`Logic/RuntimeData/Execution/ExecutionGraph.cs`): `Inject(ref compiled,
  BlueprintInstanceId)` — chain-декомпозиция Static-топологии (батч = линейная цепочка), **накапливает
  батчи по инстансам** (multi-instance одного блюпринта уже работает; индексы потомков — глобальные).
  `Drain(Span<NodeInstanceId>)` — детерминированный single-thread обход (ready-queue, FIFO от
  `_startBatches`, декремент `remainingDeps`). `ResetDeps`/`Dispose`. `ExecutionBatch{ inDegree,
  remainingDeps (синхронный int), nextBatches, nodesOrder }`. `enum RuntimeType{Unmanaged,Managed}`.
- **Диспатч ноды.** `NodeInvoker` (`Logic/RuntimeData/Execution/NodeInvoker.cs`): `Invoke(ref scope, ref
  compiled, NodeInstanceId)` — сборка `NodeContext` из памяти инстанса + выбор бэкенда раз/ноду + вызов
  `InvokeBurst`(blittable function-table `UnsafeArray<FunctionPointer<ExecuteFn>>`, `[BurstCompile]`)/
  `InvokeManaged`(`ExecuteFn[]`). **`Run(ref scope, ref compiled, ref graph, Span<NodeInstanceId>)`** —
  run-prologue (`ResetAllCache`+`ResetDeps`) → `Drain` → per-node `Invoke`. **`Run` — временный seam M6-F,
  переезжает в M7** (память `logicgraph-run-temporary`).
- **Память инстанса.** `ExecutionScope` (`Logic/ExecutionScope.cs`): владелец `InstanceCache`/
  `InstancePersistence` (off-allocator) + `ContextRegistry<INodeContext>` + shared-копия
  `NodeFunctionRegistry _registry` (managed-поле ⇒ scope **не Burst-блиттабл**). `GetInstanceCachePtr`/
  `GetInstancePersistencePtr` отдают **blittable** `SafePtr<…>` (валидны, пока store не ресайзится).
- **Function-table.** `NodeFunctionRegistry` (`Logic/RuntimeData/Execution/NodeFunctionRegistry.cs`):
  `BurstTable` (`UnsafeArray<FunctionPointer<ExecuteFn>>`, off-allocator, **blittable**, под `#if UNITY`) +
  `ManagedTable` (`ExecuteFn[]`, managed). `UseManaged(runtimeType)` — выбор бэкенда. Индекс — ordinal
  `TypeId<ILogicNode>` (== `NodeHeader.typeId`).
- **Параллелизм-задел.** `ExecutionBatch.nextBatches` уже несёт зависимости для параллелизма;
  `remainingDeps` — синхронный счётчик; phase-4 wave-модель (директива) описывает чередование Burst↔Managed
  и parked-буфер. Форк 6 phase-4 (`iterationsToSchedule`) был **отложен «до гейта M7»** — здесь решается.
- **Кросс-среда.** Весь диспатч M6 компилируется и под Unity (Burst), и в чистом .NET (память
  `logicgraph-dispatch-cross-env`): Burst-зависимости строго под `#if UNITY_5_3_OR_NEWER`. M7 это
  наследует (threading seam — два impl).

## Решения по развилкам

> ⚠️ **Частично отменено (2026-06-18).** Развилки 2/3/5/7 (threading/wave-barrier/scope-in-job/детерминизм
> параллелизма) относились к **батч-DAG параллелизму** — он вынесен за M7 (поздняя оптимизация), форма
> пересматривается. Развилка 1 (оркестратор) — сделана. Развилка 6 (мульти-блюпринт) — в силе (M7-E), но
> резолв `compiled` идёт через work-list, не per-node-в-Drain. Развилка 4 (wave) — в силе как «граница
> рантайма» (M7-C). Актуальная модель — раздел «Целевая модель исполнения» выше.

| # | Развилка | Решение | Где лендится |
|---|---|---|---|
| 1 | **Сущность оркестратора + где живёт `Run`** | Новый struct (`Orchestrator`/`GraphRunner`), владеющий `ExecutionGraph` + order-буфером + next-wave-буфером; `Run(ref scope, ref storage, …)` принимает scope/storage по ref. `NodeInvoker` откатывается до **чистого диспатча** (`Invoke`/`InvokeBurst`/`InvokeManaged` остаются, `Run` уходит). Закрывает долг «`Run` временный». | **M7-A** |
| 2 | **Threading primitive (кросс-среда Unity+.NET)** | ✅ **Узкий seam «parallel-for по готовым батчам wave» с двумя impl**: `IJobParallelFor` под `#if UNITY_5_3_OR_NEWER` / `Parallel.For` (или пул) в чистом .NET. Зеркало dual-backend диспатча M6-C/D. | M7-D |
| 3 | **Бухгалтерия `remainingDeps` под параллелизмом** | ✅ **Wave-barrier**: параллелизм *внутри* wave, декремент зависимостей и набор next-ready — **сериально между wave'ами** (планировщик собирает завершения, декрементит в один поток, запускает следующую параллельную пачку). Без атомиков, детерминизм гарантирован конструктивно. **Отменяет** заложенное phase-4 (форк 2) «поток = стартовый батч + его wave». | M7-D |
| 4 | **Wave-чередование + parked-буфер (`iterationsToSchedule`, форк 6 phase-4)** | Готовые батчи бакетятся в две очереди по `RuntimeType`; исполняем очередь текущего типа wave, новые ready капаем в нужный бакет; тип wave флипается, когда текущий бакет опустел (Burst↔Managed чередование). Точную форму буфера финализировать на гейте M7-B. | M7-B |
| 5 | **Scope-в-Burst-job (blittable payload)** | `ExecutionScope` несёт managed-поле ⇒ job его не видит. Managed-glue **пре-собирает payload wave'а** (off-allocator буфер `NodeContext` + ordinals — всё blittable), Burst-job итерирует `InvokeBurst`. **Managed-wave** идёт обычным путём (без job). Чистое разделение: Burst-wave джобится, Managed-wave — нет. | M7-D |
| 6 | **Мульти-блюпринт: резолв `compiled` per-node** | Оркестратор резолвит per-node: `NodeInstanceId.blueprintId → BlueprintInstanceHeader.blueprintId (VersionedId) → storage.Get(key)`; кешировать compiled-ptr на инстанс (не лукапить на каждую ноду). Группа = **независимые** блюпринты; вложенность/ExecRef → M9. | M7-C |
| 7 | **Контракт детерминизма под параллелизмом** | Записи в cache-ячейки **непересекающиеся** (нода — в одном батче, deps упорядочены; fan-out — read-only конкурентно). M7 не трогает тела нод ⇒ детерминизм = «результат не зависит от порядка завершения». Верификация — **A/B тест** parallel-vs-serial + forceManaged-vs-Burst, сверка стейта инстансов. | спина M7-D |

## Разбивка на под-фазы (ПЕРЕПЛАНИРОВАНО 2026-06-18 под демандную модель)

> Старая разбивка (батч-DAG wave-passes + parallelism) — **отменена** (см. «Целевая модель исполнения»).
> Новая разбивка — **предложение, ждёт гейта пользователя** (`/logicgraph-phase` milestone-expansion).

| Под-фаза | Концепт | Статус |
|---|---|---|
| **M7-A — Orchestrator relocation** | `Run` вынесен из `NodeInvoker` в `Orchestrator`; per-node диспатч чистый; сброс кеша убран из `Run`. | ◐ сделано (валидно, остаётся) |
| **M7-B — Work-list core (eager, single-thread, один рантайм)** | **Заменяет `ExecutionGraph`/батч-DAG/`Drain` на месте** (и сносит тупиковую entry-node работу + её тесты). Демандный work-list в оркестраторе; `Inject(span NodeInstanceId)`; **eager-резолв инпутов** до запуска ноды; **per-port мемоизация** (посчитанное не пересчитываем); **push** изменённых аутпутов. Тела атомарны. Без lazy/wave/параллели. Тесты — forceManaged. | 🔄 план-гейт ([plan](B-worklist-core/plan.md)) |
| **M7-C — Runtime-wave (Burst↔managed)** | Граница рантайма: нода другого рантайма откладывается на следующий wave, чередование; нужно **даже single-thread**. Мемоизация через wave. | ☐ не начата |
| **M7-D — Lazy-ноды (re-execution)** | Контракт `ctx.TryGet`-yield; оркестратор резолвит запрошенного продюсера + перезапускает тело (fast-forward по кешу); side-effect commit-on-completion. Рукописно (без кодогена). | ☐ не начата |
| **M7-E — Мульти-блюпринт + command-buffer** | Прогон входов **нескольких** независимых блюпринтов в одном run; deferred command-buffer для managed side-effect'ов (аналитика и т.п.). | ☐ не начата |

Порядок **B→C→D→E** (B — ядро, заменяет батч-DAG; C и D независимы поверх B; E — поверх B).

**Сдвиги дорожной карты:** мемоизация (`Is-Calculated`) была M8 — теперь **ядро M7-B** (демандная модель без
неё не работает). Параллелизм (джоб-батчинг same-runtime wave) — **вынесен за M7** в отдельную позднюю веху, по
замерам. ExecRef / рантайм-ссылка-на-ноду / сброс кеша поддерева (динамический инжект из тела) — **M9**.
Кодоген (обвязка + lazy-стейт-машина) — **M10**.

## Целевая модель исполнения (СОГЛАСОВАНО 2026-06-18) — авторитетно

> Этот раздел **отменяет** статический батч-DAG/`Drain` подход (4A–4B) и старую разбивку «wave-passes/
> parallelism» ниже. Договорились по итогам обсуждения в чате (примеры: ранне-выходная `for`-нода с managed-
> инпутом посередине; аналитика как deferred-команда; чтение managed-меты как целая managed-нода).

### Принципы

1. **Нет статического `Drain`/батч-DAG.** Расписание **демандное, рождается во время исполнения**. Запечка
   расписания под конкретные входы — возможная будущая оптимизация, не сейчас.
2. **Оркестратор владеет демандным work-list'ом.** `Inject` сидит его **несколькими** входными нодами
   (`NodeInstanceId[]`/span), а не одной.
3. **Резолв зависимостей — в оркестраторе (managed work-list), НЕ в телах нод.** Тело ноды **никогда не зовёт
   тело другой ноды** ⇒ нет глубокого стека, нет размотки потребителей при прерывании.
4. **Eager-нода (дефолт, большинство):** оркестратор резолвит **все** инпуты до запуска тела; тело атомарно,
   читает готовое, без `Get`/yield — **ноль бойлерплейта**.
5. **Lazy-нода (opt-in):** для **раннего выхода** / ленивого подтягивания тяжёлых инпутов. Тело тянет через
   `ctx.TryGet`; на miss → **уступает** (`if (!TryGet) return;`); оркестратор резолвит запрошенного продюсера
   и **возобновляет**. Возобновление: **re-execution сейчас** (перезапуск с верха + fast-forward по кешу) /
   **кодогенная стейт-машина на M10** (контракт автора тот же). Платят только lazy-ноды.
6. **Инпуты — pull (`Get`/`TryGet`); аутпуты — push:** в конце исполнения нода триггерит зависимых своих
   **изменённых** аутпутов (оркестратор их планирует).
7. **Кеш — per-port** (`InstanceCache.IsCalculated`). Мемоизация: посчитанный порт **не пересчитывается**.
   **Сброс кеша — вызывающим, один раз перед итерацией апдейта** (Update-/LateUpdate-пачка), НЕ в `Run`.
8. **Side-effect'ы — deferred command-buffer**, коммит только при полном завершении ноды (перезапуск сбрасывает
   буфер — нет двойной аналитики). Managed side-effect (аналитика) = **0 границ рантайма**.
9. **Wave = граница рантайма** (Burst↔managed): на следующий wave уходят только ноды **другого рантайма**, всё
   одинакового — продолжает течь. Число wave = чередование рантаймов на крит-пути (**интринсивно**, моделью не
   уменьшается). Нужно **даже в single-thread** (Burst-нода не может синхронно вызвать managed).
10. **Параллелизм** (батчить готовые same-runtime ноды в джобы) — **отдельная поздняя оптимизация**, по замерам.
11. **Три категории нод:** pure-Burst · managed-read (целая managed-нода, граница = ребро) · deferred-side-effect
    (команда, без границы).
12. **Dual backend:** managed-нода исполняется managed **везде**; на сервере (всё managed) cross-runtime-промахов
    нет → нет yield'ов; на клиенте — Burst/managed split. Результаты консистентны (мемоизация).

### Кодоген — два разных (модель работает и без него)

- **(a) Обвязка** (все ноды): регистрация в function-table, Burst/managed-адаптеры, бейк port-handle'ов,
  `NodeContract`-хеш. Аддитивна, тело не трогает. **M10.**
- **(b) Стейт-машина** (только lazy-ноды): разворачивает `Get`-yield тело в `switch`+кадр (как Roslyn для
  итераторов, но Burst-вид без GC; кадр — фикс-слот в transient-состоянии ноды, сохраняется только `_state`,
  живые-через-yield локали подняты в кадр). **M10.** До неё lazy живут на re-execution, пишутся руками.

### Последствия для текущего кода

- **`ExecutionGraph` (батч-DAG/`Inject`/`Drain`, 4A–4B) — вытесняется**: его роль (топология+обход) заменяет
  демандный work-list оркестратора. Решение по очистке — см. разбивку (M7-A').
- **Работа этой сессии по entry-node `Inject` + миграция тестов — тупиковая** (батч-DAG уходит).
- **Остаётся валидным:** relocation `Run`→`Orchestrator` (M7-A), per-node диспатч `NodeInvoker`
  (`InvokeBurst`/`InvokeManaged`), убранный из `Run` сброс кеша, per-port `InstanceCache`/`IsCalculated`.

## Не входит (M8/M9/M10)

- **Pull-based Is-Calculated мемоизация** в run'е (гейт `NodeState.HasCache`) — **M8**.
- **ExecRef / blueprint-as-node / вложенный вызов / typed I/O** — **M9** (M7 = только *независимая* группа).
- **Кодоген** (генерируемый initializer таблицы, partial `Execute`) — **M10**; в M7 reflection-startup как в M6.

## Решения пользователя (гейт разбивки, 2026-06-18)

1. **Бухгалтерия deps (развилка 3).** ✅ **Wave-barrier** — параллель внутри wave, dep-учёт сериально
   между wave'ами. Отменяет phase-4 «поток на стартовый батч».
2. **Threading primitive (развилка 2).** ✅ **Seam с двумя impl** (Unity Jobs под `#if` / .NET parallel) —
   зеркало dual-backend.
3. **Порядок под-фаз.** ✅ **A→B→C→D** (сначала wave-структура single-thread, потом multi-blueprint, потом
   параллелизм-капстоун).

> Развилки 1, 4, 5, 6, 7 — приняты по рекомендации; точная форма каждой финализируется на план-гейте
> своей под-фазы (`/logicgraph-phase`), как было в 4A–4F / M6-A…F.

## Верификация (как в 4A–4F / M6)

Batchmode-раннер не гоняем (segfault в EDM4U — окружение). Верификация — компиляция-инспекция +
adversarial-сабагент по working-tree diff + изолированный `dotnet build` минимального репро для нетривиальной
компиляции. **Особенность M7-D:** managed-путь исполняется в plain .NET → даёт реальные A/B
детерминизм-тесты (parallel-vs-serial, forceManaged-vs-Burst) даже без Burst/Unity (Burst-таблица и
`IndexedTypes`-инициализация в EditMode — под `Assert.Ignore`, как в 4E/4F-2/M6-C).
