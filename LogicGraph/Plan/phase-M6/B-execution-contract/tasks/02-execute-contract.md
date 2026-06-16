# 02 — Execute-контракт + дисптач-вход

**Статус:** ✅ done

## Цель
Сигнатура тела ноды + тип function-table + generic-вход.

## Шаги
1. `Logic/ILogicNode.cs`: + `void Execute(ref NodeContext ctx)` (был пустой маркер).
2. `Logic/RuntimeData/Execution/NodeInvoker.cs`: `delegate void NodeFn(ref NodeContext)` + статический
   `Execute<T>(ref NodeContext)` — `ref T body = ctx.StaticSlice().Cast<T>().Value(); body.Execute(ref ctx);`
   (constrained call, без боксинга).
3. Обновить `StubLogicA/B` (в `DispatchIndexTests`) — реализовать пустой `Execute` (компиляция).

## Done-criteria
Компилируется; `Execute<T>` зовёт тело через constrained call; существующие реализации `ILogicNode` обновлены.

## Зависимости
Задача 01.

## Notes
Имя `NodeInvoker` — на подтверждение гейта (альтернативы NodeDispatch/NodeFunction).
