# Collections — off-arena collection library (`Sapientia.Collections`)

> Parent: [Sapientia root](../CLAUDE.md). Siblings: [MemoryAllocator](MemoryAllocator.md) · [Arena Mem-collections](MemoryAllocator/Collections.md) · [State / World / Entity](MemoryAllocator/State.md).

## 1. Purpose

This is the **standalone, off-arena collection library** in namespace `Sapientia.Collections`. It is the **distinct cousin** of the allocator-backed `Mem*` collections documented in [Arena Mem-collections](MemoryAllocator/Collections.md): those live inside the serializable `WorldState` arena and take a `WorldState` on every call; **these do not**. This family splits into two worlds:

- **Managed (GC-heap) collections** — plain C# classes/structs over `T[]`, `Dictionary`, `HashSet`, often with `ArrayPool<T>` rental for low allocation churn (`SimpleList`, `SparseSet`, `HashMap`, `HashList`, `BidirectionalMap`, `CircularBuffer`, `Registry`, `EnumArray`, `Array`, …). They are **not** part of the world snapshot and are subject to GC.
- **Unsafe-native collections** — `unmanaged struct` containers (`UnsafeList<T>`, `UnsafeArray<T>`, `UnsafeSparseSet<T>`, `UnsafeDictionary`, `UnsafeHashSet`, `UnsafeBitArray`, `UnsafeIndexAllocSparseSet`) whose storage comes from a `MemoryManager` (`Id<MemoryManager>`), **not** the arena `Allocator`. They are GC-free but **must be `Dispose()`d** or they leak native memory.

Use these for **transient / tooling / view-side / cross-frame-managed** data that does not belong in the deterministic world snapshot. For simulation state that must serialize with the world, use the arena `Mem*` collections instead.

## 2. Where it lives

- **Folder:** `Assets/Submodules/Sapientia/Collections/` (~39 `.cs` files; subfolders `Unsafe/`, `HashMap/`, `FixedString/`, `Span/`, `Comparers/`, `Ext/`).
- **Assembly:** `Sapientia` (`../Sapientia.asmdef`, `allowUnsafeCode: true`).
- **Namespaces:** `Sapientia.Collections` (the bulk), `Sapientia.Collections.FixedString` (`FixedString/FixedString.cs:10`).
- **Note:** the arena equivalents live one level down under `../MemoryAllocator/Collections/` (namespace `Sapientia.MemoryAllocator`). The root [Sapientia doc §2](../CLAUDE.md) explicitly calls out `Sapientia.Collections` as the "off-allocator" namespace — do not confuse the two.

## 3. Key types & entry points

Real types present (verified by listing the folder):

**Managed list / array:**
- `SimpleList.cs:9` — `SimpleList<T>` (class, `IList<T>`, `IDisposable`). The workhorse growable list; optional `ArrayPool<T>` rental. **Start here.**
- `ReadOnlySimpleList.cs:7` — `ReadOnlySimpleList<T>` (readonly struct wrapping a `SimpleList<T>`; `IReadOnlyList<T>`).
- `SimpleListExt.cs:6` — `SimpleListExt` extension helpers.
- `Array.cs:11` — `Array<T>` (class) — pooled fixed-length array (rents from `ArrayPool<T>`). `Array.cs` also declares `ArraySection<T>` (`Array.cs:24`).
- `ArrayReference.cs:5` — `ArrayReference<T>` (struct) — `(T[], index)` pair exposing `ref readonly Value`.
- `EnumArray.cs:10` — `EnumArray<TEnum, TValue>` — array indexed by an `enum` (length = enum value count).
- `ListSegment.cs:6` — `ListSegment<T>` (readonly struct) — a `[start,end]` window over any `IReadOnlyList<T>` with a `ref`-less indexer.

