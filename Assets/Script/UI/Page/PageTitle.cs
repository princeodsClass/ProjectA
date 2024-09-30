using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using System.IO;
using DG.Tweening;

#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif // #if UNITY_IOS

public class PageTitle : UIDialog
{
	[SerializeField]
	GameObject _BG, _goStarter, _goButton;

	[SerializeField]
	GameObject _Progress;

	[SerializeField]
	Slider _Slider;

	[SerializeField]
	GameObject _CI;

	[SerializeField]
	Animator _animator;

	[SerializeField]
	TextMeshProUGUI _txtStarterSkip, _txtStarterContinue, _txtVersionValue, _txtIndicator;

	[SerializeField]
	GameObject[] _Indicator;

	public enum TitleSequence
	{
		Entrance_CI = 0,
		Permission,
		DataProgress,
		Sign,
		GetInventoryInfo,
		Initialize3Party,
		Ready,
		END,
	}

	static TitleSequence titleSequence = TitleSequence.Entrance_CI;

    private void Update()
    {
        for ( int i = 0; i < _Indicator.Length; i++ )
			_Indicator[i].SetActive(i <= (int)titleSequence);

		if ( _Progress.activeSelf )
        {
			switch (titleSequence)
			{
				case TitleSequence.Entrance_CI:
					_txtIndicator.text = "Initialize Application";
					break;
				case TitleSequence.Permission:
					_txtIndicator.text = "Permission";
					break;
				case TitleSequence.DataProgress:
					_txtIndicator.text = "Download & Load Datatable";
					break;
				case TitleSequence.Sign:
					_txtIndicator.text = "Signing in";
					break;
				case TitleSequence.GetInventoryInfo:
					_txtIndicator.text = "Loading Account Infomation";
					break;
				case TitleSequence.Initialize3Party:
                    _txtIndicator.text = "Initializing Base";
                    break;
                case TitleSequence.Ready:
					_txtIndicator.text = "Complete Initialize Application";
					break;
			}
		}
    }

    private void Awake()
	{
		base.Initialize();
		DOTween.SetTweensCapacity(byte.MaxValue, sbyte.MaxValue);
	}

	private void OnEnable()
	{
		GameAudioManager.ChangeAudioMixSnapShot(MenuManager.Singleton.CurScene.ToString());
		GameAudioManager.PlayBGM(ESceneType.Title);

		_goStarter.SetActive(false);

		m_GameMgr._gameState = GameManager.GameState.title;
		titleSequence = TitleSequence.Entrance_CI;
		_Slider.value = 0;

		TitleProcess(TitleSequence.Entrance_CI);
	}

	public static void SetTitleSequence(TitleSequence sequence)
	{
		titleSequence = sequence;
	}

	public static TitleSequence GetTitleSequence()
	{
		return titleSequence;
	}

	public void OnClickBG()
    {
		PopupSysMessage popWaitPlease = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
		popWaitPlease.InitializeInfo("Infomation", "Please wait a moment until the loading is complete", "OK");
	}

	public void OnClick()
	{
		PlayerPrefs.SetInt(ComType.G_KEY_TUTORIAL_STEP, (int)ETutorialStep.NONE);
		PlayerPrefs.Save();

		m_MenuMgr.SceneEnd();
		m_GameMgr.EventStart();

		// 타이틀 씬이 아닐 경우
		if (UIRootBase.FirstAwakeSceneType != ESceneType.Title)
		{
			m_MenuMgr.SceneNext(UIRootBase.FirstAwakeSceneType);
		}
		else
		{
			m_MenuMgr.SceneNext(ESceneType.Lobby);
		}
	}

	public void OnClickTutorialSkip()
	{
		PopupDefault tu = m_MenuMgr.OpenPopup<PopupDefault>(EUIPopup.PopupDefault, true);
		tu.SetTitle("ui_error_title");
		tu.SetMessage("ui_popup_tutorial_skip_desc");
		tu.SetButtonText("ui_popup_tutorial_skip_button_cap_tutorial", "ui_popup_tutorial_skip_button_cap_continue",
						 OnClickTutorialGo, OnClick);
	}

	public void OnClickTutorialGo()
	{
		m_GameMgr.EventStart();

#if DISABLE_THIS
		PlayerPrefs.SetInt(ComType.G_KEY_TUTORIAL_STEP, (int)ETutorialStep.NPC_LOCK_ON);
		PlayerPrefs.Save();
#endif // #if DISABLE_THIS

		CMapInfoTable.Singleton.LoadMapInfos(EMapInfoType.TUTORIAL);
		var oMapInfoDict = CMapInfoTable.Singleton.GetChapterMapInfos(EMapInfoType.TUTORIAL, 0, 0);

		GameDataManager.Singleton.SetPlayStageID(0);
		GameDataManager.Singleton.SetContinueTimes(0);
		GameDataManager.Singleton.SetupPlayMapInfo(EMapInfoType.TUTORIAL, EPlayMode.TUTORIAL, oMapInfoDict);

		MenuManager.Singleton.SceneEnd();
		MenuManager.Singleton.SceneNext(ESceneType.Battle);
	}

