# Logs and diagnostics
<!-- visibility: normal -->

If an action “doesn’t work”, logs usually explain why.

## Where to find logs (typical paths)
### SKSE log
- `Documents\My Games\Skyrim Special Edition\SKSE\skse64.log`

Look for lines like:
- `TDL_StreamPlugin.dll ... loaded correctly`

### Plugin log (if enabled)
- `Documents\My Games\Skyrim Special Edition\SKSE\TDL_StreamPlugin.log`

### Papyrus user log (if your build writes it)
- `Documents\My Games\Skyrim Special Edition\Logs\Script\User\TDL.0.log`

## Quick checklist
1) Game is running and you are in a loaded save.
2) `skse64.log` shows `TDL_StreamPlugin.dll` loaded.
3) `tdl_send.exe` exists at `Data\TDL\Tools\tdl_send.exe`.
4) Run PING:
   ```txt
   tdl_send.exe NORMAL SYSTEM_PING 2
   ```

## Common causes
### Pipe not found / PING fails
- game not running or you are in the main menu;
- plugin did not load (wrong runtime, missing dependency, wrong path).

### Accepted but no visible effect
- scene state (dialog/menu/cutscene);
- cooldown/limit (`TDL_Cooldowns.ini`);
- action missing/disabled in your build;
- INI changed but a restart is required.

## What to include in a bug report
1) Game version (SE/AE) and SKSE version.
2) `skse64.log`
3) `TDL_StreamPlugin.log` (if present)
4) `TDL.0.log` (if present)
5) Exact command and in-game context.

## See also
- **01_QuickStart.md**
- **04_Protocol_Modes.md**
