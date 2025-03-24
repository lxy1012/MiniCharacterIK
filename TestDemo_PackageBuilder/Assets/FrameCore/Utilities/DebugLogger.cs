using System;
using System.Diagnostics;
using System.Text;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class DebugLogger : Singletion<DebugLogger>
{
    private static StringBuilder stringBuilder = new StringBuilder();

    [Conditional(EditorConditionalHelpEnum.DEBUG_LOG_OPEN)]
    public void DebugLog(string message)
    {
        stringBuilder?.Clear();
        stringBuilder?.Append(message);
        UnityEngine.Debug.Log(stringBuilder.ToString());
    }


    [Conditional(EditorConditionalHelpEnum.DEBUG_LOG_OPEN)]
    public void DebugLog(int message)
    {
        stringBuilder.Clear();
        stringBuilder.Append(message);
        UnityEngine.Debug.Log(stringBuilder.ToString());
    }

    [Conditional(EditorConditionalHelpEnum.DEBUG_LOG_OPEN)]
    public void DebugWarn(string message)
    {
        stringBuilder.Clear();
        stringBuilder.Append(message);
        UnityEngine.Debug.LogWarning(stringBuilder.ToString());
    }


    [Conditional(EditorConditionalHelpEnum.DEBUG_LOG_OPEN)]
    public void DebugWarn(int message)
    {
        stringBuilder.Clear();
        stringBuilder.Append(message);
        UnityEngine.Debug.LogWarning(stringBuilder.ToString());
    }

    [Conditional(EditorConditionalHelpEnum.DEBUG_LOG_OPEN)]
    public void DebugError(string message)
    {
        stringBuilder.Clear();
        stringBuilder.Append(message);
        UnityEngine.Debug.LogError(stringBuilder.ToString());
    }

    [Conditional(EditorConditionalHelpEnum.DEBUG_LOG_OPEN)]
    public void DebugError(int message)
    {
        stringBuilder.Clear();
        stringBuilder.Append(message);
        UnityEngine.Debug.LogError(stringBuilder.ToString());
    }

    /*    [Conditional("DEBUG_LOG_OPEN")]
        public static void DebugLogPositioning(string message, System.Object target)
        {
            string str = $"<color=#B55AB5>{message}</color>";
            UnityEngine.Debug.LogFormat("() at");
        }*/
}
