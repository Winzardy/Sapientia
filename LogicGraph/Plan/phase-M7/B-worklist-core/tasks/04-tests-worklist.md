# T04 — NodeDispatchTests под work-list + мемоизация

**Статус:** ✅ done

## Шаги
1. Переписать `Run_*` под `Inject(span)` (вместо entry-node/Drain).
2. + тест `Run_Memoizes_SharedAncestorRunsOnce`: общий предок двух потребителей исполняется один раз
   (счётчик через Persistence-инкремент).

## Done-criteria
- Все ассерты сохранены; мемоизация-тест зелёный (forceManaged).
