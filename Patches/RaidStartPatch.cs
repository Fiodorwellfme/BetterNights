using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace BetterNightSkies.Patches;

internal sealed class RaidStartPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        => AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));

    [PatchPostfix]
    private static void Postfix()
    {
        Plugin.Instance?.OnRaidStarted();
    }
}
