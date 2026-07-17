# Таска 04 — Tests + README

**Статус: ✅ done** — все 20 тестов сборки зелёные (13 новых Phase-2 + 7 Phase 0/1), batchmode PlayMode.

## Цель
Доказать Фазу 2 минимальными stub-нодами: sizing, выравнивание/non-overlap, lockstep, reset-семантика,
zero-size.

## Файлы
- `Tests/StubNode.cs` (новый)
- `Tests/LayoutTests.cs` (новый)
- `Tests/InstanceScopeTests.cs` (новый)
- `Tests/README.md` (правка — список тестов)

## Шаги
1. `StubNode : INode` — поле `DataSizes _sizes` (через ctor), `DataSizes => _sizes`. Портовые члены
   минимальны: `NodeTypeId => default`, `GetInputs/Outputs/Bodies/States => Array.Empty<...>()`,
   `SetBody`/`SetStateAndOutput` — no-op. Хелпер сборки `Blueprint` из stub-нод (id/version + пустые
   кеши связей).
2. `LayoutTests` (PlayMode, `CompiledBlueprint.CompileLayout` в `RawBumpAllocator`,
   `try/finally Dispose`):
   - `PerNodeSizesSumToBlockSizes`, `OffsetsAlignedAndNonOverlapping`, `AlignmentPadsSlots`,
     `LockstepReserveEqualsBump`, `ZeroSizeNodesLayoutCleanly`, `StaticSliceAddressableAndIsolated`.
3. `InstanceScopeTests` (PlayMode, `WorldManager.CreateWorld`, `try/finally Dispose`):
   - `CreateAllocatesBothBlocks`, `ResetCacheClearsCacheKeepsPersistent`, `ZeroSizeInstanceBlocksClean`,
     `DisposeFreesBlocks`.
4. `README` — дописать новые тесты.

## Done-criteria
- Все тесты зелёные (Test Runner / batchmode).
- Покрыт каждый пункт test-list Фазы 2 из PLAN.

## Зависимости
- Таски 01–03.

## Заметки / находки
- Тесты гоняют только scope-путь; legacy `SetupBlueprint`/порты не вызываются.
- Для lockstep сверять `arena.Value.UsedBytes - BumpHeader.HeaderSize` с `CalculateLayoutSizeToReserve`.
