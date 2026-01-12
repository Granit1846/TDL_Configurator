# INI: параметры и безопасная настройка
<!-- visibility: advanced -->

Configurator редактирует:
- `Data\SKSE\Plugins\TDL_StreamPlugin.ini`

Формат:
```ini
[Section]
Key=Value
```

## Быстрые рекомендации (без “воды”)
- Если ловите фризы/нагрузку: уменьшайте **количество** (Inventory scatter), увеличивайте **интервалы**, снижайте **радиусы**.
- Для физики (Chaos): начинайте с малой силы/радиуса, особенно в интерьерах.
- После изменения INI безопасное правило: **перезапустите Skyrim**, если эффект не меняется сразу.

---

## [Chaos]
- **BackfireChance** — шанс backfire (%) (default 20; 0..100)
- **BackfireDuration** — длительность (сек) (default 60; 1..600)
- **KnockbackForce** — сила отталкивания (default 25; 0..200)
- **KnockbackCooldown** — кулдаун (сек) (default 0.35; 0.0..2.0)
- **KnockbackRadius** — радиус (units) (default 900; 0..20000)
- **KnockbackMeleeDelay** — задержка ближнего боя (сек) (default 0.10; 0.0..0.5)
- **KnockbackBowDelay** — задержка лука (сек) (default 0.10; 0.0..0.5)
- **ShoutPushForce** — сила отталкивания игрока при крике (default 20; 0..200)
- **ShoutPushDelay** — задержка при крике (сек) (default 0.05; 0.0..0.5)

---

## [Inventory]
- **DropBatchSize** — предметов за “пакет” (default 10; 1..100)
- **DropInterval** — интервал между пакетами (сек) (default 0.20; 0.05..1.0)
- **DropTimeout** — таймаут процесса (сек) (default 30; 5..120)
- **ScatterExactCount** — точное число “rolls” (>0 перебивает min/max) (default 0; 0..2000)
- **ScatterMinCount** — минимум предметов (default 150; 1..2000)
- **ScatterMaxCount** — максимум предметов (default 200; 1..2000)
- **ScatterRadius** — радиус (units) (default 800; 100..5000)
- **ProtectTokensByName** — защита токенов по имени/тегу (0/1, если используется)
- **DropShowProgress** — показывать прогресс (0/1, если используется)

---

## [Wrath]
- **TotalBursts** — число волн (default 6; 1..50)
- **Interval** — интервал (сек) (default 0.4; 0.05..2.0)
- **Radius** — радиус (units) (default 300; 100..2000)
- **ZOffset** — вертикальное смещение (default 50; 0..500)
- **DamageMin / DamageMax** — урон (default 5/15; 1..100)
- **FireDamageMult / StormMagickaMult / FrostStaminaMult** — множители (default 1.0; 0.0..5.0)
- **LevelScale** — скейл от уровня (default 0.0; 0.0..0.10)
- **LevelCap** — кап для скейла (default 3.0; 1.0..5.0)
- **ShakeChance / ShakeStrength / ShakeDuration** — шейк камеры (0..100 / 0..1 / 0..1)

---

## [Hunter]
- **Duration** — длительность (сек) (default 90; 5..600)
- **ReAggroInterval** — интервал ре-агро (сек) (default 4.0; 1.0..10.0)
- **MaxDistance** — максимальная дистанция (units) (default 5500; 1500..10000)
- **SpawnOffset** — смещение спавна (units) (default 1200; 300..3000)
- **CorpseLifetime** — жизнь трупа (сек), 0 — исчезает сразу (default 20; 0..300)

---

## [Gigant]
- **SizeDuration / SpeedDuration** — длительность (сек) (default 60/60; 5..600)
- **ScaleBig / ScaleSmall** — множители размера (default 2.0/0.33; 0.1..5.0 / 0.1..1.0)
- **DamageBig / DamageSmall** — множители/добавки урона (зависит от реализации) (default 5/0.5)
- **SpeedFast / SpeedSlow** — множители скорости (default 3.0/0.5; 1.0..10.0 / 0.1..1.0)

---

## [Comedy] (если включено в сборке)
- **ArenaPerWave** (default 3; 1..20)
- **ArenaSpawnRadius** (default 800; 200..3000)
- **ArenaWaveInterval** (default 3.0; 0.5..10.0)
- **ArenaWaves** (default 3; 1..10)
- **EscortDuration** (default 120; 30..600)
- **FakeHeroActionInterval** (default 3.0; 0.5..10.0)
- **FakeHeroDamageMult** (default 1.0; 0.2..5.0)
- **FakeHeroDuration** (default 120; 10..600)
- **FakeHeroPushForce** (default 5; 0..50)
- **FakeHeroShoutChance** (default 30; 0..100)
- **FakeHeroSpellChance** (default 30; 0..100)
- **HorrorDuration** (default 120; 10..600)
- **HorrorHealth** (default 300; 50..5000)
- **HorrorMaxDistance** (default 3000; 1000..6000)
- **HorrorSpawnDistance** (default 800; 200..3000)
- **HorrorTeleportDistance** (default 600; 200..2000)

---

## См. также
- **07_FORCE_Safety_Cooldowns.md** — если используете FORCE/BURST.
- **08_Logs_Diagnostics.md** — как проверить, что INI действительно читается вашей Data.
