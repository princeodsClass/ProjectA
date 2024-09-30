using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/** 탐험 미션 팝업 */
public class PopupMissionAdventure : UIDialog
{
	/** 컬러 */
	public enum EColor
	{
		NONE = -1,
		NORM,
		BOSS,
		CLEAR,
		ACTIVE,
		[HideInInspector] MAX_VAL
	}

	/** 스크롤러 셀 뷰 상태 */
	public enum EScrollerCellViewState
	{
		NONE = -1,
		LOCK,
		CLEAR,
		ACTIVE,
		[HideInInspector] MAX_VAL
	}

	/** 매개 변수 */
	public struct STParams
	{
		public int m_nGroup;
		public bool m_bIsClear;
	}

	#region 변수
	[Tooltip("0 : 일반 스테이지\n1 : 보스 스테이지\n2 : 클리어 스테이지\n3 : 활성 스테이지")]
	[SerializeField] private List<Color> m_oColorList = new List<Color>();

	private bool m_bIsAcquiring = false;

	private int m_nPlayableOrder = 0;
	private int m_nAcquireRewardOrder = 0;

	private Tween m_oClearAni = null;
	private Tween m_oScrollAni = null;

	private PlayerController m_oPlayerController = null;
	private List<PopupMissionAdventureScrollerCellView> m_oScrollerCellViewList = new List<PopupMissionAdventureScrollerCellView>();

	[Header("=====> UIs <=====")]
	[SerializeField] private TMP_Text m_oNumPassTicketsText = null;
	[SerializeField] private TMP_Text m_oNumGlobalTicketsText = null;

	[SerializeField] private TMP_Text m_oRemainTimeText = null;
	[SerializeField] private TMP_Text m_oTicketChargeTimeText = null;

	[SerializeField] private ScrollRect m_oScrollRect = null;
	[SerializeField] private List<ContentSizeFitter> m_oSizeFitterList = new List<ContentSizeFitter>();

	[Header("=====> Game Objects <=====")]
	[SerializeField] private GameObject m_oPlayerRoot = null;
	[SerializeField] private GameObject m_oRenderTexRoot = null;

	[SerializeField] private GameObject m_oTopUIs = null;
	[SerializeField] private GameObject m_oBottomUIs = null;

	[SerializeField] private GameObject m_oRenderContents = null;
	[SerializeField] private GameObject m_oFGScrollViewContents = null;

	[SerializeField] private GameObject m_oScrollViewViewport = null;
	[SerializeField] private GameObject m_oScrollViewContents = null;

	[SerializeField] private GameObject m_oOriginScrollerCellView = null;
	[SerializeField] private GameObject m_oOriginFGScrollerCellView = null;

	[SerializeField] private List<GameObject> m_oPlayTicketUIsList = new List<GameObject>();
	#endregion // 변수

