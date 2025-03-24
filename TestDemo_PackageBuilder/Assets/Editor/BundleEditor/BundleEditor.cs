using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class BundleEditor
{
    //key:abname; value:path
    private static Dictionary<string, string> m_AllFileDir = new Dictionary<string, string>();
    //key:prefab-2-abname; value:path
    private static Dictionary<string, List<string>> m_AllPrefabDir = new Dictionary<string, List<string>>();
    //already add abs
    private static Dictionary<string, List<string>> m_AllFileAB = new Dictionary<string, List<string>>();
    private static string m_ABOutputPath = "";
    private static string m_ABVersion = "";
    [MenuItem("Tools/打包/2.更新AB包")]
    [Tooltip("生成所有资源文件并生成AB包管理表")]
    public static void BuildABPackage()
    {
        m_AllFileDir.Clear();
        m_AllFileAB.Clear();
        m_AllPrefabDir.Clear();
        PackagePlatformSettings settings = AssetDatabase.LoadAssetAtPath<PackagePlatformSettings>(EditorConstParm.ASSET_BUNDLE_GLOBAL_SETTINGS);
        if (settings == null)
        {
            DebugLogger.Instance.DebugError($"not exist asset file:{EditorConstParm.ASSET_BUNDLE_GLOBAL_SETTINGS}");
            return;
        }


        AssetBundleConfigSettings abConfig = AssetDatabase.LoadAssetAtPath<AssetBundleConfigSettings>(settings.AssetBundleConfigSettingsPath);
        m_ABVersion = abConfig.BundleVersion;
        foreach (AssetBundleConfigSettings.FolderDirInfo info in abConfig.AllFolderDir)
        {
            if (m_AllFileDir.ContainsKey(info.ABName))
            {
                DebugLogger.Instance.DebugError($"存在重复的AB文件夹名:{info.ABName}，请检查！");
            }
            else
            {
                m_AllFileDir.Add(info.ABName, info.Path);
                var files = Directory.GetFiles(info.Path);
                m_AllFileAB.Add(info.ABName, new List<string>());
                List<string> list = m_AllFileAB[info.ABName];
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        if (file.EndsWith(".meta")) continue;
                        if (list.Contains(file))
                        {
                            DebugLogger.Instance.DebugError($"AB文件夹名:{info.ABName}，存在重复的资源:{file} 请检查！");
                            continue;
                        }
                        list.Add(file.Replace('\\', '/'));
                    }
                }
            }
        }


        string[] allPrefab = AssetDatabase.FindAssets("t:prefab", abConfig.AllPrefabsDir.ToArray());
        string abName = string.Empty;
        List<string> abFile = null;
        for (int i = 0; i < allPrefab.Length; ++i)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(allPrefab[i]);
            EditorUtility.DisplayProgressBar("查找预制体", "预制体路径：" + prefabPath, i / allPrefab.Length * i);

            if (!AlreadyInFilesAB(prefabPath, out abName))
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                string[] dependencies = AssetDatabase.GetDependencies(prefabPath);
                List<string> allDependcy = new List<string>();
                for (int j = 0; j < dependencies.Length; ++j)
                {
                    if (!dependencies[j].EndsWith(".cs"))
                    {
                        if (AlreadyInFilesAB(dependencies[j], out abName))
                        {
                            allDependcy.Add(abName);
                            //DebugLogger.Instance.DebugLog($"预制体依赖其他包！！依赖 {abName}的{dependencies[j]}");
                        }
                        else
                        {
                            if (!m_AllFileAB.TryGetValue(go.name, out abFile))
                            {
                                abFile = new List<string>();
                            }
                            abFile.Add(dependencies[j]);
                            m_AllFileAB[go.name] = abFile;
                            //DebugLogger.Instance.DebugLog($"预制体依赖！！资源 {dependencies[j]}  只依赖本预制体{go.name}");
                        }
                    }
                }
                if (m_AllPrefabDir.ContainsKey(go.name))
                {
                    DebugLogger.Instance.DebugError($"存在重复的预制体名:{go.name}，请检查！");
                }
                else
                {
                    m_AllPrefabDir.Add(go.name, m_AllFileAB[go.name]);
                }
            }
        }

        foreach (string name in m_AllFileDir.Keys)
        {
            SetABName(name, m_AllFileDir[name]);
        }

        foreach (string name in m_AllPrefabDir.Keys)
        {
            SetABName(name, m_AllPrefabDir[name]);
        }

        m_ABOutputPath = $"{Application.dataPath}{settings.AssetBundleBuildABTargetPath}{EditorUserBuildSettings.activeBuildTarget}";
        if (!Directory.Exists(m_ABOutputPath))
        {
            Directory.CreateDirectory(m_ABOutputPath);
        }

        BuildAssetBundle();

        string[] oldABNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < oldABNames.Length; ++i)
        {
            AssetDatabase.RemoveAssetBundleName(oldABNames[i], true);
            EditorUtility.DisplayProgressBar("清除AB包名中...", $"包名:{oldABNames[i]}", i / oldABNames.Length * i);
        }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }


    private static void BuildAssetBundle()
    {
        string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
        //key:path; value:abname
        Dictionary<string, string> resPathDic = new Dictionary<string, string>();
        for (int i = 0; i < allBundles.Length; ++i)
        {
            string[] allBundlePath = AssetDatabase.GetAssetPathsFromAssetBundle(allBundles[i]);
            for (int j = 0; j < allBundlePath.Length; ++j)
            {
                //DebugLogger.Instance.DebugLog($"此AB包:{allBundles[i]}下面包含的资源文件路径:{allBundlePath[j]}");
                resPathDic.Add(allBundlePath[j], allBundles[i]);
            }
        }

        DeletePrevAssetBundle();
        WriteAssetBundleConfig(resPathDic);

        BuildPipeline.BuildAssetBundles(m_ABOutputPath, BuildAssetBundleOptions.ChunkBasedCompression,
                EditorUserBuildSettings.activeBuildTarget);
    }

    /// <summary>
    /// 生成AB配置表
    /// </summary> <summary>
    private static void WriteAssetBundleConfig(Dictionary<string, string> resPathDic)
    {
        AssetBundleConfigData assetBundleConfig = new AssetBundleConfigData();
        assetBundleConfig.Version = m_ABVersion;
        foreach (string path in resPathDic.Keys)
        {
            if (resPathDic[path] == CommonConstParm.ASSET_BUNDLE_CONFIG_NAME)
                continue;
            AssetBundleBase abBase = new AssetBundleBase();
            abBase.Path = path;

            abBase.Crc = CRC32.CRC32Cls.GetCRC32Str(path);

            abBase.ABName = resPathDic[path];
            abBase.AssetName = path.Remove(0, path.LastIndexOf('/') + 1);
            string[] depedencies = AssetDatabase.GetDependencies(path);
            for (int i = 0; i < depedencies.Length; ++i)
            {
                string depency = depedencies[i];
                if (depency == path || path.EndsWith(".cs"))
                {
                    continue;
                }

                if (resPathDic.TryGetValue(depency, out string abName))
                {
                    if (abName == resPathDic[path])
                    {
                        continue;
                    }

                    if (!abBase.ABDependcies.Contains(abName))
                    {
                        abBase.ABDependcies.Add(abName);
                    }
                }
            }
            assetBundleConfig.AssetBundleLists.Add(abBase);
        }

        //生成AB配置表数据：JSON格式 --编辑器需添加[JSON_VIEW_OPEN]标识
        CsJsonHelper.ToJsonAssets(assetBundleConfig, CommonConstParm.ASSET_BUNDLE_CONFIG_NAME + ".json");

        //生成AB配置表数据
        foreach (var item in assetBundleConfig.AssetBundleLists)
        {
            item.Path = "";
        }
        CsProtoHelper.ToBinaryAssets<AssetBundleConfigData>(assetBundleConfig,
                CommonConstParm.ASSET_BUNDLE_CONFIG_NAME + ".bytes",
                Path.Combine(Application.dataPath, CommonConstParm.ASSET_BUNDLE_CONFIG_ASSET));

        DebugLogger.Instance.DebugLog($"更新AB包完毕！\n  路径：{m_ABOutputPath}");
    }

    /// <summary>
    /// 删除无用的AB包
    /// </summary>
    private static void DeletePrevAssetBundle()
    {
        string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
        DirectoryInfo dir = new DirectoryInfo(m_ABOutputPath);
        FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories);
        for (int i = files.Length - 1; i >= 0; --i)
        {
            //DebugLogger.Instance.DebugLog(files[i].ToString());
            if (ContainABName(files[i].Name, allBundles) || files[i].Name.EndsWith(".meta"))
            {
                continue;
            }
            else
            {
                //DebugLogger.Instance.DebugError($"此AB包:{files[i]}已重命名或被删除！");
                files[i].Delete();
            }
        }
    }

    private static bool ContainABName(string name, string[] strs)
    {
        for (int i = 0; i < strs.Length; ++i)
        {
            if (name == strs[i])
            {
                return true;
            }
        }
        return false;
    }


    private static void SetABName(string name, string path)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(path);
        if (assetImporter == null)
        {
            DebugLogger.Instance.DebugError($"不存在此路径:{path}，请检查！");
        }
        else
        {
            assetImporter.assetBundleName = name;
        }
    }

    private static void SetABName(string name, List<string> paths)
    {
        for (int i = 0; i < paths.Count; ++i)
        {
            SetABName(name, paths[i]);
        }
    }

    private static bool AlreadyInFilesAB(string path, out string abName)
    {
        abName = string.Empty;
        foreach (string key in m_AllFileAB.Keys)
        {
            foreach (string p in m_AllFileAB[key])
            {
                if (p == path || path.Contains(p))
                {
                    abName = key;
                    return true;
                }
            }
        }

        return false;
    }

}
