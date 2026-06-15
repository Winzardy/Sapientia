# Под-фаза 4C — CacheHeader (Cache-регион = ячейки DataCache)

**Статус: 🔄 in progress (план, гейт).** Развилка 1 (CacheData ↔ RegionPtr). Обзор — [../README.md](../README.md).

## Цель

Сделать **Cache-регион инстанса массивом ячеек `DataCache<T>`** (мемоизация Is-Calculated + passthrough-link)
и оживить `CacheHeader`: alloc/reset/read/write + резолв link. Это связывает Static.Map (статическая
разводка Cache-портов через `RegionPtr`) с per-instance value-слоем.

## Ключевая модель (форк 1)

- **Cache-Out ноды → ячейка `DataCache<T>`** в Cache-блоке инстанса: `{ CacheState state @0; T value @8 / link @8 }`.
  `state`: `Uninitialized` (не посчитан) / `Value` (мемоизирован) / `Link` (passthrough на другую ячейку).
- **`RegionPtr` остаётся static-проводкой**: для Cache-порта `byteOffset` = **офсет ячейки** в Cache-блоке
  (а не сырого значения). In Cache-порта указывает на ячейку своего источника (как и сейчас — тот же офсет).
- **`CacheHandler<T>{ PtrOffset<DataCache<T>> offset }`** — типизированный вид того же офсета; `CacheHeader`
  резолвит `база Cache-блока + offset → ref DataCache<T>`.
- **Static/Persistence — без ячеек** (сырые значения): Static — RO-константы/бейк; Persistence — durable-стейт,
  не мемоизируется и не сбрасывается. Ячейки — только Cache.

## Решения на гейт (рекомендации)

1. **Размер ячейки в раскладке (ripple в закоммиченный `BlueprintCompiler`).** Нода объявляет
   `DataSizes.Cache` в **байтах ячеек**; компилятор пакует Cache-Out'ы по новому `NodeOutput.CacheCellSize`
   (= `TSize<DataCache<T>>`, virtual; для `T≤8` = 16). `SetupMap` Cache-ветка двигает `localRover`/офсеты на
   `CacheCellSize` (не на сырой `DataSize`); assert «помещается в слайс» — по `CacheCellSize`. Static/Persistence
   не трогаем. **Затрагивает:** `BlueprintCompiler.SetupMap`, `NodeOutput`/`NodeOutput<T>`, stub-размеры +
   `MapTests` (Cache-офсеты теперь кратны 16, не 8). *Рекомендую так.*
2. **`CacheHeader` — владение блоком.** `CacheHeader` живёт в голове instance Cache-блока; `dataCache: RelativePtr`
   указывает на массив ячеек сразу за ним (self-relative, как в блобе). API: `Setup(cellsBlockSize)` (раскладка
   + clear), `Reset` (→ все `Uninitialized`), `Get<T>(CacheHandler<T>) → ref DataCache<T>`, `Read<T>`/`Write<T>`,
   `ResolveLink<T>` (следует passthrough-цепочке до не-Link). **Память блока аллоцирует владелец инстанса (4F);**
   тесты — raw-блок (off-alloc). *Рекомендую так* (согласуется с self-relative моделью блоба и off-allocator runtime).
3. **Семантика чтения / Is-Calculated.** `Read<T>`: резолв link → если `state==Value` вернуть `value`; если
   `Uninitialized` — вернуть «не посчитано» (флаг/`out bool`), **исполнение ноды-производителя — M6** (здесь не
   гейтим запуск, только различаем посчитан/нет). `Write<T>`: `state=Value`, `value=v`. *Рекомендую.*
4. **Split.** Объём большой → предлагаю разбить: **4C-1 — layout** (ячейки в компиляторе + `CacheCellSize` +
   stub/`MapTests`), **4C-2 — поведение** (`CacheHeader` + `NodeIn`/`NodeOut` + тесты). Гейтим дизайн целиком,
   реализуем по очереди. *Рекомендую split.*

