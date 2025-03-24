using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


/// <summary>
/// AB版本对比加载模块
/// </summary>
public class AssetBundleUpdateVersion
{
    MonoBehaviour mono;
    AssetBundleConfigData m_localConfigData;
    AssetBundleConfigData m_remoteConfigData;
    List<AssetBundleBase> m_needUpdateResources = new List<AssetBundleBase>();
    public delegate void AssetBundleUpdateVersionEndCallBack();
    AssetBundleUpdateVersionEndCallBack m_updateVersionCallBack;
    List<uint> abCrc = new List<uint>();

    public AssetBundleConfigData CurABConfigData
    {
        get { return m_localConfigData; }
    }


    public void CheckAssetBundleConfigIfChange(MonoBehaviour mono, AssetBundleUpdateVersionEndCallBack updateVersionCallBack)
    {
        this.mono = mono;
        this.m_updateVersionCallBack = updateVersionCallBack;
        mono.StartCoroutine(CheckAssetBundleConfigIfChange());
    }

    private IEnumerator CheckAssetBundleConfigIfChange()
    {
        //加载本地配置清单
        yield return LoadLocalAssetBundleConfig();
        //加载远端配置清单
        yield return LoadRemoteAssetBundleConfig();
        //对比更新
        CompareAssetBundleConfig();
        if (m_needUpdateResources.Count > 0)
        {
            DebugLogger.Instance.DebugLog($"发现{m_needUpdateResources.Count}个资源需要更新");
            yield return mono.StartCoroutine(DownloadABUpdates());
        }
        else
        {
            DebugLogger.Instance.DebugLog("资源已是最新版本");
        }

        m_updateVersionCallBack?.Invoke();
    }

    private IEnumerator LoadLocalAssetBundleConfig()
    {
        string path = Path.Combine(Application.persistentDataPath, $"{CommonConstParm.ASSET_BUNDLE_CONFIG_NAME}.bytes");
        if (File.Exists(path))
        {
            m_localConfigData = LoadFileUnility.LoadDataAssets<AssetBundleConfigData>($"{Application.persistentDataPath}/{CommonConstParm.ASSET_BUNDLE_CONFIG_NAME}.bytes");
        }
        else
        {
            m_localConfigData = new AssetBundleConfigData() { Version = "0.0.0" };
        }
        yield return m_localConfigData;
    }

    private IEnumerator LoadRemoteAssetBundleConfig()
    {
        IEnumerator<AssetBundle> loader = new WaitForABLoadFromWeb($"file:///{CommonConstParm.SimulateNetPath}/{CommonConstParm.ASSET_BUNDLE_CONFIG_NAME}", $"{Application.persistentDataPath}/{CommonConstParm.ASSET_BUNDLE_CONFIG_NAME}");
        yield return loader;
        if (loader.Current == null)
        {
            DebugLogger.Instance.DebugError($"资源配置包assetbundleconfig加载失败");
            yield break;
        }

        AssetBundle bundle = loader.Current;
        if (bundle == null)
            yield break;
        TextAsset remoteAsset = bundle.LoadAsset<TextAsset>(CommonConstParm.ASSET_BUNDLE_CONFIG_NAME + ".bytes");
        m_remoteConfigData = ProtobufSerializer.Deserialize<AssetBundleConfigData>(remoteAsset.bytes);
    }

    /// <summary>
    /// 对比更新
    /// </summary>
    /// <returns></returns>
    private void CompareAssetBundleConfig()
    {
        m_needUpdateResources.Clear();
        //TODO:判断本地AB配置与远端AB配置对比
        if (m_localConfigData?.Version == m_remoteConfigData?.Version) return;

        foreach (var remoteABBase in m_remoteConfigData.AssetBundleLists)
        {
            AssetBundleBase localABBase = m_localConfigData.AssetBundleLists.Find(e => e.ABName == remoteABBase.ABName);
            if (localABBase == null || localABBase.Crc != remoteABBase.Crc)
            {
                m_needUpdateResources.Add(remoteABBase);
            }
        }
        m_localConfigData = m_remoteConfigData;
        SaveLocalAssetBundleConfig();
    }

    /// <summary>
    /// 本地配置表更新
    /// </summary>
    private void SaveLocalAssetBundleConfig()
    {
        string path = Path.Combine(Application.persistentDataPath, $"{CommonConstParm.ASSET_BUNDLE_CONFIG_NAME}.bytes");
        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
        {
            fs.Position = 0;
            byte[] bytes = ProtobufSerializer.Serialize(m_remoteConfigData);
            fs.Write(bytes, 0, bytes.Length);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private IEnumerator DownloadABUpdates()
    {
        abCrc.Clear();

        IEnumerator<AssetBundle> ienum = null;
        foreach (AssetBundleBase abBase in m_needUpdateResources)
        {
            uint crc = 0;
            for (int i = 0; i < abBase?.ABDependcies?.Count; ++i)
            {
                crc = CRC32.CRC32Cls.GetCRC32Str(abBase?.ABDependcies[i]);
                if (abCrc.Contains(crc)) continue;
                ienum = new WaitForABLoadFromWeb($"{CommonConstParm.SimulateNetPath}/{abBase?.ABDependcies[i]}", $"{CommonConstParm.SimulateLocalPath}/{abBase?.ABDependcies[i]}");
                yield return ienum;
                if (ienum?.Current != null)
                {
                    abCrc.Add(crc);
                }
            }

            crc = CRC32.CRC32Cls.GetCRC32Str(abBase.ABName);
            if (abCrc.Contains(crc)) continue;
            ienum = new WaitForABLoadFromWeb($"{CommonConstParm.SimulateNetPath}/{abBase.ABName}", $"{CommonConstParm.SimulateLocalPath}/{abBase.ABName}");
            yield return ienum;
            if (ienum?.Current != null)
            {
                abCrc.Add(crc);
            }
        }
    }


}
