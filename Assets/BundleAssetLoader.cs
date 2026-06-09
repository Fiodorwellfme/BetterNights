using System;
using System.Collections;
using System.IO;
using BepInEx.Logging;
using UnityEngine;

namespace BetterNights;

internal sealed class BundleAssetLoader
{
    private readonly string _pluginDirectory;
    private readonly ManualLogSource _log;

    private AssetBundle _mainBundle;
    private AssetBundle _backgroundBundle;
    private string _mainBundlePath;
    private string _backgroundBundlePath;
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
        yield return LoadBundleAsync(bundlePath, true, loadedBundle => bundle = loadedBundle);
        if (bundle == null)
        {
            onLoaded(assets);
            yield break;
        }

        AssetBundleRequest textureRequest = bundle.LoadAssetAsync<Texture2D>(Settings.TextureAssetName.Value);
        yield return textureRequest;
        assets.SourceNightSkyTexture = textureRequest.asset as Texture2D;

        yield return LoadMaterialAsync(bundle, loadedMaterial => assets.ReplacementMaterial = loadedMaterial);

        if (assets.SourceNightSkyTexture == null && assets.ReplacementMaterial == null)
        {
            LogBundleAssetNames(bundle, bundlePath);

            _log.LogError(
                $"Neither texture asset '{Settings.TextureAssetName.Value}' nor material asset " +
                $"'{Settings.MaterialAssetName.Value}' was found in {bundlePath}");
            onLoaded(assets);
            yield break;
        }

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

        yield return LoadBackgroundTextureAsync(assets);
        onLoaded(assets);
    }

    internal void Unload()
    {
        if (_backgroundBundle != null && _backgroundBundle != _mainBundle)
            _backgroundBundle.Unload(false);

        if (_mainBundle != null)
            _mainBundle.Unload(false);

        _mainBundle = null;
        _backgroundBundle = null;
        _mainBundlePath = null;
        _backgroundBundlePath = null;
        _materialAssetName = null;
        _replacementMaterial = null;
    }

    private IEnumerator LoadBackgroundTextureAsync(SkyAssets assets)
    {
        if (string.IsNullOrWhiteSpace(Settings.BackgroundTextureAssetName.Value))
            yield break;

        string bundlePath = Path.Combine(_pluginDirectory, Settings.BackgroundBundleFileName.Value);
        AssetBundle bundle = null;
        yield return LoadBundleAsync(bundlePath, false, loadedBundle => bundle = loadedBundle);
        if (bundle == null)
            yield break;

        AssetBundleRequest textureRequest = bundle.LoadAssetAsync<Texture2D>(Settings.BackgroundTextureAssetName.Value);
        yield return textureRequest;
        assets.BackgroundTexture = textureRequest.asset as Texture2D;

        if (assets.BackgroundTexture == null)
        {
            LogBundleAssetNames(bundle, bundlePath);
            _log.LogError($"Background texture asset '{Settings.BackgroundTextureAssetName.Value}' was not found in {bundlePath}");
            yield break;
        }

        assets.BackgroundTexture.wrapMode = TextureWrapMode.Repeat;
        _log.LogInfo($"Loaded background star texture '{assets.BackgroundTexture.name}'.");
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

    private IEnumerator LoadBundleAsync(string bundlePath, bool mainBundle, Action<AssetBundle> onLoaded)
    {
        if (mainBundle)
        {
            if (_mainBundle != null && _mainBundlePath == bundlePath)
            {
                onLoaded(_mainBundle);
                yield break;
            }

            if (_mainBundle != null)
                Unload();
        }
        else
        {
            if (_mainBundle != null && _mainBundlePath == bundlePath)
            {
                onLoaded(_mainBundle);
                yield break;
            }

            if (_backgroundBundle != null && _backgroundBundlePath == bundlePath)
            {
                onLoaded(_backgroundBundle);
                yield break;
            }

            if (_backgroundBundle != null)
            {
                _backgroundBundle.Unload(false);
                _backgroundBundle = null;
                _backgroundBundlePath = null;
            }
        }

        if (!File.Exists(bundlePath))
        {
            _log.LogError($"{(mainBundle ? "Night sky" : "Background star")} bundle not found: {bundlePath}");
            onLoaded(null);
            yield break;
        }

        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
        yield return bundleRequest;

        AssetBundle bundle = bundleRequest.assetBundle;
        if (bundle == null)
        {
            _log.LogError($"Failed to load {(mainBundle ? "night sky" : "background star")} bundle: {bundlePath}");
            onLoaded(null);
            yield break;
        }

        if (mainBundle)
        {
            _mainBundle = bundle;
            _mainBundlePath = bundlePath;
            _materialAssetName = null;
            _replacementMaterial = null;
        }
        else
        {
            _backgroundBundle = bundle;
            _backgroundBundlePath = bundlePath;
        }

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
