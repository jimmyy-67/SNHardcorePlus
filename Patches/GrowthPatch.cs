using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using SNHardcorePlus.Configuration;

namespace SNHardcorePlus.Patches;

/// <summary>
/// Plant growth. GrowingPlant.GetGrowthDuration() is the real choke point:
/// total duration is divided by the speed (2 = twice as fast).
/// </summary>
[HarmonyPatch(typeof(GrowingPlant), nameof(GrowingPlant.GetGrowthDuration))]
internal static class PlantGrowthPatch
{
    static void Postfix(ref float __result)
    {
        float mult = DifficultyConfig.PlantGrowRateMultiplier.Value;
        if (mult <= 0.01f)
            mult = 0.01f;
        __result /= mult;
    }
}

/// <summary>
/// Scanner Room. Real drain verified in dnSpy (MapRoomFunctionality.UpdateScanning,
/// once per second):
///   powerConsumer.ConsumePower(scanActive ? 0.5f : 0.15f, out _)
/// The 0.5/0.15 are inline consts: they cannot be assigned, and touching
/// PowerConsumer.consumptionRate does nothing (that field is only written by
/// PollPowerConsumption for UI display; scaling it drains nothing).
/// This transpiler multiplies both literals by the configured value at drain time,
/// so the Mods menu applies live. If the pattern is missing (game changed the
/// method), it logs an error instead of hiding it.
/// </summary>
[HarmonyPatch(typeof(MapRoomFunctionality), "UpdateScanning")]
internal static class MapRoomPowerPatch
{
    static float DrainMultiplier()
    {
        float mult = DifficultyConfig.MaproomPowerDrainMultiplier.Value;
        return mult < 0f ? 0f : mult;
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var consumePower = AccessTools.Method(typeof(PowerConsumer), nameof(PowerConsumer.ConsumePower));
        var getMult = AccessTools.Method(typeof(MapRoomPowerPatch), nameof(DrainMultiplier));

        var codes = new List<CodeInstruction>(instructions);
        var hits = new List<int>();

        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode != OpCodes.Ldc_R4 || !(codes[i].operand is float f))
                continue;
            if (f != 0.5f && f != 0.15f)
                continue;

            bool feedsConsume = false;
            for (int j = i + 1; j < codes.Count && j <= i + 12; j++)
            {
                if (codes[j].Calls(consumePower))
                {
                    feedsConsume = true;
                    break;
                }
            }
            if (feedsConsume)
                hits.Add(i);
        }

        if (hits.Count != 2)
        {
            Plugin.Logger.LogError($"[SNHardcorePlus] MapRoom transpiler: expected 2 literals (0.5/0.15), found {hits.Count}. Drain stays vanilla.");
            return codes;
        }

        for (int k = hits.Count - 1; k >= 0; k--)
        {
            int i = hits[k];
            codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, getMult));
            codes.Insert(i + 2, new CodeInstruction(OpCodes.Mul));
        }

        return codes;
    }
}
