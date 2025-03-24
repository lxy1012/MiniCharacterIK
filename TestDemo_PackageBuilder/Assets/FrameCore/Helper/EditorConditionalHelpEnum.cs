using UnityEngine;
public class EditorConditionalHelpEnum
{
    //开发时标签设置

    [Tooltip("在编辑器模式，资源加载模式 <不勾选>为通过AB包方式")]
    public const string IN_EDITOR_NOT_LOAD_BY_AB = "IN_EDITOR_LOAD_BY_RESOURCE";
    [Tooltip("在编辑器模式<不勾选>关闭Debug日志功能")]
    public const string DEBUG_LOG_OPEN = "DEBUG_LOG_OPEN";
    [Tooltip("打包时，<不勾选>AssetBundleConfig不在GameAssets/Json目录下导出Json表")]
    public const string JSON_VIEW_OPEN = "JSON_VIEW_OPEN";

}
