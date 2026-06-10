using BepInEx.Logging;
using UnityEngine;

namespace BetterNightSkies;

internal sealed class SkyMaterialController
{
    private readonly ManualLogSource _log;
    private readonly VanillaSkyState _vanillaSkyState = new VanillaSkyState();

    private SkyAssets _assets = new SkyAssets();
    private bool _textureApplied;

    internal SkyMaterialController(ManualLogSource log)
    {
        _log = log;
    }

    internal void SetAssets(SkyAssets assets)
    {
        _assets = assets ?? new SkyAssets();
        MarkDirty();
    }

    internal void MarkDirty()
    {
        _textureApplied = false;
    }

    internal void ResetForMissingSky()
    {
        _textureApplied = false;
        _vanillaSkyState.Reset();
    }

    internal void TryApply(TOD_Sky sky)
    {
        if (sky == null || sky.Resources == null)
        {
            _textureApplied = false;
            return;
        }

        if (!_assets.HasReplacement)
            return;

        _vanillaSkyState.Capture(sky);
        ApplyDomeScale(sky);

        if (_assets.ReplacementMaterial != null)
        {
            ApplyReplacementMaterial(sky);
            return;
        }

        ApplyReplacementTexture(sky);
    }

    internal void RestoreVanillaSky(TOD_Sky sky)
    {
        if (!_vanillaSkyState.Captured || sky == null)
            return;

        _vanillaSkyState.Restore(sky);

        if (_textureApplied)
        {
            _log.LogInfo("Restored vanilla night sky rendering.");
            _textureApplied = false;
        }
    }

    private void ApplyReplacementTexture(TOD_Sky sky)
    {
        if (_assets.SourceNightSkyTexture == null)
        {
            _textureApplied = false;
            return;
        }

        Material spaceMaterial = sky.Resources.SpaceMaterial;

        if (spaceMaterial == null)
        {
            _textureApplied = false;
            return;
        }

        spaceMaterial.SetTexture(Settings.TexturePropertyName.Value, _assets.SourceNightSkyTexture);
        sky.Stars.Brightness = Settings.StarsBrightness.Value;

        if (!_textureApplied)
        {
            _log.LogInfo($"Applied night sky texture to {spaceMaterial.name}.{Settings.TexturePropertyName.Value}");
            _textureApplied = true;
        }
    }

    private void ApplyReplacementMaterial(TOD_Sky sky)
    {
        Material material = _assets.ReplacementMaterial;

        if (_assets.SourceNightSkyTexture != null)
        {
            material.SetTexture(Settings.TexturePropertyName.Value, _assets.SourceNightSkyTexture);
        }

        if (_assets.BackgroundCubemap != null && material.HasProperty(ShaderProperties.BackgroundCube))
        {
            material.SetTexture(ShaderProperties.BackgroundCube, _assets.BackgroundCubemap);
        }

        ApplyShaderSettings(sky, material);

        sky.Resources.SpaceMaterial = material;

        if (sky.Components != null && sky.Components.SpaceRenderer != null)
        {
            sky.Components.SpaceRenderer.sharedMaterial = material;
        }

        sky.Stars.Brightness = Settings.StarsBrightness.Value;

        if (!_textureApplied)
        {
            _log.LogInfo($"Applied night sky material '{material.name}'.");
            _textureApplied = true;
        }
    }

