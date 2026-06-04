# Фаза 2 — Five-scope layout

**Статус фазы: ✅ done (2026-06-05) — реализация готова, тесты 20/20 зелёные, approval получен, закоммичено в сабмодуле (Step 8 выполнен).**

> Источник цели и scope — [../../PLAN.md → Phase 2](../../PLAN.md). Дизайн-инварианты —
> [../../CLAUDE.md §4](../../CLAUDE.md). Этот документ — живое состояние фазы.

---

## ⚠️ ТЕКУЩЕЕ СОСТОЯНИЕ (handoff для нового чата — читать первым)

**Где мы в workflow `/logicgraph-phase 2`:** Steps 1–5 пройдены (план + gate ACK + реализация +
self-review + Review Packet выдан). Идёт **Step 6–7** (ревью пользователя + post-review правки) —
пользователь прогнал серию правок по неймингу/дизайну. **Step 8 (close: обновить CLAUDE.md status-map,
PLAN.md → ✅, коммит в сабмодуле) НЕ выполнен. Изменения лежат в рабочем дереве сабмодуля без коммита.**

**НЕ нужно перепланировать с нуля.** Вся реализация существует (4 таски ✅). Возобновляй с верификации
ниже, затем закрытие.

### ✅ Тесты перепрогнаны (2026-06-04) — 20/20 зелёные

Переписанный код (self-relative `RelativePtr`/`BumpArray`, `DataSizes.Alignment`, `.ToInt()`/`.ToEnum()`,
explicit-конверсии) **перепрогнан в batchmode: 20/20 passed** (13 Phase-2 + 7 Phase 0/1),
`Logs/lg.xml` → `result="Passed" total="20" failed="0"`. Adversarial-review диффа: блокеров/мажоров нет.
Осталось только: финальный approval пользователя → Step 8 (close + commit). История правок ниже —
для контекста.

Что было переписано после прошлого зелёного прогона (всё теперь покрыто зелёным re-run):
1. `BumpArray<T>` переписан на **self-relative** через новый `RelativePtr<T>` (поле `allocatorOffset`
   убрано; аллокация — **in-place метод `Alloc(ref BumpHeader, int)`**, не конструктор; добавлен `GetSpan()`).
2. Новый foundation-тип **`RelativePtr` / `RelativePtr<T>`** (`Data/RelativePtr.cs`) — self-relative
   указатель (offset от адреса своего поля). Заменил прежний путь `PtrOffset.GetValue` и
   `BumpHeaderExt.GetRelativeAllocator` (тот метод **удалён**).
3. `DataSizes`: константа выравнивания переехала сюда (`DataSizes.Alignment = 8`); добавлены
   `GetAligned()`/`GetAligned(int)` и `operator +`. Цикл-ровер в `SetupLayout` заменён на `foreach` +
   `blockSizes += node.DataSizes.GetAligned()`.
4. Enum-касты переведены на `.ToInt()` / `.ToEnum<DataLayout>()`.
5. Кросс-семейные конверсии `PtrOffset ↔ RelativePtr` сделаны **explicit** (same-family
   `RelativePtr ↔ RelativePtr<T>` остались implicit).

**Команда прогона** (редактор должен быть закрыт, ~80с):
```sh
"/Applications/Unity/Hub/Editor/6000.0.60f1/Unity.app/Contents/MacOS/Unity" \
  -runTests -batchmode -projectPath "$(pwd)" -testPlatform PlayMode \
  -assemblyNames Sapientia.LogicGraph.Tests \
  -testResults "$(pwd)/Logs/lg.xml" -logFile "$(pwd)/Logs/lg.log"
```
Ожидаемо **20/20** (13 Phase-2 + 7 Phase 0/1). Если красно — чинить до зелёного перед Step 8.
Особое внимание: self-relative резолв `RelativePtr` (DEBUG bounds) и lockstep после перехода на
`foreach`/`+=`.

