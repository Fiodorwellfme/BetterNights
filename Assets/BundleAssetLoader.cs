using System.IO;
using BepInEx.Logging;
using UnityEngine;

namespace BetterNights;

internal sealed class BundleAssetLoader
{
    private readonly string _pluginDirectory;
    private readonly ManualLogSource _log;

    internal BundleAssetLoader(string pluginDirectory, ManualLogSource log)
    {
        _pluginDirectory = pluginDirectory;
        _log = log;
    }

    internal SkyAssets Load()
    {
        SkyAssets assets = new SkyAssets();
        string bundlePath = Path.Combine(_pluginDirectory, Settings.BundleFileName.Value);

        if (!File.Exists(bundlePath))
        {
            _log.LogError($"Night sky bundle not found: {bundlePath}");
            return assets;
        }

        AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
        if (bundle == null)
        {
            _log.LogError($"Failed to load night sky bundle: {bundlePath}");
            return assets;
        }

        assets.SourceNightSkyTexture = bundle.LoadAsset<Texture2D>(Settings.TextureAssetName.Value);
        assets.ReplacementMaterial = bundle.LoadAsset<Material>(Settings.MaterialAssetName.Value);

        if (assets.SourceNightSkyTexture == null && assets.ReplacementMaterial == null)
        {
            LogBundleAssetNames(bundle, bundlePath);
            bundle.Unload(false);

            _log.LogError(
                $"Neither texture asset '{Settings.TextureAssetName.Value}' nor material asset " +
                $"'{Settings.MaterialAssetName.Value}' was found in {bundlePath}");
            return assets;
        }

        bundle.Unload(false);

        if (assets.SourceNightSkyTexture != null)
        {
            assets.SourceNightSkyTexture.wrapMode = TextureWrapMode.Repeat;
            _log.LogInfo($"Loaded night sky texture '{assets.SourceNightSkyTexture.name}'.");
        }

        if (assets.ReplacementMaterial != null)
        {
            assets.ReplacementMaterial.renderQueue = 1010;
            _log.LogInfo($"Loaded night sky material '{assets.ReplacementMaterial.name}'.");
        }

        LoadBackgroundTexture(assets);
        return assets;
    }

    private void LoadBackgroundTexture(SkyAssets assets)
    {
        if (string.IsNullOrWhiteSpace(Settings.BackgroundTextureAssetName.Value))
            return;

        string bundlePath = Path.Combine(_pluginDirectory, Settings.BackgroundBundleFileName.Value);
        if (!File.Exists(bundlePath))
        {
            _log.LogError($"Background star bundle not found: {bundlePath}");
            return;
        }

        AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
        if (bundle == null)
        {
            _log.LogError($"Failed to load background star bundle: {bundlePath}");
            return;
        }

        assets.BackgroundTexture = bundle.LoadAsset<Texture2D>(Settings.BackgroundTextureAssetName.Value);

        if (assets.BackgroundTexture == null)
        {
            LogBundleAssetNames(bundle, bundlePath);
            bundle.Unload(false);
            _log.LogError($"Background texture asset '{Settings.BackgroundTextureAssetName.Value}' was not found in {bundlePath}");
            return;
        }

        bundle.Unload(false);

        assets.BackgroundTexture.wrapMode = TextureWrapMode.Repeat;
        _log.LogInfo($"Loaded background star texture '{assets.BackgroundTexture.name}'.");
    }

    private void LogBundleAssetNames(AssetBundle bundle, string bundlePath)
    {
        string[] assetNames = bundle.GetAllAssetNames();

        if (assetNames.Length == 0)
        {
            _log.LogError($"Bundle contains no named assets: {bundlePath}");
            return;
        }

        _log.LogError($"Assets found in {bundlePath}:");

        foreach (string assetName in assetNames)
        {
            _log.LogError($"- {assetName}");
        }
    }
}
