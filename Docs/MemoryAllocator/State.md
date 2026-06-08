# State — World / WorldState / Entity / ComponentSet

> The ECS-style simulation model. Parent: [Sapientia root](../../CLAUDE.md).
> Siblings: [MemoryAllocator](../MemoryAllocator.md) · [Collections](Collections.md).

## 1. Purpose

This folder defines the **World/State/Entity model** the entire game simulation runs on. A `World` (managed driver) owns a `WorldState` (the arena + registries). Game objects are **`Entity`** value types (id + generation + worldId). There are **no classic per-component arrays exposed to gameplay**; instead simulation state lives in **`*StatePart`** structs and per-entity data lives in **`ComponentSet`s** (sparse-set storage, the renamed "Archetype"). Behavior runs in **`*System`** structs across tick phases. Type identity for all of this comes from the **`TypeIndexer`** (`IndexedTypes`), the registry that replaced the old `ServiceLocator`.

## 2. Where it lives

- **Folder:** `Assets/Submodules/Sapientia/MemoryAllocator/State/` — subfolders `World/`, `World/WorldState/`, `World/WorldElements/`, `World/WorldBuilder/`, `StateParts/` (incl. `Entity/`, `Update/`, `Destroy/`), `ComponentSets/`, `Data/`, `ServiceLocator/`. The `TypeIndexer` lives one level up at `../TypeIndexer/`.
- **Assembly:** `Sapientia` (`../../Sapientia.asmdef`).
- **Namespaces:** `Sapientia.MemoryAllocator` (`World`, `WorldState`, `WorldId`, `WorldManager`, `WorldBuilder`, `IWorld*`, ptr types), `Sapientia.MemoryAllocator.State` (`Entity`, `EntityStatePart`, `ComponentSet`, `IComponent`, StateParts), `Sapientia.TypeIndexer` (`IIndexedType`, `TypeId`, `TypeIdOf`, `IndexedTypes`, `ProxyPtr`).

## 3. Key types & entry points

- `World/World.cs:10` — `World` (managed `partial class`): build/initialize/start/tick/dispose driver. **Start here.**
- `World/WorldState/WorldState.cs:11` — `WorldState` (struct): `SafePtr<WorldStateData>` + `WorldId`; the handle threaded through every API.
- `World/WorldState/WorldStateData.cs:9` — `WorldStateData` (internal struct): the actual state — `allocator`, three registries, `version`/`tick`/`time`.
- `World/WorldId.cs:8` — `WorldId` (struct: `id` + `version`, 4 bytes).
- `World/WorldManager.cs:11` — `WorldManager` (static): world array, id allocation/recycling, current-world thread context.
- `StateParts/Entity/Entity.cs:8` — `Entity` (8-byte struct). **The exact definition is in §7.**
- `StateParts/Entity/EntityStatePart.cs:15` — `EntityStatePart`: entity id pool + generation table; `CreateEntity`/`DestroyEntities`/`IsEntityExist`.
- `ComponentSets/ComponentSet.cs:80` — `ComponentSet`: per-component sparse-set storage (the "Archetype").
- `ComponentSets/ComponentSetContext.cs:10` — `ComponentSetContext<T>`: ergonomic typed wrapper over a `ComponentSet`.
- `World/WorldElements/IWorldElement.cs:5` / `IWorldStatePart.cs:3` / `IWorldSystem.cs:3` — the element/StatePart/System interfaces.
- `World/WorldBuilder/WorldBuilder.cs:8` — `WorldBuilder`: registers StateParts/Systems, builds the `World`.
- `World/WorldElements/WorldElementsService.cs:5` — stores the ordered `MemList`s of elements/systems that the tick iterates.
- `../TypeIndexer/IndexedTypes.cs:25` — `IndexedTypes`: type→`TypeId`, proxy/delegate tables. `../TypeIndexer/Data/TypeId.cs:7` / `Data/TypeIdOf.cs:3` — `TypeId`, `TypeIdOf<T>`, `TypeIdOf<TContext,T>`.
- `Data/IndexedPtr.cs:14` / `Data/CachedPtr.cs` / `Data/ProxyPtr.cs:25` — typed arena-pointer wrappers used to store services/components.

