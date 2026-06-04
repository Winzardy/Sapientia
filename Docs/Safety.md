# Safety — DisposeSentinel

> Code is the source of truth. Every concrete claim below cites `file_path:line`.
> Parent: [Sapientia root](../CLAUDE.md). Related: [MemoryAllocator](MemoryAllocator.md).

## 1. Purpose

`Safety/` is a **plumbing subsystem** — it provides a lightweight, type-partitioned **lifetime-tracking token** (`DisposeSentinel`) that can be embedded into any unmanaged or managed resource to detect use-after-free, double-dispose, and leak patterns at runtime. There is no gameplay logic here. The sentinel is a generation-versioned handle: when an allocation is created it receives a live sentinel; when it is disposed the sentinel's version is bumped and the slot released, so any stale copy held by an old caller will fail `IsValid()`. This gives the same generation-guard principle that [`Entity`](MemoryAllocator/State.md) uses for ECS object lifetime, applied to arbitrary resource types.

## 2. Where it lives

- **Folder:** `Assets/Submodules/Sapientia/Safety/`
- **Assembly:** `Sapientia` (`../Sapientia.asmdef`) — same assembly as everything else in the submodule; no separate `.asmref`.
- **Namespace:** `Submodules.Sapientia.Safety`

## 3. Key types & entry points

- `Safety/DisposeSentinel.cs:6` — **`DisposeSentinel`** (`readonly struct`, `IDisposable`, `IEquatable<DisposeSentinel>`): the public token held by a resource owner. Fields: `id` (slot index), `version` (generation), `typeId` (per-type allocator index). **Start here.**
- `Safety/DisposeSentinelAllocator.cs:9` — **`DisposeSentinelAllocator`** (`internal struct`): one instance per tracked type; owns the `_versions` sparse-set and the `_asyncValue` mutex. Allocates, checks, and releases individual sentinels.
- `Safety/DisposeSentinelManager.cs:7` — **`DisposeSentinelManager`** (`internal class`): the static root; maps `T` → `typeId` via a nested `DisposeSentinelTypeId<T>` class and owns the global `UnsafeIndexAllocSparseSet<DisposeSentinelAllocator>` registry.

## 4. Data / State / Logic / View breakdown

This subsystem is pure infrastructure — no Data/State/Logic/View layers apply.

- **Data:** `DisposeSentinel` (a 12-byte `readonly struct`: `id: int`, `version: int`, `typeId: int` — `DisposeSentinel.cs:8-10`).
- **State:** `DisposeSentinelAllocator._versions` — an `UnsafeIndexAllocSparseSet<int>` storing the current version for every live slot (`DisposeSentinelAllocator.cs:11`).
- **Logic:** `DisposeSentinelManager` (static routing) + `DisposeSentinelAllocator` (per-type alloc/check/release).
- **View:** none.

## 5. Lifecycle & tick

There is no tick. The sentinel lifecycle is driven entirely by the owning resource:

1. **Create** — resource owner calls `DisposeSentinel.Create<T>()` (`DisposeSentinel.cs:21`) or `DisposeSentinel.Create()` (`DisposeSentinel.cs:27`), which routes to `DisposeSentinelManager.AllocateDisposeSentinel<T>()` (`DisposeSentinelManager.cs:80`):
   - `GetTypeId<T>()` lazily allocates a `typeId` (an integer index into the global allocator sparse-set) the first time a type is seen (`DisposeSentinelManager.cs:59-63`).
   - `DisposeSentinelAllocator.AllocateDisposeSentinel()` acquires `_asyncValue.SetBusy()`, allocates a slot id from `_versions`, increments its version counter, releases the lock, and returns `new DisposeSentinel(id, version, typeId)` (`DisposeSentinelAllocator.cs:26-36`).

