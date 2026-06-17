# 01 — `ILogicNode.RuntimeType` (capability) + деривация `INode<T>`

**Статус:** ✅ done

## Цель

Поднять «managed-ность» на **logic-тип**, чтобы реестр мог решать Burst-skip по ordinal'у, а блоб
(`NodeHeader.runtimeType`) гарантированно совпадал с этим решением для `INode<T>`-нод.

## Шаги

1. `Logic/ILogicNode.cs`: добавить `RuntimeType RuntimeType { get; }` как **default interface member**
   (`=> RuntimeType.Unmanaged;`). Комментарий (RU): capability — может ли тело быть Burst-компилировано;
   читается реестром на сборке (managed-путь), не на горячем пути; `Managed` ⇒ Burst-skip (развилка 5).
2. `Blueprint/INode.cs`: в `INode<TLogicNode>` добавить explicit-DIM
   `RuntimeType INode.RuntimeType => ((ILogicNode)default(TLogicNode)).RuntimeType;` (рядом с существующей
   деривацией `NodeTypeId`). Комментарий-инвариант (RU): per-node `runtimeType` блоба выводится из logic-типа ⇒
   совпадает с тем, по чему реестр решал Burst-skip; рассинхрон ⇒ диспатч в пустой `FunctionPointer`.

## Done-criteria

- `ILogicNode` несёт `RuntimeType` с дефолтом `Unmanaged`; существующие stub-реализации компилируются без правок.
- `INode<T>.RuntimeType` возвращает `RuntimeType` своего `TLogicNode`.
- Компилируется в обе среды (DIM + boxing — кросс-средовые конструкции, без Burst-зависимостей).

## Зависимости

- `RuntimeType` (enum, `ExecutionGraph.cs`), `IIndexedType`/`TypeIdOf` (как в существующей деривации).

## Заметки/находки

- **Сделано.** `ILogicNode.RuntimeType` (DIM, дефолт `Unmanaged`); `INode<T>.RuntimeType` выводит из logic-типа через boxing. Stub-ноды компилируются без правок.
