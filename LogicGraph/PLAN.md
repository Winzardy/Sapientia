# LogicGraph — implementation plan

> **Снимок как-построено: [Plan/STATE.md](Plan/STATE.md)** — единый источник правды о том, что реально в
> коде на 2026-06-14 (Static/Runtime 3-региона + каркас runtime/cache/execution). Читать первым; этот
> PLAN.md — роадмап, CLAUDE.md — дизайн-навигатор (его §3/§4/§7 описывают **до-рефакторную** 5-scope/edge
> модель — устарели, см. STATE.md).
>
> Living roadmap for building out the LogicGraph system. Design intent and the code↔design status
> live in [CLAUDE.md](CLAUDE.md); this file is the **phased execution plan**. The **per-phase working
> pattern** (plan → gate → implement → self-review → packet → your review → fixes → close) is the
> `/logicgraph-phase` command at `.claude/commands/logicgraph-phase.md` — run it to execute a phase.
>
> **Decisions locked (2026-06-03):** verification = **EditMode unit tests** (harness introduced in
> Phase 0); plan = **foundation detailed, later phases as milestones**; delivery = **each phase is a
> cohesive set of changes left uncommitted in the working tree** — you review the diff and commit.
> Sapientia is a submodule ("orchestrator handles git"), so Claude does **not** commit here by default.
> *(Исключение — снимок 2026-06-14: по явному запросу пользователя зафиксирован коммитом + push в
> `rnd/nodes_graph`, чтобы продолжить работу в другом чате. См. [Plan/STATE.md](Plan/STATE.md).)*

## How phases are sized

Each phase is **one concept, reviewable in a single sitting**, and leaves the module **compiling with
green tests**. A phase that would exceed ~1 sitting of review is split. The foundation (Phases 0–5) is
pure **memory model + blueprint lifecycle** — it is built and tested **without** node execution, using
minimal *stub nodes* in tests (a node that only declares its scope sizes and does nothing). Execution,
dispatch, codegen, and authoring come later (M6+), once the memory substrate is proven.

## Phase overview

| # | Phase | Concept | Depends on | Status |
|---|---|---|---|---|
| 0 | Test harness | EditMode test asmdef + smoke test; establish the loop | — | ☑ |
| 1 | `BumpHeader` + wrappers | memory-agnostic bump header + raw/`Allocator` block-provider wrappers | 0 | ☑ |
| 2 | Five-scope layout | per-node compile-time sizing + layout of all 5 scope blocks; `static` keyed by (id,version) | 1 | ☑ |
| 3 | Compiled-blueprint storage | `CompiledBlueprintStorage` (evolved `BlueprintCompiler`): batch `Add(arena, offsets)`, dedup + coexisting versions by `(id,version)`, jump-by-id lookup; never removes (Dispose-only) | 2 | ☑ |
| 4 | Static/Runtime 3-region model | **переосмыслена** (см. STATE.md): 5 scope → 3 региона `Static/Cache/Persistence`; снос edge-модели; Static = Data+Map(`RegionPtr`); instance identity (`BlueprintInstanceHeader/Storage/Id` + generation); **каркас** runtime/cache/execution (`DataCache`/`CacheHeader`/`ExecutionGraph`/`NodeMapHeader` — компилируется, поведения нет). `ExecutionScope` отложен (проектируется последним) | 3 | ◐ static-модель done; runtime-каркас + 8 развилок → M6/M7 |
| 5 | Save/load persistent | слой **State** (`Persistence`+`Static`→`Runtime`), **не** снапшот мира; рантайм off-allocator | 4 | ☐ |

> **Реордер (2026-06-07):** scope строится поверх БД, поэтому Фаза 3 ↔ старая Фаза 4 поменялись местами:
> Фаза 3 = Compiled-blueprint DB/manager (эволюция `BlueprintCompiler`), Фаза 4 = `ExecutionScope` (бывшая
> Фаза 3, переделывается поверх БД: массив ленивых per-blueprint менеджеров, индексируемый по
> `Id<CompiledBlueprint>`). Ambient context на scope не хранится — передаётся в методы исполнения (M7).
| M6 | Node dispatch + dual backend | Burst fn-pointer registry by index + managed path + version gate; диспатч на Static.Map (`runtimeType`/`NodeState` wiring) | 5 | ✅ done (A–F закоммичены, [Plan/phase-M6/README.md](Plan/phase-M6/README.md)); single-thread прогон — параллелизм/wave → M7 |
| M7 | Orchestrator | dependency scheduling + Burst/non-Burst passes + parallelism + мульти-блюпринт; `Run` переезжает из `NodeInvoker` | M6 | ◐ **разбивка согласована** (A–D, [Plan/phase-M7/README.md](Plan/phase-M7/README.md)); substrate-каркас (`ExecutionGraph`/`NodeInvoker.Run` single-thread) есть, под-фазы не начаты |
| M8 | Memoized evaluation | pull-based `Is Calculated` gating | M7 | ☐ milestone |
| M9 | Typed + composable blueprints | `Graph<TIn,TOut>`, blueprint-as-node, capability interfaces | M7 | ☐ milestone |
| M10 | Codegen | node ⇒ managed/logic partials + dispatch switch | M6,M9 | ☐ milestone |
| M11 | Authoring + transfer | config baking, binary server→client, end-to-end versioning | M10 | ☐ milestone |

