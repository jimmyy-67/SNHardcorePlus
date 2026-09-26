using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using SNHardcorePlus.Configuration;

namespace SNHardcorePlus.Patches;

/// <summary>
/// Food/water HUD bars. Vanilla (dnSpy):
///   LateUpdate: capacity = 100f literal; SetValue(has, capacity) with
///     fill = has/capacity and text = ceil(fill*capacity) = current value.
///   pulseReferenceCapacity = 100f (public field) for the low-stat pulse warning.
/// The literal is replaced with FoodMax/WaterMax and the pulse is synced.
/// If the pattern does not match, the original is returned untouched (fail-safe).
/// </summary>
[HarmonyPatch(typeof(uGUI_FoodBar), "LateUpdate")]
internal static class FoodBarPatch
{
    static void Prefix(uGUI_FoodBar __instance)
    {
        __instance.pulseReferenceCapacity = DifficultyConfig.GetFoodCapacity();
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return BarCapacity.Replace(instructions,
            AccessTools.Method(typeof(DifficultyConfig), nameof(DifficultyConfig.GetFoodCapacity)),
            "FoodBar");
    }
}

[HarmonyPatch(typeof(uGUI_WaterBar), "LateUpdate")]
internal static class WaterBarPatch
{
    static void Prefix(uGUI_WaterBar __instance)
    {
        __instance.pulseReferenceCapacity = DifficultyConfig.GetWaterCapacity();
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return BarCapacity.Replace(instructions,
            AccessTools.Method(typeof(DifficultyConfig), nameof(DifficultyConfig.GetWaterCapacity)),
            "WaterBar");
    }
}

internal static class BarCapacity
{
    internal static IEnumerable<CodeInstruction> Replace(IEnumerable<CodeInstruction> instructions, MethodInfo getCapacity, string tag)
    {
        var codes = new List<CodeInstruction>(instructions);
        var hits = new List<int>();
        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i].operand is float f && f == 100f)
                hits.Add(i);
        }
        if (hits.Count != 1)
        {
            Plugin.Logger.LogError($"[SNHardcorePlus] {tag} transpiler: expected 1 literal (100), found {hits.Count}. Bar stays vanilla.");
            return codes;
        }
        codes[hits[0]] = new CodeInstruction(OpCodes.Call, getCapacity);
        return codes;
    }
}