	public void ContinueProgress()
	{
		StartCoroutine(GameDataTableManager.Singleton.InitializeVersion());
	}

	void SetStarter()
	{
		PlayerPrefs.SetString("isNew", "First");
		//PlayerPrefs.DeleteKey("isNew");

		_txtStarterSkip.text = UIStringTable.GetValue("ui_popup_boxreward_button_skip");
		_txtStarterContinue.text = UIStringTable.GetValue("ui_popup_tutorial_skip_button_cap_tutorial");

		// 일단 빼기
		_goStarter.SetActive(false); //  PlayerPrefs.GetString("isNew") == "true");
		_goButton.GetComponent<SoundButton>().interactable = !_goStarter.activeSelf;
	}

	public void CompleteCI()
	{
		if ( false == m_DataMgr.AbleToConnect() ) return;

		if ( ComUtil.CheckOS() == EOSType.iOS )
        {
			string title = string.Empty;
			string desc = string.Empty;
			string captionPositive = string.Empty;
			string captionNegative = string.Empty;

			switch ((ELanguage)PlayerPrefs.GetInt(ComType.STORAGE_LANGUAGE_INT))
			{
				case ELanguage.Korean:
					title = "알림";
					desc = "게임 시작을 위해 최대 <color=yellow>2.5MB</color> 정보 파일 다운로드가 필요합니다\n\n다운로드 후 게임을 시작하시겠습니까?";
					captionPositive = "시작";
					captionNegative = "종료";
					break;
				default:
					title = "infomation";
					desc = "To Start, need to download about <color=yellow>2.5MB</color> addtional file\n\nStart the game after download?";
					captionPositive = "START";
					captionNegative = "EXIT";
					break;
			}

			PopupDefault pop = m_MenuMgr.OpenPopup<PopupDefault>(EUIPopup.PopupDefault, true);
			pop.SetTitle(title);
			pop.SetMessage(desc);
			pop.SetButtonText(captionPositive, captionNegative, StartInitialize, () =>
			{
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
			});
		}
		else
        {
			StartInitialize();
		}
	}

	void StartInitialize()
	{
		StartCoroutine(GameDataTableManager.Singleton.InitializeVersion());
	}

	public void GetDatatable()
	{
		TitleProcess(TitleSequence.DataProgress);
		StartCoroutine(CheckDataProgress());

		_animator.SetTrigger("Start");
	}

	/** 약관 동의 팝업을 출력한다 */
	private void ShowAgreePopup()
	{
		var oPopupAgree = m_MenuMgr.OpenPopup<PopupAgree>(EUIPopup.PopupAgree);
		oPopupAgree.Init(PopupAgree.MakeParams(this.OnReceiveAgreeResult));
	}

	/** 추적 동의 팝업이 닫혔을 경우 */
	private void OnCloseTracikingPopup(bool a_bIsSuccess)
	{
		Initialize3Party();

		PlayerPrefs.SetInt("is_enable_show_tracking_popup", 1);
		PlayerPrefs.Save();

#if DISABLE_THIS
		TitleProcess(TitleSequence.DataProgress);

		StartCoroutine(CheckDataProgress());
		Initialize3Party();

		_animator.SetTrigger("Start");
#endif // #if DISABLE_THIS
	}

	/** 추적 설명 팝업 결과를 수신했을 경우 */
	private void OnReceiveTrackingDescResult(PopupTrackingDesc a_oSender)
	{
		a_oSender?.Close();
		StartCoroutine(this.HandleOnReceiveTrackingDescResult());
	}

	private IEnumerator HandleOnReceiveTrackingDescResult()
	{
		yield return null;

#if !UNITY_EDITOR && UNITY_IOS
		ATTrackingStatusBinding.RequestAuthorizationTracking();
		var eState = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();

		do
		{
			yield return null;
			eState = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
		} while (eState == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED);

		bool bIsSuccess = eState != ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED;
		this.OnCloseTracikingPopup(bIsSuccess && eState != ATTrackingStatusBinding.AuthorizationTrackingStatus.DENIED);
#else
		this.OnCloseTracikingPopup(true);
#endif // #if !UNITY_EDITOR && UNITY_IOS
	}

	/** 추적 동의 팝업을 출력한다 */
	private IEnumerator ShowTrackingPopup()
	{
		yield return null;

		var oPopupTrackingDesc = m_MenuMgr.OpenPopup<PopupTrackingDesc>(EUIPopup.PopupTrackingDesc);
		oPopupTrackingDesc.Init(PopupTrackingDesc.MakeParams(this.OnReceiveTrackingDescResult));
	}

