# 01 — relatives + startNodes в Static-блобе

**Статус: ✅ done** · Под-фаза [4A](../plan.md) · Развилка 3.

## Цель

Переписать `NodeMapHeader` под per-node `BumpArray<NodeRelativesHeader>` + `startNodes`, реально
аллоцировать его в Static-блобе и держать lockstep с `CalculateLayoutSizeToReserve`.

## Шаги

1. `NodeMapHeader.cs`: `relatives: BumpArray<NodeRelativesHeader>`, `startNodes: BumpArray<Id<NodeHeader>>`;
   `NodeRelativesHeader{ inputs, outputs: BumpArray<Id<NodeHeader>> }`.
2. `CalculateLayoutSizeToReserve` — шаг 5 (nodeMap): размер массива relatives + Σ рёбер + startNodes.
3. `SetupLayout` — вызвать `BuildNodeMap` после `SetupMap`; аллокации через `ref relatives.Get(i)`
   (self-relative — только через арена-ref, не на копии).

## Done-criteria

- Компилируется; `nodesMap` реально аллоцируется; lockstep-тест (резерв == bump) зелёный на stub-графах.

## Зависимости

- Хелпер `BuildAdjacency` (таска 02) — счётчики для шага 5 и заполнение в `BuildNodeMap`.

## Заметки

- (заполняется по ходу)
