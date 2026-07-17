# Таска 03 — BlueprintInstance restructure

**Статус: ✅ done**

## Цель
Переструктурировать данные инстанса в два явных блока — `instance cache` + `instance persistent` —
вместо текущих `edgesData`/`nodesState`. Размер по `blockSizes`; reset обнуляет только cache.

## Файлы
- `Logic/BlueprintInstance.cs` (правка)

## Шаги
1. Заменить поля `nodesState`/`edgesData` на `CachedPtr instancePersistent; CachedPtr instanceCache;`
   (оставить `version`/`blueprintId`/`instanceId`).
2. `Create(WorldState, in CompiledBlueprint, Id<BlueprintInstance>)`: завести инстанс через
   `CachedPtr<BlueprintInstance>.Create`; для каждого instance-блока — если
   `blockSizes[InstanceCache/InstancePersistent] > 0`: `worldState.MemAlloc(size, out ptr)` + `MemClear`
   (основной Allocator не обнуляет); иначе оставить `CachedPtr` невалидным (zero-size).
3. `ResetCache(WorldState, in CompiledBlueprint)`: если cache-блок валиден — `MemClear` на
   `blockSizes[InstanceCache]`.
4. `Dispose(WorldState)`: освободить оба блока (`Dispose` на валидных `CachedPtr`).
5. Удалить legacy `BeginRun`/`EndRun`/`ResetEdges` (вызывающих нет во всём сабмодуле — проверено grep'ом).

## Done-criteria
- Компилируется. Аксессоры портов (`InputData`/`StateData`, параметризованы `SafePtr state`) не
  ломаются (они не читают поля инстанса).
- Тесты: оба блока аллоцируются/обнуляются; reset чистит только cache; persistent цел; zero-size чисто;
  Dispose инвалидирует.

## Зависимости
- Таски 01, 02 (`blockSizes`).

## Заметки / находки
- Внешних вызовов `BlueprintInstance.*` нет — рестракт безопасен.
- Шаблоны дефолтов (копирование `nodesState`) убираем: дефолты портов — это M9, не Фаза 2. Блоки
  стартуют обнулёнными.
