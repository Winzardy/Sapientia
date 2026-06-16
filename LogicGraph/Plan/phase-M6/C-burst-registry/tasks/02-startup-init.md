# 02 — `Initialize()`: reflection-startup по детям `ILogicNode`, once/кеш

**Статус:** ✅ done

## Цель

`Initialize(bool buildManaged = true)` — собрать таблицу на startup из **плотных детей контекста `ILogicNode`**,
уже построенных генератором `TypeIndexer` (не сырой AppDomain-скан). Один раз/кеш (повтор — no-op). `buildManaged`
гейтит население managed-таблицы (см. plan.md).

## Шаги

1. `Initialize(bool buildManaged = true)`: idempotent-гард `if (_initialized) return;`.
2. `var children = IndexedTypes.GetContextChildren(typeof(ILogicNode));` — `TypeId[]`, index == dense ordinal.
   Если пусто (EditMode/IndexedTypes не init) → таблицы `Array.Empty`, `_count = 0`, `_initialized = true`, выход.
3. `_count = children.Length`. Под `#if` аллоцировать `_burst` размера `_count`. managed: `fillManaged = buildManaged`
   (под `#if UNITY` — по флагу; в .NET — единственный путь, но флаг честно соблюдаем); `_managed = fillManaged ?
   new ExecuteFn[_count] : Array.Empty<ExecuteFn>()`.
4. Закешировать `MethodInfo` `NodeInvoker.GetManaged<>` (и под `#if` `Compile<>`) один раз вне цикла.
5. Цикл `ordinal in 0.._count`: `Type t = IndexedTypes.GetType(children[ordinal])`; под `#if`
   `_burst[ordinal] = (FunctionPointer<ExecuteFn>) compileMI.MakeGenericMethod(t).Invoke(null, null)` (всегда —
   все типы unmanaged в C); `if (fillManaged) _managed[ordinal] = (ExecuteFn) getManagedMI.MakeGenericMethod(t).Invoke(null, null)`.
6. `_initialized = true`.
7. Комментарий (RU): интерим до кодогена M10; IL2CPP/AOT-риск `MakeGenericMethod`; C компилит Burst для всех
   (managed-тел нет — skip для `RuntimeType.Managed` вводится в M6-D).

## Done-criteria

- `Initialize()` собирает таблицу размера `TypeId<ILogicNode>.Count`, ordinal'ы совпадают с
  `TypeIdOf<ILogicNode,T>` (конструктивно — источник `GetContextChildren`).
- Повторный вызов — no-op. Пустой `IndexedTypes` обрабатывается без падения.

## Зависимости

- Задача 01 (хранилище). `IndexedTypes.GetContextChildren/GetType`, `TypeId<ILogicNode>.Count`.

## Заметки/находки

- **Ревью 2026-06-16:** `Initialize(bool)` → статический фабричный метод **`Build(bool buildManaged = true)`**,
  возвращающий инстанс `NodeFunctionRegistry`. Idempotent-гард не нужен (нет статики; caller строит раз и шарит).
  Burst-таблица строится в `UnsafeArray<FunctionPointer<ExecuteFn>>` (off-allocator). Прочее (источник типов —
  `GetContextChildren`, reflection `MakeGenericMethod`, гарды на `GetMethod`, IL2CPP-интерим) — без изменений.
