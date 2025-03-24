using System.Collections.Generic;
using UnityEditor;
public class BuildEditor 
{
  
    /// <summary>
    ///获取激活场景
    /// </summary>
    /// <returns></returns>
    private static string[] GetAcitveScenes()
    {
        List<string> scenes = new List<string>();
        foreach(EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                scenes.Add(scene.path);
            }
        }
        return scenes.ToArray();
    }
}
