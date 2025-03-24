using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class GameObjectManager : Singletion<GameObjectManager>, IClassObjectPoolDispose
{
    public Transform m_RecyclePoolTrans;
    public Transform m_SceneTrans;
    protected Dictionary<uint, List<ObjectItem>> m_ObjectPoolDict = new Dictionary<uint, List<ObjectItem>>();
    protected ClassObjectPool<ObjectItem> m_ObjectItemPool = ClassObjectPoolManager.Instance.GetOrCreateObjectPool<ObjectItem>(1000);

    //根据异步的guid存储ObjectItem，判断是否在异步队列
    protected Dictionary<long, ObjectItem> m_AysncObjectItem = new Dictionary<long, ObjectItem>();
    protected Dictionary<int, ObjectItem> m_ResourceObjectDict = new Dictionary<int, ObjectItem>();
    public void Init(Transform recycler, Transform sceneTrans)
    {
        m_RecyclePoolTrans = recycler;
        m_SceneTrans = sceneTrans;
    }


    protected ObjectItem GetObjectFromPool(uint crc)
    {
        List<ObjectItem> list = null;
        if (m_ObjectPoolDict.TryGetValue(crc, out list) && list.Count > 0)
        {
            ResourceManager.Instance.IncreaseResourceRefCount(crc);
            ObjectItem objItem = list[0];
            list.RemoveAt(0);
            GameObject obj = objItem.m_CloneObj;
            if (!System.Object.ReferenceEquals(obj, null))
            {
#if UNITY_EDITOR
                if (obj.name.EndsWith("(Recycle)"))
                {
                    obj.name = obj.name.Replace("(Recycle)", "");
                }
#endif
            }
            return objItem;
        }
        return null;
    }
    public GameObject InstantiateObjects(string path, bool bSetSceneObj, bool bClear = true)
    {
        uint crc = CRC32.CRC32Cls.GetCRC32Str(path);
        ObjectItem objItem = GetObjectFromPool(crc);
        if (objItem == null)
        {
            objItem = m_ObjectItemPool.Spawn(true);
            objItem.m_Crc = crc;
            objItem.m_ClearSwitchScene = bClear;
            objItem = ResourceManager.Instance.LoadResource(path, objItem);
            if (objItem.m_ResourceItem.m_Obj != null)
            {
                objItem.m_CloneObj = GameObject.Instantiate(objItem.m_ResourceItem.m_Obj) as GameObject;
            }
        }

        if (bSetSceneObj)
        {
            objItem.m_CloneObj.transform.SetParent(m_SceneTrans, false);
        }

        objItem.m_IsReturn = false;
        int tempID = objItem.m_CloneObj.GetInstanceID();
        if (!m_ResourceObjectDict.ContainsKey(tempID))
        {
            m_ResourceObjectDict.Add(tempID, objItem);
        }
        return objItem.m_CloneObj;

    }

    /// <summary>
    /// 异步对象加载
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="maxCacheCount"></param>
    /// <param name="destoryCache"></param>
    /// <param name="recycleParent"></param>
    public void ReleaseObject(GameObject obj, int maxCacheCount = -1, bool destoryCache = false, bool recycleParent = true)
    {
        if (obj == null)
            return;

        ObjectItem objItem = null;
        int tempID = obj.GetInstanceID();
        if (!m_ResourceObjectDict.TryGetValue(tempID, out objItem))
        {
            DebugLogger.Instance.DebugError("对象不是GameObjectManager创建");
            return;
        }

        if (objItem == null)
        {
            DebugLogger.Instance.DebugError("缓存的ObjectItem为空！");
            return;
        }

        if (objItem.m_IsReturn)
        {
            DebugLogger.Instance.DebugError("该对象已经放回对象池，检测是否有额外引用");
            return;
        }

#if UNITY_EDITOR
        obj.name += "(Recycle)";
#endif

        List<ObjectItem> list = null;
        if (maxCacheCount == 0)
        {
            m_ResourceObjectDict.Remove(tempID);
            ResourceManager.Instance.ReleaseResource(objItem, destoryCache);
            objItem.Reset();
            m_ObjectItemPool.Release(objItem);
        }
        else //回收到对象池
        {
            if (!m_ObjectPoolDict.TryGetValue(objItem.m_Crc, out list) || list == null)
            {
                list = new List<ObjectItem>();
                m_ObjectPoolDict.Add(objItem.m_Crc, list);
            }

            if (objItem.m_CloneObj)
            {
                if (recycleParent)
                {
                    objItem.m_CloneObj.transform.SetParent(m_RecyclePoolTrans);
                }
                else
                {
                    objItem.m_CloneObj.SetActive(false);
                }
            }

            if (maxCacheCount < 0 || list.Count < maxCacheCount)
            {
                list.Add(objItem);
                objItem.m_IsReturn = true;
                ResourceManager.Instance.DecreaseResourceRefCount(objItem);
            }
            else
            {
                m_ResourceObjectDict.Remove(tempID);
                ResourceManager.Instance.ReleaseResource(objItem, destoryCache);
                objItem.Reset();
                m_ObjectItemPool.Release(objItem);

            }
        }

    }

    public long AsyncInstantiateObjects<T>(string path, OnAsyncLoadObjFinished dealFinish, LoadResPriority priority, bool bSetSceneObj = false
            , bool bClear = true, object param1 = null, object param2 = null, object param3 = null) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(path))
        {
            return 0;
        }
        uint crc = CRC32.CRC32Cls.GetCRC32Str(path);
        ObjectItem objItem = GetObjectFromPool(crc);
        if (objItem != null)
        {
            if (bSetSceneObj)
            {
                objItem.m_CloneObj.transform.SetParent(m_SceneTrans, false);
            }

            if (dealFinish != null)
            {
                dealFinish(path, objItem.m_CloneObj, param1, param2, param3);
            }
            return objItem.m_Guid;
        }

        long guid = ResourceManager.Instance.GetGuid();
        objItem = m_ObjectItemPool.Spawn(true);
        objItem.m_Crc = crc;
        objItem.m_SetSceneParent = bSetSceneObj;
        objItem.m_ClearSwitchScene = bClear;
        objItem.m_DealFinish = dealFinish;
        objItem.param1 = param1;
        objItem.param2 = param2;
        objItem.param3 = param3;
        objItem.m_Guid = guid;
        m_AysncObjectItem.Add(guid, objItem);
        ResourceManager.Instance.AsyncLoadResource<T>(path, objItem, OnLoadResourceObjFinish, priority);
        return guid;
    }

    private void OnLoadResourceObjFinish(string path, ObjectItem obj
            , object parms1 = null, object parms2 = null, object parms3 = null)
    {
        if (obj == null)
            return;
        if (obj.m_ResourceItem.m_Obj == null)
        {
#if UNITY_EDITOR
            DebugLogger.Instance.DebugError("异步加载的资源为空：" + path);
#endif
        }
        else
        {
            obj.m_CloneObj = GameObject.Instantiate(obj.m_ResourceItem.m_Obj) as GameObject;
        }

        if (m_AysncObjectItem.ContainsKey(obj.m_Guid))
        {
            m_AysncObjectItem.Remove(obj.m_Guid);
        }

        if (obj.m_CloneObj != null && obj.m_SetSceneParent)
        {
            obj.m_CloneObj.transform.SetParent(m_SceneTrans, false);
        }

        if (obj.m_DealFinish != null)
        {
            int tempID = obj.m_CloneObj.GetInstanceID();
            if (!m_ResourceObjectDict.ContainsKey(tempID))
            {
                m_ResourceObjectDict.Add(tempID, obj);
            }

            obj.m_DealFinish(path, obj.m_CloneObj, parms1, parms2, parms3);
        }
    }

    public void PreloadGameObejct(string path, int count = 1, bool clear = false)
    {
        List<GameObject> tempGOList = new List<GameObject>();
        for (int i = 0; i < count; ++i)
        {
            GameObject go = InstantiateObjects(path, false, clear);
            tempGOList.Add(go);
        }

        for (int i = 0; i < count; ++i)
        {
            GameObject go = tempGOList[i];
            ReleaseObject(go);
            go = null;
        }
        tempGOList.Clear();
    }

    public void CancelAsyncLoadObject(long guid)
    {
        ObjectItem item = null;
        if (m_AysncObjectItem.TryGetValue(guid, out item) && ResourceManager.Instance.CancelAsyncLoad(item))
        {
            m_AysncObjectItem.Remove(guid);
            item.Reset();
            m_ObjectItemPool.Release(item);
        }
    }

    /// <summary>
    /// 是否正在异步加载
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public bool IsWaitAsyncLoading(long guid)
    {
        return m_AysncObjectItem.ContainsKey(guid) && m_AysncObjectItem[guid] != null;
    }

    public bool IsObjectManagerCreate(GameObject obj)
    {
        ObjectItem objItem = null;
        return m_ResourceObjectDict.TryGetValue(obj.GetInstanceID(), out objItem) && objItem != null;
    }

    /// <summary>
    /// 清空对象池
    /// </summary>
    public void Clear(bool force)
    {
        List<uint> tempList = new List<uint>();
        foreach (uint key in m_ObjectPoolDict.Keys)
        {
            List<ObjectItem> objectItems = m_ObjectPoolDict[key];
            for (int i = 0; i < objectItems.Count; i++)
            {
                ObjectItem item = objectItems[i];
                if (!System.Object.ReferenceEquals(item.m_CloneObj, null) && item.m_ClearSwitchScene)
                {
                    GameObject.Destroy(item.m_CloneObj);
                    m_ResourceObjectDict.Remove(item.m_CloneObj.GetInstanceID());
                    item.Reset();
                    m_ObjectItemPool.Release(item);
                }
            }

            if (objectItems.Count <= 0)
            {
                tempList.Add(key);
            }
        }

        for (int i = 0; i < tempList.Count; i++)
        {
            uint temp = tempList[i];
            if (m_ObjectPoolDict.ContainsKey(temp))
            {
                m_ObjectPoolDict.Remove(temp);
            }
        }
        tempList.Clear();
        m_ObjectItemPool.Clear(force);
    }
}
