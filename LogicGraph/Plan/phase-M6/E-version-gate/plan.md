# M6-E — Version gate

> **Статус: ✅ done (одобрено 2026-06-17).** Под-фаза вехи M6 ([../README.md](../README.md), развилка 7).
> Источник правды — код, затем [../README.md](../README.md) / [../../STATE.md](../../STATE.md).
> Предыдущее: M6-A (dispatch-index = `TypeId<ILogicNode>` ordinal), M6-B (контракт `NodeContext`/`Execute`),
> M6-C (`NodeFunctionRegistry` — function-table по ordinal), M6-D (выбор бэкенда + managed-исполнение).
> M6-E вводит **version gate**: блоб несёт «версию кода» (хеш контракта нод-функций) и **отклоняется**, если
> она не совпадает с локальной. Прогон `Drain` через диспетчер — **M6-F**.

## Цель

Блоб (`CompiledBlueprintHeader`) бейкает per-node `NodeHeader.typeId` = **локальный ordinal** `TypeId<ILogicNode>`.
Этот ordinal **позиционный**: он валиден только если набор/порядок детей `ILogicNode` в `IndexedTypes` на
клиенте **тот же**, что был при компиляции блоба. Если набор разошёлся (нода добавлена/удалена/переставлена в
function-table), забейканный ordinal укажет на **чужую** функцию ⇒ тихая порча (misdispatch). Version gate это
**детектирует и отклоняет** (CLAUDE.md §7: «version mismatch must be detected and rejected, **not executed**»).

«Версия кода» — детерминированный **авто-хеш контракта нод-функций**, **инвариантный к набору/количеству/порядку
блюпринтов и нод** (id блюпринтов рантайм-генерятся и адресуются отдельно; блоб самодостаточен по static-данным).
Хеш бейкается в каждый блоб на компиляции и сверяется при `CreateInstance`.

## Решения по развилке 7 (финализация — на гейт)

fork 7 оставил три вопроса открытыми («что входит в хеш, per-node по стабильному id vs глобально, где сверяется»).
Предлагаемые решения:

| # | Вопрос | Решение | Обоснование |
|---|---|---|---|
| 7.1 | **Что входит в хеш** *(уточнено пользователем 2026-06-17: + тело + данные)* | `FormatVersion` (ручная ABI-версия) **+** на каждый дочерний `ILogicNode` (в порядке ordinal'а) **три слоя**: `Type.FullName` (**структурный** — кто на какой позиции), **IL тела `Execute`** (**поведенческий** — что нода делает), **раскладка данных** (поля+размер — **слой данных**: static-слайс реинтерпретируется как `T`). | Имя ловит set/order/rename (misdispatch ordinal'а), IL-тело — смену поведения без переименования, раскладка — смену полей/размера (тихая порча данных, т.к. `Body<T>` кастует слайс на `T`). Ограничения IL+layout (токены/Debug-Release/Burst-native/IL2CPP-стрип/нет транзитивности) **приняты осознанно**; робастная замена — content-digest из M10-кодогена. |
| 7.2 | **`sizeof(NodeContext)` в хеш?** | **Нет.** Раскладку `NodeContext`/ABI представляет **ручная** `FormatVersion` (бампится при изменении контракта исполнения). | `sizeof(NodeContext)` зависит от размера указателя (32/64-бит) ⇒ ложные reject при server(x64)→client(arm64, оба 64) не страшны, но кросс-битность сломала бы. «Версия кода» должна быть **source-level**, не runtime-layout. |
| 7.3 | **Где живёт хеш** *(уточнено пользователем 2026-06-17)* | **В окружении группы** (`CompiledEnvironment`), **не** в блобе и **не** per-node. `CompiledBlueprintHeader` о хеше **ничего не знает**. Хеш — **верхнеуровневая сущность контроля версий группы блюпринтов**: один на группу. | Группа блюпринтов компилируется под одно окружение (одну версию function-table); per-blob/per-node бейк лишний — версию вычитывает группа. Блоб остаётся чистым (только static-данные). |
| 7.4 | **Где сверяется + откуда версия** *(уточнено пользователем 2026-06-17)* | **«Версия кода» в рантайме НЕ считается рефлексией** — она **загружается из конфига окружения** (`CompiledEnvironment`), скомпилированного заранее вместе с группой блюпринтов. **`CompiledBlueprintStorage` рождается со своим окружением** (`Create(CompiledEnvironment, …)`); **`Add` принимает окружение добавляемой группы** и сверяет его со своим (`environment.IsCompatibleWith(_environment)`): рассинхрон → арена освобождается + **реальный `throw`** (не `E.ASSERT`: load-bearing). `ExecutionScope` гейта **не несёт**. | Группа всегда имеет окружение ⇒ проверка на ingest (fail-fast, до любого инстанса), на уровне группы (а не блоба). Рантайм без рефлексии (IL2CPP/AOT-риск только build-time при `Compile()`). `NodeContractHash.Local` — **build/bake-only**. |

## Хеш-функция

