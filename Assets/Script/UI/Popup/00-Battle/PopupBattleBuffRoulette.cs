using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

/** 전투 버프 룰렛 팝업 */
public class PopupBattleBuffRoulette : UIDialog, IPointerDownHandler
{
	/** 매개 변수 */
	public struct STParams
	{
		public int m_nRespinPrice;
		public uint m_nBonusEffectGroup;

		public System.Action<PopupBattleBuffRoulette, List<EffectTable>> m_oCallback;
	}

	#region 변수
	[SerializeField] private float m_fBuffUIsOffset = 150.0f;
	[SerializeField] private AudioClip m_oRouletteSpinAudioClip = null;

	private bool m_bIsSpin = false;
	private Sequence m_oSpinAni = null;
	private Sequence m_oBlinkAni = null;
	private Sequence m_oShuffleAni = null;

	private List<uint> m_oBuffKeyList = new List<uint>();
	private List<uint> m_oApplyBuffKeyList = new List<uint>();

	[Header("=====> UIs <=====")]
	[SerializeField] private TMP_Text m_oPriceText = null;
	[SerializeField] private TMP_Text m_oTicketCountText = null;
	[SerializeField] private List<STApplyBuffUIs> m_oApplyBuffUIsList = new List<STApplyBuffUIs>();

	[Header("=====> Game Objects <=====")]
	[SerializeField] private GameObject m_oBase = null;
	[SerializeField] private GameObject m_oVIPUIs = null;
	[SerializeField] private GameObject m_oFreeUIs = null;
	[SerializeField] private GameObject m_oAdsUIs = null;
	[SerializeField] private GameObject m_oTicketUIs = null;

	[SerializeField] private GameObject m_oSpinUIs = null;
	[SerializeField] private GameObject m_oRespinUIs = null;
	[SerializeField] private GameObject m_oRouletteUIs = null;
	[SerializeField] private GameObject m_oCompleteUIs = null;

	[SerializeField] private List<GameObject> m_oRouletteBuffUIsList = new List<GameObject>();
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	private float RouletteBuffUIsAngleOffset => 360.0f / m_oRouletteBuffUIsList.Count;
	#endregion // 프로퍼티

	#region IPointerDownHandler
	/** 터치를 시작했을 경우 */
	public void OnPointerDown(PointerEventData a_oEventData)
	{
		// 스핀 상태가 아닐 경우
		if (!m_bIsSpin)
		{
			return;
		}

		m_oSpinAni.timeScale = 10.0f;
	}
	#endregion // IPointerDownHandler

	#region 함수
	/** 초기화 */
	private void Awake()
	{
		base.Initialize();
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
		m_oBase.transform.localScale = Vector3.zero;
	}

	/** 초기화 */
	public void Start()
	{
		this.SetupBuffs();
		this.UpdateUIsState();

		bool bIsImmediateSpin = GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.CAMPAIGN;
		bIsImmediateSpin = bIsImmediateSpin || GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.TUTORIAL;

		// 즉시 모드 일 경우
		if (bIsImmediateSpin)
		{
			this.OnTouchSpinBtn();
		}
	}

	/** 제거 되었을 경우 */
	public void OnDestroy()
	{
		ComUtil.AssignVal(ref m_oSpinAni, null);
		ComUtil.AssignVal(ref m_oBlinkAni, null);
		ComUtil.AssignVal(ref m_oShuffleAni, null);
	}

	/** 닫혔을 경우 */
	public override void Close()
	{
		base.Close();
		ComUtil.SetTimeScale(1.0f, false);
	}

	/** 백 키를 처리한다 */
	public override void Escape()
	{
		// Do Something
	}

	/** 플레이 버튼을 눌렀을 경우 */
	public void OnTouchPlayBtn()
	{
		var oEffectTableList = new List<EffectTable>();
		m_oApplyBuffKeyList.ExCopyTo(oEffectTableList, (a_nKey) => EffectTable.GetData(a_nKey));

		this.Params.m_oCallback?.Invoke(this, oEffectTableList);
	}

