# Better Nights

Client BepInEx plugin for SPT 4.0 / Unity 2022.3.43f1.

## What It Does

The plugin waits for `TOD_Sky.Instance` in raid, then assigns a custom `Texture2D` to the TOD space material:

```csharp
TOD_Sky.Instance.Resources.SpaceMaterial
```

It also applies configured `TOD_Sky.Instance.Stars.Brightness` and `TOD_Sky.Instance.Stars.Tiling`.

## Asset Bundle

Create a Unity asset bundle with Unity 2022.3.43f1:

- Bundle file name: `nightsky.bundle`
- Texture asset name: `NightSkyTexture`
- Texture type: `Texture2D`

Place `nightsky.bundle` next to the compiled plugin DLL in `BepInEx/plugins`.

## Custom Shader Material Mode

For full-sky coverage, use the files in `UnityAssets`:

- `NightSkyEquirectangular.shader`
- `BuildNightSkyBundle.cs`

Copy them into the Unity project like this:

```text
Assets/NightSkyEquirectangular.shader
Assets/Editor/BuildNightSkyBundle.cs
Assets/NightSkyTexture.png
```

Then in Unity:

1. Select `Assets/NightSkyTexture.png`.
2. Use these import settings:
   - `Texture Type`: `Default`
   - `Texture Shape`: `2D`
   - `sRGB`: enabled
   - `Alpha Source`: none if available, otherwise use a PNG without alpha
   - `Generate Mipmaps`: off for first testing
   - `Wrap Mode`: repeat
   - `Filter Mode`: trilinear
   - `Max Size`: `8192`
   - `Compression`: none
3. Click `Tools > Night Sky > Create Material`.
4. Click `Tools > Night Sky > Build Bundle`.
5. Copy `AssetBundles/nightsky.bundle` next to the plugin DLL.

The helper creates:

```text
Assets/NightSkyMaterial.mat
```

and assigns the texture, shader, and material to the `nightsky.bundle` asset bundle.

For this mode, set the config to:

```text
Texture.MaterialAssetName = assets/nightskymaterial.mat
```

## Config

After first launch, BepInEx creates:

```text
BepInEx/config/com.fiodor.betternights.cfg
```

Config keys:

- `General.Enabled`
- `Texture.BundleFileName`
- `Texture.TextureAssetName`
- `Texture.BackgroundBundleFileName`
- `Texture.BackgroundTextureAssetName`
- `Texture.TexturePropertyName`
- `Stars.Brightness`
- `Stars.BackgroundBrightness`
- `Stars.Tiling`
- `Stars.Saturation`
- `Material.HorizontalScale`
- `Material.VerticalScale`
- `Material.BackgroundHorizontalScale`
- `Material.BackgroundVerticalScale`
- `Material.HorizontalOffsetDegrees`
- `Material.VerticalOffsetDegrees`
- `Material.BackgroundHorizontalOffsetDegrees`
- `Material.BackgroundVerticalOffsetDegrees`
- `Material.MainBandEnabled`
- `Material.MainBandCenterU`
- `Material.MainBandCenterV`
- `Material.MainBandWidth`
- `Material.MainBandHeight`
- `Material.MainClampToTransparent`
- `Material.MainVerticalFade`
- `Sky.Fade Time Multiplier`
- `Sky.Horizon Fade Start Degrees`
- `Sky.Horizon Fade End Degrees`
- `Sky.Horizon Brightness Multiplier`
- `Debug.Show Horizon Fade Zone`
- `Material.YawDegrees`
- `Material.PitchDegrees`
- `Material.RollDegrees`

Default texture property is `_MainTex`.

`Texture.TextureAssetName` and `Texture.BackgroundTextureAssetName` use the texture assets found in their configured bundles as selectable values. Changing either setting in-game reloads the textures and reapplies the material.

The material settings are reapplied every second, so they can be changed in-game with BepInEx Configuration Manager:

