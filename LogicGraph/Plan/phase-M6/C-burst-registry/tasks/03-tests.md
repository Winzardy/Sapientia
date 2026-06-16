# 03 — `NodeFunctionRegistryTests`

**Статус:** ✅ done

## Цель

Покрыть реестр: реальный managed round-trip через mirror-инъекцию (без Burst/IndexedTypes), гарды
(double-init, Count, Reset), и Assert.Ignore-round-trip через reflection-`Initialize()`.

## Шаги

1. `Tests/NodeFunctionRegistryTests.cs` (`#if UNITY_5_4_OR_NEWER`, как соседи).
2. Stub-logic тела (в тесте): `StubLogicAdd` (читает In из Cache, пишет In+addend в Out — как `StubAdd` в
   `NodeExecutionTests`), `StubLogicNeg` (пишет −In). Используют ручной `NodeContext`/`InstanceCache` (хелперы
   повторить из `NodeExecutionTests`: `H(index)`, `CreateCache`).
3. `[TearDown]` → `NodeFunctionRegistry.Reset()` (изоляция статики).
4. `Registry_InjectedManagedRoundTrips`: `Initialize(new[]{ GetManaged<StubLogicAdd>(), GetManaged<StubLogicNeg>() })`
   → `GetManaged(0)`/`GetManaged(1)` исполнить над собранным `NodeContext` → проверить результаты в Cache.
5. `Registry_CountMatchesTable` / `Registry_DoubleInitIsNoOp` / `Registry_ResetClears` — гарды.
6. `Registry_StartupBuildsDenseTable`: `if (TypeId<ILogicNode>.Count==0) Assert.Ignore(...)`; иначе
   `Initialize()` → `Count==TypeId<ILogicNode>.Count`, `GetManaged(ordinal)!=null` для каждого.

## Done-criteria

- Managed round-trip + гарды зелёные (логически; раннер не гоняем — компиляция-инспекция).
- Startup-тест под `Assert.Ignore` в EditMode.
- Нет протечки статики между тестами (`Reset` в TearDown).

## Зависимости

- Задачи 01/02; `NodeInvoker.GetManaged<T>`, `InstanceCache`, `CacheHandler<long>`, `BlueprintCompiler`.

## Заметки/находки

—
