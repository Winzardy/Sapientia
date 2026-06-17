# M6-E · Задача 01 — `NodeContractHash`

**Статус: ✅ done** (реализовано + self-review; ревью-гейт пользователя — Step 6)

## Цель

Детерминированная «версия кода» — авто-хеш контракта нод-функций, инвариантный к набору/порядку блюпринтов и нод,
но чувствительный к набору/порядку/идентичности logic-типов (function-table) и к ABI-версии контракта исполнения.

## Шаги

1. Новый файл `Logic/RuntimeData/Execution/NodeContractHash.cs`, `static class NodeContractHash`.
2. `public const ulong FormatVersion = 1;` — ручная ABI-версия (`ExecuteFn`-сигнатура + `NodeContext`-раскладка),
   бампится автором при изменении контракта исполнения. XML-комментарий с этим инвариантом.
3. FNV-1a 64: приватные const `FnvOffsetBasis = 14695981039346656037UL`, `FnvPrime = 1099511628211UL`. Хелперы
   `Fold(ulong hash, byte b)` и `Fold(ulong hash, ulong value)` (8 байт LE).
4. `public static ulong Compute(params Type[] orderedLogicTypes)` — seed = `Fold(FnvOffsetBasis, FormatVersion)`,
   затем для каждого типа по порядку **три слоя**: `Type.FullName` (**структурный**) → **IL тела `Execute`**
   (`FoldExecuteBody`: резолв через `GetInterfaceMap`, `MethodBody.GetILAsByteArray()`; недоступно → маркер) —
   **поведенческий** → **раскладка данных** (`FoldDataLayout`: `Marshal.SizeOf` + поля `[name, type FullName]` по
   `MetadataToken`-порядку; `SizeOf`-исключение → маркер) — **слой данных**. `null`-тип → маркер.
5. `public static ulong Local` → упорядоченные типы из `IndexedTypes.GetContextChildren(typeof(ILogicNode))`
   (`TypeId[]`, по ordinal) → `IndexedTypes.GetType(child)` → `Compute(types)`. В EditMode детей нет ⇒ `Compute()` пустой.

## Done-criteria

- `Compute` детерминирован в пределах сборки (без `string.GetHashCode`/`Random`/wall-clock), чувствителен к
  порядку, набору, **телу** `Execute` **и раскладке данных** (поля/размер). `null`/без IL/без размера → маркер (без краша).
- `Local` не падает при пустом `IndexedTypes` (EditMode) — возвращает стабильный seed-хеш.
- Ограничения IL-хеша (токены/Debug-Release/Burst-native/IL2CPP-стрип/транзитивность) задокументированы в коде
  (приняты пользователем; робастная замена — content-digest M10-кодогена).

## Зависимости

- `Sapientia.TypeIndexer.IndexedTypes` (`GetContextChildren`/`GetType`), `ILogicNode`.

## Notes / findings

(заполняется по ходу)
