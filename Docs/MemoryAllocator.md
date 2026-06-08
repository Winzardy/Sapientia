# MemoryAllocator — Allocator deep-dive

> The single most important foundation doc. Every reviewer relies on these invariants.
> Parent: [Sapientia root](../CLAUDE.md). Siblings: [Collections](MemoryAllocator/Collections.md) · [State / World / Entity](MemoryAllocator/State.md).

## 1. Purpose

`Allocator` is a custom **arena allocator**: it owns one or more large contiguous **zones** of native memory and carves them into **blocks** using a buddy-style, power-of-two free-list. All simulation state (every `Mem*` collection, every StatePart, every component) is allocated out of this single arena, which is what makes the whole world **serializable as one blob** and **GC-free**. Memory is addressed by **`MemPtr`** (a stable `(zoneId, zoneOffset)` handle) and dereferenced through **`SafePtr`** (a raw, optionally bounds-checked pointer).

## 2. Where it lives

- **Folder:** `Assets/Submodules/Sapientia/MemoryAllocator/Allocator/` (core), plus `Data/SafePtr.cs` and `Memory/` one level up in the submodule.
- **Assembly:** `Sapientia` (`../Sapientia.asmdef`).
- **Namespaces:** `Sapientia.MemoryAllocator` (`Allocator`, `MemPtr`, `CachedPtr`, `IndexedPtr`), `Sapientia.Data` (`SafePtr`, `SafePtr<T>`, `PtrOffset`, `SentinelPtr`), `Submodules.Sapientia.Memory` (`MemoryManager`, `MemoryExt`, `MemoryType`).

## 3. Key types & entry points

