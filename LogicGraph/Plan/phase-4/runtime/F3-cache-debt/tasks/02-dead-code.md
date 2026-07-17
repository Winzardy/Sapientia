# 02 — BumpHeader/RawBumpAllocator: `Size` + `Reset`

**Статус: ✅ done**

## История

Первая итерация удаляла `BumpHeader.Reset()`/`Size` + `RawBumpAllocator.Size` как dead-code (подтверждено grep'ом:
0 потребителей). По **re-gate пользователя** удаление отменено — члены **возвращены** (на вопрос «оставить удалёнными
vs вернуть» выбрано «вернуть»: возможны будущие потребители — переиспользование/перезаливка арены).

## Итог

- `Memory/BumpAllocator/BumpHeader.cs` — `Size` (стр. 40) и `Reset()` восстановлены.
- `Memory/BumpAllocator/RawBumpAllocator.cs` — `Size` восстановлен.
- Чистый diff по этим файлам (vs HEAD) — пустой.

## Done-criteria

- Три члена на месте; submodule компилируется.
