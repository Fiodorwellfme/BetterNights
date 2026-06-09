using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildNightSkyBundle
{
    private const string OutputPath = "AssetBundles";
    private const string BackgroundCubemapPath = "Assets/BackgroundStars_CubemapStrip_PosX_NegX_PosY_NegY_PosZ_NegZ.png";
    private const string ShaderPath = "Assets/NightSkyEquirectangular.shader";
    private const string MaterialPath = "Assets/NightSkyMaterial.mat";
    private const string BundleName = "nightsky.bundle";

    [MenuItem("Tools/Night Sky/Create Material")]
    public static void CreateMaterial()
    {
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
        Cubemap backgroundCubemap = AssetDatabase.LoadAssetAtPath<Cubemap>(BackgroundCubemapPath);
        Material material = new Material(shader)
        {
            name = "NightSkyMaterial"
        };

        material.SetTexture("_BackgroundCube", backgroundCubemap);
        material.SetFloat("_Brightness", 0.5f);
        material.SetFloat("_Saturation", 2.5f);
        material.SetFloat("_BackgroundBrightness", 3f);
        material.SetFloat("_BackgroundSaturation", 1f);
        material.SetFloat("_TodVisibility", 1f);
        material.SetFloat("_HorizonFadeStartDegrees", 0f);
        material.SetFloat("_HorizonFadeEndDegrees", 25f);
        material.SetFloat("_HorizonBrightnessMultiplier", 0.25f);
        material.SetFloat("_HorizonFadeDebug", 0f);
        material.SetFloat("_HorizontalScale", 1f);
        material.SetFloat("_VerticalScale", 1f);
        material.SetFloat("_BackgroundHorizontalScale", 3f);
        material.SetFloat("_BackgroundVerticalScale", 3f);
        material.SetFloat("_HorizontalOffsetDegrees", 0f);
        material.SetFloat("_VerticalOffsetDegrees", 0f);
        material.SetFloat("_BackgroundHorizontalOffsetDegrees", 0f);
        material.SetFloat("_BackgroundVerticalOffsetDegrees", 0f);
        material.SetFloat("_MainBandEnabled", 1f);
        material.SetFloat("_MainBandCenterU", 0.5f);
        material.SetFloat("_MainBandCenterV", 0.5f);
        material.SetFloat("_MainBandWidth", 1f);
        material.SetFloat("_MainBandHeight", 0.3f);
        material.SetFloat("_MainClampToTransparent", 1f);
        material.SetFloat("_MainHorizontalFade", 0.05f);
        material.SetFloat("_MainVerticalFade", 0.5f);
        material.SetFloat("_YawDegrees", 180f);
        material.SetFloat("_PitchDegrees", 0f);
        material.SetFloat("_RollDegrees", 0f);

        AssetDatabase.CreateAsset(material, MaterialPath);
        SetBundleName(BackgroundCubemapPath);
        SetBundleName(ShaderPath);
        SetBundleName(MaterialPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Night Sky/Build Bundle")]
    public static void Build()
    {
        Directory.CreateDirectory(OutputPath);

        BuildPipeline.BuildAssetBundles(
            OutputPath,
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows64);
    }

    private static void SetBundleName(string assetPath)
    {
        AssetImporter importer = AssetImporter.GetAtPath(assetPath);
        if (importer != null)
        {
            importer.assetBundleName = BundleName;
        }
    }
}
