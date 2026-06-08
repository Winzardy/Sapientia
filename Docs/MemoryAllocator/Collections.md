# Collections — Mem-collections (allocator-backed)

> Parent: [Sapientia root](../../CLAUDE.md). Siblings: [MemoryAllocator](../MemoryAllocator.md) · [State / World / Entity](State.md).

## 1. Purpose

The `Mem*` collections are the **allocator-backed container library**: `unmanaged` struct collections whose backing storage lives in the `Allocator` arena ([MemoryAllocator](../MemoryAllocator.md)) and is addressed by `MemPtr`/`CachedPtr`. They are the data structures every StatePart, component, and service uses to hold simulation state — so they inherit the no-GC, deterministic, serializable properties of the arena. They are *not* `System.Collections` replacements you can use casually: nearly every method takes a `WorldState` (or `Allocator`) argument because the collection struct itself stores only handles, not pointers.

## 2. Where it lives

- **Folder:** `Assets/Submodules/Sapientia/MemoryAllocator/Collections/` (plus `MemArray/` and `Enumerators/` subfolders).
- **Assembly:** `Sapientia` (`../../Sapientia.asmdef`).
- **Namespace:** `Sapientia.MemoryAllocator`.
- **Note:** off-arena collections (`UnsafeList`, `SimpleList`, `UnsafeArray`) live under `../../Collections/` (the submodule-root `Collections` folder, namespace `Sapientia.Collections`) — those use `MemoryManager` directly, not the arena. Don't confuse the two.

## 3. Key types & entry points

Real types present in this folder (verified by listing):

- `MemArray/MemArray.cs:12` — `MemArray` (untyped, `elementSize`-based) and `MemArray/MemArray.Generic.cs:12` `MemArray<T>` — fixed-length arena array; the building block under most others. **Start here.**
- `MemList.cs:12` — `MemList<T>` — growable list (a `MemArray<T>` + `_count`).
- `MemSparseSet.cs:11` — `MemSparseSet` (untyped) and `MemSparseSet.Generic.cs` `MemSparseSet<T>` — dense+sparse set keyed by integer id; **the storage behind `ComponentSet`**.
- `MemIndexAllocSparseSet.cs:8` — `MemIndexAllocSparseSet<T>` — sparse set that also allocates/recycles its own ids.
- `MemDictionary.cs:18` — `MemDictionary<TKey, TValue>` — open-addressing hash dictionary (`Entry` buckets).
- `MemHashSet.cs:11` — `MemHashSet<T>` — hash set (`Slot` buckets).
- `MemQueue.cs:7` — `MemQueue<T>` — ring-buffer FIFO queue.
- `MemStack.cs:8` — `MemStack<T>` — LIFO stack.
- `MemLinkedList.cs:9` — `MemLinkedList<T>` (+ `MemLinkedListNode<T>` `:415`, `MemLinkedListNodeData<T>` `:395`) — intrusive linked list of arena nodes.
- `MemBitArray.cs:8` — `MemBitArray` — packed bit set.
- `MemCollectionsExt.cs` / `MemListExt.cs` / `MemArray/MemArrayExt.cs` — extension helpers (copy, fill, search).
- `Enumerators/` — `MemListEnumerators.cs`, `MemDictionaryEnumerators.cs`, `MemHashSetEnumerators.cs`, `MemLinkedListEnumerators.cs`, `MemCircleListEnumerator.cs` — `ref struct` enumerators.

## 4. Layer this provides

The **collection layer** between the raw arena and the State layer. No Data/State/Logic/View split:
- Each `Mem*` struct stores **handles** (`MemArray` holds a `CachedPtr ptr`, `Length`, `ElementSize`; `MemArray.cs:16`), not raw pointers — so the struct is blittable and copies cheaply, but you must pass `WorldState` to read/write.
- `MemSparseSet` composes three `MemArray`s: `_values` (untyped), `_dense` (`MemArray<int>`), `_sparse` (`MemArray<int>`) (`MemSparseSet.cs:15`). `_sparse[id] → dense index`; iteration is over the dense `[0, Count)` range.

## 5. Lifecycle & resize semantics

