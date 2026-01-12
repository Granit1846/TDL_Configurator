# Quick start
<!-- visibility: normal -->

Goal: quickly confirm the chain **tdl_send.exe → Named Pipe → SKSE → mod** is working.

## 1) Preparation
- The game is running and you **loaded a save** (not the main menu).
- Configurator **Settings** point to the correct Skyrim root folder.

## 2) Connectivity check (PING)
Equivalent command:
```txt
tdl_send.exe NORMAL SYSTEM_PING 2
```

If PING fails, go to **08_Logs_Diagnostics.md**.

## 3) First safe action (HEALING)
```txt
tdl_send.exe NORMAL SYSTEM_HEALING 2
```

## 4) If “PING works but nothing happens”
Common reasons:
- the action is disabled/not present in your build;
- you are in a menu/dialog/cutscene;
- a cooldown/limit blocked it;
- INI was changed but Skyrim needs a restart.

## See also
- **03_Bot_Integration_Guide.md** — bot/stream tool integration.
- **05_Stages_Actions.md** — what each action does.
