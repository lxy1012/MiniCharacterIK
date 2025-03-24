using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class DataAssetsGenerator : EditorWindow
{
    [MenuItem("Tools/打包/1.更新游戏配置表数据")]
    [Tooltip("生成所有二进制文件，游戏运行必须！")]
    public static void GenerateBinFiles()
    {
        string rootPath = Path.GetDirectoryName(Application.dataPath);

        //1.根据xlsx-> bin + proto文件
        RunBatchScript($"{rootPath}/ProjXlsx2Protobuf/1xlsx2proto_binaryproto.bat");
        DebugLogger.Instance.DebugLog($"阶段1完毕！");

        //2.根据.proto生成Cs文件
        RunBatchScript($"{rootPath}/ProjXlsx2Protobuf/2proto2cs_csharpclass.bat");
        DebugLogger.Instance.DebugLog($"阶段2完毕！");

        //3.导入Cs文件到Configs文件夹
        RunBatchScript($"{rootPath}/ProjXlsx2Protobuf/3copycs2proj.bat");
        DebugLogger.Instance.DebugLog($"阶段3完毕！");

        //4.导入byte文件到ProtoBin文件夹 
        RunBatchScript($"{rootPath}/ProjXlsx2Protobuf/4copybinary2proj.bat");
        DebugLogger.Instance.DebugLog($"阶段4完毕！");

        //5.读取并加密ProtoBin文件夹文件
        AssetDatabase.Refresh();
        CsProtoHelper.EncryptProtoBinAssets();

        DebugLogger.Instance.DebugLog($"更新配置表数据完毕！\n  路径：{Application.dataPath}/{CommonConstParm.ASSET_BUNDLE_CONFIG_CONFIGS}");
    }

    private static void RunBatchScript(string path)
    {
        if (!File.Exists(path))
        {
            DebugLogger.Instance.DebugError($"批处理文件路径不存在：{path}");
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
                DebugLogger.Instance.DebugError($"批处理执行失败，错误码：{process.ExitCode}");
            }
            else
            {
                DebugLogger.Instance.DebugLog($"批处理执行成功");
            }
        }
    }

}