    private static void ApplyShaderSettings(TOD_Sky sky, Material material)
    {
        SetMaterialFloat(material, ShaderProperties.Brightness, Settings.StarsBrightness.Value);
        SetMaterialFloat(material, ShaderProperties.BackgroundBrightness, Settings.BackgroundBrightness.Value);
        SetMaterialFloat(material, ShaderProperties.BackgroundSaturation, Settings.BackgroundSaturation.Value);
        SetMaterialFloat(material, ShaderProperties.Saturation, Settings.SkySaturation.Value);
        SetMaterialFloat(material, ShaderProperties.TodVisibility, CalculateTodVisibility(sky));
        SetMaterialFloat(material, ShaderProperties.HorizonFadeStartDegrees, Settings.HorizonFadeStartDegrees.Value);
        SetMaterialFloat(material, ShaderProperties.HorizonFadeEndDegrees, Settings.HorizonFadeEndDegrees.Value);
        SetMaterialFloat(material, ShaderProperties.HorizonBrightnessMultiplier, Settings.HorizonBrightnessMultiplier.Value);
        SetMaterialFloat(material, ShaderProperties.HorizonFadeDebug, Settings.HorizonFadeDebug.Value ? 1f : 0f);
        SetMaterialFloat(material, ShaderProperties.HorizontalScale, Settings.HorizontalScale.Value);
        SetMaterialFloat(material, ShaderProperties.VerticalScale, Settings.VerticalScale.Value);
        SetMaterialFloat(material, ShaderProperties.BackgroundHorizontalScale, Settings.BackgroundScale.Value);
        SetMaterialFloat(material, ShaderProperties.BackgroundVerticalScale, Settings.BackgroundScale.Value);
        SetMaterialFloat(material, ShaderProperties.HorizontalOffsetDegrees, Settings.HorizontalOffsetDegrees.Value);
        SetMaterialFloat(material, ShaderProperties.VerticalOffsetDegrees, Settings.VerticalOffsetDegrees.Value);
        SetMaterialFloat(material, ShaderProperties.BackgroundHorizontalOffsetDegrees, Settings.BackgroundHorizontalOffsetDegrees.Value);
        SetMaterialFloat(material, ShaderProperties.BackgroundVerticalOffsetDegrees, Settings.BackgroundVerticalOffsetDegrees.Value);
        SetMaterialFloat(material, ShaderProperties.MainBandEnabled, Settings.MainBandEnabled.Value ? 1f : 0f);
        SetMaterialFloat(material, ShaderProperties.MainBandCenterU, Settings.MainBandCenterU.Value);
        SetMaterialFloat(material, ShaderProperties.MainBandCenterV, Settings.MainBandCenterV.Value);
        SetMaterialFloat(material, ShaderProperties.MainBandWidth, Settings.MainBandWidth.Value);
        SetMaterialFloat(material, ShaderProperties.MainBandHeight, Settings.MainBandHeight.Value);
        SetMaterialFloat(material, ShaderProperties.MainClampToTransparent, Settings.MainClampToTransparent.Value ? 1f : 0f);
        SetMaterialFloat(material, ShaderProperties.MainHorizontalFade, Settings.MainHorizontalFade.Value);
        SetMaterialFloat(material, ShaderProperties.MainVerticalFade, Settings.MainVerticalFade.Value);
        SetMaterialFloat(material, ShaderProperties.YawDegrees, Settings.YawDegrees.Value);
        SetMaterialFloat(material, ShaderProperties.PitchDegrees, Settings.PitchDegrees.Value);
        SetMaterialFloat(material, ShaderProperties.RollDegrees, Settings.RollDegrees.Value);
    }

    private void ApplyDomeScale(TOD_Sky sky)
    {
        if (sky.Components == null || sky.Components.DomeTransform == null)
            return;

        Vector3 vanillaScale = _vanillaSkyState.GetDomeScale();
        if (vanillaScale == Vector3.zero)
            return;

        sky.Components.DomeTransform.localScale = vanillaScale * Settings.SkyDomeScaleMultiplier.Value;
    }

    private static float CalculateTodVisibility(TOD_Sky sky)
    {
        float fogVisibility = 1f - Mathf.Clamp01(sky.Atmosphere.Fogginess);
        float nightVisibility = 1f - Mathf.Clamp01(sky.LerpValue * Settings.FadeTimeMultiplier.Value);
        return Mathf.Clamp01(fogVisibility * nightVisibility);
    }