	/** 돌리기 버튼을 눌렀을 경우 */
	public void OnTouchSpinBtn()
	{
		float fAngle = 360.0f * 5.0f;
		float fExtraAngle = Random.Range(0.0f, 360.0f);

		GameAudioManager.PlaySFX(m_oRouletteSpinAudioClip, mixerGroup: "Master/SFX/ETC");

		var oSequence = DOTween.Sequence().SetUpdate(true);
		oSequence.Append(m_oRouletteUIs.transform.DORotate(Vector3.forward * (fAngle + fExtraAngle), 5.0f, RotateMode.WorldAxisAdd).SetEase(Ease.OutQuad).SetUpdate(true));
		oSequence.AppendCallback(() => this.OnCompleteSpinAni(oSequence));

		m_bIsSpin = true;
		this.UpdateUIsState();

		ComUtil.AssignVal(ref m_oSpinAni, oSequence);
	}

	/** 취소 버튼을 눌렀을 경우 */
	public void OnTouchCancelBtn()
	{
		this.OnTouchPlayBtn();
	}

	/** 다시 돌리기 광고 버튼을 눌렀을 경우 */
	public void OnTouchRespinAdsBtn()
	{
		// VIP 일 경우
		if (GameManager.Singleton.user.IsVIP())
		{
			this.OnReceiveShowRewardAdsCallback(null, default, true);
		}
		else
		{
#if UNITY_EDITOR
			this.OnReceiveShowRewardAdsCallback(null, default, true);
#else
			C3rdPartySDKManager.Singleton.ShowRewardAds(this.OnReceiveShowRewardAdsCallback);
#endif // #if UNITY_EDITOR
		}
	}

	/** 다시 돌리기 재화 버튼을 눌렀을 경우 */
	public void OnTouchRespinGoodsBtn()
	{
		// 크리스탈이 부족 할 경우
		if (GameManager.Singleton.invenMaterial.CalcTotalCrystal() < this.Params.m_nRespinPrice)
		{
			MenuManager.Singleton.OpenPopup<PopupShopCrystal>(EUIPopup.PopupShopCrystal, true);
		}
		else
		{
			StartCoroutine(this.ConsumeCrystal(this.Params.m_nRespinPrice));
		}
	}

	/** UI 상태를 갱신한다 */
	private void UpdateUIsState()
	{
		bool bIsFirstSpin = m_oApplyBuffKeyList.Count <= 0;
		bool bIsCompleteSpin = m_oApplyBuffKeyList.Count >= m_oApplyBuffUIsList.Count;

		m_oSpinUIs.SetActive(!m_bIsSpin && bIsFirstSpin && !bIsCompleteSpin);
		m_oRespinUIs.SetActive(!m_bIsSpin && !bIsFirstSpin && !bIsCompleteSpin);
		m_oCompleteUIs.SetActive(!m_bIsSpin && bIsCompleteSpin);

		m_oPriceText.text = $"{this.Params.m_nRespinPrice}";
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_oPriceText.rectTransform);

		SetTicket();

