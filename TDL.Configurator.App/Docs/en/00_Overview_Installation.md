# TDL — Twitch Dragonborn Legacy: overview and installation
<!-- visibility: normal -->

TDL connects Skyrim SE/AE with streaming platforms and tools (Twitch / YouTube / TikTok, etc.). Viewers, donations, points, or polls can trigger curated “show stages” in-game via controlled commands.

## What TDL consists of
1) **The mod (ESP/scripts/content)** — runs stages (Chaos, Comedy, Hunter, Inventory, etc.).
2) **SKSE DLL plugin** (`TDL_StreamPlugin.dll`) — a bridge between external stream events and the game.
3) **TDL Configurator** — an app for INI tuning, log viewing, and testing actions.

## Requirements
- Skyrim **Special Edition** or **Anniversary Edition** (SE/AE).
- SKSE that matches your game runtime.
- Installed TDL package (mod + DLL plugin + `tdl_send.exe` tool).

## Required files and typical locations
Check these paths (relative to the game root):
- `Data\SKSE\Plugins\TDL_StreamPlugin.dll`
- `Data\SKSE\Plugins\TDL_StreamPlugin.ini` (created by Configurator if missing)
- `Data\TDL\Tools\tdl_send.exe`
- (optional) `Data\TDL\Config\TDL_Cooldowns.ini` — limits/cooldowns (especially for FORCE/BURST)

## Installation (short)
1) Install TDL like a regular mod (Vortex/MO2 or manual).
2) Confirm the DLL is in `Data\SKSE\Plugins\`.
3) Confirm `tdl_send.exe` is in `Data\TDL\Tools\`.
4) Run **TDL Configurator** and set the game root folder in **Settings** (where `SkyrimSE.exe` / `SkyrimSELauncher.exe` are).
5) If the INI is missing: **QuickAccess → “Create INI (template)”**.

## Mod managers (MO2/Vortex)
A common “it doesn’t work” case: Configurator writes the INI to one folder, while the game reads Data from another (virtual/deployed).
- Ensure the **game path** in Configurator matches the build you actually launch.
- After changing INI, a safe rule is: **restart Skyrim** if you don’t see changes immediately.

## First check (2 minutes)
1) Launch Skyrim and **load a save** (your character is in the world).
2) In Configurator open **QuickAccess** and run **PING**.
3) Then test a visible action: **SYSTEM_HEALING**.

Next: **01_QuickStart.md**.
