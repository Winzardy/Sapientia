# T03 — NodeInvoker → чистый диспатч

**Статус:** ✅ done

## Цель

Откатить `NodeInvoker` до чистого диспатча: `Run` уехал в `Orchestrator`.

## Шаги

1. Удалить `public static int Run(...)` (строки ~116–135).
2. Поправить class-doc/секцию «Интеграция диспетчера (M6-F)»: убрать упоминание Drain-driver/прогона;
   оставить описание per-node `Invoke` + Burst/Managed точек. Отметить, что прогон теперь в
   `Orchestrator` (M7-A).
3. `Execute`/`Compile`/`GetManaged`/`InvokeBurst`/`InvokeManaged`/`Invoke` — **не трогать**.

## Done-criteria

- `NodeInvoker` больше не ссылается на `ExecutionGraph`/`ExecutionScope.ResetAllCache` в run-контексте
  (только `Invoke` собирает `NodeContext` из scope — это остаётся).
- Нет «висячих» using'ов после удаления `Run` (проверить `System` для `Span` — `Span` больше не нужен в
  файле, если только не используется ещё где-то; убрать `using System;`, если стал лишним).
- Компилируется под Unity и .NET.

## Зависимости

T02 (Run должен уже существовать в Orchestrator до удаления здесь — иначе тесты/код не компилируются в
промежутке; делать T02 → T03 подряд).

## Notes
