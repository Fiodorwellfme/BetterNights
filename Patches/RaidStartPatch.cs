using Comfort.Common;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace BetterNightSkies.Patches;

internal sealed class RaidStartPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        => AccessTools.Method(typeof(BloodOnScreen), nameof(BloodOnScreen.Start));

    [PatchPrefix]
    private static void Prefix()
    {
        GameWorld gameWorld = Singleton<GameWorld>.Instance;
        Player player = gameWorld?.MainPlayer;

        if (player == null || player is HideoutPlayer)
            return;

        TOD_Sky sky = Object.FindObjectOfType<TOD_Sky>();
        if (sky == null)
            return;

        Plugin.Instance?.OnRaidStarted(sky);
    }
}