## 4. Layer this provides

This is the **State layer** gameplay plugs into (its own Data/State/Logic/View is in `Game.Core`). What each piece holds:

- **Entity store:** `EntityStatePart` keeps `_freeEntitiesIds` (`MemArray<ushort>`), `_entityIdToGeneration` (`MemArray<ushort>`), destroy subscribers, and (DEBUG) `_aliveEntities` (`EntityStatePart.cs:24`). It is itself a registered `IWorldStatePart`.
- **Component storage:** each `ComponentSet` wraps a `MemSparseSet _elements` of `ComponentSetElement<T>` (= `{Entity entity; T value;}`, `ComponentSet.cs:26`) keyed by `entity.id`. Sets are registered per component type in `WorldStateData.componentsManager` (`WorldStateData.cs:28`).
- **Services / StateParts / Systems:** registered as `IndexedPtr`/`SafePtr` payloads in the three registries (see §5). StateParts = state; Systems = behavior; Logic structs = reusable service operations (registered as `IWorldLocalUnmanagedService`).

## 5. Lifecycle, registries & tick

**Three registries** in `WorldStateData` (`WorldStateData.cs:9`), all `UnsafeIndexedRegistry<TBase, TPayload>` (`../State/ServiceLocator/UnsafeIndexedRegistry.cs:18`), each sized by `TypeId<TBase>.Count`:
- `serviceRegistry` — `IWorldService` → `IndexedPtr` (StateParts, systems, configs, save structs). **In the snapshot.**
- `componentsManager` — `IComponent` → `CachedPtr<ComponentSet>`. **In the snapshot.**
- `noStateServiceRegistry` — `IWorldLocalUnmanagedService` → `SafePtr` (Logic structs, unmanaged local parts). **Heap-only, not serialized**, recreated on load (`WorldStateData.cs:79`).
- Managed services (`IWorldLocalService`) live in `LocalStatePartService` (`StateParts/LocalStatePartService.cs:45`), indexed by `TypeIdOf<IWorldLocalService,T>`.

**Registration order = execution order.** `WorldBuilder.Build` (`WorldBuilder.cs:22`) calls `AddStateParts()` then `AddSystems()`, queuing each into `_stateParts`/`_systems` (`WorldBuilder.cs:73`,`:85`). `World.Initialize` (`World.cs:42`) adds them to `WorldElementsService` (`WorldElementsService.cs:16`) in that order; the tick iterates that `MemList` in order.

**Tick phases** (`World.cs:98`): per `Update(deltaTime)` the world bumps `Tick`/`Time`, then runs every system's `BeforeUpdate` → `Update(deltaTime)` → `AfterUpdate`. `LateUpdate` (`World.cs:126`) runs `BeforeLateUpdate` → `LateUpdate` → `AfterLateUpdate` (View phase). Element lifecycle: `Initialize` → `LateInitialize` → `EarlyStart` → `Start` … `BeforeDispose` → `Dispose` (`IWorldElement.cs`).

**Entity lifecycle:**
- `Entity.Create(worldState)` → `EntityStatePart.CreateEntity` (`EntityStatePart.cs:112`): pops a free id, **increments that id's generation** (`++_entityIdToGeneration[id]`, `EntityStatePart.cs:118`), returns `new Entity(id, generation, worldId)`.
- `EntityStatePart.DestroyEntities` (`EntityStatePart.cs:139`): notifies `IEntityDestroySubscriber`s (every `ComponentSet` subscribes, `ComponentSet.cs:121`) so components are removed, then **increments the generation again** and returns the id to the free pool (`EntityStatePart.cs:153`).
- Destruction requests/kill/aliveness flow through the `Destroy` StatePart (`StateParts/Destroy/State/DestroyStatePart.cs`) and its systems.