## Раскладка / API (эскиз)

```csharp
// CacheHeader живёт в голове instance Cache-блока; ячейки — сразу за ним (self-relative).
public struct CacheHeader
{
    public RelativePtr dataCache; // → массив ячеек DataCache (self-relative)

    public void Setup(SafePtr cacheBlock, int cellsBlockSize); // разложить dataCache + clear
    public void Reset();                                       // все ячейки → Uninitialized (MemClear)

    public ref DataCache<T> Get<T>(CacheHandler<T> handler) where T : unmanaged;   // резолв база+offset
    public bool Read<T>(CacheHandler<T> handler, out T value) where T : unmanaged; // следует link; true если Value
    public void Write<T>(CacheHandler<T> handler, in T value) where T : unmanaged; // state=Value
    public ref DataCache<T> ResolveLink<T>(CacheHandler<T> handler) where T : unmanaged; // до не-Link
}

public abstract class NodeOutput { /* ... */ public virtual int CacheCellSize => DataSize; } // override в NodeOutput<T>
public class NodeOutput<T> { public override int CacheCellSize => TSize<DataCache<T>>.size; }

public struct NodeIn<T>  { public CacheHandler<T> input;  /* Read через CacheHeader */ }
public struct NodeOut<T> { public CacheHandler<T> output; /* Write через CacheHeader */ }
```

## Задачи (предварительно; финализирую после гейта)

| # | Таска | Статус |
|---|---|---|
| 01 | layout: Cache=ячейки в `BlueprintCompiler` + `NodeOutput.CacheCellSize` + stub/`MapTests` (4C-1) | ✅ реализовано (на ревью) |
| 02 | `CacheHeader` поведение: Setup/Reset/Get/Read/Write/ResolveLink (4C-2) | ☐ |
| 03 | wiring `NodeIn`/`NodeOut` + тесты (4C-2) | ☐ |

> **4C-1 done (на ревью):** `NodeOutput.CacheCellSize` (= `TSize<DataCache<T>>`); `BlueprintCompiler.SetupMap`
> пакует Cache-Out по cell-size; stub-размеры Cache-Out'ов и `MapTests`-офсеты приведены к ячейкам (16/32).
> Lockstep не затронут (Cache-регион не в Static-арене). Рантайм-поведение — 4C-2.

## Тест-лист (предварительно)

- **Cache layout**: Cache-Out `long` → слайс 16 байт (ячейка), офсет кратен 16; lockstep цел; `MapTests` обновлены.
- **Write/Read roundtrip**: `Write(h, 42)` → `Read(h, out v)` ⇒ true, v==42.
- **Is-Calculated**: свежая ячейка `Read` ⇒ false (Uninitialized); после `Write` ⇒ true.
- **Reset**: после `Write` + `Reset` ⇒ `Read` false (Uninitialized).
- **Link (passthrough)**: ячейка A→Link→B(Value=7) ⇒ `Read(A)` ⇒ 7; цепочка A→B→C.
- **In читает источник**: In Cache-порта и его источник-Out резолвятся в одну ячейку (через Map-офсет).

## Non-goals (отложено)

- **Гейтинг запуска ноды по Is-Calculated (pull-based)** — **M8**; здесь только различаем посчитан/нет.
- **Исполнение тел нод / запись Out из ноды** — **M6**.
- **Аллокация Cache-блока владельцем инстанса** — **4F (`ExecutionScope`)**; тесты — raw-блок.
- **Persistence-слой / save-load** — Фаза 5.

## Отклонения / риски

- Меняется смысл `DataSizes.Cache` (байты ячеек, не сырого `T`) — ripple в `MapTests`/stub-размеры из 4A
  (Cache-офсеты 16, не 8). Это ожидаемо (форк 1 на гейте 4A был помечен как «потребует подправить 4C»).