	/** 약관 동의 결과를 수신했을 경우 */
	private void OnReceiveAgreeResult(PopupAgree a_oSender, bool a_bIsAgree)
	{
		a_oSender?.Close();

		PlayerPrefs.SetInt("is_agreement_privacy_and_services", 1);
		PlayerPrefs.Save();

#if DISABLE_THIS
		m_TableMgr.InitializeVersion();
#endif // #if DISABLE_THIS

#if UNITY_IOS
		bool bIsEnableShowTrackingPopup = PlayerPrefs.GetInt("is_enable_show_tracking_popup") <= 0;

		// 추적 동의가 필요 할 경우
		if (ComUtil.IsNeedsTrackingConsent && bIsEnableShowTrackingPopup)
		{
			StartCoroutine(this.ShowTrackingPopup());
		}
		else
		{
			this.OnCloseTracikingPopup(true);
		}
#else
		this.OnCloseTracikingPopup(true);
#endif // #if UNITY_IOS

#if DISABLE_THIS
		// 기존 구문
		TitleProcess(TitleSequence.DataProgress);

		StartCoroutine(CheckDataProgress());
		Initialize3Party();

		_animator.SetTrigger("Start");
#endif // #if DISABLE_THIS
	}

	void Initialize3Party()
	{
		C3rdPartySDKManager.Singleton.Init();
	}

	IEnumerator CheckDataProgress()
	{
		// while ( _Slider.value < ( 1.0f - float.Epsilon ) )
		// while ( !Mathf.Approximately(_Slider.value, 1.0f) )
		while ( m_TableMgr._nCurDataTableProgressCount < m_TableMgr._nTarDataTableProgressCount )
		{
			_Slider.value = (float)m_TableMgr._nCurDataTableProgressCount / (float)m_TableMgr._nTarDataTableProgressCount;
			yield return new WaitForSeconds(0.5f);
		}

        _Slider.value = (float)m_TableMgr._nCurDataTableProgressCount / (float)m_TableMgr._nTarDataTableProgressCount;
        // yield return StartCoroutine(WaitForInitTableData());
		// _Slider.value = 1.0f;

		yield return StartCoroutine(Sign());
	}

	IEnumerator WaitForInitTableData()
	{
		// m_TableMgr.InitializeDataTable();

		while (!m_ResourceMgr.IsInitTableData)
		{
			yield return new WaitForSeconds(0.2f);
		}
	}

	IEnumerator Sign()
	{
		titleSequence = TitleSequence.Sign;

		yield return PlayerPrefs.GetInt(ComType.STORAGE_UID) == 0 ?
					 StartCoroutine(m_DataMgr.Signup(this)) :
					 StartCoroutine(m_DataMgr.Signin(this));
	}

	public IEnumerator LateSign()
	{
		titleSequence = TitleSequence.GetInventoryInfo;

		yield return StartCoroutine(m_DataMgr.GetInventoryList(EDatabaseType.inventory));
		yield return StartCoroutine(m_DataMgr.GetInventoryList(EDatabaseType.box));
		yield return StartCoroutine(m_DataMgr.GetBattlePassInfo());
		yield return StartCoroutine(m_DataMgr.GetAbyssInfo());
		yield return StartCoroutine(m_DataMgr.GetQuestInfo());
		yield return StartCoroutine(m_DataMgr.GetQuestAccountRewardsInfo());

		if (PlayerPrefs.GetString("isNew") == "true")
		{
			yield return StartCoroutine(m_GameMgr.AccountLevelUp(1));

			SetStarter();
		}

		GameManager.Singleton.user.CalcGearPower();

		InitializeBase();
	}


	void InitializeBase()
	{
		DateTime now = DateTime.Now;
		titleSequence = TitleSequence.Initialize3Party;

        UIRootBase.SetIsInit(true);
        C3rdPartySDKManager.Singleton.LateInit(() =>
		{
			GameManager.Log($"c3 initialize time : {DateTime.Now - now}", "green");
			Ready();
		});
    }

	public void Ready()
	{
		_txtVersionValue.text = $"{UIStringTable.GetValue("ui_version")} : <color=yellow>{Application.version}</color>";

		#region 추가
		string oAppVer = Application.version;
		string oSavedAppVer = PlayerPrefs.GetString("application_version");

		// 버전이 다를 경우
		if (string.IsNullOrEmpty(oSavedAppVer) || oAppVer.Equals(oSavedAppVer))
		{
			// 맵 데이터 디렉토리가 존재 할 경우
			if (Directory.Exists(CMapInfoTable.Singleton.MapInfoDownloadDirPath))
			{
				Directory.Delete(CMapInfoTable.Singleton.MapInfoDownloadDirPath, true);
			}

			PlayerPrefs.GetString("application_version", oAppVer);
			PlayerPrefs.Save();
		}

		ComUtil.DestroyChildren(GameObject.Find(ComType.UI_ROOT_POPUP).transform);

		// 약관 동의가 필요 할 경우
		if (PlayerPrefs.GetInt("is_agreement_privacy_and_services") <= 0)
		{
			this.ShowAgreePopup();
		}
		else
		{
			this.OnReceiveAgreeResult(null, true);
		}
		#endregion // 추가

		titleSequence = TitleSequence.Ready;
		TitleProcess(TitleSequence.Ready);
	}

	void TitleProcess(TitleSequence value)
	{
		_CI.gameObject.SetActive(value == TitleSequence.Entrance_CI);
		_Progress.gameObject.SetActive((int)value >= (int)TitleSequence.DataProgress &&
									   (int)value < (int)TitleSequence.Ready);
		_BG.gameObject.SetActive(value == TitleSequence.Ready);
	}
}
