# FORCE/BURST: safety, limits, cooldowns
<!-- visibility: advanced -->

FORCE modes are great for events, but dangerous when spammed (load, broken scenes, instability).

## Modes
- **FORCE1** — single force:
  ```txt
  tdl_send.exe FORCE1 <ACTION>
  ```
- **FORCE** — series:
  ```txt
  tdl_send.exe FORCE <ACTION> <COUNT> <INTERVAL> [SOURCE]
  ```

## Practical safe limits (recommended)
### Teleports (high risk)
- `COUNT`: 1–3
- `INTERVAL`: 5.0–15.0 sec
Avoid `COUNT > 3` or `INTERVAL < 5.0`.

### Summons (AI load)
- `COUNT`: 3–10
- `INTERVAL`: 1.0–3.0 sec
For `SUMMON_DRAGON`, prefer **COUNT=1**.

### Inventory
- Do **not** use `INVENTORY_DROP_ALL` in BURST.
- Use `INVENTORY_SCATTER` only with `COUNT=1` (or NORMAL).

### Chaos/physics
- Indoors and on stairs: lower force/radius.
- Test single calls (FORCE1) before series.

## Cooldowns/limits (if enabled)
Typical file:
- `Data\TDL\Config\TDL_Cooldowns.ini`

Usually controls:
- max COUNT / min INTERVAL for BURST,
- per-action cooldowns,
- heavy-event throttling.

## If things go wrong
Symptoms: freezes, endless effects, constant teleports, broken AI.
1) Stop sending commands immediately.
2) Wait 10–30 seconds (some effects time out).
3) If stuck: restart the game.
4) Reduce COUNT / increase INTERVAL; enable/strengthen cooldowns.

## See also
- **08_Logs_Diagnostics.md**
- **05_Stages_Actions.md**
