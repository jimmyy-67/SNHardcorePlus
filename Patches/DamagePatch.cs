using HarmonyLib;
using SNHardcorePlus.Configuration;

namespace SNHardcorePlus.Patches;

/// <summary>
/// Player max health. LiveMixin.maxHealth is the real getter.
/// Player only exists on the player, so creatures and vehicles are untouched.
/// </summary>
[HarmonyPatch(typeof(LiveMixin), nameof(LiveMixin.maxHealth), MethodType.Getter)]
internal static class MaxHealthPatch
{
    static void Postfix(LiveMixin __instance, ref float __result)
    {
        if (__instance.GetComponent<Player>() == null)
            return;
        float max = DifficultyConfig.HealthMax.Value;
        if (max < 1f)
            max = 1f;
        __result = max;
    }
}

/// <summary>
/// Incoming player damage scaling. DamageSystem.CalculateDamage(damage, type,
/// target, dealer) is the real choke point. RawDamageMultiplier to everything;
/// Starve/Dehydrate by state (food<=0 alone = starving, water<=0 alone = dehydrated,
/// both = average); Radiation with its multiplier (before suit protection).
/// </summary>
[HarmonyPatch(typeof(DamageSystem), nameof(DamageSystem.CalculateDamage))]
internal static class DamagePatch
{
    static void Prefix(ref float __0, DamageType __1, UnityEngine.GameObject __2, UnityEngine.GameObject __3)
    {
        if (Player.main == null || __2 != Player.main.gameObject)
            return;

        float raw = DifficultyConfig.RawDamageMultiplier.Value;
        if (raw < 0f)
            raw = 0f;

        if (__1 == DamageType.Starve)
        {
            float starveCfg = DifficultyConfig.StarvationDamageMultiplier.Value;
            float dehyCfg = DifficultyConfig.DehydrationDamageMultiplier.Value;
            if (starveCfg < 0f)
                starveCfg = 0f;
            if (dehyCfg < 0f)
                dehyCfg = 0f;

            var survival = Player.main.GetComponent<Survival>();
            bool starving = survival == null || survival.food <= 0f;
            bool dehydrated = survival == null || survival.water <= 0f;

            float factor;
            if (starving && !dehydrated)
                factor = starveCfg;
            else if (dehydrated && !starving)
                factor = dehyCfg;
            else
                factor = (starveCfg + dehyCfg) * 0.5f;

            __0 = __0 * raw * factor;
        }
        else
        {
            __0 *= raw;
            if (__1 == DamageType.Radiation)
            {
                float mult = DifficultyConfig.RadiationDamageMultiplier.Value;
                if (mult < 0f)
                    mult = 0f;
                __0 *= mult;
            }
        }
    }
}
