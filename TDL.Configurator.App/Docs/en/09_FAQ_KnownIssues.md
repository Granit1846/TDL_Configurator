# FAQ / Known issues
<!-- visibility: normal -->

## Common questions
**Q: Why does PING fail?**  
A: Most often you are in the main menu, the DLL did not load, or your paths don’t match (MO2/Vortex). Start with **08_Logs_Diagnostics.md**.

**Q: Can I trigger stages without the Configurator?**  
A: Yes. Any bot/script can run `tdl_send.exe` with arguments. See **03_Bot_Integration_Guide.md**.

**Q: Why was the command accepted but nothing happened?**  
A: Scene state (dialog/cutscene), cooldown/limit, action missing/disabled, or Skyrim needs a restart after INI changes.

**Q: Why is FORCE/BURST risky?**  
A: It runs repeated triggers and can overload the game or break scenes (especially teleports/inventory). See **07_FORCE_Safety_Cooldowns.md**.

## Typical issues
- **MO2/Vortex:** Configurator writes to a physical folder while the game reads virtual/deployed Data. Verify paths.
- **Runtime mismatch:** DLL built for a different runtime → plugin won’t load or may crash.
- **Teleports:** easy to break quests during dialogs/scripted scenes.
- **Inventory scatter/drop:** aggressive counts/intervals cause performance problems.

## Practical tips
- Keep heavy actions rare and expensive.
- Do not allow viewers to use FORCE without cooldowns.
- Always start diagnostics with `SYSTEM_PING`.
