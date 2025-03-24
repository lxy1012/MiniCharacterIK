using CRC32;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public enum LoadResPriority
{
    LOAD_HIGH,
    LOAD_NORMAL,
    LOAD_LOW,
    PRIORITY_NUM
}
public class AsyncLoadResParam
{
    public List<AsyncCallBack> m_CallBackList = new List<AsyncCallBack>();
    public uint m_Crc;
    public string m_Path;
    public LoadResPriority m_Priority = LoadResPriority.LOAD_NORMAL;
    public bool m_IsSprite = false;

    public void Reset()
    {
        m_CallBackList.Clear();
        m_Crc = 0;
        m_Path = string.Empty;
        m_Priority = LoadResPriority.LOAD_NORMAL;
        m_IsSprite = false;
    }
}
public class AsyncCallBack
{
    //加载完成的回调
    public OnAsyncLoadObjFinished m_DealFinished = null;
    //回调参数
    public object m_parms1 = null;
    public object m_parms2 = null;
    public object m_parms3 = null;

    //实例化加载完成的回调
    public OnAsyncObjFinished m_ObjDealFinished = null;
    public ObjectItem m_Item = null;


    public void Reset()
    {
        m_parms1 = null;
        m_parms2 = null;
        m_parms3 = null;
        m_DealFinished = null;
        m_ObjDealFinished = null;
        m_Item = null;
    }
}

//资源加载完成回调
public delegate void OnAsyncLoadObjFinished(string path, Object obj,
            object parms1 = null, object parms2 = null, object parms3 = null);
//实例化对象加载完成回调
public delegate void OnAsyncObjFinished(string path, ObjectItem obj,
            object parms1 = null, object parms2 = null, object parms3 = null);

//TODO: AssetBundleMaanger <->ResourceManager <->GameObjectManager为IOC注入模式
public partial class ResourceManager : Singletion<ResourceManager>
{
    protected ClassObjectPool<AsyncLoadResParam> m_AsyncLoadResParamPool = new ClassObjectPool<AsyncLoadResParam>(50);
    protected ClassObjectPool<AsyncCallBack> m_AsyncCallBackPool = new ClassObjectPool<AsyncCallBack>(100);
    //管理缓存引用计数为0的资源，达到缓存最大时释放最早在列表的资源
    protected LRUMapList<ResourceItem> m_NoReferenceAssetMapList = new LRUMapList<ResourceItem>();
    //缓存使用过的资源包
    public Dictionary<uint, ResourceItem> AssetDict { get; set; } = new Dictionary<uint, ResourceItem>();
    //正在异步加载的资源列表
    public List<AsyncLoadResParam>[] m_LoadingAssetList = new List<AsyncLoadResParam>[(int)LoadResPriority.PRIORITY_NUM];
    //正在异步加载的DI
    public Dictionary<uint, AsyncLoadResParam> m_LoadingAssetDict = new Dictionary<uint, AsyncLoadResParam>();

    private MonoBehaviour m_Mono;
    private long lastYieldTime;
    private const long MAX_LOADING_RES_TIME = 200000;  //单位微妙
    private long m_Guid = 0;
    public void Init(MonoBehaviour mono)
    {
        m_Mono = mono;
        for (int i = 0; i < (int)LoadResPriority.PRIORITY_NUM; i++)
        {
            m_LoadingAssetList[i] = new List<AsyncLoadResParam>();
        }
        mono.StartCoroutine(AsyncLoadCoroutine());
    }

