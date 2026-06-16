# M6 — Node dispatch + dual backend (разбивка вехи на под-фазы)

> **Статус: 🔄 разбивка на гейте (план, код не писан).** Это milestone-expansion (как
> [phase-4/runtime/README.md](../phase-4/runtime/README.md) для Фазы 4): сначала разбить M6 на под-фазы
> и согласовать решения, затем исполнять по одной (каждая = своё ревью-сидение, свой план-гейт,
> компилируется, без полу-состояний).
>
> Источник правды — код, затем [STATE.md](../STATE.md). Цель вехи (PLAN.md):
> startup-реестр Burst-функций нод через `BurstCompiler.CompileFunctionPointer`, адресация **по индексу**
> (как `IndexedTypes.Initialize`); параллельный plain-.NET путь, детерминизм в обоих; **version gate**
> (отклонять блоб с несовпадающей версией); закрыть заглушку `NodeTypeId` и кеш `NodeInvoker`.

## Контекст как-построено (что substrate уже даёт)

- **Dispatch-id ноды.** `Data/NodeTypeId.cs` **снесён**. Сейчас id метода ноды — `TypeId<INode>` в
  `NodeHeader.typeId` (`StaticData/NodeHeader.cs:25`), пишется на компиляции из `INode.NodeTypeId`
  (`BlueprintCompiler.cs:92`), а `INode<TLogicNode>` отдаёт `TypeIdOf<INode, TLogicNode>.typeId`
  (`Blueprint/INode.cs:10`). **Заглушка в том, что `INode` — НЕ `IIndexedType`-контекст**, поэтому
  `TypeIdOf<INode, …>` не имеет реального плотного индекса (контекст-дети не зарегистрированы). Зато
  `ILogicNode : IIndexedType` (`Logic/ILogicNode.cs:9`) — это **готовый** индексируемый контекст: генератор
  (`TypeIndexerGenerator.BuildContextMappings`) уже строит дети-индексы для любого индексируемого
  интерфейса (как сделано для `INodeContext`, 4E).
- **Тело ноды.** `ILogicNode` — **пустой маркер** (DoBurst-овэрлоды снесены вместе с edge-моделью). Нет ни
  сигнатуры исполнения, ни контекста.
- **Реестр по индексу — прецедент.** `IndexedTypes.Initialize` (`TypeIndexer/IndexedTypes.cs:54`) заполняет
  плоский `Delegate[] _delegates` + словари, адресуемые по индексу; генератор эмитит initializer. **NB:
  `CompileFunctionPointer` в кодовой базе сейчас НЕ используется нигде** — прокси ходят через managed
  `Delegate[]`. Burst-fn-pointer-таблицу M6 вводит впервые.
- **Память инстанса.** `ExecutionScope` (`Logic/ExecutionScope.cs`) — владелец `InstanceCache` /
  `InstancePersistence` + `ContextRegistry<INodeContext>`; `GetInstanceCache`/`GetInstancePersistent`/
  `GetContext<T>`. Static-слайс/In-Out/Map — на `CompiledBlueprintHeader` (`GetStaticNodeSlice`/
  `GetNodeInOut`/`GetNodeRelatives`). **Всё, что нужно ноде при исполнении, уже резолвится — нет только
  seam'а, который соберёт это в один контекст и вызовет функцию.**
- **Порядок обхода.** `ExecutionGraph.Drain` даёт детерминированный single-thread порядок `NodeInstanceId`.
  M6 прогоняет этот порядок **через диспетчер** (без параллелизма/wave — M7).
- **`TypeId`→`int`.** `TypeId`/`TypeId<T>` неявно приводятся к `int` — индекс в массив реестра без явного
  каста (память `no-explicit-id-int-cast`).

## Решения по развилкам (на гейт)

