using UnityEngine;

namespace BetterNightSkies;

internal sealed class VanillaSkyState
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
