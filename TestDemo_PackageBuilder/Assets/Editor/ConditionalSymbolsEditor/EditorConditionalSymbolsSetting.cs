using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;


class EditorConditionalSymbolsSetting : EditorWindow
{
    private BuildTargetGroup targetGroup = BuildTargetGroup.Standalone;
    private Dictionary<string, bool> sysbolStates = new Dictionary<string, bool>();
    private Dictionary<string, bool> states = new Dictionary<string, bool>();
    private Dictionary<string, string> symbolTips = new Dictionary<string, string>();
    EditorConditionalHelpEnum help;
    [MenuItem("Tools/开发平台Conditional标签设置")]
    public static void ShowWindow()
    {
        /* Rect _rect = new Rect(0, 0, 700, 300);
         GetWindowWithRect(typeof(EditorSymbolsSetting), _rect, true, "Define Symbols");*/
        var window = GetWindow<EditorConditionalSymbolsSetting>("Conditional标签设置");
        window.autoRepaintOnSceneChange = true;

    }

    private void OnEnable()
    {
        LoadDefineSymbols();
    }

    private void LoadDefineSymbols()
    {
        if (help == null)
            help = new EditorConditionalHelpEnum();
        string currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        sysbolStates.Clear();
        symbolTips.Clear();
        var field = help.GetType().GetFields();
        TooltipAttribute target;
        foreach (var fieldInfo in field)
        {
            target = null;
            var attr = fieldInfo.GetCustomAttribute(typeof(TooltipAttribute));
            if (attr != null)
            {
                target = (TooltipAttribute)attr;
            }
            symbolTips.Add(fieldInfo.Name, attr == null ? "" : target.tooltip);
            sysbolStates.Add(fieldInfo.Name, currentSymbols.Contains(fieldInfo.Name));
        }

    }



    private void OnGUI()
    {
        targetGroup = (BuildTargetGroup)EditorGUILayout.EnumPopup("Target Platform", targetGroup);

        //显示当前符号
        GUILayout.Label("Current Define Symbols", EditorStyles.boldLabel);
        string currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        EditorGUILayout.TextArea(currentSymbols, GUILayout.Height(50));

        //显示选项
        GUILayout.Label("Custom Symbols", EditorStyles.boldLabel);

        states.Clear();

        foreach (var symbol in sysbolStates.Keys)
        {
            states.Add(symbol, EditorGUILayout.ToggleLeft($"{symbol}：{symbolTips[symbol]}", sysbolStates[symbol]));
            GUILayout.Space(10);
        }
        foreach (var symbol in states.Keys)
        {
            sysbolStates[symbol] = states[symbol];
        }

        //保存按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save", GUILayout.Width(150), GUILayout.Height(30)))
        {
            SaveDefineSysmbols();
        }

        if (GUILayout.Button("Refresh View", GUILayout.Width(150), GUILayout.Height(30)))
        {
            LoadDefineSymbols();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void SaveDefineSysmbols()
    {
        List<string> symbols = new List<string>();
        foreach (var symbol in sysbolStates.Keys)
        {
            if (sysbolStates[symbol])
            {
                symbols.Add(symbol);
            }
        }

        string newsymbols = string.Join(";", symbols);

        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, newsymbols);
        DebugLogger.Instance.DebugLog($"Scriptinf Define Symbols 已更新（{targetGroup} :{newsymbols}）");
    }



}

