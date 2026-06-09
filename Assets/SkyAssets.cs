using UnityEngine;

namespace BetterNightSkies;

internal sealed class SkyAssets
{
    internal Texture2D SourceNightSkyTexture;
    internal Cubemap BackgroundCubemap;
    internal Material ReplacementMaterial;

    internal bool HasReplacement
        => SourceNightSkyTexture != null || ReplacementMaterial != null;
}
