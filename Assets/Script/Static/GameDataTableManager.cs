using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.IO;

public class GameDataTableManager : SingletonMono<GameDataTableManager>
{
	GameResourceManager m_ResMgr = null;
	GameDataManager m_DataMgr = null;

	public static string m_URL { get; set; } = string.Empty;

	JObject _JA;
	JToken _versionData;

	public int _nTarDataTableProgressCount = 0;
	public int _nCurDataTableProgressCount = 0;

	private void Awake()
	{
		if (null == m_ResMgr) m_ResMgr = GameResourceManager.Singleton;
		if (null == m_DataMgr) m_DataMgr = GameDataManager.Singleton;

		_nTarDataTableProgressCount = (int)ENTITY_TYPE.END - 1;
	}

	public IEnumerator InitializeVersion()
	{
		if (GameDataManager.Singleton.AbleToConnect())
			StartCoroutine(GetVersion());

		yield return null;
	}

	public void InitializeDataTable()
	{
		StartCoroutine(LoadRoutine());
	}

	private IEnumerator LoadRoutine()
	{
		DateTime now = DateTime.Now;

		yield return m_ResMgr.LoadDataBase();

		GameManager.Log($"datatable initialize time : {DateTime.Now - now}", "green");
	}

	IEnumerator GetVersion()
	{
		using (UnityWebRequest www =
			   UnityWebRequest.Get($"{ComType.GATEKEEPER_URL}{ComType.SERVER_CONTROLLER_DATATABLE_VERSION}"))
		{
			yield return www.SendWebRequest();

			if (www.isDone)
			{
				GameManager.Log($"www.downloadHandler.text : {www.downloadHandler.text}");
				_JA = JObject.Parse(www.downloadHandler.text);

				string title = UIStringTable.IsContainsKey("ui_error_title") ?
							   "ui_error_title" :
							   "Infomation";

				string message = UIStringTable.IsContainsKey("ui_error_require_update") ?
								 "ui_error_require_update" :
								 "Would you like to update the app from the store?";

				string store = UIStringTable.IsContainsKey("ui_store") ?
							   "ui_store" :
							   "Store";

				string exit = UIStringTable.IsContainsKey("ui_popup_default_button_exit") ?
							  "ui_popup_default_button_exit" :
							  "Quit";

				string cancel = UIStringTable.IsContainsKey("ui_popup_default_button_cancel") ?
								"ui_popup_default_button_cancel" : "Cancel";

				if (_JA.TryGetValue(Application.version, out JToken versionData))
				{
					_versionData = versionData;

					if (Application.version == (string)_versionData["LiveVersion"])
						StartCheck();
					else
					{
						GameManager.Log(Application.version);

						PopupDefault move = MenuManager.Singleton.OpenPopup<PopupDefault>(EUIPopup.PopupDefault);
						move.SetTitle(title);
						move.SetMessage(message);
						move.SetButtonText(store, cancel, MoveStore, StartCheck);
					}
				}
				else
				{
					PopupDefault move = MenuManager.Singleton.OpenPopup<PopupDefault>(EUIPopup.PopupDefault);
					move.SetTitle(title);
					move.SetMessage(message);
					move.SetButtonText(store, exit, MoveStore, ExitApp);
				}
			}
			else
				GameManager.Log("request time out", "red");
		}
	}

    void StartCheck()
    {
#if DEBUG
#if UNITY_IOS
		m_URL = (string)_versionData["URL_iOS_Debug"];
#else
        m_URL = ComType.SERVER_URL;
		// m_URL = (string)_versionData["URL"];
		// m_URL = "http://ec2-15-164-210-94.ap-northeast-2.compute.amazonaws.com:29661/";
		// m_URL = (string)_versionData["URL_Debug"];
#endif
#else
#if UNITY_IOS
		m_URL = (string)_versionData["URL_iOS"];
#else
		m_URL = (string)_versionData["URL"];
#endif
#endif
		CheckEnKey();
        FindObjectOfType<PageTitle>().GetDatatable();
        StartCoroutine(CheckDataTableVersion(_versionData));
    }

	void CheckEnKey()
	{
		GameManager.Singleton._enKey = (string)_versionData["key"];
    }

    public void ExitApp()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
	}

	void MoveStore()
	{
#if UNITY_ANDROID
		Application.OpenURL(ComType.MARKET_URL_AOS);
#elif UNITY_IOS
		Application.OpenURL(ComType.MARKET_URL_iOS);
#endif
	}

	IEnumerator CheckDataTableVersion(JToken data)
	{
		for (int i = 1; i < (int)ENTITY_TYPE.END; i++)
		{
			DateTime now = DateTime.Now;

			string filename = Extensions.fileName[i];

			if (null != data[filename])
			{
				int version = (int)data[filename];

				if (version != PlayerPrefs.GetInt($"{filename}_version") ||
					File.Exists($"{Application.persistentDataPath}/{ComType.DATA_PATH}/{filename}.csv"))
					yield return StartCoroutine(GetDateTable(filename, version));
				else
					AddDataTableProgressCount();
			}

			GameResourceManager.Singleton.LoadDataBase((ENTITY_TYPE)i);

			GameManager.Log($"{filename} initialize time : {DateTime.Now - now}", "green");
		}
	}

	IEnumerator GetDateTable(string filename, int version)
	{
		using (UnityWebRequest www =
			   UnityWebRequest.Get($"{m_URL}{ComType.SERVER_CONTROLLER_DATATABLE_DOWNLOAD}/{filename}"))
		{
			yield return www.SendWebRequest();

			if (www.isDone)
			{
				try
				{
                    string directoryPath = $"{Application.persistentDataPath}/{ComType.DATA_PATH}";
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
#if UNITY_IOS
						UnityEngine.iOS.Device.SetNoBackupFlag(directoryPath);
#endif
                    }

                    File.WriteAllBytes($"{directoryPath}/{filename}.csv", www.downloadHandler.data);
                    PlayerPrefs.SetInt($"{filename}_version", version);
                    AddDataTableProgressCount();
                }
				catch (Exception ex)
				{
                    string title = "Infomation";
					string message = "Failed to download the data table\nPlease try again in a monent";
					string button = "Confirm";

                    PopupSysMessage sys = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
					sys.InitializeInfo(title, message, button, GameManager.Singleton.Restart);

					GameManager.Log(ex.ToString());
					PlayerPrefs.DeleteKey($"{filename}_version");
				}
				finally
				{
                    PlayerPrefs.Save();
                }
			}
			else
				GameManager.Log("request time out", "red");
		}
	}

	public void AddDataTableProgressCount()
	{
		_nCurDataTableProgressCount++;
	}

