using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Jobs;
using Unity.Collections;
using TMPro;
using DG.Tweening;

public partial class PageBattle : UIDialog
{
	[Header("=====> Page Battle - Etc <=====")]
	[SerializeField] Transform _tWeawponButtonRoot;
	[SerializeField] Color[] _colorWeaponFrame, _colorWeaponGlow;

	private Tween m_oTalkAni = null;

	[Header("=====> Page Battle - UIs <=====")]
	[SerializeField] private TMP_Text m_oTalkText = null;
	[SerializeField] private TMP_Text m_oStageInstLVText = null;

	[SerializeField] private Image m_oTalkAgentImg = null;

	[Header("=====> Page Battle - Game Objects <=====")]
	[SerializeField] private GameObject m_oTopUIs = null;
	[SerializeField] private GameObject m_oBottomUIs = null;

	[SerializeField] private GameObject m_oTalkUIs = null;
	[SerializeField] private GameObject m_oContentsUIs = null;
	[SerializeField] private List<GameObject> m_oContentsUIsList = new List<GameObject>();

	public GameObject BottomUIs => m_oBottomUIs;
	public GameObject ContentsUIs => m_oContentsUIs;

	void Awake()
	{
		base.Initialize();
		m_oBattleInventoryUIs.SetActive(false);

		m_oBattleInventory.gameObject.SetActive(false);

		#region 추가
		m_oCamera = Camera.main;
		m_nBoundsLayerMask = LayerMask.GetMask(ComType.G_LAYER_BOUNDS);
		m_oGradeColorWrapper = this.GetComponentInChildren<CGradeColorWrapper>();
		m_oSightLineColorWrapper = this.GetComponentInChildren<CSightLineColorWrapper>();
		m_nAdsTicketKey = GlobalTable.GetData<uint>("valueAdsRemoveTicketDecKey");

		m_oReloadGuideAni = m_oReloadGuideText.DOFade(0.5f, 1.5f).SetLoops(-1, LoopType.Yoyo);
		CObjsPoolManager.Singleton.Reset();

		m_oTalkUIs.transform.localScale = new Vector3(1.0f, 0.0f, 1.0f);

		string oBoostGuideStrFmt = string.Format(UIStringTable.GetValue("ui_multiple_speed"), 2);
		m_oBoostGuideText.text = oBoostGuideStrFmt;

		this.GaugeBossUIsHandler = m_oBossUIs.GetComponentInChildren<CGaugeBossUIsHandler>();
		this.GaugeAbyssUIsHandler = m_oAbyssUIs.GetComponentInChildren<CGaugeAbyssUIsHandler>();
		this.GaugeInfiniteUIsHandler = m_oInfiniteGaugeUIs.GetComponentInChildren<CGaugeInfiniteUIsHandler>();

		this.GaugeStageUIsHandler = m_oTitleUIs.GetComponentInChildren<CGaugeStageUIsHandler>();
		this.GaugeStageUIsHandler.SetGaugePercent(0.0f);

		this.GaugeCampaignUIsHandler = m_oTitleUIs.GetComponentInChildren<CGaugeCampaignUIsHandler>();
		this.GaugeCampaignUIsHandler.SetGaugePercent(0.0f, 0.0f);

		// 튜토리얼 UI 를 설정한다
		for (int i = 0; i < m_oTutorialUIsRoot.transform.childCount; ++i)
		{
			this.HelpTutorialUIsList.Add(m_oTutorialUIsRoot.transform.GetChild(i).gameObject);
		}

		this.TutorialFinger.SetActive(false);
		this.ShowHelpTutorialUIs(EHelpTutorialKinds.NONE);

#if DEBUG || DEVELOPMENT_BUILD
		m_oTestUIs.SetActive(true);

		this.OnChangeSliderVal(GameDataManager.Singleton.MovingShootSpeedRatio);
		this.OnChangeJoystickSliderVal(GameDataManager.Singleton.MovingShootJoystickRatio);
#else
		m_oTestUIs.SetActive(false);
#endif // #if DEBUG || DEVELOPMENT_BUILD

#if DISABLE_THIS
		m_oTutorialFinger.SetActive(PlayerPrefs.GetInt(ComType.G_KEY_TUTORIAL_STEP) == (int)ETutorialStep.EQUIP_WEAPON_CHANGE);
#endif // #if DISABLE_THIS
		#endregion // 추가
	}

	void Start()
	{
		m_GameMgr._gameState = GameManager.GameState.battle;
		GameAudioManager.PlayBGM(ESceneType.Battle);

		#region 추가
		this.BattleController.InfiniteDescUIs.SetActive(false);
		this.BattleController.InfiniteFocusUIs.SetActive(false);

		this.BattleController.InfiniteDescText.text = string.Empty;
		this.BattleController.InfiniteDescText.gameObject.SetActive(false);

		this.TutorialOverlayUIs.SetActive(false);
		m_oBattleInventory.Init(BattleInventory.MakeParams(this));

		int nNumStages = GameDataManager.Singleton.PlayMapInfoDict.Count;

		float fCurStagePercent = (GameDataManager.Singleton.PlayStageID + 1) / (float)nNumStages;
		float fPrevStagePercent = GameDataManager.Singleton.PlayStageID / (float)nNumStages;

		this.GaugeCampaignUIsHandler.SetGaugePercent(fCurStagePercent, fPrevStagePercent);
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_oBoostAdsUIs.m_oIconUIs.transform.parent as RectTransform);

		// 이미지를 설정한다 {
		m_oDamageImg = GameObject.Find("DamageImg").GetComponent<Image>();
		m_oDamageImg.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
		m_oDamageImg.gameObject.SetActive(true);

		m_oTutorialFocusUIs = GameObject.Find("TutorialFocusImg");
		m_oTutorialFocusUIs.SetActive(false);

		m_oTutorialFocusImg = m_oTutorialFocusUIs.GetComponentInChildren<Image>();
		m_oTutorialFocusImg.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
		m_oTutorialFocusImg.gameObject.SetActive(false);

		var oFocusText = m_oTutorialFocusUIs.GetComponentInChildren<TMP_Text>();
		m_oTutorialFocusAni = oFocusText.DOFade(0.5f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);

		m_oUpBoundsImg.GetComponent<BoxCollider2D>().size = new Vector2((m_oMarkerBounds.transform as RectTransform).rect.size.x, 10.0f);
		m_oDownBoundsImg.GetComponent<BoxCollider2D>().size = new Vector2((m_oMarkerBounds.transform as RectTransform).rect.size.x, 10.0f);

		m_oLeftBoundsImg.GetComponent<BoxCollider2D>().size = new Vector2(10.0f, (m_oMarkerBounds.transform as RectTransform).rect.size.y);
		m_oRightBoundsImg.GetComponent<BoxCollider2D>().size = new Vector2(10.0f, (m_oMarkerBounds.transform as RectTransform).rect.size.y);

