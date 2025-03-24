using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class OpenPathsHelper
{
    [MenuItem("Tools/Paths/����AB��·��")]
    public static void OpenBuildABTargetPath()
    {
        PackagePlatformSettings data = AssetDatabase.LoadAssetAtPath<PackagePlatformSettings>(EditorConstParm.ASSET_BUNDLE_GLOBAL_SETTINGS);

        string path = $"{Application.dataPath}{data.AssetBundleBuildABTargetPath}{EditorUserBuildSettings.activeBuildTarget}/";
#if UNITY_ANDORID
        ASSET_BUNDLE_LOCAL_SIMULATE_URL ="jar:file://"+path;
#endif

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        EditorUtility.RevealInFinder(path);
    }

    [MenuItem("Tools/Paths/���ػ���·��")]
    public static void OpenPersistentDataPath()
    {

        string path = $"{Application.persistentDataPath}/";
#if UNITY_ANDORID
        ASSET_BUNDLE_LOCAL_SIMULATE_URL ="jar:file://"+path;
#endif

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        EditorUtility.RevealInFinder(path);

    }

    [MenuItem("Tools/Paths/���ش��·��")]
    public static void OpenBuildAppTargetPath()
    {
        PackagePlatformSettings data = AssetDatabase.LoadAssetAtPath<PackagePlatformSettings>(EditorConstParm.ASSET_BUNDLE_GLOBAL_SETTINGS);

        string path = $"{Application.dataPath}{data.AssetBundleBuildAppTargetPath}{EditorUserBuildSettings.activeBuildTarget}/";
        Debug.Log(path);
#if UNITY_ANDORID
        ASSET_BUNDLE_LOCAL_SIMULATE_URL ="jar:file://"+path;
#endif
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        EditorUtility.RevealInFinder(path);

    }
}
