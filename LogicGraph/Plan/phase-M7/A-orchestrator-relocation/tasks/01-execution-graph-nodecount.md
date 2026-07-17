# T01 — Учёт числа нод (в оркестраторе)

**Статус:** ✅ done

## Цель

Оркестратору нужен суммарный размер буфера порядка (число нод по всем `Inject`'ам). **Решение ревью
(2026-06-18):** считать это **в самом `Orchestrator`** (владельце буфера), а **не** добавлять `NodeCount`
в `ExecutionGraph` — учёт размера буфера не его ответственность.

## Шаги

1. `private int _nodeCount;` в `Orchestrator`.
2. В `Orchestrator.Inject`: `_nodeCount += compiled.NodesCount;` перед форвардом в `_graph.Inject`.
3. `_nodeCount` сбрасывается в `Dispose` через `this = default`.
4. `ExecutionGraph` **не трогаем** (первоначальная правка `NodeCount` откатана).

## Done-criteria

- `ExecutionGraph` без изменений (`git diff` пуст).
- `_nodeCount` == сумме `compiled.NodesCount` по всем `Inject` (== числу записей, которые пишет `Drain`).
- Компилируется под Unity и .NET.

## Зависимости

Нет (поле живёт в T02-структуре; задачи слиты).

## Notes

- Первоначально планировалось как `ExecutionGraph.NodeCount`; по ревью перенесено в `Orchestrator`
  (владелец буфера сам знает свой размер; `ExecutionGraph` остаётся чистым substrate'ом).
