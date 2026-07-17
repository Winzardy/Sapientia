# 01 — ContextRegistry

**Статус: ✅ done** · Под-фаза [4F-2](../plan.md).

## Цель

`Logic/RuntimeData/ContextRegistry.cs` — структура-владелец массива `SafePtr` по id контекста.
Отдельный тип (как `InstancePersistence`/`InstanceCache`), тестируемый без `ExecutionScope`.

## Шаги (generic-модель)

- `struct ContextRegistry<TContext> : IDisposable where TContext : IIndexedType`.
- Поля: `Id<MemoryManager> _memoryId`, `UnsafeArray<SafePtr> _slots` (индекс = `TypeId<TContext>` → `int` неявно).
- `Create(memoryId)`: `count = TypeId<TContext>.Count`; `count<=0` → `default`; иначе `_slots = new UnsafeArray<SafePtr>(memoryId, count)`.
- `SetContext<T>(in T value)` (`unmanaged, TContext`): `id = TypeIdOf<TContext,T>.typeId`; bounds-assert; если слот невалиден →
  `MemAlloc(TSize<T>.size, ClearMemory)`; `slot.Value<T>() = value`.
- `GetContext<T>()` (`TContext`): bounds-assert; `_slots[id]` (пусто → `default`).
- `HasContext<T>()` (`TContext`): в границах И `_slots[id].IsValid`.
- `Dispose()`: для каждого валидного слота `MemFree`, затем `_slots.Dispose()` (идемпотентно).
- **Нет** id-based методов, **нет** `count`-параметра, **нет** `_sizes` (generic-only ⇒ размер фиксирован `T`).

## Done-criteria

- Компилируется (generic-constraint `T : TContext`, `TContext : IIndexedType` → `TypeIdOf` ок — проверено `dotnet build` репро).
- Каждый блок контекста и `_slots` — off-allocator, освобождаются в `Dispose` (нет утечки).
- Без `(int)`-каста (implicit `TypeId→int`); tabs, `_camelCase`, без LINQ; doc по-русски.

## Зависимости

- Инфра: `UnsafeArray<T>`, `SafePtr.Value<T>()`, `TSize<T>`, `MemoryManager.MemAlloc/MemFree`,
  `TypeId<TContext>.Count`/`TypeIdOf<TContext,T>`, `IIndexedType`, `INodeContext`.

## Заметки

- `GetContext`/`HasContext` — `readonly`. `UnsafeArray` позиционно-независим ⇒ defensive-copy от `readonly` безопасна.
- Повторный `SetContext<T>` переиспользует блок (один `T` ⇒ один размер) — reuse-overflow невозможен (нет id-based с произвольным размером).
