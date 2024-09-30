using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/** 전투 이어하기 팝업 */
public class PopupBattleContinue : UIDialog
{
	/** 매개 변수 */
	public struct STParams
	{
		public int m_nTimes;
		public int m_nMaxTimes;
		public int m_nStandardPrice;

		public System.Action<PopupBattleContinue, bool> m_oCallback;
	}

	#region 변수
	private bool m_bIsCounting = true;
	private uint m_nAdsTicketKey = 0;
	private float m_fRemainTime = 0.0f;
	private Sequence m_oOpenAni = null;

	[Header("=====> UIs <=====")]
	[SerializeField] private TMP_Text m_oCountText = null;
	[SerializeField] private TMP_Text m_oPriceText = null;
	[SerializeField] private TMP_Text m_oTimesText = null;
	[SerializeField] private TMP_Text m_oTitleText = null;
	[SerializeField] private TMP_Text m_oContinueText = null;
	[SerializeField] private TMP_Text m_oCancelText = null;

	[SerializeField] private TMP_Text m_oNumTicketsText = null;
	[SerializeField] private TMP_Text m_oNumCrystalsText = null;

	[SerializeField] private Image m_oGaugeImg = null;
	[SerializeField] private SoundButton m_oContinueBtn = null;
	[SerializeField] private SoundButton m_oContinueBtnAds = null;
	[SerializeField] private SoundButton m_oContinueBtnFree = null;

	[Header("=====> Game Objects <=====")]
	[SerializeField] private GameObject m_oBaseUIs = null;
	[SerializeField] private GameObject m_oPriceUIs = null;
	[SerializeField] private GameObject m_oContinueUIs = null;
	[SerializeField] private GameObject m_oCancelUIs = null;
	[SerializeField] private GameObject m_oCurrencyUIs = null;

	[SerializeField] private GameObject m_oAdsIconUIs = null;
	[SerializeField] private GameObject m_oVIPIconUIs = null;
	[SerializeField] private GameObject m_oTicketIconUIs = null;
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	public int TotalPrice => ComUtil.CalcContinueTotalPrice(this.Params.m_nTimes + 1, this.Params.m_nStandardPrice);
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	private void Awake()
	{
		Initialize();
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
		m_fRemainTime = ComType.G_MAX_TIME_CONTINUE;

		m_bIsCounting = true;
		m_nAdsTicketKey = GlobalTable.GetData<uint>("valueAdsRemoveTicketDecKey");

		// 애니메이션을 설정한다 {
		m_oContinueUIs.transform.localScale = Vector3.zero;
		m_oCancelUIs.transform.localScale = Vector3.zero;

		var oSequence = DOTween.Sequence();
		oSequence.Join(m_oContinueUIs.transform.DOScale(1.0f, 0.35f).SetEase(Ease.OutBack));
		oSequence.Join(m_oCancelUIs.transform.DOScale(1.0f, 0.35f).SetEase(Ease.OutBack).SetDelay(2.0f));

		ComUtil.AssignVal(ref m_oOpenAni, oSequence);
		// 애니메이션을 설정한다 }

		this.UpdateUIsState();
		this.ExLateCallFunc((a_oSender) => ComUtil.RebuildLayouts(this.gameObject), 0.15f);

		InitializeText();
	}

	void InitializeText()
	{
		m_oTitleText.text = UIStringTable.GetValue("ui_popup_battlecontinue_title");
		m_oContinueText.text = UIStringTable.GetValue("ui_popup_battlecontinue_button_caption_continue");
		m_oCancelText.text = UIStringTable.GetValue("ui_popup_battlecontinue_button_caption_cancel");
	}

	/** 제거 되었을 경우 */
	public void OnDestroy()
	{
		ComUtil.AssignVal(ref m_oOpenAni, null);
	}

	/** 상태를 갱신한다 */
	public void Update()
	{
		// 카운팅이 불가능 할 경우
		if (!m_bIsCounting)
		{
			return;
		}

		m_fRemainTime = Mathf.Max(0.0f, m_fRemainTime - Time.deltaTime);
		this.UpdateUIsState();

		// 일정 시간이 지났을 경우
		if (m_fRemainTime.ExIsLessEquals(0.0f))
		{
			this.OnClickCancelBtn();
		}
	}

	/** 다시 오픈 되었을 경우 */
	public void Reopen()
	{
		m_bIsCounting = true;
	}

	/** 백 키를 처리한다 */
	public override void Escape()
	{
		// Do Something
	}

	/** 이어하기 버튼을 눌렀을 경우 */
	public void OnClickContinueBtn()
	{
		m_bIsCounting = false;

		// 크리스탈이 부족 할 경우
		if (GameManager.Singleton.invenMaterial.CalcTotalCrystal() < this.TotalPrice)
		{
			MenuManager.Singleton.OpenPopup<PopupShopCrystal>(EUIPopup.PopupShopCrystal, true);
		}
		else
		{
			StartCoroutine(this.ConsumeCrystal(this.TotalPrice));
		}
	}

	/** 무료 이어하기 버튼을 눌렀을 경우 */
	public void OnClickContinueBtnAds()
	{
		m_bIsCounting = false;

		// VIP 일 경우
		if (GameManager.Singleton.user.IsVIP())
		{
			this.OnReceiveShowRewardAdsCallback(null, default, true);
		}
		else
		{
			C3rdPartySDKManager.Singleton.ShowRewardAds(this.OnReceiveShowRewardAdsCallback);
		}
	}

	/** 무료 이어하기 버튼을 눌렀을 경우 */
	public void OnClickContinueBtnFree()
	{
		m_bIsCounting = false;
		var wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

		StartCoroutine(this.OnCompleteConsumeCrystal(wait));
	}

