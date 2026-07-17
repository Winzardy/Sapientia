# 03 — `NodeDispatchTests` (интеграционный прогон, real/managed)

**Статус: ✅ done (на ревью)**

## Цель

Доказать end-to-end run-путь: `ExecutionGraph.Drain`-порядок → `NodeInvoker.Run`/`Invoke` → резолв памяти из
`ExecutionScope` → dispatch через реестр → тела нод реально считают и проброшенная память меняется.

## Подход

- Реестр: `NodeFunctionRegistry.Create(managed[], forceManaged: true)` ⇒ managed-путь исполняется **реально** в
  EditMode/.NET (Burst/`IndexedTypes` не нужны; `forceManaged` гонит и Unmanaged-ноды managed-таблицей ⇒ зелено и
  под Unity-эдитором). `managed[ordinal] = NodeInvoker.GetManaged<TBody>()`.
- Ноды: `StubNode(staticSize, cacheSize, inputs, outputs, typeId: (TypeId<ILogicNode>)i)` — явный ordinal ⇒
  слот реестра под ноду. Связи — через `bp.inputToOutput[in] = out` (как в `ExecutionGraphTests`).
- Тела (`struct : ILogicNode`): читают/пишут Cache через `CacheHandler<long>`, собранные **из Map блоба**
  (`GetNodeInOut(nodeId)` → `RegionPtr` по индексу порта → `cacheData` ordinal → `CacheHandler{ cell = ordinal*sizeof(CacheLink) }`).
  Тело кладём в static-слайс ноды вручную (авто-бейк handle'ов — M9): `GetStaticNodeSlice(nodeId).Cast<TBody>().Value() = …`.
- Инстанс: `scope.CreateInstance(ref storage, bp.blueprintKey)` (реальный cache/persistence по размерам блоба).
  Граф: `ExecutionGraph.Inject(ref compiled, instanceId)`.
- Прогон: `NodeInvoker.Run(ref scope, ref compiled, ref graph, order)`; ассерты — по `scope.GetInstanceCache(id).Read(...)`.

## Тесты

| Тест | Что |
|---|---|
| `Invoke_SingleNode_ResolvesMemoryAndDispatches` | одна нода; seed In-кеша; `NodeInvoker.Invoke` напрямую; Out посчитан (NodeContext собран из scope + dispatch через `scope.Registry`) |
| `Run_Chain_ExecutesInDependencyOrder` | A→B→C; каждое тело: read In, +k, write Out; после `Run` C-Out = накопленное (порядок + проброс) |
| `Run_Diamond_JoinSeesBothBranches` | A→B,C→D; D пишет сумму обоих входов ⇒ D исполнена после B,C |
| `Run_MultiInstanceSameBlueprint_IndependentMemory` | 2 инстанса; `Inject` обоих; один `Run`; кеши независимы, оба посчитаны (резолв по `NodeInstanceId.blueprintId`) |
| `Run_PersistenceNode_PersistsAcrossRuns` | нода инкрементит `ctx.PersistenceSlice().Cast<long>()`; 2× `Run` ⇒ ==2 (persistence проброшен + переживает reset кеша). Узел с `persistanceSize >= 8` |
| `Run_ResetsCacheEachRun_Deterministic` | 2× `Run` ⇒ побитово равный результат (reset + детерминизм) |

## Done-criteria

- Все 6 тестов — real (без `Assert.Ignore`); `finally`-Dispose всех арен/scope/graph/registry/storage (нет утечек).
- Handle'ы берутся из Map (без хардкода ordinal'ов).

## Зависимости

- Task 01 + 02 (seam + аксессоры).

## Notes / findings

_(заполняется по ходу)_
