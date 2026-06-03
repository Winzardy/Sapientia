# Data — Low-level reusable data types & pointer wrappers

> Plumbing module. No gameplay logic here — these are the primitive value types and managed-bridge
> utilities the rest of Sapientia and `Game.Core` are built on.
> Parent: [Sapientia root](../CLAUDE.md). Allocator pointer context: [MemoryAllocator](../MemoryAllocator/CLAUDE.md).

## 1. Purpose

`Data/` is an internal toolbox of **small, reusable value types** that other modules consume. It provides:

- **Pointer wrappers** for allocator-backed memory (`SafePtr`/`SafePtr<T>`, `SentinelPtr`/`SentinelPtr<T>`, `PtrOffset`/`PtrOffset<T>`, `ClassPtr`/`ClassPtr<T>`) — the raw and sentinel layers above `MemPtr`.
- **Identity and packaging primitives** (`Id`, `Id<T>`, `Pack<T>`) for typed array indices and quantity-with-target bundles.
- **Bit-flag masks** over enums (`EnumMask<T>`, `ByteEnumMask<T>`, `UndefinedEnumMask`) and a safe enum-serialization wrapper (`EnumSerializableContainer<T>`).
- **Thread-safe primitives** (`AsyncValue`, `AsyncValue<T>`, `AsyncClass`, `AsyncValueClass<T>`) for managed-layer concurrency outside the unmanaged simulation tick.
- **Event/callback utilities** (`ActionContainer<TContext>`, `ComplexAction`, `DelayableAction`, `DelayableAction<TContext>`) for observer patterns in the managed layer.
- **Layout helpers** (`Toggle<T>`, `OptionalRange<T>`, `OptionalVector2`, `Range<T>`, `Pack<T>`, `Union32`, `Union64`, `SerializableDateTime`) for config and serialization use.

This module is **plumbing**, not simulation state. It has no `StatePart`, no `System`, no lifecycle. `SafePtr` is the only type here that directly touches the arena allocator.

## 2. Where it lives

- **Folder:** `Assets/Submodules/Sapientia/Data/`
- **Assembly:** `Sapientia` (`Assets/Submodules/Sapientia/Sapientia.asmdef`), `allowUnsafeCode: true`.
- **Namespaces:**
  - `Sapientia.Data` — pointer wrappers, masks, async primitives (`SafePtr`, `PtrOffset`, `SentinelPtr`, `EnumMask`, `AsyncValue`, `AsyncClass`, `DelayableAction`, …)
  - `Submodules.Sapientia.Data` — `Id`, `Id<T>`, `EnumSerializableContainer<T>`
  - `Submodules.Sapientia.Data.Convertors` — `Union32`, `Union64`
  - `Sapientia.Data.Events` — `ActionContainer<TContext>`, `ComplexAction`
  - `Sapientia` (root namespace) — `Pack<T>`, `Range<T>`, `Toggle<T>`, `OptionalRange<T>`, `OptionalVector2`
  - `Survivor.Interop` — `SerializableDateTime` (note: this file lives physically in `Data/` but belongs to the Interop namespace)

## 3. Key types & entry points

