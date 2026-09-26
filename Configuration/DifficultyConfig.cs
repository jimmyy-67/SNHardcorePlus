using BepInEx.Configuration;
using UnityEngine;

namespace SNHardcorePlus.Configuration;

/// <summary>
/// Central config of the remake. Written from scratch,
/// inspired only by the option list of the original 2018 mod.
/// Defaults = vanilla behavior.
/// </summary>
internal static class DifficultyConfig
{
    const string SecTime = "Time & world";
    const string SecCraft = "Crafting";
    const string SecCyclops = "Cyclops";
    const string SecPlants = "Plants & Scanner";
    const string SecVitals = "Survival";
    const string SecDamage = "Damage & health";
    const string SecRespawn = "Respawn";

    public static ConfigEntry<float> DayNightCycleMultiplier { get; private set; }
    public static ConfigEntry<int> CraftingCostMultiplier { get; private set; }
    public static ConfigEntry<float> PowerUsedPerCraft { get; private set; }

    public static ConfigEntry<float> CyclopsShieldPowerCost { get; private set; }
    public static ConfigEntry<float> CyclopsSonarPowerCost { get; private set; }
    public static ConfigEntry<float> CyclopsSilentPowerCost { get; private set; }

    public static ConfigEntry<float> PlantGrowRateMultiplier { get; private set; }
    public static ConfigEntry<float> MaproomPowerDrainMultiplier { get; private set; }

    public static ConfigEntry<float> HealthRegenerationThreshold { get; private set; }
    public static ConfigEntry<float> HealthRegenerationMultiplier { get; private set; }
    public static ConfigEntry<float> HealthKitRestoreAmount { get; private set; }
    public static ConfigEntry<float> RawDamageMultiplier { get; private set; }
    public static ConfigEntry<float> HealthMax { get; private set; }

    public static ConfigEntry<float> FoodMax { get; private set; }
    public static ConfigEntry<float> WaterMax { get; private set; }
    public static ConfigEntry<float> FoodOverchargeMax { get; private set; }

    public static ConfigEntry<float> HealthRespawnRatio { get; private set; }
    public static ConfigEntry<float> FoodStart { get; private set; }
    public static ConfigEntry<float> WaterStart { get; private set; }

    public static ConfigEntry<float> FoodDrainMultiplier { get; private set; }
    public static ConfigEntry<float> WaterDrainMultiplier { get; private set; }
    public static ConfigEntry<float> OxygenDrainMultiplier { get; private set; }

    public static ConfigEntry<float> PrawnSeamothPowerDrainMultiplier { get; private set; }
    public static ConfigEntry<float> HandToolsPowerDrainMultiplier { get; private set; }
    public static ConfigEntry<float> CraftTimeMultiplier { get; private set; }
    public static ConfigEntry<float> BatteryCapacityMultiplier { get; private set; }

    public static ConfigEntry<float> StarvationDamageMultiplier { get; private set; }
    public static ConfigEntry<float> DehydrationDamageMultiplier { get; private set; }
    public static ConfigEntry<float> RadiationDamageMultiplier { get; private set; }

    public static ConfigEntry<float> FoodNutritionMultiplier { get; private set; }
    public static ConfigEntry<float> WaterNutritionMultiplier { get; private set; }

    public static ConfigEntry<float> CyclopsEnginePowerMultiplier { get; private set; }

    /// <summary>
    /// Real food cap: the greater of FoodMax and FoodOverchargeMax.
    /// Vanilla = 200 (kMaxOverfillStat). Used in clamps and transpilers.
    /// </summary>
    public static float GetFoodUpper()
    {
        float upper = Mathf.Max(FoodMax.Value, FoodOverchargeMax.Value);
        return upper < 1f ? 1f : upper;
    }

    /// <summary>
    /// HUD bar capacity (the displayed number is the current food/water value;
    /// the bar normalizes against this). Vanilla = 100 for both.
    /// </summary>
    public static float GetFoodCapacity()
    {
        return FoodMax.Value < 1f ? 1f : FoodMax.Value;
    }

    public static float GetWaterCapacity()
    {
        return WaterMax.Value < 1f ? 1f : WaterMax.Value;
    }