**Serialization:** `WorldState.Serialize` → `WorldStateData.Serialize` (`WorldStateData.cs:54`) writes the allocator blob + `serviceRegistry` + `componentsManager` + version/tick/time. `Deserialize` (`WorldStateData.cs:65`) rebuilds them and **increments `version`** (so every `CachedPtr` re-resolves), and recreates `noStateServiceRegistry` empty. Caller must then `SetupNewWorldId` (`WorldState.cs:115`).

## 6. Dependencies

- **Depends-on:** [Allocator](../MemoryAllocator.md) (arena), [Collections](Collections.md) (`MemArray`/`MemSparseSet`/`MemList`), `TypeIndexer` (`IndexedTypes`/`TypeId` — initialized by a generator, see §8), `ProxyPtr` + generated `*Proxy` types for virtual dispatch.
- **Depended-by:** all `Game.Core` StateParts/Systems/Logic and the runtime builder (`GameWorldBuilder` derives from `WorldBuilder`; see [Architecture](../../../../../Docs/Core/ARCHITECTURE.md)).

## 7. Gotchas & invariants

### Exact `Entity` definition (`StateParts/Entity/Entity.cs:8`)
```csharp
[StructLayout(LayoutKind.Explicit)]
public struct Entity : IEquatable<Entity>
{
    public const ushort GENERATION_ZERO = 0;            // :13  — generation 0 ⇒ empty/invalid
    public static readonly Entity EMPTY = new (default, GENERATION_ZERO, default); // :15
    [FieldOffset(0)] private readonly int _raw;         // :19  — overlaps id+generation, for ==/hash
    [FieldOffset(0)] public readonly ushort id;         // :22
    [FieldOffset(2)] public readonly ushort generation; // :24
    [FieldOffset(4)] public WorldId worldId;            // :26  — (WorldId = ushort id + ushort version)
}
```
So an `Entity` is **8 bytes**: `id` (u16) and `generation` (u16) packed into `_raw` (int) at offset 0, plus `worldId` (4 bytes) at offset 4. `WorldId` (`World/WorldId.cs:8`) is itself `[Explicit]` with `id`+`version` overlapping its own `_raw`.

### Generation-based use-after-free protection (the core safety property)
- Each entity id carries a **generation counter** in `EntityStatePart._entityIdToGeneration`. `CreateEntity` increments it on allocation and `DestroyEntities` increments it again on free. So a destroyed-then-reused id gets a **different generation**.
- A stale `Entity` handle is detected by `EntityStatePart.IsEntityExist` (`EntityStatePart.cs:133`): `entity.id < Capacity && _entityIdToGeneration[entity.id] == entity.generation`. `Entity.IsValid()` (`World.cs:229`) and `Entity.IsEmpty()` (generation == 0) build on this.
- **Invariant for all gameplay code:** never dereference an `Entity` whose liveness you have not re-checked this frame. A cached `Entity` from a previous tick may now point at a different live entity (same id, new generation). `ComponentSet.GetElement`/`TryGetElement` assert `element.entity == entity` (`ComponentSet.cs:270`,`:309`) to catch generation mismatch in DEBUG.
- ⚠️ Several known gameplay bugs are exactly this class (e.g. the "dead entities reaching SpellActionSystem" TODO, `HealthView` invalid-Entity workaround — see review). Treat generation re-checks as mandatory.

### ComponentSet = the renamed "Archetype"
- `ComponentSet` (`ComponentSet.cs:80`) is per-component sparse-set storage keyed by `entity.id`; `ComponentSetContext<T>` still names its field `_innerArchetype` (`ComponentSetContext.cs:13`) — a fossil of the old "Archetype" terminology. The obsolete Notion page (`Docs/Reference/Notion/architecture-obsolete.md`) documents an "Archetypes (ECS)" design; **today it is `ComponentSet`** with the "few fat sets over many atomic components" philosophy preserved. Notion is **stale** on naming; code is truth.
- Capacity is pre-reserved per type in StatePart `Initialize` (`DestroyStatePart.cs:7`); expansion logs a warning (`ComponentSet.cs:417`).