#region 추가
	private System.Action<EMapInfoType, STIDInfo, bool> m_oMapInfoLoadCallback = null;

	/** 맵 정보를 로드한다 */
	public void LoadMapInfos(EMapInfoType a_eMapInfoType, STIDInfo a_stIDInfo, int a_nVer, System.Action<EMapInfoType, STIDInfo, bool> a_oCallback)
	{
		m_oMapInfoLoadCallback = a_oCallback;
		var stIDInfo = new STIDInfo(1, a_stIDInfo.m_nChapterID, a_stIDInfo.m_nEpisodeID);

		string oFilePath = CMapInfoTable.Singleton.GetMapInfoDownloadPath(a_eMapInfoType, stIDInfo);

#if UNITY_EDITOR
		stIDInfo = new STIDInfo(0, a_stIDInfo.m_nChapterID, a_stIDInfo.m_nEpisodeID);
		StartCoroutine(this.CoLoadMapInfo(a_eMapInfoType, oFilePath, (a_bIsSuccess) => this.OnLoadMapInfo(a_eMapInfoType, stIDInfo, a_nVer, a_bIsSuccess)));
#else
		// 다운로드가 필요 할 경우
		if (!System.IO.File.Exists(oFilePath) || a_nVer != PlayerPrefs.GetInt($"{oFilePath}_version"))
		{
			stIDInfo = new STIDInfo(0, a_stIDInfo.m_nChapterID, a_stIDInfo.m_nEpisodeID);
			StartCoroutine(this.CoLoadMapInfo(a_eMapInfoType, oFilePath, (a_bIsSuccess) => this.OnLoadMapInfo(a_eMapInfoType, stIDInfo, a_nVer, a_bIsSuccess)));
		}
		else
		{
			this.ExLateCallFunc((a_oSender) => this.OnLoadMapInfo(a_eMapInfoType, a_stIDInfo, a_nVer, true));
		}
#endif // #if UNITY_EDITOR
	}

	/** 맵 정보를 로드했을 경우 */
	private void OnLoadMapInfo(EMapInfoType a_eMapInfoType, STIDInfo a_stIDInfo, int a_nVer, bool a_bIsSuccess)
	{
		// 맵 정보 로드에 성공했을 경우
		if (a_bIsSuccess)
		{
			var stIDInfo = new STIDInfo(1, a_stIDInfo.m_nChapterID + 1, a_stIDInfo.m_nEpisodeID + 1);
			string oFilePath = CMapInfoTable.Singleton.GetMapInfoDownloadPath(a_eMapInfoType, stIDInfo);

			PlayerPrefs.SetInt($"{oFilePath}_version", a_nVer);
			PlayerPrefs.Save();
		}

		m_oMapInfoLoadCallback?.Invoke(a_eMapInfoType, a_stIDInfo, a_bIsSuccess);
		m_oMapInfoLoadCallback = null;
	}

	/** 맵 정보를 로드한다 */
	private IEnumerator CoLoadMapInfo(EMapInfoType a_eMapInfoType, string a_oFilePath, System.Action<bool> a_oCallback)
	{
		string oMapInfoURL = string.Format("{0}{1}/?type={2}&&value={3}",
			m_URL, ComType.SERVER_CONTROLLER_MAP_DOWNLOAD, a_eMapInfoType, Path.GetFileNameWithoutExtension(a_oFilePath));
			
		using (UnityWebRequest www = UnityWebRequest.Get(oMapInfoURL))
		{
			www.timeout = GlobalTable.GetData<int>("timeRequest");
			yield return www.SendWebRequest();

			if (www.result == UnityWebRequest.Result.Success)
			{
				string oMapInfoDownloadDirPath = string.Format("{0}{1}/", 
					CMapInfoTable.Singleton.MapInfoDownloadDirPath, a_eMapInfoType);

				// 디렉토리가 없을 경우
				if (!System.IO.Directory.Exists(oMapInfoDownloadDirPath))
				{
					System.IO.Directory.CreateDirectory(oMapInfoDownloadDirPath);

#if UNITY_IOS
					UnityEngine.iOS.Device.SetNoBackupFlag(oMapInfoDownloadDirPath);
#endif
				}

				string oFileName = CMapInfoTable.Singleton.GetMapInfoPath(a_eMapInfoType, Path.GetFileNameWithoutExtension(a_oFilePath));
				System.IO.File.WriteAllBytes(CMapInfoTable.Singleton.GetMapInfoDownloadPath(oFileName), www.downloadHandler.data);

				a_oCallback?.Invoke(true);
			}
			else
			{
				GameManager.Log("request time out", "red");
				a_oCallback?.Invoke(false);
			}
		}
	}
#endregion // 추가
}
