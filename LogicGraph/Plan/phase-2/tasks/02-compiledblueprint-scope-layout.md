# Таска 02 — CompiledBlueprint scope layout

**Статус: ✅ done**

## Цель
Разложить все 5 областей в скомпилированном блюпринте: размер блока каждой области + пер-нодовые офсеты;
реально аллоцировать `static`-блок в raw-арене. Lockstep `CalculateLayoutSizeToReserve` ⟷ `SetupLayout`.

## Файлы
- `Logic/StaticData/CompiledBlueprint.cs` (правка — секция «Phase 2: 5-scope layout»)
- `Memory/BumpAllocator/BumpHeader.cs` (правка — `UsedBytes`)

## Шаги
1. `BumpHeader`: `public int UsedBytes => _rover.byteOffset;` (read-only).
2. Поля в `CompiledBlueprint`: `DataSizes blockSizes; BumpArray<NodeLayoutOffsets> nodeLayoutOffsets;
   PtrOffset staticBlock;` + `const int LayoutAlignment = 8`.
3. `CalculateLayoutSizeToReserve(Blueprint)` — `TSize<CompiledBlueprint>` (1) + (если нод > 0)
   `TSize<NodeLayoutOffsets> * nodesCount` (2) + (если static-блок > 0) `Σ AlignUp(node.DataSizes[Static],
   LayoutAlignment)` (3). Зеркалит guard'ы `SetupLayout`.
4. `CompileLayout(Blueprint, out PtrOffset<CompiledBlueprint>)` → `RawBumpAllocator`: резерв =
   `CalculateLayoutSizeToReserve`; `MemAlloc<CompiledBlueprint>`; `CreateRelativeOffset`; `SetupLayout`.
5. `SetupLayout(Blueprint)`: записать `version`/`id` (ключ static-блока). Если нод 0 — выйти.
   Аллоцировать таблицу офсетов (2). Двойной цикл `по областям × по нодам`: офсет ноды = текущий rover,
   rover += `AlignUp(size, LayoutAlignment)`; в конце области `blockSizes[scope] = rover`. Если
   `blockSizes[Static] > 0` — аллоцировать `staticBlock` (3).
6. Аксессоры: `GetBlockSize(DataLayout)`, `GetNodeOffset(NodeId, DataLayout)` (читает таблицу),
   `GetStaticNodeSlice(NodeId)` (`staticBlock + GetNodeOffset(.., Static)` → `SafePtr`).

## Done-criteria
- Компилируется; legacy-члены целы.
- На stub-графе: `blockSizes`/офсеты верны, выровнены, не перекрываются; `static`-слайсы адресуемы;
  lockstep сходится; zero-size без ассертов.

## Зависимости
- Таска 01.

## Заметки / находки
- `BumpHeader.MemAlloc` ассертит `size > 0` → guard'ы на 0 обязательны и должны быть в обоих методах.
- `RawBumpAllocator(reservedSize)` сам добавляет `HeaderSize`; `_rover` стартует с `HeaderSize`. Lockstep-
  тест: `UsedBytes - BumpHeader.HeaderSize == CalculateLayoutSizeToReserve`.
