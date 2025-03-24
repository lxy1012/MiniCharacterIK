using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

/// <summary>
/// 从C#生成json文件
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
            DebugLogger.Instance.DebugLog($"生成json文件：{name} 成功！路径：{path}");
        }
        catch (Exception ex)
        {
            DebugLogger.Instance.DebugError($"{ex.Message}  生成json文件：{name} 失败！路径：{path}");
        }

    }
}