- `Allocator/Core/Allocator.cs:7` — `Allocator` struct: holds `_zonesList` (`UnsafeList<MemoryZone>`) and `_freeBlockPools` (`UnsafeList<MemoryBlockPtrCollection>`, one pool per power-of-two size class). **Start here.**
- `Allocator/Core/Allocator.Alloc.cs:51` — `MemAlloc(size, out SafePtr)` / `MemReAlloc` (`:119`) / `MemFree` (`:144`): the public alloc API.
- `Allocator/Core/Allocator.MemoryBlock.cs:10` — `MemoryBlock` header struct (`id`, `prevBlockOffset`, `blockSize`, `+dataSize` in DEBUG) prepended to every allocation.
- `Allocator/Core/Allocator.MemoryBlockPtrCollection.cs:245` — `AllocateBlock` / `:397` `FreeBlock` / `:329` `ReAllocateBlock`: the free-list + split/merge engine.
- `Allocator/Core/Allocator.MemoryZone.cs:14` — `MemoryZone` (raw `SafePtr memory`, `zoneEnd`, `size`).
- `Allocator/Core/Allocator.MemPtr.cs:33` — `GetSafePtr(MemPtr)`: resolve a `MemPtr` to a live `SafePtr`.
- `Allocator/Data/MemPtr.cs:8` — `MemPtr` (stable handle).
- `../Data/SafePtr.cs:16` / `:227` — `SafePtr` / `SafePtr<T>` (raw pointer + DEBUG bounds).
- `State/Data/CachedPtr.cs:19` / `:128` — `CachedPtr` / `CachedPtr<T>`: `MemPtr` + version-checked cached `SafePtr` (the safe way to hold a pointer across calls).
- `../Memory/MemoryManager.cs:29` — `MemoryManager`: raw native alloc/free (the arena's zones and all off-arena buffers come from here).

## 4. Layer this provides

This is the **memory layer** under everything. There is no Data/State/Logic/View here; instead:

- **Raw allocation:** `MemoryManager` wraps `UnsafeUtility.Malloc`/`Free` (Unity) or `Marshal.AllocHGlobal` (non-Unity, `MemoryManager.cs:115`), with optional tracking (`MemoryTracker`) and a `MemoryType` (`Default`/`Temp`/`NoTrack`/`NoTrackTemp`/`Inner`, `MemoryManager.cs:19`).
- **Arena:** `Allocator` requests zones from `MemoryManager` (`MemoryZone` ctor → `MemoryExt.MemAlloc`, `Allocator.MemoryZone.cs:22`) and sub-allocates blocks.
- **Handles:** `MemPtr` (stable), `SafePtr`/`SafePtr<T>` (raw), `CachedPtr`/`CachedPtr<T>` (versioned cache), `IndexedPtr` (`MemPtr` + `TypeId`).

## 5. Lifecycle

- **Init:** `Allocator.Initialize(zoneSize)` (`Allocator.cs:18`) clamps to `MIN_ZONE_SIZE` (30 MB, `Allocator.Consts.cs:11`), creates one free-block pool per size class from `MIN_BLOCK_SIZE` (32 bytes) up, then allocates the first zone.
- **Allocate:** `MemAlloc(size)` aligns `size + sizeof(MemoryBlock)` to 8 bytes (`Align`, `Allocator.Alloc.cs:11`), finds/splits a free block (`AllocateBlock`), and returns a `MemPtr` pointing **past** the block header (`memPtr = zoneOffset + sizeof(MemoryBlock)`, `Allocator.Alloc.cs:68`). Size 0 returns a special "zero-sized" `MemPtr` (`zoneOffset = -1`).
- **Grow zones:** if no free block of a big-enough size class exists, `AllocateBlock` allocates a whole new zone (`AllocateMemoryZone`, `Allocator.MemoryZone.cs:71`) and logs a warning when more than one zone exists (`Allocator.MemoryZone.cs:82`) — that warning means you under-reserved.
- **Free:** `MemFree(MemPtr)` converts to `MemoryBlockRef` and calls `FreeBlock`, which **coalesces** with the previous and/or next block if they are free (`Allocator.MemoryBlockPtrCollection.cs:397`).
- **Realloc:** `MemReAlloc` tries to grow in place by eating a free *next* block; otherwise it allocates a new block, `MemCopy`s, and frees the old one (`Allocator.MemoryBlockPtrCollection.cs:329`) — **this moves the data**.
- **Reset/Dispose:** `Clear()` resets every zone to one free block (`Allocator.cs:38`); `Dispose()` frees all pools and zones (`Allocator.cs:58`).
- **Serialize:** `Allocator.Serialize`/`Deserialize` (`Allocator.Serialization.cs`) writes each zone's raw bytes plus the free-block pools; deserialize rebuilds zones and pools. Because zones are re-`MemAlloc`'d at new addresses, **every cached raw pointer is invalid after load** (see §7).

## 6. Dependencies

- **Depends-on:** `MemoryManager`/`MemoryExt` (`UnsafeUtility`), `UnsafeList` (`Collections/Unsafe/UnsafeList.cs`), `TSize<T>`/`TAlign<T>`, `E.ASSERT`.
- **Depended-by:** all `Mem*` collections ([Collections](MemoryAllocator/Collections.md)), `WorldStateData.allocator` (`State/World/WorldState/WorldStateData.cs:11`), and therefore all StateParts/components/services.

## 7. Gotchas & invariants — `MemPtr` vs `SafePtr` (critical)

**`MemPtr`** (`Allocator/Data/MemPtr.cs:8`):
- Just `(int zoneId, int zoneOffset)` — a **logical, stable** address into the arena. Survives resizes and (re-resolved) survives serialization because it's an offset, not an address.
- `IsValid()` means `zoneOffset != 0`; `IsZeroSized()` means `zoneOffset < 0`; `MemPtr.Invalid` is `(0,0)`.
- **This is what you store** for long-lived references. Re-resolve to a `SafePtr` via `allocator.GetSafePtr(memPtr)` (`Allocator.MemPtr.cs:33`) each time you actually dereference.

**`SafePtr` / `SafePtr<T>`** (`../Data/SafePtr.cs:16` / `:227`):
- A **raw `byte*`/`T*`** plus, **only under `DEBUG`**, `lowBound`/`hiBound` for bounds asserts. In release builds it is just the pointer — `IsValidLength`/`AssertValidLength`/index asserts are no-ops (`SafePtr.cs:128`).
- **Goes stale** whenever the underlying block moves: `MemReAlloc` relocation, zone reallocation on deserialize, or any collection resize that reallocs its backing array. A stale `SafePtr` is a dangling pointer with no runtime guard in release.
- **Never cache a bare `SafePtr` across a call that may allocate/resize/serialize.** Re-fetch it.

**`CachedPtr` / `CachedPtr<T>`** (`State/Data/CachedPtr.cs:19`) is the **safe middle ground**: it stores the `MemPtr` plus a cached `SafePtr` tagged with `WorldState.Version`. On access, `GetPtr(worldState)` calls `WorldState.UpdateSafePtr` (`State/World/WorldState/WorldState.Ptr.cs:10`), which re-resolves the `SafePtr` from the `MemPtr` only when the version changed (i.e. after a serialize/deserialize). Use `CachedPtr` (or the `MemArray`/registry wrappers built on it) to hold pointers that must survive snapshots.

Other invariants:
- **Alignment:** blocks are 8-byte aligned (`BLOCK_ALIGN = 8`, `Allocator.Consts.cs:5`); min block 32 bytes (`MIN_BLOCK_SIZE`).
- **Header invariant:** the `MemoryBlock` header sits immediately before the user data; `GetSafePtr` reconstructs `dataSize` from `(safePtr - 1)->dataSize` under DEBUG (`Allocator.MemPtr.cs:40`). Do not write before a `MemPtr`'s data start.
- **`GetZeroRef<T>` / `MemShow`** (`Allocator.MemPtr.cs:14`) hand back a pointer **without** committing the allocation — the comment warns the result "must not be used" for storage; it exists for `ref default` returns. Treat as read-only scratch.
- **Determinism:** the allocator itself is deterministic given the same alloc/free sequence, but block *addresses* are not stable across runs/loads — never serialize a raw address, only a `MemPtr`.
- **No partial-struct layout traps**, but every persisted struct here is `[StructLayout(LayoutKind.Sequential)]` (`MemPtr`, `MemoryBlock`, `MemoryZone`, `BlockId`, `CachedPtr`); do not reorder fields — the serialized blob and the `_raw`-overlap tricks elsewhere depend on layout.

## 8. Open questions / TODO / risks

- `MemoryExt.MemCopy<T>(T* source, T* destination, int length)` non-Unity fallback calls `Buffer.MemoryCopy(source, source, …)` (`../Memory/MemoryExt.cs:167`) — destination should be `destination`; looks like a copy-paste bug (Unity path is correct).
- `MemReAlloc` only tries to merge with the **next** free block, never the previous one (`Allocator.MemoryBlockPtrCollection.cs:340` comment) — intentional, but means in-place growth is opportunistic; most growth copies.
- DEBUG-only `dataSize` (`Allocator.MemoryBlock.cs:16`): `GetDataSize` returns `blockSize - sizeof(MemoryBlock)` in release vs the exact requested size in DEBUG (`Allocator.MemoryBlockPtrCollection.cs:135`). Code relying on exact data size will behave differently between configs.
- Multi-zone allocation logs a `LogWarning` every time a second+ zone is created (`Allocator.MemoryZone.cs:82`) — by design a signal to raise `initialSize`, but noisy.
- No `TODO`/`BUG`/`HACK`/`FIXME` markers found in the allocator core itself; many comments are Russian (legacy team notes).
