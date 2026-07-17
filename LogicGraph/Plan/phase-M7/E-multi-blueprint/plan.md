# M7-E — Мульти-блюпринтовый прогон

> **Статус: ✅ реализовано (гейт в чате 2026-06-18).** Источник модели — [README «Целевая модель»](../README.md).

## Цель
Прогон группы **независимых** блюпринтов в одном `Run`: `compiled` различается per-instance — резолвить на лету.

## Реализовано
- `Orchestrator.Run(ref scope, ref CompiledBlueprintStorage storage)` (вместо `ref compiled`).
- Per-instance кеш блоба `_compiled` (`UnsafeList<SafePtr<CompiledBlueprintHeader>>`, слот = `blueprintId.id`):
  `EnsureInstance` резолвит `key→storage.Get` **раз/инстанс**, `CompiledOf` отдаёт `ref` блоба per-node.
  Schedule сайзится по per-instance `compiled.NodesCount`.
- `ExecutionScope.GetBlueprintKey(id)` — `BlueprintInstanceHeader.blueprintId` (через `TryGet`).
- Ref-инвариант: пре-резолв инстансов входов до цикла (pull/push не создают новых) ⇒ `_compiled`/`_schedule`
  не ресайзятся в цикле; `CompiledOf` отдаёт ref в стабильный off-allocator storage-блоб.

## Тесты
- `Run_MultiBlueprint_IndependentResults` — два разных блюпринта (ключи (1,1)/(2,1)), входы обоих в одном
  `Run`, независимые результаты (100/200). Остальные `Run_*` мигрированы на `Run(…, storage)`.

## Не входит
- **command-buffer** (deferred side-effects) — **M7-F** (своя развилка).
- Вложенность/ExecRef — M9. Параллелизм — вне M7.