2. **Check liveness** — `sentinel.IsValid()` (`DisposeSentinel.cs:33`) calls `DisposeSentinelManager.CheckDisposeSentinel(this)` → `DisposeSentinelAllocator.CheckDisposeSentinel(handle)` (`DisposeSentinelAllocator.cs:39`): returns `false` if the slot `id` is no longer present in `_versions` (already released) or if the stored version differs from the handle's `version` (double-release incremented it).

3. **Dispose/Release** — `sentinel.Dispose()` (`DisposeSentinel.cs:39`) guards against double-dispose via `IsValid()`, then calls `DisposeSentinelManager.ReleaseDisposeSentinel(this)` → `DisposeSentinelAllocator.ReleaseDisposeSentinel(handle)` (`DisposeSentinelAllocator.cs:51`): acquires the lock, **increments the version** of the slot (so any stale copy held elsewhere becomes invalid), then calls `_versions.ReleaseId(id, false)` to free the slot (`DisposeSentinelAllocator.cs:57-58`).

## 6. Dependencies

- **Depends-on:**
  - `Sapientia.Collections.UnsafeIndexAllocSparseSet<T>` (`Collections/Unsafe/UnsafeIndexAllocSparseSet.cs`) — the unmanaged backing store for both the global allocator registry and each per-type version table.
  - `Sapientia.Data.AsyncValue` (`Data/AsyncValues/AsyncValue.cs:9`) — a lock-free `SetBusy`/`SetFree` mutex used to guard concurrent `AllocateDisposeSentinel` / `ReleaseDisposeSentinel` calls.
  - `Submodules.Sapientia.Memory.MemoryManager` — raw memory id `MemoryManager.NoTrackMemoryId` used when constructing sparse-sets (`DisposeSentinelAllocator.cs:19`, `DisposeSentinelManager.cs:54`).
  - `Sapientia.E.ASSERT` (`Exceptions/E.Assert.cs:37`) — type-id mismatch assertions in `CheckDisposeSentinel` and `ReleaseDisposeSentinel` (compiled out in non-DEBUG builds — see §7).
  - Under `UNITY_5_3_OR_NEWER`: `Unity.Burst.SharedStatic<T>` for static storage compatible with Burst compilation (`DisposeSentinelManager.cs:14-43`).
- **Depended-by:** no internal Sapientia code was found calling `DisposeSentinel.Create` — as of this reading, the sentinel is not yet embedded in any `Mem*` collection or allocator path. It is a utility ready for adoption by any resource type (e.g., a pool slot, a native buffer wrapper). No usages were found in `Assets/Game/` either.

## 7. Gotchas & invariants

**This is the most important section for safety-critical review.**

### DEBUG vs. release compilation behavior

The sentinel mechanism is **not guarded by any `#if DEBUG` or `#if UNITY_EDITOR` block**. All three classes compile fully in every configuration. However:

- `E.ASSERT` in `DisposeSentinelAllocator.CheckDisposeSentinel` (`DisposeSentinelAllocator.cs:41`) and `ReleaseDisposeSentinel` (`DisposeSentinelAllocator.cs:53`) is decorated with `[Conditional("DEBUG")]` (`Exceptions/E.Assert.cs:36`). **These assert calls are silently removed by the C# compiler in builds where the `DEBUG` symbol is not defined (i.e., non-debug Unity builds).** The version check logic itself (`_versions.Has` / version equality) is **always present** and runs in all builds — it is not conditional.
- The `UNITY_5_3_OR_NEWER` guards in `DisposeSentinelManager` (`DisposeSentinelManager.cs:14`, `21`, `32`, `39`, `48`) switch between `Unity.Burst.SharedStatic<T>` (Unity path) and a plain `static` field (non-Unity path) for compatibility with server-side execution. This is a **platform compatibility switch**, not a debug/release gate.

### Generation semantics (version counter)

