# Protocol and modes (NORMAL / FORCE1 / FORCE)
<!-- visibility: advanced -->

## Chain
Configurator/bot runs `tdl_send.exe`, which sends a command via Windows Named Pipe:
- `\\.\pipe\TDL_Stream`

Then the SKSE plugin processes the request and triggers the in-game action.

## NORMAL (main mode)
```txt
tdl_send.exe NORMAL <ACTION> [SOURCE]
```
- Recommended for most stream use.
- Safer and respects build-specific conditions/limits.

## FORCE1 (single force)
```txt
tdl_send.exe FORCE1 <ACTION>
```
- Useful for manual verification or special events.
- May bypass some NORMAL restrictions (build-dependent).

## FORCE (BURST) — series
```txt
tdl_send.exe FORCE <ACTION> <COUNT> <INTERVAL> [SOURCE]
```
- `COUNT` — number of repeats.
- `INTERVAL` — delay between repeats (seconds).

Use with care: it increases load (Papyrus/AI/FX) and can break scenes if spammed.

## SOURCE (input priority)
SOURCE helps when multiple channels are active (donations/points/polls).
Suggested values:
- `1` — high (donations/bits)
- `2` — default (points)
- `3` — low (votes)

If unsure, use `2`.

## What counts as “success”
- The command reaches the pipe and is accepted by the plugin.
- You may still see no visible effect due to scene state, cooldowns/limits, or missing actions.
Diagnostics: **08_Logs_Diagnostics.md**.

## See also
- **07_FORCE_Safety_Cooldowns.md**
