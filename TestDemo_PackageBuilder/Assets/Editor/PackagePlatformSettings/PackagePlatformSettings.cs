using UnityEngine;


[CreateAssetMenu(fileName = "PackagePlatformSettings", menuName = "PackageBuilder/PackagePlatformSettings")]
public class PackagePlatformSettings : ScriptableObject, IPackagePlatformSettings
{
    [SerializeField]
    private string m_AssetBundleConfigSettingsPath;
    [SerializeField]
    private string m_AssetBundleBuildABTargetPath;
    [SerializeField]
    private string m_AssetBundleBuildAppTargetPath;

    public string AssetBundleConfigSettingsPath { get { return m_AssetBundleConfigSettingsPath; } }
    public string AssetBundleBuildABTargetPath { get { return m_AssetBundleBuildABTargetPath; } }
    public string AssetBundleBuildAppTargetPath { get { return m_AssetBundleBuildAppTargetPath; } }

}

