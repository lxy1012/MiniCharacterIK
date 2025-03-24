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
/// ��C#����ָ����.proto�ļ�������Ե�����
/// </summary>
[Tooltip("����������дclass/proto�ļ�����-��ѡ")]
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
            DebugLogger.Instance.DebugError("���Ͳ������ļ�ת��ʧ�ܣ�");
            return;
        }
        bool hasProtoContract = Attribute.IsDefined(obj.GetClass(), typeof(ProtoContractAttribute), false);
        if (!hasProtoContract)
        {
            DebugLogger.Instance.DebugError("���Ͳ������ļ�ת��ʧ�ܣ�");
            return;
        }

        BuildProtoClassAssets(CommonConstParm.ASSET_BUNDLE_CONFIG_ASSET, obj.name, obj.GetClass());
        AssetDatabase.Refresh();
        DebugLogger.Instance.DebugLog("��ǰ�ļ�ת����ɣ�");
    }

    [MenuItem("Assets/Create/.proto -> class", priority = 9)]
    public static void GenerateClassFile()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (!path.EndsWith(".proto"))
        {
            DebugLogger.Instance.DebugError("���Ͳ������ļ�ת��ʧ�ܣ�");
            return;
        }

        BuildClassProtoAssets(path);
        AssetDatabase.Refresh();
        DebugLogger.Instance.DebugLog("��ǰ�ļ�ת����ɣ�");
    }

    /// <summary>
    /// ����proto���Ŀ��
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
                if (File.Exists(outputPath)) File.Delete(outputPath);  //��������.proto�ı�
                string protoContent = RuntimeTypeModel.Default.GetSchema(type, ProtoSyntax.Proto3);
                File.WriteAllText(outputPath, protoContent);

            }
            catch (Exception)
            {

                DebugLogger.Instance.DebugError($"����{name}.proto�ļ�ʧ�ܣ�");
                return;
            }
        }
    }

    /// <summary>
    /// ����class���Ŀ��
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

                DebugLogger.Instance.DebugError($"{ex.Message}   ����{ppath.Replace(".proto", ".cs")}�ļ�ʧ�ܣ�");
                throw;
            }
        }
    }

    /// <summary>
    /// ��������-�������ļ�
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
    /// �ڵ�����Դ���ñ�ʱ������byte�ļ�
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
