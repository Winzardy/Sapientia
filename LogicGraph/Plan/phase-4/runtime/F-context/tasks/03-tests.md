# 03 — Tests

**Статус: ✅ done** · Под-фаза [4F-2](../plan.md).

## Цель

Generic-API адресует контекст по типу `T` ⇒ нужен зарегистрированный `TypeId<INodeContext>`; в EditMode
`IndexedTypes` не инициализирован (`Count==0`) ⇒ round-trip под `Assert.Ignore`. Stub — `StubContext : INodeContext`
(в `StubNode.cs`). Lifecycle/empty — прогоняются.

## `Tests/ContextRegistryTests.cs`

- `Context_RoundtripWhenRegistered` — `Create(default)`; если `!IsCreated` → `Assert.Ignore`; иначе `SetContext(new StubContext{value=1234})`,
  `HasContext<StubContext>()`, `GetContext<StubContext>().Value<StubContext>().value == 1234`.
- `Context_EmptyRegistryAndDispose` — *(runs)* `Create(default)` (Count=0 → пуст): `HasContext<StubContext>()==false`; `Dispose` ×2 ок.

## `Tests/ExecutionScopeTests.cs`

- `Scope_CreateDisposeSmoke` — *(runs)* `Create(default,8)` → `IsCreated`; `Dispose` → `!IsCreated`; повторный `Dispose` ок.
- `Scope_ContextRoundtripWhenRegistered` — `Count==0` → `Assert.Ignore`; иначе set/get/has через scope.

## Done-criteria

- Тесты компилируются; lifecycle/empty прогоняются зелёными (логически), round-trip — `Ignore`. **Полный прогон отложен** (EDM4U).
- Реестр/scope освобождаются в `finally` (блоки контекстов — внутри `Dispose`).

## Зависимости

- Задачи 01, 02. Хелпер `StubContext` — в `StubNode.cs`.

## Заметки

- Без `unsafe`: `SafePtr.Value<StubContext>()`. По решению пользователя «пусть тесты пока что не работают» —
  round-trip готов к оживлению при регистрации `INodeContext`-типов.
