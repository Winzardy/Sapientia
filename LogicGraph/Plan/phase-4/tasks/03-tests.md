# Таска 03 — Тесты ExecutionScope

**Статус: ✅ done**

## Цель
PlayMode-тесты (`Sapientia.LogicGraph.Tests`), доказывающие модель scope на stub-нодах.

## Тесты
- `Scope_StaticSite_SharedAcrossInstances` — 2 инстанса `(1,1)` → один site (адреса cache/persistent совпадают).
- `Scope_MultiScope_Isolation` — `bp2` в двух scope → разные `static *`; dispose одного не трогает другой.
- `Scope_DistinctVersions_DistinctSites` — `(1,1)`/`(1,2)` → 2 site.
- `Scope_ResetStaticCache_ClearsCacheKeepsPersistent` — запись → reset → cache=0, persistent цел.
- `Scope_DisposeInstance_FreesInstanceNotSite` — 2 инстанса; dispose одного → site/второй живы, count−1.
- `Scope_Dispose_FreesAll` — инстансы по нескольким site → `Dispose` без падений; `IsCreated`=false.
- `Scope_ZeroSizeStaticScopes_Clean` — нулевые static cache/persistent → site без аллокаций; reset/dispose чисты.

## Done-criteria
- Все зелёные; нет утечек/двойного free; storage компилируется снаружи (как в Phase-3 тестах).

## Зависимости
- Таски 01–02; `StubNode`/`StubBlueprint` (есть); `CompiledBlueprint.CompileLayout` + storage.
</content>
