using ProtoBuf;
using System;
using System.IO;
using System.Text;

public class ProtobufSerializer
{
    /// <summary>
    /// 序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="inst"></param>
    /// <returns></returns>
    public static byte[] Serialize<T>(T inst) where T : class, new()
    {
        string data = "";
        using (MemoryStream ms = new MemoryStream())
        {
            Serializer.Serialize(ms, inst);
            ms.Position = 0;
            data = Convert.ToBase64String(ms.ToArray());
        }
        return Encoding.UTF8.GetBytes(data);
    }

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    public static T Deserialize<T>(byte[] data) where T : class
    {
        T target = default(T);
        var rawStr = Encoding.UTF8.GetString(data);
        byte[] rawData = Convert.FromBase64String(rawStr);
        using (MemoryStream ms = new MemoryStream(rawData))
        {
            target = Serializer.Deserialize<T>(ms);
        }
        return target;
    }

}
