# 02 — BuildAdjacency / BuildNodeMap (топология из связей)

**Статус: ✅ done** · Под-фаза [4A](../plan.md) · Развилки 3, 4 (Build).

## Цель

Построить нод-граф зависимостей из `Blueprint.inputToOutput` + портов, с дедупом по ноде. Единый
managed-хелпер `BuildAdjacency` — источник и для sizing (`Calculate`), и для заполнения (`Setup`).

## Шаги

1. `BuildAdjacency(Blueprint) → (int[][] preds, int[][] succs, int[] startNodes)`:
   - Собрать `Dictionary<NodeOutput,int> owner` из `GetOutputs()` каждой ноды.
   - Для ноды `n`: по `GetInputs()` → `inputToOutput[in]` → `owner[out]` (если есть) → добавить в preds(n),
     n — в succs(owner). **Дедуп по ноде** (`HashSet<int>` на ноду).
   - Константы (нет owner) — пропустить (зависимость не создают).
   - `startNodes` = ноды с пустым preds.
2. `BuildNodeMap(ref BumpHeader, Blueprint)`: `relatives.Alloc(n)`; per-node `ref r = ref relatives.Get(i)`,
   `r.inputs.Alloc(...)`/`r.outputs.Alloc(...)` + заполнить; `startNodes.Alloc(...)` + заполнить.

## Done-criteria

- Топология совпадает с ожиданием тестов (таска 03); дедуп работает; константы не создают рёбер.

## Зависимости

- Нет (managed compile-path; зовётся из таски 01).

## Заметки

- Детерминизм порядка рёбер: обходить ноды/порты в порядке массива (стабильно) — важно для воспроизводимости.
