# Таска 02 — Жизненный цикл инстансов в scope

**Статус: ✅ done**

## Цель
Создание/удаление инстансов внутри scope и единый teardown.

> **Пост-ревью:** трекинг инстансов вынесен в отдельный `BlueprintInstanceStorage`
> (`Logic/BlueprintInstanceStorage.cs`); `ExecutionScope` его композирует и делегирует `CreateInstance`→`Add`,
> `DisposeInstance`→`Remove`, `TryGetInstance`→`TryGet`, `Dispose`→`Dispose`. Шаги ниже — исходный inline-вариант.

## Шаги
- `InstanceSlot` (id + `CachedPtr<BlueprintInstance>`), поля `_instances`, `_nextInstanceId`.
- `CreateInstance(ws, ref storage, id, version)`: `EnsureSite` → `BlueprintInstance.Create(ws, ref compiled,
  scopeLocalId)` → трек в `_instances`; вернуть `Id<BlueprintInstance>`.
- `TryGetInstance(id, out CachedPtr<BlueprintInstance>)` (линейный поиск — ок на эту фазу).
- `DisposeInstance(ws, id)`: найти slot → `instance.GetValue(ws).Dispose(ws)` (блоки инстанса) →
  `instance.Dispose(ws)` (сам инстанс) → удалить из списка (`RemoveAtSwapBack`). Site **не** трогать.
- `Dispose(ws)`: пройти все инстансы (`DisposeInstance`/инлайн), затем все site
  (`staticPersistent.Dispose(ws)` + off-alloc `MemFree(staticCache)`); `this = default`. Идемпотентно.

## Done-criteria
- Create/Dispose инстанса; `InstanceCount` корректен; site переживает dispose отдельного инстанса;
  `Dispose` освобождает всё без двойного free.

## Зависимости
- Таска 01 (sites), Фаза 2 (`BlueprintInstance.Create/Dispose`).
</content>
