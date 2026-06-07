using UnityEngine;

namespace BetterNights;

internal sealed class SkyAssets
{
    internal Texture2D SourceNightSkyTexture;
    internal Texture2D BackgroundTexture;
    internal Material ReplacementMaterial;

    internal bool HasReplacement
        => SourceNightSkyTexture != null || ReplacementMaterial != null;
}
