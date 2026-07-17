# M6-E · Задача 02 — `CompiledEnvironment` (версия группы)

**Статус: ✅ done** (реализовано + self-review; ревью-гейт пользователя — Step 6)

## Цель

«Версия кода» — **верхнеуровневая сущность группы блюпринтов**: живёт в носителе окружения
(`CompiledEnvironment`), который компилируется заранее и грузится в рантайме. **Блоб о хеше не знает.**

## Шаги

1. **`CompiledEnvironment`** (`Logic/StaticData/CompiledEnvironment.cs`): struct с `ulong contractHash`, ctor от
   значения (рантайм-load), `static Compile()` (build-time, фиксирует `NodeContractHash.Local`),
   `readonly bool IsCompatibleWith(in CompiledEnvironment other) => contractHash == other.contractHash` (предикат гейта).
2. `NodeContractHash.Local` — **build/bake-only** (используется `CompiledEnvironment.Compile`), рантайм её не зовёт.
3. **`CompiledBlueprintHeader` и `BlueprintCompiler` — НЕ трогаются** (блоб остаётся чистым, бейка хеша нет).

## Done-criteria

- `CompiledEnvironment.Compile().contractHash == NodeContractHash.Local`; ctor-from-value работает.
- `IsCompatibleWith` — чистый предикат (равенство версий), тестируется без `IndexedTypes`.
- `CompiledBlueprintHeader`/`BlueprintCompiler` не изменены (нет per-blob hash).

## Зависимости

- Задача 01 (`NodeContractHash`).

## Notes / findings

- Хеш поднят на уровень **группы** (окружения), не per-blob/per-node — решение пользователя 2026-06-17.
  Блоб (`CompiledBlueprintHeader`) о хеше ничего не знает.