		for (int i = 0; i < m_oRouletteBuffUIsList.Count; ++i)
		{
			var oEffectTable = EffectTable.GetData(m_oBuffKeyList[i]);

			var oGlowImg = m_oRouletteBuffUIsList[i].transform.Find("GlowImg").GetComponent<Image>();
			oGlowImg.color = new Color(oGlowImg.color.r, oGlowImg.color.g, oGlowImg.color.b, 0.0f);

			var oIconImg = m_oRouletteBuffUIsList[i].transform.Find("IconImg").GetComponent<Image>();
			oIconImg.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Common, oEffectTable.Icon);
		}

		for (int i = 0; i < m_oApplyBuffUIsList.Count; ++i)
		{
			bool bIsActive = i < m_oApplyBuffKeyList.Count;
			m_oApplyBuffUIsList[i].m_oEmptyUIs.SetActive(!bIsActive);
			m_oApplyBuffUIsList[i].m_oEmptyText.gameObject.SetActive(!bIsActive);

			m_oApplyBuffUIsList[i].m_oActiveUIs.SetActive(bIsActive);
			m_oApplyBuffUIsList[i].m_oDescText.gameObject.SetActive(bIsActive);
			m_oApplyBuffUIsList[i].m_oNameText.gameObject.SetActive(bIsActive);

			m_oApplyBuffUIsList[i].m_oDescText.text = string.Empty;

			// 액티브 상태 일 경우
			if (bIsActive)
			{
				uint nKey = m_oApplyBuffKeyList[i];
				var oEffectTable = EffectTable.GetData(nKey);

				int nVal = oEffectTable.Value.ExIsLessEquals(1.0f) ?
					(int)(oEffectTable.Value * 100.0f) : (int)oEffectTable.Value;

				string oName = NameTable.GetValue(oEffectTable.NameKey);
				string oOperator = oEffectTable.Value.ExIsGreatEquals(0.0f) ? "+" : "";

				m_oApplyBuffUIsList[i].m_oIconImg.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Common, oEffectTable.Icon);
				m_oApplyBuffUIsList[i].m_oNameText.text = oName;

				m_oApplyBuffUIsList[i].m_oDescText.text = oEffectTable.Value.ExIsLessEquals(1.0f) ?
					$"<color=#FFD02A>{oOperator}{nVal} %</color>" : $"<color=#FFD02A>{oOperator}{nVal}</color>";
			}

			LayoutRebuilder.ForceRebuildLayoutImmediate(m_oApplyBuffUIsList[i].m_oNameText.rectTransform);
			LayoutRebuilder.ForceRebuildLayoutImmediate(m_oApplyBuffUIsList[i].m_oDescText.rectTransform);
		}
	}

	public void Update()
	{
		ComUtil.SetTimeScale(0.00001f, true);
	}

	public void SetTicket()
	{
		m_oTicketCountText.text = m_InvenMaterial.GetItemCount(GlobalTable.GetData<uint>("valueAdsRemoveTicketDecKey")).ToString();
		SetVIPMarker();
	}

	void SetVIPMarker()
	{
		if (m_Account.IsVIP())
		{
			m_oVIPUIs.SetActive(true);
			m_oAdsUIs.SetActive(false);
			m_oFreeUIs.SetActive(false);
			m_oTicketUIs.SetActive(false);
		}
		else
		{
			m_oVIPUIs.SetActive(false);
			m_oFreeUIs.SetActive(false);

			m_oTicketUIs.SetActive(int.Parse(m_oTicketCountText.text) > 0); ;
			m_oAdsUIs.SetActive(!m_oTicketUIs.activeSelf);
		}
	}

	/** 버프를 설정한다 */
	private void SetupBuffs()
	{
		var oEffectKeyList = CCollectionPoolManager.Singleton.SpawnList<uint>();

		try
		{
			var oEffectGroupTableList = EffectGroupTable.GetGroup((int)this.Params.m_nBonusEffectGroup);
			oEffectGroupTableList.ExCopyTo(oEffectKeyList, (a_oEffectGroupTable) => a_oEffectGroupTable.EffectKey);

			m_oBuffKeyList.Clear();
			oEffectKeyList.ExShuffle();

			while (m_oBuffKeyList.Count < m_oRouletteBuffUIsList.Count)
			{
				uint nKey = oEffectKeyList[m_oBuffKeyList.Count];

#if DISABLE_THIS
				// 적용 중인 버프 일 경우
				if (m_oApplyBuffKeyList.Contains(nKey))
				{
					continue;
				}
#endif // #if DISABLE_THIS

				m_oBuffKeyList.ExAddVal(nKey);
			}
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oEffectKeyList);
		}
	}

	/** 룰렛 UI 를 설정한다 */
	private void SetupRouletteUIs()
	{
		for (int i = 0; i < m_oRouletteBuffUIsList.Count; ++i)
		{
			float fAngle = 90.0f - (i * this.RouletteBuffUIsAngleOffset) - (this.RouletteBuffUIsAngleOffset / 2.0f);
			var stDirection = new Vector3(Mathf.Cos(fAngle * Mathf.Deg2Rad), Mathf.Sin(fAngle * Mathf.Deg2Rad), 0.0f);

			m_oRouletteBuffUIsList[i].transform.right = stDirection.normalized;
			(m_oRouletteBuffUIsList[i].transform as RectTransform).anchoredPosition = stDirection.normalized * m_fBuffUIsOffset;
		}
	}

	/** 보상 광고 출력 콜백을 수신했을 경우 */
	private void OnReceiveShowRewardAdsCallback(CAdsManager a_oSender, CAdsManager.STAdsRewardInfo a_stRewardInfo, bool a_bIsSuccess)
	{
		var eAdsViewType = (GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.CAMPAIGN) ?
			EADSViewType.Roulette : EADSViewType.AdventureRoulette;

		GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ADSView(eAdsViewType, a_bIsSuccess));

		// 광고 시청 상태 일 경우
		if (a_bIsSuccess)
		{
			SetTicket();
			this.OnTouchSpinBtn();
		}
	}

	/** 스핀 애니메이션이 완료 되었을 경우 */
	private void OnCompleteSpinAni(Sequence a_oSender)
	{
		a_oSender?.Kill();
		int nIdx = (int)(m_oRouletteUIs.transform.localEulerAngles.z.ExGet360Deg() / this.RouletteBuffUIsAngleOffset);

		var oGlowImg = m_oRouletteBuffUIsList[nIdx].transform.Find("GlowImg").GetComponent<Image>();

		var oSequence = DOTween.Sequence().SetUpdate(true);
		oSequence.Append(oGlowImg.DOColor(new Color(oGlowImg.color.r, oGlowImg.color.g, oGlowImg.color.b, 1.0f), 0.15f).SetLoops(6, LoopType.Yoyo).SetUpdate(true));
		oSequence.AppendCallback(() => this.OnCompleteBlinkAni(oSequence, nIdx));

		ComUtil.AssignVal(ref m_oBlinkAni, oSequence);
	}

	/** 블링크 애니메이션이 완료 되었을 경우 */
	private void OnCompleteBlinkAni(Sequence a_oSender, int a_nIdx)
	{
		a_oSender?.Kill();
		bool bIsFirstSpin = m_oApplyBuffKeyList.Count <= 0;

		m_oApplyBuffKeyList.Add(m_oBuffKeyList[a_nIdx]);

		// 최초 스핀이였을 경우
		if (bIsFirstSpin)
		{
			var oSequence = DOTween.Sequence().SetUpdate(true);
			oSequence.Append(m_oRouletteUIs.transform.DOScale(0.0f, 0.25f).SetUpdate(true));
			oSequence.AppendCallback(() => { this.SetupBuffs(); this.UpdateUIsState(); });
			oSequence.Append(m_oRouletteUIs.transform.DOScale(1.0f, 0.25f).SetUpdate(true));
			oSequence.AppendCallback(() => this.OnCompleteShuffleAni(oSequence));

			ComUtil.AssignVal(ref m_oShuffleAni, oSequence);
		}
		else
		{
			this.OnCompleteShuffleAni(null);
		}
	}

	/** 재배치 애니메이션이 완료 되었을 경우 */
	private void OnCompleteShuffleAni(Sequence a_oSender)
	{
		a_oSender?.Kill();

		m_bIsSpin = false;
		this.UpdateUIsState();
	}

	/// <summary>
	/// 티켓 팝업 처리 함수.
	/// </summary>
	public void OnClickTicket()
	{
		PopupMaterial pop = m_MenuMgr.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial, true);
		pop.InitializeInfo(new ItemMaterial(0, GlobalTable.GetData<uint>("valueAdsRemoveTicketDecKey"), 0));
	}
	#endregion // 함수

	#region 코루틴 함수
	/** 크리스탈을 소비한다 */
	private IEnumerator ConsumeCrystal(int a_nNumCrystals)
	{
		var wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);
		yield return m_InvenMaterial.ConsumeCrystal(a_nNumCrystals);

		wait.Close();
		this.OnTouchSpinBtn();
	}
	#endregion // 코루틴 함수

	#region 클래스 함수
#if UNITY_EDITOR
	/** 룰렛 버프 UI 설정 버튼을 눌렀을 경우 */
	[UnityEditor.MenuItem("CONTEXT/PopupBattleBuffRoulette/Setup Roulette Buff UIs")]
	private static void SetupRouletteBuffUIs(UnityEditor.MenuCommand a_oMenuCmd)
	{
		var oPopupBattleBuffRoulette = a_oMenuCmd.context as PopupBattleBuffRoulette;
		oPopupBattleBuffRoulette.SetupRouletteUIs();
	}
#endif // #if UNITY_EDITOR
	#endregion // 클래스 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(int a_nRespinPrice,
		uint a_nBonusEffectGroup, System.Action<PopupBattleBuffRoulette, List<EffectTable>> a_oCallback)
	{
		return new STParams()
		{
			m_nRespinPrice = a_nRespinPrice,
			m_nBonusEffectGroup = a_nBonusEffectGroup,

			m_oCallback = a_oCallback
		};
	}
}
#endregion // 클래스 팩토리 함수
