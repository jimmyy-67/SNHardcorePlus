using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using SNHardcorePlus.Configuration;
using UnityEngine;

namespace SNHardcorePlus.Patches;

/// <summary>
/// Hunger/thirst drain. Survival.UpdateStats(float timePassed) is the real choke point.
/// Vanilla formula verified in dnSpy (every 10 s tick from UpdateHunger):
///   foodDrain = 10/2520*100 = 0.397; waterDrain = 10/1800*100 = 0.556
///   damage = (foodDeficit + waterDeficit) * 25 (kStarveDamage)
///   food = clamp(food-drain, 0, 200); water = clamp(water-drain, 0, 100)
/// Since the rates are inline consts, the vanilla drain of the tick is measured and
/// re-scaled: final = postVanilla - vanillaDrain*(mult-1).
/// </summary>
[HarmonyPatch(typeof(Survival), nameof(Survival.UpdateStats))]
internal static class VitalsTickPatch
{
    sealed class TickState
    {
        public float foodBefore;
        public float waterBefore;
    }

    static void Prefix(Survival __instance, out TickState __state)
    {
        __state = new TickState { foodBefore = __instance.food, waterBefore = __instance.water };
    }

    static void Postfix(Survival __instance, float __0, TickState __state)
    {
        float foodMult = DifficultyConfig.FoodDrainMultiplier.Value;
        float waterMult = DifficultyConfig.WaterDrainMultiplier.Value;
        if (foodMult < 0f)
            foodMult = 0f;
        if (waterMult < 0f)
            waterMult = 0f;

        float vanillaFoodDrain = __state.foodBefore - __instance.food;
        float vanillaWaterDrain = __state.waterBefore - __instance.water;

        if (vanillaFoodDrain > 0f && foodMult != 1f)
            __instance.food -= vanillaFoodDrain * (foodMult - 1f);
        if (vanillaWaterDrain > 0f && waterMult != 1f)
            __instance.water -= vanillaWaterDrain * (waterMult - 1f);

        float foodUpper = DifficultyConfig.GetFoodUpper();
        float waterMax = DifficultyConfig.WaterMax.Value;
        if (waterMax < 1f)
            waterMax = 1f;

        __instance.food = Mathf.Clamp(__instance.food, 0f, foodUpper);
        __instance.water = Mathf.Clamp(__instance.water, 0f, waterMax);
    }
}

/// <summary>
/// Regen as a multiplier of vanilla, with exact replacement.
/// Vanilla (dnSpy, Survival.UpdateHunger): if food+water >= 150
/// (kFoodWaterHealThreshold) heals 0.041666668*10 = 0.4167 per tick, and only
/// if RequiresSurvival() and !freezeStats (that gate is respected here too:
/// in creative vanilla does not regen and neither do we).
/// Total per tick = mult x 0.4167 while above our own threshold (1 = vanilla,
/// 0 = disabled); if our threshold is not met but vanilla's is, pre-tick health
/// is restored so raising the threshold actually works.
/// </summary>
[HarmonyPatch(typeof(Survival), nameof(Survival.UpdateHunger))]
internal static class RegenPatch
{
    const float VanillaThreshold = 150f;
    const float VanillaRegenPerTick = 0.4166667f;

    static void Prefix(Survival __instance, out float __state)
    {
        __state = __instance.GetComponent<LiveMixin>().health;
    }

    static void Postfix(Survival __instance, float __state)
    {
        if (!GameModeUtils.RequiresSurvival() || __instance.freezeStats)
            return;

        float mult = DifficultyConfig.HealthRegenerationMultiplier.Value;
        if (mult < 0f)
            mult = 0f;

        float sum = __instance.food + __instance.water;
        bool ourCond = sum >= DifficultyConfig.HealthRegenerationThreshold.Value
            * (DifficultyConfig.FoodMax.Value + DifficultyConfig.WaterMax.Value);
        bool vanillaFired = sum >= VanillaThreshold;

        var mixin = __instance.GetComponent<LiveMixin>();
        if (!mixin.IsAlive() || mixin.IsFullHealth())
            return;

        if (ourCond)
        {
            float extra = mult * VanillaRegenPerTick - (vanillaFired ? VanillaRegenPerTick : 0f);
            if (extra > 0f)
                mixin.AddHealth(extra);
            else if (extra < 0f)
                mixin.health = Mathf.Max(1f, mixin.health + extra);
        }
        else if (vanillaFired)
        {
            // No hunger damage possible here (sum >= 150), vanilla only added
            // regen: restoring pre-tick health cancels it exactly.
            mixin.health = Mathf.Min(__state, mixin.maxHealth);
        }
    }
}

