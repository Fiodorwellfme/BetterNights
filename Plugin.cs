using System;
using System.Collections;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BetterNightSkies.Patches;
using UnityEngine;

namespace BetterNightSkies;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "com.fiodor.betternightskies";
    public const string PluginName = "Better Night Skies";
    public const string PluginVersion = "1.0.0";

    internal static Plugin Instance { get; private set; }

    private ManualLogSource _log;
    private BundleAssetLoader _assetLoader;
    private SkyMaterialController _skyController;
    private Coroutine _loadAssetsCoroutine;
    private bool _reloadAssetsPending;
    private TOD_Sky _currentSky;

    private void Awake()
    {
        Instance = this;
        _log = Logger;

        string pluginDirectory = Path.GetDirectoryName(typeof(Plugin).Assembly.Location);
        Settings.Init(Config, pluginDirectory);

        _assetLoader = new BundleAssetLoader(pluginDirectory, _log);
        _skyController = new SkyMaterialController(_log);
        new RaidStartPatch().Enable();
        new SkyInitializedPatch().Enable();

        SubscribeSettings();

        QueueAssetLoad();

        _log.LogInfo($"{PluginName} {PluginVersion} loaded.");
    }

    private void OnSettingChanged(object sender, EventArgs args)
    {
        ConfigEntryBase entry = sender as ConfigEntryBase;
        if (RequiresAssetReload(entry))
        {
            _log.LogInfo("Reloading night sky assets after config change.");
            QueueAssetLoad();
            return;
        }

        _skyController.MarkDirty();
        ApplyCurrentSky();
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

        UnsubscribeSettings();

        if (Instance == this)
            Instance = null;
    }

    internal void OnRaidStarted()
    {
        TryRandomizeMainTextureForRaid();
    }

    internal void OnSkyInitialized(TOD_Sky sky)
    {
        _currentSky = sky;
        _skyController.ResetForMissingSky();
        ApplyCurrentSky();
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
            ApplyCurrentSky();
        }
        while (_reloadAssetsPending);

        _loadAssetsCoroutine = null;
    }

    private void ApplyCurrentSky()
    {
        if (_currentSky == null)
            return;

        if (!Settings.ModEnabled.Value)
        {
            _skyController.RestoreVanillaSky(_currentSky);
            return;
        }

        _skyController.TryApply(_currentSky);
    }

    private void TryRandomizeMainTextureForRaid()
    {
        if (!Settings.RandomMainTextureOnRaidStart.Value)
            return;

        string selectedTexture = GetRandomMainTextureName();
        if (string.IsNullOrWhiteSpace(selectedTexture) || selectedTexture == Settings.TextureAssetName.Value)
            return;

        Settings.TextureAssetName.Value = selectedTexture;
        _log.LogInfo($"Selected random raid sky texture '{selectedTexture}'.");
    }

    private static string GetRandomMainTextureName()
    {
        string[] textureNames = Settings.TextureAssetNames;
        string[] candidates = new string[textureNames.Length];
        int candidateCount = 0;

        for (int i = 0; i < textureNames.Length; i++)
        {
            string textureName = textureNames[i];
            if (string.IsNullOrWhiteSpace(textureName))
                continue;

            if (textureName == Settings.BackgroundTextureAssetName.Value)
                continue;

            string lowerName = textureName.ToLowerInvariant();
            if (lowerName.Contains("doge") || lowerName.Contains("acidmode"))
                continue;

            candidates[candidateCount] = textureName;
            candidateCount++;
        }

        if (candidateCount == 0)
            return null;

        return candidates[UnityEngine.Random.Range(0, candidateCount)];
    }

    private static bool RequiresAssetReload(ConfigEntryBase entry)
    {
        return entry == Settings.BundleFileName ||
            entry == Settings.TextureAssetName ||
            entry == Settings.MaterialAssetName ||
            entry == Settings.BackgroundBundleFileName ||
            entry == Settings.BackgroundTextureAssetName;
    }

    private void SubscribeSettings()
    {
        SubscribeSetting(Settings.ModEnabled);
        SubscribeSetting(Settings.RandomMainTextureOnRaidStart);
        SubscribeSetting(Settings.BundleFileName);
        SubscribeSetting(Settings.TextureAssetName);
        SubscribeSetting(Settings.MaterialAssetName);
        SubscribeSetting(Settings.BackgroundBundleFileName);
        SubscribeSetting(Settings.BackgroundTextureAssetName);
        SubscribeSetting(Settings.TexturePropertyName);
        SubscribeSetting(Settings.StarsBrightness);
        SubscribeSetting(Settings.BackgroundBrightness);
        SubscribeSetting(Settings.BackgroundSaturation);
        SubscribeSetting(Settings.SkySaturation);
        SubscribeSetting(Settings.FadeTimeMultiplier);
        SubscribeSetting(Settings.HorizontalScale);
        SubscribeSetting(Settings.VerticalScale);
        SubscribeSetting(Settings.HorizonFadeStartDegrees);
        SubscribeSetting(Settings.HorizonFadeEndDegrees);
        SubscribeSetting(Settings.HorizonBrightnessMultiplier);
        SubscribeSetting(Settings.HorizonFadeDebug);
        SubscribeSetting(Settings.BackgroundScale);
        SubscribeSetting(Settings.HorizontalOffsetDegrees);
        SubscribeSetting(Settings.VerticalOffsetDegrees);
        SubscribeSetting(Settings.BackgroundHorizontalOffsetDegrees);
        SubscribeSetting(Settings.BackgroundVerticalOffsetDegrees);
        SubscribeSetting(Settings.MainBandEnabled);
        SubscribeSetting(Settings.MainBandCenterU);
        SubscribeSetting(Settings.MainBandCenterV);
        SubscribeSetting(Settings.MainBandWidth);
        SubscribeSetting(Settings.MainBandHeight);
        SubscribeSetting(Settings.MainClampToTransparent);
        SubscribeSetting(Settings.MainHorizontalFade);
        SubscribeSetting(Settings.MainVerticalFade);
        SubscribeSetting(Settings.YawDegrees);
        SubscribeSetting(Settings.PitchDegrees);
        SubscribeSetting(Settings.RollDegrees);
    }

    private void UnsubscribeSettings()
    {
        UnsubscribeSetting(Settings.ModEnabled);
        UnsubscribeSetting(Settings.RandomMainTextureOnRaidStart);
        UnsubscribeSetting(Settings.BundleFileName);
        UnsubscribeSetting(Settings.TextureAssetName);
        UnsubscribeSetting(Settings.MaterialAssetName);
        UnsubscribeSetting(Settings.BackgroundBundleFileName);
        UnsubscribeSetting(Settings.BackgroundTextureAssetName);
        UnsubscribeSetting(Settings.TexturePropertyName);
        UnsubscribeSetting(Settings.StarsBrightness);
        UnsubscribeSetting(Settings.BackgroundBrightness);
        UnsubscribeSetting(Settings.BackgroundSaturation);
        UnsubscribeSetting(Settings.SkySaturation);
        UnsubscribeSetting(Settings.FadeTimeMultiplier);
        UnsubscribeSetting(Settings.HorizontalScale);
        UnsubscribeSetting(Settings.VerticalScale);
        UnsubscribeSetting(Settings.HorizonFadeStartDegrees);
        UnsubscribeSetting(Settings.HorizonFadeEndDegrees);
        UnsubscribeSetting(Settings.HorizonBrightnessMultiplier);
        UnsubscribeSetting(Settings.HorizonFadeDebug);
        UnsubscribeSetting(Settings.BackgroundScale);
        UnsubscribeSetting(Settings.HorizontalOffsetDegrees);
        UnsubscribeSetting(Settings.VerticalOffsetDegrees);
        UnsubscribeSetting(Settings.BackgroundHorizontalOffsetDegrees);
        UnsubscribeSetting(Settings.BackgroundVerticalOffsetDegrees);
        UnsubscribeSetting(Settings.MainBandEnabled);
        UnsubscribeSetting(Settings.MainBandCenterU);
        UnsubscribeSetting(Settings.MainBandCenterV);
        UnsubscribeSetting(Settings.MainBandWidth);
        UnsubscribeSetting(Settings.MainBandHeight);
        UnsubscribeSetting(Settings.MainClampToTransparent);
        UnsubscribeSetting(Settings.MainHorizontalFade);
        UnsubscribeSetting(Settings.MainVerticalFade);
        UnsubscribeSetting(Settings.YawDegrees);
        UnsubscribeSetting(Settings.PitchDegrees);
        UnsubscribeSetting(Settings.RollDegrees);
    }

    private void SubscribeSetting<T>(ConfigEntry<T> entry)
    {
        if (entry != null)
            entry.SettingChanged += OnSettingChanged;
    }

    private void UnsubscribeSetting<T>(ConfigEntry<T> entry)
    {
        if (entry != null)
            entry.SettingChanged -= OnSettingChanged;
    }
}
