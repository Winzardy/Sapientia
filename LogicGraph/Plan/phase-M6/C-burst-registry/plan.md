# M6-C — Burst registry (by index) + cache

> **Статус: ✅ done (одобрено 2026-06-16).** Под-фаза вехи M6 ([../README.md](../README.md), развилки 3, 4).
> Источник правды — код, затем [../README.md](../README.md) / [../../STATE.md](../../STATE.md).
> Реестр — **инстанс** (`struct : IDisposable`, без `SharedStatic`), Burst-таблица = off-allocator
> `UnsafeArray<FunctionPointer<ExecuteFn>>`, managed-таблица = `ExecuteFn[]`. Интеграция в `ExecutionScope` — M6-F.

## Цель

Дать **function-table нод по индексу** `TypeId<ILogicNode>` — то, что в M6-B было только заготовкой
(`NodeInvoker.Compile<T>`/`GetManaged<T>` адаптируют **один** тип в указатель, но никто не собирает их в
таблицу). M6-C вводит `NodeFunctionRegistry`: на startup один раз заполняет таблицу (Burst-`FunctionPointer`
под Unity + managed-делегаты всегда), адресуемую плотным ordinal'ом `TypeId<ILogicNode>`, и отдаёт по
индексу. Это и есть «кеш `NodeInvoker`» (компиляция fn-pointer'а — один раз/тип, не на каждый run).

**Дисптача в run'е ещё нет** (это M6-F): здесь — только реестр (хранилище + by-index lookup + startup-сборка).
**Выбора бэкенда по `runtimeType` нет** (M6-D). **Version gate нет** (M6-E).

## Контекст (что substrate уже даёт)

- `NodeInvoker.Execute<T>` (монолитный адаптер) + `Compile<T>()` (Burst fn-pointer, `#if UNITY`) +
  `GetManaged<T>()` (managed-делегат) — **готовы** (M6-B). Нужна таблица, которая вызовет их по каждому типу.
- `ILogicNode : IIndexedType` — **индексируемый контекст**: генератор `TypeIndexer` уже строит **плотных
  детей** контекста. `IndexedTypes.GetContextChildren(typeof(ILogicNode))` → `TypeId[]`, **упорядоченный по
  `TypeId<ILogicNode>` ordinal** (index массива == dense ordinal == `TypeIdOf<ILogicNode,T>.typeId`);
  `IndexedTypes.GetType(typeId)` → `Type`; `TypeId<ILogicNode>.Count` → число типов. ⇒ **реестр не сканит
  AppDomain сам — он переиспользует уже построенный генератором плотный список детей** (гарантирует
  совпадение ordinal'а реестра с тем, что бейкается в блоб как `NodeHeader.typeId`).
- Прецедент `IndexedTypes.Initialize(...)` (`TypeIndexer/IndexedTypes.cs:54`): публичный `Initialize`,
  принимающий **уже построенные** данные; генератор эмитит initializer, который их собирает. M6-C повторяет
  форму: `Initialize()` (reflection-startup, интерим до кодогена M10) **плюс** `Initialize(таблица)` (mirror —
  приём готовой таблицы; используется тестами и будущим генератором).

## Решения по развилкам (на гейт)

| # | Развилка | Решение |
|---|---|---|
| 3 | **Кто строит function-table** | `NodeFunctionRegistry.Initialize()` — startup, **один раз/кеш** (idempotent-гард). Источник типов — **плотные дети `ILogicNode` из `IndexedTypes`** (`GetContextChildren`), не сырой AppDomain-скан: ordinal реестра == `TypeIdOf<ILogicNode,T>` автоматически. Согласуется с M10 (initializer станет генерируемым). |
| 4 | **Хранилище реестра** | **Инстанс** (`struct : IDisposable`), не статика/`SharedStatic` (тот капризен в эдиторе). Поля: `ExecuteFn[] _managed` (managed-путь) + `UnsafeArray<FunctionPointer<ExecuteFn>> _burst` (off-allocator, под `#if UNITY` — передаётся в Burst). Индекс = `TypeId<ILogicNode>` ordinal. Строится раз, шарится копией, диспозится владельцем. |

## Решаемая на гейте развилка C/D (важно)

**Где строится managed-таблица — в C или D?** Дословная разбивка пишет «M6-C — Burst registry», «M6-D —
параллельная managed-таблица». Но **развилка 3 (закреплена за C)** прямо гласит: *«managed-таблица строится
всегда (универсальный путь, единственный в .NET)»*. **Рекомендация (этот план): строить ОБЕ таблицы в C**
(managed всегда + Burst под Unity), потому что:
1. **Кросс-среда** (директива гейта M6): весь M6-код компилируется и в Unity, и в plain .NET; Burst-only-реестр
   в .NET — пустой no-op класс, бессмысленный.
2. **Тестируемость:** managed-таблица даёт **реальный** round-trip в EditMode/plain .NET (без Burst/IndexedTypes —
   через mirror-`Initialize(таблица)` со stub-нодами), а не сплошной `Assert.Ignore`.

Тогда **M6-D** остаётся когерентным: **выбор бэкенда** по `NodeHeader.runtimeType` (Managed-нода → Burst не
берём), **глобальный managed-форс**, **детерминизм Burst↔.NET**, **реальные .NET-тесты исполнения**. То есть
C даёт *населённую таблицу + lookup по индексу*; D даёт *кто из таблицы что выбирает + доказательство
детерминизма*. **Полагание таблиц ≠ их потребление** — потребление в M6-F, так что половин-состояния нет.

**Население managed-таблицы — опционально (директива 2026-06-16).** `Initialize(bool buildManaged = true)`:
под Unity `_burst` строится **всегда** (все типы — в C они все unmanaged), а `_managed` — **только если
`buildManaged`** (в проде можно пропустить, не дублируя unmanaged-ноды, уже покрытые Burst; тесты/.NET зовут
с `true` ⇒ managed round-trip прогоняется без Burst). Дефолт `true` — корректность вперёд (managed-путь
доступен), оптимизация-пропуск — opt-in. В чистом .NET Burst-ветка вырезана `#if` ⇒ managed строится всегда
(единственный путь); при `buildManaged:false` в .NET реестр осознанно пуст (документируем).

## Burst-компиляция всех типов в C (интерим)

Реестр в C **Burst-компилит `Compile<T>()` для каждого** типа `ILogicNode`. Сейчас это безопасно: **нод с
managed-телом ещё нет** (`RuntimeType.Managed` как способность вводится/используется в M6-D). M6-D добавит
**пропуск Burst-компиляции** для типов, помеченных Managed (иначе `CompileFunctionPointer` на managed-теле
упадёт). Явно фиксируем как осознанный интерим: *C компилит Burst для всех (managed-тел нет), D добавляет skip
+ выбор*. (`runtimeType` живёт per-node в блобе, не на logic-типе ⇒ выбор — это рантайм-решение диспетчера M6-D/F,
а не свойство реестра.)

## Публичный API (эскиз) — обновлено по ревью

> **Правки на ревью (2026-06-16):**
> - (a) **Инстанс, а не статика/`SharedStatic`.** `SharedStatic` капризен в эдиторе (domain reload / межсессионное
>   состояние) ⇒ реестр — обычный инстанс (`struct : IDisposable`), строится один раз и **передаётся в исполнение**
>   (в `ExecutionScope` — на M6-F). Кэш = сам инстанс (caller строит раз и шарит копией). Статик-протечки нет.
> - (b) **Burst-таблица — `UnsafeArray<FunctionPointer<ExecuteFn>>`** (off-allocator unmanaged) внутри инстанса:
>   передаётся в Burst как аргумент/в контексте (managed-массив Burst не видит). Managed-таблица — `ExecuteFn[]`
>   (путь .NET, Burst не трогает).
> - (c) **`Count` убран** (избыточен — размер есть как `TypeId<ILogicNode>.Count`/`.Length`).
> - (d) **`Dispose()` освобождает** off-allocator Burst-таблицу (`FunctionPointer` per-se диспозить не надо —
>   нативный код держит Burst весь процесс). Вызывает владелец.

```csharp
namespace Sapientia.LogicGraph
{
    // Function-table нод по индексу TypeId<ILogicNode>. Инстанс: строится раз, передаётся в исполнение (→ scope, M6-F).
    public struct NodeFunctionRegistry : IDisposable
    {
        private ExecuteFn[] _managed;                                  // managed-путь (.NET/fallback); Burst НЕ трогает
#if UNITY_5_3_OR_NEWER
        private UnsafeArray<FunctionPointer<ExecuteFn>> _burst;        // off-allocator ⇒ передаётся/читается из Burst
#endif
        public readonly bool IsCreated { get; }

        // Сборка по плотным детям ILogicNode из IndexedTypes (интерим до M10). Один раз; результат шарится копией.
        // buildManaged: под Unity заполнять ли managed-таблицу (false → только Burst, без дублей unmanaged).
        public static NodeFunctionRegistry Build(bool buildManaged = true);

        // Из уже построенной таблицы (тесты + будущий генератор). Реестр владеет копией Burst-таблицы.
        public static NodeFunctionRegistry Create(ExecuteFn[] managed
#if UNITY_5_3_OR_NEWER
            , FunctionPointer<ExecuteFn>[] burst = null
#endif
        );

        public readonly ExecuteFn GetManaged(int ordinal);                // managed-путь (всегда)
#if UNITY_5_3_OR_NEWER
        public readonly FunctionPointer<ExecuteFn> GetBurst(int ordinal);  // Burst-путь: _burst[ordinal]
        public readonly UnsafeArray<FunctionPointer<ExecuteFn>> BurstTable { get; } // целиком → в Burst-диспетчер (M6-F)
#endif

        public void Dispose();   // освобождает off-allocator Burst-таблицу (вызывает владелец)
    }
}
```

> **Интеграция в scope** (передача инстанса в `ExecutionScope` + прогон через диспетчер) — **M6-F**. M6-C отдаёт
> инстанс standalone; scope его не хранит и не владеет (как `CompiledBlueprintStorage` «передаётся, не хранит»).

**Сборка (`Initialize(buildManaged)`):**
```
var children = IndexedTypes.GetContextChildren(typeof(ILogicNode)); // TypeId[], index == dense ordinal
#if UNITY   _burst = new FunctionPointer<ExecuteFn>[children.Length];  #endif
// managed: под Unity — по флагу; в .NET — всегда (Burst вырезан #if)
bool fillManaged = buildManaged; // в .NET де-факто единственный путь
_managed = (fillManaged) ? new ExecuteFn[children.Length] : Array.Empty<ExecuteFn>();
for (ordinal = 0..children.Length):
    Type t = IndexedTypes.GetType(children[ordinal]);
#if UNITY _burst[ordinal] = (FunctionPointer<ExecuteFn>) Compile<t>()   // всегда (все unmanaged в C) #endif
    if (fillManaged) _managed[ordinal] = (ExecuteFn) GetManaged<t>()    // через MakeGenericMethod
_initialized = true;
```
> `Count` отдаёт размер таблицы по `children.Length` (хранить отдельным полем — не привязываться к `_managed.Length`,
> который при `buildManaged:false` под Unity пуст, тогда как `_burst` полон).

## Файлы

| Файл | Что |
|---|---|
| `Logic/RuntimeData/Execution/NodeFunctionRegistry.cs` | **new** — реестр (статические таблицы + `Initialize()`/`Initialize(таблица)`/`GetManaged`/`GetBurst`/`Reset`) |
| `Tests/NodeFunctionRegistryTests.cs` | **new** — round-trip реестра (mirror-инъекция) + гарды (double-init, Count, Reset) + Assert.Ignore-round-trip через `Initialize()` |

## Тесты

| Тест | Что проверяет | Среда |
|---|---|---|
| `Registry_InjectedManagedRoundTrips` | `Create(managed[])` с делегатами `GetManaged<StubLogicAdd>`/`<StubLogicNeg>` по ordinal'ам 0/1 → `GetManaged(0)`/`GetManaged(1)` возвращают и **исполняют** нужный адаптер над ручным `NodeContext` (5+100=105 / −7) | plain managed (реально) |
| `Registry_InjectedTableIsRetrievable` | после `Create` — `IsCreated`, `GetManaged(0/1)` != null | managed |
| `Registry_DisposeReleases` | `Dispose()` → `IsCreated==false`; повторный Dispose не падает (идемпотентно, освобождение off-allocator) | managed |
| `Registry_BuildEmptyWhenNoTypesRegistered` | `Build()` при пустом `IndexedTypes` (EditMode) → созданный, но пустой реестр, без падений | managed |
| `Registry_BuildBuildsDenseTable` | `Build()` (reflection) строит таблицу размера `TypeId<ILogicNode>.Count`, `GetManaged(ordinal)` != null | **`Assert.Ignore`** если `Count==0` (EditMode: IndexedTypes не init) |

> Burst-`FunctionPointer` и `IndexedTypes`-инициализация в EditMode недоступны ⇒ Burst-ветка и
> reflection-`Build` проверяются под `Assert.Ignore`/инспекцией (как 4E/4F-2/M6-A). Managed-таблица через
> mirror-`Create` даёт реальное покрытие хранилища+lookup+диспатча-по-индексу. Реестр — инстанс ⇒ каждый тест
> строит+диспозит локально (статик-протечки нет, `[TearDown]`/`Reset` не нужны).

## Задачи

| # | Задача | Статус |
|---|---|---|
| [01](tasks/01-registry.md) | `NodeFunctionRegistry`: статические таблицы + by-index lookup + mirror-`Initialize(таблица)` + `Reset` | ✅ done |
| [02](tasks/02-startup-init.md) | `Initialize()`: reflection-startup по детям `ILogicNode` из `IndexedTypes` (`MakeGenericMethod` `GetManaged`/`Compile`), once/кеш | ✅ done |
| [03](tasks/03-tests.md) | `NodeFunctionRegistryTests`: round-trip (инъекция) + гарды + Assert.Ignore-startup | ✅ done |

## Non-goals (последующие под-фазы / вехи)

- **Выбор бэкенда** по `NodeHeader.runtimeType` (Managed → без Burst), **глобальный managed-форс**,
  **детерминизм Burst↔.NET**, реальные .NET-тесты исполнения — **M6-D**.
- **Version gate** (хеш контракта нод-функций, reject несовместимого блоба) — **M6-E**.
- **Прогон `ExecutionGraph.Drain` через диспетчер** (`NodeInvoker.Invoke(ref scope, ref compiled, NodeInstanceId)`) —
  **M6-F**.
- **Кодоген initializer'а** (как у `TypeIndexer`, вместо reflection-startup) — **M10**.
- Ambient-context Burst-резолв — **M7**.

## Риски

- **IL2CPP/AOT + `MakeGenericMethod`.** Reflection-инстанцирование generic-методов (`GetManaged<T>`/`Compile<T>`)
  на типах, нигде статически не упомянутых, под IL2CPP может быть вырезано/упасть. Это **интерим** — кодоген M10
  (генерируемый initializer, как `TypeIndexer`) снимает риск. В Mono-EditMode/plain .NET reflection работает.
  Фиксируем в пакете как известное ограничение.
- **Ordinal-контракт.** Таблица обязана быть индексирована **тем же** ordinal'ом, что бейкается в
  `NodeHeader.typeId` (`TypeIdOf<ILogicNode,T>`). Источник типов — `GetContextChildren` (упорядочен по этому
  ordinal'у) ⇒ совпадение гарантировано конструктивно; проверить инспекцией + комментарием-инвариантом.
- **Статика process-wide.** Реестр статичен (не per-world) — как `IndexedTypes`. `Reset()` для тест-изоляции
  обязателен (иначе протечка состояния между тестами).

## Верификация

Компиляция-инспекция + adversarial-сабагент по working-tree diff + (при необходимости) изолированный
`dotnet build` минимального репро. Batchmode-раннер не гоняем (segfault в EDM4U — окружение).
