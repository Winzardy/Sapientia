# 04 — Reconcile: STATE.md / CLAUDE.md status-map / README / PLAN.md

**Статус: ✅ done (закоммичено, 2026-06-17)**

## Цель

Синхронизировать документацию с закрытием вехи M6 (M6-F = последняя под-фаза).

## Шаги

1. **`Plan/phase-M6/README.md`** — строка `M6-F` в таблице разбивки → ✅ done (закоммичено); ссылка на
   `F-dispatcher-integration/plan.md`.
2. **`Plan/STATE.md`**:
   - §5 п.9 — добавить `✅ M6-F`: seam `NodeInvoker.Invoke`/`Run`, реестр в `ExecutionScope`, прогон Drain-порядка
     через диспетчер (single-thread); тела нод **исполняются** (run-путь замкнут end-to-end).
   - §2 «Execution»/«Runtime» — отметить, что тела нод теперь исполняются через диспетчер (M6 закрыт; параллелизм — M7).
3. **`CLAUDE.md` status-map** (design → code):
   - строка «Burst functions … by index (#3.2)» и «Dual backend (#3.3)» — пометить **M6-F**: прогон через диспетчер;
   - строка «**Orchestrator**: deps + parallel + Burst/non-Burst (#7)» — обновить: **single-thread прогон есть (M6-F)**,
     параллелизм/wave — M7 (◐).
   - (точечные правки строк, не переписывать §3/§4/§7 — они помечены устаревшими отдельной задачей.)
4. **root `PLAN.md`** — строка `M6` `Status` → ✅ (веха закрыта: A–F все done); тик чекбокса вехи, если есть.
5. Зафиксировать follow-up'ы для M7 (мульти-блюпринтовый `Run`, wave, scope-в-Burst-job) в README/STATE.md «Не входит».

## Done-criteria

- Все артефакты F (`plan.md` + 4 task-дока) → ✅ done; README M6-F → ✅; STATE/CLAUDE/PLAN отражают «M6 закрыт».
- Ни один статус не противоречит коду.

## Зависимости

- Делается в Step 8 (после апрува), вместе с коммитом.

## Notes / findings

- README phase-M6 (top-status + M6-F row), root PLAN.md (M6 → ✅), CLAUDE.md status-map (#3.2/#3.3/#7),
  STATE.md (§2 CompiledBlueprintHeader/ExecutionScope/InstanceCache/Execution rows, §3/§5 `_template`-строки, §5 п.9 → ✅ + M6-F-абзац),
  F-plan (статус + task-index + пост-рефактор-нота) — все синхронизированы.
- **Поправлены устаревшие по коду упоминания** (рефактор тем же сидением): `NodeFunctionRegistry.Invoke` (удалён → `InvokeBurst`/`InvokeManaged`),
  `GetNodeTypeId`/`GetNodeRuntimeType` (→ `GetNode`), `InstanceCache._template`/`Reset()` (→ `Reset(template)` из статики), ordinal → `Id<ExecuteFn>`.
- M7-долги (мульти-блюпринтовый `Run`, wave/параллелизм, scope-в-Burst-job, `Run` переедет из `NodeInvoker`) зафиксированы в README «Не входит» + STATE.md §5 п.9.
