# Тесты LogicGraph

EditMode-юнит-тесты для `Sapientia.LogicGraph`. Это harness верификации, на который опирается каждая
фаза LogicGraph (см. [../PLAN.md](../PLAN.md)). Сборка **только для редактора**
(`includePlatforms: ["Editor"]`, `defineConstraints: ["UNITY_INCLUDE_TESTS"]`), поэтому она никогда
не попадает в плеер-билд.

- **Сборка:** `Sapientia.LogicGraph.Tests` — ссылается на `Sapientia.LogicGraph`, `Sapientia`,
  `Sapientia.MemoryAllocator` и Unity Test Framework.
- **Фреймворк:** Unity Test Framework `1.6.0` (NUnit), уже прописан в `Packages/manifest.json`.

## Запуск в редакторе (по умолчанию)

`Window ▸ General ▸ Test Runner` → вкладка **EditMode** → **Run All** (или запустить фикстуру
`ArenaAllocatorSmokeTests`). Это обычный локальный цикл.

## Запуск в batchmode (без CI / headless)

Редактор не должен держать проект открытым (блокировка проекта эксклюзивна). Из корня репозитория:

```sh
"/Applications/Unity/Hub/Editor/6000.0.60f1/Unity.app/Contents/MacOS/Unity" \
  -runTests -batchmode \
  -projectPath "$(pwd)" \
  -testPlatform EditMode \
  -testResults "$(pwd)/Logs/logicgraph-tests.xml" \
  -logFile "$(pwd)/Logs/logicgraph-tests.log"
```

Код выхода `0` = все тесты прошли. Результаты пишутся как NUnit XML в `-testResults`; лог прогона —
в `-logFile`. Чтобы запустить только эту сборку, добавьте `-assemblyNames Sapientia.LogicGraph.Tests`.

> Версия редактора зафиксирована в `ProjectSettings/ProjectVersion.txt` (`6000.0.60f1`); если у вас
> установлена другая версия — поправьте путь к редактору выше.

## Тесты сейчас (Фаза 0)

- `Harness_Runs` — доказывает, что Test Runner обнаруживает и выполняет эту сборку.
- `Arena_RoundTripsOneInt` — выделяет `ArenaAllocator`, пишет int через `PtrOffset`, читает его обратно
  с тем же значением и корректно освобождает память.
