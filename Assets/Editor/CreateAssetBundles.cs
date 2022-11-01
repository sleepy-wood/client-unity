using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/StreamingAssets";
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
#if UNITY_STANDALONE
                BuildPipeline.BuildAssetBundles(
                      assetBundleDirectory,
                      BuildAssetBundleOptions.None,
                     EditorUserBuildSettings.activeBuildTarget);
#elif UNITY_ANDROID
                BuildPipeline.BuildAssetBundles(
                        assetBundleDirectory,
                        BuildAssetBundleOptions.None,
                        BuildTarget.Android);
#elif UNITY_IOS
                BuildPipeline.BuildAssetBundles(
                        assetBundleDirectory,
                        BuildAssetBundleOptions.None,
                        EditorUserBuildSettings.activeBuildTarget);
#endif
    }
}