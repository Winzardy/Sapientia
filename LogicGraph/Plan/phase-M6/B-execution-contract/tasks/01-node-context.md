# 01 — NodeContext (seam резолва памяти)

**Статус:** ✅ done

## Цель
Unmanaged Burst-совместимый seam: указатели на блоб/Cache/Persistence + nodeId + аксессоры.

## Шаги
1. `Logic/RuntimeData/Execution/NodeContext.cs`: поля `compiled`/`cache`/`persistence` (`SafePtr<…>`) + `nodeId`.
2. Аксессоры `Compiled()`/`Cache()` (ref в блок), `StaticSlice()`/`InOut()`/`PersistenceSlice()`.
3. Self-relative: `Compiled().Get*` дёргать через ref (не копию) — задокументировать в xml-doc.

## Done-criteria
Компилируется; аксессоры резолвят корректные базы; нет managed-ссылок (Burst-safe).

## Зависимости
M6-A (dispatch-id).

## Notes
—
