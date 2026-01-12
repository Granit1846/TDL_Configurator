# TDL — Twitch Dragonborn Legacy: обзор и установка
<!-- visibility: normal -->

TDL интегрирует Skyrim SE/AE со стриминговыми платформами и инструментами (Twitch / YouTube / TikTok и др.). Зрители, донаты, points или голосования могут запускать «стадии шоу» в игре через управляемые команды.

## Из чего состоит TDL
1) **Мод (ESP/скрипты/контент)** — запускает стадии (Chaos, Comedy, Hunter, Inventory и т.п.).
2) **SKSE DLL-плагин** (`TDL_StreamPlugin.dll`) — мост между внешними событиями стрима и игрой.
3) **TDL Configurator** — программа для настройки INI, просмотра логов и тестирования команд.

## Требования
- Skyrim **Special Edition** или **Anniversary Edition** (SE/AE).
- SKSE, соответствующий вашему runtime игры.
- Установленный мод TDL (мод + DLL-плагин + утилита `tdl_send.exe`).

## Где должны лежать ключевые файлы
Проверьте наличие (пути от корня игры):
- `Data\SKSE\Plugins\TDL_StreamPlugin.dll`
- `Data\SKSE\Plugins\TDL_StreamPlugin.ini` (создаётся Configurator, если отсутствует)
- `Data\TDL\Tools\tdl_send.exe`
- (опционально) `Data\TDL\Config\TDL_Cooldowns.ini` — лимиты/кулдауны (особенно для FORCE/BURST)

## Установка (коротко)
1) Установите мод TDL как обычный мод (через Vortex/MO2 или вручную).
2) Убедитесь, что DLL лежит именно в `Data\SKSE\Plugins\`.
3) Убедитесь, что `tdl_send.exe` лежит в `Data\TDL\Tools\`.
4) Запустите **TDL Configurator**, в **Settings** укажите путь к корню игры (где `SkyrimSE.exe` / `SkyrimSELauncher.exe`).
5) Если INI отсутствует: **QuickAccess → “Создать INI (шаблон)”**.

## Важное про менеджеры модов (MO2/Vortex)
Частая причина «не работает»: Configurator пишет INI в одну папку, а игра читает Data из другой (виртуальной/развёрнутой).
- Проверяйте, что **путь к игре** в Configurator — это та сборка, из которой реально запускается Skyrim.
- После изменения INI безопасное правило: **перезапустите Skyrim**, если эффект не меняется сразу.

## Первичная проверка (2 минуты)
1) Запустите Skyrim и **загрузите сохранение** (персонаж уже в мире).
2) В Configurator откройте **QuickAccess** и нажмите **PING**.
3) Затем проверьте видимый эффект: **SYSTEM_HEALING**.

Дальше: см. **01_QuickStart.md**.
