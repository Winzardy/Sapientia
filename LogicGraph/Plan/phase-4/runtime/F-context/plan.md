# Под-фаза 4F-2 — Context-реестр на ExecutionScope

**Статус: ✅ done (одобрено пользователем, не закоммичено → коммитится).** Обзор слоя — [../README.md](../README.md); снимок — [../../../STATE.md](../../../STATE.md).

## Цель

Дать `ExecutionScope` **ambient-context-реестр** «тип контекста → указатель»: ноды при исполнении (M7)
достают нужный контекст по типу. Контексты объявлены нодами и забейканы компилятором в
`CompiledBlueprintHeader.contextTypes` (4E); здесь — runtime-владелец, который их **держит**.

## Решение пользователя (зафиксировано, финал)

> Реестр **владеет** памятью контекстов: массив `SafePtr` размера `TypeId<TContext>.Count`, индексируется по id
> типа; блок выделяется при первой установке (размер — `TSize<T>`). **API чисто generic** (`SetContext<T>`/
> `GetContext<T>`/`HasContext<T>`) — **без id-based методов и без передачи count**. Реестр **generic по категории**
> контекста: `ContextRegistry<TContext>` (scope использует `ContextRegistry<INodeContext>`).

Эволюция решения (итог обсуждения): сырые внешние `SafePtr` → блок+offset-таблица (нужна таблица размеров — нет
в коде) → **владеющий реестр с lazy per-context alloc** (размер из `TSize<T>`, generic-only). Это **переопределяет**
CLAUDE.md §14 (`IndexedPtr`/`ProxyPtr`). Глобальная таблица размеров **не нужна**, генератор TypeIndexer **не трогаем**.
Блоки — off-allocator raw (`MemoryManager`), позиции стабильны ⇒ `SafePtr` не протухает. `count` берётся **внутри**
из `TypeId<TContext>.Count` (не параметр). **Следствие:** id-based reuse-overflow (бывш. M1) **невозможен** — generic-путь
фиксирует размер по `T`, поэтому `_sizes`-гард удалён.

## Разбивка долга 4F-1 — вынесено в 4F-3

Долг 4F-1 (дуальная кеш-раскладка в компиляторе + чистка `BumpHeader.Reset/Size`/`RawBumpAllocator.Size`)
содержит **дизайн-развилку** (как Cache-порт получает два офсета cell+value) и правки компилятора/`MapTests`.
Чтобы ревью реестра осталось плотным, долг вынесен в отдельную **под-фазу 4F-3** (свой гейт). См.
[../README.md](../README.md) и заметку в конце.

## Файлы

| Действие | Путь | Что |
|---|---|---|
| **+** | `Logic/RuntimeData/ContextRegistry.cs` | Реестр `SafePtr` по id контекста (отдельная структура, как `InstancePersistence`). |
| **~** | `Logic/ExecutionScope.cs` | Поле `_contexts: ContextRegistry`; create/dispose; `SetContext`/`GetContext`/`HasContext` (generic + id-based). |
| **+** | `Tests/ContextRegistryTests.cs` | Round-trip / разные id / отсутствующий / overwrite / пустой / dispose (id-based, фабрикуем id — без `IndexedTypes`). |
| **+** | `Tests/ExecutionScopeTests.cs` | Реестр заводится/диспоузится; context set/get через scope (явная capacity); lifecycle-smoke. |

## Public API

```csharp
// ContextRegistry.cs — generic по категории контекста TContext (scope: TContext = INodeContext)
public struct ContextRegistry<TContext> : IDisposable where TContext : IIndexedType
{
    private Id<MemoryManager> _memoryId;
    private UnsafeArray<SafePtr> _slots;            // индекс = (int)TypeId<TContext>; SafePtr на реестр-владеемый блок

    public readonly bool IsCreated { get; }         // _slots.IsCreated
    public readonly int Capacity { get; }           // _slots.Length

    public static ContextRegistry<TContext> Create(Id<MemoryManager> memoryId);  // count = TypeId<TContext>.Count; <=0 → default

    public void SetContext<T>(in T value) where T : unmanaged, TContext;          // первый раз MemAlloc(TSize<T>); затем перезапись
    public readonly ref readonly T GetContext<T>() where T : unmanaged, TContext; // read-only ref; задан обязан (DEBUG-assert)
    public readonly bool HasContext<T>() where T : TContext;                      // в границах и .IsValid

    public void Dispose();                                                        // free каждого блока + _slots
}
```

