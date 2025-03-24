using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadFileUnility 
{
    /// <summary>
    /// �������ݱ���Դ
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
            DebugLogger.Instance.DebugError($"{e.Message}     ��ȡ�������ļ� ʧ�ܣ�·����{path}");
        }

        return default(T);
    }
}