    private static void SetMaterialFloat(Material material, string propertyName, float value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetFloat(propertyName, value);
        }
    }

    private static class ShaderProperties
    {
        internal const string BackgroundCube = "_BackgroundCube";
        internal const string Brightness = "_Brightness";
        internal const string BackgroundBrightness = "_BackgroundBrightness";
        internal const string BackgroundSaturation = "_BackgroundSaturation";
        internal const string Saturation = "_Saturation";
        internal const string TodVisibility = "_TodVisibility";
        internal const string HorizonFadeStartDegrees = "_HorizonFadeStartDegrees";
        internal const string HorizonFadeEndDegrees = "_HorizonFadeEndDegrees";
        internal const string HorizonBrightnessMultiplier = "_HorizonBrightnessMultiplier";
        internal const string HorizonFadeDebug = "_HorizonFadeDebug";
        internal const string HorizontalScale = "_HorizontalScale";
        internal const string VerticalScale = "_VerticalScale";
        internal const string BackgroundHorizontalScale = "_BackgroundHorizontalScale";
        internal const string BackgroundVerticalScale = "_BackgroundVerticalScale";
        internal const string HorizontalOffsetDegrees = "_HorizontalOffsetDegrees";
        internal const string VerticalOffsetDegrees = "_VerticalOffsetDegrees";
        internal const string BackgroundHorizontalOffsetDegrees = "_BackgroundHorizontalOffsetDegrees";
        internal const string BackgroundVerticalOffsetDegrees = "_BackgroundVerticalOffsetDegrees";
        internal const string MainBandEnabled = "_MainBandEnabled";
        internal const string MainBandCenterU = "_MainBandCenterU";
        internal const string MainBandCenterV = "_MainBandCenterV";
        internal const string MainBandWidth = "_MainBandWidth";
        internal const string MainBandHeight = "_MainBandHeight";
        internal const string MainClampToTransparent = "_MainClampToTransparent";
        internal const string MainHorizontalFade = "_MainHorizontalFade";
        internal const string MainVerticalFade = "_MainVerticalFade";
        internal const string YawDegrees = "_YawDegrees";
        internal const string PitchDegrees = "_PitchDegrees";
        internal const string RollDegrees = "_RollDegrees";
    }

    private sealed class VanillaSkyState
    {
        private Material _spaceMaterial;
        private Material _rendererMaterial;
        private Texture _spaceTexture;
        private float _starsBrightness;
        private Vector3 _domeScale;

        internal bool Captured { get; private set; }

        internal void Reset()
        {
            _spaceMaterial = null;
            _rendererMaterial = null;
            _spaceTexture = null;
            _starsBrightness = 0f;
            _domeScale = Vector3.zero;
            Captured = false;
        }

        internal void Capture(TOD_Sky sky)
        {
            if (Captured || sky.Resources == null)
                return;

            _spaceMaterial = sky.Resources.SpaceMaterial;
            _rendererMaterial = sky.Components != null && sky.Components.SpaceRenderer != null
                ? sky.Components.SpaceRenderer.sharedMaterial
                : null;

            if (_spaceMaterial != null && _spaceMaterial.HasProperty(Settings.TexturePropertyName.Value))
            {
                _spaceTexture = _spaceMaterial.GetTexture(Settings.TexturePropertyName.Value);
            }

            _starsBrightness = sky.Stars.Brightness;
            _domeScale = sky.Components != null && sky.Components.DomeTransform != null
                ? sky.Components.DomeTransform.localScale
                : Vector3.zero;
            Captured = true;
        }

        internal void Restore(TOD_Sky sky)
        {
            if (!Captured || sky.Resources == null)
                return;

            if (_spaceMaterial != null)
            {
                sky.Resources.SpaceMaterial = _spaceMaterial;

                if (_spaceMaterial.HasProperty(Settings.TexturePropertyName.Value))
                {
                    _spaceMaterial.SetTexture(Settings.TexturePropertyName.Value, _spaceTexture);
                }
            }

            if (sky.Components != null && sky.Components.SpaceRenderer != null && _rendererMaterial != null)
            {
                sky.Components.SpaceRenderer.sharedMaterial = _rendererMaterial;
            }

            sky.Stars.Brightness = _starsBrightness;

            if (sky.Components != null && sky.Components.DomeTransform != null && _domeScale != Vector3.zero)
            {
                sky.Components.DomeTransform.localScale = _domeScale;
            }
        }

        internal Vector3 GetDomeScale()
        {
            return _domeScale;
        }
    }
}