    //TODO优化：可以用链表实现异步循环逻辑---避免for循环里删减导致迭代器指针丢失：取消异步
    IEnumerator AsyncLoadCoroutine()
    {
        List<AsyncCallBack> callBacks = null;
        lastYieldTime = System.DateTime.Now.Ticks;
        while (true)
        {
            bool haveYield = false;
            for (int i = 0; i < (int)LoadResPriority.PRIORITY_NUM; i++)
            {
                List<AsyncLoadResParam> loadList = m_LoadingAssetList[i];
                if (loadList.Count <= 0)
                    continue;
                AsyncLoadResParam loadingItem = loadList[0];
                loadList.RemoveAt(0);
                callBacks = loadingItem.m_CallBackList;

                Object obj = null;
                ResourceItem item = null;

#if UNITY_EDITOR
                YieldInstruction yieldInstruction = null;
                LoadResourceInEditor<Object>(loadingItem.m_Path, loadingItem.m_Crc, ref obj, ref item, ref yieldInstruction);
                yield return yieldInstruction;
#endif

                if (obj == null)
                {
                    item = AssetBundleManager.Instance.LoadResourceAssetBundle(loadingItem.m_Crc);
                    if (item != null && item.m_AssetBundle != null)
                    {

                        AssetBundleRequest req = null;
                        if (loadingItem.m_IsSprite)
                        {
                            req = item.m_AssetBundle.LoadAssetAsync<Sprite>(item.m_AssetName);
                        }
                        else
                        {
                            req = item.m_AssetBundle.LoadAssetAsync(item.m_AssetName);
                        }
                        yield return req;
                        if (req.isDone)
                        {
                            obj = req.asset;
                        }
                        lastYieldTime = System.DateTime.Now.Ticks;
                    }
                }


                CacheResource(loadingItem.m_Path, ref item, loadingItem.m_Crc, obj, loadingItem.m_CallBackList.Count);
                for (int j = 0; j < callBacks.Count; j++)
                {
                    AsyncCallBack callBack = callBacks[j];
                    if (callBack != null && callBack.m_ObjDealFinished != null && callBack.m_Item != null)
                    {
                        ObjectItem tempItem = callBack.m_Item;
                        tempItem.m_ResourceItem = item;
                        callBack.m_ObjDealFinished(loadingItem.m_Path, tempItem, callBack.m_parms1, callBack.m_parms2, callBack.m_parms3);
                        callBack.m_ObjDealFinished = null;
                        tempItem = null;
                    }

                    if (callBack != null && callBack.m_DealFinished != null)
                    {
                        callBack.m_DealFinished(loadingItem.m_Path, obj, callBack.m_parms1, callBack.m_parms2, callBack.m_parms3);
                        callBack.m_DealFinished = null;
                    }
                    callBack.Reset();
                    m_AsyncCallBackPool.Release(callBack);
                }

                obj = null;
                callBacks = null;
                m_LoadingAssetDict.Remove(loadingItem.m_Crc);
                loadingItem.Reset();
                m_AsyncLoadResParamPool.Release(loadingItem);

                if (System.DateTime.Now.Ticks - lastYieldTime > MAX_LOADING_RES_TIME)
                {
                    yield return null;
                    lastYieldTime = System.DateTime.Now.Ticks;
                    haveYield = true;
                }

            }

            if (haveYield || System.DateTime.Now.Ticks - lastYieldTime > MAX_LOADING_RES_TIME)
            {
                yield return null;
                lastYieldTime = System.DateTime.Now.Ticks; ;
            }
        }
    }

    public long GetGuid()
    {
        return ++m_Guid;
    }
    /// <summary>
    /// 异步资源加载--不需要实例化的资源
    /// </summary>
    public void AsyncLoadResource<T>(string path, OnAsyncLoadObjFinished dealFinish, LoadResPriority priority, uint crc = 0,
            object parms1 = null, object parms2 = null, object parms3 = null) where T : UnityEngine.Object
    {
        if (crc == 0)
        {
            crc = CRC32.CRC32Cls.GetCRC32Str(path);
        }

        ResourceItem item = GetCacheResourceItem(crc);
        if (item != null)
        {
            dealFinish?.Invoke(path, item.m_Obj, parms1, parms2, parms3);
            return;
        }

