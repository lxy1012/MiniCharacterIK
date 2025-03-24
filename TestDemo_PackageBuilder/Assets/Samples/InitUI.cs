using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InitUI : MonoBehaviour
{
    [SerializeField]
    private Button m_BtnStart;
    [SerializeField]
    private Text m_Text;
    [SerializeField]
    private RawImage m_Img;
    private void Start()
    {
        /*        DebugLogger.DebugLog("ttttttt");
        uint monster = CRC32.CRC32Cls.GetCRC32Str("Assets/AutoAssetBundlesFrame/GameAssets/Prefabs/monster.prefab");
        var ite = AssetBundleManager.Instance.LoadResourceAssetBundle(monster);
        var node = ite.m_AssetBundle.LoadAsset<GameObject>("monster.prefab");
        GameObject.Instantiate(node);*/
    }

    public void ShowUI()
    {
        m_BtnStart.onClick.AddListener(EnterGameMain);
        m_BtnStart.gameObject.SetActive(true);
        m_Text.gameObject.SetActive(false);
    }

    private void EnterGameMain()
    {
        SceneManager.LoadScene(1);
    }

    public void UpdateText(string text)
    {
        m_Text.text = text;
    }
    public void UpdateIcon(Texture2D sp)
    {
        if (sp != null)
        {
            m_Img.texture = sp;
        }
        m_Img.enabled = sp != null;
    }

    private void OnDestroy()
    {
        m_Text.text = string.Empty;
        m_Img.texture = null;
        m_BtnStart.onClick.RemoveListener(EnterGameMain);
    }
}
