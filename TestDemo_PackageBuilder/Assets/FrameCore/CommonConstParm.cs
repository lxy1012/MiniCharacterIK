using UnityEditor;
using UnityEngine;


public class CommonConstParm
{

    static CommonConstParm()
    {
#if UNITY_EDITOR
        IPackagePlatformSettings data = AssetDatabase.LoadAssetAtPath<Object>(@"Assets/Editor/GlobalConfig/PackagePlatformSettings.asset") as IPackagePlatformSettings;
        SimulateNetPath = $"{Application.dataPath}{data.AssetBundleBuildABTargetPath}{EditorUserBuildSettings.activeBuildTarget}";
        SimulateLocalPath = $"{Application.persistentDataPath}/";
#elif UNITY_ANDORID              
        SimulateNetPath = "TODO:Company_Server_URL";
        SimulateLocalPath = "jar:file://" + Application.persistentDataPath;
#elif UNITY_IOS
        SimulateNetPath = "TODO:Company_Server_URL";
        SimulateLocalPath = "file:///" + Application.persistentDataPath;
#endif
    }


    //打包地址--模拟网络下载地址
    public static string SimulateNetPath
    {
        get; private set;
    }

    //本地缓存地址
    public static string SimulateLocalPath
    {
        get; private set;
    }

    //资源配置表--单独放置，防止被删除
    public static readonly string ASSET_BUNDLE_CONFIG_ASSET = "GameAssets/ABConfigData";
    public static readonly string ASSET_BUNDLE_CONFIG_CONFIGS = "GameAssets/DataConfigs";
    public static readonly string ASSET_BUNDLE_CONFIG_JSON = "GameAssets/Json";
    public static readonly string ASSET_BUNDLE_CONFIG_NAME = "assetbundleconfig";


}