	/** 보상 광고 출력 콜백을 수신했을 경우 */
	private void OnReceiveShowRewardAdsCallback(CAdsManager a_oSender, CAdsManager.STAdsRewardInfo a_stRewardInfo, bool a_bIsSuccess)
	{
		GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ADSView(EADSViewType.Revive, a_bIsSuccess));

		// 광고 시청 성공일 때
		if (a_bIsSuccess)
		{
			var wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);
			StartCoroutine(this.OnCompleteConsumeCrystal(wait));
		}
		else
		{
			m_bIsCounting = true;
		}
	}

	/** 크리스탈 소비가 완료되었을 경우 */
	private IEnumerator OnCompleteConsumeCrystal(PopupWait4Response a_oWait)
	{
		yield return YieldInstructionCache.WaitForEndOfFrame;
		var eMapInfoType = (EMapInfoType)(GameDataManager.Singleton.PlayMode - 1);

#if DISABLE_THIS
		switch (GameDataManager.Singleton.PlayMode)
		{
			case EPlayMode.HUNT: eMapInfoType = EMapInfoType.HUNT; break;
			case EPlayMode.CAMPAIGN: eMapInfoType = EMapInfoType.CAMPAIGN; break;
			case EPlayMode.TUTORIAL: eMapInfoType = EMapInfoType.TUTORIAL; break;
			case EPlayMode.ADVENTURE: eMapInfoType = EMapInfoType.ADVENTURE; break;
		}
#endif // #if DISABLE_THIS

		// 테스트 모드 일 경우
		if (eMapInfoType == EMapInfoType.NONE)
		{
			goto POPUP_BATTLE_CONTINUE_ON_COMPLETE_CONSUME_CRYSTAL_EXIT;
		}

		// 사냥 모드 일 경우
		if (GameDataManager.Singleton.PlayMode == EPlayMode.HUNT)
		{
			yield return GameManager.Singleton.user.Revive(eMapInfoType, 0, GameDataManager.Singleton.PlayHuntLV);
		}
		else
		{
			bool bIsWaveMode = GameDataManager.Singleton.PlayMode == EPlayMode.DEFENCE || 
				GameDataManager.Singleton.PlayMode == EPlayMode.INFINITE;

			var stIDInfo = GameDataManager.Singleton.PlayMapInfo.m_stIDInfo;
			int nExtraChapterID = bIsWaveMode ? 0 : 1;
			
			yield return GameManager.Singleton.user.Revive(eMapInfoType, stIDInfo.m_nEpisodeID, stIDInfo.m_nChapterID + nExtraChapterID);
		}

	POPUP_BATTLE_CONTINUE_ON_COMPLETE_CONSUME_CRYSTAL_EXIT:
		a_oWait?.Close();
		this.Params.m_oCallback?.Invoke(this, true);
	}

	/** 크리스탈을 소비한다 */
	private IEnumerator ConsumeCrystal(int a_nNumCrystals)
	{
		var wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);
		yield return m_InvenMaterial.ConsumeCrystal(a_nNumCrystals);

		yield return this.OnCompleteConsumeCrystal(wait);
	}

	/** 취소 버튼을 눌렀을 경우 */
	public void OnClickCancelBtn()
	{
		this.Params.m_oCallback?.Invoke(this, false);
	}

	/** UI 상태를 갱신한다 */
	private void UpdateUIsState()
	{
		int nNumAdsTicket = GameManager.Singleton.invenMaterial.GetItemCount(m_nAdsTicketKey);

		bool bIsVIP = GameManager.Singleton.user.IsVIP();
		bool bIsReverseAdsTicket = !bIsVIP && nNumAdsTicket > 0;

		m_oVIPIconUIs.SetActive(bIsVIP);
		m_oTicketIconUIs.SetActive(bIsReverseAdsTicket);
		m_oAdsIconUIs.SetActive(!bIsVIP && !bIsReverseAdsTicket);

		// 무료 부활이 가능 할 경우
		bool bIsEnableFreeRevive = GameDataManager.Singleton.PlayMapInfo.m_stIDInfo.m_nEpisodeID <= 0 &&
			GameDataManager.Singleton.PlayMode == EPlayMode.TUTORIAL;

		m_oContinueBtnFree.gameObject.SetActive(bIsEnableFreeRevive);

		m_oContinueBtn.gameObject.SetActive(!bIsEnableFreeRevive);
		m_oContinueBtnAds.gameObject.SetActive(!bIsEnableFreeRevive && this.Params.m_nTimes <= 0);

		// 텍스트를 갱신한다 {
		int nCount = (int)m_fRemainTime + 1;
		int nRemainTimes = Mathf.Max(1, this.Params.m_nMaxTimes - this.Params.m_nTimes);

		m_oCountText.text = $"{nCount}";
		m_oPriceText.text = $"{this.TotalPrice}";
		m_oTimesText.text = $"{UIStringTable.GetValue("ui_popup_battlecontinue_remain_count")}<color=#ffff00>{nRemainTimes}/{this.Params.m_nMaxTimes}</color>";
		m_oNumTicketsText.text = $"{nNumAdsTicket}";
		m_oNumCrystalsText.text = $"{GameManager.Singleton.invenMaterial.CalcTotalCrystal()}";
		// 텍스트를 갱신한다 }

		// 이미지를 갱신한다
		m_oGaugeImg.fillAmount = m_fRemainTime / ComType.G_MAX_TIME_CONTINUE;
	}
	#endregion // 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(int a_nTimes, int a_nMaxTimes, int a_nStandardPrice, System.Action<PopupBattleContinue, bool> a_oCallback)
	{
		return new STParams()
		{
			m_nTimes = a_nTimes,
			m_nMaxTimes = a_nMaxTimes,
			m_nStandardPrice = a_nStandardPrice,
			m_oCallback = a_oCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}