**FNV-1a 64-бит** (фикс-константы) — детерминирован в пределах сборки, без `Random`/wall-clock. **Запрещён
`string.GetHashCode()`** (рандомизирован per-process). На каждый тип фолдим: имя (`FullName` по символам) +
**IL тела `Execute`** (`MethodBody.GetILAsByteArray()`, резолв через `GetInterfaceMap`; недоступно → стабильный
маркер, не падаем). Считается **только на build/managed-стороне** (сборка окружения `CompiledEnvironment.Compile`),
никогда на Burst-горячем пути ⇒ reflection/IL допустимы. **Ограничения IL** задокументированы в коде (см. 7.1).

## Файлы

| Файл | Что |
|---|---|
| `Logic/RuntimeData/Execution/NodeContractHash.cs` | **new** — `FormatVersion` (const), FNV-1a `Compute(params Type[])` (ядро) + `Local` (хеш function-table из `IndexedTypes`, **build/bake-only**). |
| `Logic/StaticData/CompiledEnvironment.cs` | **new** — носитель версии **группы**: `ulong contractHash` + `Compile()` (build-time, фиксирует `Local`) + ctor от значения (рантайм-load) + `IsCompatibleWith(in CompiledEnvironment)` (предикат гейта). |
| `Logic/StaticData/CompiledBlueprintStorage.cs` | `Create(CompiledEnvironment, …)` (рождается со своим окружением) + `_environment` + `Environment`; **`Add(arena, offsets, environment)`** — сверка окружения группы со своим, рассинхрон → `arena.Dispose()` + `throw`. |
| `Tests/VersionGateTests.cs` | **new** — хеш (детерм./порядок/набор/пустой), предикат окружения, сборка окружения, accept/reject на `Add`, happy-path `CreateInstance`. |
| `Tests/CompiledBlueprintStorageTests.cs` | `Add`-хелпер → `storage.Add(arena, offset, CompiledEnvironment.Compile())`; 5 `Create()` → `Create(CompiledEnvironment.Compile())`. |

> **Не трогаются:** `CompiledBlueprintHeader` (блоб **о хеше не знает**) и `BlueprintCompiler` (бейка хеша нет). `ExecutionScope` гейта не несёт.

## Публичный API (эскиз)

```csharp
// NodeContractHash.cs (build/bake-only)
public static class NodeContractHash
{
    public const ulong FormatVersion = 1;                  // ручная ABI-версия (ExecuteFn/NodeContext)
    public static ulong Compute(params Type[] orderedLogicTypes); // ядро, index == ordinal
    public static ulong Local { get; }                     // хеш текущей function-table; EditMode → seed пустой таблицы (!= 0)
}

// CompiledEnvironment.cs — версия ГРУППЫ; компилируется заранее + грузится в рантайме
public struct CompiledEnvironment
{
    public ulong contractHash;
    public CompiledEnvironment(ulong contractHash);                     // рантайм-load из конфига
    public static CompiledEnvironment Compile();                        // build-time: фиксирует NodeContractHash.Local
    public readonly bool IsCompatibleWith(in CompiledEnvironment other); // равенство версий — предикат гейта
}

// CompiledBlueprintHeader.cs — НЕ меняется (блоб о хеше не знает).

// CompiledBlueprintStorage.cs — рождается со своим окружением; гейт в Add (окружение группы vs своё)
public static CompiledBlueprintStorage Create(CompiledEnvironment environment, int blueprintCapacity = 8);
public CompiledEnvironment Environment { get; }
public void Add(RawBumpAllocator arena, PtrOffset<CompiledBlueprintHeader> offset, CompiledEnvironment environment);
public void Add(RawBumpAllocator arena, Span<PtrOffset<CompiledBlueprintHeader>> offsets, CompiledEnvironment environment); // рассинхрон → Dispose+throw
```

## Execution steps

1. `NodeContractHash` (FNV-1a, `FormatVersion`, `Compute`, `Local` build-only) + его тесты (детерминизм/
   чувствительность — чисто, без `IndexedTypes`).
2. Бейк `contractHash` в блоб (`CompiledBlueprintHeader` поле + `IsContractCompatible`; `BlueprintCompiler`
   присваивание) + `CompiledEnvironment` (носитель + `Compile()`) + тесты бейка/сборки окружения.
3. Гейт в `CompiledBlueprintStorage.Add` (сторедж рождается с окружением; несовместимый блоб → `Dispose`+`throw`);
   обновить call-site'ы существующих тестов; accept/reject тесты на `Add` + happy-path `CreateInstance`.
4. Self-review (Step 4): lockstep не тронут, throw переживает release, нет утечки/двойного-free арены на reject,
   детерминизм; adversarial-сабагент.

## Task index

| # | Задача | Статус |
|---|---|---|
| [01](tasks/01-contract-hash.md) | `NodeContractHash` (FNV-1a + `FormatVersion` + `Compute`/`Local` build-only) | ✅ done |
| [02](tasks/02-bake-and-gate.md) | Бейк `contractHash` в блоб + предикат + `CompiledEnvironment` (носитель + `Compile`) | ✅ done |
| [03](tasks/03-tests.md) | Гейт в `CompiledBlueprintStorage.Add` (рождение с окружением) + `VersionGateTests` + апдейт call-site'ов | ✅ done |

