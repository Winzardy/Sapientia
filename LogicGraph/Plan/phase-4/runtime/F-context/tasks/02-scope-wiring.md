# 02 — ExecutionScope wiring

**Статус: ✅ done** · Под-фаза [4F-2](../plan.md).

## Цель

Вшить `ContextRegistry<INodeContext>` в `ExecutionScope`: создание (размер внутри из `TypeId<INodeContext>.Count`),
освобождение, делегирующее generic-API.

## Шаги

- Поле `private ContextRegistry<INodeContext> _contexts;`.
- `Create(memoryId, instanceCapacity = 8)` (**без** `contextCapacity`): `_contexts = ContextRegistry<INodeContext>.Create(memoryId);`.
- Методы (generic-only): `SetContext<T>(in T)`/`GetContext<T>()`/`HasContext<T>()` — делегируют в `_contexts`.
- `Dispose()`: `_contexts.Dispose();` до `this = default`.
- Убрать неиспользуемый `using Sapientia.TypeIndexer;` (ссылок на `TypeId` в scope больше нет).
- Обновить doc-комментарий класса: реестр контекста — **сделан** (убрать «4F-2 next»).

## Done-criteria

- Компилируется; `Create` обратносовместим (убран только незакоммиченный `contextCapacity`).
- `Dispose` освобождает реестр (нет утечки).

## Зависимости

- Задача 01 (`ContextRegistry<TContext>`).

## Заметки

- `TypeId<INodeContext>.Count` в тест-окружении без `IndexedTypes` = 0 ⇒ реестр `default` (пуст). Тесты round-trip — под `Assert.Ignore`.
