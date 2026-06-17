# Pooling

> Code is the source of truth. Every concrete claim below cites `file_path:line`.
> Parent: [Sapientia root](../CLAUDE.md). See also `Assets/Game/Scripts/Generic/CLAUDE.md` for the
> game-layer `PrefabPool` (a different, Unity-GameObject-specific pool at a higher layer).

## 1. Purpose

`Pooling/` is a **managed-object pooling utility library** — pure infrastructure, no gameplay logic. It eliminates GC pressure from frequent allocations of short-lived managed objects (lists, dictionaries, string builders, etc.) by recycling instances. Two pool strategies are provided: a **thread-safe, concurrent `ObjectPool<T>`** for general use and an **ordered `OrderedPool<T>`** that tracks creation order and reuses by index. A set of **static facade pools** (`ListPool<T>`, `DictionaryPool<K,V>`, etc.) built on a per-process singleton `StaticObjectPool<T>` give call-site convenience without needing to hold a pool reference.

This subsystem operates entirely on **managed heap** objects and has no connection to the Sapientia arena allocator or `MemPtr`/`SafePtr` handles.

## 2. Where it lives

- **Folder:** `Assets/Submodules/Sapientia/Pooling/` (root types) and `Assets/Submodules/Sapientia/Pooling/Static/` (static facade pools).
- **Assembly:** `Sapientia` (`../Sapientia.asmdef`).
- **Namespaces:** `Sapientia.Pooling` (all types), `Sapientia.Pooling.Concurrent` (`ConcurrentDictionaryPool<K,V>` only).

## 3. Key types & entry points

**Core abstractions**

- `Pooling/IObjectPool.cs:3` — **`IObjectPool<T>`**: minimal interface, `Get()` / `Release(T)`.
- `Pooling/IObjectPoolPolicy.cs:3` — **`IObjectPoolPolicy<T>`**: lifecycle callbacks — `Create()`, `OnGet(T)`, `OnRelease(T)`, `OnDispose(T)`.
- `Pooling/DefaultObjectPoolPolicy.cs:5` — **`DefaultObjectPoolPolicy<T>`**: default policy for `new()`-constrained `T`; calls `IPoolable.OnGet`/`Release` if the object implements `IPoolable`, and `IDisposable.Dispose` on `OnDispose`. **Base class for all static-pool policies.**

**Pool implementations**

- `Pooling/ObjectPool.cs:18` — **`ObjectPool<T>`**: thread-safe, fixed-capacity pool backed by a `ConcurrentQueue<T>` plus a fast `_single` slot (lock-free CAS). `DEFAULT_CAPACITY = Environment.ProcessorCount * 3` (`ObjectPool.cs:21`). Objects beyond `_maxCapacity` are disposed, not queued. **Start here for the general pool.**
- `Pooling/OrderedPool.cs:14` — **`OrderedPool<T>`**: non-thread-safe pool that retains creation order. Uses a `ulong` bitmask (`_usageMask`) for up to 64 items, then switches to a `bool[]` array for larger sets (`OrderedPool.cs:80-87`). Tracks object → index via `Dictionary<T, int>`. Returns the lowest-index free item on `Get()`.
- `Pooling/PooledObject.cs:7` — **`PooledObject<T>`** (`[MustDisposeResource]` struct, `IDisposable`): an RAII wrapper returned by `pool.Get(out T obj)`. Calling `Dispose()` returns the object to the pool. `Release()` throws if already disposed; `ReleaseSafe()` silently ignores double-dispose.

**Static singleton layer**

- `Pooling/Static/StaticObjectPool.cs:9` — **`StaticObjectPool<T>`** (`sealed class` extending `StaticWrapper<ObjectPool<T>>`): a per-`T` process-singleton `ObjectPool<T>`. Initialized once by each facade's static constructor. `StaticObjectPool` (non-generic static class, same file `:31`) is the internal API hub for `Get`/`Release`/`Initialize`.
- `Pooling/Static/Pool.cs:13` — **`Pool<T>`** (where `T : class, IPoolable, new()`): facade for objects that implement `IPoolable`; delegates to `StaticObjectPool`. Also defines the `IPoolable` interface (`:3`).