```csharp
// ExecutionScope.cs (добавления — делегируют в ContextRegistry<INodeContext> _contexts)
public static ExecutionScope Create(Id<MemoryManager> memoryId = default, int instanceCapacity = 8); // без contextCapacity

public void SetContext<T>(in T value) where T : unmanaged, INodeContext;
public readonly ref readonly T GetContext<T>() where T : unmanaged, INodeContext;
public readonly bool HasContext<T>() where T : INodeContext;
```

> `GetContext<T>` отдаёт **`readonly ref T`** (read-only — ambient-контекст меняется только через `SetContext`),
> а не `SafePtr`. Ref берётся из `SafePtr.Value<T>()` (pointer-deref ⇒ unscoped, безопасно возвращается; прецедент —
> `InstanceCache.Cell`). Поэтому `T : unmanaged`.

## Раскладка данных

- `ContextRegistry<TContext>._slots: UnsafeArray<SafePtr>` — off-allocator, размер = `TypeId<TContext>.Count` (все типы
  категории). Индекс = локальный id типа в категории (`TypeId<TContext>` → `int` неявно). Элемент — `SafePtr` на
  **реестр-владеемый** raw-блок (или `default`, если контекст не задан).
- Память контекста — отдельный `MemoryManager.MemAlloc(TSize<T>)` при первой установке типа; повторный `SetContext<T>`
  **переиспользует** тот же блок (один `T` ⇒ один размер) и перезаписывает значение. Generic-only ⇒ размер всегда
  фиксирован по `T` (нет reuse-overflow).
- `ExecutionScope._contexts: ContextRegistry<INodeContext>` — один на scope (ambient, общий для всех инстансов домена).

## Индекс по TypeId — без `(int)`-каста

`TypeId<TContext>` неявно приводится к `int` (`implicit operator int`), поэтому индексирует массив и сравнивается
**напрямую** (`_slots[id]`, `id < _slots.Length`) — явный `(int)`-каст не нужен (директива пользователя; проверено
компиляцией). Отражено в doc-комментарии `ContextRegistry`.

## Execution steps

1. `ContextRegistry` (тип + API) — alloc/set/get/has/clear/dispose.
2. Вшить в `ExecutionScope` (поле, create с `contextCapacity`, dispose, делегирующие методы), обновить doc-комментарий
   (убрать «реестр — 4F-2 next» → описать как сделанный).
3. Тесты: `ContextRegistryTests` (низкоуровнево, фабрикуем id), `ExecutionScopeTests` (через scope, явная capacity).
4. Self-review (компиляция + чеклист + adversarial-сабагент).

## Tasks (индекс)

| # | Задача | Статус |
|---|---|---|
| 01 | [ContextRegistry](tasks/01-context-registry.md) — структура + API | ✅ done |
| 02 | [ExecutionScope wiring](tasks/02-scope-wiring.md) — поле/create/dispose/методы + doc | ✅ done |
| 03 | [Tests](tasks/03-tests.md) — ContextRegistryTests + ExecutionScopeTests | ✅ done |

## Self-review (итог)

- **Компиляция** — сверены символы/usings; generic-рефактор (`TypeIdOf<TContext,T>` при `T : TContext`, `TContext : IIndexedType`
  + implicit `TypeId→int` в сравнениях/индексе) **проверен изолированным `dotnet build`** репро. Прогон тестов отложен (EDM4U).
- **Allocator-safety** — каждый блок контекста + `_slots` освобождаются в `Dispose`; идемпотентно (guard по `_slots.IsCreated`
  + self-null `UnsafeArray.Dispose`); reuse того же типа не теряет блок.
