using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupBattlePause : UIDialog
{
	[SerializeField] private GameObject m_oBaseUIs = null;
	[SerializeField] private GameObject m_oTestUIs = null;
	[SerializeField] private GameObject m_oBuffUIs = null;
	[SerializeField] private List<BuffUIsHandler> m_oBuffUIsHandlerList = new List<BuffUIsHandler>();

	#region 프로퍼티
	public CamDummy CamDummy => this.BattleController.CamDummy.GetComponent<CamDummy>();
	public CameraMove CameraMove => this.BattleController.CameraMove;

	public PageBattle PageBattle => MenuManager.Singleton.GetPage<PageBattle>(EUIPage.PageBattle);
	public BattleController BattleController => this.PageBattle?.BattleController;
	#endregion // 프로퍼티

#if DEBUG || DEVELOPMENT_BUILD
	[SerializeField] private Text m_oCameraFOVText = null;
	[SerializeField] private Text m_oCameraHeightText = null;
	[SerializeField] private Text m_oCameraForwardText = null;
	[SerializeField] private Text m_oCameraDistanceText = null;
	[SerializeField] private Text m_oCameraSmoothTimeText = null;

	[SerializeField] private Slider m_oCameraFOV = null;
	[SerializeField] private Slider m_oCameraHeight = null;
	[SerializeField] private Slider m_oCameraForward = null;
	[SerializeField] private Slider m_oCameraDistance = null;
	[SerializeField] private Slider m_oCameraSmoothTime = null;

	public void OnChangeSliderVal(float a_fVal, bool a_bIsOverride)
	{
		this.BattleController.MainCamera.fieldOfView = m_oCameraFOV.value;

		this.CameraMove._fCamHeight = m_oCameraHeight.value;
		this.CameraMove._fCamForward = m_oCameraForward.value;

		this.CamDummy._fDistance = m_oCameraDistance.value;
		this.CamDummy.ttt = m_oCameraSmoothTime.value;

		this.CamDummy.Update();
		this.CameraMove.LateUpdate();

		m_oCameraFOVText.text = $"{Camera.main.fieldOfView}";
		m_oCameraHeightText.text = $"{m_oCameraHeight.value}";
		m_oCameraForwardText.text = $"{m_oCameraForward.value}";
		m_oCameraDistanceText.text = $"{m_oCameraDistance.value}";
		m_oCameraSmoothTimeText.text = $"{m_oCameraSmoothTime.value}";

		PlayerPrefs.SetInt("IS_OVERRIDE_CAMERA_VALS", a_bIsOverride ? 1 : 0);

		PlayerPrefs.SetFloat("CAMERA_FOV", Camera.main.fieldOfView);
		PlayerPrefs.SetFloat("CAMERA_HEIGHT", this.CameraMove._fCamHeight);
		PlayerPrefs.SetFloat("CAMERA_FORWARD", this.CameraMove._fCamForward);
		PlayerPrefs.SetFloat("CAMERA_DISTANCE", this.CamDummy._fDistance);
		PlayerPrefs.SetFloat("CAMERA_SMOOTH_TIME", this.CamDummy.ttt);

		PlayerPrefs.Save();
	}

	public void OnTouchResetSliderValBtn()
	{
		m_oCameraFOV.SetValueWithoutNotify(GameDataManager.Singleton.PlayMapInfo.m_stCameraInfo.m_fFOV);
		m_oCameraHeight.SetValueWithoutNotify(GameDataManager.Singleton.PlayMapInfo.m_stCameraInfo.m_fHeight);
		m_oCameraForward.SetValueWithoutNotify(GameDataManager.Singleton.PlayMapInfo.m_stCameraInfo.m_fForward);
		m_oCameraDistance.SetValueWithoutNotify(GameDataManager.Singleton.PlayMapInfo.m_stCameraInfo.m_fDistance);
		m_oCameraSmoothTime.SetValueWithoutNotify(GameDataManager.Singleton.PlayMapInfo.m_stCameraInfo.m_fSmoothTime);

		this.OnChangeSliderVal(0.0f, false);
	}
#endif // #if DEBUG || DEVELOPMENT_BUILD

	[SerializeField] protected TextMeshProUGUI _txtTitle = null;
	[SerializeField] protected TextMeshProUGUI _txtOptionButton = null;
	[SerializeField] protected TextMeshProUGUI _txtExitButton = null;
	[SerializeField] protected TextMeshProUGUI _txtContinueButton = null;

	protected Action<int> m_OnCallbackPositive = null;
	protected Action m_OnCallbackNegative = null;

	protected int m_nData = 0;
	private int m_nSiblingIndex = 0;
	private float m_fPrevTimeScale = 0.0f;

	private void OnEnable()
	{
		m_fPrevTimeScale = Time.timeScale;
		StartCoroutine(this.CoStart());

#if DISABLE_THIS
		Time.timeScale = 0f;
#else
		ComUtil.SetTimeScale(0.0f);
#endif // #if DISABLE_THIS

#if DEBUG || DEVELOPMENT_BUILD
		m_oTestUIs.SetActive(true);
		bool bIsOverride = PlayerPrefs.GetInt("IS_OVERRIDE_CAMERA_VALS", 0) != 0;

		// 오버라이드 일 경우
		if (bIsOverride)
		{
			m_oCameraFOV.SetValueWithoutNotify(PlayerPrefs.GetFloat("CAMERA_FOV", Camera.main.fieldOfView));
			m_oCameraHeight.SetValueWithoutNotify(PlayerPrefs.GetFloat("CAMERA_HEIGHT", this.CameraMove._fCamHeight));
			m_oCameraForward.SetValueWithoutNotify(PlayerPrefs.GetFloat("CAMERA_FORWARD", this.CameraMove._fCamForward));
			m_oCameraDistance.SetValueWithoutNotify(PlayerPrefs.GetFloat("CAMERA_DISTANCE", this.CamDummy._fDistance));
			m_oCameraSmoothTime.SetValueWithoutNotify(PlayerPrefs.GetFloat("CAMERA_SMOOTH_TIME", this.CamDummy.ttt));
		}
		else
		{
			m_oCameraFOV.SetValueWithoutNotify(Camera.main.fieldOfView);
			m_oCameraHeight.SetValueWithoutNotify(this.CameraMove._fCamHeight);
			m_oCameraForward.SetValueWithoutNotify(this.CameraMove._fCamForward);
			m_oCameraDistance.SetValueWithoutNotify(this.CamDummy._fDistance);
			m_oCameraSmoothTime.SetValueWithoutNotify(this.CamDummy.ttt);
		}

		m_oCameraFOV.onValueChanged.AddListener((a_fVal) => this.OnChangeSliderVal(a_fVal, true));
		m_oCameraHeight.onValueChanged.AddListener((a_fVal) => this.OnChangeSliderVal(a_fVal, true));
		m_oCameraForward.onValueChanged.AddListener((a_fVal) => this.OnChangeSliderVal(a_fVal, true));
		m_oCameraDistance.onValueChanged.AddListener((a_fVal) => this.OnChangeSliderVal(a_fVal, true));
		m_oCameraSmoothTime.onValueChanged.AddListener((a_fVal) => this.OnChangeSliderVal(a_fVal, true));

		this.OnChangeSliderVal(0.0f, bIsOverride);
#else
		m_oTestUIs.SetActive(false);
#endif // #if DEBUG || DEVELOPMENT_BUILD
	}

	private void OnDisable()
	{
		RecoverTimeScale();
	}

	void RecoverTimeScale()
	{
#if DISABLE_THIS
		Time.timeScale = 1f;
#else
		ComUtil.SetTimeScale(Mathf.Max(Time.timeScale, m_fPrevTimeScale));
#endif // #if DISABLE_THIS
	}

	private void Awake()
	{
		Initialize();
	}

	public override void Open()
	{
		base.Open();

		_txtTitle.text = UIStringTable.GetValue("ui_pause");
		_txtOptionButton.text = UIStringTable.GetValue("ui_popup_button_option");
		_txtExitButton.text = UIStringTable.GetValue("ui_popup_button_exit");
		_txtContinueButton.text = UIStringTable.GetValue("ui_popup_button_continue");
	}

	public override void Close()
	{
		RecoverTimeScale();

		base.Close();

		m_MenuMgr.ShowPopupDimmed(false);
	}

	public override void Escape()
	{
		base.Escape();
	}

	public void OnClickOption()
	{
		PopupOption option = m_MenuMgr.OpenPopup<PopupOption>(EUIPopup.PopupOption, true);
	}

	public void OnClickExit()
	{
		if ( m_DataMgr.PlayMode == EPlayMode.TUTORIAL )
		{
            PopupDefault pop = m_MenuMgr.OpenPopup<PopupDefault>(EUIPopup.PopupDefault, true);
            pop.SetTitle("ui_error_title");
            pop.SetMessage("ui_hint_exit_tutorial");
            pop.SetButtonText("ui_popup_button_next", "ui_popup_button_skip", OnClickContinue, ExitNow);
        }
		else
		{
			ExitNow();
        }
    }

	void ExitNow()
	{
		StartCoroutine(this.CoExit());

#if DISABLE_THIS
		RecoverTimeScale();

		m_MenuMgr.SceneEnd();
		m_MenuMgr.SceneNext(ESceneType.Lobby);
#endif // #if DISABLE_THIS
	}

	private IEnumerator CoExit()
	{
		var wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);
		yield return null;

		// 테스트 모드가 아닐 경우
		if (GameDataManager.Singleton.PlayMode != EPlayMode.TEST)
		{
			float fRate = ComUtil.GetGoldenPointRate();
			GameManager.Singleton.user.IncreaseBonusPoint(this.BattleController.GoldenPoint * fRate);

			yield return YieldInstructionCache.WaitForSecondsRealtime(0.5f);
		}

		switch (GameDataManager.Singleton.PlayMode)
		{
			case EPlayMode.CAMPAIGN:
			case EPlayMode.TUTORIAL:
				yield return GameManager.Singleton.user.ClearCampaign(GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nEpisodeID,
																	  GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nChapterID,
                                                                      GameDataManager.Singleton.PlayStageID,
                                                                      ECompleteMapType.Exit,
																	  GameDataManager.Singleton.PlayMode == EPlayMode.TUTORIAL);

				break;
			case EPlayMode.HUNT:
				var oHuntTable = HuntTable.GetHuntData(GameDataManager.Singleton.PlayHuntLV);
				yield return GameManager.Singleton.user.ClearHuntLevel(oHuntTable.PrimaryKey, ECompleteMapType.Exit);

				break;
			case EPlayMode.ADVENTURE:
				yield return GameDataManager.Singleton.EndAdventureChapter(ECompleteMapType.Exit);
				break;
			case EPlayMode.ABYSS:
				uint nAbyssTableKey = ComUtil.GetAbyssKey(GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nEpisodeID,
					GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nChapterID + 1);

				yield return GameManager.Singleton.user.ClearAbyss(nAbyssTableKey,
					ECompleteMapType.Exit, Mathf.CeilToInt(this.BattleController.SkipPlayTime / ComType.G_UNIT_MS_TO_S));

				break;
		}

		wait.Close();
		RecoverTimeScale();

		PlayerPrefs.SetInt(ComType.G_KEY_TUTORIAL_STEP, (int)ETutorialStep.NONE);
		PlayerPrefs.Save();

		switch (GameDataManager.Singleton.PlayMode)
		{
			case EPlayMode.ADVENTURE:
				MenuManager.Singleton.IsClearMissionAdventure = false;
				MenuManager.Singleton.IsMoveToMissionAdventure = true;

				break;
			case EPlayMode.ABYSS: MenuManager.Singleton.IsMoveToAbyss = true; break;
			case EPlayMode.DEFENCE: yield return this.BattleController.CoFinishBattle(false, 1.0f); break;
		}

		// 방어전이 아닐 경우
		if (GameDataManager.Singleton.PlayMode != EPlayMode.DEFENCE)
		{
			m_MenuMgr.SceneEnd();
			m_MenuMgr.SceneNext(ESceneType.Lobby);
		}
	}

	public void OnClickContinue()
	{
		RecoverTimeScale();

		Close();
	}

	private IEnumerator CoStart()
	{
		yield return YieldInstructionCache.WaitForEndOfFrame;

		for (int i = 0; i < m_oBuffUIsHandlerList.Count; ++i)
		{
			bool bIsValid = i < this.PageBattle.BuffEffectTableInfoList.Count;
			m_oBuffUIsHandlerList[i].m_oApplyBuffUIs.SetActive(bIsValid);

			// 버프 효과 정보가 없을 경우
			if (!bIsValid)
			{
				continue;
			}

			m_oBuffUIsHandlerList[i].m_oEmptyUIs.SetActive(!bIsValid);
			m_oBuffUIsHandlerList[i].m_oEmptyText.gameObject.SetActive(!bIsValid);

			m_oBuffUIsHandlerList[i].m_oActiveUIs.SetActive(bIsValid);
			m_oBuffUIsHandlerList[i].m_oDescText.gameObject.SetActive(bIsValid);
			m_oBuffUIsHandlerList[i].m_oNameText.gameObject.SetActive(bIsValid);

			m_oBuffUIsHandlerList[i].m_oDescText.text = string.Empty;
			var oEffectTableInfo = this.PageBattle.BuffEffectTableInfoList[i];

			float fSign = Mathf.Sign(oEffectTableInfo.Item2);
			float fEnhanceVal = oEffectTableInfo.Item2.ExIsLess(1.0f) ? oEffectTableInfo.Item2 * 100.0f : oEffectTableInfo.Item2;

			string oName = NameTable.GetValue(oEffectTableInfo.Item1.NameKey);
			string oPostfixStr = oEffectTableInfo.Item2.ExIsLess(1.0f) ? "%" : string.Empty;

			string oDescStr = oEffectTableInfo.Item2.ExIsLess(1.0f) ? 
				$"<color=#FFD02A>{fEnhanceVal:0.00}{oPostfixStr}</color>" : $"<color=#FFD02A>{fEnhanceVal}{oPostfixStr}</color>";

			m_oBuffUIsHandlerList[i].m_oIconImg.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Common,
				oEffectTableInfo.Item1.Icon);

			m_oBuffUIsHandlerList[i].m_oNameText.text = oName;
			m_oBuffUIsHandlerList[i].m_oDescText.text = oDescStr;

			LayoutRebuilder.ForceRebuildLayoutImmediate(m_oBuffUIsHandlerList[i].m_oNameText.rectTransform);
			LayoutRebuilder.ForceRebuildLayoutImmediate(m_oBuffUIsHandlerList[i].m_oDescText.rectTransform);

			LayoutRebuilder.ForceRebuildLayoutImmediate(m_oBuffUIsHandlerList[i].m_oDescUIs.transform as RectTransform);
		}

		LayoutRebuilder.MarkLayoutForRebuild(m_oBuffUIs.transform as RectTransform);
		LayoutRebuilder.MarkLayoutForRebuild(m_oBaseUIs.transform as RectTransform);
	}
}