**Static facade pools (all in `Pooling/Static/`)**

| Type | File | Element type | `OnRelease` behavior |
|---|---|---|---|
| `Pool<T>` | `Pool.cs:13` | `T : IPoolable` | `obj.Release()` |
| `ListPool<T>` | `ListPool.cs:5` | `List<T>` | `list.Clear()` |
| `ArrayPool<T>` | `ArrayPool.cs:5` | `Array<T>` (Sapientia) | `array.Clear()` |
| `DictionaryPool<K,V>` | `DictionaryPool.cs:5` | `Dictionary<K,V>` | `dict.Clear()` |
| `HashSetPool<T>` | `HashSetPool.cs:5` | `HashSet<T>` | `hashSet.Clear()` |
| `HashMapPool<K,V>` | `HashMapPool.cs:5` | `HashMap<K,V>` (Sapientia) | `dict.Clear()` |
| `QueuePool<T>` | `QueuePool.cs:5` | `Queue<T>` | `list.Clear()` |
| `StackPool<T>` | `StackPool.cs:5` | `Stack<T>` | `list.Clear()` |
| `LinkedListPool<T>` | `LinkedListPool.cs:5` | `LinkedList<T>` | `list.Clear()` |
| `StringBuilderPool` | `StringBuilderPool.cs:7` | `StringBuilder` | `obj.Clear()` |
| `ConcurrentDictionaryPool<K,V>` | `ConcurrentDictionaryPool.cs:5` | `ConcurrentDictionary<K,V>` | `dict.Clear()` |

`ArrayPool<T>` additionally calls `array.Initialize(minimumLength)` on every `Get` to size the buffer (`ArrayPool.cs:11`).

## 4. Data / State / Logic / View breakdown

This is plumbing infrastructure — no gameplay Data/State/Logic/View layers apply.

- **Data:** `DisposeSentinel`-style: not used here. Pool metadata is ordinary managed heap (queue, dictionary, arrays).
- **State:** `ObjectPool<T>._queue` (`ConcurrentQueue<T>`), `._single` (`T?`), `._count` (`int`), `._maxCapacity` (`int`). `OrderedPool<T>._items` (`T[]`), `._usageMask` (`ulong`) / `._usageArray` (`bool[]`), `._indexMap` (`Dictionary<T,int>`), `._count`, `._nextFreeIndex`.
- **Logic:** `IObjectPoolPolicy<T>` callbacks wired in each facade's private `Policy` class.
- **View:** none.

## 5. Lifecycle & tick

No tick. Lifecycle is demand-driven:

- **Initialization (static pools):** each static facade's `static` constructor calls `StaticObjectPool.Initialize(new Policy())` (`ListPool.cs:7`, etc.). This is lazy — triggered on first access to the facade type. `Initialize` is idempotent: it does nothing if `StaticObjectPool<T>.IsInitialized` is already true (`StaticObjectPool.cs:33-36`).
- **Get:** `ObjectPool<T>.Get()` (`ObjectPool.cs:64`): tries `_single` (lock-free CAS), then dequeues from `_queue`; if both empty, calls `_policy.Create()`. Then calls `_policy.OnGet(item)`.
- **Release:** `ObjectPool<T>.Release(T)` (`ObjectPool.cs:79`): fills `_single` first (CAS), then enqueues if under capacity; otherwise calls `_policy.OnDispose(obj)` and discards. Always calls `_policy.OnRelease(obj)` on accepted objects.
- **Dispose pool:** `ObjectPool<T>.Dispose()` (`ObjectPool.cs:54`): calls `Clear()` (disposes all retained objects via `_policy.OnDispose`), then disposes `_policy` if it implements `IDisposable`.
- **`PooledObject<T>`:** the RAII pattern — `pool.Get(out T obj)` (`ObjectPoolExtensions`, `ObjectPool.cs:122`) returns a `PooledObject<T>` that can be used in a `using` block; `Dispose()` calls `pool.Release(obj)`.
- **`OrderedPool<T>` resize:** `Resize()` (`OrderedPool.cs:160`) doubles `_items` and `_usageArray` (if in array mode) via `Array.Resize`. The `_indexMap` dictionary is not resized separately — it grows naturally as new items are added.