**Managed set / map:**
- `SparseSet.cs:9` — `SparseSet<T>` (class, `IDisposable`) — dense+sparse set keyed by integer id; pooled `_values`/`_dense`/`_sparse` arrays.
- `IndexAllocSparseSet.cs:8` — `IndexAllocSparseSet<T>` (class, `IDisposable`) — `SparseSet<T>` + a `Stack<int>` id recycler that allocates/frees its own ids.
- `HashMap/HashMap.cs:10` — `HashMap<TKey, TValue>` (sealed partial class, `where TValue : struct`) — `Dictionary<TKey,int>` index + `SimpleList<TValue>` values, giving `ref TValue` access. Partials: `HashMap.IEnumarable.cs`, `HashMap.Newtonsoft.cs` (JSON).
- `HashList.cs:12` — `HashList<T>` (class) — a `List<T>` + `HashSet<T>` combo for O(1) `Contains`/dedup with stable order; raises `Added`/`Removed` events.
- `BidirectionalMap.cs:8` — `BidirectionalMap<TFirst,TSecond>` (class, `IDisposable`) — two mirrored `Dictionary`s (pooled via `DictionaryPool`).
- `Registry.cs:7` — `Registry<T>` (abstract class) — a `HashSet<T>` with `Register`/`Unregister` events and subscribe helpers.

**Managed buffer:**
- `CircularBuffer.cs:7` — `CircularBuffer<T>` (struct, `IDisposable`) — pooled ring buffer (FIFO/deque), grows by `_expandStep`.

**Span / fixed:**
- `Span/SpanList.cs:5` — `SpanList<T>` (`ref struct`, `where T : unmanaged`) — append-only view over a caller-owned `Span<T>` (stack buffer); **no allocation, no resize**.
- `FixedString/FixedString.cs` — `FixedString32Bytes` (`:210`), `FixedString64Bytes` (`:1070`), `FixedString128Bytes` (`:1956`), `FixedString512Bytes` (`:2961`), `FixedString4096Bytes` (`:5087`) — inline fixed-capacity UTF-8 strings (no heap). `IFixedString.cs:6` interface; `Unicode.cs` UTF-8 codec; `FixedString/Ext/` append/format helpers. This is a Unity.Collections-style `FixedString` port.

**Unsafe-native (`unmanaged struct`, backed by `MemoryManager`):**
- `Unsafe/UnsafeList.cs:11` — `UnsafeList<T>` (`IDisposable`, `where T : unmanaged`) — growable native list; `SafePtr<T> ptr`, `count`, `capacity`, `Id<MemoryManager> memoryId`.
- `Unsafe/UnsafeArray.cs:22` — `UnsafeArray<T>` (`IDisposable`) — fixed-length native array; declares the `ResizeSettings` enum (`UnsafeArray.cs:11`).
- `Unsafe/UnsafeSparseSet.cs:11` — `UnsafeSparseSet<T>` — native sparse set (three `UnsafeArray<T>`/`<int>`).
- `Unsafe/UnsafeIndexAllocSparseSet.cs:11` — `UnsafeIndexAllocSparseSet<T>` — native sparse set with id allocation.
- `Unsafe/UnsafeDictionary.cs:20` — `UnsafeDictionary<TKey,TValue>` (`IDisposable`) — open-addressing native dictionary (`Entry` buckets, `:24`).
- `Unsafe/UnsafeHashSet.cs:14` — `UnsafeHashSet<T>` (`IDisposable`, `where T : unmanaged, IEquatable<T>`) — native hash set (`Slot` buckets, `:17`).
- `Unsafe/UnsafeBitArray.cs:14` — `UnsafeBitArray` (`IDisposable`) — packed native bit set (`ulong` words).
- `Ext/UnsafeListExt.cs:6` — `UnsafeListExt` helpers.

**Comparers / extensions:**
- `Comparers/DefaultComparer.cs:6` — `DefaultComparer<T>` (struct `IComparer<T>`, `where T : IComparable<T>`).
- `Comparers/LambdaComparer.cs:6` — `LambdaComparer<T>` (struct `IComparer<T>` wrapping a delegate).
- `Ext/ArrayExt.cs:11` (`Move`/`Expand_WithPool`/…), `Ext/SpanExt.cs:6`, `Ext/DictionaryExt.cs:9`, `Ext/HashSetExt.cs:5`, `Ext/CollectionsExt.cs:14`.

## 4. Layer this provides

A **general-purpose collection layer** that is deliberately **outside** the world/arena model. There is no Data/State/Logic/View split. The dividing line is allocation backing:

