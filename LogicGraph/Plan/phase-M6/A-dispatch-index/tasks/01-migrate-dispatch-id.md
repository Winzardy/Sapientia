# 01 — Миграция типа dispatch-id

**Статус:** ✅ done

## Цель
Перевести dispatch-id ноды `TypeId<INode>` → `TypeId<ILogicNode>` во всех точках LogicGraph.

## Шаги
1. `Blueprint/INode.cs`: тип `INode.NodeTypeId` → `TypeId<ILogicNode>`; дефолт в `INode<TLogicNode>` →
   `TypeIdOf<ILogicNode, TLogicNode>.typeId`.
2. `Logic/StaticData/NodeHeader.cs`: поле `typeId` → `TypeId<ILogicNode>`; обновить doc-коммент.
3. `Logic/StaticData/CompiledBlueprintHeader.cs`: `GetNodeTypeId` → `TypeId<ILogicNode>`; doc.
4. `Tests/StubNode.cs`: тип `NodeTypeId` + опциональный ctor-параметр для задания id.
5. Греп `TypeId<INode>`/`TypeIdOf<INode` по `LogicGraph/**` → 0 остатков.

## Done-criteria
Компилируется; ни одной ссылки `TypeId<INode>` в LogicGraph; doc-комменты отражают `ILogicNode`-ordinal.

## Зависимости
Развилка 1 (одобрена).

## Notes
—