| # | Развилка | Предлагаемое решение | Где лендится |
|---|---|---|---|
| 1 | **Index space диспатча** | Перевести dispatch-id на **`TypeId<ILogicNode>`** (плотный ordinal: `ILogicNode` — уже индексируемый контекст, дети = unmanaged logic-типы). Миграция `NodeHeader.typeId`/`CompiledBlueprintHeader.GetNodeTypeId`/`INode.NodeTypeId`: `TypeId<INode>` → `TypeId<ILogicNode>`; `INode<TLogicNode>` бейкает `TypeIdOf<ILogicNode, TLogicNode>.typeId`. **`INode` индексируемым НЕ делаем** (он managed-authoring). Это и есть закрытие заглушки `NodeTypeId`. **Локальный ordinal бейкается прямо в блоб** — его валидность гарантирует version gate (развилка 7): при совпадении «версии кода» набор/порядок `IndexedTypes` идентичны на сервере и клиенте, значит ordinal стабилен (в отличие от id блюпринтов, которые рантайм и требуют guid-резолва). | **M6-A** |
| 2 | **Контракт исполнения ноды** | Узловой (не node-agnostic). Тело ноды — данные в static-слайсе (`NodeContext.Body<T>()`) + логика `ILogicNode.Execute(ref NodeContext)`. `NodeContext` — Burst-совместимый seam (без managed-ссылок): static-слайс/Body + Cache + persistence + блок In/Out (через Map). Дисптач — `FunctionPointer<ExecuteFn>` + generic-адаптер `NodeInvoker.Execute<T>`/`Compile<T>` (как исторический `CompileDoNode/DoBurst`). **Без виртуальных методов в рантайме:** диспатч по fn-pointer-индексу (без vtable), `body.Execute` в монолитной `Execute<T>` девиртуализуется Burst'ом. Ambient-context — M7. | M6-B |
| 3 | **Кто строит function-table** | Mirror `IndexedTypes`: отдельный `Initialize`, заполняемый **reflection-сканом** `ILogicNode`-типов на startup (кодогена нод нет до M10). **Managed-таблица (`NodeFn`-делегаты) строится всегда** (универсальный путь, единственный в .NET); **Burst-таблица (`FunctionPointer`) — только под Unity** (`#if UNITY_5_3_OR_NEWER`) и **только для `Unmanaged`-нод**, компиляция один раз/кеш. Согласовать с M10-кодогеном (initializer станет генерируемым, как у TypeIndexer). | M6-C |
| 4 | **Хранилище реестра** | Плоский статический `FunctionPointer<NodeFn>[]` (Burst) + `NodeFn[]` (managed), индекс = `TypeId<ILogicNode>` ordinal, размер = `TypeId<ILogicNode>.Count`. Статический (process-wide), не per-world — как `IndexedTypes`. | M6-C/D |
| 5 | **Выбор бэкенда** | Per-node по `NodeHeader.runtimeType`: **`Managed` = строго NoBurst** (тело может лезть в managed-код) ⇒ реестр **не** компилит для неё Burst (упал бы), держит только managed-делегат; **`Unmanaged`** → Burst fn-ptr (под Unity) **+** managed-делегат (fallback/.NET). В чистом .NET Burst недоступен ⇒ исполнение всегда по managed-таблице. Чередование Burst↔Managed pass (wave-модель) — M7. | M6-D |
| 6 | **Сигнатура Execute** | `ILogicNode.Execute(ref NodeContext)` (тело резолвится адаптером `NodeInvoker.Execute<T>` через `Body<T>()`). Адаптация типа в указатель — `Compile<T>()` → `CompileFunctionPointer<ExecuteFn>(Execute<T>)` (как исторический `CompileDoNode<T>/DoBurst<T>`). Реестр по индексу `TypeId<ILogicNode>` (managed `ExecuteFn` всегда / Burst `FunctionPointer` под Unity) — M6-C. | M6-B/C |
| 7 | **Version gate: что и где** | Гейт сверяет **только «версию кода»** — детерминированный **авто-хеш сигнатур/контракта нод-функций** (`NodeFn`-ABI + раскладка `NodeContext` + сигнатуры используемых `ILogicNode`), **инвариантный к набору/количеству/порядку** блюпринтов и нод. Набор/порядок в контракт НЕ входят: id блюпринтов рантайм-генерятся, адресуются по **guid** (рантайм-id — кеш, как в Content-системе), а блоб самодостаточен по static-данным. Гейт при `CreateInstance`/загрузке блоба: хеш в блобе == локальный → исполнять, иначе → reject. Точную форму хеша (что входит, per-node по стабильному id vs глобально, где сверяется) — финализировать на гейте M6-E. | M6-E |

## Разбивка на под-фазы

| Под-фаза | Концепт | Развилки | Статус |
|---|---|---|---|
| **M6-A — Dispatch index** | Закрыть заглушку `NodeTypeId`: dispatch-id → `TypeId<ILogicNode>` (плотный ordinal). Миграция `NodeHeader.typeId`/`GetNodeTypeId`/`INode.NodeTypeId`/`INode<T>`. Исполнения нет. | 1 | ✅ одобрено (не закоммичено) |
| **M6-B — Node execution contract** | `NodeContext` (seam резолва памяти) + `ILogicNode.Execute(ref NodeContext)` + `delegate ExecuteFn` + адаптер `NodeInvoker.Execute<T>`/`Compile<T>` (FunctionPointer, как исторический `CompileDoNode/DoBurst`). Диспатч по fn-pointer-индексу, без vtable. | 2, 6 | ✅ done (закоммичено) |
| **M6-C — Burst registry (by index) + cache** | `NodeFunctionRegistry.Initialize`: startup-компиляция `FunctionPointer<NodeFn>` по `TypeId<ILogicNode>` ordinal (mirror `IndexedTypes.Initialize`), компиляция один раз/кеш. Закрыть «кеш `NodeInvoker`». | 3, 4 | ☐ todo |
| **M6-D — Managed (.NET) backend + selection** | Параллельная managed-таблица `NodeFn[]`; выбор бэкенда по `NodeHeader.runtimeType`; глобальный managed-форс. **Детерминизм Burst↔.NET.** Реальные .NET-тесты исполнения (managed-путь идёт без Burst). | 4, 5 | ☐ todo |
| **M6-E — Version gate** | Версия function-table + отклонение несовместимого блоба при `CreateInstance`/dispatch. | 7 | ☐ todo |
| **M6-F — Dispatcher integration + reconcile** | `NodeInvoker.Invoke(ref scope, ref compiled, NodeInstanceId)` — резолв памяти + dispatch через реестр; прогон `ExecutionGraph.Drain`-порядка через него (single-thread). Свод + апдейт STATE.md/CLAUDE.md status-map. | — | ☐ todo |