    public static void BindAll(ConfigFile cfg)
    {
        DayNightCycleMultiplier = cfg.Bind(SecTime, "DayNightCycleMultiplier", 1f,
            "Day/night cycle speed. 1 = vanilla. Higher = shorter days.");

        CraftingCostMultiplier = cfg.Bind(SecCraft, "CraftingCostMultiplier", 1,
            "Multiplies the amount of every ingredient. 1 = vanilla. 5 = every ingredient costs x5.");
        PowerUsedPerCraft = cfg.Bind(SecCraft, "PowerUsedPerCraft", 5f,
            "Energy consumed per craft at fabricators. 5 = vanilla.");

        CyclopsShieldPowerCost = cfg.Bind(SecCyclops, "CyclopsShieldPowerCost", 50f,
            "Cyclops shield energy cost (SubRoot.shieldPowerCost).");
        CyclopsSonarPowerCost = cfg.Bind(SecCyclops, "CyclopsSonarPowerCost", 10f,
            "Cyclops sonar energy cost (SubRoot.sonarPowerCost).");
        CyclopsSilentPowerCost = cfg.Bind(SecCyclops, "CyclopsSilentPowerCost", 5f,
            "Cyclops silent running energy cost (SubRoot.silentRunningPowerCost).");
        CyclopsEnginePowerMultiplier = cfg.Bind(SecCyclops, "CyclopsEnginePowerMultiplier", 1f,
            "Cyclops engine power consumption multiplier. 1 = vanilla.");

        PlantGrowRateMultiplier = cfg.Bind(SecPlants, "PlantGrowRateMultiplier", 1f,
            "Plant growth speed. 1 = vanilla. 0.5 = half as fast.");
        MaproomPowerDrainMultiplier = cfg.Bind(SecPlants, "MaproomPowerDrainMultiplier", 0.5f,
            "Scanner Room power drain multiplier. 0.5 = half drain (original default). 1 = vanilla.");

        HealthRegenerationThreshold = cfg.Bind(SecVitals, "HealthRegenerationThreshold", 0.75f,
            "Fraction of (FoodMax+WaterMax) required to regen. 0.75 x 200 = 150 = exact vanilla threshold.");
        HealthRegenerationMultiplier = cfg.Bind(SecVitals, "HealthRegenerationMultiplier", 1f,
            "Vanilla regen multiplier (0.42 per 10 s tick). 1 = vanilla, 2 = double, 0 = disabled.");
        HealthKitRestoreAmount = cfg.Bind(SecVitals, "HealthKitRestoreAmount", 50f,
            "Health restored per medkit.");
        RawDamageMultiplier = cfg.Bind(SecDamage, "RawDamageMultiplier", 1f,
            "Multiplies all damage dealt to the player. 2 = double damage.");
        HealthMax = cfg.Bind(SecDamage, "HealthMax", 100f,
            "Player max health.");

        FoodMax = cfg.Bind(SecVitals, "FoodMax", 100f, "Max food (vanilla kMaxStat = 100).");
        WaterMax = cfg.Bind(SecVitals, "WaterMax", 100f, "Max water (vanilla = 100, no overfill).");
        FoodOverchargeMax = cfg.Bind(SecVitals, "FoodOverchargeMax", 200f,
            "Food overcharge cap (vanilla kMaxOverfillStat = 200).");

        HealthRespawnRatio = cfg.Bind(SecRespawn, "HealthRespawnRatio", 0.6f,
            "Fraction of HealthMax you respawn with.");
        FoodStart = cfg.Bind(SecRespawn, "FoodStart", 60f, "Food on respawn.");
        WaterStart = cfg.Bind(SecRespawn, "WaterStart", 40f, "Water on respawn.");

        FoodDrainMultiplier = cfg.Bind(SecVitals, "FoodDrainMultiplier", 1f, "Hunger rate.");
        WaterDrainMultiplier = cfg.Bind(SecVitals, "WaterDrainMultiplier", 1f, "Thirst rate.");
        OxygenDrainMultiplier = cfg.Bind(SecVitals, "OxygenDrainMultiplier", 1f, "Oxygen consumption rate.");
        FoodNutritionMultiplier = cfg.Bind(SecVitals, "FoodNutritionMultiplier", 1f,
            "Multiplier for food gained per item. Below 1 = poor diets.");
        WaterNutritionMultiplier = cfg.Bind(SecVitals, "WaterNutritionMultiplier", 1f,
            "Multiplier for water gained per item/drink.");

        PrawnSeamothPowerDrainMultiplier = cfg.Bind(SecCraft, "PrawnSeamothPowerDrainMultiplier", 1f,
            "Vehicle power drain multiplier (Seamoth/Prawn).");
        HandToolsPowerDrainMultiplier = cfg.Bind(SecCraft, "HandToolsPowerDrainMultiplier", 1f,
            "Hand tool power drain multiplier.");
        CraftTimeMultiplier = cfg.Bind(SecCraft, "CraftTimeMultiplier", 1f,
            "Crafting time multiplier. 1 = vanilla, 2 = twice as slow, 0 = instant.");
        BatteryCapacityMultiplier = cfg.Bind(SecCraft, "BatteryCapacityMultiplier", 1f,
            "Battery and power cell capacity multiplier. Below 1 = shorter lived.");

        StarvationDamageMultiplier = cfg.Bind(SecDamage, "StarvationDamageMultiplier", 1f,
            "Starvation damage multiplier. 1 = vanilla (base 25).");
        DehydrationDamageMultiplier = cfg.Bind(SecDamage, "DehydrationDamageMultiplier", 1f,
            "Dehydration damage multiplier. 1 = vanilla (base 25).");
        RadiationDamageMultiplier = cfg.Bind(SecDamage, "RadiationDamageMultiplier", 1f,
            "Radiation damage multiplier (applied before suit protection).");

    }
}
