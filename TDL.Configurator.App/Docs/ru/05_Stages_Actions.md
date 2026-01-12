# Стадии и ACTION: что можно запускать
<!-- visibility: normal -->

Формат запуска (NORMAL):
```txt
tdl_send.exe NORMAL <ACTION> [SOURCE]
```

> Примечание: перечень зависит от версии мода. Ниже — набор, который используется в текущей сборке документации.

## SYSTEM
- `SYSTEM_PING` — проверка связи. **Риск: низкий**
- `SYSTEM_HEALING` — лечение/восстановление. **Риск: низкий**
- `SYSTEM_BLESSING` — благословение/бафф (если поддерживается). **Риск: низкий–средний**

## SUMMON (призывы)
- `SUMMON_ANY`, `SUMMON_ANIMAL`, `SUMMON_HUMANOID`, `SUMMON_UNDEAD`, `SUMMON_SKELETON` — призывы. **Риск: средний** (нагрузка на AI)
- `SUMMON_STRONG` — сильный противник. **Риск: высокий**
- `SUMMON_DRAGON` — дракон. **Риск: высокий** (делайте редким)

## TELEPORT (телепорты)
- `TELEPORT_RANDOM_CITY`, `TELEPORT_RANDOM_DANGER` — случайные телепорты. **Риск: высокий**
- `TELEPORT_WHITERUN`, `TELEPORT_SOLITUDE`, `TELEPORT_WINDHELM`, `TELEPORT_RIFTEN`, `TELEPORT_MARKARTH`,
  `TELEPORT_FALKREATH`, `TELEPORT_DAWNSTAR`, `TELEPORT_HIGH_HROTHGAR` — фиксированные точки. **Риск: высокий**

**Практика:** не запускать в диалоге/катсценах/скриптовых сценах.

## VIRUS (болезни/проклятия)
- `VIRUS_DISEASE` — болезнь/дебафф. **Риск: средний**
- `VIRUS_VAMPIRE` — вампиризм. **Риск: высокий**
- `VIRUS_WEREWOLF` — оборотень. **Риск: высокий**

## WEATHER (погода)
- `WEATHER_CLEAR`, `WEATHER_RAIN`, `WEATHER_SNOW`, `WEATHER_STORM`, `WEATHER_FOG`, `WEATHER_RESET`
  — смена погоды. **Риск: низкий**

## INVENTORY (инвентарь)
- `INVENTORY_SCATTER` — разбрасывание предметов. **Риск: средний–высокий** (нагрузка)
- `INVENTORY_DROP_ALL` — сбросить всё. **Риск: высокий** (разрушительно)

## CHAOS (хаос/физика)
- `CHAOS_LOW_G` — низкая гравитация. **Риск: средний**
- `CHAOS_BACKFIRE` — “обратка” (отбрасывания/урон/эффекты). **Риск: средний–высокий**

## WRATH (пресеты серий)
- `WRATH_11`, `WRATH_12`, `WRATH_13` — пресеты Wrath. **Риск: средний**
(различия задаются вашей сборкой через INI)

## HUNTER
- `HUNTER_START` — преследующий охотник на время. **Риск: средний**

## GIGANT / Characteristics
- `GIGANT_BIG`, `GIGANT_SMALL` — размер. **Риск: средний**
- `GIGANT_SPEED`, `GIGANT_SLOW` — скорость. **Риск: средний**
- `GIGANT_RESET` — сброс. **Риск: низкий**

## COMEDY (сценарии)
- `COMEDY_FAKE_HERO` — “ложный герой”. **Риск: средний**
- `COMEDY_HORROR` — хоррор-сценарий. **Риск: высокий**
- `COMEDY_ARENA` — волны врагов. **Риск: высокий**
- `COMEDY_ESCORT` — сопровождение. **Риск: средний**

## Совет по подбору контента для стрима
- Часто: **PING/Healing/Weather/лёгкие Summon**
- Редко и дорого: **Dragon/Teleport/Arena/Drop All/Vampire**

## См. также
- **06_INI_Settings.md** — настройки стадий.
- **07_FORCE_Safety_Cooldowns.md** — если используете серии/форс.