### TypeIndexer replaces ServiceLocator
- All world types implement `IIndexedType` (marker, `../TypeIndexer/IndexedTypes.cs:18`) and get a `TypeId` from `IndexedTypes` (`TypeIdOf<T>.typeId`, `Data/TypeIdOf.cs:3`). Registries are sized by these ids. The legacy `Sapientia.ServiceManagement.ServiceLocator` (`../../ServiceManagement/ServiceLocator.cs:7`) still exists but its `GetOrCreate` overloads are `[Obsolete("Низя")]` — the world-local registries (this folder) are the current mechanism. Notion's "Service Locator" page is **stale**.
- `IndexedTypes` is populated by generated code (`../TypeIndexer/Generator/*`, `_scripts.generated/InterfaceProxyGenerator/*`). If the generator hasn't run / types changed, `TypeId<TBase>.Count` and registry sizes can be wrong → a full domain reload is required (`UnsafeIndexedRegistry.cs:27` note).

### Pointer staleness & versioning
- StateParts/components are stored as `CachedPtr`/`IndexedPtr` (`MemPtr` + version). After serialize/deserialize, `WorldStateData.version` increments, invalidating every cached `SafePtr`; access re-resolves via `WorldState.UpdateSafePtr` (`World/WorldState/WorldState.Ptr.cs:10`). Holding a bare `SafePtr` to a service across a snapshot is a use-after-free. See [MemoryAllocator](../MemoryAllocator.md) §7.
- `noStateServiceRegistry` (Logic structs) is **not** serialized — anything you store there is gone after load and must be re-created (`WorldStateData.cs:62`,`:79`).

### Roles: StatePart vs System vs Logic
- **`IWorldStatePart`** (`IWorldStatePart.cs:3`, extends `IWorldElement`+`IWorldService`): owns/initializes state (registers component sets, holds `Mem*` collections). Registered in `serviceRegistry`, in the snapshot.
- **`IWorldSystem`** (`IWorldSystem.cs:3`): the behavior, with the six tick hooks. Registration order = tick order.
- **Logic** structs implement `IWorldLocalUnmanagedService` (+ `IInitializableService`) and live in `noStateServiceRegistry` (heap-only) — reusable operation bundles, not serialized.
- **Local StateParts:** `IWorldUnmanagedLocalStatePart` (unmanaged, in `noStateServiceRegistry`) and `IWorldLocalStatePart` (managed, in `LocalStatePartService`) — for runtime-only state with lifecycle hooks (`StateParts/LocalStatePartService.cs:9`).

### Determinism / thread-safety
- Single-threaded model; `WorldManager` keeps a thread-static current world set via `GetWorldScope`/`SetCurrentWorld` (`WorldManager.cs:150`). `WorldId.version` recycling guards stale world handles (`WorldManager.IsValid`, `WorldManager.cs:266`).

## 8. Open questions / TODO / risks

- `WorldManager.DeserializeWorld` (`WorldManager.cs:159`) is `throw new NotImplementedException();` with unreachable code after it — world load through `WorldManager` is unfinished.
- `Entity.CompareTo` is `[Obsolete]` "works incorrectly; compare `Entity.id`" (`Entity.cs:157`).
- `Entity.Name` and DEBUG `_aliveEntities` only exist under `ENABLE_ENTITY_NAMES`/`DEBUG` (`Entity.cs:28`, `EntityStatePart.cs:27`) — names/alive-tracking are unavailable in release; don't rely on them in shipping logic.
- `ProxyPtr.cs:8` contains stray `public class SomeClass : ISomeInterface{}` + `ISomeInterface` declarations inside a production file (illustration left in code).
- The interface-proxy / TypeIndexer code generation is **out of scope** here — documented at the concept level only; the generated `*Proxy.generated.cs` files under `../TypeIndexer/_scripts.generated/` are not audited.
- `WorldStateData.componentsManager`/`serviceRegistry` length on deserialize is read from the stream and "may differ from current build's `TypeId<TBase>.Count`" (`UnsafeIndexedRegistry.cs:113` note) — cross-build save compatibility is fragile; `unknown` how migrations handle it.