- **Construct:** ctors take a `WorldState` and a capacity; the parameterless/Unity ctors default to `WorldManager.CurrentWorldState` (e.g. `MemArray.cs:42`, `MemList.cs:34`) — convenient but relies on the thread-static current world being set.
- **Allocate:** ctor calls `worldState.MemAlloc(elementSize * length, out ptr)` and wraps the result in a `CachedPtr` (`MemArray.cs:52`). `ClearOptions.ClearMemory` zeroes it; `UninitializedMemory` does not.
- **Grow:** `MemArray.Resize` (`MemArray.cs:186`) calls `MemReAlloc` — **which may move the block** — and rebuilds the `CachedPtr`. `MemList.EnsureCapacity` rounds the new capacity up to the **next power of two** (`MemList.cs:146`) before resizing. Resize only ever grows (`newLength <= Length` returns false / no-op).
- **Clear:** `MemList.Clear()` just sets `_count = 0` (`MemList.cs:116`) — it does **not** free or zero memory.
- **Dispose:** `Dispose(worldState)` frees the backing `MemPtr` and resets the struct to `default` (`MemArray.cs:124`). Collections are not auto-freed — owners (StateParts) must dispose them, or they live until the arena is cleared/disposed.
- **Serialize:** collections serialize implicitly as part of the whole arena blob (their data is in arena memory); their `CachedPtr` re-resolves on load via the `WorldState.Version` check.

## 6. Dependencies

- **Depends-on:** `Allocator` / `WorldState` alloc API, `CachedPtr`, `SafePtr`, `MemoryExt` (`MemMove`/`MemCopy`/`MemClear`/`MemFill`), `TSize<T>`.
- **Depended-by:** `ComponentSet` (`MemSparseSet`), `EntityStatePart` (`MemArray<ushort>`, `MemSparseSet<Entity>`), `WorldElementsService` (`MemList<ProxyPtr<…>>`), `UnsafeIndexedRegistry` (off-arena `UnsafeArray`), and essentially every gameplay StatePart.

## 7. Gotchas & invariants

- **Resize invalidates raw pointers.** Any `Add`/`Insert`/`EnsureCapacity`/`Resize`/`EnsureGet` that grows the collection calls `MemReAlloc` and **may relocate the backing array**. A `SafePtr<T>`, `ref T`, or `Span<T>` you grabbed *before* the growth is now dangling (no guard in release). Re-fetch after any mutation that can grow. This is the most common allocator bug class.
- **Enumerators capture a raw pointer + count at creation.** `MemListEnumerator<T>` stores a `SafePtr<T> _valuePtr` and `_count` snapshot (`Enumerators/MemListEnumerators.cs:24`). **Adding/removing during a `foreach` is undefined**: the captured pointer can go stale (relocation) and `_count` is frozen. Never mutate a collection while enumerating it; snapshot to a local list or iterate by index with bounds re-checked.
- **`ComponentSet` capacity warnings.** `MemSparseSet`-backed component sets log a `LogWarning` when they expand (`../State/ComponentSets/ComponentSet.cs:417`) — that means the pre-reserved capacity (set in the StatePart's `Initialize`, e.g. `../State/StateParts/Destroy/State/DestroyStatePart.cs:7`) was too small. Honor "pre-reserve capacity"; expansion both reallocs and is a perf smell.
- **`Clear()` ≠ free.** `MemList.Clear` only resets the count; capacity and memory stay. Use `Dispose` to release.
- **Current-world reliance.** The parameterless ctors use `WorldManager.CurrentWorldState`; if no world scope is active the collection is created in the wrong/invalid world. Prefer the explicit `WorldState` ctors in library code.
- **DEBUG world-id checks.** `MemArray` stores a `_worldId` under `DEBUG` and asserts every access belongs to the world it was created in (`MemArray.cs:33`). A "wrong world" assert means a collection leaked across worlds. These checks vanish in release.
- **Determinism / no LINQ.** Iterate with the provided `ref struct` enumerators or index loops; do not pull these into `System.Linq` (allocations + the client bans LINQ — see `CONVENTIONS.md`).
- **Struct field layout.** `MemArray` is `[StructLayout(LayoutKind.Sequential)]` and embeds `CachedPtr` (also sequential); these are serialized as part of the arena, so do not reorder fields.

## 8. Open questions / TODO / risks

- No `TODO`/`BUG`/`HACK`/`FIXME` markers found in this folder.
- `MemDictionary`/`MemHashSet` resize/rehash thresholds and collision strategy are not fully audited here — `unknown` whether they shrink (they appear grow-only like the rest).
- `MemSparseSet` separates value `Capacity` from `FreeIndexesCapacity`/`_sparseCapacity` (`MemSparseSet.cs:35`); the exact growth coupling between dense and sparse arrays on expansion was not exhaustively traced — verify before relying on `Capacity` math.
- Several collections have `*NoCheck` fast paths (e.g. `MemList.SetCountNoCheck`, `MemList.cs:169`) that skip capacity/bounds checks — only safe when the caller has guaranteed capacity.
