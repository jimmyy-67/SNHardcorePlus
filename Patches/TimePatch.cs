using HarmonyLib;
using SNHardcorePlus.Configuration;

namespace SNHardcorePlus.Patches;

/// <summary>
/// Day/night cycle speed.
/// The game may advance time from the _dayNightSpeed field inside Update()
/// without going through the dayNightSpeed getter, so scaling the getter is not enough.
/// This patch measures how far timePassed advanced in the vanilla Update and tops it
/// up to the multiplier: total = advance * mult.
/// Works whether the game uses the field or the getter, and respects pauses, console
/// and time skips (only positive advances of the tick itself are scaled).
/// </summary>
[HarmonyPatch(typeof(DayNightCycle), nameof(DayNightCycle.Update))]
internal static class DayNightAdvancePatch
{
    static void Prefix(DayNightCycle __instance, out float __state)
    {
        __state = __instance.timePassedAsFloat;
    }

    static void Postfix(DayNightCycle __instance, float __state)
    {
        float mult = DifficultyConfig.DayNightCycleMultiplier.Value;
        if (mult == 1f)
            return;
        if (mult < 0f)
            mult = 0f;
        float advanced = __instance.timePassedAsFloat - __state;
        if (advanced > 0f)
        {
            // SetTimePassed is marked [Obsolete] by the devs ("Do not mess
            // with timePassed!"), but it is the only public API that keeps the
            // derived state consistent; assigning the field would break it.
#pragma warning disable CS0618
            __instance.SetTimePassed(__state + advanced * mult);
#pragma warning restore CS0618
        }
    }
}
