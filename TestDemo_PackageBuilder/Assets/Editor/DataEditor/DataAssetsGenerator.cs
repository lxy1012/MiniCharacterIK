using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class DataAssetsGenerator : EditorWindow
{
    [MenuItem("Tools/���/1.������Ϸ���ñ�����")]
    [Tooltip("�������ж������ļ�����Ϸ���б��룡")]
    public static void GenerateBinFiles()
    {
        string rootPath = Path.GetDirectoryName(Application.dataPath);

        //1.����xlsx-> bin + proto�ļ�
        RunBatchScript($"{rootPath}/ProjXlsx2Protobuf/1xlsx2proto_binaryproto.bat");
        DebugLogger.Instance.DebugLog($"�׶�1��ϣ�");

        //2.����.proto����Cs�ļ�
        RunBatchScript($"{rootPath}/ProjXlsx2Protobuf/2proto2cs_csharpclass.bat");
        DebugLogger.Instance.DebugLog($"�׶�2��ϣ�");

        //3.����Cs�ļ���Configs�ļ���
        RunBatchScript($"{rootPath}/ProjXlsx2Protobuf/3copycs2proj.bat");
        DebugLogger.Instance.DebugLog($"�׶�3��ϣ�");

        //4.����byte�ļ���ProtoBin�ļ��� 
        RunBatchScript($"{rootPath}/ProjXlsx2Protobuf/4copybinary2proj.bat");
        DebugLogger.Instance.DebugLog($"�׶�4��ϣ�");

        //5.��ȡ������ProtoBin�ļ����ļ�
        AssetDatabase.Refresh();
        CsProtoHelper.EncryptProtoBinAssets();

        DebugLogger.Instance.DebugLog($"�������ñ�������ϣ�\n  ·����{Application.dataPath}/{CommonConstParm.ASSET_BUNDLE_CONFIG_CONFIGS}");
    }

    private static void RunBatchScript(string path)
    {
        if (!File.Exists(path))
        {
            DebugLogger.Instance.DebugError($"�������ļ�·�������ڣ�{path}");
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = path,
            WorkingDirectory = Path.GetDirectoryName(path),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardErrorEncoding = Encoding.GetEncoding("GB2312"),
            StandardOutputEncoding = Encoding.GetEncoding("GB2312")
        };

        using (Process process = Process.Start(startInfo))
        {
            process.OutputDataReceived += (sender, args) =>
            {
                if (string.IsNullOrEmpty(args.Data)) return;
                DebugLogger.Instance.DebugLog($"[BAT OUTPUT]{args.Data}");
            };
            process.ErrorDataReceived += (sender, args) =>
            {
                if (string.IsNullOrEmpty(args.Data)) return;
                DebugLogger.Instance.DebugError($"[BAT ERROR]{args.Data}");
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                DebugLogger.Instance.DebugError($"������ִ��ʧ�ܣ������룺{process.ExitCode}");
            }
            else
            {
                DebugLogger.Instance.DebugLog($"������ִ�гɹ�");
            }
        }
    }

}
