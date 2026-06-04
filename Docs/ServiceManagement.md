# ServiceManagement — Legacy global service locator

> LEGACY module. For the in-world registry (current architecture) see
> [State / World / Entity](MemoryAllocator/State.md) and the `TypeIndexer`/`IndexedTypes`
> discussion in the root [Sapientia CLAUDE.md §7](../CLAUDE.md).
> Parent: [Sapientia root](../CLAUDE.md).

## 1. Purpose

`ServiceManagement/` is a **global, process-wide service locator** that predates the in-world
`TypeIndexer`/`IndexedTypes` registry. It is **still used** for services that are not owned by a
`World` instance: application-layer singletons, cross-world managers, platform adapters, and any
managed object that needs to be looked up by type from anywhere in the codebase without a
`WorldState` reference. The in-world component and StatePart registry has been replaced by
`TypeIndexer` (see §6 and §7 below); this module handles the **non-world / global** tier.

The module exposes three concentric abstractions:

1. **`ServiceLocator<TService>`** — per-type singleton slot (one instance per `TService`).
2. **`ServiceLocator<TContext, TService>`** — per-context map (one instance per `(TContext, TService)` pair), with a "current" context that can be switched.
3. **`ServiceLocator`** (non-generic facade) — type-erased helpers plus an optional `IServicesSupplier` override for dependency injection.
4. **`ServiceContext<TContext>`** — broadcasts context changes to all registered `IContextSubscriber<TContext>` listeners, including all `ServiceLocator<TContext, TService>` instances.

## 2. Where it lives

- **Folder:** `Assets/Submodules/Sapientia/ServiceManagement/`
- **Assembly:** `Sapientia` (`Assets/Submodules/Sapientia/Sapientia.asmdef`).
- **Namespaces:**
  - `Sapientia.ServiceManagement` — `ServiceLocator`, `ServiceLocator<TService>`, `ServiceLocator<TContext, TService>`, `ServiceContext<TContext>`, `ServiceScope<TService>`, `IContextSubscriber`, `IContextSubscriber<TContext>`
  - `Game.App.ServiceManagement` — `IServicesSupplier` (lives in this file but uses a `Game.App` namespace — a layering anomaly)

## 3. Key types & entry points

- `ServiceManagement/ServiceLocator.cs:7` — `ServiceLocator` (static, non-generic): the public facade. `Get<TService>()`, `TryGet<TService>()`, `RegisterAsService<TService>()`, `UnRegisterAsService<TService>()`, `ReplaceService<TService>()`. **Start here for usage.**
- `ServiceManagement/ServiceLocator.Generic.cs:8` — `ServiceLocator<TService>` (static `partial`): per-type singleton storage (`AsyncValue<TService> _instance`), `Register`, `UnRegister`, `TryGet`, `ReplaceService`, `RemoveAllContext`. Also the bridge to context-keyed variants via `SetService<TContext>`, `GetService<TContext>`, etc. (second `partial` block in the same file at line 358).
- `ServiceManagement/ServiceLocator.Context.cs:10` — `ServiceLocator<TContext, TService>` (static): per-context dictionary (`Dictionary<ContextContainer, TService>`) plus a hot "current service" cached outside the dictionary. Also defines `ServiceScope<TService>` (disposable RAII handle) and `ServicesEnumerator`/`ServicesEnumerable`.
- `ServiceManagement/ServiceContext.cs:7` — `ServiceContext<TContext>` (static): owns the "current context" for `TContext`; broadcasts `SetContext`/`ReplaceContext`/`RemoveContext`/`RemoveAllContext` to all `IContextSubscriber<TContext>` subscribers, which includes all `ServiceLocator<TContext, *>` instances.
- `ServiceManagement/IServicesSupplier.cs:5` — `IServicesSupplier`: optional external DI override; if set via `ServiceLocator.SetServiceSupplier`, `Get`/`TryGet` query it first.

## 4. Data / State / Logic / View breakdown

This module is infrastructure, not a gameplay feature. The classification below maps to its role:

- **Data:** per-type static fields (`_instance`, `_contextToService`, `_currentService`, `_subscribers`) — all process-static, no allocator backing, no serialization.
- **State:** effectively global mutable state (static fields); see §7 Thread-safety.
- **Logic:** none. Registration/unregistration is the only "behavior".
- **View:** none.

## 5. Lifecycle & tick

No lifecycle hooks. Services are registered/unregistered imperatively by callers:

- **Registration:** `service.RegisterAsService<TService>()` → `ServiceLocator<TService>.Register(service)` (`ServiceLocator.cs:76`, `ServiceLocator.Generic.cs:67`). Asserts the slot is empty (`E.ASSERT(Instance == null)`, `ServiceLocator.Generic.cs:69`).
- **Unregistration:** `service.UnRegisterAsService<TService>()` → `ServiceLocator<TService>.UnRegister(service)` (`ServiceLocator.Generic.cs:99`). No-ops if the stored instance doesn't match.
- **Replacement:** `service.ReplaceService<TService>()` → `ServiceLocator<TService>.ReplaceService(service)` returns the old instance (`ServiceLocator.Generic.cs:85`); no assertion.
- **Context switching:** `ServiceContext<TContext>.SetContext(ctx)` (`ServiceContext.cs:44`) updates `_currentContext` and calls `subscriber.SetContext` on all registered `IContextSubscriber<TContext>`s, which causes each `ServiceLocator<TContext, TService>` to swap `_currentService` from its dictionary.
- **Context cleanup:** `ServiceLocator<TService>.RemoveAllContext()` (`ServiceLocator.Generic.cs:106`) propagates to all `IContextSubscriber`s, including `ServiceLocator<TContext, TService>.RemoveAllContext(dispose)` which optionally calls `(service as IDisposable)?.Dispose()` (`ServiceLocator.Context.cs:148`).

## 6. Dependencies

- **Depends-on:**
  - `Sapientia.Data.AsyncValue<T>` / `AsyncClass` (`Data/AsyncValues/`) — all mutable fields are wrapped in `AsyncValue<TService>` or guarded by `AsyncClass` scopes for thread-safety.
  - `Sapientia.Collections.SimpleList<T>` — used for subscriber lists.
  - `Game.App.ServiceManagement.IServicesSupplier` (and `Sapientia.IObjectsProvider`) — external DI override interface.
- **Depended-by:**
  - `Game.Core` application-layer bootstrappers and managed singletons (platform services, asset loaders, UI managers) that pre-date / don't need a `WorldState`.
  - Any system that calls `ServiceLocator.Get<T>()` without a `WorldState` reference.
- **Contrast with `TypeIndexer` / `IndexedTypes`** ([MemoryAllocator/State/CLAUDE.md §7](MemoryAllocator/State.md)): `TypeIndexer` is the **in-world** registry for `IIndexedType`-tagged unmanaged types; it is sized at build time by code-gen and stored inside `WorldState`. `ServiceLocator` is a **process-global** registry for managed objects with no world affinity. They are complementary, not interchangeable.

## 7. Gotchas & invariants

### This is a global mutable registry — not deterministic, not serializable

Every `ServiceLocator<TService>._instance` and `ServiceLocator<TContext, TService>._contextToService` are **process-static** fields. They survive across worlds, across reloads (in-editor), and are **not** part of any `WorldState` snapshot. Do not store simulation state here; it will not be saved or rolled back.

### [Obsolete] APIs

Three methods are marked `[Obsolete("Низя")]` ("forbidden"):

- `ServiceLocator.GetOrCreate<TService>()` (`ServiceLocator.cs:17`): only compiled on `#if !CLIENT && !UNITY_EDITOR` — server/test builds only.
- `ServiceLocator.GetOrCreate<TService>(out TService service)` (`ServiceLocator.cs:27`): same guard.
- `ServiceLocator<TService>.GetOrCreate<T>()` (`ServiceLocator.Generic.cs:33`): compiled always, but marked obsolete.

These auto-create a `new T()` instance if the slot is empty. The pattern encourages implicit singletons; callers should instead explicitly register. Do not add new usages.

### Pending migration TODOs

`ServiceLocator.Get<TService>()` (`ServiceLocator.cs:36`) and `TryGet<TService>()` (`ServiceLocator.cs:61`) both contain a commented-out alternative implementation that would require `_supplier` to be non-null, replacing the current fallback to `ServiceLocator<TService>`:

```csharp
//TODO: change to this, once everything is stable.
//if (_supplier == null)
//    throw new Exception("Service Supplier is null.");
//return _supplier.Get<TService>();
```

(`ServiceLocator.cs:46`–`:51`, `:67`–`:72`). Until this migration completes, `Get`/`TryGet` consult both the `_supplier` and the per-type static slot, with a `LogError` if both have a value (`ServiceLocator.cs:101`–`:104`).

### Thread-safety