	#region 클래스 변수
	private static bool m_bIsAwake = true;
	#endregion // 클래스 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	public PopupMissionAdventureKeyBuy KeyBuyPopup { get; set; } = null;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		base.Initialize();
	}

	/** 그룹 상태를 리셋한다 */
	public void ResetAdventureGroup(int a_nGroup)
	{
		var stParams = this.Params;
		stParams.m_nGroup = a_nGroup;

		m_nPlayableOrder = GameManager.Singleton.user.m_nAdventureLevel;
		this.Init(stParams);

		for (int i = 0; i < m_oScrollerCellViewList.Count; ++i)
		{
			m_oScrollerCellViewList[i].ResetAdventureGroup(a_nGroup);
		}
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
		string cName = string.Empty;

		// 플레이어가 존재 할 경우
		if (m_oPlayerController != null)
		{
			GameResourceManager.Singleton.ReleaseObject(m_oPlayerController.gameObject, false);
		}

		m_nPlayableOrder = GameManager.Singleton.user.m_nAdventureLevel;

		foreach (var character in GameManager.Singleton.invenCharacter)
			if (character.id == GameManager.Singleton.user.m_nCharacterID)
				cName = CharacterTable.GetData(character.nKey).Prefab;

		var oGameObj = GameResourceManager.Singleton.CreateObject(EResourceType.Character, cName, m_oPlayerRoot.transform);
		m_oPlayerController = oGameObj.GetComponent<PlayerController>();

		this.SetupScrollerCellViews();
		ComUtil.RebuildLayouts(this.gameObject);

		// 스크롤 뷰를 설정한다 {
		var oSizeFitters = m_oScrollRect.GetComponentsInChildren<ContentSizeFitter>();

		for (int i = oSizeFitters.Length - 1; i >= 0; --i)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(oSizeFitters[i].transform as RectTransform);
		}
		// 스크롤 뷰를 설정한다 }

		this.UpdateUIsState();
		GameManager.Singleton.ExLateCallFunc((a_oSender) => this.LateInit());
	}

	/** 초기화 */
	public virtual void LateInit()
	{
		var stRect = (m_oScrollViewContents.transform as RectTransform).rect;
		var oLayoutGroup = m_oScrollViewContents.GetComponent<VerticalLayoutGroup>();

		int nPlayableOrder = Mathf.Min(m_nPlayableOrder, m_oScrollerCellViewList.Count - 1);

		(m_oTopUIs.transform as RectTransform).anchoredPosition = new Vector2(0.0f, -oLayoutGroup.padding.top);
		(m_oBottomUIs.transform as RectTransform).anchoredPosition = new Vector2(0.0f, oLayoutGroup.padding.bottom);

		var stPos = m_oScrollerCellViewList[nPlayableOrder].PlayerPosDummy.transform.position.ExToLocal(m_oScrollViewContents);
		var stViewportRect = (m_oScrollViewViewport.transform as RectTransform).rect;

		float fPosY = (Mathf.Abs(stPos.y) + stViewportRect.size.y / 2.0f) - stViewportRect.size.y;
		float fHeight = (m_oScrollViewContents.transform as RectTransform).rect.size.y - stViewportRect.size.y;

		m_bIsAcquiring = false;
		m_oRenderTexRoot.transform.position = m_oScrollerCellViewList[nPlayableOrder].PlayerPosDummy.transform.position;

		// 최초 상태 일 경우
		if (!this.Params.m_bIsClear && PopupMissionAdventure.m_bIsAwake)
		{
			float fNormPosY = 1.0f;
			PopupMissionAdventure.m_bIsAwake = false;
			m_oScrollRect.verticalNormalizedPosition = 1.0f;

			var oSequence = DOTween.Sequence().AppendInterval(0.5f);
			oSequence.Append(DOTween.To(() => fNormPosY, (a_fVal) => m_oScrollRect.verticalNormalizedPosition = a_fVal, Mathf.Clamp01(1.0f - (fPosY / fHeight)), 3.5f).SetEase(Ease.OutExpo));
			oSequence.AppendCallback(() => this.OnCompleteScrollAni(oSequence));

			ComUtil.AssignVal(ref m_oScrollAni, oSequence);
		}
		else
		{
			// 클리어 상태 일 경우
			if (this.Params.m_bIsClear)
			{
				var stParams = this.Params;
				stParams.m_bIsClear = false;

				this.Params = stParams;
				this.AcquireRewards();
			}

			m_oScrollRect.verticalNormalizedPosition = Mathf.Clamp01(1.0f - (fPosY / fHeight));
		}
	}

	/** 제거 되었을 경우 */
	public virtual void OnDestroy()
	{
		m_oClearAni?.Kill();
		m_oScrollAni?.Kill();
	}

	/** 빠른 진행 필요 티켓 개수를 반환한다 */
	private int GetNumNeedsPassTickets()
	{
		var oAdventureTableList = MissionAdventureTable.GetGroup(this.Params.m_nGroup + 1);
		return oAdventureTableList.ExIsValidIdx(m_nPlayableOrder) ? oAdventureTableList[m_nPlayableOrder].AutoCompItemCount : 0;
	}

	/** 시간 정보를 갱신한다 */
	public void UpdateTimeInfos(string a_oRemainTime, string a_oTicketChargeTime, bool a_bIsCharging)
	{
		var oKeyMatTable = MaterialTable.GetData(ComType.G_KEY_MAT_ADVENTURE_KEY);
		string oName = NameTable.GetValue(oKeyMatTable.NameKey);

		m_oRemainTimeText.text = a_oRemainTime;
		m_oTicketChargeTimeText.text = oName + " : " + a_oTicketChargeTime;

		this.KeyBuyPopup?.SetChargeTime(a_oTicketChargeTime);
		m_oTicketChargeTimeText.gameObject.SetActive(a_bIsCharging);

		for (int i = 0; i < m_oSizeFitterList.Count; ++i)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(m_oSizeFitterList[i].transform as RectTransform);
		}

		for (int i = m_oSizeFitterList.Count - 1; i >= 0; --i)
		{
			LayoutRebuilder.MarkLayoutForRebuild(m_oSizeFitterList[i].transform as RectTransform);
		}
	}

	/** 상태를 갱신한다 */
	public void UpdateUIsState()
	{
		uint nKey = GlobalTable.GetData<uint>(ComType.G_VALUE_TICKET_DEC_KEY);
		uint nGlobalTicketKey = GlobalTable.GetData<uint>(ComType.G_VALUE_GLOBAL_TICKET_DEC_KEY);

		int nNumTickets = m_InvenMaterial.GetItemCount(nKey);
		int nNumGlobalTickets = m_InvenMaterial.GetItemCount(nGlobalTicketKey);

		int nNumNeedsTickets = this.GetNumNeedsPassTickets();
		int nNumNeedsGlobalTickets = 1;

		m_oNumPassTicketsText.text = $"{nNumTickets}";
		m_oNumPassTicketsText.color = (nNumTickets < nNumNeedsTickets) ? Color.red : Color.white;

		m_oNumGlobalTicketsText.text = $"{nNumGlobalTickets}";
		m_oNumGlobalTicketsText.color = (nNumGlobalTickets < nNumNeedsGlobalTickets) ? Color.red : Color.white;

		for (int i = 0; i < m_oPlayTicketUIsList.Count; ++i)
		{
			var oIconImg = m_oPlayTicketUIsList[i].transform.Find("IconImg");
			oIconImg.gameObject.SetActive(i < GameManager.Singleton.user.m_nCurrentAdventureKeyCount);
		}

		for (int i = 0; i < m_oScrollerCellViewList.Count; ++i)
		{
			m_oScrollerCellViewList[i].Params.m_oFGScrollerCellView.PriceText.text = $"{m_oScrollerCellViewList[i].Params.m_oTable.AutoCompItemCount}";

			// 플레이 가능 상태 일 경우
			if (i == m_nPlayableOrder)
			{
				m_oScrollerCellViewList[i].SetState(EScrollerCellViewState.ACTIVE);
			}
			else
			{
				var eState = (i < m_nPlayableOrder) ? EScrollerCellViewState.CLEAR : EScrollerCellViewState.LOCK;
				m_oScrollerCellViewList[i].SetState(eState);
			}
		}
	}

	/** 스크롤러 셀 뷰를 설정한다 */
	private void SetupScrollerCellViews()
	{
		var oAdventureTableList = MissionAdventureTable.GetGroup(this.Params.m_nGroup + 1);
		m_oScrollerCellViewList.Clear();

		for (int i = 0; i < oAdventureTableList.Count; ++i)
		{
			PopupMissionAdventureScrollerCellView oScrollerCellView = null;
			PopupMissionAdventureFGScrollerCellView oFGScrollerCellView = null;

			// 스크롤러 셀 뷰가 존재 할 경우
			if (i < m_oScrollViewContents.transform.childCount)
			{
				oScrollerCellView = m_oScrollViewContents.transform.GetChild(i).GetComponent<PopupMissionAdventureScrollerCellView>();
				oFGScrollerCellView = m_oFGScrollViewContents.transform.GetChild(i).GetComponent<PopupMissionAdventureFGScrollerCellView>();
			}
			else
			{
				oScrollerCellView = GameResourceManager.Singleton.CreateObject<PopupMissionAdventureScrollerCellView>(m_oOriginScrollerCellView,
					m_oScrollViewContents.transform, null);

				oFGScrollerCellView = GameResourceManager.Singleton.CreateObject<PopupMissionAdventureFGScrollerCellView>(m_oOriginFGScrollerCellView,
					m_oFGScrollViewContents.transform, null);
			}

			m_oScrollerCellViewList.ExAddVal(oScrollerCellView);

			var stParams = PopupMissionAdventureScrollerCellView.MakeParams(this.Params.m_nGroup,
				i, this, oFGScrollerCellView, oAdventureTableList[i]);

			var stFGParams = PopupMissionAdventureFGScrollerCellView.MakeParams(this.Params.m_nGroup,
				i, this, oAdventureTableList[i], this.OnReceiveBuyCallback, this.OnReceivePlayCallback, this.OnReceivePassCallback);

			oScrollerCellView.Init(stParams);
			oFGScrollerCellView.Init(stFGParams);

			(oScrollerCellView.transform as RectTransform).sizeDelta = (m_oOriginScrollerCellView.transform as RectTransform).sizeDelta;
			(oFGScrollerCellView.transform as RectTransform).sizeDelta = (m_oOriginFGScrollerCellView.transform as RectTransform).sizeDelta;
		}
	}

	/** 구입 콜백을 수신했을 경우 */
	private void OnReceiveBuyCallback(PopupMissionAdventureFGScrollerCellView a_oSender)
	{
		var oPopupMissionAdventureKeyBuy = MenuManager.Singleton.OpenPopup<PopupMissionAdventureKeyBuy>(EUIPopup.PopupMissionAdventureKeyBuy, true);
		oPopupMissionAdventureKeyBuy.Init(PopupMissionAdventureKeyBuy.MakeParams(this, a_oSender.Params.m_oTable, this.OnReceivePopupMissionAdventureKeyBuyCallback));

		this.KeyBuyPopup = oPopupMissionAdventureKeyBuy;
	}

	/** 플레이 콜백을 수신했을 경우 */
	private void OnReceivePlayCallback(PopupMissionAdventureFGScrollerCellView a_oSender)
	{
		// 플레이가 불가능 할 경우
		if (!this.IsEnablePlay())
		{
			PopupBoxMaterial pbm = MenuManager.Singleton.OpenPopup<PopupBoxMaterial>(EUIPopup.PopupBoxMaterial, true);
			pbm.InitializeInfo(true);
		}
		else
		{
			GameDataManager.Singleton.StartCoroutine(this.CoPlayAdventure(a_oSender.Params.m_oTable, false));
		}
	}

	/** 패스 콜백을 수신했을 경우 */
	private void OnReceivePassCallback(PopupMissionAdventureFGScrollerCellView a_oSender)
	{
		// 빠른 진행이 불가능 할 경우
		if (!this.IsEnablePass(a_oSender.Params.m_oTable))
		{
			return;
		}

		GameDataManager.Singleton.StartCoroutine(this.CoPlayAdventure(a_oSender.Params.m_oTable, true));
	}

	/** 탐험 미션 키 구입 팝업 콜백을 수신했을 경우 */
	private void OnReceivePopupMissionAdventureKeyBuyCallback(PopupMissionAdventureKeyBuy a_oSender, bool a_bIsSuccess)
	{
		// 구입에 실패했을 경우
		if (!a_bIsSuccess)
		{
			return;
		}

		a_oSender?.Close();
		this.UpdateUIsState();
	}

	/** 전투 준비 로딩이 완료 되었을 경우 */
	private void OnCompleteLoadingBattleReady(PopupLoading a_oSender, int a_nGroup, int a_nOrder)
	{
		GameDataManager.Singleton.StartCoroutine(this.CoHandleOnCompleteLoadingBattleReady(a_oSender, a_nGroup, a_nOrder));
	}

	/** 클리어 애니메이션이 완료 되었을 경우 */
	private void OnCompleteClearAni(Sequence a_oSender)
	{
		a_oSender?.Kill();

		for (int i = 0; i < m_oScrollerCellViewList.Count; ++i)
		{
			var oFGScrollerCellView = m_oScrollerCellViewList[i].Params.m_oFGScrollerCellView;
			oFGScrollerCellView.SetIsEnableOverlayInfoUIs(true);
		}
	}

	/** 스크롤 애니메이션이 완료 되었을 경우 */
	private void OnCompleteScrollAni(Sequence a_oSender)
	{
		a_oSender?.Kill();
	}

	/** 보상 획득 애니메이션이 완료 되었을 경우 */
	private void OnCompleteAcquireRewardAni(PopupMissionAdventureScrollerCellView a_oSender, RewardListTable a_oRewardTable)
	{
		int nNumItems = Random.Range(a_oRewardTable.RewardCountMin, a_oRewardTable.RewardCountMax + 1);
		m_nAcquireRewardOrder += 1;

		switch (a_oRewardTable.RewardKey.ToString("X").Substring(0, 2))
		{
			case "24":
				var box = new ItemBox(0, a_oRewardTable.RewardKey, 0);
				var popupReward = MenuManager.Singleton.OpenPopup<PopupBoxReward>(EUIPopup.PopupBoxReward, true);

				popupReward.InitializeInfo(box, false, true, 1, PopupBoxReward.EBoxType.normal, null, a_oCallback: this.OnCloseBoxRewardPopup);
				break;

			case "25":
				var oPopupDiceReward = MenuManager.Singleton.OpenPopup<PopupDiceReward>(EUIPopup.PopupDiceReward, true);
				oPopupDiceReward.Init(PopupDiceReward.MakeParams(DiceTable.GetData(a_oRewardTable.RewardKey), this.OnReceiveDiceRewardPopupCallback));

				break;

			default:
				GameDataManager.Singleton.StartCoroutine(this.CoAcquireReward(a_oRewardTable, nNumItems));
				break;
		}
	}

	public override void Escape()
	{
		// 보상 획득 중 일 경우
		if (m_bIsAcquiring)
		{
			return;
		}

		base.Escape();
	}

	/** 맵 정보를 로드한다 */
	private void LoadMapInfos(int a_nGroup, int a_nOrder)
	{
		var oHandler = new PopupLoadingHandlerBattleReady();
		oHandler.Init(PopupLoadingHandlerBattleReady.MakeParams(EMapInfoType.ADVENTURE, new STIDInfo(0, a_nOrder, a_nGroup)));

		var oLoadingPopup = m_MenuMgr.OpenPopup<PopupLoading>(EUIPopup.PopupLoading, true);
		oLoadingPopup.Init(PopupLoading.MakeParams(PopupLoading.ELoadingType.BATTLE_READY, oHandler, (a_oSender) => this.OnCompleteLoadingBattleReady(a_oSender, a_nGroup, a_nOrder)));

#if DISABLE_THIS
		// 기존 구문
		uint nItemKey = GlobalTable.GetData<uint>(ComType.G_VALUE_GLOBAL_TICKET_DEC_KEY);
		int nNumItems = GameManager.Singleton.invenMaterial.GetItemCount(nItemKey);

		// 티켓이 부족 할 경우
		if (nNumItems <= 0)
		{
			PopupBoxMaterial pbm = MenuManager.Singleton.OpenPopup<PopupBoxMaterial>(EUIPopup.PopupBoxMaterial, true);
			pbm.InitializeInfo(true);

#if DISABLE_THIS
			PopupDefault pop = MenuManager.Singleton.OpenPopup<PopupDefault>(EUIPopup.PopupDefault, true);
			pop.SetMessage("ui_error_lackofticket_defence");

			pop.SetButtonText("ui_common_open_shopbox", "ui_common_close", () =>
			{
				SlotShopBox[] boxes = GameObject.FindObjectsOfType<SlotShopBox>();
				boxes[1].GetComponent<SlotShopBox>().OnClick(true);
			});
#endif // #if DISABLE_THIS
		}
		else
		{
			var oHandler = new PopupLoadingHandlerBattleReady();
			oHandler.Init(PopupLoadingHandlerBattleReady.MakeParams(EMapInfoType.ADVENTURE, new STIDInfo(0, a_nOrder, a_nGroup)));

			var oLoadingPopup = m_MenuMgr.OpenPopup<PopupLoading>(EUIPopup.PopupLoading, true);
			oLoadingPopup.Init(PopupLoading.MakeParams(PopupLoading.ELoadingType.BATTLE_READY, oHandler, (a_oSender) => this.OnCompleteLoadingBattleReady(a_oSender, a_nGroup, a_nOrder)));
		}
#endif // #if DISABLE_THIS
	}

	/** 보상을 획득한다 */
	public void AcquireRewards()
	{
		m_bIsAcquiring = true;
		m_nAcquireRewardOrder = 0;

		this.UpdateUIsState();

		for (int i = 0; i < m_oScrollerCellViewList.Count; ++i)
		{
			var oFGScrollerCellView = m_oScrollerCellViewList[i].Params.m_oFGScrollerCellView;
			oFGScrollerCellView.SetIsEnableOverlayInfoUIs(false);
		}

		this.DoAcquireRewards();
	}

	/** 보상 팝업이 닫혔을 경우 */
	private void OnCloseBoxRewardPopup(PopupBoxReward a_oSender)
	{
		this.ExLateCallFunc((a_oFuncSender) => this.DoAcquireRewards(), 0.5f);
	}

	/** 주사위 보상 팝업 콜백을 수신했을 경우 */
	private void OnReceiveDiceRewardPopupCallback(PopupDiceReward a_oSender)
	{
		this.ExLateCallFunc((a_oFuncSender) => this.DoAcquireRewards());
	}

	/** 보상을 획득한다 */
	private void DoAcquireRewards()
	{
		// 보상이 존재 할 경우
		if (m_oScrollerCellViewList[m_nPlayableOrder].RewardTableList.ExIsValidIdx(m_nAcquireRewardOrder))
		{
			var oRewardTable = m_oScrollerCellViewList[m_nPlayableOrder].RewardTableList[m_nAcquireRewardOrder];
			m_oScrollerCellViewList[m_nPlayableOrder].StartAcquireRewardAni(oRewardTable, this.OnCompleteAcquireRewardAni);
		}
		else
		{
			m_bIsAcquiring = false;
			m_nPlayableOrder += 1;

			GameManager.Singleton.user.m_nAdventureLevel = m_nPlayableOrder;
			GameDataManager.Singleton.StartCoroutine(this.CoStartClearDirecting());
		}
	}

	/** 티켓을 클릭했을 경우 */
	public void OnClickTicket()
	{
		PopupMaterial pop = m_MenuMgr.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial, true);
		pop.InitializeInfo(new ItemMaterial(0, GlobalTable.GetData<uint>(ComType.G_VALUE_TICKET_DEC_KEY), 0));
	}

	/** 전역 티켓을 클릭했을 경우 */
	public void OnClickGlobalTicket()
	{
		PopupMaterial pop = m_MenuMgr.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial, true);
		pop.InitializeInfo(new ItemMaterial(0, GlobalTable.GetData<uint>(ComType.G_VALUE_GLOBAL_TICKET_DEC_KEY), 0));
	}
	#endregion // 함수

	#region 접근 함수
	/** 플레이 가능 여부를 검사한다 */
	public bool IsEnablePlay()
	{
		return true;

#if DISABLE_THIS
		// 기존 구문
		uint nGlobalTicketKey = GlobalTable.GetData<uint>(ComType.G_VALUE_GLOBAL_TICKET_DEC_KEY);
		return m_InvenMaterial.GetItemCount(nGlobalTicketKey) >= 1;
#endif // #if DISABLE_THIS
	}

	/** 패스 가능 여부를 검사한다 */
	public bool IsEnablePass(MissionAdventureTable a_oTable)
	{
		return this.IsEnablePlay() && m_InvenMaterial.GetItemCount(a_oTable.AutoCompItemKey) >= a_oTable.AutoCompItemCount;
	}

	/** 스크롤류 셀 뷰 상태 색상을 반환한다 */
	public Color GetScrollerCellViewStateColor(int a_nOrder, EScrollerCellViewState a_eState)
	{
		switch (a_eState)
		{
			case EScrollerCellViewState.CLEAR: return m_oColorList[(int)EColor.CLEAR];
			case EScrollerCellViewState.ACTIVE: return m_oColorList[(int)EColor.ACTIVE];
		}

		bool bIsBoss = m_oScrollerCellViewList[a_nOrder].Params.m_oTable.ChapterType != 0;
		return bIsBoss ? m_oColorList[(int)EColor.BOSS] : m_oColorList[(int)EColor.NORM];
	}
	#endregion // 접근 함수

	#region 코루틴 함수
	/** 보상을 획득한다 */
	private IEnumerator CoAcquireReward(RewardListTable a_oRewardTable, int a_nNumItems)
	{
		var oWaitPopup = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);
		yield return GameManager.Singleton.AddItemCS(a_oRewardTable.RewardKey, a_nNumItems);

		oWaitPopup.Close();
		this.DoAcquireRewards();
	}

	/** 탐험 챕터를 클리어했을 경우 */
	private IEnumerator OnSkipAdventureChapter(PopupWait4Response a_oWaitPopup, MissionAdventureTable a_oTable)
	{
		yield return YieldInstructionCache.WaitForEndOfFrame;

		long nID = m_InvenMaterial.GetItemID(a_oTable.AutoCompItemKey);
		long nNumItems = m_InvenMaterial.GetItemCount(a_oTable.AutoCompItemKey);
		uint nItemKey = GlobalTable.GetData<uint>(ComType.G_VALUE_GLOBAL_TICKET_DEC_KEY);

		m_InvenMaterial.ModifyItem(nID, 
			InventoryData<ItemMaterial>.EItemModifyType.Volume, (int)System.Math.Max(0, nNumItems - a_oTable.AutoCompItemCount));

#if DISABLE_THIS
		GameManager.Singleton.invenMaterial.ModifyItem(GameManager.Singleton.invenMaterial.GetItemID(nItemKey),
			InventoryData<ItemMaterial>.EItemModifyType.Volume, Mathf.Max(0, GameManager.Singleton.invenMaterial.GetItemCount(nItemKey) - 1));
#endif // #if DISABLE_THIS

		a_oWaitPopup.Close();
		this.AcquireRewards();
	}

	/** 탐험을 플레이한다 */
	private IEnumerator CoPlayAdventure(MissionAdventureTable a_oTable, bool a_bIsPass)
	{
		yield return null;

		// 빠른 진행 일 경우
		if (a_bIsPass)
		{
			var oWaitPopup = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

			yield return GameDataManager.Singleton.SkipAdventureChapter((bool a_bIsSuccess) =>
			{
				// 성공했을 경우
				if (a_bIsSuccess)
				{
					GameDataManager.Singleton.StartCoroutine(this.OnSkipAdventureChapter(oWaitPopup, a_oTable));
				}
				else
				{
					oWaitPopup.Close();
					this.UpdateUIsState();
				}
			});
		}
		else
		{
			this.LoadMapInfos(GameManager.Singleton.user.m_nAdventureGroup - 1, m_nPlayableOrder);
		}
	}

	/** 클리어 연출을 시작한다 */
	private IEnumerator CoStartClearDirecting()
	{
		m_oClearAni?.Kill();
		float fDurection = 1.25f;

		// 다음 순서가 존재 할 경우
		if (m_oScrollerCellViewList.ExIsValidIdx(m_nPlayableOrder))
		{
			m_oScrollerCellViewList[m_nPlayableOrder].StartUnlockAni();
			var stPos = m_oScrollerCellViewList[m_nPlayableOrder].PlayerPosDummy.transform.position.ExToLocal(this.m_oRenderContents);

			var oSequence = DOTween.Sequence();
			oSequence.Append(m_oRenderTexRoot.transform.DOLocalMoveY(stPos.y, fDurection).SetEase(Ease.InOutQuad));
			oSequence.AppendCallback(() => this.OnCompleteClearAni(oSequence));

			ComUtil.AssignVal(ref m_oClearAni, oSequence);
		}
		else
		{
			GameManager.Singleton.user.m_nAdventureLevel += 1;
			GameObject.Find("BattlePage")?.GetComponent<PageLobbyBattle>()?.InitializeAdventure();

			PopupSysMessage.ShowMissionAdventureClearMsg();
		}

		yield return YieldInstructionCache.WaitForSeconds(fDurection / 2.0f);
		this.UpdateUIsState();
	}

	/** 전투 준비 로딩이 완료 되었을 경우 */
	private IEnumerator CoHandleOnCompleteLoadingBattleReady(PopupLoading a_oSender, int a_nGroup, int a_nOrder)
	{
		MissionAdventureTable tar = MissionAdventureTable.GetGroup(a_nGroup + 1)[a_nOrder];

         yield return m_DataMgr.StartAdventureChapter(GameManager.Singleton.invenMaterial.GetItemID(tar.ItemKey),
                                                      tar.ItemCount,
        							 				  () =>
        {
            var oMapInfoDict = CMapInfoTable.Singleton.LoadMapInfosDownloaded(EMapInfoType.ADVENTURE, a_nOrder, a_nGroup);

#if DISABLE_THIS
            uint nItemKey = GlobalTable.GetData<uint>(ComType.G_VALUE_GLOBAL_TICKET_DEC_KEY);

            GameManager.Singleton.invenMaterial.ModifyItem(GameManager.Singleton.invenMaterial.GetItemID(nItemKey),
                                                            InventoryData<ItemMaterial>.EItemModifyType.Volume,
                                                            Mathf.Max(0, GameManager.Singleton.invenMaterial.GetItemCount(nItemKey) - 1));
#endif // #if DISABLE_THIS

            m_DataMgr.SetPlayStageID(0);
            m_DataMgr.SetContinueTimes(0);
            m_DataMgr.SetupPlayMapInfo(EMapInfoType.ADVENTURE, EPlayMode.ADVENTURE, oMapInfoDict);

            m_MenuMgr.SceneEnd();
            m_MenuMgr.SceneNext(ESceneType.Battle);
        });
	}
	#endregion // 코루틴 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(int a_nGroup, bool a_bIsClear)
	{
		return new STParams
		{
			m_nGroup = a_nGroup,
			m_bIsClear = a_bIsClear
		};
	}
	#endregion // 클래스 팩토리 함수
}
