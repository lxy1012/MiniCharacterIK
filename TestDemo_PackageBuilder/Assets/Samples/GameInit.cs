using UnityEngine;


public class GameInit : MonoBehaviour
{
    bool isLoading = false;
    private InitUI m_InitUI;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        AssetBundleManager.Instance.Init(this);
        ResourceManager.Instance.Init(this);
        GameObjectManager.Instance.Init(transform.Find("RecyclePools"), transform.Find("SceneTrans"));
    }
    // Start is called before the first frame update
    void Start()
    {
        m_InitUI = GameObject.Find("Canvas").GetComponent<InitUI>();
        isLoading = true;
        AssetBundleManager.Instance.CheckAssetBundleVersion(CheckVersion);
    }

    void CheckVersion()
    {
        isLoading = false;
        m_InitUI.ShowUI();
        //GameObjectManager.Instance.InstantiateObjects("Assets/GameAssets/Prefabs/akai.prefab", true);
        GameObjectManager.Instance.PreloadGameObejct("Assets/GameAssets/Prefabs/akai.prefab", 20);
        ResourceManager.Instance.AsyncLoadResource<Texture2D>("Assets/GameAssets/Textures/ID_362.bmp", OnAsyncLoadObjFinished, LoadResPriority.LOAD_HIGH);
        //ResourceManager.Instance.PreloadRes("Assets/AutoAssetBundlesFrame/GameAssets/Sounds/menusound.mp3");
    }

    private void OnAsyncLoadObjFinished(string path, Object obj,
            object parms1 = null, object parms2 = null, object parms3 = null)
    {
        m_InitUI.UpdateIcon(obj as Texture2D);
    }

    // Update is called once per frame
    void Update()
    {
        if (isLoading)
        {
            m_InitUI.UpdateText("\"更新资源加载中...\"");
        }

    }

#if UNITY_EDITOR
    private void OnApplicationQuit()
    {
        GameObjectManager.Instance.Clear(true);
        ResourceManager.Instance.Clear(true);
        Resources.UnloadUnusedAssets();
        AssetBundleManager.Instance.Clear(true);
        DebugLogger.Instance.DebugLog("清空缓存");
    }
#endif
}
