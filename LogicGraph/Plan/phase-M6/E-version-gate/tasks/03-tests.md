# M6-E · Задача 03 — gate в `CompiledBlueprintStorage.Add` + тесты

**Статус: ✅ done** (реализовано + self-review; ревью-гейт пользователя — Step 6)

## Цель

Сторедж рождается с окружением; сверка версии кода — при добавлении блоба: совместимый принимается,
несовместимый отклоняется (до любого инстанса).

## Шаги

1. `CompiledBlueprintStorage`: поле `private CompiledEnvironment _environment;`; `Create(CompiledEnvironment
   environment, int blueprintCapacity = 8)` сохраняет его; `Environment`-аксессор.
2. `Add(arena, offsets, CompiledEnvironment environment)` (+ single-offset перегрузка): **гейт первым делом**,
   до любой мутации — `if (!environment.IsCompatibleWith(_environment)) { arena.Dispose(); throw new
   InvalidOperationException(...); }`. **Реальный throw** (не `E.ASSERT`: load-bearing). Сверка на уровне
   **группы** (окружение vs окружение), не per-blob. Арена освобождается (Add владеет ею) ⇒ нет утечки/полу-добавления.
3. Обновить `CompiledBlueprintStorageTests`: `Add`-хелпер → `storage.Add(arena, offset, CompiledEnvironment.Compile())`;
   5 `Create()` → `Create(CompiledEnvironment.Compile())`.
4. `Tests/VersionGateTests.cs` (`#if UNITY_5_4_OR_NEWER`):
   - **Хеш (чисто):** `Hash_IsDeterministic`/`Hash_OrderSensitive`/`Hash_SetSensitive`/`Hash_EmptyIsStableNonZero`.
   - **Окружение:** `Environment_Compile_CapturesLocal` (реально); `Environment_IsCompatibleWith_SameVersion`/
     `_DifferentVersion` (чисто).
   - **Accept (реально):** `Storage_CompatibleGroup_Added` — сторедж + группа с одним `Compile()` ⇒ `Has(key)`, `Count==1`.
   - **Reject (реально):** `Storage_StaleGroup_Rejected` — группа с `new CompiledEnvironment(Local+1)` ⇒ `Add`
     бросает `InvalidOperationException`; `Count==0`, `!Has`. Ключ снимать **до** `Add` (на reject арена освобождается).
   - **Happy-path (реально):** `CreateInstance_FromCompatibleStorage_Works`.

## Done-criteria

- 13 тестов зелёные (реально/чисто), **без** `Assert.Ignore`. Существующие `CompiledBlueprintStorageTests` — зелёные.
- Reject-тест: группа **не** добавлена (Count 0), арена освобождена (нет утечки/двойного-free).
- Happy-path `CreateInstance` впервые покрыт. `ExecutionScope` M6-E не касается.

## Зависимости

- Задачи 01, 02. `CompiledBlueprintStorage`, `CompiledEnvironment`, `BlueprintCompiler.CompileLayout`,
  `StubBlueprint`/`StubNode`, `ExecutionScope`.

## Notes / findings

- Гейт переехал с `ExecutionScope.CreateInstance` на `CompiledBlueprintStorage.Add` (решение пользователя:
  сторедж рождается с окружением, проверка на ingest). `ExecutionScope` от M6-E больше не зависит.