## 6. Dependencies

- **Depends-on:**
  - `Sapientia.Collections.Array<T>` (used by `ArrayPool<T>`) and `Sapientia.Collections.HashMap<K,V>` (used by `HashMapPool<K,V>`) — these are Sapientia's custom collection types.
  - `Sapientia.Extensions` (used by `StaticObjectPool.cs:6`) — provides extension methods like `ReferenceContains` on `ConcurrentQueue`.
  - Standard BCL: `System.Collections.Concurrent.ConcurrentQueue<T>`, `System.Collections.Generic.*`, `System.Threading.Interlocked`, `System.Text.StringBuilder`.
  - `JetBrains.Annotations.MustDisposeResourceAttribute` on `PooledObject<T>` (`PooledObject.cs:6`) — Rider annotation, no runtime effect.
- **Depended-by:**
  - `Assets/Game/Scripts/` — `ListPool<T>` is heavily used across `Game.Client` and `_Features/` (UI viewmodels, quest logic, mission payloads). `StaticObjectPoolUtility` (`ReleaseAndSetNull`, `ReleaseAndSetNullSafe`) is used in viewmodel dispose paths.
  - No Sapientia-internal code was found consuming these pools — they are an outward-facing utility.

## 7. Gotchas & invariants

### Forgetting to return to pool

Static pools (`ListPool<T>`, etc.) **do not auto-return** objects. A caller who takes `ListPool<T>.Get()` and does not call `Release()` (or use the `PooledObject<T>` RAII pattern) silently leaks the list from the pool. Over time the pool runs dry and falls back to `_policy.Create()`, defeating the purpose. The preferred idiom is:

```csharp
using (ListPool<Foo>.Get(out var list))
{
    // use list
} // Release called automatically by PooledObject<T>.Dispose()
```

Or the manual `Release` when `using` is not applicable.

### Pooled state NOT reset by default

`DefaultObjectPoolPolicy<T>` calls `IPoolable.Release()` on `OnRelease` (`DefaultObjectPoolPolicy.cs:17-19`). **Only** if `T` implements `IPoolable`. For types like `List<T>`, `Dictionary<K,V>`, etc., each facade's private `Policy` overrides `OnRelease` to call `Clear()` — so items are cleared before returning to the pool. `ArrayPool<T>` calls `Initialize(minimumLength)` on every `Get` to set the correct size. `StringBuilderPool` calls `Clear()` on release. If you write a custom `IObjectPoolPolicy<T>` without implementing `OnRelease`, the recycled object carries its previous state.

### `ObjectPool<T>._single` fast-path vs. thread safety

`ObjectPool<T>` uses `Interlocked.CompareExchange` on `_single` (`ObjectPool.cs:67`) and `Interlocked.Increment`/`Decrement` on `_count` (`ObjectPool.cs:70`, `88`, `94`). The `ConcurrentQueue` is itself thread-safe. `ObjectPool<T>` is **thread-safe for concurrent `Get`/`Release` calls**.

### `OrderedPool<T>` is NOT thread-safe

The type doc comment is explicit: "Non thread-safe version" (`OrderedPool.cs:12`). The usage pattern for `OrderedPool<T>` is single-threaded (e.g., per-scene or per-system pools where access is from one thread at a time).

### `DEV` guard for double-return detection

Both `ObjectPool<T>.Release` and `OrderedPool<T>.Release` have `#if DEV` guards (`ObjectPool.cs:81`, `OrderedPool.cs:113`) that throw exceptions on double-return. These guards are **not** `#if DEBUG` — they use a custom `DEV` symbol. Unless that symbol is defined in the build configuration, **double-return is silently ignored in all builds** (including development builds). Verify whether `DEV` is ever enabled in CI or editor configurations.

