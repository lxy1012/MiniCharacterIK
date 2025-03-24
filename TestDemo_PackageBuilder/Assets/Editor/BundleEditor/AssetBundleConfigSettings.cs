using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


[CreateAssetMenu(fileName = "AssetBundleConfigSettings", menuName = "PackageBuilder/AssetBundleConfigSettings")]
public class AssetBundleConfigSettings : ScriptableObject
{
    [SerializeField]
    private string m_BundleVersion;
    [SerializeField]
    private List<string> m_AllPrefabsAB = new List<string>();
    [SerializeField]
    private List<FolderDirInfo> m_AllFolderDirAB = new List<FolderDirInfo>();

    [System.Serializable]
    public struct FolderDirInfo
    {
        public string ABName;
        public string Path;
    }

    public string BundleVersion
    {
        get { return m_BundleVersion; }
    }

    public List<string> AllPrefabsDir
    {
        get { return m_AllPrefabsAB; }
    }
    public List<FolderDirInfo>  AllFolderDir
    {
        get { return m_AllFolderDirAB; }
    }

    public void OnUpdateBundleVersion(string bundleVersion)
    {
        m_BundleVersion = bundleVersion;
    }

    public int OnAddPrefabElement()
    {
        m_AllPrefabsAB.Add(string.Empty);
        return m_AllPrefabsAB.Count - 1;
    }
    public bool OnRemovePrefabElement(int index)
    {
        if (m_AllPrefabsAB.Count <= index)
            return false;
        m_AllPrefabsAB.RemoveAt(index);
        return true;
    }
    public bool OnSetPrefabElement(int index,string value)
    {
        if (m_AllPrefabsAB.Count <= index)
            return false;
        m_AllPrefabsAB[index] = value;
        return true;
    }

    public int OnAddFolderElement()
    {
        m_AllFolderDirAB.Add(new FolderDirInfo());
        return m_AllFolderDirAB.Count - 1;
    }

    public bool OnRemoveFolderElement(int index)
    {
        if (m_AllFolderDirAB.Count <= index)
            return false;
        m_AllFolderDirAB.RemoveAt(index);
        return true;
    }

    public bool OnSetFolderElement(int index, FolderDirInfo value)
    {
        if (m_AllFolderDirAB.Count <= index)
            return false;
        m_AllFolderDirAB [index] = value;
        return true;
    }
}