All writes to `_instance` go through `AsyncValue<TService>.SetValue` (`ServiceLocator.Generic.cs:19`) which uses `Interlocked.CompareExchange` (`AsyncValue.cs:62`). All reads of `_currentService` in `ServiceLocator<TContext, TService>` are guarded by `_asyncClass.GetBusyScope()` (`ServiceLocator.Context.cs:65`). `ServiceContext<TContext>` state changes (`SetContext`, `ReplaceContext`, `RemoveContext`) are also guarded (`ServiceContext.cs:46`). However:
- The `Register` call asserts `Instance == null` with `E.ASSERT` (`ServiceLocator.Generic.cs:69`) — this is not an atomic check-then-set; there is a TOCTOU window if two threads race to register the same service type. Use `TryRegister` (`ServiceLocator.Generic.cs:58`) for concurrent registration.
- `RemoveAllContext(dispose: true)` calls `IDisposable.Dispose()` inside the `AsyncClass` lock (`ServiceLocator.Context.cs:148`) — if `Dispose()` re-enters the locator, it will deadlock (re-entrant locking is permitted only with `ignoreThreadId = false` path, i.e. same-thread only).

### `ServiceScope<TService>` — lock held for scope duration

`ServiceLocator<TContext, TService>.GetServiceScope(context)` (`ServiceLocator.Context.cs:253`) returns a `ref struct ServiceScope<TService>` that **holds the `AsyncClass` busy lock** until `Dispose()` is called (`ServiceLocator.Context.cs:440`). All other context-keyed operations on the same `TService` will spin-wait for the duration of the scope. Use only in short, bounded code paths.

### `ComplexAction` and context `ServicesEnumerator` allocate

`ServiceLocator<TContext, TService>.ServicesEnumerator.Create()` (`ServiceLocator.Context.cs:317`) copies `_contextToService.Values` into a `new SimpleList<TService>` — this allocates. Do not call in hot loops.

### `ServiceContext<TContext>` auto-wires on `ServiceLocator<TContext, TService>` construction

The static constructor of `ServiceLocator<TContext, TService>` (`ServiceLocator.Context.cs:75`) immediately calls `ServiceContext<TContext>.AddSubscriber<ContextSubscriber>()` and reads `ServiceContext<TContext>.CurrentContext`. This means **merely referencing** `ServiceLocator<SomeContext, SomeService>` for the first time (triggering the static ctor) registers it as a subscriber and adopts the current context. Side-effects on first use are expected.

## 8. Open questions / TODO / risks

**[Obsolete] markers:**

- `Assets/Submodules/Sapientia/ServiceManagement/ServiceLocator.cs:17` — `ServiceLocator.GetOrCreate<TService>()` (`[Obsolete("Низя")]`, server/non-client builds only)
- `Assets/Submodules/Sapientia/ServiceManagement/ServiceLocator.cs:27` — `ServiceLocator.GetOrCreate<TService>(out service)` (`[Obsolete("Низя")]`, server/non-client builds only)
- `Assets/Submodules/Sapientia/ServiceManagement/ServiceLocator.Generic.cs:33` — `ServiceLocator<TService>.GetOrCreate<T>()` (`[Obsolete("Низя")]`)

**TODO markers:**

- `Assets/Submodules/Sapientia/ServiceManagement/ServiceLocator.cs:46` — `//TODO: change to this, once everything is stable.` (pending migration to supplier-only `Get<TService>`)
- `Assets/Submodules/Sapientia/ServiceManagement/ServiceLocator.cs:67` — `//TODO: change to this, once everything is stable.` (pending migration to supplier-only `TryGet<TService>`)

**Risks / open questions:**

- `IServicesSupplier` (`IServicesSupplier.cs:5`) is in namespace `Game.App.ServiceManagement` but its file lives inside the Sapientia submodule — a cross-layer namespace leak. The actual supplier implementation is in `Game.Core`/`Game.App`; `unknown` whether the submodule's `Sapientia.asmdef` references the `Game.App` assembly or vice versa (needs assembly-reference audit before cleanup).
- TOCTOU race in `ServiceLocator<TService>.Register` (`ServiceLocator.Generic.cs:67`–`:71`): `E.ASSERT(Instance == null)` is not atomic with `Instance = service`. Use `TryRegister` for concurrent registration.
- `RemoveAllContext(dispose: true)` calls `Dispose()` inside the `_asyncClass` lock — potential for deadlock if `Dispose()` re-enters the locator on the same thread with `ignoreThreadId = false` (the default). No existing examples of this pattern found, but the risk is structural.
- No existing mechanism to enumerate all registered service types (the registry is split across unbounded `ServiceLocator<TService>` static generics); observability / leak detection at shutdown is `unknown`.