## Тесты

| Тест | Что проверяет | Среда |
|---|---|---|
| `Hash_IsDeterministic` | `Compute(A,B)` дважды → равно (гард против `string.GetHashCode`-рандомизации) | чисто |
| `Hash_OrderSensitive` | `Compute(A,B) != Compute(B,A)` (ordinal позиционен) | чисто |
| `Hash_SetSensitive` | `Compute(A) != Compute(A,B)` (добавление типа меняет версию) | чисто |
| `Hash_EmptyIsStableNonZero` | `Compute()` детерминирован и `!= 0` (seed от `FormatVersion`) | чисто |
| `Body_DifferentBodies_HaveDifferentIL` | две `ILogicNode` с разным `Execute` ⇒ IL различается (механизм читает тело) | реально |
| `Hash_BodySensitive` | разные тела `Execute` ⇒ разный хеш (поведенческий слой) | реально |
| `Hash_BodyDeterministic` | повтор `Compute` с телом ⇒ равно (IL детерминирован в пределах сборки) | реально |
| `Hash_DataLayoutSensitive` | одинаковый `Execute`, разные поля ⇒ разный хеш (слой данных: раскладка static-слайса) | реально |
| `Environment_Compile_CapturesLocal` | `CompiledEnvironment.Compile().contractHash == NodeContractHash.Local` | реально |
| `Environment_IsCompatibleWith_SameVersion` | равные версии ⇒ `IsCompatibleWith` == true | чисто |
| `Environment_IsCompatibleWith_DifferentVersion` | разные версии ⇒ `IsCompatibleWith` == false | чисто |
| `Storage_CompatibleGroup_Added` | сторедж + группа с одним окружением (`Compile()`) ⇒ `Has(key)`, `Count==1` (принята) | реально |
| `Storage_StaleGroup_Rejected` | группа с **другой** версией (`Local+1`) ⇒ `Add` **бросает** `InvalidOperationException`; `Count==0`, `!Has` (арена освобождена, нет полу-состояния) | реально |
| `CreateInstance_FromCompatibleStorage_Works` | совместимый сторедж ⇒ `CreateInstance` даёт **валидный** хендл (happy-path, ранее не покрыт) | реально |

> Хеш/предикат/окружение/accept/reject покрыты **реально** в EditMode (managed-семантика, `IndexedTypes` не
> нужен: сторедж и группа читают одну — пустую в EditMode — таблицу через `Compile()` ⇒ совпадают; reject
> делается окружением группы с другой версией). Под `Assert.Ignore` ничего нет — первая под-фаза M6 без отложенных проверок.

## Non-goals (последующие под-фазы / вехи)

- **Прогон `ExecutionGraph.Drain` через диспетчер** (`NodeInvoker.Invoke(ref scope, ref compiled, NodeInstanceId)`) +
  интеграция реестра в `ExecutionScope` — **M6-F**.
- **Load-time гейт** (сверка при приёме блоба в `CompiledBlueprintStorage.Add` / в transfer-слое) — **M11**
  (identity/authoring + server→client). M6-E ставит переиспользуемый предикат, точка применения — `CreateInstance`.
- **Резолв guid→рантайм-id блюпринтов** (Content-подобный) — **M11**; к диспатчу/гейту нод не относится.
- **Кодоген initializer'а function-table** (вместо reflection в `Build`/`Local`) — **M10**.

## Риски

- **`string.GetHashCode` недетерминирован** → используем собственный FNV-1a (явные const). Зафиксировано тестом
  `Hash_IsDeterministic` + комментарием-инвариантом в коде.
- **`FormatVersion` ручная** — её обязан бампить автор при изменении ABI `ExecuteFn`/`NodeContext` (хеш типов это
  не поймает). Документируем XML-комментарием у константы; кодоген M10 может автоматизировать.
- **IL2CPP/AOT (как M6-C/D)** — `Local` ходит в `IndexedTypes`/reflection; в редакторе/.NET работает, под IL2CPP —
  генерируемый initializer (**M10**). Не блокер верификации (редактор/.NET).
- **Throw в `CreateInstance`** — метод managed-сторонний (не Burst), throw легален и **переживает release**
  (намеренно не `E.ASSERT`). Если позже `CreateInstance` уйдёт под Burst — гейт вынести в managed-обёртку.
- **`ExecutionScope.Create` теперь читает `IndexedTypes`** (раз/scope, дёшево, в EditMode — пустая таблица). На
  горячий путь не попадает.

## Верификация

Как M6-A…D: компиляция-инспекция + adversarial-сабагент по working-tree diff + (при необходимости) изолированный
`dotnet build` минимального репро. Batchmode-раннер не гоняем (segfault в EDM4U — окружение). **Особенность M6-E:**
весь гейт (хеш/предикат/бейк/accept/reject) исполняется в plain managed-семантике ⇒ покрыт **реальными** unit-тестами
без Burst/Unity, без `Assert.Ignore`.