/// <summary>
/// Respawn and new game.
/// Vanilla (dnSpy): Survival.OnRespawn calls ResetStats (50.5/90.5).
/// This postfix applies FoodStart/WaterStart on top.
/// Health is NOT touched here: LiveMixin.OnRespawn (health = maxHealth*1) is another
/// handler of the same event in unknown order and would overwrite it; it is patched
/// separately in HealthRespawnPatch so it always runs after the vanilla body.
/// </summary>
[HarmonyPatch(typeof(Survival), nameof(Survival.OnRespawn))]
internal static class RespawnPatch
{
    static void Postfix(Survival __instance)
    {
        __instance.food = DifficultyConfig.FoodStart.Value;
        __instance.water = DifficultyConfig.WaterStart.Value;
    }
}

/// <summary>
/// Respawn health: LiveMixin.OnRespawn postfix, player only.
/// Runs after the vanilla body regardless of handler order.
/// </summary>
[HarmonyPatch(typeof(LiveMixin), nameof(LiveMixin.OnRespawn))]
internal static class HealthRespawnPatch
{
    static void Postfix(LiveMixin __instance)
    {
        if (__instance.GetComponent<Player>() == null)
            return;
        float max = DifficultyConfig.HealthMax.Value;
        if (max < 1f)
            max = 1f;
        __instance.health = Mathf.Clamp(max * DifficultyConfig.HealthRespawnRatio.Value, 1f, max);
    }
}

[HarmonyPatch(typeof(Survival), nameof(Survival.ResetStats))]
internal static class NewGameStatsPatch
{
    static void Postfix(Survival __instance)
    {
        __instance.food = DifficultyConfig.FoodStart.Value;
        __instance.water = DifficultyConfig.WaterStart.Value;
    }
}

/// <summary>
/// Eating gate. Vanilla (dnSpy, Survival.Eat): food is only added if food <= 99.
/// Without this, any FoodMax above 100 is unreachable no matter what.
/// The literal (the only 99f in the method) is replaced with FoodMax-1
/// (100 gives exactly 99), evaluated live.
/// </summary>
[HarmonyPatch(typeof(Survival), nameof(Survival.Eat))]
internal static class EatGatePatch
{
    internal static float GateValue()
    {
        float g = DifficultyConfig.FoodMax.Value - 1f;
        return g < 0f ? 0f : g;
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var getGate = AccessTools.Method(typeof(EatGatePatch), nameof(GateValue));
        var codes = new List<CodeInstruction>(instructions);
        var hits = new List<int>();
        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i].operand is float f && f == 99f)
                hits.Add(i);
        }
        if (hits.Count != 1)
        {
            Plugin.Logger.LogError($"[SNHardcorePlus] Eat gate transpiler: expected 1 literal (99), found {hits.Count}. High FoodMax unreachable.");
            return codes;
        }
        codes[hits[0]] = new CodeInstruction(OpCodes.Call, getGate);
        return codes;
    }
}

/// <summary>
/// Food and water overcharge. Vanilla (dnSpy, Survival.Eat): food only if
/// food <= gate with a 200 clamp; water always, with a hard 100 clamp.
/// The food gate is respected and before+nutritive is rebuilt capped at the
/// configured ceilings, correcting upwards if vanilla clamped it.
/// </summary>
[HarmonyPatch(typeof(Survival), nameof(Survival.Eat))]
internal static class OverchargePatch
{
    sealed class EatState
    {
        public float foodBefore;
        public float nutritive;
        public float waterBefore;
        public float waterValue;
        public bool valid;
    }

    static void Prefix(GameObject __0, out EatState __state)
    {
        __state = new EatState { valid = false };
        var eatable = __0.GetComponent<Eatable>();
        if (eatable == null)
            return;
        var survival = Player.main.GetComponent<Survival>();
        if (survival == null)
            return;
        __state.foodBefore = survival.food;
        __state.nutritive = eatable.GetFoodValue();
        __state.waterBefore = survival.water;
        __state.waterValue = eatable.GetWaterValue();
        __state.valid = true;
    }

