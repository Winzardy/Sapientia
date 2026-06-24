# T03 — Снос ExecutionGraph / батч-DAG

**Статус:** ✅ done

## Шаги
1. Удалить `ExecutionGraph.cs` (+ `.meta`), `ExecutionBatch`. `RuntimeType` — перенести (нужен в M7-C) в
   отдельный файл/`NodeHeader`-сосед.
2. Удалить `Tests/ExecutionGraphTests.cs` (+ `.meta`).
3. Снять все ссылки (Orchestrator уже не зависит от ExecutionGraph после T02).

## Done-criteria
- Сборка без `ExecutionGraph`/`Drain`/`ExecutionBatch`.
- `RuntimeType` доступен.
