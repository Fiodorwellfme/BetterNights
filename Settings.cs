using System.Collections.Generic;
using System.IO;
using BepInEx.Configuration;
using UnityEngine;

namespace BetterNights;

internal static class Settings
{
    internal const string DefaultBundleFileName = "nightsky.bundle";
    internal const string DefaultTextureAssetName = "assets/Ringed Brown Dwarf.png";
    internal const string DefaultMaterialAssetName = "assets/nightskymaterial.mat";
    internal const string DefaultBackgroundTextureAssetName = "assets/Background Stars.png";
    internal const string DefaultTexturePropertyName = "_MainTex";

    internal static readonly List<ConfigEntryBase> ConfigEntries = new List<ConfigEntryBase>();

    internal static ConfigEntry<bool> ModEnabled;

    internal static ConfigEntry<string> BundleFileName;
    internal static ConfigEntry<string> TextureAssetName;
    internal static ConfigEntry<string> MaterialAssetName;
    internal static ConfigEntry<string> BackgroundBundleFileName;
    internal static ConfigEntry<string> BackgroundTextureAssetName;
    internal static ConfigEntry<string> TexturePropertyName;

    internal static ConfigEntry<float> StarsBrightness;
    internal static ConfigEntry<float> BackgroundBrightness;
    internal static ConfigEntry<float> SkySaturation;
    internal static ConfigEntry<float> FadeTimeMultiplier;

    internal static ConfigEntry<float> HorizontalScale;
    internal static ConfigEntry<float> VerticalScale;
    internal static ConfigEntry<float> HorizonFadeStartDegrees;
    internal static ConfigEntry<float> HorizonFadeEndDegrees;
    internal static ConfigEntry<float> HorizonBrightnessMultiplier;
    internal static ConfigEntry<bool> HorizonFadeDebug;
    internal static ConfigEntry<float> BackgroundScale;
    internal static ConfigEntry<float> HorizontalOffsetDegrees;
    internal static ConfigEntry<float> VerticalOffsetDegrees;
    internal static ConfigEntry<float> BackgroundHorizontalOffsetDegrees;
    internal static ConfigEntry<float> BackgroundVerticalOffsetDegrees;
    internal static ConfigEntry<bool> MainBandEnabled;
    internal static ConfigEntry<float> MainBandCenterU;
    internal static ConfigEntry<float> MainBandCenterV;
    internal static ConfigEntry<float> MainBandWidth;
    internal static ConfigEntry<float> MainBandHeight;
    internal static ConfigEntry<bool> MainClampToTransparent;
    internal static ConfigEntry<float> MainHorizontalFade;
    internal static ConfigEntry<float> MainVerticalFade;
    internal static ConfigEntry<float> YawDegrees;
    internal static ConfigEntry<float> PitchDegrees;
    internal static ConfigEntry<float> RollDegrees;

    internal static void Init(ConfigFile config, string pluginDirectory)
    {
        ConfigEntries.Clear();

        ConfigEntries.Add(ModEnabled = config.Bind("General", "Enabled", true,
            new ConfigDescription(
                "",
                null,
                new global::ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false })));

