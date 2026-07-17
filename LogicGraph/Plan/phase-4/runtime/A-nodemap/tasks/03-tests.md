# 03 — NodeMapTests

**Статус: ✅ done** · Под-фаза [4A](../plan.md).

## Цель

`Tests/NodeMapTests.cs` — доказать топологию на stub-графах (по образцу `MapTests`/`LayoutTests`:
`CompileLayout` → `ref compiled` → assert → `arena.Dispose()` в `finally`).

## Тесты

- `NodeMap_LinearChain` — `A→B→C`: startNodes=[A]; preds/succs/inDegree.
- `NodeMap_Diamond` — `A→B,A→C,B→D,C→D`: preds(D)=[B,C] (inDegree 2), succs(A)=[B,C].
- `NodeMap_ParallelIndependent` — 3 несвязанные: startNodes = все, inDegree 0.
- `NodeMap_DuplicateEdgeDeduped` — 2 In из одной ноды → preds = 1 (inDegree 1).
- `NodeMap_ConstantSourceIsRoot` — In от `ConstOutput` → нода в startNodes, inDegree 0.
- `NodeMap_LockstepWithNodeMap` — резерв == bump для всех графов выше.

## Done-criteria

- Все тесты написаны; компилируются. **Прогон отложен** (раннер падает в EDM4U) — фиксируется как риск в пакете.

## Зависимости

- Таски 01, 02.

## Заметки

- Связи задаются через `bp.inputToOutput[in] = out` (как в `MapTests`). Хелпер `ConstOutput<T>` —
  скопировать из `MapTests` или вынести в общий тест-хелпер (решить при реализации; вынос — опционально).
