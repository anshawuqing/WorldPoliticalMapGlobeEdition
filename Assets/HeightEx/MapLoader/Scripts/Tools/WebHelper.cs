using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Networking;
using WPM;

public class WebHelper : MonoBehaviour
{
    public static Queue<LoadInfo> loadDic = new Queue<LoadInfo>();

    public static WebHelper instance;

     void Awake()
     {
         instance = this;
     }

    //读取资源并存入本地
    public void GetAsset(string url, string filePath, Action<TileInfo> callback, TileInfo info)
    {
        //创建路径
        FileInfo fileInfo = new FileInfo(filePath);
        if (!fileInfo.Directory.Exists)
        {
            fileInfo.Directory.Create();
        }

        LoadInfo loadInfo = new LoadInfo()
        {
            url = url,
            filePath = filePath,
            callback = callback,
            info = info
        };
        loadDic.Enqueue(loadInfo);
        StartLoad();
    }

    public  void StartLoad()
    {
        StartCoroutine(DoLoad());
    }
    public IEnumerator DoLoad()
    {
        if (loadDic.Count <= 0) yield break;
        LoadInfo info = loadDic.Dequeue();

        UnityWebRequest request = new UnityWebRequest(info.url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        request.downloadHandler = texDl;
        yield return request.SendWebRequest();
        if (request.isHttpError||request.isNetworkError)
        {
            Debug.Log(request.error);
            yield break;
        }
        //从网络资源中返回数据流
        using (var writer = new FileStream(info.filePath, FileMode.OpenOrCreate, FileAccess.Write))
        {
            writer.Write(texDl.data, 0, texDl.data.Length);
        }

        if (info.callback != null)
        {
            info.callback(info.info);
        }
    }

    //加载本地图片
    public static bool LoadAsset(string path, ref Texture2D texture2D)
    {
        //判断图片路径是否存在
        if (!File.Exists(path))
        {
            return false;
        }

        //使用流的方式加载本地图片
        using (var fileStream = new FileStream(path, FileMode.Open))
        {
            int length = (int) fileStream.Length;
            byte[] bytes = new byte[length];
            var len = fileStream.Read(bytes, 0, length);
            if (len == length)
            {
                bool result = ImageConversion.LoadImage(texture2D, bytes);
                return result;
            }
        }

        return false;
    }

    public static string GetWebUrl(TileInfo ti)
    {
        string url =
            string.Format("http://mt2.google.cn/vt/lyrs=t&hl=zh-CN&gl=CN&src=app&x={1}&y={2}&z={0}&s=G",
                ti.zoomLevel, ti.x, ti.y);
        return url;
    }
    public static string GetLocalFilePathForUrl( TileInfo ti)
    {
        string url = GetWebUrl(ti);
        string filePath = Application.persistentDataPath + "/HeightCache";
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        filePath += "/z" + ti.zoomLevel + "_x" + ti.x + "_y" + ti.y + "_" + url.GetHashCode().ToString() + ".jpeg";
        return filePath;
    }
}

public struct LoadInfo
{
    public string url;
    public string filePath;
    public Action<TileInfo> callback;
    public TileInfo info;
}