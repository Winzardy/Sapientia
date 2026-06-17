# 02 — `ExecutionScope` реестр + ptr-аксессоры + `GetNodeRuntimeType`

**Статус: ✅ done (на ревью)**

## Цель

Дать seam'у (Task 01) доступ к реестру и к памяти инстанса как к `SafePtr<...>`, и к `runtimeType` ноды из блоба.

## Шаги

1. `ExecutionScope`:
   - поле `private NodeFunctionRegistry _registry;` — **shared-копия, scope НЕ владеет** (см. ниже Dispose).
   - `Create(Id<MemoryManager> memoryId = default, int instanceCapacity = 8, NodeFunctionRegistry registry = default)`
     — реестр **последним** опц. параметром (call-site'ы без диспатча не ломаются); присвоить `_registry = registry`.
   - `public readonly NodeFunctionRegistry Registry => _registry;`
   - `public SafePtr<InstanceCache> GetInstanceCachePtr(BlueprintInstanceId id)` — `E.ASSERT(_instances.Has(id), …)`,
     `return _cache[id.id].AsSafePtr();` (адрес элемента off-allocator store'а; стабилен на время прогона).
   - `public SafePtr<InstancePersistence> GetInstancePersistencePtr(BlueprintInstanceId id)` — аналогично по `_memory`.
   - `Dispose`: **не** трогает `_registry` (владелец — тот, кто строил `Build`/`Create`). Комментарий-инвариант.
2. `CompiledBlueprintHeader`:
   - `public RuntimeType GetNodeRuntimeType(Id<NodeHeader> nodeId) => nodes.Get(nodeId).runtimeType;`
     (рядом с `GetNodeTypeId`; XML-doc: бэкенд исполнения ноды — для выбора таблицы реестром, M6-D/F).

## Done-criteria

- Существующие `ExecutionScope.Create(...)` call-site'ы (ExecutionScopeTests/ContextRegistryTests/InstanceScopeTests
  и т.п.) компилируются без правок (реестр опц. в хвосте).
- `GetInstance*Ptr` дают валидный указатель на элемент store'а; на мёртвом/stale хендле — DEBUG-assert.
- `Dispose` не диспозит реестр (нет двойного-free shared-таблиц).

## Зависимости

- Нет (база для Task 01/03).

## Notes / findings

_(заполняется по ходу)_