---

## Foundation phases (detailed)

### Phase 0 — Test harness

**Goal.** Stand up the EditMode test mechanism every later phase relies on, and prove the full working
loop on something trivial.

**In scope.**
- A test assembly for LogicGraph (working name `Sapientia.LogicGraph.Tests`, EditMode), referencing the
  Unity Test Framework + `Sapientia.LogicGraph` + `Sapientia` + `Sapientia.MemoryAllocator`.
- One smoke test (e.g. allocate an `ArenaAllocator`, write/read a value) that passes.
- A short `Tests/README` note on how to run them (editor Test Runner; batchmode command if available).

**Out of scope.** Any production code change.

**Test list.** harness-runs smoke test; arena round-trips one int.

**Definition of done.** Test Runner discovers and passes the smoke test; no production code touched.

**Status: done (2026-06-03).** Harness lives at `LogicGraph/Tests/` (`Sapientia.LogicGraph.Tests`,
EditMode, Editor-only). Resolved at planning: Unity Test Framework `1.6.0` is present
(`Packages/manifest.json`); the asmdef sits under `LogicGraph/Tests/` (co-located, not submodule-wide);
batchmode runs work via `Unity -runTests -batchmode -testPlatform EditMode` (see `Tests/README.md`) when
the editor isn't holding the project lock. Two smoke tests added: `Harness_Runs`, `Arena_RoundTripsOneInt`.

**Risks / decide at planning.** Is the Unity Test Framework package present in the project? Where should
the test asmdef physically live (under `LogicGraph/Tests/` vs a submodule-wide `Tests/`)? Does a
batchmode test command exist for CI-less local runs, or is running tests a manual editor step?

---

### Phase 1 — Universal `ArenaAllocator` backend

**Status: done (2026-06-04).** Shipped, but the **design diverged from the plan below** (gated and
agreed during implementation). Instead of a tagged-struct backend with a cached base re-resolved on
`WorldState.Version` change, the final shape is:
- **`ArenaAllocator` → renamed `BumpHeader`** (`Memory/BumpAllocator/BumpHeader.cs`) and made
  **memory-agnostic + position-independent**: it owns no backing memory and stores **no absolute
  pointer** — the base is derived from the struct's own address (`Memory => this.AsSafePtr(_reservedSize)`,
  the header lives at byte 0 of its block). So a move/serialize/deserialize needs **no re-resolve and no
  version cache** — the "cached base / `ResolveBase` on version change" mechanism was dropped as unneeded.
  `BumpHeader.Create(SafePtr, int)` only lays out the header in a caller-provided block.
- **Two thin wrapper structs own allocation/free** (the "block provider"):
  `RawBumpAllocator` (`Memory/BumpAllocator/`, raw `MemoryExt` via `Id<MemoryManager>`, base never moves —
  standalone/server/tests/binary) and `MemBumpAllocator` (`MemoryAllocator/BumpAllocator/`, main
  `Allocator` via a stable `MemPtr`, resolves the header from `WorldState` each access — for `*persistent`
  that rides the world snapshot). `Create` allocates, `Dispose` frees.
- **Assembly split resolved without moving `BumpHeader`:** `BumpHeader` + `RawBumpAllocator` stay in
  `Sapientia`; `MemBumpAllocator` lives in `Sapientia.MemoryAllocator` (which already references
  `Sapientia`). No new asmdef, no circular dependency.
- **Open sub-questions resolved:** (1) the raw path keeps a fully context-free API — `BumpHeader` takes no
  `WorldState` at all; (2) `MemBumpAllocator` stores only a `MemPtr` and resolves via `WorldState` — no
  kind-enum/tagged struct needed.
