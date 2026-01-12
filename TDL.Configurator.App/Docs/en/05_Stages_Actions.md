# Stages and ACTIONs: what you can trigger
<!-- visibility: normal -->

NORMAL format:
```txt
tdl_send.exe NORMAL <ACTION> [SOURCE]
```

> Note: the exact list depends on your build. The set below matches the current documentation build.

## SYSTEM
- `SYSTEM_PING` — connectivity check. **Risk: low**
- `SYSTEM_HEALING` — restore/heal. **Risk: low**
- `SYSTEM_BLESSING` — blessing/buff (if supported). **Risk: low–medium**

## SUMMON
- `SUMMON_ANY`, `SUMMON_ANIMAL`, `SUMMON_HUMANOID`, `SUMMON_UNDEAD`, `SUMMON_SKELETON` — summons. **Risk: medium** (AI load)
- `SUMMON_STRONG` — strong enemy. **Risk: high**
- `SUMMON_DRAGON` — dragon. **Risk: high** (keep it rare)

## TELEPORT
- `TELEPORT_RANDOM_CITY`, `TELEPORT_RANDOM_DANGER` — random teleports. **Risk: high**
- `TELEPORT_WHITERUN`, `TELEPORT_SOLITUDE`, `TELEPORT_WINDHELM`, `TELEPORT_RIFTEN`, `TELEPORT_MARKARTH`,
  `TELEPORT_FALKREATH`, `TELEPORT_DAWNSTAR`, `TELEPORT_HIGH_HROTHGAR` — fixed locations. **Risk: high**

Best practice: do not teleport during dialogs/cutscenes/scripted scenes.

## VIRUS
- `VIRUS_DISEASE` — disease/debuff. **Risk: medium**
- `VIRUS_VAMPIRE` — vampirism. **Risk: high**
- `VIRUS_WEREWOLF` — werewolf. **Risk: high**

## WEATHER
- `WEATHER_CLEAR`, `WEATHER_RAIN`, `WEATHER_SNOW`, `WEATHER_STORM`, `WEATHER_FOG`, `WEATHER_RESET`
  — weather control. **Risk: low**

## INVENTORY
- `INVENTORY_SCATTER` — scatter/drop items around the player. **Risk: medium–high** (performance)
- `INVENTORY_DROP_ALL` — drop everything. **Risk: high** (destructive)

## CHAOS
- `CHAOS_LOW_G` — low gravity. **Risk: medium**
- `CHAOS_BACKFIRE` — backfire effects (pushback/damage/physics). **Risk: medium–high**

## WRATH
- `WRATH_11`, `WRATH_12`, `WRATH_13` — Wrath presets. **Risk: medium**
(variations are INI/build-defined)

## HUNTER
- `HUNTER_START` — timed hunter pursuit. **Risk: medium**

## GIGANT / Characteristics
- `GIGANT_BIG`, `GIGANT_SMALL` — size changes. **Risk: medium**
- `GIGANT_SPEED`, `GIGANT_SLOW` — speed changes. **Risk: medium**
- `GIGANT_RESET` — reset. **Risk: low**

## COMEDY
- `COMEDY_FAKE_HERO` — “fake hero”. **Risk: medium**
- `COMEDY_HORROR` — horror scenario. **Risk: high**
- `COMEDY_ARENA` — enemy waves. **Risk: high**
- `COMEDY_ESCORT` — escort scenario. **Risk: medium**

## Stream content suggestions
- Frequent: PING/Healing/Weather/light Summons
- Rare & expensive: Dragon/Teleport/Arena/Drop All/Vampire

## See also
- **06_INI_Settings.md**
- **07_FORCE_Safety_Cooldowns.md**
