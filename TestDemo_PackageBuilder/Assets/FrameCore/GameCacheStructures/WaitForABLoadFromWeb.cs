using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;


public class WaitForABLoadFromWeb : IEnumerator<AssetBundle>
{
    protected private UnityWebRequest request;
    protected AssetBundle assetBundle;
    protected string downloadUrl;
    protected string loadUrl;
    protected int state = 0; //0:未开始，1：加载中，2：完成
    public WaitForABLoadFromWeb(string downloadUrl, string loadUrl)
    {
        this.downloadUrl = downloadUrl;
        this.loadUrl = loadUrl;
    }

    AssetBundle IEnumerator<AssetBundle>.Current => Current as AssetBundle;
    public object Current
    {
        get
        {
            if (state != 2)
                return null;
            else
                return assetBundle;
        }
    }

    public bool MoveNext()
    {
        return CheckState();
    }

    public void Reset()
    {
        assetBundle = null;
        state = 0;
        request?.Dispose();
    }
    public void Dispose()
    {
        assetBundle = null;
        state = 0;
        request?.Dispose();
    }

    protected bool CheckState()
    {
        if (state == 2)
        {
            return false;
        }
        if (state == 0)
        {
            request = new UnityWebRequest(downloadUrl);

            if (File.Exists(loadUrl))
            {
                try
                {
                    File.Delete(loadUrl);
                }
                catch (System.Exception e)
                {
                    DebugLogger.Instance.DebugError($"删除旧AB包文件失败: {e.Message}");
                    return false;
                }
            }

            request.downloadHandler = new DownloadHandlerFile(loadUrl);
            request.SendWebRequest();
            state = 1;
        }


        if (!request.isDone)
        {
            return true;
        }
        else
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                var abName = Path.GetFileName(loadUrl);
                assetBundle = AssetBundleManager.Instance.LoadAssetBundle(abName);
                if (assetBundle == null)
                {
                    DebugLogger.Instance.DebugError($"网络下载完成，保存至：{loadUrl}，但本地文件加载失败");
                }
                else
                {
                    DebugLogger.Instance.DebugLog($"下载完成，保存至：{loadUrl}");
                }
            }
            else
            {
                DebugLogger.Instance.DebugError($"加载AB包失败！net_url：{downloadUrl}    local_url：{loadUrl}  原因：{request.result}");
                request.Dispose();
            }

            state = 2;
            return false;
        }
    }

}
