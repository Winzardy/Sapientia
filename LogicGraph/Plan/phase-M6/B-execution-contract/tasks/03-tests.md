# 03 — Тесты seam'а (managed round-trip)

**Статус:** ✅ done

## Цель
Доказать, что контракт реально резолвит память и исполняется (managed, без Burst).

## Шаги
1. `Tests/NodeExecutionTests.cs`: stub logic-нода `StubAdd : ILogicNode` (читает In, пишет In+const в Out
   через `CacheHandler` + `ctx.Cache()`).
2. Вручную собрать: блоб со static-слайсом = тело `StubAdd`, `InstanceCache` из шаблона, `NodeContext`.
3. `NodeInvoker.Execute<StubAdd>(ref ctx)` → `cache.Read(out)` == ожидаемое.
4. `Exec_StaticSliceResolves` / `Exec_PersistenceSliceResolves` — резолв баз через ref.

## Done-criteria
Round-trip зелёный (managed); резолв static/persistence корректен.

## Зависимости
Задачи 01, 02.

## Notes
Burst-компиляция `Execute<T>` — M6-C (под Assert.Ignore).
