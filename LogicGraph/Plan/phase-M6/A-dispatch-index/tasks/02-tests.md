# 02 — Тесты dispatch-id

**Статус:** ✅ done

## Цель
Доказать проводку компилятора (node → header) и плотность ordinal.

## Шаги
1. `Dispatch_CompilerWritesNodeTypeId`: stub-нода с заданным `TypeId<ILogicNode>` (через ctor) →
   скомпилировать → `compiled.GetNodeTypeId(id)` == заданный. Не требует `IndexedTypes`.
2. `Dispatch_LogicTypesGetDenseIds` (под `Assert.Ignore`): два `ILogicNode`-типа → разные плотные id
   через `TypeIdOf<ILogicNode, T>` (IndexedTypes не init в EditMode).

## Done-criteria
Тест проводки зелёный (инспекция/компиляция); round-trip под Ignore компилируется.

## Зависимости
Задача 01.

## Notes
—
