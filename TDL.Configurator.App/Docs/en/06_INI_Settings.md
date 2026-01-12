# INI: settings and safe tuning
<!-- visibility: advanced -->

Configurator edits:
- `Data\SKSE\Plugins\TDL_StreamPlugin.ini`

Format:
```ini
[Section]
Key=Value
```

## Quick guidance
- For performance issues: reduce **counts**, increase **intervals**, lower **radii** (especially Inventory scatter).
- For Chaos/physics: start with low force/radius, especially indoors.
- After INI edits: **restart Skyrim** if you don’t see changes immediately.

---

## [Chaos]
- **BackfireChance** — backfire chance (%) (default 20; 0..100)
- **BackfireDuration** — duration (sec) (default 60; 1..600)
- **KnockbackForce** — push force (default 25; 0..200)
- **KnockbackCooldown** — cooldown (sec) (default 0.35; 0.0..2.0)
- **KnockbackRadius** — radius (units) (default 900; 0..20000)
- **KnockbackMeleeDelay** — melee delay (sec) (default 0.10; 0.0..0.5)
- **KnockbackBowDelay** — bow delay (sec) (default 0.10; 0.0..0.5)
- **ShoutPushForce** — player push on shout (default 20; 0..200)
- **ShoutPushDelay** — shout delay (sec) (default 0.05; 0.0..0.5)

---

## [Inventory]
- **DropBatchSize** — items per batch (default 10; 1..100)
- **DropInterval** — delay between batches (sec) (default 0.20; 0.05..1.0)
- **DropTimeout** — process timeout (sec) (default 30; 5..120)
- **ScatterExactCount** — exact rolls (>0 overrides min/max) (default 0; 0..2000)
- **ScatterMinCount** — min items (default 150; 1..2000)
- **ScatterMaxCount** — max items (default 200; 1..2000)
- **ScatterRadius** — radius (units) (default 800; 100..5000)
- **ProtectTokensByName** — token protection by name/tag (0/1, if used)
- **DropShowProgress** — show progress (0/1, if used)

---

## [Wrath]
- **TotalBursts** — waves (default 6; 1..50)
- **Interval** — wave interval (sec) (default 0.4; 0.05..2.0)
- **Radius** — radius (units) (default 300; 100..2000)
- **ZOffset** — vertical offset (default 50; 0..500)
- **DamageMin / DamageMax** — damage (default 5/15; 1..100)
- **FireDamageMult / StormMagickaMult / FrostStaminaMult** — multipliers (default 1.0; 0.0..5.0)
- **LevelScale** — level scaling (default 0.0; 0.0..0.10)
- **LevelCap** — scaling cap (default 3.0; 1.0..5.0)
- **ShakeChance / ShakeStrength / ShakeDuration** — camera shake

---

## [Hunter]
- **Duration** — duration (sec) (default 90; 5..600)
- **ReAggroInterval** — re-aggro interval (sec) (default 4.0; 1.0..10.0)
- **MaxDistance** — max distance (units) (default 5500; 1500..10000)
- **SpawnOffset** — spawn offset (units) (default 1200; 300..3000)
- **CorpseLifetime** — corpse lifetime (sec), 0 = immediate (default 20; 0..300)

---

## [Gigant]
- **SizeDuration / SpeedDuration** — durations (sec) (default 60/60; 5..600)
- **ScaleBig / ScaleSmall** — size multipliers (default 2.0/0.33; 0.1..5.0 / 0.1..1.0)
- **DamageBig / DamageSmall** — damage multipliers/additions (build-dependent)
- **SpeedFast / SpeedSlow** — speed multipliers (default 3.0/0.5; 1.0..10.0 / 0.1..1.0)

---

## [Comedy] (if enabled)
- **ArenaPerWave** (default 3; 1..20)
- **ArenaSpawnRadius** (default 800; 200..3000)
- **ArenaWaveInterval** (default 3.0; 0.5..10.0)
- **ArenaWaves** (default 3; 1..10)
- **EscortDuration** (default 120; 30..600)
- **FakeHeroActionInterval** (default 3.0; 0.5..10.0)
- **FakeHeroDamageMult** (default 1.0; 0.2 .. 5.0)
- **FakeHeroDuration** (default 120; 10..600)
- **FakeHeroPushForce** (default 5; 0..50)
- **FakeHeroShoutChance** (default 30; 0..100)
- **FakeHeroSpellChance** (default 30; 0..100)
- **HorrorDuration** (default 120; 10..600)
- **HorrorHealth** (default 300; 50..5000)
- **HorrorMaxDistance** (default 3000; 1000..6000)
- **HorrorSpawnDistance** (default 800; 200..3000)
- **HorrorTeleportDistance** (default 600; 200..2000)

## See also
- **07_FORCE_Safety_Cooldowns.md**
- **08_Logs_Diagnostics.md**