		var oCanvas = MenuManager.Singleton.GetUICanvas();
		m_oDamageImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (oCanvas.transform as RectTransform).sizeDelta.y);
		m_oDamageImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (oCanvas.transform as RectTransform).sizeDelta.x);

		m_oTutorialFocusImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (oCanvas.transform as RectTransform).sizeDelta.y);
		m_oTutorialFocusImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (oCanvas.transform as RectTransform).sizeDelta.x);
		// 이미지를 설정한다 }

		// 탐험 일 경우
		if (GameDataManager.Singleton.PlayStageID <= 0 && GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.ADVENTURE)
		{
			uint nTableKey = ComUtil.GetAdventureKey(1, 1);
			MissionAdventureTable oTable = MissionAdventureTable.GetData(nTableKey);

			ComUtil.SetTimeScale(0.00001f, true);
			this.BattleController.SetIsEnableUpdate(false);

			var stParams = PopupBattleBuffRoulette.MakeParams(oTable.AddBonusItemCount,
				(uint)oTable.BonusBuffGroup, this.OnReceiveBattleBuffRoulettePopupCallback);

			var oPopupBattleBuffRoulette = MenuManager.Singleton.OpenPopup<PopupBattleBuffRoulette>(EUIPopup.PopupBattleBuffRoulette);
			oPopupBattleBuffRoulette.Init(stParams);
		}

		// 마커 UI 를 설정한다 {
		var oTransList = CCollectionPoolManager.Singleton.SpawnList<Transform>();

		try
		{
			for (int i = 0; i < this.BattleController.NonPlayerControllerList.Count; ++i)
			{
				var oMarkerUIs = GameResourceManager.Singleton.CreateUIObject(EResourceType.UI_Battle,
					"BattleMarkerUIs", m_oMarkerUIsRoot.transform).GetComponent<CMarkerUIsHandler>();

				oTransList.Add(oMarkerUIs.transform);
				m_oMarkerUIsHandlerList.Add(oMarkerUIs);
			}

			m_oMarkerUIsTransforms = new TransformAccessArray(oTransList.ToArray());
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oTransList);
		}
		// 마커 UI 를 설정한다 }
		#endregion // 추가
	}

	private void OnButtonWeaponSeletClick(int index)
	{
		this.PlayerController.EquipWeapon(index);
	}

	public void OpenOption()
	{
		this.OpenOption(false);
	}

	public void OpenOption(bool a_bIsForceOpenPause)
	{
		// 힌트 연출 상태 일 경우
		if (!this.BattleController.IsEnableHintDirecting || this.BattleController.HintInfoQueue.Count >= 1 || this.BattleController.IsPlaySecneDirecting)
		{
			return;
		}

		if (a_bIsForceOpenPause)
		{
			PopupBattlePause option = m_MenuMgr.OpenPopup<PopupBattlePause>(EUIPopup.PopupBattlePause);
		}
		else
		{
			int nPlayStageID = GameDataManager.Singleton.PlayStageID;

			bool bIsFinishBattle = !this.BattleController.IsMoveToNextStage &&
				this.BattleController.IsOpenNextStagePassage && !GameDataManager.Singleton.PlayMapInfoDict.ContainsKey(nPlayStageID + 1);

			// 전투가 종료되었을 경우
			if (bIsFinishBattle)
			{
				this.BattleController.FinishBattle(true, 1.0f);
			}
			else
			{
				PopupBattlePause option = m_MenuMgr.OpenPopup<PopupBattlePause>(EUIPopup.PopupBattlePause);
			}
		}
	}

	public override void EscapePage()
	{
		OpenOption(true);
	}

	// 터치 시작을 처리한다
	public void HandleOnTouchBegin(CTouchDispatcher a_oSender, PointerEventData a_oEventData)
	{
		// 구동 상태가 아닐 경우
		if (!this.BattleController.IsRunning)
		{
			return;
		}

		m_bIsTouchBegin = true;
		int nPlayEpisodeID = GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nEpisodeID;

		// 초반 에피소드 일 경우
		if (nPlayEpisodeID <= 0)
		{
			PlayerPrefs.SetInt(ComType.HelpTutorialKindsKeyDict[m_eShowedHelpTutorialKinds], 1);
			PlayerPrefs.Save();
		}

		ComUtil.SetTimeScale(1.0f);
		bool bIsActiveTutorialOverlayUIs = m_eShowedHelpTutorialKinds != EHelpTutorialKinds.NONE;

		this.ShowHelpTutorialUIs(EHelpTutorialKinds.NONE);
		this.TutorialOverlayUIs.SetActive(bIsActiveTutorialOverlayUIs);

		ExecuteEvents.Execute<IPointerDownHandler>(m_oJoystick.gameObject, a_oEventData, ExecuteEvents.pointerDownHandler);
	}

	// 터치 이동을 처리한다
	public void HandleOnTouchMove(CTouchDispatcher a_oSender, PointerEventData a_oEventData)
	{
		// 구동 상태가 아닐 경우
		if (!m_bIsTouchBegin || !this.BattleController.IsRunning)
		{
			return;
		}

		ExecuteEvents.Execute<IDragHandler>(m_oJoystick.gameObject, a_oEventData, ExecuteEvents.dragHandler);
	}

	// 터치 종료를 처리한다
	public void HandleOnTouchEnd(CTouchDispatcher a_oSender, PointerEventData a_oEventData)
	{
		// 구동 상태가 아닐 경우
		if (!m_bIsTouchBegin || !this.BattleController.IsRunning)
		{
			return;
		}

		this.TutorialOverlayUIs.SetActive(false);
		this.ShowHelpTutorialUIs(EHelpTutorialKinds.NONE);

		ComUtil.SetTimeScale(1.0f);
		ExecuteEvents.Execute<IPointerUpHandler>(m_oJoystick.gameObject, a_oEventData, ExecuteEvents.pointerUpHandler);
	}
}

/** 전투 페이지 - 추가 */
public partial class PageBattle : UIDialog
{
	/** 정보 UI */
	[System.Serializable]
	private struct STInfoUIs
	{
		public TMP_Text m_oTimeText;
		public TMP_Text m_oNumGoldsText;
		public TMP_Text m_oNumCrystalsText;
	}

	/** 제목 UI */
	[System.Serializable]
	private struct STTitleUIs
	{
		public TMP_Text m_oTitleText;
		public TMP_Text m_oAdventureTitleText;
		
		public List<Image> m_oStageImgList;
		public GameObject m_oAdventureUIs;
	}

	#region 변수
	[Header("=====> Page Battle - Etc <=====")]
	[SerializeField] private FloatingJoystick m_oJoystick = null;

	private bool m_bIsTouchBegin = false;
	private bool m_bIsDirtyUpdateUIsState = true;

	private int m_nBoundsLayerMask = 0;
	private EHelpTutorialKinds m_eShowedHelpTutorialKinds = EHelpTutorialKinds.NONE;
	private TransformAccessArray m_oMarkerUIsTransforms;

	private Tween m_oShakeAni = null;
	private Tween m_oDamageAni = null;
	private Tween m_oReloadGuideAni = null;
	private Tween m_oTutorialFocusAni = null;
	private Tween m_oTutorialBlindAni = null;

	private Camera m_oCamera = null;
	private List<CMarkerUIsHandler> m_oMarkerUIsHandlerList = new List<CMarkerUIsHandler>();
	private List<CMarkerUIsHandler> m_oGateMarkerUIsHandlerList = new List<CMarkerUIsHandler>();
	private List<CMarkerUIsHandler> m_oBoxMarkerUIsHandlerList = new List<CMarkerUIsHandler>();

	[Header("=====> Page Battle - UIs <=====")]
	[SerializeField] private STInfoUIs m_stInfoUIs;
	[SerializeField] private STTitleUIs m_stTitleUIs;

	[SerializeField] private TMP_Text m_oBestStageText = null;
	[SerializeField] private TMP_Text m_oRemainTimeText = null;
	[SerializeField] private TMP_Text m_oReloadGuideText = null;

	[SerializeField] private Image m_oUpBoundsImg = null;
	[SerializeField] private Image m_oDownBoundsImg = null;
	[SerializeField] private Image m_oLeftBoundsImg = null;
	[SerializeField] private Image m_oRightBoundsImg = null;
	[SerializeField] private Image m_oPlayerDirectionMarkerImg = null;

	[SerializeField] private List<BattleBuffUIsHandler> m_oBuffUIsHandlerList = new List<BattleBuffUIsHandler>();

	private Image m_oDamageImg = null;
	private Image m_oTutorialFocusImg = null;
	private CGradeColorWrapper m_oGradeColorWrapper = null;
	private CSightLineColorWrapper m_oSightLineColorWrapper = null;

