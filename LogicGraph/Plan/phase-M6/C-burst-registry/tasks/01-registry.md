# 01 — `NodeFunctionRegistry`: хранилище + by-index lookup + mirror-init

**Статус:** ✅ done

## Цель

Завести статический реестр function-table нод: плоские таблицы по ordinal'у `TypeId<ILogicNode>`, lookup по
индексу, mirror-`Initialize(таблица)` (приём готовой таблицы — для тестов и будущего генератора), `Reset`.

## Шаги

1. `Logic/RuntimeData/Execution/NodeFunctionRegistry.cs` — `public static class NodeFunctionRegistry`.
2. Поля (static, process-wide): `ExecuteFn[] _managed = Array.Empty<…>()`; под `#if UNITY_5_3_OR_NEWER`
   `FunctionPointer<ExecuteFn>[] _burst = Array.Empty<…>()`; `int _count`; `bool _initialized`.
3. `IsInitialized` / `Count` (= `_count` — **отдельное поле**, не `_managed.Length`: под Unity при
   `buildManaged:false` `_managed` пуст, а таблица/`_burst` — нет).
4. `Initialize(ExecuteFn[] managed` + под `#if` `, FunctionPointer<ExecuteFn>[] burst)` — приём готовой
   таблицы: присвоить поля, `_count` = длина (под Unity — `burst.Length`, иначе `managed.Length`),
   `_initialized = true`. (Idempotent-гард — в задаче 02 на параметрless-перегрузке.)
5. `GetManaged(int ordinal)` → `_managed[ordinal]`; под `#if` `GetBurst(int ordinal)` → `_burst[ordinal]`.
6. `Reset()` — обнулить таблицы (`Array.Empty`), `_initialized = false`. Idempotent.
7. Комментарии (RU): инвариант ordinal == `TypeIdOf<ILogicNode,T>`; статика process-wide (как `IndexedTypes`).

## Done-criteria

- Компилируется в Unity и в plain .NET (Burst-ветка строго под `#if`).
- Lookup по индексу + mirror-init + `Reset` доступны; managed-таблица существует в обеих средах.

## Зависимости

- M6-B: `ExecuteFn`, `NodeInvoker`, `NodeContext`, `ILogicNode` (готовы).

## Заметки/находки

- **Ревью 2026-06-16:** реестр сделан **инстансом** (`struct : IDisposable`), не статикой/`SharedStatic`
  (тот капризен в эдиторе). Поля: `ExecuteFn[] _managed` + `UnsafeArray<FunctionPointer<ExecuteFn>> _burst`
  (off-allocator, под `#if UNITY`). `Count` убран (избыточен). Mirror — `Create(...)`; lifecycle — `Build`/`Dispose`.
  Burst-таблица отдаётся в Burst как `UnsafeArray` (через `BurstTable`/аргумент), не через статику. Интеграция
  в `ExecutionScope` — M6-F.
