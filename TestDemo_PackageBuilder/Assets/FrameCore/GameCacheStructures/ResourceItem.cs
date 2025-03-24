using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectItem
{
    public uint m_Crc = 0;
    public GameObject m_CloneObj = null;
    public bool m_ClearSwitchScene = false; //是否跳场景清除
    public long m_Guid = 0; //存储GUID
    public ResourceItem m_ResourceItem = null; 
    public bool m_IsReturn = false; //是否已经放回对象池
    public bool m_SetSceneParent = false; //是否放到场景节点下面
    public OnAsyncLoadObjFinished m_DealFinish = null;
    public object param1 = null, param2=null, param3=null;


    public void Reset()
    {
        m_Crc = 0;
        m_CloneObj = null;
        m_ClearSwitchScene = false;
        m_Guid = 0;
        m_ResourceItem = null;
        m_IsReturn = false;
        m_SetSceneParent = false;
        m_DealFinish = null;
        param1 = null;
        param2 = null;
        param3 = null;
    }
}

public class ResourceItem
{
    public uint m_Crc;
    public string m_ABName;
    public string m_AssetName;
    public List<string> m_Dependcies;
    //-------------------------------------------------- 

    public AssetBundle m_AssetBundle; //AB包
    public Object m_Obj; //资源对象
    private int m_RefCount;
    public int RefCount
    {
        get { return m_RefCount; }
        set
        {
            m_RefCount = value;
            if (m_RefCount < 0)
            {
                DebugLogger.Instance.DebugLog("AB包计数引用小于0！");
            }
        }
    }
    public float m_LastUseTime;
    public int m_Guid;
    public bool m_ClearSwitchScene;
}


public class AssetBundleItem
{
    public AssetBundle assetBundle = null;
    public int RefCount;

    public void Reset()
    {
        assetBundle = null;
        RefCount = 0;
    }
}