        //判断是否在加载中
        AsyncLoadResParam parm = null;
        if (!m_LoadingAssetDict.TryGetValue(crc, out parm) || parm == null)
        {
            parm = m_AsyncLoadResParamPool.Spawn(true);
            parm.m_Crc = crc;
            parm.m_Path = path;
            parm.m_Priority = priority;
            parm.m_IsSprite = typeof(T) == typeof(Sprite);
            m_LoadingAssetDict.Add(crc, parm);
            m_LoadingAssetList[(int)priority].Add(parm);
        }

        AsyncCallBack callBack = m_AsyncCallBackPool.Spawn(true);
        callBack.m_DealFinished = dealFinish;
        callBack.m_parms1 = parms1;
        callBack.m_parms2 = parms2;
        callBack.m_parms3 = parms3;
        parm.m_CallBackList.Add(callBack);
    }


    /// <summary>
    /// 同步资源加载，外部直接通用。加载不需要实例化的资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T LoadResource<T>(string path) where T : UnityEngine.Object
    {
        WashOut();

        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        uint crc = CRC32.CRC32Cls.GetCRC32Str(path);
        ResourceItem item = GetCacheResourceItem(crc);
        if (item != null)
        {
            return item.m_Obj as T;
        }

        T obj = null;

#if UNITY_EDITOR

        LoadResourceInEditor(crc, path, ref obj, ref item);

#endif
        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Obj != null)
                    obj = item.m_Obj as T;
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<T>(item.m_AssetName);
                }
            }
        }

        CacheResource(path, ref item, crc, obj);
        return obj;
    }

    public bool ReleaseResource(string path, bool destroyObj = false)
    {
        if (string.IsNullOrEmpty(path))
            return false;
        uint crc = CRC32Cls.GetCRC32Str(path);
        ResourceItem item = null;
        if (!AssetDict.TryGetValue(crc, out item) || item == null)
        {
            DebugLogger.Instance.DebugError("AssetDict里不存在改资源：" + path + "    可能释放了多次");
            return false;
        }

        item.RefCount--;
        DestroyResourceItem(item, destroyObj);
        return true;
    }

    public bool ReleaseResource(Object obj, bool destroyObj = false)
    {
        if (obj == null) return false;
        ResourceItem item = null;
        foreach (ResourceItem res in AssetDict.Values)
        {
            if (res.m_Guid == obj.GetInstanceID())
                item = res;
        }

        if (item == null)
        {
            DebugLogger.Instance.DebugError("AssetDict里不存在改资源：" + obj.name + "    可能释放了多次");
            return false;
        }

        item.RefCount--;
        DestroyResourceItem(item, destroyObj);
        return true;
    }

    public bool ReleaseResource(ObjectItem obj, bool destroyObj = false)
    {
        if (obj == null) return false;

        ResourceItem item = null;
        if (!AssetDict.TryGetValue(obj.m_Crc, out item) || item == null)
        {
            DebugLogger.Instance.DebugError("AssetDict里不存在改资源：" + obj.m_CloneObj.name + "    可能释放了多次");
            return false;
        }

        GameObject.Destroy(obj.m_CloneObj);
        obj.m_CloneObj = null;

        item.RefCount--;
        DestroyResourceItem(item, destroyObj);
        return true;
    }

    //TODO优化：根据不同平台实现不同刷新策略--策略模式
    protected void WashOut()
    {

    }

    protected void DestroyResourceItem(ResourceItem item, bool destroy = false)
    {
        if (item == null || item.RefCount > 0)
        {
            return;
        }
        if (!destroy)
        {
            m_NoReferenceAssetMapList.InsertToHead(item);
            return;
        }

        if (!AssetDict.Remove(item.m_Crc))
        {
            return;
        }

        AssetBundleManager.Instance.ReleaseAsset(item);


        if (item.m_Obj != null)
        {
#if UNITY_EDITOR
            Resources.UnloadAsset(item.m_Obj);
#endif
            item.m_Obj = null;

        }

    }

    void CacheResource(string path, ref ResourceItem item, uint crc, Object obj, int addrefcount = 1)
    {
        //判断缓存容量
        WashOut();

        if (item == null)
        {
            DebugLogger.Instance.DebugError("ResourceItem is null,path:" + path);
        }
        if (obj == null)
        {
            DebugLogger.Instance.DebugError("Resource Fail:" + path);
        }

        item.m_Obj = obj;
        item.m_LastUseTime = Time.realtimeSinceStartup;
        item.RefCount += addrefcount;
        item.m_Guid = obj.GetInstanceID();
        ResourceItem oldItem = null;
        if (AssetDict.TryGetValue(item.m_Crc, out oldItem))
        {
            AssetDict[item.m_Crc] = item;
        }
        else
        {
            AssetDict.Add(item.m_Crc, item);
        }
    }

    ResourceItem GetCacheResourceItem(uint crc, int addrefcount = 1)
    {
        ResourceItem item;
        if (AssetDict.TryGetValue(crc, out item))
        {
            if (item != null)
            {
                item.RefCount += addrefcount;
                item.m_LastUseTime = Time.realtimeSinceStartup;
            }
        }
        return item;
    }


    public void Clear(bool force = false)
    {
        List<ResourceItem> list = new List<ResourceItem>();
        while (m_NoReferenceAssetMapList.Size() > 0)
        {
            ResourceItem item = m_NoReferenceAssetMapList.Back();
            if (!item.m_ClearSwitchScene && !force)
                list.Add(item);
            DestroyResourceItem(item, item.m_ClearSwitchScene);
            m_NoReferenceAssetMapList.Pop();
        }

        int index = list.Count - 1;
        while (index >= 0)
        {
            m_NoReferenceAssetMapList.Reflesh(list[index]);
            list.RemoveAt(index);
            --index;
            if (index < 0)
                break;
        }

    }

    public bool CancelAsyncLoad(ObjectItem obj)
    {
        AsyncLoadResParam parm = null;
        if (m_LoadingAssetDict.TryGetValue(obj.m_Crc, out parm) && m_LoadingAssetList[(int)parm.m_Priority].Contains(parm))
        {
            for (int i = parm.m_CallBackList.Count - 1; i >= 0; --i)
            {
                AsyncCallBack tempCall = parm.m_CallBackList[i];
                if (tempCall != null && obj == tempCall.m_Item)
                {
                    tempCall.Reset();
                    m_AsyncCallBackPool.Release(tempCall);
                    parm.m_CallBackList.Remove(tempCall);
                }
            }

            if (parm.m_CallBackList.Count <= 0)
            {
                parm.Reset();
                m_LoadingAssetList[(int)parm.m_Priority].Remove(parm);
                m_AsyncLoadResParamPool.Release(parm);
                m_LoadingAssetDict.Remove(obj.m_Crc);
                return true;
            }
        }

        return false;
    }

    public void PreloadRes(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;
        uint crc = CRC32.CRC32Cls.GetCRC32Str(path);

        ResourceItem item = GetCacheResourceItem(crc, 0);
        if (item != null)
            return;

        Object obj = null;
#if UNITY_EDITOR
        LoadResourceInEditor(crc, path, ref obj, ref item);
#endif
        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Obj != null)
                    obj = item.m_Obj;
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<Object>(item.m_AssetName);
                }
            }
        }

        CacheResource(path, ref item, crc, obj);
        item.m_ClearSwitchScene = false;
        ReleaseResource(obj, false);
    }

    public ObjectItem LoadResource(string path, ObjectItem objItem)
    {
        if (objItem == null)
            return null;
        uint crc = objItem.m_Crc == 0 ? CRC32Cls.GetCRC32Str(path) : objItem.m_Crc;
        ResourceItem item = GetCacheResourceItem(crc);
        if (item != null)
        {
            objItem.m_ResourceItem = item;
            return objItem;
        }

        Object obj = null;
#if UNITY_EDITOR
        LoadResourceInEditor(crc, path, ref obj, ref item);
#endif
        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Obj != null)
                    obj = item.m_Obj as Object;
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<Object>(item.m_AssetName);
                }
            }
        }

        CacheResource(path, ref item, crc, obj);
        objItem.m_ResourceItem = item;
        objItem.m_ClearSwitchScene = item.m_ClearSwitchScene;
        return objItem;
    }

    /// <summary>
    /// 根据ObjItem增加引用计数
    /// </summary>
    /// <returns></returns>
    public int IncreaseResourceRefCount(ObjectItem obj, int count = 1)
    {
        return obj != null ? IncreaseResourceRefCount(obj.m_Crc, count) : 0;
    }

    public int IncreaseResourceRefCount(uint crc, int count = 1)
    {
        ResourceItem item = null;
        if (!AssetDict.TryGetValue(crc, out item) || item == null)
            return -1;

        item.RefCount += count;
        item.m_LastUseTime = Time.realtimeSinceStartup;
        return item.RefCount;
    }
    public int DecreaseResourceRefCount(ObjectItem obj, int count = 1)
    {
        return obj != null ? DecreaseResourceRefCount(obj.m_Crc, count) : 0;
    }

    public int DecreaseResourceRefCount(uint crc, int count = 1)
    {
        ResourceItem item = null;
        if (!AssetDict.TryGetValue(crc, out item) || item == null)
            return -1;

        item.RefCount -= count;
        return item.RefCount;
    }

    public void AsyncLoadResource<T>(string path, ObjectItem obj, OnAsyncObjFinished dealFinish, LoadResPriority priority) where T : UnityEngine.Object
    {
        ResourceItem item = GetCacheResourceItem(obj.m_Crc);
        if (item != null)
        {
            obj.m_ResourceItem = item;
            if (dealFinish != null)
            {
                dealFinish(path, obj, priority);
            }
            return;
        }

        //判断是否在加载中
        AsyncLoadResParam parm = null;
        if (!m_LoadingAssetDict.TryGetValue(obj.m_Crc, out parm) || parm == null)
        {
            parm = m_AsyncLoadResParamPool.Spawn(true);
            parm.m_Crc = obj.m_Crc;
            parm.m_Path = path;
            parm.m_Priority = priority;
            parm.m_IsSprite = typeof(T) == typeof(Sprite);
            m_LoadingAssetDict.Add(obj.m_Crc, parm);
            m_LoadingAssetList[(int)priority].Add(parm);
        }

        AsyncCallBack callBack = m_AsyncCallBackPool.Spawn(true);
        callBack.m_ObjDealFinished = dealFinish;
        callBack.m_Item = obj;
        parm.m_CallBackList.Add(callBack);
    }

#if UNITY_EDITOR

    [Conditional(EditorConditionalHelpEnum.IN_EDITOR_NOT_LOAD_BY_AB)]
    private void LoadResourceInEditor<T>(uint crc, string path, ref T obj, ref ResourceItem item) where T : UnityEngine.Object
    {
        DebugLogger.Instance.DebugLog("编辑模式加载目录资源");
        item = AssetBundleManager.Instance.FindResourceItem(crc);
        if (item.m_Obj != null)
            obj = item.m_Obj as T;
        else
        {
            obj = AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }

    [Conditional(EditorConditionalHelpEnum.IN_EDITOR_NOT_LOAD_BY_AB)]
    private void LoadResourceInEditor<T>(string path, uint crc, ref Object obj, ref ResourceItem item, ref YieldInstruction yieldInstruction) where T : UnityEngine.Object
    {
        DebugLogger.Instance.DebugLog("编辑模式加载目录资源");
        obj = AssetDatabase.LoadAssetAtPath<T>(path);
        yieldInstruction = new WaitForSeconds(1f);
        item = AssetBundleManager.Instance.FindResourceItem(crc);
        item.m_Obj = obj;
    }

#endif
}