- **Managed** types store ordinary `T[]` / BCL `Dictionary` / `HashSet` references and live on the GC heap. Many rent their arrays from `System.Buffers.ArrayPool<T>` to cut churn (e.g. `SimpleList.cs:79`, `SparseSet.cs:63`, `CircularBuffer`, `Array<T>` at `Array.cs:39`).
- **Unsafe** types store a `SafePtr<T>` + an `Id<MemoryManager>` and sub-allocate from a `MemoryManager` via `memoryId.GetManager().MakeArray<T>(…)` (`Unsafe/UnsafeList.cs:35`, `Unsafe/UnsafeArray.cs:44`). The `default` `memoryId` resolves to the global Default manager (`../Memory/MemoryManager.cs:53`); growth/free go through `ResizeArray`/`MemFree` (`Unsafe/UnsafeList.cs:145,154`). **This is the `MemoryManager`, not the arena `Allocator`** — so these containers are GC-free but are *not* part of the `WorldState` serialization blob.

Contrast with the arena `Mem*` family: those embed a `CachedPtr` (`MemPtr`) into arena memory, require a `WorldState` per call, and serialize implicitly with the world — see [Arena Mem-collections](MemoryAllocator/Collections.md) §4–§5. Pick `Mem*` for simulation state; pick this family for tooling/view/transient buffers.

## 5. Lifecycle & resize semantics

- **Construct:** managed types take a capacity (and often an `isRented`/pool flag). `SimpleList` defaults to `DEFAULT_CAPACITY = 8` and `isRented = true` (`SimpleList.cs:13,76`). Unsafe types take an optional `Id<MemoryManager> memoryId` (default = global Default manager) plus capacity (`UnsafeList.cs:26`, `UnsafeArray.cs:42`).
- **Grow (managed):** `SimpleList.Expand` rents a larger pooled array and copies (`SimpleList.cs:392`); if the list was non-rented it switches to rented on first growth (`SimpleList.cs:399-402`). `SparseSet` rounds new capacity up to a multiple of `expandStep` via `SnapCeilCapacity` (`SparseSet.cs:194`) and grows dense+value arrays together (`SparseSet.cs:178`), sparse separately (`SparseSet.cs:156`). `CircularBuffer` grows by `_expandStep`.
- **Grow (unsafe):** `UnsafeList.EnsureCapacity` calls `MemoryManager.ResizeArray` which **reallocates and may move `ptr`** (`UnsafeList.cs:145`). `UnsafeArray.Resize` allocates a brand-new `UnsafeArray`, copies per `ResizeSettings` (`CopyOldValues`/`ClearMemory`/`UninitializedMemory`), disposes the old, and reassigns `this` (`UnsafeArray.cs:91-113`) — the old `ptr` is freed.
- **Clear vs free (managed):** `SimpleList` has `Clear()` (fills with default + count=0, `SimpleList.cs:425`), `ClearPartial()` (`:432`), and `ClearFast()` (count only, `:441`). `SparseSet` has `Clear()` (`:239`) and `ClearFast()` (`:245`). **`ClearFast` does not zero — stale references linger in the backing array.**
- **Dispose (managed, pooled):** `SimpleList.Dispose` returns the rented array to `ArrayPool<T>.Shared` and nulls it (`SimpleList.cs:447`); it has a finalizer `~SimpleList` calling `Dispose` (`:474`). `SparseSet.Dispose(clearArray)` returns all three pooled arrays (`SparseSet.cs:224`). `Array<T>` and `BidirectionalMap` similarly return pooled storage on `Dispose` (`Array.cs:28`, finalizer `Array.cs:26`).
- **Dispose (unsafe, native):** `UnsafeList.Dispose` / `UnsafeArray.Dispose` / `UnsafeBitArray.Dispose` call `memoryId.GetManager().MemFree(ptr)` and reset to `default` (`UnsafeList.cs:149-156`, `UnsafeBitArray.cs:143-151`). **There is no finalizer on the unsafe structs — forgetting `Dispose` leaks native memory** (see §7).
- **Serialize:** these collections are **not** part of the arena snapshot. `HashMap` has explicit Newtonsoft JSON support (`HashMap/HashMap.Newtonsoft.cs`); `FixedString`/`EnumArray` are `[Serializable]`. Unsafe native buffers have no built-in world serialization here.