- `AllocateDisposeSentinel` increments the version **before** issuing the handle (`_versions.Get(id)++` at line `DisposeSentinelAllocator.cs:32`). This ensures a re-used slot id always produces a fresh, distinct handle.
- `ReleaseDisposeSentinel` increments the version **again before** releasing the slot (`DisposeSentinelAllocator.cs:57`). Any copy of the sentinel that was captured before `Dispose()` will now see a version mismatch on `IsValid()` — this is the use-after-free detection mechanism.
- `DisposeSentinel.Dispose()` is idempotent at the public API level: it calls `IsValid()` first and returns early if the sentinel is already invalid (`DisposeSentinel.cs:41`). However, because `IsValid()` reads `_versions.Has(id)` from external state, there is a **potential race window** between `IsValid()` and `ReleaseDisposeSentinel()` in concurrent scenarios. The `AsyncValue` lock in `AllocateDisposeSentinel` / `ReleaseDisposeSentinel` guards the write side, but `CheckDisposeSentinel` (`IsValid`) does **not** acquire the lock (it reads `_versions.Has` without synchronization, `DisposeSentinelAllocator.cs:43`). This means that in a concurrent context, `IsValid()` may race with a concurrent `Release`.

### `AsyncValue` lock scope

`_asyncValue.SetBusy()` / `SetFree()` guard only `AllocateDisposeSentinel` and `ReleaseDisposeSentinel` (`DisposeSentinelAllocator.cs:28-34`, `55-60`). `CheckDisposeSentinel` is unguarded. This is intentional for read-only checks but means check results can be stale under concurrent mutation.

### Type-id binding

- Each distinct `T` used with `Create<T>()` gets a unique `typeId`. The `typeId` is stored in the sentinel and verified by `E.ASSERT` on every check/release in DEBUG builds. If a sentinel with the wrong `typeId` is passed to the wrong allocator, the assert fires (DEBUG only — silently wrong behavior in release).
- `DisposeSentinel.Create()` (no type parameter) registers under the type `DisposeSentinel` itself (`DisposeSentinel.cs:29`). This means un-typed sentinels share a single allocator bucket, which is fine but may be surprising.

### Allocator storage

Both the global allocator table (`DisposeSentinelManager._disposeSentinelAllocators`) and each per-type version table (`DisposeSentinelAllocator._versions`) use `MemoryManager.NoTrackMemoryId` — the memory for these structures is allocated **outside** the arena's tracking system and is not serialized with the world snapshot. This means sentinel state is **ephemeral** (does not survive serialization/deserialization).

### No leak detection at the system level

`DisposeSentinel` detects **use-after-free** and **double-dispose** via `IsValid()`. It does **not** currently log a warning for sentinels that were allocated but never disposed (leak detection would require scanning live slots at shutdown — no such scan exists in the current code).

## 8. Open questions / TODO / risks

- No `TODO`, `BUG`, `HACK`, or `FIXME` markers were found in any of the three files in `Safety/`.
- **No callers found.** A repo-wide grep found zero calls to `DisposeSentinel.Create` or `DisposeSentinel.Create<T>` outside of `Safety/` itself. The mechanism is implemented but appears unused as of this reading. Risk: the lack of usage means the concurrent-access behavior (§7) has not been exercised in practice.
- **`CheckDisposeSentinel` is not lock-guarded** (`DisposeSentinelAllocator.cs:39-47`) — the comment in `DisposeSentinelManager.cs:9-11` explicitly acknowledges the design intent (per-type split reduces potential race conditions), but concurrent read vs. write is still unprotected. Whether this is acceptable depends on the intended calling pattern (`unknown` without callers).
- **Leak detection gap** — there is no `Dispose`-on-shutdown scan that would log a warning for unreleased sentinels. If this is meant to detect leaks, it is incomplete (`unknown` whether this was intentional).
- The `DisposeSentinel.Create()` overload (no type parameter) silently uses the `DisposeSentinel` type as its bucket key (`DisposeSentinel.cs:27-29`). If a caller forgets to supply `<T>`, all such sentinels share one allocator — this is a subtle misuse footgun with no compile-time guard.
