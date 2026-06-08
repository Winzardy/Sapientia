# Таска 01 — CompiledBlueprintStorage

**Статус: ✅ done (тесты 25/25 зелёные)**

## Цель
Хранилище скомпилированных блюпринтов (эволюция `BlueprintCompiler`). Не знает о `Blueprint`/компиляции —
принимает готовые `CompiledBlueprint` + арены.

## Итог (что реализовано)
- `CompiledBlueprintStorage` (off-allocator): `_arenas` (список арен-батчей) + `_blueprints`
  (`UnsafeList<RootSlot>` по `Id<Blueprint>`, jump-by-id). `RootSlot` = текущая версия инлайн + список старых.
- `Add(arena, offset)` / `Add(arena, Span<offsets>)` (батч): читает `(id, version)` из блоба, дедуп,
  supersede (старая в `next`); владение ареной → стореджу.
- `Has(id, version)`, `Get(id, version)`, `Count`, `Dispose`. Адресация — `(Id<Blueprint>, version)`,
  без плоского `Id<CompiledBlueprint>`.
- Удалён `BlueprintCompiler`; `LogicGraph` stub → storage.
- Core-фиксы (вне LogicGraph): `UnsafeList.SetCount(zero-init)/EnsureCount`, `RawBumpAllocator.Dispose`
  идемпотентный.

## Done-criteria
- Add/Count/Has/Get; дедуп; рантайм-add нового id; сосуществование версий; Dispose. ✅
