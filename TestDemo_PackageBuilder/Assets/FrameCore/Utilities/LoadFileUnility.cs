using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadFileUnility 
{
    /// <summary>
    /// 加载数据表资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static T LoadDataAssets<T>(string path) where T : class, new()
    {
        try
        {
            byte[] data = default;
            data = File.ReadAllBytes(path);
            return ProtobufSerializer.Deserialize<T>(data);
        }
        catch (Exception e)
        {
            DebugLogger.Instance.DebugError($"{e.Message}     读取二进制文件 失败！路径：{path}");
        }

        return default(T);
    }
}
