# 03 — `BackendSelectionTests`: selection + managed-исполнение + форс + детерминизм

**Статус:** ✅ done

## Цель

Реально покрыть managed-путь и выбор бэкенда (без Burst/IndexedTypes); Burst-прогон и `Build`-skip — под
`Assert.Ignore`.

## Шаги (тесты)

1. **Stub-ноды** в тест-файле: `StubAdd` (Unmanaged, In+addend→Out), `StubManagedAdd`
   (`RuntimeType => Managed`, та же логика), `StubFloat` (float-математика для детерминизма), generic
   `GenNode<T> : INode<T>` (минимальные `GetInputs/GetOutputs`) для проверки деривации.
2. `Select_ManagedRuntimeType_UsesManaged` — `Create(managed).UseManaged(Managed) == true`.
3. `Select_UnmanagedRuntimeType_PerEnv` — `#if UNITY` `UseManaged(Unmanaged)==false`, `#else` `==true`.
4. `Select_ForceManaged_AlwaysManaged` — `Create(…, forceManaged:true)` ⇒ оба `UseManaged` == true.
5. `Invoke_ManagedNode_ExecutesManaged` — блоб + ручной `NodeContext`/`InstanceCache` (как в
   `NodeFunctionRegistryTests`); `Invoke(0, Managed, ctx)` ⇒ Out посчитан (5+100=105).
6. `Invoke_ForceManaged_RunsUnmanagedNodeManaged` — `forceManaged:true`; `Invoke(0, Unmanaged, ctx)` исполняет.
7. `Invoke_DeterministicAcrossRuns` — `StubFloat`: повтор `Invoke` и два инстанса ⇒ `Assert.AreEqual` побитово.
8. `RuntimeType_LogicTypeCapability` — `((ILogicNode)default(StubManagedAdd)).RuntimeType==Managed`;
   `((ILogicNode)default(StubAdd)).RuntimeType==Unmanaged`.
9. `RuntimeType_DerivedByGenericNode` — `((INode)new GenNode<StubManagedAdd>()).RuntimeType==Managed`.
10. `Build_SkipsBurstForManaged` — под `Assert.Ignore` если `TypeId<ILogicNode>.Count==0` (EditMode);
    иначе: `Build` даёт managed-делегат для Managed-типа и не падает на Burst-skip.

## Done-criteria

- Managed-исполнение, selection и деривация — **реальные** assert'ы (зелёные в plain managed).
- Burst/Build-зависимые от `IndexedTypes` — под `Assert.Ignore`, без падений.
- Все аллокации (`UnsafeArray`/`InstanceCache`/арена/реестр) освобождаются в `finally`.

## Зависимости

- Задачи 01–02. `StubBlueprint`/`StubNode`, `BlueprintCompiler.CompileLayout`, `InstanceCache`, `CacheHandler`.

## Заметки/находки

- **Сделано.** `BackendSelectionTests`: selection (UseManaged per-env+форс), реальное managed-исполнение (`Invoke`), форс гонит Unmanaged managed-путём, детерминизм (`Invoke_Managed_DeterministicAcrossRuns`), capability+деривация `RuntimeType`, Build-skip под `Assert.Ignore`.