- `Data/SafePtr.cs:16` — `SafePtr`: untyped raw pointer (`byte*`) + DEBUG bounds (`lowBound`/`hiBound`). **The raw dereference primitive; see §7 for hazards.**
- `Data/SafePtr.cs:227` — `SafePtr<T>`: typed `T*` variant; `Ref`, `Value()`, `Slice`, `GetSpan`.
- `Data/PtrOffset.cs:6` — `PtrOffset`: signed byte-offset arithmetic between two `SafePtr`s; integrates with `SafePtr` operators.
- `Data/PtrOffset.cs:73` — `PtrOffset<T>`: typed variant; `++`/`--` advance by `TSize<T>.size`.
- `Data/SentinelPtr.cs:15` — `SentinelPtr`: `SafePtr` + `DisposeSentinel` — raises exception on access after `Dispose()` (DEBUG only via `[Conditional]`).
- `Data/SentinelPtr.cs:165` — `SentinelPtr<T>`: typed variant.
- `Data/ClassPtr.cs:15` — `ClassPtr`: stores a managed class instance as `GCHandle`+`IntPtr` inside an `unsafe struct` — the managed-to-unmanaged bridge.
- `Data/ClassPtr.cs:71` — `ClassPtr<T>`: typed variant; `Value()`, `Cast<T>`, `[DebuggerTypeProxy]`.
- `Data/Id.cs:7` — `Id`: 1-based `int` index (`id=0` → `Invalid`; `(int)id` subtracts 1). `[Serializable]`.
- `Data/Id.Generic.cs:7` — `Id<T>`: phantom-typed wrapper around `Id`; type-safety only, no extra data.
- `Data/Pack.cs:14` — `Pack<T>`: `(T target, int count)` quantity bundle; serializable, implicit `[CLIENT]` `FormerlySerializedAs` migration.
- `Data/EnumMasks/EnumMask.cs:17` — `EnumMask<T>`: 32-bit flag mask over any `unmanaged Enum`.
- `Data/EnumMasks/ByteEnumMask.cs:8` — `ByteEnumMask<T>`: 8-bit variant, max 8 enum values.
- `Data/UndefinedEnumMask.cs:6` — `UndefinedEnumMask`: untyped 32-bit mask; supports cross-type `Union`.
- `Data/EnumSerializableContainer.cs:10` — `EnumSerializableContainer<TEnum>`: Unity-editor-safe enum serialization with name-based migration.
- `Data/AsyncValues/AsyncValue.cs:9` — `AsyncValue`: `[LayoutKind.Explicit]` lock (`_threadId`+`_count`) with `SpinWait`-based `SetBusy`/`SetFree`; Burst-aware (falls back for non-Burst via `[BurstDiscard]`).
- `Data/AsyncValues/AsyncValue.cs:161` — `AsyncValue<T>`: value-wrapper with `ReadValue`/`SetValue`.
- `Data/AsyncValues/AsyncClass.cs:6` — `AsyncClass`: managed base class wrapping `AsyncValue`; provides `GetBusyScope`/`GetAsyncBusyScope` and scope structs.
- `Data/AsyncValues/AsyncValueClass.cs:6` — `AsyncValueClass<T>`: `AsyncClass` subclass with a typed `value`; `GetValueBusyScope(out T)` pattern.
- `Data/Events/ActionContainer.cs:5` — `ActionContainer<TContext>`: observable `Action<TContext>` wrapper; `executeIfInvoked` flag replays last event to late subscribers.
- `Data/Events/ComplexAction.cs:6` — `ComplexAction`: fires a `Action` only when all registered `ActionContainer<object>` executors have acted (barrier pattern); `IsOneShot` clears on fire.
- `Data/DelayableAction.cs:6` — `DelayableAction`: deferred `Action` dispatcher; `DelayInvoke` increments a counter, `InvokeDelayed` drains it.
- `Data/DelayableAction.cs:139` — `DelayableAction<TContext>`: context-carrying variant; queues contexts in `SimpleList<TContext>`.
- `Data/Toggle/Toggle.cs:20` — `Toggle<T>`: `(bool enable, T value)` optional value; serializable with `[FormerlySerializedAs]` migration.
- `Data/Range.cs:10` — `Range<T>`: `(T min, T max)` serializable range; `RangeUtility.Contains` for `float`/`int`.
- `Data/Toggle/OptionalRange.cs:12` — `OptionalRange<T>`: `(Toggle<T> min, Toggle<T> max)` independently enabled bounds.
- `Data/Toggle/OptionalVector2.cs:11` — `OptionalVector2`: `(Toggle<float> x, Toggle<float> y)`.
- `Data/Unions/Union32.cs:6` — `Union32`: explicit-layout 4-byte reinterpret union (byte/short/int).
- `Data/Unions/Union64.cs:6` — `Union64`: 8-byte variant.
- `Data/SerializableDateTime.cs:11` — `SerializableDateTime`: UTC ticks wrapper, `Survivor.Interop` namespace.

> **Note:** `ProxyPtr<T>` lives at `MemoryAllocator/State/Data/ProxyPtr.cs:25`, not in this folder, despite being closely related. See [State/World/Entity](../MemoryAllocator/State/CLAUDE.md).

