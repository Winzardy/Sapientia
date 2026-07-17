# 02 — Реестр: Burst-skip для `Managed` + `_forceManaged` + selection seam

**Статус:** ✅ done

## Цель

Закрыть развилку 5 в `NodeFunctionRegistry`: на сборке пропускать Burst для managed-тел, дать глобальный
managed-форс и **selection seam** (`UseManaged`/`Invoke`), исполняющий ноду по выбранной таблице.

## Шаги

1. **`Build(bool buildManaged = true, bool forceManaged = false)`:**
   - `_managed` аллоцировать при `count>0` **всегда** (слоты для `Managed`-нод даже при `buildManaged:false`).
   - В цикле: `rt = ((ILogicNode)Activator.CreateInstance(t)).RuntimeType`.
   - Под `#if UNITY`: `_burst[ordinal]` компилить **только если `rt == Unmanaged`** (Managed → оставить
     `default`, иначе `CompileFunctionPointer` упадёт на managed-теле).
   - `_managed[ordinal]` заполнять если `buildManaged || rt == Managed`.
   - Сохранить `_forceManaged = forceManaged`.
2. **`Create(…, bool forceManaged = false)`:** добавить параметр, сохранить в `_forceManaged`.
3. **`UseManaged(RuntimeType rt)`** (readonly): под `#if UNITY` `return _forceManaged || rt == Managed;`,
   в `#else` `return true;` (Burst-таблица вырезана `#if`).
4. **`Invoke(int ordinal, RuntimeType rt, ref NodeContext ctx)`** (readonly): под `#if UNITY`
   `if (!UseManaged(rt)) { _burst[ordinal].Invoke(ref ctx); return; }`; затем `_managed[ordinal].Invoke(ref ctx)`.
5. Комментарии (RU): что `buildManaged` теперь = «заполнять managed и для Unmanaged-нод»; что `forceManaged`
   требует населённой managed-таблицы; что `Invoke` исполняет **одну** ноду (прогон `Drain` — M6-F).

## Done-criteria

- `Build` не вызывает `Compile<T>()` для `Managed`-типов, но строит для них managed-делегат.
- `_forceManaged` шарится копией инстанса; `UseManaged`/`Invoke` соблюдают его и `runtimeType`.
- В чистом .NET (`#else`) `Invoke` всегда идёт managed-путём; код компилируется без `Unity.Burst`.

## Зависимости

- Задача 01 (`ILogicNode.RuntimeType`). `NodeInvoker.Compile/GetManaged`, `IndexedTypes`, `FunctionPointer`.

## Заметки/находки

- **Сделано.** Build: `buildManaged|=forceManaged`, `_managed` при `count>0`, per-type `isManaged` через `Activator.CreateInstance`, Burst-skip для Managed, managed-делегат для Managed безусловно. `_forceManaged`+`ForceManaged`, `Create(...,forceManaged)`, `UseManaged`/`Invoke` (+DEBUG-`E.ASSERT` на выбранный слот — по ревью).