- `General.Enabled`: enables the replacement. Disable to restore vanilla TOD space rendering at runtime.
- `Stars.Brightness`: shader brightness.
- `Stars.Saturation`: shader-side main texture saturation; does not require `Read/Write Enabled`.
- `Stars.BackgroundBrightness`: brightness of the optional secondary background star texture.
- `Material.HorizontalScale`: higher values make horizontal features smaller/repeated.
- `Material.VerticalScale`: higher values make vertical features smaller.
- `Material.BackgroundHorizontalScale`: background star horizontal scale.
- `Material.BackgroundVerticalScale`: background star vertical scale.
- `Material.HorizontalOffsetDegrees`: pans the texture left/right.
- `Material.VerticalOffsetDegrees`: pans the texture up/down.
- `Material.BackgroundHorizontalOffsetDegrees`: pans the background stars left/right.
- `Material.BackgroundVerticalOffsetDegrees`: pans the background stars up/down.
- `Material.MainBandEnabled`: uses normalized sky coverage for the main texture.
- `Material.MainBandCenterU`: horizontal center of the main texture band.
- `Material.MainBandCenterV`: vertical center of the main texture band.
- `Material.MainBandWidth`: horizontal coverage. `1` is full 360 degrees, `0.5` is half the horizon.
- `Material.MainBandHeight`: vertical coverage. `0.5` is half the vertical skydome.
- `Material.MainClampToTransparent`: makes the main texture transparent outside its vertical UV range.
- `Material.MainVerticalFade`: softens the top/bottom edge of the main texture band.
- `Sky.Fade Time Multiplier`: multiplies TOD `LerpValue` before fading. Higher values make the custom sky disappear sooner at sunrise.
- `Sky.Horizon Fade Start Degrees`: sky altitude where horizon dimming is strongest.
- `Sky.Horizon Fade End Degrees`: sky altitude where horizon dimming reaches full brightness.
- `Sky.Horizon Brightness Multiplier`: brightness multiplier at and below the horizon fade start.
- `Debug.Show Horizon Fade Zone`: tints the area affected by horizon fade in pink.
- `Material.YawDegrees`: rotates the whole sky around the vertical axis.
- `Material.PitchDegrees`: tilts the sky forward/backward.
- `Material.RollDegrees`: rolls the sky.

## Optional Background Stars

The shader can draw a secondary star texture behind the main panorama. This is useful when the main panorama has transparent/empty bands.

Put a sparse star texture in the same bundle or a second bundle, then set:

```text
Texture.BackgroundBundleFileName = nightsky.bundle
Texture.BackgroundTextureAssetName = assets/backgroundstars.png
Stars.BackgroundBrightness = 0.2
```

Set `Texture.BackgroundTextureAssetName` empty to disable this layer.

The background appears only where the main night-sky texture alpha is transparent:

```text
main alpha 1 = main texture visible
main alpha 0 = background stars visible
```

For this to work, export the main texture with an alpha channel and set the transparent areas to alpha 0. In Unity, keep `Alpha Source = Input Texture Alpha`. Do not enable `Alpha Is Transparency`; this is a color-edge helper for sprites/UI and can alter the transparent edge colors.

Do not use a huge mostly-transparent PNG to position the band. The bundle may stay small, but the loaded texture still costs memory for every pixel. For example, `16384x8192` RGBA32 is about 512 MB before mipmaps. Prefer an `8192x4096` or smaller useful-content texture and let `Material.MainClampToTransparent` plus `Material.MainVerticalFade` simulate the transparent padding in shader UV space.

For a main texture that always covers the same half of the skydome regardless of source resolution, use:

```text
Material.MainBandEnabled = true
Material.MainBandCenterU = 0.5
Material.MainBandCenterV = 0.5
Material.MainBandWidth = 1
Material.MainBandHeight = 0.5
Material.MainVerticalFade = 0.02
```

With this mode, texture resolution changes sharpness only; it does not change how much sky the main texture covers.

Unity import recommendations:

- Prefer `Max Size = 8192` unless a larger texture is truly needed.
- Disable `Read/Write Enabled`; saturation is handled by the shader.
- Use BC7 or DXT5/BC3 if alpha is needed and compression quality is acceptable.
- Test mipmaps off for star textures if stars become too blurry or large.
- Avoid `16384x8192` mostly-transparent textures.

The main panorama and background stars both fade with fog and modified TOD night visibility:

```text
(1 - Atmosphere.Fogginess) * (1 - LerpValue * Sky.Fade Time Multiplier)
```

## Troubleshooting

If the log says the configured texture asset was not found, the plugin prints every asset name inside the bundle. Copy the matching name into:

```text
Texture.TextureAssetName
```

Unity often stores bundle asset names as project paths, for example:

```text
assets/nightskytexture.psd
```

## Notes

Weather can hide the result. The decompiled TOD code multiplies space brightness by fog and time-of-day state:

```csharp
Stars.Brightness * (1 - Atmosphere.Fogginess) * (1 - LerpValue)
```