## 4. Data / State / Logic / View breakdown

This module provides **Data** in the broad sense — reusable primitive types. It has no StateParts, Systems, or View.

- **Data (this folder):** value types, pointer wrappers, masks, event utilities, thread-safe containers.
- **State / Logic / View:** none.

## 5. Lifecycle & tick

No lifecycle. Types here are instantiated on demand by callers. `AsyncClass` subclasses (`DelayableAction`, `ActionContainer`) manage their own per-instance `AsyncValue` lock. `ComplexAction.AddExecutor` / `RemoveExecutor` subscribe to `ActionContainer<object>` events and fire only when all registered executors have acted — all managed-layer, outside the simulation tick.

## 6. Dependencies

- **Depends-on:**
  - `Sapientia.Extensions` (`UnsafeExt.As`, `ToByte`, `ToInt`, `EnumValues<T>`, `EnumNames<T>`) — extension helpers.
  - `Sapientia.Collections.SimpleList` (used by `DelayableAction<TContext>` context queue and `ServiceLocator` subscriber list).
  - `Submodules.Sapientia.Safety.DisposeSentinel` (used by `SentinelPtr`/`SentinelPtr<T>`).
  - `Unity.Burst` (conditional `[BurstDiscard]` on `AsyncValue`'s thread-id path) and `Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestrictionAttribute` (guarded by `UNITY_5_3_OR_NEWER`).
- **Depended-by:** [MemoryAllocator](../MemoryAllocator/CLAUDE.md) (`SafePtr` is the arena's dereference type), [ServiceManagement](../ServiceManagement/CLAUDE.md) (`AsyncValue`/`AsyncClass`), and the entirety of `Game.Core` runtime (masks, toggles, ids, events).

## 7. Gotchas & invariants

### `SafePtr` / `SafePtr<T>` — the #1 hazard

`SafePtr` is a **raw pointer**. In DEBUG it carries `lowBound`/`hiBound` for range assertions; in Release it is just the pointer and all bounds checks are compiled away (`SafePtr.cs:128`–`:130`). Key invariants:

- **Goes stale on any allocator move** (realloc, zone growth, deserialize) — see [MemoryAllocator §7](../MemoryAllocator/CLAUDE.md).
- `IsValid` is only `ptr != null` — a stale non-null pointer passes this check silently.
- `SafePtr(void* ptr)` (single-arg ctor, `SafePtr.cs:54`) sets `lowBound`/`hiBound` to `null` in DEBUG — bounds asserts will always pass (no bounds known). Use the size-carrying overloads when you need range checks.
- Arithmetic operators (`+`, `-`, `++`, `--`) re-assert bounds in DEBUG but are no-ops in Release.
- `SafePtr == SafePtr` in DEBUG **also compares bounds**, so two pointers to the same address but different allocation slices are not equal (`SafePtr.cs:202`–`:207`). Be aware when using as dictionary key.

### `PtrOffset` arithmetic

`PtrOffset` wraps a signed `int byteOffset` plus `bool isValid` (set to `true` in the only public ctor, `PtrOffset.cs:13`). The default-constructed `PtrOffset` has `isValid = false`; callers do not check this flag themselves — it is informational only. `PtrOffset<T>++`/`--` advance by `TSize<T>.size` (`PtrOffset.cs:171`,`:178`), not by 1 byte.

### `SentinelPtr` — DEBUG-only safety

`SentinelPtr` adds `DisposeSentinel` to a `SafePtr`. `CheckNullRef` is `[Conditional("DEBUG")]` / `[Conditional(E.DEBUG)]` (`SentinelPtr.cs:63`,`:207`) — in Release, accesses after `Dispose()` are **not caught**. `SentinelPtr.ResetDisposeSentinel` (`SentinelPtr.cs:151`) is the only way to re-arm a reused pointer without allocating a new wrapper.

### `ClassPtr` — GC pinning

`ClassPtr.Create<T>` calls `GCHandle.Alloc(data)` (`ClassPtr.cs:45`), which pins the object and prevents GC collection. **Must call `Dispose()` to release the handle** (`ClassPtr.cs:55`–`:61`). `Cast<T>()` on the untyped variant (`ClassPtr.cs:33`) uses `UnsafeExt.As<object, T>` — no runtime type check; unsafe if type mismatch.

### `Id` — 1-based index with `Invalid = 0`

`Id.id` is stored 1-based. The implicit `(int)id` conversion returns `id - 1` (`Id.cs:24`), and `(Id)int` adds 1 (`Id.cs:29`). `Id.IsValid` is `id > 0` (`Id.cs:18`). `Id<T>` wraps `Id` with phantom type T and has the same conversions. Do not compare raw `Id.id` values to array indices without accounting for the offset.

### `EnumMask<T>` — 32-bit limit; `HasAnything` bug

`EnumMask<T>` stores bits as `int mask`, limit 32 bits (`BitsCount = 32`, `EnumMask.cs:24`). **`HasAnything()` returns `mask == 0`** (`EnumMask.cs:58`) — this is identical to `HasNothing()` and appears to be a copy-paste bug; it should likely return `mask != 0`. `ByteEnumMask<T>` has the same pattern (`ByteEnumMask.cs:40`). The debug type-proxy correctly interrogates the mask (`EnumMask.cs:261`), so the display in the IDE is accurate, but the `HasAnything()` API is unreliable.

### `AsyncValue` — Burst / thread-id interaction

`AsyncValue.GetThreadId` uses `[BurstDiscard]` on the managed path and falls back to Unity's `JobsUtility.ThreadIndex` (negated) under Burst (`AsyncValue.cs:136`–`:141`). The `ignoreThreadId = true` path treats all recursive callers on any thread as non-reentrant (deadlock risk on recursive calls); `false` (default) allows re-entrant locking from the **same** thread. See the inline doc comment (`AsyncValue.cs:66`–`:91`) for the contract.

### `SerializableDateTime` — Interop namespace in Data folder

`SerializableDateTime.cs` physically lives in `Data/` but uses namespace `Survivor.Interop` (`SerializableDateTime.cs:5`). This is a layering anomaly — it belongs to a different assembly boundary conceptually.

### `ComplexAction` uses `Dictionary` and `foreach`

`ComplexAction` holds `_executorToExecutionState = new Dictionary<object, (int, int)>()` (`ComplexAction.cs:11`) and iterates it with `foreach` (`ComplexAction.cs:115`,`:125`). Both allocate managed memory. This is appropriate for managed-layer use only — **do not use in hot simulation loops**.

### `Pack<T>` field rename migration

`Pack<T>.count` was previously serialized as `amount`. The `[FormerlySerializedAs("amount")]` attribute (`Pack.cs:19`) is guarded by `#if CLIENT`, so the migration is Unity-editor/client only.

## 8. Open questions / TODO / risks

No `TODO`/`BUG`/`HACK`/`FIXME` markers found in `Data/` source files (grep confirmed).

**Bugs / code smells found by inspection:**

- `EnumMask<T>.HasAnything()` (`Data/EnumMasks/EnumMask.cs:58`): returns `mask == 0`, identical to `HasNothing()` — almost certainly a copy-paste bug; should be `mask != 0`.
- `ByteEnumMask<T>.HasAnything()` (`Data/EnumMasks/ByteEnumMask.cs:40`): same bug — `mask == 0` instead of `mask != 0`.
- `SafePtr(void* ptr)` single-arg ctor (`Data/SafePtr.cs:54`): sets both `lowBound` and `hiBound` to `null` in DEBUG, so bounds assertions always pass for `SafePtr`s created this way — the bounds-check safety mechanism is silently disabled.
- `SerializableDateTime` is in namespace `Survivor.Interop` but physically lives in `Data/` (`Data/SerializableDateTime.cs:5`) — layering inconsistency.

**Note on `ProxyPtr`:** The stray code at `MemoryAllocator/State/Data/ProxyPtr.cs:8` (`public class SomeClass : ISomeInterface{}` and `public interface ISomeInterface{}`) is in a file inside `MemoryAllocator/State/Data/`, **not** inside this `Data/` folder. Confirmed present at that path. See [State/World/Entity §8](../MemoryAllocator/State/CLAUDE.md) and the root [CLAUDE.md §8](../CLAUDE.md).
