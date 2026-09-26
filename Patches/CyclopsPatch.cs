using HarmonyLib;
using SNHardcorePlus.Configuration;
using UnityEngine;

namespace SNHardcorePlus.Patches;

/// <summary>
/// Cyclops costs. Verified in build 82304: they are instance fields
/// on SubRoot (shieldPowerCost / sonarPowerCost / silentRunningPowerCost),
/// read on every ShieldIteration / SonarPing / SilentRunningIteration.
/// Applied on spawn and on config change (event, no per-frame tick).
/// </summary>
[HarmonyPatch(typeof(SubRoot), nameof(SubRoot.Start))]
internal static class CyclopsCostsPatch
{
    static void Postfix(SubRoot __instance)
    {
        if (!__instance.isCyclops)
            return;
        Apply(__instance);
    }

    internal static void Apply(SubRoot sub)
    {
        sub.shieldPowerCost = DifficultyConfig.CyclopsShieldPowerCost.Value;
        sub.sonarPowerCost = DifficultyConfig.CyclopsSonarPowerCost.Value;
        sub.silentRunningPowerCost = DifficultyConfig.CyclopsSilentPowerCost.Value;
    }

    internal static void ApplyToAll()
    {
        foreach (var sub in Object.FindObjectsOfType<SubRoot>())
        {
            if (sub.isCyclops)
                Apply(sub);
        }
    }
}

/// <summary>
/// Cyclops engine consumption. The drain reads the motorModePowerConsumption array
/// (indexed by mode: off/slow/standard/flank), whether SubControl reads it directly
/// or via GetPowerConsumption. Scaling the array itself covers ALL readers without
/// a transpiler: vanilla is stored per instance and vanilla*mult is assigned
/// (idempotent, live). The getter postfix was removed: with the array scaled,
/// scaling the getter would square the multiplier.
/// </summary>
[HarmonyPatch(typeof(CyclopsMotorMode), nameof(CyclopsMotorMode.Start))]
internal static class CyclopsEnginePatch
{
    internal static readonly System.Collections.Generic.Dictionary<int, float[]> Vanilla = new System.Collections.Generic.Dictionary<int, float[]>();

    static void Postfix(CyclopsMotorMode __instance)
    {
        lock (Vanilla)
        {
            Vanilla[__instance.GetInstanceID()] = (float[])__instance.motorModePowerConsumption.Clone();
            Apply(__instance);
        }
    }

    internal static void Apply(CyclopsMotorMode mode)
    {
        int id = mode.GetInstanceID();
        lock (Vanilla)
        {
            if (!Vanilla.TryGetValue(id, out var v))
            {
                v = (float[])mode.motorModePowerConsumption.Clone();
                Vanilla[id] = v;
            }
            float mult = DifficultyConfig.CyclopsEnginePowerMultiplier.Value;
            if (mult < 0f)
                mult = 0f;
            var arr = mode.motorModePowerConsumption;
            for (int i = 0; i < v.Length && i < arr.Length; i++)
                arr[i] = v[i] * mult;
        }
    }

    internal static void ApplyToAll()
    {
        foreach (var mode in Object.FindObjectsOfType<CyclopsMotorMode>())
            Apply(mode);
    }
}