	[Header("=====> Page Battle - Game Objects <=====")]
	[SerializeField] private GameObject m_oBossUIs = null;
	[SerializeField] private GameObject m_oBonusUIs = null;
	[SerializeField] private GameObject m_oTitleUIs = null;
	[SerializeField] private GameObject m_oStageUIs = null;
	[SerializeField] private GameObject m_oStageGaugeUIs = null;
	[SerializeField] private GameObject m_oDefenceStageGuideUIs = null;

	[SerializeField] private GameObject m_oTimeUIs = null;
	[SerializeField] private GameObject m_oCurrencyUIs = null;
	[SerializeField] private GameObject m_oTutorialUIsRoot = null;
	[SerializeField] private GameObject m_oTutorialOverlayUIs = null;

	[SerializeField] private GameObject m_oAbyssUIs = null;
	[SerializeField] private GameObject m_oMarkerBounds = null;
	[SerializeField] private GameObject m_oMarkerUIsRoot = null;
	[SerializeField] private GameObject m_oTutorialFinger = null;

	[SerializeField] private GameObject m_oPauseBtnUIs = null;
	[SerializeField] private GameObject m_oFinishBtnUIs = null;
	[SerializeField] private GameObject m_oOriginGaugeBonusUIsHandler = null;

	private GameObject m_oTutorialFocusUIs = null;
	#endregion // 변수

	#region 프로퍼티
	public int MaxContinueTimes { get; private set; } = 0;
	public int StandardContinuePrice { get; private set; } = 0;

	public BattleController BattleController { get; private set; } = null;
	public PlayerController PlayerController { get; private set; } = null;
	public CGaugeBossUIsHandler GaugeBossUIsHandler { get; private set; } = null;
	public CGaugeAbyssUIsHandler GaugeAbyssUIsHandler { get; private set; } = null;
	public CGaugeStageUIsHandler GaugeStageUIsHandler { get; private set; } = null;
	public CGaugeCampaignUIsHandler GaugeCampaignUIsHandler { get; private set; } = null;
	public CGaugeInfiniteUIsHandler GaugeInfiniteUIsHandler { get; private set; } = null;

	public List<GameObject> BuffUIsList { get; } = new List<GameObject>();
	public List<GameObject> TalkUIsList { get; } = new List<GameObject>();
	public List<GameObject> HelpTutorialUIsList { get; } = new List<GameObject>();

	public List<(EffectTable, float)> BuffEffectTableInfoList { get; } = new List<(EffectTable, float)>();
	public List<CWeaponUIsHandler> WeaponUIsHandlerList { get; } = new List<CWeaponUIsHandler>();
	public List<CGaugeBonusUIsHandler> GaugeBonusUIsHandlerList { get; } = new List<CGaugeBonusUIsHandler>();

	public EHelpTutorialKinds ShowedHelpTutorialKinds => m_eShowedHelpTutorialKinds;
	public CGradeColorWrapper GradeColorWrapper => m_oGradeColorWrapper;
	public CSightLineColorWrapper SightLineColorWrapper => m_oSightLineColorWrapper;

	public TMP_Text RemainTimeText => m_oRemainTimeText;
	public Image PlayerDirectionMarkerImg => m_oPlayerDirectionMarkerImg;

	public GameObject BossUIs => m_oBossUIs;
	public GameObject BonusUIs => m_oBonusUIs;
	public GameObject TitleUIs => m_oTitleUIs;
	public GameObject StageUIs => m_oStageUIs;
	public GameObject MarkerUIsRoot => m_oMarkerUIsRoot;
	public GameObject TutorialOverlayUIs => m_oTutorialOverlayUIs;
	public GameObject TutorialFinger => m_oTutorialFinger;
	public GameObject OriginGaugeBonusUIsHandler => m_oOriginGaugeBonusUIsHandler;
	#endregion // 프로퍼티

	#region 함수
	/** 제거 되었을 경우 */
	public void OnDestroy()
	{
		ComUtil.SetTimeScale(1.0f);
		m_oMarkerUIsTransforms.Dispose();

		ComUtil.AssignVal(ref m_oTalkAni, null);
		ComUtil.AssignVal(ref m_oShakeAni, null);
		ComUtil.AssignVal(ref m_oDamageAni, null);
		ComUtil.AssignVal(ref m_oReloadGuideAni, null);
		ComUtil.AssignVal(ref m_oTutorialFocusAni, null);
		ComUtil.AssignVal(ref m_oTutorialBlindAni, null);
	}

	/** 상태를 갱신한다 */
	public void LateUpdate()
	{
		// UI 상태 갱신이 필요 할 경우
		if (m_bIsDirtyUpdateUIsState)
		{
			this.SetIsDirtyUpdateUIsState(false, true);
			this.UpdateUIsState();
		}

		this.UpdateMarkerUIsState();
		this.GaugeAbyssUIsHandler.UpdateUIsState();

		int nPlayStageID = GameDataManager.Singleton.PlayStageID;

		bool bIsFinishBattle = !this.BattleController.IsMoveToNextStage &&
			this.BattleController.IsOpenNextStagePassage && !GameDataManager.Singleton.PlayMapInfoDict.ContainsKey(nPlayStageID + 1);

		m_oPauseBtnUIs.SetActive(!bIsFinishBattle);
		m_oFinishBtnUIs.SetActive(bIsFinishBattle);

		// 플레이어 방향 마커 이미지를 갱신한다 {
		var stDirection = new Vector3(m_oJoystick.Horizontal, m_oJoystick.Vertical, 0.0f);
		float fInclination = Mathf.Min(1.0f, stDirection.magnitude);

		var stWorldMarkerPos = this.BattleController.PlayerController.transform.position;
		stWorldMarkerPos += this.BattleController.PlayerController.transform.forward * 1.5f * fInclination;

		var stScreenPos = m_oCamera.WorldToScreenPoint(stWorldMarkerPos);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(MenuManager.Singleton.GetUICanvas().transform as RectTransform, stScreenPos, null, out Vector2 stAnchorPos);

		var oRangeFXHandler = this.PlayerController.RangeFXHandler;
		oRangeFXHandler.gameObject.SetActive(this.PlayerController.IsSurvive && this.PlayerController.StateMachine.IsEnable);

		m_oPlayerDirectionMarkerImg.rectTransform.anchoredPosition = stAnchorPos;
		m_oPlayerDirectionMarkerImg.gameObject.SetActive(!stDirection.magnitude.ExIsEquals(0.0f) && this.PlayerController.StateMachine.IsEnable);
		// 플레이어 방향 마커 이미지를 갱신한다 }
	}

	/** 무기 UI 를 설정한다 */
	public void SetupWeaponUIs()
	{
		for (int i = 0; i < _tWeawponButtonRoot.childCount; ++i)
		{
			var oUIsHandler = _tWeawponButtonRoot.GetChild(i).GetComponent<CWeaponUIsHandler>();

			// UI 처리자가 없을 경우
			if (oUIsHandler == null)
			{
				continue;
			}

			oUIsHandler.Init(new CWeaponUIsHandler.STParams()
			{
				m_nSlotIdx = this.WeaponUIsHandlerList.Count,
				m_oTouchCallback = this.OnReceiveWeaponUIsTouchCallback,
				m_oReloadCallback = this.OnReceiveWeaponUIsReloadCallback
			});

			this.WeaponUIsHandlerList.Add(oUIsHandler);
		}

		bool bIsCampaign = GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.CAMPAIGN;
		bIsCampaign = bIsCampaign || GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.TUTORIAL;

		// 캠페인 일 경우
		if (bIsCampaign && GameDataManager.Singleton.IsContinuePlay && GameDataManager.Singleton.PlayStageID > 0)
		{
			for (int i = 0; i < GameDataManager.Singleton.ReloadInfos.Length; ++i)
			{
				this.PlayerController.ReloadInfoList[i] = GameDataManager.Singleton.ReloadInfos[i];
			}

			for (int i = 0; i < GameDataManager.Singleton.MagazineInfos.Length; ++i)
			{
				this.PlayerController.MagazineInfoList[i] = GameDataManager.Singleton.MagazineInfos[i];
			}

			this.SetIsDirtyUpdateUIsState(true);
		}

		this.BottomUIs.SetActive(false);
		this.WeaponUIsHandlerList[this.PlayerController.CurWeaponIdx].SetState(CWeaponUIsHandler.EState.SEL);
	}

