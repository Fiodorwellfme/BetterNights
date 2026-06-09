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
    private Coroutine _loadAssetsCoroutine;
    private bool _reloadAssetsPending;

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

        QueueAssetLoad();
        StartCoroutine(ApplyWhenSkyExists());

        _log.LogInfo($"{PluginName} {PluginVersion} loaded.");
    }

    private void ReloadAssets()
    {
        _log.LogInfo("Reloading night sky textures after config change.");
        QueueAssetLoad();
    }

    private void OnDestroy()
    {
        if (_loadAssetsCoroutine != null)
        {
            StopCoroutine(_loadAssetsCoroutine);
            _loadAssetsCoroutine = null;
        }

        if (_assetLoader != null)
            _assetLoader.Unload();
    }

    private void QueueAssetLoad()
    {
        if (_loadAssetsCoroutine != null)
        {
            _reloadAssetsPending = true;
            return;
        }

        _loadAssetsCoroutine = StartCoroutine(LoadAssetsAsync());
    }

    private IEnumerator LoadAssetsAsync()
    {
        do
        {
            _reloadAssetsPending = false;
            SkyAssets assets = null;

            yield return _assetLoader.LoadAsync(loadedAssets => assets = loadedAssets);
            _skyController.SetAssets(assets);
        }
        while (_reloadAssetsPending);

        _loadAssetsCoroutine = null;
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