### Что осталось до закрытия фазы
1. Перепрогнать тесты → зелёные.
2. Получить финальный approval пользователя на дифф.
3. **Step 8:** CLAUDE.md design→code status-map (static + instance cache/persistent → ◐/✅), PLAN.md
   Phase 2 → ✅ + чекбокс; коммит **внутри сабмодуля** `Assets/Submodules/Sapientia`. **Branch-check:**
   сабмодуль сейчас в detached HEAD на типе `rnd/nodes_graph` — перед коммитом встать на ветку
   `rnd/nodes_graph` (не коммитить в detached/main). Сообщение: `LogicGraph: Phase 2 — Five-scope layout`
   + trailer `Co-Authored-By: Claude Opus 4.8`. Только commit, без push.

### Открытые мелочи (на усмотрение пользователя, не блокеры)
- `BumpArray.Alloc` должен вызываться на **уже размещённой в арене** структуре (`ref` поля), т.к.
  self-relative `offset` считается от финального адреса. На временной (`new` + копирование) сломается —
  поэтому конструктора-аллокатора нет, только `Alloc`. Можно при желании спрятать за хелпер на
  `BumpHeader`, чтобы шаг нельзя было забыть.
- Legacy-портовые пары `offset+count` (`nodeHeaders`/`edgeToData`/…) можно позже перевести на
  `BumpArray` — но это M9 (когда оживёт портовый путь), не сейчас.

---

## Цель

Представить все **5 областей данных** (`static`, `static cache`, `static persistent`,
`instance cache`, `instance persistent`) с **фиксированным на этапе компиляции размером**,
объявленным на каждой ноде, и разложить их по блокам в скомпилированном блюпринте и инстансе.
Только разметка памяти (sizing + layout) — без модели портов/рёбер, без исполнения, без владельца
scope-блоков (это Фазы 3–5).

## Ключевое архитектурное решение (gate ACK)

Существующий путь компиляции (`CompiledBlueprint.Compile` → `SetupBlueprint`) завязан на незаконченную
модель портов (`INode.GetInputs()` throws; `BumpHeader.MemAlloc` ассертит `size > 0`). Поэтому Фаза 2
добавляет **отдельный, независимый от портов путь разметки** в `CompiledBlueprint`
(`CalculateLayoutSizeToReserve` / `CompileLayout` / `SetupLayout`), **не трогая** legacy-методы — они
остаются заготовкой под M9. Тесты гоняют только новый layout-путь на stub-нодах. Сосуществование
legacy-портового и нового layout до M9 — согласовано на gate.

## Маппинг 5 областей на код

| Область | Владелец (модель) | Где в Фазе 2 |
|---|---|---|
| `static` | blueprint manager (Фаза 4) | блок в `CompiledBlueprint` (raw-арена), реально аллоцируется здесь |
| `static cache` | scope (Фаза 3) | только **размер блока + пер-нодовые офсеты** в `CompiledBlueprint`; блок НЕ аллоцируется |
| `static persistent` | scope (Фаза 3) | только размер блока + офсеты; блок НЕ аллоцируется |
| `instance cache` | instance | блок в `BlueprintInstance` (worldState), аллоцируется + ресетится здесь |
| `instance persistent` | instance | блок в `BlueprintInstance` (worldState), аллоцируется здесь |

## Файлы (актуально)

**Новые**
- `Data/RelativePtr.cs` — **foundation-тип** `RelativePtr` / `RelativePtr<T>`: self-relative указатель,
  хранит offset от адреса **своего поля** до цели (`GetPtr() = &this + byteOffset`, `SetPtr(p) = p - &this`,
  `GetValue<T>()`). Резолвится без внешней базы. Конверсии `PtrOffset ↔ RelativePtr` — **explicit**
  (меняют смысл offset), `RelativePtr ↔ RelativePtr<T>` — implicit.