	/** 무기 UI 터치 콜백을 수신했을 경우 */
	public void OnReceiveWeaponUIsTouchCallback(CWeaponUIsHandler a_oSender, int a_nIdx)
	{
		// 무기 장착이 가능 할 경우
		if (this.PlayerController.IsEnableEquipWeapon(a_nIdx))
		{
			int nTutorialStep = PlayerPrefs.GetInt(ComType.G_KEY_TUTORIAL_STEP);

#if DISABLE_THIS
			// 무기 변경 튜토리얼 일 경우
			if (nTutorialStep == (int)ETutorialStep.EQUIP_WEAPON_CHANGE)
			{
				PlayerPrefs.SetInt(ComType.G_KEY_TUTORIAL_STEP, (int)ETutorialStep.TOUCH_INVENTORY_FOR_UPGRADE);
				PlayerPrefs.Save();
			}
#endif // #if DISABLE_THIS

			m_oTutorialFinger.SetActive(false);
			this.PlayerController.EquipWeapon(a_nIdx);
		}

		this.SetIsDirtyUpdateUIsState(true);
	}

	/** 무기 UI 재장전 콜백을 수신했을 경우 */
	public void OnReceiveWeaponUIsReloadCallback(CWeaponUIsHandler a_oSender, int a_nIdx)
	{
		this.PlayerController.ReloadWeapon(a_nIdx);
	}

	/** 이어하기 팝업을 출력한다 */
	public void OpenContinuePopup()
	{
		// 이어하기 팝업 출력이 필요 없을 경우
		if (!this.BattleController.IsKillNonPlayers && GameDataManager.Singleton.PlayStageID <= 0)
		{
			this.OnReceiveContinuePopupCallback(null, false);
		}
		else
		{
			var oPopupBattleContinue = m_MenuMgr.OpenPopup<PopupBattleContinue>(EUIPopup.PopupBattleContinue);
			oPopupBattleContinue.Init(PopupBattleContinue.MakeParams(GameDataManager.Singleton.ContinueTimes, this.MaxContinueTimes, this.StandardContinuePrice, this.OnReceiveContinuePopupCallback));
		}
	}

	/** 버프 룰렛 팝업 콜백을 수신했을 경우 */
	private void OnReceiveBattleBuffRoulettePopupCallback(PopupBattleBuffRoulette a_oSender, List<EffectTable> a_oEffectTableList)
	{
		a_oSender?.Close();
		a_oEffectTableList.ExCopyTo(GameDataManager.Singleton.BuffEffectTableInfoList, (a_oEffectTable) => (a_oEffectTable, a_oEffectTable.Value));

		this.SetIsDirtyUpdateUIsState(true);
		this.BattleController.SetIsEnableUpdate(true);
		this.PlayerController.AddPassiveEffects(a_oEffectTableList);

		for (int i = 0; i < a_oEffectTableList.Count; ++i)
		{
			this.OnReceiveBattleInstEffectPopupResult(null, a_oEffectTableList[i], a_oEffectTableList[i].Value);
		}

		// 보스 스테이지 일 경우
		if (this.BattleController.IsBossStage)
		{
			this.BattleController.StartBossEnterDirecting();
		}
	}

	/** 이어하기 팝업 콜백을 수신했을 경우 */
	private void OnReceiveContinuePopupCallback(PopupBattleContinue a_oSender, bool a_bIsTrue)
	{
		a_oSender?.Close();
		this.SetIsDirtyUpdateUIsState(true);

		// 이어하기를 선택했을 경우
		if (a_bIsTrue)
		{
			this.BattleController.Continue();

			ComUtil.SetTimeScale(m_bIsBoost ? 2.0f : 1.0f, true);
			GameDataManager.Singleton.SetContinueTimes(GameDataManager.Singleton.ContinueTimes + 1);
		}
		else
		{
			this.BattleController.FinishBattle(false, 1.0f);
		}
	}

	/** 도움말 튜토리얼 UI 를 출력한다 */
	public void ShowHelpTutorialUIs(EHelpTutorialKinds a_eKinds)
	{
		string oKey = ComType.HelpTutorialKindsKeyDict[a_eKinds];

		for (int i = 0; i < this.HelpTutorialUIsList.Count; ++i)
		{
			string oName = this.HelpTutorialUIsList[i].name;
			this.HelpTutorialUIsList[i].SetActive(oKey.Equals(oName));
		}

		m_eShowedHelpTutorialKinds = a_eKinds;
		this.TutorialOverlayUIs.SetActive(this.HelpTutorialUIsList.Any((a_oTutorialUIs) => a_oTutorialUIs.activeSelf));

		// 튜토리얼 포커스 UI 가 없을 경우
		if (m_oTutorialFocusUIs == null || m_oTutorialFocusImg == null)
		{
			return;
		}

		ComUtil.AssignVal(ref m_oTutorialBlindAni, null);

		switch (a_eKinds)
		{
			case EHelpTutorialKinds.NPC_HELP_AROUND:
			case EHelpTutorialKinds.NPC_SUMMON_SKILL:
				m_oTutorialBlindAni = m_oTutorialFocusImg.DOColor(Color.black, ComType.G_DURATION_TUTORIAL_FOCUS_ANI).SetEase(Ease.Linear).SetUpdate(true);
				m_oTutorialFocusUIs.SetActive(true);

				break;
			default:
				m_oTutorialFocusImg.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
				m_oTutorialFocusUIs.SetActive(false);

				break;
		}
	}

