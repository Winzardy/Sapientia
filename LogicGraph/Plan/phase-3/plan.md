# Фаза 3 — Compiled-blueprint storage (БД)

**Статус фазы: ✅ done (2026-06-08) — реализация `CompiledBlueprintStorage`, тесты 25/25 зелёные, approval получен, закоммичено (Step 8).**

> Источник цели — [../../PLAN.md → Phase 3 (реордер)](../../PLAN.md). Дизайн-инварианты —
> [../../CLAUDE.md §4 → Blueprint manager](../../CLAUDE.md). Этот документ — живое состояние фазы.

## Цель

**Хранилище** скомпилированных блюпринтов (static-данные), отдельное от scope. Блюпринты компилируются
**все сразу** в хранилище; плюс **рантайм-добавление** новых id/версий и **retain-old** для живых ссылок.
Scope (Фаза 4) просто знает о хранилище и читает из него по `Id<CompiledBlueprint>`.

## Утверждённый дизайн (финальный)

- **Сторедж ничего не знает о `Blueprint` (authoring) и о компиляции.** На вход — уже готовый
  `CompiledBlueprint` вместе с ареной (`Add(arena, offset)`); компиляция — у вызывающего
  (`CompiledBlueprint.CompileLayout`). `id`+`version` сторедж читает из самого `CompiledBlueprint`.
  **Add передаёт владение ареной стореджу.**
- **Адресация по `(Id<Blueprint>, version)`** — плоского `Id<CompiledBlueprint>` нет (`compiledId` убран).
- **Ничего не удаляем по одной.** Версии только добавляются и сосуществуют; всё освобождается единым
  `Dispose` на выходе. ⇒ нет `refCount`/`Retain`/`Release`/`Remove`/retire/free-list.
- **Раскладка `jump-by-id`:** `_blueprints` = `UnsafeList<RootSlot>`, индекс = `(int)Id<Blueprint>`.
  `RootSlot { Slot slot; bool hasSlot; UnsafeList<Slot> next; }` — `slot` = текущая версия инлайн (1 jump),
  `next` = список старых. Lookup: прыжок по id → совпала текущая? иначе walk по `next`. Без Dictionary.
- **Арены — отдельным списком `_arenas` (по батчам)**, слот ссылается по индексу
  (`Slot { int version; int arenaId; PtrOffset<CompiledBlueprint> offset; }`), арену не держит.
  `Count = _arenas.count`.
- **Off-allocator** (`UnsafeList` + `RawBumpAllocator`): static-блоб = сериализуемый бинарь, в снапшот мира
  не идёт (Static вне стейта).
- **Dedup** по `(id,version)` (повтор → входная арена освобождается); **supersede**: новая версия →
  текущая, прежняя в `next` (и живёт).
- **Эволюция `BlueprintCompiler`** → `CompiledBlueprintStorage` (старый fixed-arena `CompileAll` удалён;
  `LogicGraph.cs` stub перенаправлен). *Имя на подтверждение.*

## Публичный API

```csharp
public struct CompiledBlueprintStorage : IDisposable
{
    public bool IsCreated { get; }
    public int Count { get; }                                             // = число compiled (ничего не удаляется)

    public static CompiledBlueprintStorage Create(int blueprintCapacity = 8);

    public void Add(RawBumpAllocator arena, PtrOffset<CompiledBlueprint> offset); // готовый compiled + арена; владение → стореджу
    public bool Has(Id<Blueprint> id, int version);                       // существует?
    public ref CompiledBlueprint Get(Id<Blueprint> id, int version);      // jump-by-id + walk; DEBUG-assert на missing
    public void Dispose();                                                // единый teardown всех арен
}
```

## Задачи

| # | Таска | Что | Done |
|---|---|---|---|
| 01 | [storage + compile-all](tasks/01-db-storage.md) | `CompiledBlueprintStorage` (jump-by-id `RootSlot`+список версий; арены в `_arenas`, слот по `arenaId`); `CompileAll`/`AddOrCompile` (dedup); `Get`/`Has`/`Count`/`Dispose` | ✅ done |
| 02 | [runtime add](tasks/02-lazy-dedup.md) | рантайм `AddOrCompile` новых id/версий (рост списка); сосуществование версий | ✅ done |
| 03 | [~~retain/retire~~ — убрано](tasks/03-versioning.md) | refCount/retire/Remove **отменены** (ничего не удаляем кроме Dispose) | ✅ n/a |

## Тест-лист (model-proving, stub-ноды)

- **Add/Count/Get/Has**: компилируем снаружи + `Add` → Count=N; `Has(id,version)`; `Get(id,version)`
  возвращает нужный; несуществующие id/version → `Has`=false.
- **Dedup**: `AddOrCompile` той же `(id,version)` → Count не растёт.
- **Runtime add**: новый id (рост списка) добавляется и резолвится.
- **Versions coexist**: добавили v1/v2/v3 — все три живут и резолвятся (Count=3), `Get` по версии корректен.
- **Dispose**: единый teardown освобождает все арены (вкл. старые версии) — без падений/двойного free.

## Non-goals (отложено)

- `ExecutionScope` (массив ленивых per-blueprint менеджеров на хранилище) — **Фаза 4**.
- Save/load `*persistent` + снапшот compiled-блобов (binary transfer) — **Фаза 5 / M11**.
- Исполнение/dispatch/ambient context — **M6/M7**.

## Scope → стейт (уточнение пользователя 2026-06-08)

| Область | В снапшоте? | Память |
|---|---|---|
| Static | нет | off-allocator (это хранилище Фазы 3 ✅) |
| StaticCache | нет | off-allocator (Фаза 4) |
| InstanceCache | нет | off-allocator (**сейчас в стейте — см. follow-up**) |
| StaticPersistent | да | allocator/worldState (Фаза 4) |
| InstancePersistent | да | allocator/worldState (Фаза 2 ✅) |

- **Фаза 3 уже соответствует**: Static off-allocator (per-compiled `RawBumpAllocator`).
- **Фаза 4**: `StaticCache` off-allocator, `StaticPersistent` в worldState.
- **Follow-up (Фаза 2, закоммичено):** `BlueprintInstance.instanceCache` сейчас в `worldState` (в стейте) —
  должен стать **off-allocator**. Поправить отдельным шагом (вероятно при рерайте инстансов в Фазе 4).
- Инвариант сложить в `CLAUDE.md §4` на Step 8.

## Отклонения / находки

- Реордер фаз 3↔4 (scope после хранилища) — по ревью.
- **Eager compile-all** (не lazy) — по уточнению; рантайм-добавление через `AddOrCompile`.
- **Без Dictionary** — intrusive chain (`byBlueprint` head + `nextVersion` в слотах) — по ревью.
- Staleness **по `(id,version)`**, без отдельного generation (инстанс уже несёт `blueprintId`+`version`).
- Переиспользуется `CompiledBlueprint.CompileLayout` (Фаза 2) как per-compiled билдер арены.