        ConfigEntries.Add(BundleFileName = config.Bind("Textures", "BundleFileName", "nightsky.bundle",
            new ConfigDescription(
                "Main asset bundle file name.",
                null,
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        ConfigEntries.Add(TextureAssetName = config.Bind("Textures", "Sky Texture", "assets/Ringed Brown Dwarf.png",
            CreateTextureAssetDescription(
                GetTextureAssetNames(pluginDirectory, BundleFileName.Value, includeEmpty: false, DefaultTextureAssetName),
                "Night sky texture.",
                false)));

        ConfigEntries.Add(MaterialAssetName = config.Bind("Textures", "MaterialAssetName", "assets/nightskymaterial.mat",
            new ConfigDescription(
                "Material asset used for custom equirectangular sky rendering.",
                null,
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        ConfigEntries.Add(BackgroundBundleFileName = config.Bind("Textures", "BackgroundBundleFileName", "nightsky.bundle",
            new ConfigDescription(
                "Optional background star asset bundle file name.",
                null,
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        ConfigEntries.Add(BackgroundTextureAssetName = config.Bind("Textures", "BackgroundTextureAssetName", "assets/Background Stars.png",
            CreateTextureAssetDescription(
                GetTextureAssetNames(pluginDirectory, BackgroundBundleFileName.Value, includeEmpty: false, DefaultBackgroundTextureAssetName),
                "Optional background star texture asset in the bundle.",
                true)));

        ConfigEntries.Add(TexturePropertyName = config.Bind("Textures", "TexturePropertyName", "_MainTex",
            new ConfigDescription(
                "Texture property assigned on the space material.",
                null,
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        ConfigEntries.Add(StarsBrightness = config.Bind("Sky", "Sky brightness", 0.5f,
            new ConfigDescription(
                "Sky brightness.",
                new AcceptableValueRange<float>(0f, 5f),
                new global::ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false })));

        ConfigEntries.Add(BackgroundBrightness = config.Bind("Stars", "Stars brightness", 1f,
            new ConfigDescription(
                "Background stars brightness.",
                new AcceptableValueRange<float>(0f, 5f),
                new global::ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false })));

        ConfigEntries.Add(SkySaturation = config.Bind("Sky", "Saturation", 2.5f,
            new ConfigDescription(
                "Sky color saturation. 0 = grayscale, 1 = original, >1 = more saturated.",
                new AcceptableValueRange<float>(0f, 10f),
                new global::ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false })));

        ConfigEntries.Add(FadeTimeMultiplier = config.Bind("General", "Time of Day Fade Multiplier", 1.6f,
            new ConfigDescription(
                "Higher values make the night sky appear later and disappear sooner.",
                new AcceptableValueRange<float>(0f, 3f),
                new global::ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false })));

        ConfigEntries.Add(HorizontalScale = config.Bind("Sky", "Horizontal Scale", 1f,
            new ConfigDescription(
                "Horizontal texture scale. Higher values make sky features smaller/repeated.",
                new AcceptableValueRange<float>(0.1f, 8f),
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        ConfigEntries.Add(VerticalScale = config.Bind("Sky", "Vertical Scale", 1f,
            new ConfigDescription(
                "Vertical texture scale. Higher values make sky features smaller vertically.",
                new AcceptableValueRange<float>(0.1f, 8f),
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        ConfigEntries.Add(HorizonFadeStartDegrees = config.Bind("Horizon Fade", "Horizon Fade Start Degrees", 0f,
            new ConfigDescription(
                "Sky altitude where horizon dimming is strongest. Lower this to move the dimmed area closer to the ground.",
                new AcceptableValueRange<float>(-15f, 45f),
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        ConfigEntries.Add(HorizonFadeEndDegrees = config.Bind("Horizon Fade", "Horizon Fade End Degrees", 20f,
            new ConfigDescription(
                "Sky altitude where horizon dimming reaches full brightness. Lower this if the fade reaches too high into the sky.",
                new AcceptableValueRange<float>(-15f, 90f),
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        ConfigEntries.Add(HorizonBrightnessMultiplier = config.Bind("Horizon Fade", "Horizon Brightness Multiplier", 0.25f,
            new ConfigDescription(
                "Brightness multiplier at the horizon. 100% means no horizon fade, 0 means no visible stars around the horizon.",
                new AcceptableValueRange<float>(0f, 1f),
                new global::ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = true })));

        ConfigEntries.Add(HorizonFadeDebug = config.Bind("Horizon Fade", "Show Horizon Fade Zone", false,
            new ConfigDescription(
                "Tint the area affected by horizon fade in pink.",
                null,
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        ConfigEntries.Add(BackgroundScale = config.Bind("Stars", "Stars Scale", 3f,
            new ConfigDescription(
                "Background star texture scale.",
                new AcceptableValueRange<float>(0.1f, 32f),
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        ConfigEntries.Add(HorizontalOffsetDegrees = config.Bind("Sky", "HorizontalOffsetDegrees", 0f,
            new ConfigDescription(
                "Horizontal texture offset in degrees.",
                new AcceptableValueRange<float>(-360f, 360f),
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        ConfigEntries.Add(VerticalOffsetDegrees = config.Bind("Sky", "VerticalOffsetDegrees", 0f,
            new ConfigDescription(
                "Vertical texture offset in degrees.",
                new AcceptableValueRange<float>(-180f, 180f),
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        ConfigEntries.Add(BackgroundHorizontalOffsetDegrees = config.Bind("Stars", "BackgroundHorizontalOffsetDegrees", 0f,
            new ConfigDescription(
                "Background star texture horizontal offset in degrees.",
                new AcceptableValueRange<float>(-360f, 360f),
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        ConfigEntries.Add(BackgroundVerticalOffsetDegrees = config.Bind("Stars", "BackgroundVerticalOffsetDegrees", 0f,
            new ConfigDescription(
                "Background star texture vertical offset in degrees.",
                new AcceptableValueRange<float>(-180f, 180f),
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        ConfigEntries.Add(MainBandEnabled = config.Bind("Sky", "MainBandEnabled", true,
            new ConfigDescription(
                "When enabled, main texture coverage is controlled by normalized band size instead of texture resolution.",
                null,
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        ConfigEntries.Add(MainBandCenterU = config.Bind("Sky", "MainBandCenterU", 0.05f,
            new ConfigDescription(
                "Main band horizontal center in normalized sky UV space.",
                new AcceptableValueRange<float>(0f, 1f),
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        ConfigEntries.Add(MainBandCenterV = config.Bind("Sky", "MainBandCenterV", 0.5f,
            new ConfigDescription(
                "Main band vertical center in normalized sky UV space.",
                new AcceptableValueRange<float>(0f, 1f),
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        ConfigEntries.Add(MainBandWidth = config.Bind("Sky", "MainBandWidth", 0.8f,
            new ConfigDescription(
                "Main band horizontal sky coverage. 1 = full 360 degrees, 0.5 = half the horizon.",
                new AcceptableValueRange<float>(0.001f, 1f),
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = true })));

        ConfigEntries.Add(MainBandHeight = config.Bind("Sky", "MainBandHeight", 0.7f,
            new ConfigDescription(
                "Main band vertical sky coverage. 0.5 = half the vertical skydome.",
                new AcceptableValueRange<float>(0.001f, 1f),
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = true })));

        ConfigEntries.Add(MainClampToTransparent = config.Bind("Sky", "MainClampToTransparent", true,
            new ConfigDescription(
                "When enabled, the sky texture alpha becomes 0 outside its vertical 0..1 UV range.",
                null,
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        ConfigEntries.Add(MainHorizontalFade = config.Bind("Sky", "MainHorizontalFade", 0.2f,
            new ConfigDescription(
                "Soft horizontal alpha fade at the left/right edges of the main band.",
                new AcceptableValueRange<float>(0f, 1f),
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = true })));

        ConfigEntries.Add(MainVerticalFade = config.Bind("Sky", "MainVerticalFade", 0.6f,
            new ConfigDescription(
                "Soft vertical alpha fade at the top/bottom of the main texture UV range.",
                new AcceptableValueRange<float>(0f, 1f),
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = true })));

        ConfigEntries.Add(YawDegrees = config.Bind("Sky", "YawDegrees", 180f,
            new ConfigDescription(
                "Rotates the sky around the vertical axis.",
                new AcceptableValueRange<float>(-360f, 360f),
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        ConfigEntries.Add(PitchDegrees = config.Bind("Sky", "PitchDegrees", -50f,
            new ConfigDescription(
                "Tilts the sky forward/backward.",
                new AcceptableValueRange<float>(-180f, 180f),
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        ConfigEntries.Add(RollDegrees = config.Bind("Sky", "RollDegrees", 20f,
            new ConfigDescription(
                "Rolls the sky around the forward axis.",
                new AcceptableValueRange<float>(-180f, 180f),
                new global::ConfigurationManagerAttributes { IsAdvanced = true, ShowRangeAsPercent = false })));

        RecalcOrder();
    }

    private static string[] GetTextureAssetNames(
        string pluginDirectory,
        string bundleFileName,
        bool includeEmpty,
        string defaultAssetName)
    {
        string bundlePath = Path.Combine(pluginDirectory, bundleFileName);
        List<string> assetNames = new List<string>();

        if (includeEmpty)
            AddUnique(assetNames, string.Empty);

        if (!string.IsNullOrWhiteSpace(defaultAssetName))
            AddUnique(assetNames, defaultAssetName);

        if (!File.Exists(bundlePath))
            return assetNames.ToArray();

        AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
        if (bundle == null)
            return assetNames.ToArray();

        foreach (string assetName in bundle.GetAllAssetNames())
        {
            if (bundle.LoadAsset<Texture2D>(assetName) != null)
                AddUnique(assetNames, assetName);
        }

        bundle.Unload(true);
        return assetNames.ToArray();
    }

    private static void AddUnique(List<string> values, string value)
    {
        if (!values.Contains(value))
            values.Add(value);
    }

    private static ConfigDescription CreateTextureAssetDescription(string[] textureAssetNames, string description, bool advanced)
    {
        if (textureAssetNames.Length == 0)
            return new ConfigDescription(description, null, Attributes(advanced));

        return new ConfigDescription(
            description,
            new AcceptableValueList<string>(textureAssetNames),
            Attributes(advanced));
    }

    private static global::ConfigurationManagerAttributes Attributes(bool advanced = false)
    {
        return new global::ConfigurationManagerAttributes
        {
            IsAdvanced = advanced,
            ShowRangeAsPercent = false
        };
    }

    private static void RecalcOrder()
    {
        int settingOrder = ConfigEntries.Count;
        foreach (ConfigEntryBase entry in ConfigEntries)
        {
            global::ConfigurationManagerAttributes attributes =
                entry.Description.Tags[0] as global::ConfigurationManagerAttributes;

            if (attributes != null)
                attributes.Order = settingOrder;

            settingOrder--;
        }
    }
}