	/** 데미지 이미지를 출력한다 */
	public void StartDamageFX()
	{
#if DISABLE_THIS
		this.StartCameraShakeDirecting(Vector3.zero);
#endif // #if DISABLE_THIS

		m_oDamageImg.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
		var oColorAni = m_oDamageImg.DOColor(Color.white, ComType.G_DURATION_DAMAGE_ANI);

		ComUtil.AssignVal(ref m_oDamageAni, oColorAni.SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo));
	}

	/** 카메라 진동 연출을 시작한다 */
	public void StartCameraShakeDirecting(Vector3 a_stDirection,
		float a_fDurection = ComType.G_DURATION_CAMERA_SHAKING_ANI, float a_nStrength = ComType.G_STRENGH_CAMERA_SHAKING_ANI)
	{
		Tween oShakeAni = a_stDirection.ExIsEquals(Vector3.zero) ?
			this.BattleController.CamDummy.transform.DOShakePosition(a_fDurection, a_nStrength) : this.BattleController.CamDummy.transform.DOShakePosition(a_fDurection, a_stDirection.normalized * a_nStrength);

		ComUtil.AssignVal(ref m_oShakeAni, oShakeAni.SetEase(Ease.Linear).SetUpdate(true));
	}

	/** UI 상태를 갱신한다 */
	public void UpdateUIsState()
	{
		// 상태 갱신이 불가능 할 경우
		if (m_oReloadGuideText == null || this.PlayerController == null)
		{
			return;
		}

		this.UpdateInstUIsState();
		this.UpdateInfoUIsState();
		this.UpdateTitleUIsState();
		this.UpdateSkillUIsState();
		this.UpdateWeaponUIsState();
		this.UpdateDefenceUIsState();

		switch (GameDataManager.Singleton.PlayMapInfoType)
		{
			case EMapInfoType.ABYSS: this.UpdateTimeUIsStateAbyss(); break;
			case EMapInfoType.BONUS: this.UpdateTimeUIsStateBonus(); break;
		}

		// 객체를 갱신한다 {
		bool bIsEnableBossUIs = this.BattleController.IsBossStage;
		bool bIsEnableBonusUIs = this.BattleController.IsBonusStage;

		bool bIsCampaign = GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.CAMPAIGN;
		bIsCampaign = bIsCampaign || GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.TUTORIAL;

		bool bIsInfiniteUIs = GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.INFINITE;
		bool bIsInfiniteGaugeUIs = bIsCampaign && GameDataManager.Singleton.IsInfiniteWaveMode();
		bool bIsEnableDefenceUIs = GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.DEFENCE;

		bool bIsEnableTitleUIs = !bIsEnableBossUIs;
		bIsEnableTitleUIs = bIsEnableTitleUIs && !bIsEnableBonusUIs;
		bIsEnableTitleUIs = bIsEnableTitleUIs && GameDataManager.Singleton.PlayMapInfoType != EMapInfoType.DEFENCE;

		bool bIsEnableAbyssUIs = !this.BattleController.IsMakeAppearBossNPC;
		bIsEnableAbyssUIs = bIsEnableAbyssUIs && GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.ABYSS;

		bool bIsEnableCurrencyUIs = GameDataManager.Singleton.PlayMapInfoType != EMapInfoType.ABYSS;
		bIsEnableCurrencyUIs = bIsEnableCurrencyUIs && GameDataManager.Singleton.PlayMapInfoType != EMapInfoType.BONUS;
		bIsEnableCurrencyUIs = bIsEnableCurrencyUIs && GameDataManager.Singleton.PlayMapInfoType != EMapInfoType.DEFENCE;

		this.BossUIs.SetActive(bIsEnableBossUIs);
		this.BonusUIs.SetActive(bIsEnableBonusUIs);
		this.TitleUIs.SetActive(bIsEnableTitleUIs && !bIsInfiniteUIs && !bIsInfiniteGaugeUIs);

#if INFINITE_MODE_INST_LEVEL_UP_ENABLE
		bIsEnableTitleUIs = !bIsEnableBossUIs;
		bIsEnableTitleUIs = bIsEnableTitleUIs && !bIsEnableBonusUIs;

		this.StageUIs.SetActive(bIsCampaign);
		this.TitleUIs.SetActive(bIsEnableTitleUIs);

		m_oStageGaugeUIs.SetActive(bIsCampaign || bIsEnableDefenceUIs || bIsInfiniteUIs);

		// 방어전 일 경우
		if (GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.DEFENCE)
		{
			m_oStageGaugeUIs.transform.position = m_oDefenceStageGuideUIs.transform.position;
		}
#endif // #if INFINITE_MODE_INST_LEVEL_UP_ENABLE

		bool bIsAdventure = GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.ADVENTURE;
		m_stTitleUIs.m_oAdventureUIs.SetActive(bIsAdventure);

		m_oTimeUIs.SetActive(bIsEnableBonusUIs || bIsEnableAbyssUIs || bIsEnableDefenceUIs);
		m_oBoostUIs.SetActive(GameDataManager.Singleton.IsWaveMode());

		m_oInfiniteUIs.SetActive(bIsInfiniteUIs && !bIsInfiniteGaugeUIs);
		m_oInfiniteGaugeUIs.SetActive(!bIsInfiniteUIs && bIsInfiniteGaugeUIs);

		m_oAbyssUIs.SetActive(bIsEnableAbyssUIs);
		m_oDefenceUIs.SetActive(bIsEnableDefenceUIs);

#if DISABLE_THIS
		m_oCurrencyUIs.SetActive(bIsEnableCurrencyUIs);
#else
		m_oCurrencyUIs.SetActive(false);
#endif // #if DISABLE_THIS
		// 객체를 갱신한다 }

		for(int i = 0; i < m_oBuffUIsHandlerList.Count; ++i)
		{
			m_oBuffUIsHandlerList[i].gameObject.SetActive(i < this.BuffEffectTableInfoList.Count);

			// 버프 UI 설정이 불가능 할 경우
			if(i >= this.BuffEffectTableInfoList.Count)
			{
				continue;
			}

			m_oBuffUIsHandlerList[i].IconImg.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Common, 
				this.BuffEffectTableInfoList[i].Item1.Icon);
		}
	}

	/** 인스턴스 UI 상태를 갱신한다 */
	private void UpdateInstUIsState()
	{
		bool bIsCampaign = GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.CAMPAIGN;
		bIsCampaign = bIsCampaign || GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.TUTORIAL;

		string oStrLV = UIStringTable.GetValue("ui_level");

		m_oBestStageText.text = $"{GameManager.Singleton.user.m_nStage}";
		m_oBestStageText.gameObject.SetActive(bIsCampaign && GameManager.Singleton.user.m_nStage > 0);

		m_oStageInstLVText.text = $"{oStrLV} {this.PlayerController.StageInstLV}";
	}

	/** 정보 UI 상태를 갱신한다 */
	private void UpdateInfoUIsState()
	{
		// 텍스트를 갱신한다 {
		m_stInfoUIs.m_oNumGoldsText.text = $"{GameManager.Singleton.invenMaterial.CalcTotalMoney()}";
		m_stInfoUIs.m_oNumCrystalsText.text = $"{GameManager.Singleton.invenMaterial.CalcTotalCrystal()}";

		m_oReloadGuideText.gameObject.SetActive(this.PlayerController.ReloadInfoList[this.PlayerController.CurWeaponIdx].m_bIsReload);
		// 텍스트를 갱신한다 }
	}

	/** 심연 시간 UI 상태를 갱신한다 */
	private void UpdateTimeUIsStateAbyss()
	{
		int nTotalSecs = Mathf.FloorToInt(this.BattleController.MaxPlayTime - this.BattleController.RemainPlayTime);
		nTotalSecs = Mathf.Max(0, nTotalSecs);

		m_oRemainTimeText.text = $"{nTotalSecs / 60:00}:{nTotalSecs % 60:00}";
	}

	/** 보너스 시간 UI 상태를 갱신한다 */
	private void UpdateTimeUIsStateBonus()
	{
		int nTotalSecs = Mathf.FloorToInt(this.BattleController.RemainPlayTime);
		nTotalSecs = Mathf.Max(0, nTotalSecs);

		m_oRemainTimeText.text = $"{nTotalSecs / 60:00}:{nTotalSecs % 60:00}";
		m_oRemainTimeText.color = (nTotalSecs <= 5) ? Color.red : Color.white;
	}

	/** 제목 UI 상태를 갱신한다 */
	private void UpdateTitleUIsState()
	{
		string oName = string.Empty;

		switch (GameDataManager.Singleton.PlayMode)
		{
			case EPlayMode.HUNT:
				var oHuntTable = HuntTable.GetHuntData(GameDataManager.Singleton.PlayHuntLV);
				m_stTitleUIs.m_oTitleText.text = $"{NameTable.GetValue(oHuntTable.NameKey)} {oHuntTable.Order}";

				break;

			case EPlayMode.ADVENTURE:
				uint nAdventureKey = ComUtil.GetAdventureKey(GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nEpisodeID + 1,
					GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nChapterID + 1);

				oName = UIStringTable.GetValue("ui_mission_adventure");
				var oAdventureTable = MissionAdventureTable.GetData(nAdventureKey);

				m_stTitleUIs.m_oAdventureTitleText.text = $"{oName} {oAdventureTable.Order}";
				break;

			case EPlayMode.ABYSS:
				uint nAbyssTableKey = ComUtil.GetAbyssKey(GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nEpisodeID,
					GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nChapterID + 1);

				oName = UIStringTable.GetValue("ui_popup_abyss_tab_abyss");
				var oAbyssTable = AbyssTable.GetData(nAbyssTableKey);

				m_stTitleUIs.m_oTitleText.text = $"{oName} {oAbyssTable.Order}";
				break;

			case EPlayMode.TUTORIAL:
				m_stTitleUIs.m_oTitleText.text = string.Format("{0} {1}",
					UIStringTable.GetValue("ui_popup_tutorial_skip_button_cap_tutorial"), GameDataManager.Singleton.PlayStageID + 1);

				break;

			default:
				m_stTitleUIs.m_oTitleText.text = string.Format("{0} - {1}",
					GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nEpisodeID + 1, GameDataManager.Singleton.PlayStageID + 1);

				break;
		}

		int nNumStages = GameDataManager.Singleton.PlayMapInfoDict.Count;

		for (int i = 0; i < m_stTitleUIs.m_oStageImgList.Count; ++i)
		{
			var oIconImg = m_stTitleUIs.m_oStageImgList[i].transform.Find("Icon").GetComponent<Image>();
			oIconImg?.gameObject.SetActive(i == GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nStageID);

			var oCompleteIconImg = m_stTitleUIs.m_oStageImgList[i].transform.Find("CompleteIcon").GetComponent<Image>();
			oCompleteIconImg?.gameObject.SetActive(i < GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nStageID);

			m_stTitleUIs.m_oStageImgList[i].gameObject.SetActive(i < nNumStages);
		}
	}

	/** 무기 UI 상태를 갱신한다 */
	private void UpdateWeaponUIsState()
	{
		for (int i = 0; i < this.WeaponUIsHandlerList.Count; ++i)
		{
			this.WeaponUIsHandlerList[i].SetState((this.PlayerController.CurWeaponIdx == i) ?
				CWeaponUIsHandler.EState.SEL : CWeaponUIsHandler.EState.UNSEL);
		}
	}

	/** 마커 UI 상태를 갱신한다 */
	private void UpdateMarkerUIsState()
	{
		var oCamera = m_oCamera;
		var oRootRectTrans = m_oMarkerUIsRoot.transform as RectTransform;
		var oBattleController = this.BattleController;

		var oDeltaList = CCollectionPoolManager.Singleton.SpawnList<Vector3>();
		var oRaycastHit2DList = CCollectionPoolManager.Singleton.SpawnList<RaycastHit2D>();

		var stPlayerPos = oCamera.WorldToScreenPoint(oBattleController.Cam.transform.position);
		var stMarkerUIsTransJob = new STMarkerUIsTransJob();

		try
		{
			for (int i = 0; i < this.TalkUIsList.Count; ++i)
			{
				bool bIsActive = this.PlayerController.StateMachine.State != null;
				this.TalkUIsList[i].SetActive(bIsActive && i < oBattleController.NonPlayerControllerList.Count);

				// NPC 가 없을 경우
				if (i >= oBattleController.NonPlayerControllerList.Count)
				{
					continue;
				}

				var stTargetPos = oBattleController.NonPlayerControllerList[i].transform.position + (Vector3.up * 3.5f);
				var stCameraDelta = stTargetPos - oCamera.transform.position;
				var stTalkUIsPos = oCamera.WorldToScreenPoint(stTargetPos);

				// 화면 좌표 계산이 불가능 할 경우
				if (Vector3.Dot(stCameraDelta.normalized, oCamera.transform.forward).ExIsLessEquals(0.0f))
				{
					this.TalkUIsList[i].SetActive(false);
					continue;
				}

				var stSize = (this.TalkUIsList[i].transform as RectTransform).sizeDelta;
				var stDelta = stTalkUIsPos - stPlayerPos;
				var stRaycastHit = Physics2D.BoxCast(stPlayerPos, stSize, 0.0f, stDelta.normalized, stDelta.magnitude, m_nBoundsLayerMask);

				var stHitPos = stPlayerPos + (stDelta.normalized * stRaycastHit.distance);
				this.TalkUIsList[i].transform.position = (stRaycastHit.collider != null) ? stHitPos : stTalkUIsPos;
			}

			for (int i = 0; i < m_oMarkerUIsHandlerList.Count; ++i)
			{
				bool bIsActive = this.PlayerController.StateMachine.State != null;
				bIsActive = bIsActive && i < oBattleController.NonPlayerControllerList.Count;
				bIsActive = bIsActive && oBattleController.NonPlayerControllerList[i].Table.isNecessary > 0;

				m_oMarkerUIsHandlerList[i].gameObject.SetActive(bIsActive);

				// NPC 가 없을 경우
				if (i >= oBattleController.NonPlayerControllerList.Count)
				{
					continue;
				}

				var stCameraDelta = oBattleController.NonPlayerControllerList[i].transform.position - oCamera.transform.position;
				var stNonPlayerPos = oCamera.WorldToScreenPoint(oBattleController.NonPlayerControllerList[i].transform.position);

				// 화면 좌표 계산이 불가능 할 경우
				if (Vector3.Dot(stCameraDelta.normalized, oCamera.transform.forward).ExIsLessEquals(0.0f))
				{
					m_oMarkerUIsHandlerList[i].gameObject.SetActive(false);
					continue;
				}

				var stDelta = stNonPlayerPos - stPlayerPos;
				var stRaycastHit = Physics2D.Raycast(stPlayerPos, stDelta.normalized, stDelta.magnitude, m_nBoundsLayerMask);

				bool bIsBoss = oBattleController.NonPlayerControllerList[i].Table.Grade == (int)ENPCGrade.BOSS;
				bool bIsVisible = Camera.main.ExIsVisible(oBattleController.NonPlayerControllerList[i].transform.position);
				bool bIsWarning = oBattleController.NonPlayerControllerList[i].TrackingTarget != null;

				float fAlpha = 1.0f - Mathf.Pow((stRaycastHit.distance / stDelta.magnitude), 15.0f);

				m_oMarkerUIsHandlerList[i].SetAlpha(bIsVisible ? 0.0f : fAlpha);
				m_oMarkerUIsHandlerList[i].SetIsWarning(bIsWarning);
				m_oMarkerUIsHandlerList[i].SetMarkerType(bIsBoss ? CMarkerUIsHandler.EMarkerType.BOSS : CMarkerUIsHandler.EMarkerType.NORM);

#if DISABLE_THIS
				m_oMarkerUIsHandlerList[i].transform.position = stRaycastHit.point;
				m_oMarkerUIsHandlerList[i].transform.localEulerAngles = new Vector3(0.0f, 0.0f, Vector3.SignedAngle(Vector3.right, stDelta.normalized, Vector3.forward));
#else
				oDeltaList.Add(stDelta);
				oRaycastHit2DList.Add(stRaycastHit);
#endif // #if DISABLE_THIS
			}

			stMarkerUIsTransJob.m_stDeltas = new NativeArray<Vector3>(oDeltaList.ToArray(), Allocator.TempJob);
			stMarkerUIsTransJob.m_stRaycastHit2Ds = new NativeArray<RaycastHit2D>(oRaycastHit2DList.ToArray(), Allocator.TempJob);

			var oJobHandler = stMarkerUIsTransJob.Schedule(m_oMarkerUIsTransforms);
			oJobHandler.Complete();
		}
		finally
		{
			stMarkerUIsTransJob.m_stDeltas.Dispose();
			stMarkerUIsTransJob.m_stRaycastHit2Ds.Dispose();

			CCollectionPoolManager.Singleton.DespawnList(oDeltaList);
			CCollectionPoolManager.Singleton.DespawnList(oRaycastHit2DList);
		}

		int nPlayStageID = GameDataManager.Singleton.PlayStageID;
		var oMapInfoType = GameDataManager.Singleton.PlayMapInfoType;

		for (int i = 0; i < m_oGateMarkerUIsHandlerList.Count; ++i)
		{
			bool bIsActive = this.PlayerController.StateMachine.State != null;
			m_oGateMarkerUIsHandlerList[i].gameObject.SetActive(bIsActive && i < m_oGateMarkerUIsHandlerList.Count);
			m_oGateMarkerUIsHandlerList[i].SetMarkerType(CMarkerUIsHandler.EMarkerType.GATE);

			// 게이트가 없을 경우
			if (i >= m_oGateMarkerUIsHandlerList.Count)
			{
				continue;
			}

			var stGatePos = oCamera.WorldToScreenPoint(oBattleController.GateObjList[i].transform.position);
			var stCameraDelta = oBattleController.GateObjList[i].transform.position - oCamera.transform.position;

			// 화면 좌표 계산이 불가능 할 경우
			if (Vector3.Dot(stCameraDelta.normalized, oCamera.transform.forward).ExIsLessEquals(0.0f))
			{
				m_oGateMarkerUIsHandlerList[i].gameObject.SetActive(false);
				continue;
			}

			var stDelta = stGatePos - stPlayerPos;
			var stRaycastHit = Physics2D.Raycast(stPlayerPos, stDelta.normalized, stDelta.magnitude, m_nBoundsLayerMask);

			bool bIsVisible = Camera.main.ExIsVisible(oBattleController.GateObjList[i].transform.position);
			float fAlpha = 1.0f - Mathf.Pow((stRaycastHit.distance / stDelta.magnitude), 15.0f);

			m_oGateMarkerUIsHandlerList[i].SetAlpha(bIsVisible ? 0.0f : fAlpha);
			m_oGateMarkerUIsHandlerList[i].SetIsWarning(false);

			m_oGateMarkerUIsHandlerList[i].transform.position = stRaycastHit.point;
			m_oGateMarkerUIsHandlerList[i].transform.localEulerAngles = new Vector3(0.0f, 0.0f, Vector3.SignedAngle(Vector3.right, stDelta.normalized, Vector3.forward));

			switch (oMapInfoType)
			{
				case EMapInfoType.ABYSS: m_oGateMarkerUIsHandlerList[i].gameObject.SetActive(i == nPlayStageID && !this.BattleController.IsMakeAppearBossNPC); break;
				default: m_oGateMarkerUIsHandlerList[i].gameObject.SetActive(this.BattleController.TargetNonPlayerControllerList.Count <= 0); break;
			}
		}

		for (int i = 0; i < m_oBoxMarkerUIsHandlerList.Count; ++i)
		{
			bool bIsActive = this.PlayerController.StateMachine.State != null;
			m_oBoxMarkerUIsHandlerList[i].gameObject.SetActive(bIsActive && i < m_oBoxMarkerUIsHandlerList.Count && oBattleController.BoxInteractableMapObjHandlerList[i].IsInteractable);
			m_oBoxMarkerUIsHandlerList[i].SetMarkerType(CMarkerUIsHandler.EMarkerType.BOX);

			// 상호 작용 객체가 없을 경우
			if (i >= m_oBoxMarkerUIsHandlerList.Count)
			{
				continue;
			}

			var stGatePos = oCamera.WorldToScreenPoint(oBattleController.BoxInteractableMapObjHandlerList[i].transform.position);
			var stCameraDelta = oBattleController.BoxInteractableMapObjHandlerList[i].transform.position - oCamera.transform.position;

			// 화면 좌표 계산이 불가능 할 경우
			if (Vector3.Dot(stCameraDelta.normalized, oCamera.transform.forward).ExIsLessEquals(0.0f))
			{
				m_oBoxMarkerUIsHandlerList[i].gameObject.SetActive(false);
				continue;
			}

			var stDelta = stGatePos - stPlayerPos;
			var stRaycastHit = Physics2D.Raycast(stPlayerPos, stDelta.normalized, stDelta.magnitude, m_nBoundsLayerMask);

			bool bIsVisible = Camera.main.ExIsVisible(oBattleController.BoxInteractableMapObjHandlerList[i].transform.position);
			float fAlpha = 1.0f - Mathf.Pow((stRaycastHit.distance / stDelta.magnitude), 15.0f);

			m_oBoxMarkerUIsHandlerList[i].SetAlpha(bIsVisible ? 0.0f : fAlpha);
			m_oBoxMarkerUIsHandlerList[i].SetIsWarning(false);

			m_oBoxMarkerUIsHandlerList[i].transform.position = stRaycastHit.point;
			m_oBoxMarkerUIsHandlerList[i].transform.localEulerAngles = new Vector3(0.0f, 0.0f, Vector3.SignedAngle(Vector3.right, stDelta.normalized, Vector3.forward));
		}
	}

	/** 게이트 마커 UI 를 설정한다 */
	public void SetupGateMarkerUIs()
	{
		for (int i = 0; i < this.BattleController.GateObjList.Count; ++i)
		{
			var oMarkerUIs = GameResourceManager.Singleton.CreateUIObject(EResourceType.UI_Battle,
							"BattleMarkerUIs", m_oMarkerUIsRoot.transform).GetComponent<CMarkerUIsHandler>();

			m_oGateMarkerUIsHandlerList.Add(oMarkerUIs);
		}
	}

	/** 상호 작용 마커 UI 를 설정한다 */
	public void SetupBoxMarkerUIs()
	{
		for (int i = 0; i < this.BattleController.BoxInteractableMapObjHandlerList.Count; ++i)
		{
			var oMarkerUIs = GameResourceManager.Singleton.CreateUIObject(EResourceType.UI_Battle,
				"BattleMarkerUIs", m_oMarkerUIsRoot.transform).GetComponent<CMarkerUIsHandler>();

			m_oBoxMarkerUIsHandlerList.Add(oMarkerUIs);
		}
	}

	/** 대화를 출력한다 */
	public void ShowTalk(string a_oTalk, bool a_bIsRealtime = false)
	{
		// 대화 출력이 불가능 할 경우
		if (m_oTalkUIs == null || m_oTalkText == null || m_oTalkAgentImg == null)
		{
			return;
		}

		var oSequence = DOTween.Sequence().SetUpdate(a_bIsRealtime);
		oSequence.AppendInterval(1.0f);
		oSequence.Append(m_oTalkUIs.transform.DOScaleY(1.0f, 0.15f).SetUpdate(a_bIsRealtime));
		oSequence.AppendInterval(3.0f);
		oSequence.Append(m_oTalkUIs.transform.DOScaleY(0.0f, 0.15f).SetUpdate(a_bIsRealtime));
		oSequence.AppendCallback(() => m_oTalkUIs.transform.localScale = new Vector3(1.0f, 0.0f, 1.0f));

		m_oTalkText.text = a_oTalk;
		m_oTalkAgentImg.sprite = ComUtil.GetIcon(this.PlayerController.Table.PrimaryKey);

		ComUtil.AssignVal(ref m_oTalkAni, oSequence);
		ComUtil.RebuildLayouts(m_oTalkUIs);
	}

	/** 인스턴스 효과 팝업을 출력한다 */
	public void ShowInstEffectPopup(InstanceLevelTable a_oTable)
	{
		ComUtil.SetTimeScale(0.00001f, true);
		var oPopupBattleInstEffect = MenuManager.Singleton.OpenPopup<PopupBattleInstEffect>(EUIPopup.PopupBattleInstEffect);

		int nEpisodeID = GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nEpisodeID;
		int nCorrectEpisodeID = (GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.CAMPAIGN) ? nEpisodeID + 1 : nEpisodeID;

		var oStageTableList = StageTable.GetStageGroup(nCorrectEpisodeID, 1);

		var stParams = PopupBattleInstEffect.MakeParams(oStageTableList[GameDataManager.Singleton.PlayStageID],
			this.OnReceiveBattleInstEffectPopupResult);

		oPopupBattleInstEffect.Init(stParams);
	}

	/** 인스턴스 효과 팝업 결과를 수신했을 경우 */
	public void OnReceiveBattleInstEffectPopupResult(PopupBattleInstEffect a_oSender,
		EffectTable a_oEffectTable, float a_fVal)
	{
		ComUtil.SetTimeScale(IsBoost ? 2.0f : 1.0f);
		this.PlayerController.SetIsDirtyUpdateUIsState(true);

		bool bIsHPRecovery = a_oEffectTable.Category == (int)EEffectCategory.INST_EFFECT && a_oEffectTable.Type == (int)EEquipEffectType.HP_RECOVERY;
		bIsHPRecovery = bIsHPRecovery || (a_oEffectTable.Category == (int)EEffectCategory.ROULETTE_EFFECT && a_oEffectTable.Type == (int)EEquipEffectType.HP_RECOVERY);
		bIsHPRecovery = bIsHPRecovery && a_fVal.ExIsGreatEquals(1.0f);

		// 체력 회복 효과 일 경우
		if (bIsHPRecovery)
		{
			this.PlayerController.SetHP(this.PlayerController.HP + Mathf.FloorToInt(a_fVal));
			string oPrefabPath = GameResourceManager.Singleton.GetPrefabPath(EResourceType.Effect, a_oEffectTable.PrefabFX);

			// 획득 효과가 존재 할 경우
			if (a_oEffectTable.PrefabFX.ExIsValid() && ComUtil.TryLoadRes<GameObject>(oPrefabPath, out GameObject oFXObj))
			{
				var oParticle = GameResourceManager.Singleton.CreateObject<ParticleSystem>(oFXObj,
					this.PlayerController.transform, null, ComType.G_LIFE_T_RECOVERY_FX);

				oParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
				oParticle?.Play(true);
			}

			return;
		}

		// NPC 대상 효과 일 경우
		if (a_oEffectTable.RangeType == (int)ERangeType.NON_PLAYER)
		{
			this.OnReceiveBattleInstEffectPopupResultNonPlayer(a_oSender, a_oEffectTable, a_fVal);
		}
		else
		{
			this.OnReceiveBattleInstEffectPopupResultPlayer(a_oSender, a_oEffectTable, a_fVal);
		}

		int nResult = this.BuffEffectTableInfoList.FindIndex((a_oTableInfo) => a_oTableInfo.Item1.Type == a_oEffectTable.Type);

		// 버프 테이블 정보가 존재 할 경우
		if(this.BuffEffectTableInfoList.ExIsValidIdx(nResult))
		{
			var stTableInfo = this.BuffEffectTableInfoList[nResult];
			stTableInfo.Item2 += a_fVal;

			this.BuffEffectTableInfoList[nResult] = stTableInfo;
		}
		else
		{
			this.BuffEffectTableInfoList.Add((a_oEffectTable, a_fVal));
		}

#if DISABLE_THIS
		this.BuffEffectTableInfoList.ExAddVal(a_oEffectTable, 
			(a_oEffectTable_Compare) => a_oEffectTable_Compare.Type == a_oEffectTable.Type);
#endif // #if DISABLE_THIS

		this.SetIsDirtyUpdateUIsState(true);
	}

	/** 플레이어 인스턴스 효과 팝업 결과를 수신했을 경우 */
	private void OnReceiveBattleInstEffectPopupResultPlayer(PopupBattleInstEffect a_oSender,
		EffectTable a_oEffectTable, float a_fVal)
	{
		// 지속 시간이 존재 할 경우
		if (a_oEffectTable.Duration > 0)
		{
			int nDurationVal = (int)EOperationType.ADD * (int)EEquipEffectType.DURATION_VAL;

			ComUtil.AddEffect((EEquipEffectType)(a_oEffectTable.Type + nDurationVal),
				(EEquipEffectType)(a_oEffectTable.Type + nDurationVal), a_oEffectTable.Duration * ComType.G_UNIT_MS_TO_S, 0.0f, 0.0f, this.PlayerController.PassiveEffectStackInfoList, 1);
		}

		bool bIsExplosionRangeRatio = a_oEffectTable.Category == (int)EEffectCategory.INST_EFFECT &&
			a_oEffectTable.Type == (int)EEquipEffectType.ExplosionRangeRatio;

		bIsExplosionRangeRatio = bIsExplosionRangeRatio ||
			(a_oEffectTable.Category == (int)EEffectCategory.ROULETTE_EFFECT && a_oEffectTable.Type == (int)EEquipEffectType.ExplosionRangeRatio);

		float fVal = bIsExplosionRangeRatio ? a_fVal * ComType.G_UNIT_MM_TO_M : a_fVal;
		float fHPPercent = this.PlayerController.HP / (float)this.PlayerController.MaxHP;

		ComUtil.AddEffect(a_oEffectTable,
			fVal, this.PlayerController.PassiveEffectStackInfoList, !bIsExplosionRangeRatio, 10, true);

		this.PlayerController.SetupAbilityValues(true);

		// 최대 체력 증가 효과 일 경우
		if (a_oEffectTable.Type == (int)EEquipEffectType.MaxHPRatio)
		{
			this.PlayerController.SetHP(Mathf.FloorToInt(this.PlayerController.MaxHP * fHPPercent));
			this.PlayerController.SetIsDirtyUpdateUIsState(true);
		}
	}

	/** NPC 인스턴스 효과 팝업 결과를 수신했을 경우 */
	private void OnReceiveBattleInstEffectPopupResultNonPlayer(PopupBattleInstEffect a_oSender,
		EffectTable a_oEffectTable, float a_fVal)
	{
		ComUtil.AddEffect(a_oEffectTable,
			a_fVal, this.BattleController.NonPlayerPassiveEffectStackInfoList, true, 10, true);

		this.BattleController.SetupNonPlayerEffectStackInfos();

		for (int i = 0; i < this.BattleController.NonPlayerControllerList.Count; ++i)
		{
			this.BattleController.NonPlayerControllerList[i].SetupAbilityValues(true);
			this.BattleController.NonPlayerControllerList[i].SetIsDirtyUpdateUIsState(true);
		}
	}

	/** 인벤토리 버튼을 눌렀을 경우 */
	public void OnTouchInventoryBtn()
	{
		m_oBattleInventory.OpenInventory();
	}
	#endregion // 함수

	#region 접근 함수
	/** UI 활성 여부를 변경한다 */
	public void SetIsVisibleUIs(bool a_bIsVisible)
	{
		m_oTopUIs.SetActive(a_bIsVisible);
		m_oBottomUIs.SetActive(a_bIsVisible);

		for (int i = 0; i < m_oContentsUIsList.Count; ++i)
		{
			m_oContentsUIsList[i].SetActive(a_bIsVisible);
		}

		var ePlayMode = GameDataManager.Singleton.PlayMode;

		// 테스트 플레이 모드 일 경우
		if (GameDataManager.Singleton.PlayMode == EPlayMode.TEST)
		{
			ePlayMode = (EPlayMode)(GameDataManager.Singleton.PlayMapInfoType + 1);
		}

		bool bIsCampaign = ePlayMode == EPlayMode.CAMPAIGN;
		bIsCampaign = bIsCampaign || ePlayMode == EPlayMode.TUTORIAL;

		m_oBattleInventoryUIs.SetActive(a_bIsVisible && bIsCampaign);
	}

	/** UI 상태 갱신 여부를 변경한다 */
	public void SetIsDirtyUpdateUIsState(bool a_bIsDirty, bool a_bIsForce = false)
	{
		// 강제 모드 일 경우
		if (a_bIsForce)
		{
			m_bIsDirtyUpdateUIsState = a_bIsDirty;
		}
		else
		{
			m_bIsDirtyUpdateUIsState = m_bIsDirtyUpdateUIsState || a_bIsDirty;
		}
	}

	/** 전투 제어자를 변경한다 */
	public void SetBattleController(BattleController a_oController)
	{
		this.BattleController = a_oController;
	}

	/** 플레이어 제어자를 변경한다 */
	public void SetPlayerController(PlayerController a_oController)
	{
		this.PlayerController = a_oController;
	}

	/** 이어하기 정보를 설정한다 */
	public void SetContinueInfo(int a_nMaxTimes, int a_nStandardPrice)
	{
		this.MaxContinueTimes = a_nMaxTimes;
		this.StandardContinuePrice = a_nStandardPrice;
	}
	#endregion // 접근 함수
}
