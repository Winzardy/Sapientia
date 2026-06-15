# Под-фаза 4A — NodeMapHeader (топология в Static-блобе)

**Статус: ✅ одобрено пользователем (не закоммичено — submodule, по директиве «не коммитить без запроса»).**
Развилки: 3 (аллокация/кратность), 4 (место Build).
Обзор и решения по всем 8 развилкам — [../README.md](../README.md).

## Цель

Забейкать **граф связей нод** в Static-блоб (инстанс-агностичная топология, детерминирована из
`Blueprint.inputToOutput`, дедуп вместе с блобом). Это substrate под батч-шедулинг (4B): по топологии
строится execution-DAG. Поведения исполнения здесь нет — только топология + accessors + lockstep.

## Файлы

- **M** `Logic/StaticData/NodeMapHeader.cs` — переписать: `relatives` → `BumpArray<NodeRelativesHeader>`
  (per node); добавить `startNodes: BumpArray<Id<NodeHeader>>`; `NodeRelativesHeader{ inputs, outputs }`
  оставить; `InDegree`.
- **A** `Logic/StaticData/BlueprintCompiler.cs` — **новый** (post-review, см. Отклонения): вся compile-логика
  `Blueprint → CompiledBlueprintHeader` (`CalculateLayoutSizeToReserve` +шаг 5 nodeMap, `CompileLayout`,
  `SetupLayout`/`SetupMap`/`BuildNodeMap`/`BuildAdjacency`). Пишет в `ref CompiledBlueprintHeader`.
- **M** `Logic/StaticData/CompiledBlueprintHeader.cs` — стала **чистой рантайм-структурой** (поля + аксессоры
  топологии `GetNodeRelatives`/`GetNodeInDegree`/`StartNodeCount`/`GetStartNode`); compile-логика вынесена.
- **M** тесты (`LayoutTests`/`MapTests`/`NodeMapTests`/…): `CompiledBlueprintHeader.Compile*` → `BlueprintCompiler.*`.
- **A** `Tests/NodeMapTests.cs` — топология на stub-графах (цепочка, ромб, параллель, дедуп, константа, self-loop, пусто).

## Раскладка данных

```csharp
public struct NodeMapHeader
{
    public BumpArray<NodeRelativesHeader> relatives; // на ноду (по Id<NodeHeader>)
    public BumpArray<Id<NodeHeader>>      startNodes; // ноды с inDegree == 0 (корни исполнения)
}

public struct NodeRelativesHeader
{
    public BumpArray<Id<NodeHeader>> inputs;  // ДЕДУПнутые ноды-предшественники (источники In'ов)
    public BumpArray<Id<NodeHeader>> outputs; // ДЕДУПнутые ноды-потомки (читатели Out'ов)
    // inDegree == inputs.Length — счётчик для батч-шедулинга (4B копирует в атомарный remainingDeps)
}
```

- **Ребро строится по нодам, не по портам.** Для ноды `n`: предшественники = `{ owner(inputToOutput[in])
  : in ∈ GetInputs(n), у источника есть нода-владелец }`; потомки — обратное. **Дедуп по ноде** (две связи
  в одну и ту же ноду → одно ребро): inDegree считает различные ноды-предшественники.
- **Готовые источники предшественником не считаются** (зависимости не создают): precalculated-Out
  (`IsPreCalculated` — забейкан в Static, **в т.ч. если принадлежит ноде**) и Out без ноды-владельца
  (висячий). Критерий «готового» един с `SetupMap` (IsPreCalculated → Static). Самопетля игнорируется.
- `startNodes` = ноды с `inputs.Length == 0` (нет нод-предшественников; вход графа / только константы).

## Публичный API (эскиз)

```csharp
// CompiledBlueprintHeader — accessors к топологии (через ref/арена-указатель: self-relative BumpArray)
public ref NodeRelativesHeader GetNodeRelatives(Id<NodeHeader> nodeId);
public int GetNodeInDegree(Id<NodeHeader> nodeId);   // = relatives[nodeId].inputs.Length
public SafePtr<Id<NodeHeader>> GetStartNodes(out int count);
```

## Шаги исполнения