## Не входит (M7/M8/M9/M10)

- **Джоб-параллелизм + wave-модель** (чередование Burst↔Managed pass, бакетинг батчей по `runtimeType`) — **M7**.
- **Pull-based Is-Calculated мемоизация** в run'е (гейт `NodeState.HasCache`) — **M8**.
- **Ambient-context Burst-side proxy-резолв** в run'е (node-side `GetContext<T>` через прокси) — **M7**
  (в M6-B `NodeContext` несёт лишь accessor-seam; полный Burst-резолв — позже).
- **Кодоген нод** (initializer таблицы из генератора, partial `Execute`) — **M10**; в M6 reflection-startup.
- **ExecRef / composability / typed I/O** — **M9**.

## Решения пользователя (гейт разбивки, 2026-06-16)

1. **Index space (развилка 1).** ✅ `TypeId<ILogicNode>` как dispatch-id; `INode` индексируемым не делаем.
2. **Глубина `NodeContext` (M6-B).** ✅ Включить In/Out **+ persistence** сейчас; ambient-context
   Burst-резолв — на M7.
3. **Version gate (развилка 7).** ✅ **Авто-хеш сигнатур/контракта нод-функций** («версия кода»),
   инвариантный к набору/порядку. Не хеш набора имён, не явный ручной int. Точная форма — на гейте M6-E.

5. **Специализация под ноды + FunctionPointer (уточнения 2026-06-16).** ✅ Дисптач **не** абстрагируем от нод
   (`IExecutable`/`Executor` откатаны). Восстановлен исходный механизм: `FunctionPointer<ExecuteFn>` + адаптер
   `NodeInvoker.Execute<T>`/`Compile<T>` (как `CompileDoNode<T>/DoBurst<T>`). «Без виртуальных» = диспатч по
   fn-pointer-индексу, без vtable (вызов тела в монолитной `Execute<T>` девиртуализуется). `ILogicNode` несёт
   `Execute(ref NodeContext)`. Static-abstract interface members в Unity 6 (runtime/Burst) недоступны.

4. **Кросс-среда + Burst/NoBurst гранулярность (уточнение 2026-06-16).** ✅ Весь код M6 (контракт, реестр,
   диспетчер) **компилируется и в Unity (Burst), и в чистом .NET-приложении** (сервер без Unity/Burst).
   Unity.Burst-зависимости (`[BurstCompile]`, `BurstCompiler.CompileFunctionPointer`) — **строго под**
   `#if UNITY_5_3_OR_NEWER` (прецедент: `SafePtr`/`MemoryExt`/`IndexedTypes`). Seam (`NodeContext`/`NodeFn`/
   `Execute<T>`/`ILogicNode.Execute`) использует только кросс-средовые core-типы Sapientia ⇒ guards не нужны.
   **Гранулярность Burst-vs-NoBurst — по ноде** (`RuntimeType`): `Managed`-нода = строго NoBurst (Burst для
   неё не компилится), `Unmanaged` — Burst+managed. Лендится в M6-C (раздельная сборка таблиц) / M6-D (выбор).

### Смежное (не входит в M6-dispatch, но всплыло на гейте)

- **Адресация блюпринтов по guid + рантайм-кеш id** (как Content-система): `int` id блюпринта
  рантайм-генерится, стабильный ключ — guid. Текущий `blueprintKey: VersionedId<Blueprint>` несёт
  рантайм-`Id<Blueprint>` + authoring-version (generation для staleness инстансов). Слой guid→id
  (резолв+кеш) — это **identity/authoring** (M11-ish), не диспатч нод M6. M6 на него не завязан: ноды
  адресуются ordinal'ом под защитой version gate, блюпринты — отдельной историей.

## Верификация (как и в 4A–4F)

Batchmode-раннер не гоняем (segfault в EDM4U — окружение). Верификация — компиляция-инспекция +
adversarial-сабагент по working-tree diff + изолированный `dotnet build` минимального репро для нетривиальной
компиляции. **Особенность M6-D: managed-путь исполняется в plain .NET** → даёт реальные детерминированные
unit-тесты исполнения нод даже без Burst/Unity (Burst-таблица и `IndexedTypes`-инициализация в EditMode — под
`Assert.Ignore`, как в 4E/4F-2).