- **Adversarial-сабагент (исходная модель)** — блокеров нет. **M1 (Major) устранён архитектурно**: переход на generic-only API
  (по ревью пользователя) убрал id-based `AllocContext` с произвольным размером ⇒ reuse-overflow невозможен, `_sizes`-гард не нужен
  (удалён). M2 (release-bounds) — согласовано с house-style. **M3 (defensive-copy)** — core-правка `UnsafeArray` → follow-up.
- **Касты `(int)`** — по комментарию пользователя убраны; `TypeId<TContext>` индексирует/сравнивается через implicit `int`
  (задокументировано в `ContextRegistry` + feedback-память `no-explicit-id-int-cast`).

## Follow-ups (на потом)

- **Тесты round-trip** — оживить, когда `INodeContext`-типы регистрируются в тест-окружении (`IndexedTypes` init / забейканные
  контексты). Сейчас под `Assert.Ignore`.
- **M3 / perf:** `readonly` `GetContext`/`HasContext` вызывают non-`readonly` indexer `UnsafeArray<T>` ⇒ defensive-copy на каждый
  вызов. На hot-path M7 — дешёвый фикс = `readonly` getter у `UnsafeArray<T>.this[int]` (core-правка, отдельно).

## Test list

> **Round-trip отложен.** Generic-API адресует контекст по типу `T`, для индекса нужен зарегистрированный в
> `IndexedTypes` `TypeId<INodeContext>` — в EditMode-окружении `IndexedTypes` **не инициализирован**
> (`TypeId<INodeContext>.Count == 0`) ⇒ реестр пуст. Поэтому round-trip идёт под `Assert.Ignore` (готов к запуску,
> когда типы контекста забейканы/зарегистрированы). Lifecycle/empty — прогоняются (по решению пользователя «пусть
> тесты пока что не работают»). Stub-контекст — `StubContext : INodeContext` в `StubNode.cs`.

- `Context_RoundtripWhenRegistered` — **Ignore** в EditMode; иначе `SetContext<StubContext>` → `Has`/`Get` round-trip.
- `Context_EmptyRegistryAndDispose` — *(runs)* пустой реестр (`Count==0`): `Has==false`, `Dispose` no-op + идемпотентен.
- `Scope_CreateDisposeSmoke` — *(runs)* `Create`→`IsCreated`→`Dispose`→`!IsCreated`, повторный `Dispose` ок.
- `Scope_ContextRoundtripWhenRegistered` — **Ignore** в EditMode; иначе set/get/has через scope.

## Non-goals (отложено)

- **Резолв контекста нодой в run'е** (Burst-side, по `contextTypes` блоба) — M7.
- **Обёртки `IndexedPtr`/world-backed контексты** (CLAUDE.md §14 open-subquestion) — не делаем; решение пользователя = `SafePtr` + реестр-владеемый блок.
- **Глобальная таблица размеров типов контекста / бейк генератором** — не нужна (размер из `TSize<T>` на set).
- **Долг 4F-1** (кеш-раскладка, dead-code `BumpHeader.Reset/Size`) — под-фаза **4F-3**.
- **Динамический рост блока контекста** — фикс-размер на тип; повторный set переиспользует блок.

## Deviations от PLAN.md

Нет. PLAN.md Phase 4 = «◐ static-модель done; runtime-каркас → M6/M7»; 4F-2 — инкремент каркаса по
[runtime/README.md](../README.md). Ambient-context на scope соответствует CLAUDE.md §4/§14 (упрощённая раскладка).

## Open questions (на гейт)

1. **Split 4F-2 / 4F-3.** Рекомендую: 4F-2 = реестр (этот план), 4F-3 = кеш-долг (развилка два-офсета).
   Альтернатива — всё в одной фазе (крупнее ревью).
2. ~~Staleness сырого `SafePtr`~~ — **снято**: реестр владеет блоками (raw `MemoryManager`, позиции стабильны),
   указатель не протухает; освобождение — в `Dispose`.