1. Переписать `NodeMapHeader.cs` (типы/поля выше).
2. `BuildAdjacency(Blueprint)` — managed-хелпер: строит `int[][] preds`, `int[][] succs`, `int[] startNodes`
   из `inputToOutput` + карты `NodeOutput → ownerNodeIndex` (собирается из `GetOutputs()`). Дедуп по ноде.
   **Единственный источник** счётчиков — зовётся и из `Calculate`, и из `Setup` (DRY lockstep, по образцу
   `CountPorts`/`CalculateConstantsSize`).
3. `CalculateLayoutSizeToReserve` — шаг 5: `TSize<NodeRelativesHeader>*nodeCount` + Σ по нодам
   `(preds[n].Length + succs[n].Length)*TSize<Id<NodeHeader>>` + `startNodes.Length*TSize<Id<NodeHeader>>`.
4. `SetupLayout` — после `SetupMap`: `BuildNodeMap(ref allocator, blueprint)` — `relatives.Alloc(n)`, per-node
   `inputs/outputs.Alloc` + заполнение (через `ref relatives.Get(i)` — self-relative в арене!), `startNodes.Alloc` + заполнение.
5. Accessors в `CompiledBlueprintHeader`.
6. `NodeMapTests` (рядом с кодом).
7. Self-review (Step 4): чек-лист + adversarial-сабагент. Тесты не прогоняются (раннер падает в EDM4U).

## Задачи

| # | Таска | Статус |
|---|---|---|
| 01 | [relatives + startNodes в блобе](tasks/01-relatives-blob.md) | ✅ done |
| 02 | [BuildAdjacency / BuildNodeMap](tasks/02-build-adjacency.md) — построение топологии из связей, дедуп | ✅ done |
| 03 | [тесты](tasks/03-tests.md) — `NodeMapTests`: цепочка/ромб/параллель/дедуп/константа/lockstep | ✅ done |

> Статус под-фазы: ✅ одобрено и коммитится. Прогон тестов отложен (раннер падает в EDM4U).

## Тест-лист (model-proving, stub-ноды)

- **Linear chain** `A→B→C`: `startNodes=[A]`; preds(B)=[A], preds(C)=[B]; succs(A)=[B]; inDegree A=0,B=1,C=1.
- **Diamond** `A→B, A→C, B→D, C→D`: `startNodes=[A]`; preds(D)=[B,C] (inDegree 2); succs(A)=[B,C].
- **Parallel** (3 несвязанные ноды): `startNodes` = все три; все inDegree 0, relatives пустые.
- **Dedup**: нода с 2 In'ами из одной ноды-источника → preds = 1 элемент (inDegree 1).
- **Константа-источник**: In, чей источник — `ConstOutput` (нет ноды) → нода в `startNodes`, inDegree 0.
- **Lockstep**: `CalculateLayoutSizeToReserve == UsedBytes - HeaderSize` для всех графов выше.

## Non-goals (отложено)

- **Разбиение на батчи-цепочки** (батч = линейная цепочка нод: последовательно, без ожидания/ветвления —
  директива пользователя) — **4B** (по топологии relatives/startNodes отсюда). Здесь только сырой нод-граф.
- **`Inject` / инстанцирование батч-DAG под инстанс** — 4B.
- **Достижимость от конкретного корня (`BuildBatches(root)` для multiple entry points #13)** — M9; здесь
  startNodes = все ноды без предшественников по всему графу.
- **Исполнение тел нод / диспатч** — M6.

## Отклонения (post-review)

- **Выделен `BlueprintCompiler`** (запрос пользователя на ревью): compile-логика отделена от данных.
  `CompiledBlueprintHeader` больше **не знает о authoring-`Blueprint`** (нодах/портах/связях) — компилятор
  единственное место, знающее об authoring-стороне. `Id<Blueprint>`/`VersionedId<Blueprint>` в заголовке —
  только тег идентичности (phantom-тип), не поведенческая связь; оставлен (пронизывает identity всей системы).

## Отклонения от исходного `NodeMapHeader`

- Поле `relatives` было `RelativePtr<NodeRelativesHeader>` (singular, не аллоцировалось) → стало
  `BumpArray<NodeRelativesHeader>` (per-node, аллоцируется). Добавлено `startNodes`.
- `BuildBatches`/`Inject` из комментария разведены: Build (топология) — компиляция/Static здесь; Inject
  (инстанцирование) — 4B на `ExecutionGraph`.
