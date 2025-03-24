using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static AssetBundleConfigSettings;

[CustomEditor(typeof(AssetBundleConfigSettings))]
public class AssetBundleConfigSettingsInspector : Editor
{
    private AssetBundleConfigSettings m_ABTarget;

    private ReorderableList m_PrefabsDirRI;
    private const float m_PrefabsElementHeight = 20f;

    private ReorderableList m_FolderDirRI;
    private const float m__FolderElementHeight = 40f;

    private string m_Version = string.Empty;
    private void OnEnable()
    {
        m_ABTarget = (AssetBundleConfigSettings)target;
        if (m_ABTarget == null)
            return;

        m_PrefabsDirRI = new ReorderableList(m_ABTarget.AllPrefabsDir, typeof(string), true, true, true, true)
        {
            drawHeaderCallback = DrawPrefabsHeader,
            drawElementCallback = DrawPrefabsElement,
            elementHeight = m_PrefabsElementHeight,
            onAddCallback = OnAddPrefabElement,
            onRemoveCallback = OnRemovePrefabElement
        };

        m_FolderDirRI = new ReorderableList(m_ABTarget.AllFolderDir, typeof(FolderDirInfo), true, true, true, true)
        {
            drawHeaderCallback = DrawFolderHeader,
            drawElementCallback = DrawFolderElement,
            elementHeight = m__FolderElementHeight,
            onAddCallback = OnAddFolderElement,
            onRemoveCallback = OnRemoveFolderElement
        };

    }

    private void OnDisable()
    {
        if (m_ABTarget == null) return;
        EditorUtility.SetDirty(m_ABTarget);
        AssetDatabase.SaveAssets();
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        UpdateBundleVersion();
        m_PrefabsDirRI.DoLayoutList();
        m_FolderDirRI.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }

    private void UpdateBundleVersion()
    {
        m_Version = EditorGUILayout.TextField("版本号", m_ABTarget.BundleVersion);
        m_ABTarget.OnUpdateBundleVersion(m_Version);
    }

    private void DrawPrefabsHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, "预制体资源路径列表");
    }

    private void DrawPrefabsElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        string path = m_ABTarget.AllPrefabsDir[index];
        Rect pathRect = new Rect(rect.x, rect.y, rect.width - 60, rect.height);
        string newPath = EditorGUI.TextField(pathRect, new GUIContent("路径"), path);
        if (newPath != path)
        {
            path = newPath;
        }

        Rect buttonRect = new Rect(rect.x + rect.width - 55f, rect.y, 50f, rect.height);
        if (GUI.Button(buttonRect, "选择"))
        {
            EditorApplication.delayCall = null;
            EditorApplication.delayCall += () => ShowPrefabPathSelector(index);
        }
        else
        {
            m_ABTarget.OnSetPrefabElement(index, path);
        }
    }

    private void OnAddPrefabElement(ReorderableList list)
    {
        m_ABTarget.OnAddPrefabElement();
    }
    private void OnRemovePrefabElement(ReorderableList list)
    {
        m_ABTarget.OnRemovePrefabElement(list.index);
    }

    private void DrawFolderHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, "文件夹资源路径列表");
    }

    private void DrawFolderElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        FolderDirInfo dirInfo = m_ABTarget.AllFolderDir[index];

        float lineHeight = EditorGUIUtility.singleLineHeight;
        Rect nameRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
        string newName = EditorGUI.TextField(nameRect, new GUIContent("包名"), dirInfo.ABName);
        if (newName != dirInfo.ABName)
        {
            dirInfo.ABName = newName;
        }

        Rect pathRect = new Rect(rect.x, rect.y + lineHeight, rect.width - 60, lineHeight);
        string newPath = EditorGUI.TextField(pathRect, new GUIContent("路径"), dirInfo.Path);
        if (newPath != dirInfo.Path)
        {
            dirInfo.Path = newPath;
        }

        Rect buttonRect = new Rect(rect.x + rect.width - 55f, rect.y + lineHeight, 50f, lineHeight);
        if (GUI.Button(buttonRect, "选择"))
        {
            EditorApplication.delayCall = null;
            EditorApplication.delayCall += () => ShowFolderPathSelector(index);
        }
        else
        {
            m_ABTarget.OnSetFolderElement(index, dirInfo);
        }
    }

    private void OnAddFolderElement(ReorderableList list)
    {
        m_ABTarget.OnAddFolderElement();
    }
    private void OnRemoveFolderElement(ReorderableList list)
    {
        m_ABTarget.OnRemoveFolderElement(list.index);
    }

    private void ShowPrefabPathSelector(int index)
    {
        string currentPath = GetFullPath(m_ABTarget.AllPrefabsDir[index]);
        string selectedPath = EditorUtility.OpenFolderPanel("选择文件夹", GetStartPath(currentPath), "");
        if (!string.IsNullOrEmpty(selectedPath))
        {
            string relativePath = ConvertToRelativePath(selectedPath);
            if (!string.IsNullOrEmpty(relativePath))
            {
                m_ABTarget.OnSetPrefabElement(index, relativePath);
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "必须选择项目内的资源文件夹", "确定");
            }
        }
    }
    private void ShowFolderPathSelector(int index)
    {
        FolderDirInfo dirInfo = m_ABTarget.AllFolderDir[index];
        string currentPath = GetFullPath(dirInfo.Path);
        string selectedPath = EditorUtility.OpenFolderPanel("选择文件夹", GetStartPath(currentPath), "");
        if (!string.IsNullOrEmpty(selectedPath))
        {
            string relativePath = ConvertToRelativePath(selectedPath);
            if (!string.IsNullOrEmpty(relativePath))
            {
                dirInfo.Path = relativePath;
                m_ABTarget.OnSetFolderElement(index, dirInfo);
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "必须选择项目内的资源文件夹", "确定");
            }
        }
    }

    private string ConvertToRelativePath(string fullPath)
    {
        if (fullPath.StartsWith(Application.dataPath))
        {
            return "Assets" + fullPath.Substring(Application.dataPath.Length);
        }
        return null;
    }

    private string GetFullPath(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return "";
        return Path.Combine(Application.dataPath, relativePath.Substring(6)); // 跳过"Assets/"
    }

    private string GetStartPath(string currentPath)
    {
        if (!string.IsNullOrEmpty(currentPath) && Directory.Exists(Path.GetDirectoryName(currentPath)))
        {
            return Path.GetDirectoryName(currentPath);
        }
        return Application.dataPath;
    }
}
