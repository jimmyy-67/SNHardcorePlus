using System.Collections.Generic;
using System.Collections.ObjectModel;
using HarmonyLib;
using SNHardcorePlus.Configuration;
using UnityEngine;

namespace SNHardcorePlus.Patches;

/// <summary>
/// Crafting cost multiplier.
/// TechData.GetIngredients(TechType) is the real choke point in the current build:
/// the UI and CrafterLogic (IsCraftRecipeFulfilled/ConsumeResources) query it on
/// every use, so returning a scaled copy applies live without mutating the
/// cached recipe database.
/// </summary>
[HarmonyPatch(typeof(TechData), nameof(TechData.GetIngredients))]
internal static class CraftCostPatch
{
    static void Postfix(TechType __0, ref ReadOnlyCollection<Ingredient> __result)
    {
        int mult = DifficultyConfig.CraftingCostMultiplier.Value;
        if (mult <= 1 || __result == null || __result.Count == 0)
            return;

        var scaled = new List<Ingredient>(__result.Count);
        foreach (var ing in __result)
        {
            int amount = ing.amount * mult;
            if (amount < 1)
                amount = 1;
            scaled.Add(new Ingredient(ing.techType, amount));
        }
        __result = new ReadOnlyCollection<Ingredient>(scaled);
    }
}

/// <summary>
/// Power per craft.
/// CrafterLogic.ConsumeEnergy(PowerRelay, float amount) receives the cost
/// (vanilla fabricator = 5): it is replaced with the configured absolute value.
/// </summary>
[HarmonyPatch(typeof(CrafterLogic), nameof(CrafterLogic.ConsumeEnergy))]
internal static class CraftPowerPatch
{
    static void Prefix(ref float __1)
    {
        float wanted = DifficultyConfig.PowerUsedPerCraft.Value;
        if (wanted < 0f)
            wanted = 0f;
        __1 = wanted;
    }
}

/// <summary>
/// Crafting time. The real choke point is CrafterLogic.Craft(TechType, float duration):
/// GhostCrafter.Craft computes the duration (GetCraftTime with a 2.7 s animation
/// floor) and CrafterLogic sets timeCraftingBegin/End from it. Scaling GetCraftTime
/// does not work (the floor eats the reduction); the final duration is scaled here,
/// which covers every station. 0 = instant.
/// </summary>
[HarmonyPatch(typeof(CrafterLogic), nameof(CrafterLogic.Craft))]
internal static class CraftDurationPatch
{
    static void Prefix(ref float __1)
    {
        float mult = DifficultyConfig.CraftTimeMultiplier.Value;
        if (mult < 0f)
            mult = 0f;
        __1 *= mult;
    }
}

/// <summary>
/// Battery and power cell capacity. Verified in dnSpy: TechData.GetMaxCharge only
/// feeds vehicle module slots, NOT batteries. The real capacity is the per-item
/// Battery._capacity field (default 100, public) and the display reads it directly,
/// so scaling it keeps "100/100" style readouts working on their own.
/// Vanilla is captured per TechType on spawn (Pickupable.Awake covers
/// fabricator/console/world/load) and the absolute vanilla*mult is assigned,
/// live when moving the slider.
/// </summary>
[HarmonyPatch(typeof(Pickupable), nameof(Pickupable.Awake))]
internal static class BatteryCapacityPatch
{
    internal static readonly System.Collections.Generic.Dictionary<TechType, float> Vanilla = new System.Collections.Generic.Dictionary<TechType, float>();

    static void Postfix(Pickupable __instance)
    {
        var battery = __instance.GetComponent<Battery>();
        if (battery == null)
            return;
        lock (Vanilla)
        {
            var tech = __instance.GetTechType();
            if (!Vanilla.TryGetValue(tech, out var v))
            {
                v = battery._capacity;
                Vanilla[tech] = v;
            }
            Apply(battery, v);
        }
    }

    internal static void Apply(Battery battery, float vanilla)
    {
        float mult = DifficultyConfig.BatteryCapacityMultiplier.Value;
        if (mult < 0f)
            mult = 0f;
        battery._capacity = vanilla * mult;
        battery.charge = Mathf.Min(battery.charge, battery._capacity);
    }

    internal static void ApplyToAll()
    {
        foreach (var battery in Object.FindObjectsOfType<Battery>())
        {
            var pickup = battery.GetComponent<Pickupable>();
            if (pickup == null)
                continue;
            lock (Vanilla)
            {
                var tech = pickup.GetTechType();
                if (!Vanilla.TryGetValue(tech, out var v))
                {
                    v = battery._capacity;
                    Vanilla[tech] = v;
                }
                Apply(battery, v);
            }
        }
    }
}
