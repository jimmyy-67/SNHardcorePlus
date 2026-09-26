using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;
using SNHardcorePlus.Configuration;
using SNHardcorePlus.Options;
using SNHardcorePlus.Patches;

namespace SNHardcorePlus;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger { get; private set; }

    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    private void Awake()
    {
        Logger = base.Logger;

        DifficultyConfig.BindAll(Config);

        DifficultyConfig.CyclopsShieldPowerCost.SettingChanged += (_, _) => CyclopsCostsPatch.ApplyToAll();
        DifficultyConfig.CyclopsSonarPowerCost.SettingChanged += (_, _) => CyclopsCostsPatch.ApplyToAll();
        DifficultyConfig.CyclopsSilentPowerCost.SettingChanged += (_, _) => CyclopsCostsPatch.ApplyToAll();
        DifficultyConfig.CyclopsEnginePowerMultiplier.SettingChanged += (_, _) => CyclopsEnginePatch.ApplyToAll();
        DifficultyConfig.BatteryCapacityMultiplier.SettingChanged += (_, _) => BatteryCapacityPatch.ApplyToAll();

        OptionsPanelHandler.RegisterModOptions(new DifficultyOptions());

        var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        int ok = 0, fail = 0;
        foreach (var type in Assembly.GetTypes())
        {
            if (type.GetCustomAttributes(typeof(HarmonyPatch), false).Length == 0)
                continue;
            try
            {
                harmony.PatchAll(type);
                ok++;
            }
            catch (System.Exception ex)
            {
                fail++;
                Logger.LogError($"[SNHardcorePlus] Failed to patch {type.FullName}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} loaded (fresh remake). Patches OK={ok} FAIL={fail}.");
    }
}
