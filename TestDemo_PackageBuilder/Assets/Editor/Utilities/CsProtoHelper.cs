using Google.Protobuf.Reflection;
using ProtoBuf;
using ProtoBuf.Meta;
using ProtoBuf.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 从C#生成指定的.proto文件，仅针对单个类
/// </summary>
[Tooltip("开发根据手写class/proto文件生成-可选")]
public class CsProtoHelper
{
    private static string protoBinPath = "";
    public static string ProtoBinPath
    {
        get
        {
            if (string.IsNullOrEmpty(protoBinPath))
                protoBinPath = $"{Application.dataPath}/{CommonConstParm.ASSET_BUNDLE_CONFIG_CONFIGS}";
            return protoBinPath;

        }
    }


    [MenuItem("Assets/Create/class ->.proto", priority = 10)]
    public static void GenerateProtoFile()
    {
        MonoScript obj = Selection.activeObject as MonoScript;
        if (obj == null)
        {
            DebugLogger.Instance.DebugError("类型不符，文件转换失败！");
            return;
        }
        bool hasProtoContract = Attribute.IsDefined(obj.GetClass(), typeof(ProtoContractAttribute), false);
        if (!hasProtoContract)
        {
            DebugLogger.Instance.DebugError("类型不符，文件转换失败！");
            return;
        }

        BuildProtoClassAssets(CommonConstParm.ASSET_BUNDLE_CONFIG_ASSET, obj.name, obj.GetClass());
        AssetDatabase.Refresh();
        DebugLogger.Instance.DebugLog("当前文件转换完成！");
    }

    [MenuItem("Assets/Create/.proto -> class", priority = 9)]
    public static void GenerateClassFile()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (!path.EndsWith(".proto"))
        {
            DebugLogger.Instance.DebugError("类型不符，文件转换失败！");
            return;
        }

        BuildClassProtoAssets(path);
        AssetDatabase.Refresh();
        DebugLogger.Instance.DebugLog("当前文件转换完成！");
    }

    /// <summary>
    /// 生成proto类的目标
    /// </summary>
    /// <param name="type"></param>
    /// <param name="path"></param>
    /// <param name="name"></param>
    public static void BuildProtoClassAssets(string toPath, string name, Type type)
    {
        if (type.GetCustomAttributes(typeof(ProtoContractAttribute), false).Length > 0)
        {
            try
            {
                string outputPath = $"{Application.dataPath}{toPath}/{name}.proto";
                if (File.Exists(outputPath)) File.Delete(outputPath);  //重新生成.proto文本
                string protoContent = RuntimeTypeModel.Default.GetSchema(type, ProtoSyntax.Proto3);
                File.WriteAllText(outputPath, protoContent);

            }
            catch (Exception)
            {

                DebugLogger.Instance.DebugError($"生成{name}.proto文件失败！");
                return;
            }
        }
    }

    /// <summary>
    /// 生成class类的目标
    /// </summary>
    /// <param name="toPath"></param>
    /// <param path="path"></param>
    public static void BuildClassProtoAssets(string path)
    {
        string pname = Path.GetFileName(path);
        string ppath = Path.GetDirectoryName(path);

        if (path.EndsWith(".proto"))
        {
            try
            {
                var set = new FileDescriptorSet { };
                set.AddImportPath(ppath);
                set.Add(pname, true);
                set.Process();
                var errors = set.GetErrors();
                int exitCode = 0;
                foreach (var err in errors)
                {
                    if (err.IsError) exitCode++;
                    UnityEngine.Debug.Log(err.ToString());
                }

                DebugLogger.Instance.DebugLog(exitCode.ToString());
                if (exitCode != 0)
                    return;

                IEnumerable<CodeFile> files = CSharpCodeGenerator.Default.Generate(set);
                foreach (var file in files)
                {
                    ppath = Path.Combine(ppath, file.Name);
                    if(File.Exists(ppath)) File.Delete(ppath);
                    File.WriteAllText(ppath, file.Text);
                }

            }
            catch (Exception ex)
            {

                DebugLogger.Instance.DebugError($"{ex.Message}   生成{ppath.Replace(".proto", ".cs")}文件失败！");
                throw;
            }
        }
    }

    /// <summary>
    /// 生成数据-二进制文件
    /// </summary>
    /// <param name="abConfig"></param>
    /// <param name="path"></param>
    /// <param name="name"></param>
    public static void ToBinaryAssets<T>(T inst, string name, string path) where T : class,new ()
    {
        string fullPath = Path.Combine(path, name);
        try
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                File.Delete(fullPath + ".meta");
            }
                
        }
        catch (Exception ex)
        {
            DebugLogger.Instance.DebugError(ex.Message);
        }

        using (FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
        {
            fs.Position = 0;
            byte[] bytes = ProtobufSerializer.Serialize(inst);
            fs.Write(bytes, 0, bytes.Length);
        }
    }

    /// <summary>
    /// 在导入资源配置表时，加密byte文件
    /// </summary>
    public static void EncryptProtoBinAssets()
    {
        string[] files = Directory.GetFiles(ProtoBinPath);
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].EndsWith(".meta")) continue;
            EncryptProtoBinAsset(files[i]);
        }
    }

    private static void EncryptProtoBinAsset(string path)
    {
        try
        {
            byte[] rawdata = File.ReadAllBytes(path);
            string data = Convert.ToBase64String(rawdata);
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            File.WriteAllBytes(path, bytes);
        }
        catch (Exception ex)
        {
            DebugLogger.Instance.DebugError(ex.Message);
        }
    }


}
