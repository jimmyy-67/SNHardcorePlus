# SNHardcorePlus Remake

Customize the difficulty of your Subnautica survival or hardcore playthrough by editing values — a from-scratch remake of [SNHardcorePlus by qwiso](https://www.nexusmods.com/subnautica/mods/17) for the current version of Subnautica 1 (Living Large / 2025 patch), running on **BepInEx + Nautilus**.

## Requirements

- Latest Subnautica 1 (Steam or Epic).
- [BepInEx Pack for Subnautica](https://www.nexusmods.com/subnautica/mods/1108) (install first, run the game once to the menu, close it).
- [Nautilus](https://www.nexusmods.com/subnautica/mods/1262) (extract into `Subnautica\BepInEx\`, so you get `BepInEx\plugins\Nautilus`).

## Installation

**Vortex (recommended):** install the downloaded zip as usual, Enable, Deploy, and launch the game from Vortex.

**Manual:** copy `SNHardcorePlus.dll` into `Subnautica\BepInEx\plugins\SNHardcorePlus\` so it looks like this:

```
Subnautica
└── BepInEx
    └── plugins
        └── SNHardcorePlus
            └── SNHardcorePlus.dll
```

## How to configure

You have two ways (both live, no restart needed):

1. **In-game menu (easiest):** `Options > Mods > SNHardcorePlus Remake` — every option is a slider.
2. **Config file:** `BepInEx\config\SNHardcorePlus.cfg` — edit with any text editor while the game is closed.

Rule of thumb: **multipliers default to 1.0** (= vanilla game). Higher makes that thing stronger/faster/hungrier; lower than 1 weakens it. Absolute values (health, food, energy costs) use normal game units.

## All options (33)

**Time & world**
- `DayNightCycleMultiplier` (1.0) — day/night speed. Higher = shorter days. Note: it also speeds up plant growth and crafting, like in vanilla.

**Crafting**
- `CraftingCostMultiplier` (1) — multiplies every ingredient in every recipe. 5 = everything costs x5.
- `PowerUsedPerCraft` (5.0) — base energy consumed per craft. Vanilla ≈ 5.
- `CraftTimeMultiplier` (1.0) — crafting time. 2 = twice as slow, 0 = instant.
- `BatteryCapacityMultiplier` (1.0) — battery and power cell capacity. Below 1 = shorter lived.

**Cyclops**
- `CyclopsShieldPowerCost` (50.0), `CyclopsSonarPowerCost` (10.0), `CyclopsSilentPowerCost` (5.0) — energy costs. Vanilla values shown.
- `CyclopsEnginePowerMultiplier` (1.0) — engine power consumption while driving.

**Plants & Scanner Room**
- `PlantGrowRateMultiplier` (1.0) — plant growth speed. 0.5 = half as fast.
- `MaproomPowerDrainMultiplier` (0.5) — Scanner Room drain. 0.5 = half drain, 1.0 = vanilla.

**Survival**
- `HealthMax` (100.0) — your max health.
- `HealthKitRestoreAmount` (50.0) — HP per medkit. Vanilla = 50.
- `HealthRegenerationThreshold` (0.75) — fraction of (FoodMax+WaterMax) needed to regen. 0.75 = vanilla threshold.
- `HealthRegenerationMultiplier` (1.0) — regen rate (vanilla ≈ 0.42 HP per 10 s). 2 = double, 0 = off. Only works in Survival mode, like vanilla.
- `FoodMax` (100.0) / `WaterMax` (100.0) — max values. The HUD bars adapt to them.
- `FoodOverchargeMax` (200.0) — how far food can overfill when you overeat. Vanilla = 200.
- `FoodStart` (60.0) / `WaterStart` (40.0) — food/water on respawn and new game.
- `FoodDrainMultiplier` (1.0) / `WaterDrainMultiplier` (1.0) / `OxygenDrainMultiplier` (1.0) — hunger, thirst and oxygen rates.
- `FoodNutritionMultiplier` (1.0) / `WaterNutritionMultiplier` (1.0) — how much food/water each item gives. Below 1 = poor diets.

**Damage (to you)**
- `RawDamageMultiplier` (1.0) — all incoming damage. 2 = double.
- `StarvationDamageMultiplier` (1.0) / `DehydrationDamageMultiplier` (1.0) — hunger/thirst damage. 1 = vanilla.
- `RadiationDamageMultiplier` (1.0) — radiation damage, applied before suit protection.

**Respawn**
- `HealthRespawnRatio` (0.6) — fraction of HealthMax on respawn.

**Energy (vehicles & tools)**
- `PrawnSeamothPowerDrainMultiplier` (1.0) — Seamoth/Prawn drain, motor included.
- `HandToolsPowerDrainMultiplier` (1.0) — scanner, laser cutter, etc.

## Upgrading from v1.0 (important)

Version 2.0 is a full rewrite: the old `config.json` is gone (deleted automatically? No — **delete it yourself** if it exists next to the dll). Settings now live in `BepInEx\config\SNHardcorePlus.cfg` and the Mods menu. Three defaults changed meaning, so **delete your old `.cfg` once to regenerate it**:

- `Starvation/DehydrationDamageMultiplier`: were absolute 25, now multipliers (1.0 = vanilla). An old 25 would mean 25× damage!
- `HealthRegenerationAmount` (absolute) → `HealthRegenerationMultiplier` (1.0 = vanilla).
- `FoodOverchargeMax`: now defaults to 200 (vanilla) instead of 150.

## Troubleshooting

- **A slider does nothing**: test in Survival mode (creative disables power Hunger/drain mechanics), make sure the value actually saved, and check `BepInEx\LogOutput.log` for lines starting with `[SNHardcorePlus]`.
- **Vortex users**: after updating the mod, Deploy again and check the dll in `BepInEx\plugins\SNHardcorePlus\` is the new one. Vortex purges unmanaged files on launch.
- **Something broke after a game update**: post your `LogOutput.log` and game build number in Bugs or join the Discord: https://discord.gg/5TqdqY27en

## Credits

Original concept and mod by [qwiso](https://www.nexusmods.com/subnautica/mods/17) — this remake honors that work with all-new code (MIT). Permissions per the Nexus page; do not redistribute qwiso's original zip.
