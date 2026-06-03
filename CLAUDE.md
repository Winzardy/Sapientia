# Sapientia

> Foundation library. Code is the source of truth — every claim below cites `file_path:line`.
> Nested deep-dives: [MemoryAllocator](MemoryAllocator/CLAUDE.md) · [Collections](MemoryAllocator/Collections/CLAUDE.md) · [State / World / Entity](MemoryAllocator/State/CLAUDE.md).

## 1. Purpose

Sapientia is the **unmanaged, data-oriented core** that the whole game simulation is built on. It is a self-contained git submodule (separate repo) providing: a custom **arena allocator** (`Allocator`), a family of allocator-backed **Mem-collections** (`MemArray`/`MemList`/`MemSparseSet`/…), and an **ECS-style World/State/Entity model** where all simulation state lives in `unmanaged` structs over the allocator. This gives the game a **no-GC, deterministic tick** and a **serializable world snapshot** (the entire allocator memory can be written/read as one blob). There is no gameplay logic here — Sapientia is the substrate; gameplay lives in `Game.Core` (see [Architecture](../../../Docs/Core/ARCHITECTURE.md)).

## 2. Where it lives

- **Folder:** `Assets/Submodules/Sapientia/` (git submodule — do not commit here; the orchestrator handles git).
- **Assembly:** `Assets/Submodules/Sapientia/Sapientia.asmdef` — assembly name `Sapientia`, references only `Unity.Burst` and `Unity.Mathematics`, `allowUnsafeCode: true`. Compiler response file `csc.rsp` sets `-nullable:enable`.
- **Namespaces:** `Sapientia.MemoryAllocator` (allocator + collections + World/State), `Sapientia.MemoryAllocator.State` (Entity/ComponentSet/StateParts), `Sapientia.Data` (`SafePtr`, `Id`, `ClassPtr`, …), `Submodules.Sapientia.Memory` (`MemoryManager`, `MemoryExt`), `Sapientia.TypeIndexer` (type→index registry, proxies), `Sapientia.Collections` (off-allocator `UnsafeList`/`SimpleList`), `Sapientia.ServiceManagement` (legacy `ServiceLocator`).

## 3. Key types & entry points

- `MemoryAllocator/Allocator/Core/Allocator.cs:7` — the arena allocator (zone/block model). **Start here for memory.** See [MemoryAllocator](MemoryAllocator/CLAUDE.md).
- `MemoryAllocator/Allocator/Data/MemPtr.cs:8` — `MemPtr` (zoneId + zoneOffset): the *stable* handle into allocator memory.
- `Data/SafePtr.cs:16` / `Data/SafePtr.cs:227` — `SafePtr` / `SafePtr<T>`: the *raw, bounds-checked* pointer; goes stale on resize/serialize.
- `MemoryAllocator/State/World/World.cs:10` — `World` (managed class): lifecycle + tick driver. **Start here for the simulation loop.**
- `MemoryAllocator/State/World/WorldState/WorldState.cs:11` — `WorldState` (struct): the arena + registries handle, passed to almost every method.
- `MemoryAllocator/State/StateParts/Entity/Entity.cs:8` — `Entity` (8-byte struct: id/generation/worldId).
- `MemoryAllocator/State/ComponentSets/ComponentSet.cs:80` — `ComponentSet` (the renamed "Archetype"; sparse-set component storage).
- `MemoryAllocator/TypeIndexer/IndexedTypes.cs:25` — `IndexedTypes`: the type→index registry (replaces the old `ServiceLocator`).
- `MemoryAllocator/State/World/WorldBuilder/WorldBuilder.cs:8` — `WorldBuilder`: abstract base for registering StateParts/Systems and building a `World`.
- See [State / World / Entity](MemoryAllocator/State/CLAUDE.md) for the full model.

## 4. Layer this library provides

Sapientia is plumbing, not a Data/State/Logic/View gameplay feature. It provides the **substrate** those layers sit on:

- **Memory layer:** `Allocator` (arena) + `MemoryManager` (raw OS/Unity allocation) + `MemPtr`/`SafePtr`/`CachedPtr` handles. See [MemoryAllocator](MemoryAllocator/CLAUDE.md).
- **Collection layer:** allocator-backed `Mem*` containers. See [Collections](MemoryAllocator/Collections/CLAUDE.md).
- **State layer:** `World`/`WorldState`/`Entity`/`ComponentSet`/`IWorldStatePart`/`IWorldSystem` — the ECS-style model gameplay `*StatePart` and `*System` structs plug into. See [State / World / Entity](MemoryAllocator/State/CLAUDE.md).
- **Type/proxy layer:** `TypeIndexer` (`IIndexedType`, `TypeId`, `ProxyPtr`, generated `*Proxy` types) — gives `unmanaged` structs interface-like virtual dispatch without boxing/GC.
- **Infrastructure/Utility:** `Infrastructure/` (Trading, Messaging, Content, ScaleTables, SharedLogic — partly server-shared), `Utility/` (Evaluator, Blackboard, Fix64, Csv, Schedule), `Pooling/`, `Safety/` (`DisposeSentinel`). These are out of the deep-doc scope but live in the same submodule.

## 5. Lifecycle & tick

The world is built and driven from `Game.Core` (`GameWorldBuilder`/`GameRuntime`), but the mechanism lives here:

- **Build:** `WorldBuilder.Build(initialSize)` (`MemoryAllocator/State/World/WorldBuilder/WorldBuilder.cs:22`) → `WorldManager.CreateWorld` allocates a `WorldState` (arena) → `AddStateParts()`/`AddSystems()` queue elements → `World.Initialize(...)`.
- **Initialize:** `World.Initialize` (`World.cs:42`) registers every StatePart then every System (registration order = execution order), then calls `Initialize` → `LocalStatePartService.Initialize` → `LateInitialize` on all elements.
- **Start:** `World.Start` (`World.cs:73`) calls `EarlyStart` then `Start`; sets `IsStarted`; sends started message.
- **Tick:** `World.Update(deltaTime)` (`World.cs:98`) increments `worldState.Tick`/`Time`, then runs all systems in registration order across three phases: `BeforeUpdate` → `Update(deltaTime)` → `AfterUpdate`. `World.LateUpdate` (`World.cs:126`) runs `BeforeLateUpdate` → `LateUpdate` → `AfterLateUpdate` (used by View).
- **Dispose:** `World.Dispose` (`World.cs:153`) runs `BeforeDispose`/`Dispose` on all elements, then `worldState.RemoveWorld()`.
- Every public entry wraps work in `using var scope = worldState.GetWorldScope();` (`WorldManager.cs:94`) to set the current world thread-static.

## 6. Dependencies

- **Depends-on:** Unity `UnsafeUtility`/Burst/Mathematics (with non-Unity fallbacks guarded by `UNITY_5_3_OR_NEWER`). No other game assemblies — this is the bottom of the dependency graph.
- **Depended-by:** `Game.Core` (Data/State/Logic/View layers) and the feature/specific systems build all simulation state on `WorldState`, `Entity`, `ComponentSet`, and the `Mem*` collections. `Fusumity`/`Survivor.Interop` also reference Sapientia utilities.

## 7. Gotchas & invariants

- **Determinism:** the simulation advances by ticks with caller-supplied `deltaTime` (`World.Update`); `worldState.Tick` counts updates, `worldState.Time` accumulates time (`WorldStateData.cs:36`). All state is `unmanaged` → no GC pauses. Do **not** introduce wall-clock or `UnityEngine.Random` into simulation code (see `CONVENTIONS.md`).
- **Pointer staleness is the #1 hazard.** `MemPtr` is stable; raw `SafePtr` is not. The allocator may move blocks on resize, and serialization bumps `WorldState.Version`, after which all cached `SafePtr`s are invalid. Always re-derive via `CachedPtr` / `WorldState.GetSafePtr`. Details in [MemoryAllocator](MemoryAllocator/CLAUDE.md) §7.
- **Entity use-after-free** is caught by **generation** (`Entity.generation`): a reused id gets a new generation, so a stale `Entity` fails `IsEntityExist` / `IsValid`. See [State](MemoryAllocator/State/CLAUDE.md) §7.
- **`DEBUG` vs release behavior diverges:** `SafePtr` carries bounds (`lowBound`/`hiBound`) and asserts only under `DEBUG` (`Data/SafePtr.cs:20`); release builds skip the checks entirely. Many `E.ASSERT` invariants are compiled out in release.
- **Thread-safety:** the world model is single-threaded by design. `WorldManager` uses thread-static "current world" context (`WorldManager.cs:150`). `MemoryManager` exposes `NoTrack*`/`Temp` ids for parallel raw allocation but notes tracking is not supported in parallel (`Memory/MemoryManager.cs:33`).
- **Terminology evolution (vs obsolete Notion):** the old Notion architecture page (`Docs/Reference/Notion/architecture-obsolete.md`) calls component storage "Archetype" and uses a class `WorldElement` + a `ServiceLocator`. Today the code uses **`ComponentSet`** (struct, `ComponentSet.cs:80`), **`unmanaged struct` StateParts/Systems** (`IWorldStatePart`/`IWorldSystem`), and the **`TypeIndexer`/`IndexedTypes` registry** (`IndexedTypes.cs:25`). Treat that Notion page as **stale**; this doc reflects current code.

## 8. Open questions / TODO / risks

- `WorldManager.DeserializeWorld` (`WorldManager.cs:159`) is `throw new NotImplementedException();` followed by dead code — world deserialization is not wired through `WorldManager` yet (the per-`WorldState` `Serialize`/`Deserialize` paths exist).
- `Entity.CompareTo` is `[Obsolete]` (`Entity.cs:157`) — "works incorrectly, callers should compare `Entity.id`".
- `ProxyPtr.cs:8` declares a stray `public class SomeClass : ISomeInterface{}` / `ISomeInterface` next to the doc comment — looks like leftover example/illustration code in a production file.
- `MemoryExt.MemCopy<T>(T* source, T* destination, …)` non-Unity fallback (`Memory/MemoryExt.cs:167`) calls `Buffer.MemoryCopy(source, source, …)` — copies source onto itself; likely a bug, but only on the non-Unity path.
- `MemoryManager.MemoryType` enum is `internal` (`Memory/MemoryManager.cs:19`) — `unknown` whether external callers ever need it.
- Whole-`Infrastructure`/`Utility`/`Pooling`/`Safety` subtrees are **not deep-documented** here (out of A2a scope) — `unknown` corners remain.
