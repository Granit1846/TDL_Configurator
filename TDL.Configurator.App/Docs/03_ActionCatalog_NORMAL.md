# Команды NORMAL: Action Catalog (обзор)

Команды **NORMAL** — основной «безопасный» режим. Формат запуска:

```txt
tdl_send.exe NORMAL <ACTION> [SOURCE]
```

Ниже — список наиболее используемых ACTION в этой сборке. В конкретной версии мода список может быть шире.

---

## SYSTEM (проверка / базовые действия)
- `SYSTEM_PING` — проверка связи
- `SYSTEM_HEALING` — лечение / восстановление
- `SYSTEM_BLESSING` — благословение / бафф (если поддерживается)

## SUMMON (призывы)
- `SUMMON_ANY`
- `SUMMON_STRONG`
- `SUMMON_UNDEAD`
- `SUMMON_SKELETON`
- `SUMMON_ANIMAL`
- `SUMMON_HUMANOID`
- `SUMMON_DRAGON`

## TELEPORT (телепорты)
- `TELEPORT_RANDOM_CITY`
- `TELEPORT_RANDOM_DANGER`
- `TELEPORT_WHITERUN`
- `TELEPORT_SOLITUDE`
- `TELEPORT_WINDHELM`
- `TELEPORT_RIFTEN`
- `TELEPORT_MARKARTH`
- `TELEPORT_FALKREATH`
- `TELEPORT_DAWNSTAR`
- `TELEPORT_HIGH_HROTHGAR`

## VIRUS (болезни/проклятия)
- `VIRUS_DISEASE`
- `VIRUS_VAMPIRE`
- `VIRUS_WEREWOLF`

## WEATHER (погода)
- `WEATHER_CLEAR`
- `WEATHER_RAIN`
- `WEATHER_SNOW`
- `WEATHER_STORM`
- `WEATHER_FOG`
- `WEATHER_RESET`

## INVENTORY (инвентарь)
- `INVENTORY_SCATTER` — «рассыпать» предметы
- `INVENTORY_DROP_ALL` — сбросить всё (опасно)

## CHAOS (хаос/физика)
- `CHAOS_BACKFIRE`
- `CHAOS_LOW_G`

## WRATH (серии урона/эффекта)
- `WRATH_11`
- `WRATH_12`
- `WRATH_13`

## HUNTER
- `HUNTER_START`

## GIGANT (размер/скорость)
- `GIGANT_BIG`
- `GIGANT_SMALL`
- `GIGANT_SPEED`
- `GIGANT_SLOW`
- `GIGANT_RESET`

## COMEDY (сценарии)
- `COMEDY_ARENA`
- `COMEDY_ESCORT`
- `COMEDY_FAKE_HERO`
- `COMEDY_HORROR`

---

## Как читать список
Если UI пишет «Телепорт → Вайтран», это обычно означает:
```txt
tdl_send.exe NORMAL TELEPORT_WHITERUN 2
```

## Где искать актуальные названия/подсказки
- В UI конфигуратора (вкладки Test/QuickAccess и т.п.).
- В документах 09_ActionReference.md (описания).
- В переводах/таблицах мода (если вы ведёте их в репозитории).

## См. также
- 09_ActionReference.md — объяснение «что будет» и «когда не сработает».
- 02_Protocol.md — режимы и аргументы.
