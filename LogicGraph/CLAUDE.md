# LogicGraph

> **🛑 ЧАСТИЧНО УСТАРЕЛО (2026-06-14). Снимок как-построено: [Plan/STATE.md](Plan/STATE.md) — читать
> первым.** После Фазы 4 модель переосмыслена: **5 scope → 3 региона** (`MemoryRegion{Static,Cache,
> Persistence}`); **edge-модель снесена** (`CompiledBlueprint`/`EdgeData`/`NodeInvoker`/`AddNode` удалены),
> заменена Static.Map (`RegionPtr`); рантайм — **off-allocator** (персистентность через будущий слой
> **State**, не снапшот мира). Поэтому **§3 (key types), §4 (five data scopes), §7 (edges/cache),
> §8/«status map»** ниже описывают **до-рефакторную** модель и устарели — сверять с STATE.md. Концепции
> верхнего уровня (#1–#14 в Design reference: оркестратор, версии, контекст, composability) — в силе.

> **⚠️ Work-in-progress / greenfield.** This is a *started, not finished* system. Several entry
> points are stubs (`throw new NotImplementedException()`), the type-index wiring is commented out,
> and the example node (`AddNode`) is explicitly a hand-sketch marked *"Тут кодогенерация"* (codegen
> goes here). This doc describes **how the code is structured today and what it appears to intend** —
> it is a design-review baseline to iterate on, not a contract. Every "appears to / intended" claim
> is flagged; unfinished pieces are collected in §8.
>
> **Implementation:** the phased roadmap is [PLAN.md](PLAN.md); execute a phase with the
> `/logicgraph-phase <n>` command (`.claude/commands/logicgraph-phase.md`) — plan-gate → implement →
> codebase self-review → review packet → your review → close. **Phase 0 done:** EditMode tests live in
> `Tests/` (`Sapientia.LogicGraph.Tests`, Editor-only); run via the Test Runner or the batchmode command
> in [Tests/README.md](Tests/README.md).
>
> **Design intent source:** the FigJam board *"Наработки нодового графа (Октябрь 2025)"*
> ([Winzardy Board, node 1588:7723](https://www.figma.com/board/Xok4N0R7BJUBygFs3Q9Uzo/WInzardy-Board?node-id=1588-7723)).
> It documents the *target* architecture — which is broader than today's code (notably an
> Instruction/Processor execution loop that does not exist in code yet). The intent is folded into
> the sections below and detailed in the **[Design reference](#design-reference)** at the
> end. **Code is the source of truth**; board↔code gaps are flagged.
>
> Parent: [Sapientia root](../CLAUDE.md). Allocator/relative-offset context: [MemoryAllocator](../MemoryAllocator/CLAUDE.md) ·
> pointer wrappers: [Data](../Data/CLAUDE.md).

## 1. Purpose

LogicGraph is a **node-graph compute system**: a designer authors a graph of typed nodes ("blueprint"),
which is **compiled (baked) once from config into a flat, position-independent unmanaged blob in an
arena allocator**, and then **executed under Burst** as unmanaged code. The aim is to let
gameplay/config logic (e.g. a damage formula: read stat → sum → compare → branch) be expressed as
data-flow graphs that run with **no GC and deterministic, Burst-compiled evaluation**, where one
compiled blueprint drives **many instances** via cheap per-instance data.

Two properties drive the whole design (team clarifications 2026-06-03 — see
[Design intent](#design-intent--team-clarifications-2026-06-03)):

- **The compiled graph is data, shipped as a binary.** A "node graph" is just *any* authoring
  abstraction (class hierarchy, formula, visual graph — doesn't matter); the contract is that it
  **bakes into an `unmanaged` blob that serializes to a binary** and is transferred **server → client**.
  Sending the blob ships behavior — the client executes it as authored.
- **Node *functions* are compiled code, not data.** You cannot ship new code server → client, so node
  bodies are **pre-compiled Burst functions**, registered once at startup via
  `BurstCompiler.CompileFunctionPointer` and addressed **by index** (the same pattern as
  `IndexedTypes`/`IndexedTypesInitializer`, `MemoryAllocator/TypeIndexer/IndexedTypes.cs:54`). This makes
  **versioning mandatory**: the binary's node indices/layout must match the client's compiled function
  table (a mismatched blob is rejected). The same logic must also run as **plain .NET** (e.g. server,
  no Burst), and be **deterministic** in both backends.

Execution is orchestrated: an **orchestrator** schedules nodes by **dependency**, runs independent
nodes **in parallel**, and **interleaves Burst and non-Burst passes** (a node that must touch managed
code runs in a non-Burst pass; its dependents wait) until every node of every involved blueprint is
evaluated. Most of this — the orchestrator, the 5-scope data model, typed/composable blueprints — is
**not yet wired in code** (see §8).

Conceptually it splits into two worlds:
- an **authoring / editor side** — managed classes (`Blueprint`, `INode`, the `NodeInput/Output/Body/State` port types) that describe the graph and its default values;
- a **logic / runtime side** — `unmanaged` structs (`CompiledBlueprint`, `BlueprintInstance`, `ILogicNode`) that hold the compiled blob and execute it.

Codegen is meant to bridge the two (one node ⇒ a managed `INode` partial + an unmanaged `ILogicNode` partial); that generator does not exist yet.

## 2. Where it lives

- **Folder:** `Assets/Submodules/Sapientia/LogicGraph/` (inside the `Sapientia` git submodule).
- **Assembly:** `Assets/Submodules/Sapientia/LogicGraph/Sapientia.LogicGraph.asmdef` — assembly name
  `Sapientia.LogicGraph`, `allowUnsafeCode: true`, `autoReferenced: true`. References (by GUID,
  resolved): `Sapientia`, `Sapientia.MemoryAllocator`, `Unity.Burst`, `Unity.Mathematics`
  (`Sapientia.LogicGraph.asmdef:4`).
- **Namespaces:**
  - `Sapientia.LogicGraph` — authoring side (`Blueprint`, `INode`, port types).
  - `Sapientia.LogicGraph.Logic` — runtime/compiled side (`CompiledBlueprint`, `BlueprintInstance`, `BlueprintCompiler`, `ILogicNode`, `NodeInvoker`, `LogicGraph`, `CompiledGraph`, example `AddNode`/`AddLogicNode`).
  - `Sapientia.LogicGraph.Data` — `NodeTypeId` (type→index id).
- **Sub-folders:** `Blueprint/` (authoring graph + node interface), `Data/` (`NodeTypeId`),
  `Logic/` (runtime), `Logic/StaticData/` (compiler + compiled blob), `Logic/ConcreteNode/`
  (the `AddNode` example).

## 3. Key types & entry points

Authoring side (managed):
- `Blueprint/Blueprint.cs:7` — **`Blueprint`**: one authored graph. Holds `INode[] nodes`
  (`[SerializeReference]`), a stable `Id<Blueprint> id`, and connection caches
  (`inputToOutput`, `outputToInputs`, ordered `outputs`, `outputToIndexMap`). **The unit that gets compiled.**
- `Blueprint/INode.cs:42` — **`INode`**: non-generic node contract — `NodeTypeId`, `GetInputs/GetOutputs/GetBodies/GetStates`, `BodySize`/`SetBody`, `StateSize`/`SetStateAndOutput`.
- `Blueprint/INode.cs:10` — **`INode<TLogicNode>`**: generic node bound to an `unmanaged ILogicNode` body type; provides default `NodeTypeId`, `BodySize`, `SetBody` via `TLogicNode`.
- `Blueprint/INode.cs:58`–`:150` — the **port types** authored on a node: `NodeInput<T>` / `NodeStateInput<T>`, `NodeOutput<T>` / `NodeStateOutput<T>`, `NodeBody<T>`, `NodeState<T>`. They carry default values and know how to write them into compiled edge/state memory (`SetValue`, `SetPreCalculated`).

Compiler / compiled data (unmanaged, in arena):
- `Logic/StaticData/BlueprintCompiler.cs:8` — **`BlueprintCompiler`**: top-level compiled container.
  **Start here for the build path.** `CompileAll(Blueprint[])` (`:15`) sizes & creates one `RawBumpAllocator` (bump arena), lays out a per-`Id<Blueprint>` table, compiles each blueprint. `Serialize`/`Deserialize` (`:54`,`:74`) round-trip the whole arena blob.
- `Logic/StaticData/CompiledBlueprint.cs:14` — **`CompiledBlueprint`**: the static, shared-across-instances
  blob for one blueprint. `Compile`/`SetupBlueprint` (`:72`,`:83`) lay out node headers, node bodies,
  edge tables, default edge data, and default node state, and wire each input to the output it connects to.
- `Logic/StaticData/CompiledBlueprint.cs:211`–`:303` — supporting layout types: `NodeHeader`, `NodeId`,
  `EdgeToData`, `EdgeDataHeader`/`EdgeData<T>`, and the typed accessors `InputData<T>`/`OutputData<T>`/`StateData<T>`.

Runtime / execution (unmanaged, Burst):
- `Logic/BlueprintInstance.cs:8` — **`BlueprintInstance`**: per-instance runtime. `Create` (`:20`)
  copies the compiled default node-state into an instance-owned buffer in the `WorldState` arena;
  `BeginRun`/`EndRun`/`ResetEdges` (`:34`,`:39`,`:44`) manage per-run edge data.
- `Logic/ILogicNode.cs:46` — **`ILogicNode`** (+ `<TInput,TOutput>` `:30`, `<TInput,TOutput,TState>` `:7`):
  the unmanaged node behavior; `DoBurst(...)` reads input/output refs out of the compiled blueprint and runs the node.
- `Logic/NodeInvoker.cs:5` — **`NodeInvoker`**: `CompileDoNode<T>()` (`:9`) Burst-compiles a
  `FunctionPointer<DoNode>` that dispatches `(ref CompiledBlueprint, NodeId)` to a node type's `DoBurst`. **The execution dispatch primitive.**
- `Data/NodeTypeId.cs:17` — **`NodeTypeId`**: an `int`-backed type id (intended to come from `IndexedTypes`; currently not wired — §8).
- `Logic/LogicGraph.cs:6` / `Logic/StaticData/CompiledGraph.cs:7` — **`LogicGraph`** / **`CompiledGraph`**:
  intended top-level "graph of blueprints with an entry blueprint" containers — **both are field-only stubs today** (§8).

Example node (illustrative, codegen target):
- `Logic/ConcreteNode/AddNode.cs:10` — **`AddNode`** (authoring) + **`AddLogicNode`** (`:58`, logic) —
  a hand-written sketch of what codegen should emit for one node. Reads as the clearest worked example
  of the input/output/state model; **not production code** (`:8`, `:31`, `:57` comments say so).

## 4. Data / State / Logic / View breakdown

This is plumbing (a compute substrate), not a gameplay `Data/State/Logic/View` feature. Mapping its
parts onto the project's mental model:

- **Data (authoring):** `Blueprint` + `INode[]` and the port objects (`NodeInput/Output/Body/State`) —
  managed, `[SerializeReference]`-serialized graph description and default values. This is the editor-time data.
- **State (runtime):** `BlueprintInstance` (`Logic/BlueprintInstance.cs:8`) — the **mutable per-instance**
  data: an instance-owned `nodesState` copy plus per-run `edgesData`. Lives in the `WorldState` arena.
- **Logic (runtime):** `ILogicNode` implementations (`AddLogicNode`) + `NodeInvoker` (Burst dispatch) +
  `CompiledBlueprint`/`BlueprintCompiler` (compile + read). The `unmanaged`, Burst-compiled behavior.
- **View:** none.
- **Compiled static data:** `CompiledBlueprint`/`BlueprintCompiler` blob — **read-only, shared by all
  instances** of a blueprint, held in a *separate* standalone `RawBumpAllocator` (a `BumpHeader` over raw memory, not the `WorldState` arena — see §7).

### Five data scopes (the authoritative state model)

Nodes and blueprints carry data in **5 distinct scopes**, separated along two axes — **owner** (the
*scope* vs. the *instance*) and **lifetime** (immutable `static` / per-run `cache` / lifetime-long
`persistent`). This supersedes the board's 3-tier sketch.

| Scope | Owner | Mutable | Reset each run | Unique per | Backing freed when | Code seed |
|---|---|---|---|---|---|---|
| **static** | blueprint manager | no | — | **(blueprint id, version)** (**deduped**; global) | no instance references it | `CompiledBlueprint` (`CompiledBlueprint.cs:14`) ✅ |
| **static cache** | scope | yes | yes | compiled node × **usage-site** (**not** deduped) | scope `Dispose` | size+offsets in `CompiledBlueprint` (`DataSizes`/`NodeLayoutOffsets`); block-owner = Phase 3 ◐ |
| **static persistent** | scope | yes | no | compiled node × **usage-site** | scope `Dispose` | size+offsets in `CompiledBlueprint`; block-owner = Phase 3 ◐ |
| **instance cache** | instance | yes | yes | instance | instance `Dispose` | `BlueprintInstance.instanceCache` (`BlueprintInstance.cs:17`, `CachedPtr`; alloc+`ResetCache`) ◐ |
| **instance persistent** | instance | yes | no | instance | instance `Dispose` | `BlueprintInstance.instancePersistent` (`BlueprintInstance.cs:15`, `CachedPtr`; alloc) ◐ |

> **`static` is owned by the blueprint manager (global), not by a scope** — compiled once per
> **(id, version)** and shared across all scopes (this is the dedup). Scopes own only the mutable
> per-scope `static cache` / `static persistent`. *(Interpretation of the 2026-06-03 versioning
> clarification — confirm: is `static` truly global, or can scopes hold divergent blueprint sets? §8.)*

> Earlier these were called just `cache` / `persistent`; the team settled on **`instance cache`** /
> **`instance persistent`** to mirror the `static *` pair and make ownership explicit.

Two subtleties that distinguish *static* from the *static\** scopes, for deps `bp1→bp2`, `bp3→bp2`, and standalone `bp2`:
- **static is deduplicated.** Exactly **3** static blueprints exist — `bp1`, `bp2`, `bp3` — and the
  single `bp2` static is *shared*/referenced by both `bp1`'s and `bp3`'s nodes.
- **static cache / static persistent are per usage-site.** **3 independent** buffers for `bp2` — one
  for the `bp1→bp2` path, one for `bp3→bp2`, one for standalone `bp2`. "static" here means *owned by the
  scope, shared by all instances within it* — **not** deduplicated by blueprint, and **not** per-instance.

### Memory layout & ownership rules

- **Fixed size per scope, known at node compile time.** Each scope's block is a **strictly-determined
  size computed when the node compiles** (the existing `CalculateSizeToReserve` / `*Size` fields are the
  seed, `CompiledBlueprint.cs:51`). Scope blocks are *not* dynamically grown.
- **Inner arrays reference out-of-block memory.** A fixed block may hold fields that are **arrays/refs
  pointing at another memory region**. Ideally **every such object is allocated through our allocator**.
- **All `*persistent` data lives in the allocator** (so it can be snapshotted — see save/load below).
- **`*cache` may use any memory** (allocator or scratch) **but must free it correctly** at run/scope end.
- **`*persistent` must both free correctly and survive save/load**: on serialize/deserialize it must
  **re-initialize or reset** any referenced memory — the **`CachedPtr` pattern** (`MemPtr` + version-checked
  `SafePtr`, see [MemoryAllocator §7](../MemoryAllocator/CLAUDE.md)) is the model to follow.

### `Scope` — a first-class system entity

`Scope` is a **concrete data structure inside the system** (working name **`NodesScope`** or similar)
used to **manage instance lifecycle** within an execution domain. It **owns** the per-scope
`static cache` and `static persistent` blocks (the mutable working sets), allocated **lazily** per
usage-site. **Multiple scopes coexist**, each with its own `static *`. It does **not** own `static` (that
is the blueprint manager's). The scope also holds the **ambient context registry** (`TypeId` → context,
#14) that nodes retrieve from. This is **not built yet** (§8) — `LogicGraph`/`CompiledGraph` are the nearest stubs.

### Blueprint manager — versioning & runtime mutation

A **blueprint manager** owns the compiled **`static`** blobs and handles live editing:
- **Keyed by `(blueprint id, version)`.** `version` acts like an **`Entity` generation**
  ([State §7](../MemoryAllocator/State/CLAUDE.md)) — an instance binds to `(id, version)`
  (`BlueprintInstance` already carries `blueprintId` + `version`, `BlueprintInstance.cs:10`–`:12`), so a
  stale instance (old/deleted version) is **detectable**, and **id reuse is safe** (resolves the
  `Blueprint.id`-reuse hazard noted at `Blueprint.cs:11`).
- **Lazy compilation.** Blueprints compile **on demand** (editor doesn't compile all up front), then cache.
- **Recompile on new version.** If an already-compiled blueprint arrives with a newer version, the manager
  **recompiles**; the new `static` supersedes for *new* instances. **Live instances of the old version
  keep their old `static`** (retained until no instance references it → then freed) so the **running
  simulation does not break**. *(Old-instance policy = graceful retain; confirm — §8.)*
- **Runtime add / remove / change** of blueprints must not corrupt in-flight instances.

> Board↔code naming: the board's **"Blueprint"** (static) = code **`CompiledBlueprint`**; the board's
> **"Нодовый граф" / node graph** (authoring) = code **`Blueprint`**. The board's "Instance Data" +
> "Blueprint Runtime" ≈ **instance persistent** + **instance cache**; the **`static cache`** /
> **`static persistent`** scopes and the **`Scope`** entity are new and have **no code yet**.

## 5. Lifecycle & tick

This system is **not yet registered with any `World`/`WorldBuilder`** (no `IWorldStatePart`/`IWorldSystem`
found here; grep), so there is **no tick wiring yet** — it is currently a set of building blocks. The
*intended* lifecycle, read from the code:

1. **Author** — designer builds `Blueprint` objects (nodes + connections). Who populates the connection
   caches (`inputToOutput`, `outputs`, `outputToIndexMap`) is **`unknown`** — no editor/builder code is present (§8).
2. **Compile** — `BlueprintCompiler.CompileAll(blueprints)` (`BlueprintCompiler.cs:15`) reserves one arena
   sized by `CompiledBlueprint.CalculateSizeToReserve` (`CompiledBlueprint.cs:51`), then `CompiledBlueprint.Compile`
   → `SetupBlueprint` (`:72`,`:83`) lays out and wires each blueprint. Output: `SafePtr<BlueprintCompiler>`.
3. **(Optional) Serialize** — the compiled arena blob is written/read via `BlueprintCompiler.Serialize`/`Deserialize`
   (`:54`,`:74`) and `RawBumpAllocator`/`BumpHeader.Serialize`.
4. **Instantiate** — `BlueprintInstance.Create(worldState, compiledBlueprint, instanceId)`
   (`BlueprintInstance.cs:20`) allocates an instance state buffer in `WorldState` and copies compiled defaults in.
5. **Run (per evaluation)** — intended: `BeginRun(edges)` → invoke each node's Burst `DoNode` function
   pointer (`NodeInvoker`) → `EndRun()` / `ResetEdges()`. **This run loop / scheduler does not exist yet**
   (no caller drives `NodeInvoker`, no evaluation-order computation, no `IsCalculated` propagation — §8).

**Messages sent / consumed:** none found.

## 6. Dependencies

- **Depends-on:**
  - [MemoryAllocator](../MemoryAllocator/CLAUDE.md) — `BumpHeader` (`Memory/BumpAllocator/BumpHeader.cs`,
    the position-independent bump-allocator header backing all compiled data) + the
    `RawBumpAllocator` / `MemBumpAllocator` block-provider wrappers, `WorldState`/`CachedPtr`/`MemAlloc`
    for instances (`BlueprintInstance.cs:20`,`:28`).
  - [Data](../Data/CLAUDE.md) — `SafePtr`/`SafePtr<T>`, `PtrOffset`/`PtrOffset<T>` (the
    relative-offset model that makes the blob position-independent), `Id<T>`, `ByteEnumMask<T>` (edge header flags).
  - `Sapientia.TypeIndexer` (part of `Sapientia.MemoryAllocator`) — `NodeTypeId` intends to use
    `IndexedTypes.GetTypeIndex` (currently commented out, `Data/NodeTypeId.cs:13`); also the mechanism for
    the **scope's ambient context registry** (`TypeId` → context, #14) and node-function dispatch by index.
  - `Sapientia.Extensions` (`TSize<T>.size`, `UnsafeExt.As`), `Submodules.Sapientia.Memory` (`MemoryExt.MemCopy`, `StreamBuffer*`).
  - `Unity.Burst` — `[BurstCompile]` + `FunctionPointer<DoNode>` (`NodeInvoker.cs`).
  - `UnityEngine` — only `[SerializeReference]`/`[Serializable]` on the authoring side (`Blueprint.cs:18`, `AddNode.cs:7`).
- **Depended-by:** **none yet** — no other assembly references `Sapientia.LogicGraph` (grep). This is a leaf, pre-integration.

## 7. Gotchas & invariants

- **Two distinct allocators.** Compiled, *static, shared* data lives in a **standalone `RawBumpAllocator`**
  (a `BumpHeader` over raw `MemoryExt`) created by `BlueprintCompiler.CompileAll` (`BlueprintCompiler.cs:33`);
  *mutable, per-instance* data lives in the **`WorldState` arena** (`BlueprintInstance.cs:28`). Do not
  confuse the two — they have different lifetimes and owners, and an instance points into both. (The
  `Allocator`-backed counterpart of the raw wrapper is `MemBumpAllocator`, used for `*persistent` that rides
  the world snapshot — Phase 3+.)
- **Everything is relative-offset / position-independent.** Compiled data never stores raw pointers; it
  stores `PtrOffset`/`PtrOffset<T>` (byte offsets) and reaches the allocator through a *self-relative*
  `PtrOffset<BumpHeader> allocatorOffset` (`CompiledBlueprint.cs:16`, set via
  `BumpHeader.CreateRelativeOffset`). `BumpHeader` itself is position-independent — its base is derived from
  the struct's own address (`Memory => this.AsSafePtr(_reservedSize)`, header at byte 0), so it stores no
  absolute pointer. This is what makes the whole arena serializable as one blob and relocatable.
  **Invariant:** never cache a `SafePtr` into compiled data across a serialize/deserialize or a move —
  re-derive from the offset. **Corollary:** never copy a `BumpHeader` by value (that moves `&this` off its
  block) — touch it only through `SafePtr<BumpHeader>`/`PtrOffset<BumpHeader>`/`ref`.
- **`BumpHeader` is bump-only, fixed-size, never frees individual objects.** `MemAlloc` just advances
  `_rover`; the owning wrapper's `Dispose` frees the whole block. So `CalculateSizeToReserve`
  (`CompiledBlueprint.cs:51`) **must** exactly account for every `MemAlloc` in `SetupBlueprint`, or the arena
  overflows / under-reserves. The two are kept in lockstep by hand (numbered comments `// 1..5` in both
  methods) — **change them together**.
- **`Blueprint.id` must be reused, not grown.** Per the code comment (`Blueprint.cs:11`–`:14`, Russian):
  ids index into a contiguous compiled table (`BlueprintCompiler` sizes it to `maxId + 1`,
  `BlueprintCompiler.cs:25`); leaving "holes" by ever-growing ids wastes/breaks the table. On delete, the id must be recycled.
- **Output is the canonical datum; inputs are references to outputs.** `SetupBlueprint` assumes **every**
  input is present in `blueprint.inputToOutput` (`CompiledBlueprint.cs:152`) — even a constant default is
  modeled as a "pre-calculated" output (`EdgeDataHeader.IsCalculated`, `CompiledBlueprint.cs:120`). A node's
  input edge just stores the offset of the output it reads.
- **"Pass-through state" edges.** A `NodeStateOutput<T>`/`NodeStateInput<T>` means the value physically
  lives in a node's mutable `State` and the edge stores a *reference* (offset) into that state rather than a
  value (`EdgeDataHeaderState.PassThroughRef`, `CompiledBlueprint.cs:238`–`:243`). This is how outputs become
  mutable/persisted and readable by other nodes. `InputData<T>.ReadData` branches on `PassThroughRef` to follow the indirection.
- **Determinism across two backends.** The same node logic must execute **under Burst** and as **plain
  .NET** (e.g. server) with **identical, deterministic** results. So: no wall-clock/`Random`, and watch
  for Burst-vs-Mono divergence (float intrinsics, `IndexedTypes` Burst-discard paths). Determinism of
  *evaluation order* is also **unverified** — no scheduler exists yet (§8), so node ordering currently =
  array order in `blueprint.nodes` (`CompiledBlueprint.cs:140`). Confirm the intended stable order before relying on it.
- **Scope memory ownership (per §4).** Each scope block is a **compile-time fixed size**; inner
  arrays/refs point at separately-allocated memory that **should come from our allocator**. `*persistent`
  data **must live in the allocator** and survive **save/load** by re-initializing/resetting referenced
  memory (the `CachedPtr` pattern). `*cache` may use scratch memory but **must free it** at run/scope end.
  Getting this wrong leaks (cache) or corrupts snapshots (persistent).
- **Save/load = `*persistent` only.** Graph state serialization persists **all `*persistent` data**;
  `*cache`/`static` are either transient or rebuilt. A serialized instance must reload with its persistent
  memory re-wired (don't serialize raw `SafePtr`s — only stable handles).
- **`stackalloc` in the compile path.** `SetupBlueprint` uses
  `stackalloc PtrOffset<EdgeDataHeader>[blueprint.outputs.Length]` (`CompiledBlueprint.cs:114`); the inline
  comment flags that large graphs should move this off-stack. Watch for stack overflow on big blueprints.
- **Burst function-pointer compilation cost.** `NodeInvoker.CompileDoNode<T>` calls
  `BurstCompiler.CompileFunctionPointer` (`NodeInvoker.cs:11`) — this is expensive and must be done once per
  node type and cached, not per-run. The intended pattern is a startup registry that compiles every node
  function and exposes it **by index** (mirroring `IndexedTypes.Initialize`,
  `MemoryAllocator/TypeIndexer/IndexedTypes.cs:54`, which fills a `Delegate[]` addressed by index). No such
  registry exists yet (§8).
- **Versioning is load-bearing, not cosmetic.** Because the compiled blueprint is a binary shipped
  **server → client** while node *functions* are pre-compiled into the client, the blob's node
  indices/layout must match the client's function table. The `version` fields already present
  (`Blueprint.version` `Blueprint.cs:9`, `CompiledBlueprint.version` `CompiledBlueprint.cs:18`,
  `BlueprintInstance.version` `BlueprintInstance.cs:10`) are the seed of this contract — a version mismatch
  between a received blob and the local function registry must be **detected and rejected**, not executed.
  The check itself is **not implemented yet** (§8).

## 8. Open questions / TODO / risks

**This system is unfinished; the items below are the design surface to work through together, not bugs to silently fix.**

**Phase 3 progress (2026-06-08) + follow-ups:**
- **`BlueprintCompiler` removed**, superseded by **`CompiledBlueprintStorage`** (`Logic/StaticData/CompiledBlueprintStorage.cs`)
  — off-allocator store of compiled blobs keyed by `(Id<Blueprint>, version)`; batch `Add(arena, offsets)`,
  dedup, coexisting versions, jump-by-id; **never removes** (Dispose-only). `LogicGraph` stub now holds a
  `CompiledBlueprintStorage`. **Prose refs to `BlueprintCompiler` in §3/§5/§6/§7 are stale** — pending a
  fuller CLAUDE.md reconciliation.
- **Scope → state invariant** (decided 2026-06-08): **off-state** = `Static`, `StaticCache`, `InstanceCache`
  (off-allocator); **in-state (snapshot)** = `StaticPersistent`, `InstancePersistent` (allocator/world).
  Phase-3 storage (Static) is off-allocator ✓.
- **Follow-up (Phase 2 code):** `BlueprintInstance.instanceCache` is currently allocated in `worldState`
  (in-state), contradicting "InstanceCache off-state" — move it off-allocator (likely during the Phase-4
  instance rework).
- **Phase reorder:** Phase 3 = storage, **Phase 4 = `ExecutionScope`** (built on the storage; worldState-agnostic,
  off-allocator, context passed into execution methods at M7). See `Plan/phase-3/` for the authoritative detail.

Stubs / not-yet-implemented:
- `INode.GetInputs/GetOutputs/GetBodies/GetStates` default impls all `throw new NotImplementedException()`
  (`Blueprint/INode.cs:24`–`:39`). The author's own comment (`:23`, Russian) questions whether ports should
  come from the node at all or be derived from the blueprint. **Open design question — and it blocks the
  compile path**, since `SetupBlueprint` calls `node.GetInputs()/GetOutputs()` (`CompiledBlueprint.cs:149`,`:162`).
- `ILogicNode<TInput,TOutput>.DoBurst(...)` 4-arg overload `throw new NotImplementedException()`
  (`ILogicNode.cs:25`).
- `NodeTypeId<T>` static ctor body is commented out (`Data/NodeTypeId.cs:13`), so
  `NodeTypeId.Create<T>()` returns `default` (index `0`) for **every** type → all node types currently share
  one id. The `IndexedTypes.GetTypeIndex` wiring must be enabled before type dispatch works.
- `LogicGraph` (`Logic/LogicGraph.cs:6`) and `CompiledGraph` (`Logic/StaticData/CompiledGraph.cs:7`) are
  field-only stubs — no methods, and nothing produces a `CompiledGraph`. The "graph of blueprints + entry
  point" concept is declared but not built.

Missing wiring (the run path is not connected end-to-end):
- **Instance data is not threaded into execution.** `NodeInvoker.DoBurst` (`NodeInvoker.cs:15`) and the node
  `DoBurst` overloads receive only `ref CompiledBlueprint` (the *static* blob) — no `BlueprintInstance`. So
  the per-instance `nodesState` and per-run `edgesData` on `BlueprintInstance` (`BlueprintInstance.cs:15`,`:18`)
  are currently unreachable from the run. Relatedly, `ILogicNode<…,TState>.DoBurst` builds
  `TState state = default` on the **stack** and passes that (`ILogicNode.cs:18`–`:19`) instead of the
  instance's real state buffer. **How instance state/edges reach `DoBurst` is the central unresolved question.**
- **No scheduler / evaluator.** Nothing computes node execution order, drives `NodeInvoker`, or propagates
  `EdgeDataHeader.IsCalculated` for lazy/dirty evaluation. `IsCalculated` is only set at compile time for
  pre-calculated constants (`CompiledBlueprint.cs:120`).
- **No `FunctionPointer` cache.** `CompileDoNode<T>` is never called from a registry/cache; a per-node-type
  table (`NodeTypeId` → compiled `DoNode`) is implied but absent.
- **Edge-buffer ownership unclear.** `BlueprintInstance.Create` copies default *state* but not default
  *edges*; `BeginRun(newEdgesData)` expects the caller to supply an `edgesData` buffer (`BlueprintInstance.cs:34`)
  and `ResetEdges` fills it from compiled defaults (`:44`) — but **who allocates that buffer, and when, is `unknown`**.
- **Connection caches unpopulated.** No code populates `Blueprint.inputToOutput` / `outputToInputs` /
  `outputs` / `outputToIndexMap` (`Blueprint.cs:22`–`:27`). An editor/graph-build step is assumed but absent.

Illustrative-only code:
- `AddNode`/`AddLogicNode` (`Logic/ConcreteNode/AddNode.cs`) is explicitly a sketch with `// Тут кодогенерация`
  ("codegen goes here") markers (`:31`,`:57`) and design musings in comments. Treat it as a **spec example** of
  the intended input/output/state shape, not as a real node. The codegen that would emit such partials does not exist.

Inline author notes / comments worth resolving (all Russian, paraphrased):
- `Blueprint/INode.cs:23` — "inputs/outputs probably shouldn't be passed from the node, maybe get them from the blueprint, dunno."
- `Logic/ILogicNode.cs` (around `DoBurst`) — "probably several methods: one Burst-capable, some not."
- `Logic/ConcreteNode/AddNode.cs:13`,`:19` — port-binding via attributes vs wrapper types is undecided.

No `TODO`/`FIXME`/`HACK`/`BUG-xxx` markers exist in the folder (grep) — the open work is captured in prose comments and stubs instead.

Big design pieces from the clarifications/board **entirely absent from code** (the largest gap — see
[Design intent](#design-intent--team-clarifications-2026-06-03)). These are the things to design together:
- **Execution orchestrator** — schedules nodes by **dependency**, runs independent nodes **in parallel**,
  and **interleaves Burst / non-Burst passes**: a node that can't run in Burst waits for a non-Burst pass,
  and all of its dependents wait for it; passes alternate until every node of every involved blueprint is
  done. Executing one blueprint is a **special case** of executing a group of blueprints. The board's
  Instruction → Processor → Continue loop is the single-threaded sketch of this; the clarifications add the
  **dependency graph + parallelism**. Code has only `NodeInvoker.DoNode` (one Burst dispatch).
- **`Scope` entity (`NodesScope`) + five data scopes** (§4). Only `static` (`CompiledBlueprint`) and the
  `instance cache`/`instance persistent` blocks (`BlueprintInstance`) have any code; the **`Scope`
  lifecycle entity** and both **`static cache` / `static persistent`** scopes are unbuilt, as are the
  layout rules (compile-time fixed size, allocator-backed inner arrays).
- **Blueprint manager (versioning + runtime mutation)** — global registry `(id, version) → static`, lazy
  compile, recompile-on-new-version with **retain-old-for-live-instances**, ref-count/free, runtime
  add/remove. Instances bind to `(id, version)` (`version` ≈ `Entity` generation for staleness). The
  `version` fields exist (`BlueprintInstance.cs:10`); the manager does not.
- **Dual execution backend** — a **plain .NET** (non-Burst) path alongside the Burst path, deterministic
  in both. Only the Burst dispatch (`NodeInvoker`) is sketched; no managed backend.
- **Save/load of `*persistent` graph state** — serialize all persistent instance data and re-wire its
  referenced (allocator) memory on load (`CachedPtr` pattern). Compiler blob serialization exists
  (`BlueprintCompiler.Serialize`), but **instance/persistent-state** save/load does not.
- **Blueprint composability + typed/dynamic I/O via input/output nodes** — a blueprint must be usable **as
  a node**, and its I/O surface is a set of **explicit input/output nodes** = **multiple typed entry points**
  (e.g. `Start()` / `Update(float)`), not one `(TInput)→TOutput`. External code picks the data, the input
  node to invoke (`ExecRef`), and the output node to read. A **universal passthrough node** implements ports
  as a **zero-cost jump** (seed: `EdgeDataHeaderState.PassThroughRef`). Capability interfaces still apply
  (board: `MyValue : IEntityContainer, IStatLogicContainer`; `ReadStat` consumes them). Code has only the
  empty `LogicGraph`/`CompiledGraph` stubs.
- **Execution references as edge values (+ multiple I/O)** — an `ExecRef` value type flowing on outputs,
  in template `(id, version, node id)` or instance `(instance id, node id)` form, targeting a
  node/group/blueprint; invoking it starts execution from there on any/that instance (the invocation
  mechanism for blueprint-as-node and callbacks; generalizes the board's Instruction). Blueprints take
  **multiple inputs**. None in code; reuses the Phase 3/4 identity model.
- **Non-Burst blueprint/node functionality** — explicit support for nodes that must touch managed code
  (e.g. access a class), integrated with the orchestrator's non-Burst pass.
- **Versioned binary transfer** — server→client blob + version gate against the local compiled-function
  registry (§7). `version` fields exist; the registry and the check do not.
- **Pull-based memoized evaluation** — *Is Calculated* gating of node execution; declared
  (`EdgeDataHeader.IsCalculated`) but never read at runtime.

**Open design questions needing the user (versioning, 2026-06-03):**
- Is **`static` global** (one compiled blob per `(id, version)`, shared by all scopes — current
  interpretation) or **per-scope** (scopes may hold divergent blueprint sets)?
- **Old-instance policy** on recompile: keep live instances on their **old version's** retained `static`
  until they dispose (graceful — current interpretation), or migrate/invalidate them?
- Is the **blueprint manager** a separate entity from `NodesScope`, or does the scope contain it? (Leaning
  separate: manager owns global `static` by `(id,version)`; scope owns per-scope `static *` + instances.)

### Active design decisions (in progress)

**Universal `ArenaAllocator` backend** — ✅ **implemented in Phase 1 (2026-06-04)** as `BumpHeader` +
`RawBumpAllocator` / `MemBumpAllocator` (see [PLAN.md → Phase 1 status](PLAN.md)). The direction below is
kept for context but **diverged**: `BumpHeader` (`Memory/BumpAllocator/BumpHeader.cs`) is
**position-independent** — its base is derived from the struct's own address (header at byte 0 of its
block), so it stores **no absolute pointer** and needs **no cached base / version re-resolve**. It owns no
memory; allocation/free live in two thin wrappers — `RawBumpAllocator` (raw `MemoryExt`) and
`MemBumpAllocator` (`MemoryAllocator/BumpAllocator/`, main `Allocator` via a stable `MemPtr`, resolved from
`WorldState` per access). The tagged-struct/cached-base mechanism sketched below was dropped as unneeded.

Blob/state scopes (`static`, `*cache`, fixed-size `persistent`) are laid out with `ArenaAllocator`, but
today it (a) takes its backing block only from raw `MemoryExt.MemAlloc` and (b) resolves only through a
fixed `SafePtr` (`ArenaAllocator.cs:26`,`:70`). It must become **backend-agnostic** so the same layout
code can be backed by either:
- **raw native** (`MemoryExt.MemAlloc`) — for the **static blob** shipped standalone server→client (the
  arena block *is* the serialized binary; base ptr never moves);
- **the main `Allocator`** (`Allocator.MemAlloc`, `MemPtr`) — for **`instance persistent`** data that must
  ride the **world snapshot** (base moves on deserialize → re-resolve via the **`CachedPtr` pattern**).

Design direction:
- Abstract only a **narrow block-provider seam** — `AllocateBackingBlock(size)` / `ResolveBase(handle)` /
  `FreeBackingBlock(handle)` — **not** the full allocator API. The arena keeps its own `_rover` bump +
  `PtrOffset` math inside the block; per-object free/realloc is unneeded (blocks are bump-filled and freed
  wholesale at scope/instance `Dispose`).
- Implement the backend as a **tagged struct** (kind enum + a `MemPtr` handle that is `Invalid` for the raw
  backend) rather than `[StructLayout(Explicit)]` union (simpler, equally Burst-friendly) — and **not** as
  `ArenaAllocator<TBackend>` generic, which would virally infect `CompiledBlueprint`/`BlueprintInstance`
  and break heterogeneous storage. `[StructLayout(Explicit)]` union only if a backend's state grows.
- **Cache the resolved base** in the arena and refresh it through the seam **only on `WorldState.Version`
  change**, so per-deref cost stays `_memory + offset`; the backend branch is amortized to once/snapshot.
- **Consequence to accept:** the `Allocator` backend needs `WorldState`/`Allocator` context to resolve
  `MemPtr→SafePtr`, so accessors shift toward `GetPtr(offset, worldState)` (the raw backend ignores the
  context). Consistent with the rest of the codebase; aligns with the pending instance-data threading.
- **Open sub-questions:** does the static (raw) path also pay the `WorldState` param for signature
  uniformity, or keep a context-free fast path? Where does the `Allocator`-backend handle store the
  allocator reference under Burst (via `WorldState` passed in, presumably)?

**LogicGraph ↔ allocator coupling** (decided 2026-06-03). LogicGraph **does not require a live world
`Allocator`** to run: it depends on the *memory abstraction* (the universal-backend arena above). The
**default/standalone path is raw `MemoryExt`** — works on a server, in tests, and for shipped binaries
with **no `World`**. The **`Allocator` backend is opt-in**, used only when data must ride the world
snapshot (`*persistent`). At the **assembly** level it still references `Sapientia.MemoryAllocator`
(unavoidable: both the optional `Allocator` backend *and* `TypeIndexer` live there) — so the goal is "no
**mandatory live allocator instance** at runtime," not "no MemoryAllocator reference." A hard split
(allocator-free core + integration assembly) is possible later but not justified now.

**Ambient context registry on `Scope`** (decided 2026-06-03). The scope holds a **`TypeId` → context**
registry (via `TypeIndexer`; contexts as `IndexedPtr`/`ProxyPtr`); nodes retrieve the context type they
need at any execution point — the precedent is `WorldState.ServiceRegistry.cs`. Separate from per-call
edge parameters. Not built; closely tied to the `Scope` work (Phase 3) and node execution (M7).
*Open sub-question:* allocator-free contexts use `UnsafeIndexedPtr`/`UnsafeProxyPtr` (no `MemPtr`),
world-backed contexts use `IndexedPtr` (`CachedPtr`) — confirm both are supported per the backend stance.

> **Notion cross-reference:** none found for this system in `Docs/Core/DOCUMENTATION_PLAN.md`.

---

## Design reference

The authoritative design intent is the **team clarifications** below (newest); the **FigJam board** is
the earlier sketch they refine. Both describe the *target* — **code is the source of truth**, and the
status of each piece is tracked in the [design → code map](#design--code-status-map).

### Design intent — team clarifications (2026-06-03)

1. **"Node graph" is an abstraction** — it can be *anything* (a class hierarchy, a formula, a visual
   graph). It is only the **authoring** front-end; its sole hard requirement is #2.
2. **It must compile to an `unmanaged` structure** that is **saved to a binary**. The binary is how
   behavior is shipped: **transfer the blob server → client and it executes as authored** (the blob *is*
   the program / "codebase transfer").
3. **Node functions are compiled code, not data** — you cannot ship new code server → client. Hence:
   1. the system needs **versioning** (blob ↔ local function table); a mismatched blob is **rejected**;
   2. functions must run **under Burst**, compiled at startup via `BurstCompiler.CompileFunctionPointer`
      (analogous to `IndexedTypesInitializer` / `IndexedTypes.Initialize`,
      `MemoryAllocator/TypeIndexer/IndexedTypes.cs:54`) and **addressed by index**;
   3. **dual execution backend**: the same logic must run both **under Burst** *and* as **plain .NET**
      (e.g. on a server without Burst). In both backends the logic must be **deterministic**.
4. **Node** = a block with **data + functionality**, exposing **input/output**.
5. **Blueprint** = a set of interconnected nodes.
   1. a blueprint can itself act **as a node** (composability / nesting);
   2. a blueprint can have **input/output**:
      1. configurable **dynamically**, and
      2. configurable **via code** — strongly-typed blueprints with a defined I/O type, so other code can
         call them, pass a context, and receive a result.
6. **Blueprints may have non-Burst functionality** (e.g. needing to reach a managed class).
7. **There must be an execution orchestrator**:
   1. can run nodes **in parallel**, honoring **inter-node dependencies**;
   2. a node that **can't run in Burst** waits for a non-Burst pass; its **dependents wait** for it;
   3. Burst / non-Burst passes **alternate** until all nodes of all involved blueprints are executed;
   4. running a **single** blueprint is a **special case** of running a **group** of blueprints.
8. **Five data scopes** — `static`, `static cache`, `static persistent`, `instance cache`,
   `instance persistent`. Each has a **fixed size known at node compile time**, may hold inner
   arrays/refs to other memory (ideally allocated **through our allocator**); `*persistent` lives in the
   allocator, `*cache` may use any memory but must free correctly. Full semantics (owner × lifetime,
   dedup vs per-usage-site, layout/ownership rules) are in **§4**.
9. **`Scope` is a first-class entity inside the system** (working name `NodesScope`) — a data structure
   that **manages blueprint/instance lifecycle** and **owns** the `static *` blocks. Many scopes coexist;
   each lazily allocates its own `static *` data. See **§4**.
10. **Graph state must save/load** — specifically **all `*persistent` data**. On serialize/deserialize,
    referenced (out-of-block) memory must be correctly re-initialized/reset (the `CachedPtr` pattern).
11. **Blueprint versioning + runtime mutation** — blueprints are **versioned**, and may be **added,
    removed, or changed at runtime** without breaking the running simulation. A **blueprint manager**:
    1. compiles blueprints **lazily** (editor compiles on demand, not all up front);
    2. if an already-compiled blueprint arrives with a **new version → recompiles** it (the new version
       supersedes; see open question on old instances);
    3. keys instances by **blueprint type *and* version** — `version` acts like an **`Entity` generation**:
       a stale instance (old/deleted version) is detectable, so id reuse is safe.
12. **Execution references as first-class output values; multiple I/O.** A node/blueprint can output an
    **execution reference** — a handle targeting a **node, a group of nodes, or a blueprint** — in
    **template** form (static `(blueprint id, version, node id)`, applicable to **any instance**) or
    **instance** form (bound: `(instance id, node id)`).
    1. Invoking it **starts execution from that node/group/blueprint**; the template form is parameterized
       by an instance at call time, so one ref drives any instance.
    2. It is the graph-level analogue of a **function pointer / delegate / continuation** — enables
       callbacks, higher-order sub-graphs, deferred execution, and is **how blueprint-as-node (#5.1) is
       invoked**. It generalizes the board's **Instruction = {Node Ref, Instance Ref}** (instance form).
    3. It **reuses the foundation identity model** — `(id, version)` (#11) and instance id (#9) — so
       **version-as-generation staleness protects refs**: a ref to an old/deleted version is detectable.
    4. A blueprint may have **multiple inputs** (and multiple outputs) — see #13 for what "input" means.
13. **Blueprint I/O is defined by explicit input/output *nodes* (multiple typed entry points).** A
    blueprint has **no single `(TInput)→TOutput` signature**; instead it designates **input nodes** and
    **output nodes** explicitly, like an object exposing several methods.
    1. "Multiple inputs" = **multiple context types / entry points** — e.g. call `Start()` or
       `Update(float)` on the same blueprint. Each input node dictates what data goes in for that entry.
    2. **External code chooses** which data to send, **which input node to invoke** (via an `ExecRef`,
       #12 — invoking `Update(float)` = starting execution from the `Update` input node), and **which
       output node** to read the result from.
    3. A **universal passthrough node** can take whatever parameters you configure and emit the same on
       output, but **occupies no extra memory** — it is a pure **jump / alias** (an intermediate
       reference, not a data copy). Input/output nodes are realized via this zero-cost passthrough; the
       code seed is the **`EdgeDataHeaderState.PassThroughRef`** flag (`CompiledBlueprint.cs:279`).
    4. The board's `Start(MyValue)` / `Exit(float)` nodes are exactly such input / output nodes.
14. **Ambient context lives in the `Scope`, retrieved by type.** Besides per-call edge parameters (#13),
    nodes need **ambient context**, and **different nodes need different context types**.
    1. You **create a scope, place global context object(s) into it**, and at **any execution point** a
       node can **retrieve the context it needs by type**.
    2. Mechanism: the existing **`TypeIndexer`** feature (`Sapientia.TypeIndexer`, part of
       `Sapientia.MemoryAllocator`, already referenced) — a `TypeId`-keyed registry, like `WorldState`'s
       service registry (`WorldState.ServiceRegistry.cs`); contexts stored as `IndexedPtr`/`ProxyPtr`
       (`TypeId` + pointer, `IndexedPtr.cs`) and accessed Burst-side via proxies. This realizes the
       board's capability-context idea (`MyValue : IEntityContainer, IStatLogicContainer`; `ReadStat`
       pulls those).
    3. **Two distinct context channels:** (a) **edge/parameter** context flowing through input nodes
       (per call, #13); (b) **ambient scope** context retrieved by type (available everywhere).

### FigJam board (earlier sketch)

> Source: **"Наработки нодового графа (Октябрь 2025)"** —
> [Winzardy Board, node 1588:7723](https://www.figma.com/board/Xok4N0R7BJUBygFs3Q9Uzo/WInzardy-Board?node-id=1588-7723).
> Read for **intent**; code is the source of truth. Captured 2026-06-03; the board is dated Oct 2025.
> Where it differs from the clarifications above, the clarifications win (e.g. 5 scopes supersede the
> board's 3 tiers; the orchestrator generalizes the board's single-threaded Instruction loop).

### Intended pipeline

```
Node graph (authoring, managed, from config)
        │  bake once (must produce unmanaged)
        ▼
┌─ Static data ───────────────────────────────────────────────┐
│ Blueprint = Node[]                                           │
│   Node: Input(refs→temp) · Data(static params) · Output(refs→temp) │
│        + Cache Size + Instruction Id (switch dispatch, codegen) │
│   Temp Data Refs: per Output field → {node ref, data ref}    │
└──────────────────────────────────────────────────────────────┘
        │  per instance
        ├─ Instance Data (persistent, fixed size, 1 per instance)
        │     Variables (graphVar<T>) · Blueprint Ref · Blueprint Runtime Ref
        └─ Blueprint Runtime (temporary, lives 1 tick)
              Cache[node] = { Is Calculated, value } · Temp Data (typed slots + node refs)

System-persistent data: parallel lists of Blueprints · Instance Data · Blueprint Runtime
```

### Execution model — Instruction / Processor loop (NOT in code yet)

> This is the board's **single-threaded sketch** of the orchestrator (clarification #7). The current
> design adds an explicit **dependency graph + parallelism** on top; the Burst "Instructions To Do" pass
> and the non-Burst "Processors + Instructions To Continue" pass are the **Burst / non-Burst alternation**.

- An **Instruction** = `{ Node Ref, Instance Ref }` — a unit of "run this node for this instance".
- **Instructions To Do**: queue executed **under Burst**. A node, when run, may **schedule further
  instructions** for the next iteration. The loop repeats until the queue is empty.
- **Instruction Processors** (described as "like ECS systems"): when the queue empties, processors run.
  They may execute **arbitrary (non-Burst) code** and handle batched requests instructions submitted —
  e.g. *casting into physics*. An instruction that needs such work submits a request and parks itself in…
- **Instructions To Continue**: instructions awaiting a processor result. After processors run, each
  parked instruction's **"continue"** method is invoked (under Burst) to consume the result; control
  then returns to the Instructions-To-Do loop. Exit when both queues are empty.
- This is effectively a **coroutine/async model for nodes** that must step outside Burst, while keeping
  the hot path Burst-compiled. `Instruction Id` + a codegen-maintained `switch` is the dispatch
  (code seed: `NodeTypeId` + `NodeInvoker`).

### Evaluation semantics — pull-based memoization

> Board, *Blueprint Runtime → Cache → Is Calculated* (paraphrased from Russian): "Was the node's result
> already computed? If yes — on access, just take the cached value. If no — execute the node and return
> its result." So nodes are evaluated **lazily on demand** and cached for the tick. Code seed:
> `EdgeDataHeader.IsCalculated` (`CompiledBlueprint.cs:286`), currently set only for pre-calculated
> constants at compile time (`CompiledBlueprint.cs:120`), never used to gate runtime execution.

### Typed graphs & node capabilities

- A graph is generically typed by **input context** and **output**: board `MyGraph : Graph<MyValue, float>`.
- The input context advertises **capability interfaces** that nodes require: board
  `MyValue : IEntityContainer, IStatLogicContainer`; the `ReadStat` node consumes `IEntityContainer` +
  `IStatLogicContainer`. Open design question: how node capability requirements are declared and checked
  against the graph's input type (no code yet; `LogicGraph`/`CompiledGraph` are stubs).

### Worked example (board)

A damage-style graph: `Start(MyValue)` → `Log("Начали")` → `Exit(float)`, with a data-flow branch
`ReadStat(StatType=Damage) → Sum(+5f) → Compare(A>B, B=10f) → Condition(yes/no float, no=0) → Exit`.
Use this as the canonical end-to-end target when wiring the runtime.

### Design → code status map

| Design concept (clarification §) | Code today | Status |
|---|---|---|
| Node graph authoring (#1) | `Blueprint` (`Blueprint.cs:7`) | ✅ skeleton |
| Compile to unmanaged `static` blob (#2, scope `static`) | `CompiledBlueprint` (`CompiledBlueprint.cs:14`) | ✅ compiles (blocked by `INode` stubs) |
| Binary save / serialize | — (was `BlueprintCompiler.Serialize`) | ✗ **`BlueprintCompiler` removed in Phase 3**; per-arena serialize will return in Phase 5/M11 on `CompiledBlueprintStorage` |
| Server→client transfer + **version gate** (#3.1) | `version` fields | ✗ check absent |
| Burst functions compiled at startup, **by index** (#3.2) | dispatch-id `NodeHeader.typeId` = `TypeId<ILogicNode>` ordinal (M6-A); адаптер `NodeInvoker.Execute<T>`/`Compile<T>` → `FunctionPointer<ExecuteFn>` (M6-B) | ◐ **M6-A/B**: индекс закрыт; контракт исполнения (`NodeContext`+`ILogicNode.Execute`) и FunctionPointer-адаптер есть (диспатч по индексу, без vtable); реестр-таблица по индексу — M6-C |
| Dual backend: Burst **and** plain .NET, deterministic (#3.3) | `NodeInvoker.Execute<T>` (cross-env) + `Compile<T>` (Burst, `#if UNITY`) / `GetManaged<T>` (.NET) | ◐ **M6-B**: единый адаптер компилируется в обе среды; раздельная сборка таблиц + выбор по `RuntimeType` — M6-C/D |
| Dual backend: Burst **and** plain .NET, deterministic (#3.3) | `NodeInvoker` (Burst only) | ✗ no managed path |
| Bake from config (#1) | `CompiledBlueprint.CompileLayout` → `CompiledBlueprintStorage.Add` | ◐ compile-then-store path exists; no config source wired |
| Node = data + I/O function (#4) | `INode` / `ILogicNode` | ◐ port methods throw |
| Blueprint **as a node** / nesting (#5.1) | — | ✗ absent |
| Blueprint I/O = explicit input/output nodes, multiple typed entry points (#13) | `LogicGraph`/`CompiledGraph`; `Start`/`Exit` board nodes | ✗ empty stubs |
| Universal zero-cost passthrough ("jump") node (#13.3) | `EdgeDataHeaderState.PassThroughRef` (`CompiledBlueprint.cs:279`) | ◐ flag only |
| Execution references on edges (template/instance) (#12) | — (instance-form ≈ board Instruction) | ✗ absent |
| Non-Burst node/blueprint functionality (#6) | — | ✗ absent |
| **Orchestrator**: deps + parallel + Burst/non-Burst (#7) | `NodeInvoker.DoNode` only | ✗ no scheduler |
| `Scope` entity / lifecycle (#9) | `LogicGraph`/`CompiledGraph` | ✗ stubs |
| Ambient context registry on scope, by type (#14) | `ContextRegistry<TContext>` on `ExecutionScope` (`Logic/RuntimeData/ContextRegistry.cs`, 4F-2) | ◐ **4F-2**: scope owns ambient contexts by value — `UnsafeArray<SafePtr>` sized `TypeId<TContext>.Count`, lazy `MemAlloc(TSize<T>)` per `SetContext<T>`; generic-only `SetContext<T>`/`GetContext<T>`→`readonly ref T`/`HasContext<T>`. **Diverges from §14**: plain `SafePtr`+owned block, not `IndexedPtr`/`ProxyPtr`. Node-side retrieval during a run (Burst proxies) → M7 |
| Compiled-blueprint storage / versioning (#11) | `CompiledBlueprintStorage` (`Logic/StaticData/CompiledBlueprintStorage.cs`) | ◐ **Phase 3**: off-allocator store of compiled blobs, batch `Add(arena, offsets)`, dedup + coexisting versions keyed by `(Id<Blueprint>, version)`, jump-by-id lookup; **never removes** (Dispose-only). No lazy-compile (eager/external), no recompile-retain-free. Lifecycle/retain → scope (Phase 4) |
| Instance bound to `(id, version)`; `version` ≈ generation (#11) | `BlueprintInstance.blueprintId`+`version` (`BlueprintInstance.cs:10`) | ◐ fields only; no staleness check |
| 5 data scopes (#8) | `CompiledBlueprint` (`DataSizes`/`NodeLayoutOffsets`/`blockSizes`) + `BlueprintInstance` | ◐ **Phase 2**: all 5 sized & laid out at compile time; `static` + `instance cache`/`instance persistent` blocks allocated; `static cache`/`static persistent` get size+offsets but their block-owner (scope) is Phase 3 |
| Per-scope fixed layout + allocator-backed inner arrays (#8) | `CalculateLayoutSizeToReserve`/`SetupLayout` (`CompiledBlueprint.cs:212`,`:246`) | ◐ **Phase 2**: per-node fixed slot sizing + aligned offsets for all 5 scopes (lockstep-tested); allocator-backed *inner* arrays/refs still later |
| Save/load `*persistent` graph state (#10) | `BlueprintCompiler.Serialize` (static blob) | ✗ instance state save/load absent |
| Pull-based memo (`Is Calculated`) | `EdgeDataHeader.IsCalculated` | ◐ declared, not used at runtime |
| Codegen (node ⇒ partials, switch) | `AddNode` hand-sketch | ✗ generator absent |
