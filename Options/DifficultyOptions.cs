using Nautilus.Options;
using SNHardcorePlus.Configuration;

namespace SNHardcorePlus.Options;

/// <summary>
/// Nautilus Mods menu bound live to the BepInEx config.
/// Standard: multipliers up to x10 (regen 1-10); absolutes with their own ranges.
/// New code, no reuse of the original QModManager config.json.
/// </summary>
internal class DifficultyOptions : ModOptions
{
    public DifficultyOptions() : base("SNHardcorePlus Remake")
    {
        AddItem(DifficultyConfig.DayNightCycleMultiplier.ToModSliderOption(0.1f, 10f, 0.1f, "{0:F1}x"));
        AddItem(DifficultyConfig.CraftingCostMultiplier.ToModSliderOption(1, 10, 1));

        AddItem(DifficultyConfig.PowerUsedPerCraft.ToModSliderOption(0f, 50f, 1f, "{0:F0}"));
        AddItem(DifficultyConfig.CraftTimeMultiplier.ToModSliderOption(0f, 10f, 0.1f, "{0:F1}x"));
        AddItem(DifficultyConfig.BatteryCapacityMultiplier.ToModSliderOption(0f, 10f, 0.1f, "{0:F1}x"));
        AddItem(DifficultyConfig.CyclopsShieldPowerCost.ToModSliderOption(0f, 200f, 5f, "{0:F0}"));
        AddItem(DifficultyConfig.CyclopsSonarPowerCost.ToModSliderOption(0f, 100f, 1f, "{0:F0}"));
        AddItem(DifficultyConfig.CyclopsSilentPowerCost.ToModSliderOption(0f, 50f, 1f, "{0:F0}"));
        AddItem(DifficultyConfig.CyclopsEnginePowerMultiplier.ToModSliderOption(0f, 10f, 0.1f, "{0:F1}x"));

        AddItem(DifficultyConfig.PlantGrowRateMultiplier.ToModSliderOption(0.1f, 10f, 0.1f, "{0:F1}x"));
        AddItem(DifficultyConfig.MaproomPowerDrainMultiplier.ToModSliderOption(0f, 10f, 0.1f, "{0:F1}x"));

        AddItem(DifficultyConfig.HealthMax.ToModSliderOption(10f, 500f, 5f, "{0:F0}"));
        AddItem(DifficultyConfig.RawDamageMultiplier.ToModSliderOption(0f, 10f, 0.1f, "{0:F1}x"));
        AddItem(DifficultyConfig.HealthRegenerationThreshold.ToModSliderOption(0f, 1f, 0.05f, "{0:P0}"));
        AddItem(DifficultyConfig.HealthRegenerationMultiplier.ToModSliderOption(1f, 10f, 0.1f, "{0:F1}x"));
        AddItem(DifficultyConfig.HealthKitRestoreAmount.ToModSliderOption(0f, 200f, 5f, "{0:F0}"));

        AddItem(DifficultyConfig.FoodMax.ToModSliderOption(10f, 500f, 5f, "{0:F0}"));
        AddItem(DifficultyConfig.WaterMax.ToModSliderOption(10f, 500f, 5f, "{0:F0}"));
        AddItem(DifficultyConfig.FoodOverchargeMax.ToModSliderOption(100f, 500f, 5f, "{0:F0}"));
        AddItem(DifficultyConfig.FoodNutritionMultiplier.ToModSliderOption(0f, 10f, 0.1f, "{0:F1}x"));
        AddItem(DifficultyConfig.WaterNutritionMultiplier.ToModSliderOption(0f, 10f, 0.1f, "{0:F1}x"));

        AddItem(DifficultyConfig.FoodDrainMultiplier.ToModSliderOption(0f, 10f, 0.1f, "{0:F1}x"));
        AddItem(DifficultyConfig.WaterDrainMultiplier.ToModSliderOption(0f, 10f, 0.1f, "{0:F1}x"));
        AddItem(DifficultyConfig.OxygenDrainMultiplier.ToModSliderOption(0f, 10f, 0.1f, "{0:F1}x"));

        AddItem(DifficultyConfig.PrawnSeamothPowerDrainMultiplier.ToModSliderOption(0f, 10f, 0.1f, "{0:F1}x"));
        AddItem(DifficultyConfig.HandToolsPowerDrainMultiplier.ToModSliderOption(0f, 10f, 0.1f, "{0:F1}x"));

        AddItem(DifficultyConfig.HealthRespawnRatio.ToModSliderOption(0f, 1f, 0.05f, "{0:P0}"));
        AddItem(DifficultyConfig.FoodStart.ToModSliderOption(0f, 300f, 5f, "{0:F0}"));
        AddItem(DifficultyConfig.WaterStart.ToModSliderOption(0f, 300f, 5f, "{0:F0}"));

        AddItem(DifficultyConfig.StarvationDamageMultiplier.ToModSliderOption(0f, 10f, 0.1f, "{0:F1}x"));
        AddItem(DifficultyConfig.DehydrationDamageMultiplier.ToModSliderOption(0f, 10f, 0.1f, "{0:F1}x"));
        AddItem(DifficultyConfig.RadiationDamageMultiplier.ToModSliderOption(0f, 10f, 0.1f, "{0:F1}x"));

    }
}