- **Tests run in PlayMode** (`Sapientia.LogicGraph.Tests` asmdef converted) so `MemoryManagerController`
  auto-initializes via `[RuntimeInitializeOnLoadMethod]`. Tests: `BumpHeaderSmokeTests`,
  `BumpAllocatorTests` (raw monotonic/serialize + world round-trip/dispose/snapshot-re-resolve) — green.
- **Side fix:** standalone `WorldState` serialize/deserialize needed a fix (`WorldManager`/`WorldState`)
  for the snapshot re-resolve test to pass.

> The plan text below is the **original pre-implementation sketch**, kept for context; the Status block
> above is what actually shipped.

**Goal.** Make `ArenaAllocator` work over **either** backing memory source without changing its bump +
`PtrOffset` layout model. See the design write-up in [CLAUDE.md §8 → Active design decisions](CLAUDE.md).

**In scope.**
- A narrow **block-provider seam**: `AllocateBackingBlock(size)` / `ResolveBase(handle)` /
  `FreeBackingBlock(handle)`. **Not** the full allocator API.
- Two backends behind a **tagged struct** (kind enum + `MemPtr` handle, `Invalid` for raw): **raw**
  (`MemoryExt.MemAlloc`, base never moves) and **main `Allocator`** (`MemPtr`, base re-resolved on
  `WorldState.Version` change — the `CachedPtr` pattern).
- **Cached base** refreshed only on version change so per-deref stays `_memory + offset`.
- **Additive / backward-compatible:** existing raw call sites in `CompiledBlueprint`/`BlueprintCompiler`
  keep working unchanged; the `Allocator` backend is opt-in.

**Out of scope.** Using the `Allocator` backend from LogicGraph proper (that lands when instance scopes
exist, Phase 3); per-object free/realloc inside the arena.

**Public API sketch (to confirm at planning).**
- `ArenaAllocator.Create(size)` (raw, unchanged) and `ArenaAllocator.Create(ref Allocator, size)` (new).
- Accessors converge toward `GetPtr(offset, worldState)`; raw backend ignores `worldState`.

**Test list.** alloc returns monotonic offsets on both backends; `GetRef` round-trips on both;
`Allocator`-backed base **re-resolves correctly after a simulated move/version bump**; raw base is
stable; `Free` releases on both; arena `Serialize`/`Deserialize` still round-trips (raw).

**Definition of done.** Both backends pass tests; existing `BlueprintCompiler.CompileAll` path unchanged
and green; no `SafePtr` cached across a version bump.

**Open sub-questions (resolve at planning).** (1) Do raw-path accessors also take `worldState` for
signature uniformity, or keep a context-free fast path? (2) The `Allocator`-backend handle stores only
`MemPtr` + `kind` and gets the `Allocator` from the passed `WorldState` at resolve time — confirm.

---

### Phase 2 — Five-scope layout

**Goal.** Represent all **5 data scopes** (`static`, `static cache`, `static persistent`,
`instance cache`, `instance persistent`) with **compile-time-fixed sizes** declared per node, laid out
in the compiled blueprint + instance. Full semantics: [CLAUDE.md §4](CLAUDE.md).

