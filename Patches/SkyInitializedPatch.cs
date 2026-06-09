using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace BetterNightSkies.Patches;

internal sealed class SkyInitializedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        => AccessTools.Method(typeof(TOD_Sky), nameof(TOD_Sky.Initialize));

    [PatchPostfix]
    private static void Postfix(TOD_Sky __instance)
    {
        Plugin.Instance?.OnSkyInitialized(__instance);
    }
}
