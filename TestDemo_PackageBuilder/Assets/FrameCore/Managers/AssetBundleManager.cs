using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static AssetBundleUpdateVersion;

public class AssetBundleManager : Singletion<AssetBundleManager>, IClassObjectPoolDispose
{
    Dictionary<uint, ResourceItem> m_resourceItemDict = new Dictionary<uint, ResourceItem>();
    Dictionary<uint, AssetBundleItem> m_assetBundlesDict = new Dictionary<uint, AssetBundleItem>();
    protected ClassObjectPool<AssetBundleItem> m_assetBundleItemPool = ClassObjectPoolManager.Instance.GetOrCreateObjectPool<AssetBundleItem>(500);
    protected ClassObjectPool<ResourceItem> m_resourceItemPool = ClassObjectPoolManager.Instance.GetOrCreateObjectPool<ResourceItem>(500);
    AssetBundleUpdateVersion m_assetBundleUpdateVersion;
    MonoBehaviour mono;
    public AssetBundleUpdateVersionEndCallBack abUpdateVersionEndCallBack;
    public void Init(MonoBehaviour mono)
    {
        this.mono = mono;
        m_assetBundleUpdateVersion = new AssetBundleUpdateVersion();
    }

    public void CheckAssetBundleVersion(AssetBundleUpdateVersionEndCallBack abUpdateVersionEndCallBack)
    {
        this.abUpdateVersionEndCallBack = abUpdateVersionEndCallBack;
        m_assetBundleUpdateVersion.CheckAssetBundleConfigIfChange(mono, LoadResourceItemsByConfigData);
    }


    private void LoadResourceItemsByConfigData()
    {
        var configData = m_assetBundleUpdateVersion.CurABConfigData;
        for (int i = 0; i < configData.AssetBundleLists.Count; i++)
        {
            AssetBundleBase abBase = configData.AssetBundleLists[i];
            ResourceItem item = m_resourceItemPool.Spawn(true);
            item.m_Crc = abBase.Crc;
            item.m_ABName = abBase.ABName;
            item.m_AssetName = abBase.AssetName;
            item.m_Dependcies = abBase.ABDependcies;
            if (m_resourceItemDict.ContainsKey(item.m_Crc))
            {
                DebugLogger.Instance.DebugError("重复的Crc 资源名:" + item.m_AssetBundle + "   包名：" + item.m_ABName);
            }
            else
            {
                m_resourceItemDict.Add(item.m_Crc, item);
            }
        }
        
        abUpdateVersionEndCallBack?.Invoke();
        
    }

    public ResourceItem LoadResourceAssetBundle(uint crc)
    {
        if (!m_resourceItemDict.TryGetValue(crc, out var item))
        {
            DebugLogger.Instance.DebugError($"LoadResourceAssetBundle error: not find crc {crc} in AssetBundleConfig");
            return item;
        }

        if (item.m_Dependcies != null)
        {
            for (int i = 0; i < item.m_Dependcies.Count; i++)
            {
                LoadAssetBundle(item.m_Dependcies[i]);
            }
        }
        item.m_AssetBundle = LoadAssetBundle(item.m_ABName);
        return item;
    }

    public AssetBundle LoadAssetBundle(string abName)
    {
        AssetBundleItem item = null;
        uint crc = CRC32.CRC32Cls.GetCRC32Str(abName);
        if (!m_assetBundlesDict.TryGetValue(crc, out item))
        {
            AssetBundle bundle = null;
            string fullPath = $"{Application.persistentDataPath}/{abName}";
            if (File.Exists(fullPath))
            {
                bundle = AssetBundle.LoadFromFile(fullPath);
            }
            if (bundle == null)
            {
                DebugLogger.Instance.DebugError($"LoadAssetBundle error: load file {abName} from {fullPath}");
            }

            item = m_assetBundleItemPool.Spawn(true);
            item.assetBundle = bundle; ;
            item.RefCount++;
            m_assetBundlesDict.Add(crc, item);
        }
        else
        {
            item.RefCount++;
        }

        return item.assetBundle;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="item"></param>
    public void ReleaseAsset(ResourceItem item)
    {
        if (item == null)
            return;
        if (item.m_Dependcies?.Count > 0)
        {
            for (int i = 0; i < item.m_Dependcies.Count; i++)
            {
                UnloadAssetBundle(item.m_Dependcies[i]);
            }
        }

        UnloadAssetBundle(item.m_ABName);
    }

    private void UnloadAssetBundle(string abName)
    {
        AssetBundleItem item = null;
        uint crc = CRC32.CRC32Cls.GetCRC32Str(abName);
        if (m_assetBundlesDict.TryGetValue(crc, out item) && item != null)
        {
            item.RefCount--;
            if (item.RefCount <= 0 && item.assetBundle != null)
            {
                item.assetBundle.Unload(true);
                item.Reset();
                m_assetBundleItemPool.Release(item);
                m_assetBundlesDict.Remove(crc);
            }
        }
    }

    /// <summary>
    /// 根据crc查找ResourceItem
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    public ResourceItem FindResourceItem(uint crc)
    {
        if (m_resourceItemDict.TryGetValue(crc, out ResourceItem item))
            return item;
        return null;
    }

    public void Clear(bool force)
    {
        m_assetBundleItemPool?.Clear(force);
        m_resourceItemPool.Clear(force);
    }
}



