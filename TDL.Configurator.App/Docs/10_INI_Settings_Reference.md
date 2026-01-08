# Справочник настроек INI (что меняют вкладки)

Файл:
- `Data\SKSE\Plugins\TDL_StreamPlugin.ini`

Ниже — параметры, которые меняет Configurator, с единицами и диапазонами.
Диапазоны и default взяты из `TDL_AllRanges.txt` (MCM‑скрипты), если указано.

Формат записи в INI:
```ini
[Section]
Key=Value
```

---

## [Chaos]
### BackfireChance
- **Смысл:** шанс backfire (в %)
- default: 20
- range: 0 .. 100

### BackfireDuration
- **Смысл:** длительность backfire (сек)
- default: 60
- range: 1 .. 600

### KnockbackForce
- **Смысл:** сила отталкивания
- default: 25
- range: 0 .. 200

### KnockbackCooldown
- **Смысл:** кулдаун между отталкиваниями (сек)
- default: 0.35
- range: 0.0 .. 2.0

### KnockbackRadius
- **Смысл:** радиус отталкивания (units)
- default: 900
- range: 0 .. 20000

### KnockbackMeleeDelay
- **Смысл:** задержка срабатывания для ближнего боя (сек)
- default: 0.10
- range: 0.0 .. 0.5

### KnockbackBowDelay
- **Смысл:** задержка срабатывания для лука (сек)
- default: 0.10
- range: 0.0 .. 0.5

### ShoutPushForce
- **Смысл:** сила отталкивания игрока при крике (например на backfire)
- default: 20
- range: 0 .. 200

### ShoutPushDelay
- **Смысл:** задержка отталкивания при крике (сек)
- default: 0.05
- range: 0.0 .. 0.5

---

## [Inventory]
### DropBatchSize
- **Смысл:** сколько предметов сбрасывать за «пакет»
- default: 10
- range: 1 .. 100

### DropInterval
- **Смысл:** интервал между пакетами (сек)
- default: 0.20
- range: 0.05 .. 1.0

### DropTimeout
- **Смысл:** таймаут процесса сброса (сек)
- default: 30
- range: 5 .. 120

### ScatterExactCount
- **Смысл:** если >0 — точное количество «rolls» (перебивает min/max)
- default: 0
- range: 0 .. 2000

### ScatterMinCount
- **Смысл:** минимальное количество предметов при scatter
- default: 150
- range: 1 .. 2000

### ScatterMaxCount
- **Смысл:** максимальное количество предметов при scatter
- default: 200
- range: 1 .. 2000

### ScatterRadius
- **Смысл:** радиус рассыпания (units)
- default: 800
- range: 100 .. 5000

### ProtectTokensByName (если используется в вашей сборке)
- **Смысл:** защита предметов «по имени/тегу», чтобы не выпадали (0/1)
- default: зависит от сборки
- диапазон: 0 или 1

### DropShowProgress (если используется в вашей сборке)
- **Смысл:** показывать прогресс/уведомления (0/1)
- default: зависит от сборки
- диапазон: 0 или 1

---

## [Wrath]
### TotalBursts
- **Смысл:** сколько волн/бурстов
- default: 6
- range: 1 .. 50

### Interval
- **Смысл:** интервал между волнами (сек)
- default: 0.4
- range: 0.05 .. 2.0

### Radius
- **Смысл:** радиус эффекта (units)
- default: 300
- range: 100 .. 2000

### ZOffset
- **Смысл:** вертикальное смещение точки эффекта
- default: 50
- range: 0 .. 500

### DamageMin / DamageMax
- **Смысл:** минимальный/максимальный урон
- default: 5 / 15
- range: 1 .. 100

### FireDamageMult / StormMagickaMult / FrostStaminaMult
- **Смысл:** множители типов урона/эффектов
- default: 1.0
- range: 0.0 .. 5.0

### LevelScale
- **Смысл:** скейл от уровня (малые значения)
- default: 0.0
- range: 0.0 .. 0.10

### LevelCap
- **Смысл:** кап уровня для скейла
- default: 3.0
- range: 1.0 .. 5.0

### ShakeChance / ShakeStrength / ShakeDuration
- **Смысл:** камера‑шейк (шанс/сила/длительность)
- default: 0 / 0.0 / 0.0
- range: 0..100, 0.0..1.0, 0.0..1.0

---

## [Hunter]
### Duration
- **Смысл:** длительность сценария Hunter (сек)
- default: 90
- range: 5 .. 600

### ReAggroInterval
- **Смысл:** интервал повторной агрессии/обновления цели (сек)
- default: 4.0
- range: 1.0 .. 10.0

### MaxDistance
- **Смысл:** максимальная дистанция работы Hunter (units)
- default: 5500
- range: 1500 .. 10000

### SpawnOffset
- **Смысл:** смещение спавна относительно игрока (units)
- default: 1200
- range: 300 .. 3000

### CorpseLifetime
- **Смысл:** сколько живёт труп после смерти (сек), 0 — исчезает сразу
- default: 20
- range: 0 .. 300

---

## [Gigant]
### SizeDuration / SpeedDuration
- **Смысл:** длительность эффектов размера/скорости (сек)
- default: 60 / 60
- range: 5 .. 600

### ScaleBig / ScaleSmall
- **Смысл:** множитель размера (большой/малый)
- default: 2.0 / 0.33
- range: 0.1..5.0 / 0.1..1.0

### DamageBig / DamageSmall
- **Смысл:** множитель/добавка урона (зависит от реализации)
- default: 5 / 0.5
- range: 0.0..10.0 / 0.0..1.0

### SpeedFast / SpeedSlow
- **Смысл:** множитель скорости (быстро/медленно)
- default: 3.0 / 0.5
- range: 1.0..10.0 / 0.1..1.0

---

## [Comedy] (если включено в сборке)
Диапазоны из MCM (в этой сборке):

### ArenaPerWave
- default: 3
- range: 1 .. 20

### ArenaSpawnRadius
- default: 800
- range: 200 .. 3000

### ArenaWaveInterval
- default: 3.0
- range: 0.5 .. 10.0

### ArenaWaves
- default: 3
- range: 1 .. 10

### EscortDuration
- default: 120
- range: 30 .. 600

### FakeHeroActionInterval
- default: 3.0
- range: 0.5 .. 10.0

### FakeHeroDamageMult
- default: 1.0
- range: 0.2 .. 5.0

### FakeHeroDuration
- default: 120
- range: 10 .. 600

### FakeHeroPushForce
- default: 5
- range: 0 .. 50

### FakeHeroShoutChance
- default: 30
- range: 0 .. 100

### FakeHeroSpellChance
- default: 30
- range: 0 .. 100

### HorrorDuration
- default: 120
- range: 10 .. 600

### HorrorHealth
- default: 300
- range: 50 .. 5000

### HorrorMaxDistance
- default: 3000
- range: 1000 .. 6000

### HorrorSpawnDistance
- default: 800
- range: 200 .. 3000

### HorrorTeleportDistance
- default: 600
- range: 200 .. 2000

(Если ваша INI‑секция `[Comedy]` использует другие ключи — ориентируйтесь по текущему INI и UI.)
