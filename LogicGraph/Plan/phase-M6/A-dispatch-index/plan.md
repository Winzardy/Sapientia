# M6-A — Dispatch index (закрытие заглушки NodeTypeId)

> **Статус: ✅ done (одобрено 2026-06-16).** Под-фаза вехи M6 ([../README.md](../README.md), развилка 1).
> Источник правды — код, затем [../README.md](../README.md) / [../../STATE.md](../../STATE.md).

## Цель

Закрыть заглушку `NodeTypeId`: перевести dispatch-id ноды с **`TypeId<INode>`** на **`TypeId<ILogicNode>`**
— плотный ordinal по уже-индексируемому контексту `ILogicNode` (дети = unmanaged logic-тела). Это даёт
реальный детерминированный индекс под function-table M6-C. **Исполнения/реестра ещё нет** — только id.

## Решение (одобрено на гейте разбивки)

- dispatch-id = `TypeId<ILogicNode>` (ordinal через `TypeIdOf<ILogicNode, TLogicNode>.typeId`).
- `INode` индексируемым **не** делаем (managed-authoring). `ILogicNode : IIndexedType` — уже контекст.
- Локальный ordinal бейкается прямо в блоб (`NodeHeader.typeId`); валидность гарантирует version gate (M6-E).

## Файлы (правка)

| Файл | Что |
|---|---|
| `Blueprint/INode.cs:15` | `INode.NodeTypeId` тип свойства `TypeId<INode>` → `TypeId<ILogicNode>` |
| `Blueprint/INode.cs:10` | `INode<TLogicNode>.NodeTypeId` дефолт: `TypeIdOf<INode, TLogicNode>` → `TypeIdOf<ILogicNode, TLogicNode>` |
| `Logic/StaticData/NodeHeader.cs:25` | поле `typeId`: `TypeId<INode>` → `TypeId<ILogicNode>`; doc «индекс метода (seed диспатча M6)» уточнить |
| `Logic/StaticData/CompiledBlueprintHeader.cs:74` | `GetNodeTypeId` возвращаемый тип → `TypeId<ILogicNode>` |
| `Logic/StaticData/BlueprintCompiler.cs:92` | `header.typeId = bpNodes[i].NodeTypeId;` — тип проброса меняется автоматически (проверить компиляцию) |
| `Tests/StubNode.cs:29` | `NodeTypeId` тип → `TypeId<ILogicNode>`; добавить опциональный параметр ctor для задания id (тест проводки) |

Грепнуть `TypeId<INode>` / `TypeIdOf<INode` по `LogicGraph/**` — не должно остаться ни одного (кроме осознанных).

## Тесты

| Тест | Что проверяет | Где |
|---|---|---|
| `Dispatch_CompilerWritesNodeTypeId` | компилятор пробрасывает `node.NodeTypeId` в `NodeHeader.typeId` (stub с заданным `TypeId<ILogicNode>`) — **не требует `IndexedTypes`** (id задаётся явно через implicit `int`→`TypeId<ILogicNode>`) | `NodeMapTests`/новый `DispatchIndexTests` |
| `Dispatch_LogicTypesGetDenseIds` | разные `ILogicNode`-типы → разные плотные ordinal через `TypeIdOf<ILogicNode, T>` | под `Assert.Ignore` (`IndexedTypes` не init в EditMode, как 4E/4F-2) |

## Задачи

| # | Задача | Статус |
|---|---|---|
| [01](tasks/01-migrate-dispatch-id.md) | Миграция типа dispatch-id во всех точках + doc-комменты | ✅ done |
| [02](tasks/02-tests.md) | Тест проводки компилятора + round-trip (Ignore) | ✅ done |

## Non-goals (последующие под-фазы)

Реестр функций (M6-C), контракт исполнения `ILogicNode.Execute`/`NodeContext` (M6-B), managed-бэкенд (M6-D),
version gate (M6-E), интеграция диспетчера (M6-F).

## Верификация

Компиляция-инспекция + adversarial-сабагент по diff + (при необходимости) изолированный `dotnet build`.
Batchmode не гоняем (EDM4U segfault — окружение).
