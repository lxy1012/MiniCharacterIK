using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

/// <summary>
/// ��C#����json�ļ�
/// </summary>
public class CsJsonHelper
{
    private static string jsonPath = "";
    public static string JsonPath
    {
        get
        {
            if (string.IsNullOrEmpty(jsonPath))
                jsonPath = $"{Application.dataPath}/{CommonConstParm.ASSET_BUNDLE_CONFIG_JSON}";
            return jsonPath;

        }
    }
    
    [Conditional(EditorConditionalHelpEnum.JSON_VIEW_OPEN)]
    public static void ToJsonAssets<T>(T target, string name) where T : class, new()
    {
        string path = $"{JsonPath}/{name}";
        try
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter> { new Vector3Converter() },
                Formatting = Formatting.Indented
            };
            string json = JsonConvert.SerializeObject(target, settings);
            if (File.Exists(path))
            {
                File.Delete(path);
                File.Delete(path+".meta");
            }
            File.WriteAllText(path, json);
            DebugLogger.Instance.DebugLog($"����json�ļ���{name} �ɹ���·����{path}");
        }
        catch (Exception ex)
        {
            DebugLogger.Instance.DebugError($"{ex.Message}  ����json�ļ���{name} ʧ�ܣ�·����{path}");
        }

    }
}
