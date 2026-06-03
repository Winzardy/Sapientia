# Тесты LogicGraph

PlayMode-юнит-тесты для `Sapientia.LogicGraph`. Это harness верификации, на который опирается каждая
фаза LogicGraph (см. [../PLAN.md](../PLAN.md)). Сборка гонится в **PlayMode** — memory-менеджеры
(`MemoryManagerController`) инициализируются сами через `[RuntimeInitializeOnLoadMethod]` при входе в
play mode, поэтому отдельный bootstrap не нужен. `defineConstraints: ["UNITY_INCLUDE_TESTS"]` не даёт
сборке попасть в плеер-билд.

- **Сборка:** `Sapientia.LogicGraph.Tests` — ссылается на `Sapientia.LogicGraph`, `Sapientia`,
  `Sapientia.MemoryAllocator` и Unity Test Framework.
- **Фреймворк:** Unity Test Framework `1.6.0` (NUnit), уже прописан в `Packages/manifest.json`.

## Запуск в редакторе (по умолчанию)

`Window ▸ General ▸ Test Runner` → вкладка **PlayMode** → **Run All** (или запустить фикстуру
`BumpHeaderSmokeTests` / `BumpAllocatorTests`). Запуск входит в play mode и выходит обратно.

## Запуск в batchmode (без CI / headless)

Редактор не должен держать проект открытым (блокировка проекта эксклюзивна). Из корня репозитория:

```sh
"/Applications/Unity/Hub/Editor/6000.0.60f1/Unity.app/Contents/MacOS/Unity" \
  -runTests -batchmode \
  -projectPath "$(pwd)" \
  -testPlatform PlayMode \
  -testResults "$(pwd)/Logs/logicgraph-tests.xml" \
  -logFile "$(pwd)/Logs/logicgraph-tests.log"
```

Код выхода `0` = все тесты прошли. Результаты пишутся как NUnit XML в `-testResults`; лог прогона —
в `-logFile`. Чтобы запустить только эту сборку, добавьте `-assemblyNames Sapientia.LogicGraph.Tests`.

> Версия редактора зафиксирована в `ProjectSettings/ProjectVersion.txt` (`6000.0.60f1`); если у вас
> установлена другая версия — поправьте путь к редактору выше.

## Тесты сейчас

- `BumpHeaderSmokeTests.Harness_Runs` — Test Runner обнаруживает и выполняет сборку.
- `BumpHeaderSmokeTests.Arena_RoundTripsOneInt` — `RawBumpAllocator`: запись/чтение int через `PtrOffset`.
- `BumpAllocatorTests.Raw_*` — монотонность смещений и serialize/deserialize raw-арены.
- `BumpAllocatorTests.World_*` — allocator-арена (через `MemPtr`): round-trip, dispose и резолв после
  переезда блока + смены версии мира (snapshot).
