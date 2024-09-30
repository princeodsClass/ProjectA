using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms;

public static class AssetBundleConfig
{
    public enum BundlePlatform { Android , IOS , UnKnown };

    private static string resourcePath = @"Assets/AssetBundles/";
    private static string patchlistName = "patchlist.bundles";
   
    public static string GetLocalPath() 
    {
        string localPath = Application.persistentDataPath + @"/Assets";
        return localPath;
    }

    public static string GetStreamingPath()
    {
        string localPath = Application.streamingAssetsPath + @"/" + GetPlatformName();
        return localPath;
    }

    public static string GetResourcePath() 
    {
       return resourcePath;
    }

    public static string GetLocalPatchListPath() 
    {
        return GetLocalPath() + @"/" + patchlistName;
    }

    public static string GetStreamingPatchListPath()
    {
        return GetStreamingPath() + @"/" + patchlistName;
    }

    public static string GetRemotePatchListPath(string url,int bundleVersion)
    {
        url = url.TrimEnd('/');
        return url + $"/{GetPlatformName()}_{bundleVersion}/" + patchlistName;
    }

    public static BundlePlatform GetPlatform() 
    {
#if UNITY_ANDROID
        return BundlePlatform.Android;
#elif UNITY_IOS
        return BundlePlatform.IOS;
#else
        return BundlePlatform.UnKnown;
#endif
    }

    public static string GetPlatformName()
    {
        if(GetPlatform() == BundlePlatform.Android) return @"Android";
        if(GetPlatform() == BundlePlatform.IOS) return @"iOS";
        return @"";
    }

    public static AssetBundleMasterFileInfo GetCurMasterFileInfo() 
    {
        AssetBundleMasterFileInfo masterFileInfo = new AssetBundleMasterFileInfo();
        string[] stringLineArray = null;

        stringLineArray = LoadTextLineArray(GetLocalPatchListPath());

        if (IsNullOrEmpty(stringLineArray))
        {
            if (File.Exists(GetLocalPatchListPath()))
                File.Delete(GetLocalPatchListPath());

            GameManager.Log("search streamingPatchList : " + GetStreamingPatchListPath());

            if (!Directory.Exists(AssetBundleConfig.GetLocalPath()))
                Directory.CreateDirectory(AssetBundleConfig.GetLocalPath());

#if UNITY_ANDROID 
            UnityWebRequest wwwfile = UnityWebRequest.Get(GetStreamingPatchListPath());
            wwwfile.SendWebRequest();
            while (!wwwfile.isDone) { }

            if(string.IsNullOrEmpty(wwwfile.error))
            {
                var filepath = System.IO.Path.Combine(AssetBundleConfig.GetLocalPath(), "patchList.text");
                GameManager.Log("localPath : " + filepath + " text : " + wwwfile.downloadHandler.text + " error : " + wwwfile.error);
                File.WriteAllBytes(filepath, wwwfile.downloadHandler.data);
                stringLineArray = File.ReadAllLines(filepath);
            }
#endif

#if UNITY_IOS
            stringLineArray = LoadTextLineArray(GetStreamingPatchListPath());
            if (!IsNullOrEmpty(stringLineArray)) 
            {
                for (int i = 0; i < stringLineArray.Length; ++i) Debug.Log(stringLineArray[i]);
            }
#endif
            if (!IsNullOrEmpty(stringLineArray)) masterFileInfo.Load(stringLineArray, true);

            return masterFileInfo;
        }

        masterFileInfo.Load(stringLineArray);
        return masterFileInfo;
    }
    
    public static bool IsNullOrEmpty(string[] stringArray)
    {
        return stringArray == null || stringArray.Length <= 0;
    }

    private static string[] LoadTextLineArray(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        string[] s = File.ReadAllLines(filePath);

        return s;
    }

    public static Dictionary<string, AssetBundleFileInfo> LoadAssetBundleFileDic(string[] stringArray , bool streamingAssets)
    {
        Dictionary<string, AssetBundleFileInfo> dic = new Dictionary<string, AssetBundleFileInfo>();

        if (stringArray == null || stringArray.Length <= 0)
        {
            return dic;
        }

        Char[] delimiters = { '\t', '\t', '\t', '\t', '\t' , '\t' };
        string key = string.Empty;

        for (int index = 1; index < stringArray.Length; ++index)
        {
            // order : 경로 , 파일이름, 번들이름 , CRC , 사이즈 , stream
            string[] items = stringArray[index].Split(delimiters);

            key = $"{items[0]}/{items[2]}".ToLower();

            dic.Add(key, new AssetBundleFileInfo(items[0], items[1], items[2], Int32.Parse(items[3]), Int32.Parse(items[4]), (items.Length <= 5) ? string.Empty : items[5] , (streamingAssets) ? 1 : ((items.Length <= 6) ? 0 : Int32.Parse(items[6])) ));
        }

        return dic;
    }

    public static string[] EncodeTextLineDic(int bundleVersion , Dictionary<string, AssetBundleFileInfo> dic)
    {
        List<string> stringList = new List<string>();

        stringList.Add($"{bundleVersion}");

        var it = dic.GetEnumerator();

        while(it.MoveNext())
        {
            string item = it.Current.Value[0] + "\t" + it.Current.Value[1] + "\t" + it.Current.Value[2] + "\t" + it.Current.Value[3] + "\t";
            item += it.Current.Value[4] + "\t" + it.Current.Value[5] + "\t" + it.Current.Value[6];
            stringList.Add(item);
        }
        return stringList.ToArray();
    }
}
