# Логи и диагностика
<!-- visibility: normal -->

Если команда “не работает”, логи обычно объясняют причину быстрее, чем догадки.

## Где искать логи (типовые пути)
### SKSE лог
- `Documents\My Games\Skyrim Special Edition\SKSE\skse64.log`

Ищите строки вида:
- `TDL_StreamPlugin.dll ... loaded correctly`

### Лог плагина (если включён)
- `Documents\My Games\Skyrim Special Edition\SKSE\TDL_StreamPlugin.log`

### Лог Papyrus (если ваша сборка пишет туда)
- `Documents\My Games\Skyrim Special Edition\Logs\Script\User\TDL.0.log`

## Быстрый чеклист
1) Игра запущена, вы в загруженном сейве.
2) В `skse64.log` видно, что `TDL_StreamPlugin.dll` загрузился.
3) `tdl_send.exe` существует по пути `Data\TDL\Tools\tdl_send.exe`.
4) PING:
   ```txt
   tdl_send.exe NORMAL SYSTEM_PING 2
   ```

## Типовые причины
### Pipe не найден / PING не проходит
- игра не запущена или вы в главном меню;
- плагин не загрузился (не тот runtime, нет зависимости, файл не там).

### Команда принята, но эффекта нет
- условия сцены (диалог/меню/катсцена);
- кулдаун/лимит (в т.ч. `TDL_Cooldowns.ini`);
- действие отсутствует/отключено в сборке;
- INI изменён, но требуется перезапуск Skyrim.

## Что приложить к баг-репорту
1) Версия игры (SE/AE) и версия SKSE.
2) `skse64.log`
3) `TDL_StreamPlugin.log` (если есть)
4) `TDL.0.log` (если есть)
5) Точная команда `tdl_send.exe ...` и что происходило в игре.

## См. также
- **01_QuickStart.md**
- **04_Protocol_Modes.md**
