using System;
using System.Collections;
using System.IO;
using BepInEx.Logging;
using UnityEngine;

namespace BetterNightSkies;

internal sealed class BundleAssetLoader
{
    private readonly string _pluginDirectory;
    private readonly ManualLogSource _log;

    private AssetBundle _mainBundle;
    private string _mainBundlePath;
    private string _materialAssetName;
    private Material _replacementMaterial;

    internal BundleAssetLoader(string pluginDirectory, ManualLogSource log)
    {
        _pluginDirectory = pluginDirectory;
        _log = log;
    }

    internal IEnumerator LoadAsync(Action<SkyAssets> onLoaded)
    {
        SkyAssets assets = new SkyAssets();
        string bundlePath = Path.Combine(_pluginDirectory, Settings.BundleFileName.Value);

        AssetBundle bundle = null;
        yield return LoadBundleAsync(bundlePath, loadedBundle => bundle = loadedBundle);
        if (bundle == null)
        {
            onLoaded(assets);
            yield break;
        }

        AssetBundleRequest textureRequest = bundle.LoadAssetAsync<Texture2D>(Settings.TextureAssetName.Value);
        yield return textureRequest;
        assets.SourceNightSkyTexture = textureRequest.asset as Texture2D;

        yield return LoadMaterialAsync(bundle, loadedMaterial => assets.ReplacementMaterial = loadedMaterial);
        yield return LoadBackgroundTextureAsync(bundle, bundlePath, assets);

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

        if (assets.SourceNightSkyTexture == null && assets.ReplacementMaterial == null)
        {
            LogBundleAssetNames(bundle, bundlePath);

            _log.LogError(
                $"Neither texture asset '{Settings.TextureAssetName.Value}', material asset " +
                $"'{Settings.MaterialAssetName.Value}'");
        }

        onLoaded(assets);
    }

    internal void Unload()
    {
        if (_mainBundle != null)
            _mainBundle.Unload(false);

        _mainBundle = null;
        _mainBundlePath = null;
        _materialAssetName = null;
        _replacementMaterial = null;
    }

    private IEnumerator LoadBackgroundTextureAsync(AssetBundle bundle, string bundlePath, SkyAssets assets)
    {
        if (string.IsNullOrWhiteSpace(Settings.BackgroundTextureAssetName.Value))
            yield break;

        AssetBundleRequest cubemapRequest = bundle.LoadAssetAsync<Cubemap>(Settings.BackgroundTextureAssetName.Value);
        yield return cubemapRequest;
        assets.BackgroundCubemap = cubemapRequest.asset as Cubemap;

        if (assets.BackgroundCubemap == null)
        {
            LogBundleAssetNames(bundle, bundlePath);
            _log.LogError($"Background cubemap asset '{Settings.BackgroundTextureAssetName.Value}' was not found in {bundlePath}");
            yield break;
        }

        assets.BackgroundCubemap.wrapMode = TextureWrapMode.Clamp;
        _log.LogInfo($"Loaded background star cubemap '{assets.BackgroundCubemap.name}'.");
    }

    private IEnumerator LoadMaterialAsync(AssetBundle bundle, Action<Material> onLoaded)
    {
        if (_replacementMaterial != null && _materialAssetName == Settings.MaterialAssetName.Value)
        {
            onLoaded(_replacementMaterial);
            yield break;
        }

        AssetBundleRequest materialRequest = bundle.LoadAssetAsync<Material>(Settings.MaterialAssetName.Value);
        yield return materialRequest;

        _replacementMaterial = materialRequest.asset as Material;
        _materialAssetName = Settings.MaterialAssetName.Value;
        onLoaded(_replacementMaterial);
    }

    private IEnumerator LoadBundleAsync(string bundlePath, Action<AssetBundle> onLoaded)
    {
        if (_mainBundle != null && _mainBundlePath == bundlePath)
        {
            onLoaded(_mainBundle);
            yield break;
        }

        if (_mainBundle != null)
            Unload();

        if (!File.Exists(bundlePath))
        {
            _log.LogError($"Night sky bundle not found: {bundlePath}");
            onLoaded(null);
            yield break;
        }

        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
        yield return bundleRequest;

        AssetBundle bundle = bundleRequest.assetBundle;
        if (bundle == null)
        {
            _log.LogError($"Failed to load night sky bundle: {bundlePath}");
            onLoaded(null);
            yield break;
        }

        _mainBundle = bundle;
        _mainBundlePath = bundlePath;
        _materialAssetName = null;
        _replacementMaterial = null;

        onLoaded(bundle);
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