## 6. Dependencies

- **Depends-on:**
  - Managed: BCL `System.Buffers.ArrayPool<T>`, `Dictionary`/`HashSet`/`List`, `Sapientia.Pooling` (`DictionaryPool` in `BidirectionalMap.cs:4`), `Sapientia.Extensions` (`EnumValues<TEnum>` in `EnumArray.cs:5,12`), the local `Ext/` and `Comparers/`.
  - Unsafe: `Sapientia.Data.SafePtr<T>`, `Submodules.Sapientia.Memory.MemoryManager` / `MemoryExt` (`MemMove`/`MemCopy`/`MemClear`/`MemFill`), `Id<MemoryManager>` (`Unsafe/UnsafeList.cs:4-6`). These reuse the same low-level `SafePtr`/`MemoryExt` primitives as the allocator core, but go through `MemoryManager`, **not** `Allocator` ([MemoryAllocator](MemoryAllocator.md) §4).
- **Depended-by:** per the root doc, `UnsafeArray`/`UnsafeList`/`SimpleList` are used widely as off-arena scratch — e.g. the `Allocator` itself holds `UnsafeList<MemoryZone>` / `UnsafeList<MemoryBlockPtrCollection>` (`../MemoryAllocator/Allocator/Core/Allocator.cs`, see [MemoryAllocator](MemoryAllocator.md) §3), and `UnsafeIndexedRegistry` uses off-arena `UnsafeArray` (noted in [Arena Mem-collections](MemoryAllocator/Collections.md) §6). `SimpleList`/`HashMap`/`HashList` are general utility types used across `Game.Core` tooling/view code (best-effort; full consumer list `unknown`).

## 7. Gotchas & invariants

