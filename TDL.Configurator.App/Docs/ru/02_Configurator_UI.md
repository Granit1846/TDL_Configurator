# Configurator: страницы и что они делают
<!-- visibility: normal -->

Configurator редактирует `Data\SKSE\Plugins\TDL_StreamPlugin.ini` и отправляет команды через `tdl_send.exe`.

## QuickAccess
- PING/открытие папок/создание INI-шаблона.
- INI не меняет (кроме создания шаблона).

## Chaos / Inventory / Wrath / Hunter / Gigant (Characteristics) / Comedy
- Эти вкладки меняют секции INI:
  - Chaos → `[Chaos]`
  - Inventory → `[Inventory]`
  - Wrath → `[Wrath]`
  - Hunter → `[Hunter]`
  - Gigant/Characteristics → `[Gigant]`
  - Comedy → `[Comedy]` (если есть в сборке)

Подробные ключи: **06_INI_Settings.md**.

## Documentation
- Просмотр и поиск по Markdown-докам, открытие файла во внешнем редакторе.

## Settings
- Путь к игре (корень Skyrim).
- **Расширенный режим** (если включён) — показывает вкладку **Тест** и полную документацию. Применяется только после **Save**.

## См. также
- **00_Overview_Installation.md** — где лежат файлы.
- **01_QuickStart.md** — быстрая проверка.