- `Memory/BumpAllocator/BumpArray.cs` — `struct BumpArray<T>`: `RelativePtr<T> offset` + `int length`.
  `Alloc(ref BumpHeader, int)` (**in-place**, self-relative), `GetPtr()`, `Get(int)`, `GetSpan()`,
  `IsValid`/`Length`. Память поэлементно не освобождает.
- `Blueprint/DataLayout.cs` — `enum DataLayout` (5); `struct DataSizes` (`Count=5`, `Alignment=8`, ctor,
  индексаторы `[DataLayout]`/`[int]`, `GetAligned(DataLayout)`/`GetAligned(int)`/`GetAligned()`,
  `operator +`); `struct NodeLayoutOffsets` (индексаторы `[DataLayout]`/`[int]`).
- `Tests/StubNode.cs` — `INode`-заглушка: только `DataSizes`, портовые методы пустые.
- `Tests/LayoutTests.cs`, `Tests/InstanceScopeTests.cs`, `Tests/BumpArrayTests.cs`.

**Изменяемые**
- `Blueprint/INode.cs` — дефолтный член `DataSizes DataSizes => default;`. Legacy не тронут.
- `Logic/StaticData/CompiledBlueprint.cs` — `allocatorOffset` теперь `RelativePtr<BumpHeader>`; секция
  Phase 2: `blockSizes` (`DataSizes`), `nodeLayoutOffsets` (`BumpArray<NodeLayoutOffsets>`), `staticBlock`;
  `CalculateLayoutSizeToReserve`/`CompileLayout`/`SetupLayout`/`GetBlockSize`/`GetNodeOffset`/
  `GetStaticNodeSlice`. `SetupLayout` зовёт `nodeLayoutOffsets.Alloc(...)` + `foreach`/`+=`. Legacy
  `Compile`/`SetupBlueprint` теперь тоже идут через `SetupRelativePtr`/`GetValue` (т.к. `allocatorOffset`
  стал `RelativePtr`).
- `Memory/BumpAllocator/BumpHeader.cs` — `UsedBytes` (lockstep-тест); `SetupRelativePtr(ref RelativePtr<BumpHeader>)`;
  `MemAlloc<T>(int count, out PtrOffset<T>)`; `BumpHeaderExt` расширяет `ref RelativePtr<BumpHeader>`.
  **`GetRelativeAllocator` удалён** (заменён `RelativePtr.GetValue`).
- `Logic/BlueprintInstance.cs` — `instanceCache`/`instancePersistent` (`CachedPtr`); `Create`/`ResetCache`/`Dispose`.
- `Logic/StaticData/BlueprintCompiler.cs` — `_allocatorRef` стал `RelativePtr<BumpHeader>`; `SetupRelativePtr`.
- `Logic/ILogicNode.cs` — `allocatorOffset.GetValue()`.
- `Tests/README.md` — список тестов.

## Раскладка / выравнивание

- Слот ноды в блоке области = `node.DataSizes.GetAligned(scope)` = `AlignUp(size, DataSizes.Alignment)`.
- Офсет ноды = префиксная сумма слотов (в `SetupLayout` = текущая `blockSizes` до этой ноды).
- Размер блока области = `blockSizes[scope]` = Σ выровненных слотов (накапливается `blockSizes += node.DataSizes.GetAligned()`).
- `DataSizes.Alignment = 8` (решение gate; легко поменять). Выравнивание относительно базы блока.
- `static`-арена резервирует: `TSize<CompiledBlueprint>` + таблица `NodeLayoutOffsets[nodesCount]` +
  `static`-блок (`blockSizes[Static]`). Остальные 4 области — только размеры в полях, без аллокации.
- **Zero-size чисто:** нулевой блок/0 нод → `MemAlloc` пропускается; `CalculateLayoutSizeToReserve`
  зеркалит guard'ы (lockstep).
- **Lockstep:** `CalculateLayoutSizeToReserve` ⟷ `SetupLayout` (нумерованные шаги); тест сверяет
  `UsedBytes - HeaderSize == CalculateLayoutSizeToReserve`.
