using System.Collections;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using UnityEngine;

namespace BetterNights;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "com.fiodor.betternights";
    public const string PluginName = "Better Nights";
    public const string PluginVersion = "1.0.0";

    private ManualLogSource _log;
    private BundleAssetLoader _assetLoader;
    private SkyMaterialController _skyController;

    private void Awake()
    {
        _log = Logger;

        string pluginDirectory = Path.GetDirectoryName(typeof(Plugin).Assembly.Location);
        Settings.Init(Config, pluginDirectory);

        _assetLoader = new BundleAssetLoader(pluginDirectory, _log);
        _skyController = new SkyMaterialController(_log);

        Settings.TextureAssetName.SettingChanged += (_, _) => ReloadAssets();
        Settings.BackgroundTextureAssetName.SettingChanged += (_, _) => ReloadAssets();
        Settings.ModEnabled.SettingChanged += (_, _) => _skyController.MarkDirty();

        LoadAssets();
        StartCoroutine(ApplyWhenSkyExists());

        _log.LogInfo($"{PluginName} {PluginVersion} loaded.");
    }

    private void ReloadAssets()
    {
        _log.LogInfo("Reloading night sky textures after config change.");
        LoadAssets();
    }

    private void LoadAssets()
    {
        _skyController.SetAssets(_assetLoader.Load());
    }

    private IEnumerator ApplyWhenSkyExists()
    {
        while (true)
        {
            if (!MonoBehaviourSingleton<TOD_Sky>.Instantiated)
            {
                _skyController.ResetForMissingSky();
                yield return new WaitForSeconds(1f);
                continue;
            }

            TOD_Sky sky = MonoBehaviourSingleton<TOD_Sky>.Instance;

            if (!Settings.ModEnabled.Value)
            {
                _skyController.RestoreVanillaSky(sky);
                yield return new WaitForSeconds(1f);
                continue;
            }

            _skyController.TryApply(sky);
            yield return new WaitForSeconds(1f);
        }
    }
}