### Capacity is fixed at construction for `ObjectPool<T>`

`DEFAULT_CAPACITY = Environment.ProcessorCount * 3` (`ObjectPool.cs:21`). On most developer machines this is 24–48 slots. Excess objects returned to a full pool are disposed immediately via `_policy.OnDispose`. There is a TODO (`ObjectPool.cs:10-11`) requesting dynamic capacity based on scene context — it is not implemented.

### `StaticObjectPool` is process-global, not world-scoped

`StaticObjectPool<T>` is a static singleton (`StaticWrapper<ObjectPool<T>>`). It persists across Unity scene loads and world re-creations. Objects pooled from one scene's usage are available to the next. This is intentional for GC amortization but means **pooled objects should be cleared to a known state on release** (all built-in facades do call `Clear()`).

### `StaticWrapper.Set` disposes the previous instance

`StaticWrapper<T>.Set(instance, disposePrevious: true)` (`Abstract/StaticWrapper.cs:29`) disposes the previous pool if `disposePrevious = true` (the default). Calling `StaticObjectPool<T>.Set(...)` from outside (e.g., in tests) will dispose all currently-pooled objects of that type. This is probably fine for tests but could corrupt state if called accidentally at runtime.

### Contrast with game-layer `PrefabPool`

`Assets/Game/Scripts/Generic/Extensions/Unity/Prefab/PrefabPool.cs` is a **different, unrelated pool** at a higher layer. It pools `UnityEngine.GameObject`/`Component` instances (prefab instances) using `UnityEngine.Object.Instantiate` and scene-lifecycle management. It does not use `ObjectPool<T>` or any Sapientia pooling types. It is scene-scoped and managed by `PrefabPoolManager`. The Sapientia `Pooling/` folder is for managed C# objects only — no Unity objects.

## 8. Open questions / TODO / risks

- `Pooling/ObjectPool.cs:10-11` — **TODO** (Russian): "Dynamic capacity — for example: a scene requires many sounds, the next one does not; it would be worth thinking about changing the pool capacity by context." Capacity is currently fixed at construction; no mechanism exists to shrink or grow the pool post-construction.
- `Pooling/Static/Pool.cs:5` — **TODO** (Russian): "Tried to rename to `OnRelease`, didn't work out, need to retry later..." — refers to the `IPoolable.Release()` method name. The method is named `Release` where the policy's callback is named `OnRelease`; the asymmetry is an acknowledged inconsistency, not yet resolved.
- **`DEV` symbol is undefined** in the repository as verified (no `csc.rsp` entry, no project-level `#define DEV`) — the double-return checks in `ObjectPool` and `OrderedPool` are effectively dead code in all current build configurations. This is a risk: double-returns silently succeed and corrupt pool state.
- **`OrderedPool<T>` `_usageMask` → `_usageArray` migration** (`OrderedPool.cs:80-87`): when `_count >= 64`, the pool creates a new `bool[]` and copies the bitmask — but the bitmask is then abandoned (not zeroed; it is simply ignored afterwards). Any future code that reads `_usageMask` after the array migration would get stale data. Currently only `IsFree` branches on `_usageArray != null` first, so this is safe today but fragile.
- **`ArrayPool<T>` calls `Initialize(minimumLength)` on every `Get`**, not just on fresh creates (`ArrayPool.cs:11`). If `Initialize` is expensive or has side-effects beyond size-setting, this is a hidden per-call cost. The implementation of `Array<T>.Initialize` is not read here — `unknown` cost.
- **`ConcurrentDictionaryPool<K,V>` is in namespace `Sapientia.Pooling.Concurrent`** (`ConcurrentDictionaryPool.cs:4`) while all other pools are in `Sapientia.Pooling`. This namespace difference is a minor discoverability gotcha.
- **No pooled-object ownership protocol.** There is no `DisposeSentinel`-style guard to detect use-after-return. Once `Release` is called, the caller's reference is still valid C# — any subsequent read or write on the returned object corrupts the next borrower's state silently. The `#if DEV` double-return check is the only mitigation, and it is inactive (see above).
