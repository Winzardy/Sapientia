# Таска 01 — Node scope sizing API

**Статус: ✅ done**

## Цель
Минимальный API объявления размеров: каждая нода декларирует байтовый размер каждой из 5 областей на
этапе компиляции. Расширяет идею `BodySize`/`StateSize`, но не трогает модель портов.

## Файлы
- `Blueprint/DataLayout.cs` (новый)
- `Blueprint/INode.cs` (правка — добавить `DataSizes`)

## Шаги
1. `enum DataLayout : byte { Static, StaticCache, StaticPersistent, InstanceCache, InstancePersistent }`
   + `public const int Count = 5` (на `DataSizes`).
2. `struct DataSizes` (`unsafe`, `fixed int _sizes[5]`): ctor по 5 размерам, индексатор
   `int this[DataLayout]` (get/set), `int Total`.
3. `struct NodeLayoutOffsets` (`unsafe`, `fixed int _offsets[5]`): индексатор `PtrOffset this[DataLayout]`
   (хранит `byteOffset`, на get оборачивает в `new PtrOffset(...)`).
4. В `INode` добавить дефолтный член `DataSizes DataSizes => default;`. Legacy `BodySize`/
   `StateSize`/портовые методы — без изменений.

## Done-criteria
- Компилируется. `default(DataSizes)` даёт все нули и `Total == 0` (zero-size ноды).
- Индексаторы возвращают то, что записали, по каждому `DataLayout`.

## Зависимости
- Нет. Основа для тасок 02/03/04.

## Заметки / находки
- `AlignUp(value, alignPow2)` живёт в `Extensions/Math/BitExt.cs` — использовать в таске 02, не здесь.
