using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPackagePlatformSettings
{
    string AssetBundleConfigSettingsPath { get; }
    string AssetBundleBuildABTargetPath { get; }
    string AssetBundleBuildAppTargetPath { get; }
}
