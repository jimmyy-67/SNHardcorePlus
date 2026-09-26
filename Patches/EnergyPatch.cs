using HarmonyLib;
using SNHardcorePlus.Configuration;

namespace SNHardcorePlus.Patches;

/// <summary>
/// Portable energy. Two real paths in this build:
/// - EnergyMixin.ConsumeEnergy(float): hand tools (PlayerTool) and
///   battery-powered consumption. Distinguished by hierarchy.
/// - Vehicle.ConsumeEnergy(float): Seamoth (SeaMoth) and Prawn (Exosuit)
///   motor drain, both inherit Vehicle. Without this second patch the
///   vehicle multiplier would not cover the motor.
/// </summary>
[HarmonyPatch(typeof(EnergyMixin), nameof(EnergyMixin.ConsumeEnergy))]
internal static class PortablePowerPatch
{
    static void Prefix(EnergyMixin __instance, ref float __0)
    {
        float mult;
        if (__instance.GetComponentInParent<Vehicle>() != null)
            mult = DifficultyConfig.PrawnSeamothPowerDrainMultiplier.Value;
        else if (__instance.GetComponentInParent<PlayerTool>() != null)
            mult = DifficultyConfig.HandToolsPowerDrainMultiplier.Value;
        else
            return;

        if (mult < 0f)
            mult = 0f;
        __0 *= mult;
    }
}

[HarmonyPatch(typeof(Vehicle), nameof(Vehicle.ConsumeEnergy), new System.Type[] { typeof(float) })]
internal static class VehicleMotorPowerPatch
{
    static void Prefix(ref float __0)
    {
        float mult = DifficultyConfig.PrawnSeamothPowerDrainMultiplier.Value;
        if (mult < 0f)
            mult = 0f;
        __0 *= mult;
    }
}

/// <summary>
/// Oxygen. Oxygen.RemoveOxygen(float amount) is the real choke point, called
/// from Player when not breathing; scaling it does not affect AddOxygen
/// at the surface nor breathing in bases/vehicles.
/// </summary>
[HarmonyPatch(typeof(Oxygen), nameof(Oxygen.RemoveOxygen))]
internal static class OxygenDrainPatch
{
    static void Prefix(ref float __0)
    {
        float mult = DifficultyConfig.OxygenDrainMultiplier.Value;
        if (mult < 0f)
            mult = 0f;
        __0 *= mult;
    }
}