**In scope.**
- A minimal **node sizing API**: each node declares the byte size of each of its 5 scopes at compile
  time (extends the existing `BodySize`/`StateSize` idea, `INode.cs`). **No** port/edge/connection model
  here (that is M9) — sizing only. A node may declare **0** for any/all scopes — **zero-size nodes must
  lay out cleanly** (this is what the universal passthrough / port node will be, #13.3).
- Extend `CompiledBlueprint` layout to reserve + offset all scope blocks, keeping
  `CalculateSizeToReserve` ⟷ `SetupBlueprint` in lockstep (`CompiledBlueprint.cs:51`,`:83`). The `static`
  block carries its `(id, version)` key in its header (the `version` field already exists,
  `CompiledBlueprint.cs:18`).
- Restructure `BlueprintInstance` data into **`instance cache`** + **`instance persistent`** blocks
  (rename/clarify today's `edgesData`/`nodesState`).
- Reset semantics: `*cache` zeroed/reset per run; `*persistent` retained.

**Out of scope.** Who allocates/owns/disposes the scope blocks (Phase 3); the blueprint manager &
versioning (Phase 4); execution; save/load (Phase 5).

**Test list.** per-node 5 sizes sum to the reserved block sizes; offsets are non-overlapping and aligned;
`*cache` reset clears only cache; `*persistent` survives a reset; lockstep assert (computed reserve ==
actual bump) holds for stub-node graphs.

**Definition of done.** A stub-node blueprint compiles with all 5 scope blocks sized and addressable;
reset semantics proven; lockstep invariant test green.

**Risks.** Touches the `INode` surface (currently throwing) — keep the change **sizing-only** to avoid
pulling in the unresolved port model. Watch `stackalloc` growth (`CompiledBlueprint.cs:114`).

---

### Phase 3 — `NodesScope` entity

**Goal.** Introduce the **`Scope`** entity (working name `NodesScope`) that **manages instance lifecycle**
within an execution domain and **owns** the per-scope `static cache` / `static persistent` blocks (per
**usage-site**, lazily allocated). `static` itself is owned by the manager (Phase 4); here it is supplied
by a direct single-version compile.

**In scope.**
- `NodesScope` struct: owns `static cache` / `static persistent` (per **usage-site**, lazy). Backed by the
  Phase-1 `Allocator` backend.
- Instance create/dispose **within a scope**; instance owns its `instance cache`/`instance persistent` and
  references a compiled `static`; instance dispose frees instance blocks; scope dispose frees scope blocks.
- Instance carries `(blueprint id, version)` and resolves its `static` through it (single version for now;
  staleness across recompiles is Phase 4).
- **Multiple coexisting scopes**, each lazily allocating its own `static *`.
- **Ambient context registry** on the scope (#14): a `TypeId` → context map (via `TypeIndexer`, contexts
  as `IndexedPtr`/`UnsafeIndexedPtr`) with **put / get-by-type**; precedent `WorldState.ServiceRegistry.cs`.
  Storage/lookup only — node-side retrieval is M7.

**Out of scope.** The blueprint manager / dedup / versioning / recompile (Phase 4); execution; save/load;
node-side context access during a run (M7).

**Test list (the model-proving tests).**
- **Per-usage-site** `static cache`/`static persistent`: for `bp1→bp2`, `bp3→bp2`, standalone `bp2`,
  there are **3 independent** `static cache`/`static persistent` buffers (one per usage-site).
- multi-scope isolation: two scopes' `static *` are independent; disposing one leaves the other intact.
- lifetime: instance dispose frees instance blocks but not scope blocks; scope dispose frees all.
- context registry: put a context, retrieve it by type; different types coexist; missing type handled.

**Definition of done.** Per-usage-site + multi-scope + lifetime + context-registry tests green; no leaks
(allocations freed on dispose).

**Risks.** This is where ownership bugs hide — emphasize leak/double-free tests. Needs a representative
"usage-site" identity (how a nested-blueprint reference is keyed) — decide at planning. If the context
registry makes the phase too large for one review sitting, **split it into its own sub-phase at planning**.

---

### Phase 4 — Blueprint manager (versioning + runtime mutation)

**Goal.** Own the global compiled `static` blobs and support live editing without breaking the running
simulation. Full intent: [CLAUDE.md §4 → Blueprint manager](CLAUDE.md) and design intent #11.

**In scope.**
- A **blueprint manager** keying `static` by **`(id, version)`** (global, deduped — single shared `static`
  per unique compiled blueprint), backed by the Phase-1 `Allocator` backend.
- **Lazy compilation**: compile on first use, then cache.
- **Recompile on new version**: a newer version supersedes for *new* instances; **old version's `static`
  is retained while any live instance references it**, then freed (ref-count or generation sweep).
- **Runtime add / remove / change** of blueprints; **staleness detection** — an instance bound to an
  old/deleted `(id, version)` is detectable (`version` ≈ `Entity.generation`); id reuse is safe.

**Out of scope.** Execution; binary server→client transfer (M11); save/load (Phase 5).

**Test list (the model-proving tests).**
- **Static dedup**: `bp1→bp2`, `bp3→bp2`, standalone `bp2` → exactly **3** `static` blobs with a **single
  shared** `bp2` static.
- **Lazy**: a blueprint is not compiled until first used.
- **Recompile**: bumping a blueprint's version produces a new `static`; a **pre-existing instance keeps
  running on the old `static`** (retained); a **new instance gets the new version**; old `static` is freed
  once its last instance disposes.
- **Staleness**: an instance of a removed/old version fails its validity check; a reused id with a new
  version does not collide with stale instances.

**Definition of done.** Dedup + lazy + recompile-retain + staleness tests green; no leak of superseded
`static` blobs after their last instance dies; simulation-safe under add/remove/change.

**Open sub-questions (resolve at planning, mirror [CLAUDE.md §8](CLAUDE.md)).** Is `static` global or can
scopes diverge? Old-instance policy = graceful retain (assumed) vs migrate/invalidate? Is the manager a
separate entity from `NodesScope` or contained by it? Retain via **ref-count** vs **generation sweep**?

---

### Phase 5 — Save/load persistent state

**Goal.** Serialize and restore **all `*persistent` data** so graph state survives save/load, re-wiring
any referenced allocator memory.

**In scope.**
- Serialize/deserialize `instance persistent` (+ `static persistent`) blocks.
- On load, **re-initialize/reset** referenced (out-of-block) memory via the `CachedPtr` pattern — never
  persist a raw `SafePtr`, only stable handles.
- Restored instances re-bind to their `(id, version)` via the manager (Phase 4); a missing version is
  handled (compile or reject), not crashed.
- Skip `*cache` (transient) and recompute/rebind as needed.

**Out of scope.** Static-blob transfer/versioning (M6/M11); execution.

**Test list.** round-trip a stub-instance's persistent state (values + referenced arrays) and verify
equality post-load; pointers re-resolve after the load's version bump; `*cache` is not serialized;
restored instance re-binds to its `(id, version)`.

**Definition of done.** Persistent round-trip is byte/value-stable; loaded instance is usable and
correctly re-bound; no stale pointers.

**Risks.** Interaction with the world snapshot (if `Allocator`-backed) vs standalone. Decide at planning
whether persistent save/load piggybacks on the world `Allocator.Serialize` or is independent.

---

## Later milestones (coarse — detailed when approached)

- **M6 — Node dispatch + dual backend.** Startup registry compiling every node's Burst function via
  `BurstCompiler.CompileFunctionPointer`, addressed by index (mirror `IndexedTypes.Initialize`); a
  parallel **plain-.NET** path; deterministic in both; **version gate** rejecting mismatched blobs.
  Resolves `NodeTypeId` (`Data/NodeTypeId.cs:13`) and the `NodeInvoker` cache.
- **M7 — Orchestrator.** Dependency-ordered scheduling, **parallel** independent nodes, **Burst/non-Burst
  pass alternation** (non-Burst node parks; dependents wait), single-blueprint as a special case of group
  execution. Generalizes the board's Instruction/Processor loop. Invocation entry point is an
  **execution reference** (#12): "start from this node/group/blueprint on this instance" — the
  instance-form ExecRef *is* an Instruction. Nodes retrieve **ambient context by type** from the scope
  registry (Phase 3) during a run, Burst-side via proxies (#14).
- **M8 — Memoized evaluation.** Pull-based `Is Calculated` gating at runtime
  (`EdgeDataHeader.IsCalculated`).
- **M9 — Typed + composable blueprints.** Blueprint I/O as **explicit input/output nodes** = **multiple
  typed entry points** (`Start()` / `Update(float)`, #13), realized via a **universal zero-cost passthrough
  node** (#13.3); blueprint-as-node nesting; capability interfaces
  (`IEntityContainer`/`IStatLogicContainer`-style); and the **`ExecRef` edge value type** (template +
  instance forms, #12) that invokes an input node / blueprint-as-node / callback. Resolves the port model
  (`INode.GetInputs/Outputs`) and `LogicGraph`/`CompiledGraph` stubs.
- **M10 — Codegen.** Generate the managed `INode` partial + `ILogicNode` partial + dispatch switch from a
  node definition (replaces the `AddNode` hand-sketch).
- **M11 — Authoring + transfer.** Config baking into the unmanaged blob, binary server→client transfer,
  end-to-end versioning.

> When a milestone is next, run its planning step to expand it into detailed phases here before coding.

## Per-phase workflow (summary)

Run **`/logicgraph-phase <n>`**. The loop (full detail in the command):
1. **Plan** — Claude writes a short phase plan (files, API, layout, **test list**, non-goals, deviations).
2. **Plan gate** — you ACK or correct (skippable for trivial phases).
3. **Implement** — types → API → logic → tests; conventions; stays compiling.
4. **Self-review** — run tests; codebase checklist (allocator/determinism/lockstep/conventions);
   adversarial review subagent over the diff; fix until clean.
5. **Packet** — Claude leaves changes uncommitted + a Review Packet (what/why per file, test results,
   review-focus hotspots).
6. **Your review** — you read the packet/diff.
7. **Fixes** — Claude applies comments, re-self-reviews the delta.
8. **Close** — Claude updates CLAUDE.md (status map) + this PLAN checkbox; **you commit**.