- Инстанс: `instancePersistent`/`instanceCache` — worldState-блоки по `blockSizes`, обнуляются `MemClear`
  при Create; `ResetCache` = `MemClear` только cache-блока.

## Индекс тасок (реализация готова)

| # | Таска | Статус |
|---|---|---|
| 01 | [Node scope sizing API](tasks/01-node-scope-sizing-api.md) | ✅ done (нейминг с тех пор → `DataLayout`/`DataSizes`) |
| 02 | [CompiledBlueprint scope layout](tasks/02-compiledblueprint-scope-layout.md) | ✅ done (методы → `*Layout`; `nodeLayoutOffsets` → `BumpArray`) |
| 03 | [BlueprintInstance restructure](tasks/03-blueprintinstance-restructure.md) | ✅ done |
| 04 | [Tests + README](tasks/04-tests.md) | ✅ done |

> Таск-доки фиксируют первоначальную реализацию; актуальные имена/дизайн — в этом `plan.md` (раздел
> «Текущее состояние» + «Файлы»).

## Тесты (13 Phase-2)

- `LayoutTests`: `Layout_PerNodeSizesSumToBlockSizes`, `Layout_OffsetsAlignedAndNonOverlapping`,
  `Layout_AlignmentPadsSlots`, `Layout_LockstepReserveEqualsBump`, `Layout_ZeroSizeNodesLayoutCleanly`,
  `Layout_StaticSliceAddressableAndIsolated`.
- `InstanceScopeTests`: `InstanceScope_CreateAllocatesBothBlocks`,
  `InstanceScope_ResetCacheClearsCacheKeepsPersistent`, `InstanceScope_ZeroSizeInstanceBlocksClean`,
  `InstanceScope_DisposeFreesBlocks`.
- `BumpArrayTests`: `BumpArray_RoundTripsElements`, `BumpArray_GetSpanRoundTrips`,
  `BumpArray_EmptyIsInvalidAndZeroLength`.

## Non-goals (отложено)

- `NodesScope` и владелец scope-блоков `static cache`/`static persistent` — **Фаза 3**.
- Blueprint manager, версионирование, дедуп `static`, recompile — **Фаза 4**.
- Save/load `*persistent` — **Фаза 5**.
- Исполнение, модель портов/рёбер, дефолты портов, унификация legacy-layout с областями — **M6/M9**.
- Абсолютное (не относительно базы блока) выравнивание слайсов.

## Отклонения от PLAN.md / находки

- **Отдельный layout-путь вместо расширения `SetupBlueprint`** (legacy-порт неработоспособен) — gate ACK.
- **Правки файлов Фазы 1** (`BumpHeader`): добавлены `UsedBytes`, `SetupRelativePtr`, перегрузка `MemAlloc`.
- **Удалён/заменён `BumpHeaderExt.GetRelativeAllocator`.** Был латентный баг: self-relative резолв через
  bounds-checked `SafePtr + offset` падал на DEBUG-ассерте (offset к базе отрицательный, выходит за
  8-байтовое поле); путь использовался всем legacy `CompiledBlueprint`/`BlueprintCompiler`, но никогда не
  исполнялся под DEBUG. Фикс обобщён в новый тип `RelativePtr` (резолв сырым указателем, минуя
  bounds-операторы; доступ к `BumpHeader` дальше — через его `Memory` с полными границами).
- **Новый foundation-тип `RelativePtr`** (вне scope Фазы 2 по букве, но потребовался как чистая
  замена self-relative механики). Кросс-конверсии с `PtrOffset` — explicit (защита от смешения смыслов).

## Решения gate (ACK 2026-06-04)

1. Параллельные layout'ы (legacy-порт + новый) до M9 — ОК.
2. Выравнивание `8` (теперь `DataSizes.Alignment`).
3. Инстанс сам владеет своими блоками в Фазе 2; полный scope-lifecycle — Фаза 3.
