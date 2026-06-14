# Handoff — продолжение работы над LogicGraph (для нового чата)

Скопируй текст ниже как первый промпт в новом чате.

---

Продолжаем `/logicgraph-phase` по системе **LogicGraph** в submodule `Sapientia`
(`Assets/Submodules/Sapientia/LogicGraph/`) — нодовый граф для unmanaged/Burst-симуляции.

**Сначала прочитай (источник правды — код, затем эти доки в порядке приоритета):**
1. `LogicGraph/Plan/STATE.md` — снимок как-построено на 2026-06-14 (доменная модель, карта сущностей,
   что готово vs заглушка, **8 открытых развилок**, следующие шаги). **Главный документ.**
2. `LogicGraph/Plan/phase-4/plan.md` — «Пост-ревью №4» (доменная модель 3-региона) и «№5» (снимок).
   Ранние №1–№3 — **исторические** (5-scope/`ExecutionScope`/`MemorySource`), игнорировать как актуальные.
3. `LogicGraph/PLAN.md` — роадмап. `LogicGraph/CLAUDE.md` — навигатор, но §3/§4/§7/§8 **устарели**
   (до-рефакторная 5-scope/edge модель) — сверять со STATE.md.

**Где мы.** Ветка `rnd/nodes_graph`, всё закоммичено и запушено. Готово: **Static-модель** (компиляция
блюпринта в 3-региональный блоб `Static/Cache/Persistence`, Map In/Out через `RegionPtr`, БД блобов) +
**instance identity/lifecycle** (`BlueprintInstanceHeader/Storage/Id` + generation-staleness). Заложен
**каркас runtime/cache/execution** (`DataCache`/`CacheHeader`/`ExecutionGraph`/`NodeMapHeader`) — он
**компилируется, но поведения нет**.

**Что нужно.** Доделать runtime/cache/execution слой. Он заблокирован **8 развилками** (STATE.md §4) —
по каждой у меня есть рекомендация. **Начни с того, что пройдись по 8 развилкам и предложи решения**
(или спроси, где я не согласен), затем реализуй в порядке STATE.md §5:
NodeMapHeader (аллокация+BuildBatches) → ExecutionGraph (шедулинг+Dispose) → CacheHeader (alloc/read/
write/link-resolve) → wiring `runtimeType`/`NodeState` → ContextType → `ExecutionScope` (последним).

**Жёсткие правила.** Чат и комментарии — **по-русски**; идентификаторы (типы/методы/поля/неймспейсы) и
устоявшиеся техтермины (smoke test, harness, batchmode, blueprint, allocator, scope, cache, persistent,
commit, build) — **не переводить**. Tabs (не пробелы), `return` на отдельной строке, **без LINQ**,
`_camelCase` приватные, `ctx`/`Context` ок. Детерминизм под Burst+.NET: без `UnityEngine.Random` и
wall-clock. Self-relative (`RelativePtr`/`BumpArray`/`inOut`-офсет) трогать только через ref/арена-
указатель, не на копии по значению.

**Тесты не прогонять** — локальный Unity batchmode-раннер падает нативным segfault в Google EDM4U
(окружение, не LogicGraph). Верификация — компиляция + инспекция + adversarial-сабагенты.

**Git.** Это submodule — **не коммить без явного запроса** (по умолчанию изменения остаются в рабочем
дереве на ревью). Прошлый коммит/push был по явной просьбе.
