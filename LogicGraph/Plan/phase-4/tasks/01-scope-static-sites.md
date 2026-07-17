# Таска 01 — ExecutionScope + ленивые static-sites

**Статус: ✅ done**

## Цель
Каркас `ExecutionScope` и владение per-`(id,version)` областями `static cache` (off-allocator) и
`static persistent` (worldState), создаваемыми лениво.

## Шаги
- Типы `StaticSite` (cache `SafePtr`+size, persistent `CachedPtr`), поля scope (`_memoryId`, `_sites`).
- `Create(memoryId, instanceCapacity)`, `IsCreated`, `InstanceCount`.
- `EnsureSite(ws, ref storage, id, version)`: ищет site; если нет — читает размеры из
  `storage.Get(id,version).GetBlockSize(StaticCache/StaticPersistent)`; аллоцирует cache off-alloc
  (`_memoryId.GetManager().MemAlloc(size, ClearMemory)`) и persistent в world (`ws.MemAlloc` + `MemClear`,
  как `BlueprintInstance.AllocBlock`); zero-size → невалидный/пустой блок без аллокации.
- `HasSite`, `GetStaticCache`, `GetStaticPersistent(ws)`, `ResetStaticCache` (MemClear по size всех site).

## Done-criteria
- Ленивое создание site; повтор `(id,version)` не плодит site; zero-size чисто; reset обнуляет только cache.
- Компилируется; внутри lockstep alloc⟷free.

## Зависимости
- Фаза 2 (`CompiledBlueprint.GetBlockSize`/layout), Фаза 3 (`CompiledBlueprintStorage.Get`).

## Заметки
- Off-allocator база неподвижна ⇒ кешировать `SafePtr` static cache безопасно (нет resize/version-bump).
</content>
