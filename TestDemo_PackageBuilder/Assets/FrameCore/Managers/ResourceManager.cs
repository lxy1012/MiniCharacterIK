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
    //������ɵĻص�
    public OnAsyncLoadObjFinished m_DealFinished = null;
    //�ص�����
    public object m_parms1 = null;
    public object m_parms2 = null;
    public object m_parms3 = null;

    //ʵ����������ɵĻص�
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

//��Դ������ɻص�
public delegate void OnAsyncLoadObjFinished(string path, Object obj,
            object parms1 = null, object parms2 = null, object parms3 = null);
//ʵ�������������ɻص�
public delegate void OnAsyncObjFinished(string path, ObjectItem obj,
            object parms1 = null, object parms2 = null, object parms3 = null);

//TODO: AssetBundleMaanger <->ResourceManager <->GameObjectManagerΪIOCע��ģʽ
public partial class ResourceManager : Singletion<ResourceManager>
{
    protected ClassObjectPool<AsyncLoadResParam> m_AsyncLoadResParamPool = new ClassObjectPool<AsyncLoadResParam>(50);
    protected ClassObjectPool<AsyncCallBack> m_AsyncCallBackPool = new ClassObjectPool<AsyncCallBack>(100);
    //���������ü���Ϊ0����Դ���ﵽ�������ʱ�ͷ��������б����Դ
    protected LRUMapList<ResourceItem> m_NoReferenceAssetMapList = new LRUMapList<ResourceItem>();
    //����ʹ�ù�����Դ��
    public Dictionary<uint, ResourceItem> AssetDict { get; set; } = new Dictionary<uint, ResourceItem>();
    //�����첽���ص���Դ�б�
    public List<AsyncLoadResParam>[] m_LoadingAssetList = new List<AsyncLoadResParam>[(int)LoadResPriority.PRIORITY_NUM];
    //�����첽���ص�DI
    public Dictionary<uint, AsyncLoadResParam> m_LoadingAssetDict = new Dictionary<uint, AsyncLoadResParam>();

    private MonoBehaviour m_Mono;
    private long lastYieldTime;
    private const long MAX_LOADING_RES_TIME = 200000;  //��λ΢��
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

    //TODO�Ż�������������ʵ���첽ѭ���߼�---����forѭ����ɾ�����µ�����ָ�붪ʧ��ȡ���첽
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
    /// �첽��Դ����--����Ҫʵ��������Դ
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

        //�ж��Ƿ��ڼ�����
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
    /// ͬ����Դ���أ��ⲿֱ��ͨ�á����ز���Ҫʵ��������Դ
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
            DebugLogger.Instance.DebugError("AssetDict�ﲻ���ڸ���Դ��" + path + "    �����ͷ��˶��");
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
            DebugLogger.Instance.DebugError("AssetDict�ﲻ���ڸ���Դ��" + obj.name + "    �����ͷ��˶��");
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
            DebugLogger.Instance.DebugError("AssetDict�ﲻ���ڸ���Դ��" + obj.m_CloneObj.name + "    �����ͷ��˶��");
            return false;
        }

        GameObject.Destroy(obj.m_CloneObj);
        obj.m_CloneObj = null;

        item.RefCount--;
        DestroyResourceItem(item, destroyObj);
        return true;
    }

    //TODO�Ż������ݲ�ͬƽ̨ʵ�ֲ�ͬˢ�²���--����ģʽ
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
        //�жϻ�������
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
    /// ����ObjItem�������ü���
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

        //�ж��Ƿ��ڼ�����
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
        DebugLogger.Instance.DebugLog("�༭ģʽ����Ŀ¼��Դ");
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
        DebugLogger.Instance.DebugLog("�༭ģʽ����Ŀ¼��Դ");
        obj = AssetDatabase.LoadAssetAtPath<T>(path);
        yieldInstruction = new WaitForSeconds(1f);
        item = AssetBundleManager.Instance.FindResourceItem(crc);
        item.m_Obj = obj;
    }

#endif
}