    static void Postfix(Survival __instance, EatState __state)
    {
        if (!__state.valid)
            return;
        if (__state.foodBefore <= EatGatePatch.GateValue())
        {
            float wanted = Mathf.Min(__state.foodBefore + __state.nutritive, DifficultyConfig.GetFoodUpper());
            if (wanted > __instance.food)
                __instance.food = wanted;
        }
        float waterMax = DifficultyConfig.WaterMax.Value;
        if (waterMax < 1f)
            waterMax = 1f;
        float wantedWater = Mathf.Min(__state.waterBefore + __state.waterValue, waterMax);
        if (wantedWater > __instance.water)
            __instance.water = wantedWater;
    }
}

/// <summary>
/// Temp damage (cold/poison) drains food. Vanilla (dnSpy,
/// Survival.OnHealTempDamage): food = Clamp(food - damage*0.25, 0, 200).
/// That 200 would cut high food down to the vanilla cap: it is replaced with
/// the configured ceiling (the only 200f in the method).
/// </summary>
[HarmonyPatch(typeof(Survival), nameof(Survival.OnHealTempDamage))]
internal static class TempDamageFoodClampPatch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var getUpper = AccessTools.Method(typeof(DifficultyConfig), nameof(DifficultyConfig.GetFoodUpper));
        var codes = new List<CodeInstruction>(instructions);
        var hits = new List<int>();
        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i].operand is float f && f == 200f)
                hits.Add(i);
        }
        if (hits.Count != 1)
        {
            Plugin.Logger.LogError($"[SNHardcorePlus] TempDamage transpiler: expected 1 literal (200), found {hits.Count}. Clamp stays vanilla.");
            return codes;
        }
        codes[hits[0]] = new CodeInstruction(OpCodes.Call, getUpper);
        return codes;
    }
}

/// <summary>
/// Medkit. Vanilla (dnSpy, Survival.Use): AddHealth(50f); only consumes the
/// item if it heals > 0.1 (at full health it shows "HealthFull" and is kept).
/// The TechType fallback via Pickupable is replicated and only the difference
/// is applied if vanilla consumed it (__result), without reimplementing inventory.
/// </summary>
[HarmonyPatch(typeof(Survival), nameof(Survival.Use))]
internal static class HealthKitPatch
{
    const float VanillaKit = 50f;

    static void Prefix(GameObject __0, out bool __state)
    {
        var tech = CraftData.GetTechType(__0);
        if (tech == TechType.None)
        {
            var pickup = __0.GetComponent<Pickupable>();
            if (pickup != null)
                tech = pickup.GetTechType();
        }
        __state = tech == TechType.FirstAidKit;
    }

    static void Postfix(Survival __instance, bool __state, bool __result)
    {
        if (!__state || !__result)
            return;
        float extra = DifficultyConfig.HealthKitRestoreAmount.Value - VanillaKit;
        if (extra == 0f)
            return;
        var mixin = __instance.GetComponent<LiveMixin>();
        if (extra > 0f)
            mixin.AddHealth(extra);
        else
            mixin.health = Mathf.Max(1f, mixin.health + extra);
    }
}

/// <summary>
/// Food nutritional value. Eatable.GetFoodValue/GetWaterValue are the real choke
/// points (read by Survival.Eat and by this same mod when rebuilding overcharge,
/// so everything stays consistent). Below 1 = poor diets.
/// </summary>
[HarmonyPatch(typeof(Eatable), nameof(Eatable.GetFoodValue))]
internal static class FoodNutritionPatch
{
    static void Postfix(ref float __result)
    {
        float mult = DifficultyConfig.FoodNutritionMultiplier.Value;
        if (mult < 0f)
            mult = 0f;
        __result *= mult;
    }
}

[HarmonyPatch(typeof(Eatable), nameof(Eatable.GetWaterValue))]
internal static class WaterNutritionPatch
{
    static void Postfix(ref float __result)
    {
        float mult = DifficultyConfig.WaterNutritionMultiplier.Value;
        if (mult < 0f)
            mult = 0f;
        __result *= mult;
    }
}
