# Stream integration: triggering stages from a bot/tool
<!-- visibility: normal -->

TDL is not tied to a specific platform. You can use any tool that can **run an executable with arguments** (exe/script): a stream bot, a control panel, macros, donation service hooks, or a custom bot.

## Base command (NORMAL)
Template:
```txt
tdl_send.exe NORMAL <ACTION> [SOURCE]
```

Example:
```txt
tdl_send.exe NORMAL COMEDY_ARENA 2
```

- `ACTION` — an event identifier (see **05_Stages_Actions.md**).
- `SOURCE` — input priority (use `2` if unsure).

Suggested SOURCE mapping (when multiple channels are used):
- `1` — high priority (donations/bits)
- `2` — default (channel points)
- `3` — low priority (polls/votes)

## Where to find tdl_send.exe
Typical path:
- `Data\TDL\Tools\tdl_send.exe`

Best practice for bots:
- either set **Working Directory** to `...\Data\TDL\Tools\`,
- or call the tool via an **absolute path** (quote paths that contain spaces).

Example:
```txt
"F:\SteamLibrary\steamapps\common\Skyrim Special Edition\Data\TDL\Tools\tdl_send.exe" NORMAL SYSTEM_PING 2
```

## Minimal mapping examples
- Channel Points “Heal” → `SYSTEM_HEALING` (SOURCE 2)
- Donation “Arena” → `COMEDY_ARENA` (SOURCE 1)
- Vote “Teleport” → `TELEPORT_RANDOM_CITY` (SOURCE 3)

## Safety rules (practical)
1) Use **NORMAL** by default.
2) Allow FORCE/series (BURST) only to moderators or rare events.
3) Set cooldowns in the bot (e.g., 60–180 seconds for heavy actions).
4) Avoid teleports during dialogs/cutscenes.
5) Make destructive actions (e.g., `INVENTORY_DROP_ALL`) rare and expensive.

## If an action does not trigger
1) Run `SYSTEM_PING`.
2) Confirm you are in a loaded save.
3) Check **08_Logs_Diagnostics.md**.

## See also
- **04_Protocol_Modes.md** — NORMAL/FORCE1/FORCE formats.
- **07_FORCE_Safety_Cooldowns.md** — BURST limits and cooldowns.