- **Native leak hazard (the #1 rule for this folder).** `UnsafeList<T>`, `UnsafeArray<T>`, `UnsafeSparseSet<T>`, `UnsafeIndexAllocSparseSet<T>`, `UnsafeDictionary`, `UnsafeHashSet`, `UnsafeBitArray` allocate from a `MemoryManager` and have **no finalizer**. If you do not call `Dispose()`, the native block is leaked — the GC cannot reclaim it. Always own them with a clear lifetime (e.g. `using`, or an owner that disposes in its own teardown).
- **Managed pooled types must be Disposed too — or they finalize.** `SimpleList`, `SparseSet`, `Array<T>`, `BidirectionalMap`, `CircularBuffer` rent from `ArrayPool<T>.Shared`. Not disposing them does not crash (GC reclaims the array, and `SimpleList`/`Array<T>` have finalizers), but the rented buffer is **never returned to the pool**, defeating the pooling. `WrapArray`/non-rented ctors are exempt (`isRented=false`, `SimpleList.cs:121-128`).
- **Resize/realloc invalidates raw pointers (unsafe).** `UnsafeList.EnsureCapacity` (`UnsafeList.cs:145`) and `UnsafeArray.Resize` (`UnsafeArray.cs:91`) reallocate; any `SafePtr<T>`/`ref`/`Span<T>` captured before an `Add`/`Insert`/`EnsureCapacity`/`Resize` is **dangling** afterward (no release-build guard — same pointer-staleness class as the arena, see [MemoryAllocator](MemoryAllocator.md) §7). Re-fetch `ptr` after any growth.
- **Reallocation invalidates managed `ref`/`Span` too.** `SimpleList.this[int]` returns `ref T` into `_array`, and `AsSpan()` returns a `Span<T>` over `_array` (`SimpleList.cs:51,479`). Any `Add`/`Insert`/`Expand` may swap `_array` for a new pooled buffer — a previously taken `ref`/`Span` then points at the old (possibly already pool-returned) array. Do not hold a `ref`/`Span` across a mutation that can grow.
- **Enumeration vs mutation.** `SimpleList.Enumerator` snapshots the list reference and walks by index against the live `_count` (`SimpleList.cs:519-537`); `SparseSet.Enumerator` indexes `_values[0.._count]` (`SparseSet.cs:265`). Adding/removing during a `foreach` is undefined (index/`_count` skew, and for grow-on-add the backing array may have been swapped). Snapshot or iterate by index with re-checked bounds.
- **`ClearFast` / `*NoCheck` leave stale data.** `SimpleList.ClearFast` (`:441`) and `SparseSet.ClearFast` (`:245`) only reset the count — old elements (incl. managed references that pin GC objects) stay in the array until overwritten. `SimpleList.AddWithoutExpand` (`:131`) skips the capacity check; only safe with guaranteed capacity.
- **`HashMap` `ref` indexer auto-inserts.** `HashMap.this[key]` calls `GetOrAdd` on a miss (`HashMap/HashMap.cs:25-26`) — reading a missing key mutates the map (adds a default entry). `where TValue : struct` is required so it can hand back `ref` into the `SimpleList<TValue>` value store.
- **`EnumArray` reinterprets the enum as `int`.** `GetIndexOf` does `*(int*)(&enumValue)` (`EnumArray.cs:35-38`) — assumes the enum's underlying type is 4 bytes and that values are dense `[0, ENUM_LENGHT)`; non-`int` backing or sparse/explicit enum values will mis-index.
- **`SpanList<T>` is a stack-only view with no growth.** It writes into a caller-supplied `Span<T>`; `Add` will throw `IndexOutOfRange` past capacity (`Span/SpanList.cs:19-22`). It is a `ref struct` — cannot be boxed, stored in a field, or captured in a closure/async.
- **`FixedString` is inline & fixed-capacity.** Appending past the byte capacity is rejected (logs a warning, `FixedString/FixedString.cs:516` and siblings); these are value types with no heap allocation — good for keys/ids in unmanaged contexts.
- **No LINQ / determinism.** Per `CONVENTIONS.md`, the Unity client bans `System.Linq`; iterate with the provided enumerators or index loops. Managed collections here are GC-allocating by nature, so keep them off hot deterministic-simulation paths (use the arena `Mem*` family there).
- **Not world-serializable.** None of these participate in the arena snapshot. Do not use them to hold authoritative simulation state that must survive a world save/load — that is what the arena `Mem*` collections are for.

## 8. Open questions / TODO / risks

- `TODO` markers found in this folder:
  - `FixedString/Ext/FixedStringExt.Append.cs:61` — `// TODO: implement math.ldexpf (which presumably handles denormals (i don't))` (float formatting edge case).
  - `FixedString/Unicode.cs:516-518` — three-line `// TODO …` about MemCpy-ing all-but-last-3 bytes when truncating UTF-8 (carried over from the upstream Unicode codec).
  - `Ext/CollectionsExt.cs:177` and `Ext/CollectionsExt.cs:192` — `//TODO:CollectionsMarshal.AsSpan() в .NET 5+` (Russian note: could use `CollectionsMarshal.AsSpan` on `.NET 5+`).
- No `BUG`/`HACK`/`FIXME` markers found in this folder (the other `BUG`/`DEBUG` grep hits are `[DebuggerTypeProxy]`/`#if DEBUG`/`Conditional("DEBUG")`, not defect markers).
- `SimpleList.ExtractLast` is `[Obsolete("Use RemoveLast instead")]` (`SimpleList.cs:258`).
- `SimpleList.Remove<T1>` calls `GC.SuppressFinalize(value)` on the *not-found* path (`SimpleList.cs:354`) — looks suspicious (suppressing a finalizer for a value that was not removed); behavior unverified, flag for review.
- `ArrayReference.cs:23` carries a Russian comment ("Есть ArraySegment, но у него доступ по индексу не ref!" — *ArraySegment exists, but its indexer is not `ref`*) explaining why `ArraySection<T>` exists — legacy team note, kept as-is.
- Exact resize/rehash thresholds and whether any of these ever **shrink** their backing storage were not exhaustively traced; they appear grow-only like the arena family — `unknown`, verify before relying on capacity math.
- Full `Depended-by` consumer map across `Game.Core` was not enumerated — `unknown`.
