using System;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleFileInfo
{
    private string bundlePath;
    private string bundleName;
    private string fileName;
    private int    crc;
    private int    size;
    private string hash;
    private int    stream;
    
    public bool downloadComplete { get; private set; }
    public bool downloading { get; private set; }
    public bool streamingAssets { get { return (stream > 0) ? true : false; } }

    public AssetBundleFileInfo(string bundlePath,string fileName , string bundleName , int crc, int size, string hash , int stream )
    {
        this.bundlePath = bundlePath;
        this.fileName = fileName;
        this.bundleName = bundleName;
        this.crc = crc;
        this.size = size;
        this.hash = hash;
        this.stream = stream;
    }

    public string this[int index]
    {
        get 
        {
            if (index == 0) return bundlePath;
            if (index == 1) return fileName;
            if (index == 2) return bundleName;
            if (index == 3) return $"{crc}";
            if (index == 4) return $"{size}";
            if (index == 5) return hash;
            if (index == 6) return $"{stream}"; 
            return string.Empty;
        }
    }

    public string GetBundlePath()
    {
        if (streamingAssets)
            return $"{AssetBundleConfig.GetStreamingPath()}/{fileName}";

        return $"{AssetBundleConfig.GetLocalPath()}/{fileName}.tfab";
    }
    public string GetBundleRemoteURL(string url,int bundleVersion)
    {
        url = url.TrimEnd('/');
        return $"{url}/{AssetBundleConfig.GetPlatformName()}_{bundleVersion}/{fileName}";
    }
    public string GetBundleKey()
    {
        return $"{bundlePath}/{bundleName}".ToLower();
    }
    public string GetName()
    {
        return fileName;
    }
    public int GetSize() 
    {
        return size;
    }
    public string GetHash()
    {
        return hash;
    }
    public int GetCRC() 
    {
        return crc;
    }
    public void SetDownloadComplete(bool downloadComplete)
    {
        this.downloadComplete = downloadComplete;
    }
    public void SetDownloading(bool downloading)
    {
        this.downloading = downloading;
    }
    public void SetStreamingAssets(bool streamingAssets)
    {
        this.stream = (streamingAssets == true) ? 1 : 0;
    }
    public bool IsEquals(AssetBundleFileInfo info)
    {
        bool check = false;

        check = hash.Equals(info.GetHash());

        if (string.IsNullOrEmpty(info.GetHash()) || string.IsNullOrEmpty(GetHash()))
        {
            check = (info.GetCRC() == GetCRC()) ? true : false;
        }

        if (streamingAssets) return check;
        if(check) check = System.IO.File.Exists(GetBundlePath()) ? true : false;

        return check;
    }
}

public class AssetBundleMasterFileInfo
{
    private int bundleVersion;
    private Dictionary<string, AssetBundleFileInfo> assetBundleFileInfoDic = new Dictionary<string, AssetBundleFileInfo>();
  
    public AssetBundleMasterFileInfo() 
    {
    }

    public int GetBundleListCount() 
    {
        return assetBundleFileInfoDic.Count;
    }

    public int GetBundleVersion() 
    {
        return bundleVersion;
    }

    public Dictionary<string, AssetBundleFileInfo> GetBundleFileInfoDic() 
    {
        return assetBundleFileInfoDic;
    }

    public bool TryGetAssetBundleFileInfo(string bundleKey,out AssetBundleFileInfo fileInfo)
    {
        return assetBundleFileInfoDic.TryGetValue(bundleKey, out fileInfo);
    }

    public void Load(string[] stringLineArray , bool streamingAssets = false)
    {
        int.TryParse(stringLineArray[0], out bundleVersion);
        assetBundleFileInfoDic = AssetBundleConfig.LoadAssetBundleFileDic(stringLineArray, streamingAssets);
    }

    public void Save(string patchListPath) 
    {
        string[] stringLineArray = AssetBundleConfig.EncodeTextLineDic(bundleVersion , assetBundleFileInfoDic);

        if (AssetBundleConfig.IsNullOrEmpty(stringLineArray))
        {
            GameManager.Log("AssetBundleFileInfo - stringLineArray invalid");
            return;
        }

        System.IO.File.WriteAllLines(patchListPath, stringLineArray);
    }
}
