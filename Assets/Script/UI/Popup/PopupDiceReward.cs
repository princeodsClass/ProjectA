using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

/** 주사위 보상 팝업 */
public class PopupDiceReward : UIDialog, IPointerDownHandler
{
	/** 매개 변수 */
	public struct STParams
	{
		public DiceTable m_oTable;
		public System.Action<PopupDiceReward> m_oCallback;
	}

	#region 변수
	[SerializeField] private List<SlotDiceReward> m_oSlotDiceRewardList = new List<SlotDiceReward>();
	[SerializeField] private List<SlotDiceReward> m_oFullSlotDiceRewardList = new List<SlotDiceReward>();

	private int m_nSelIdx = -1;
	private bool m_bIsSpin = false;
	private Sequence m_oSpinAni = null;

	private List<int> m_oSelRewardIdxList = new List<int>();
	private List<RewardListTable> m_oRewardTableList = new List<RewardListTable>();
	private List<RewardListTable> m_oFullRewardTableList = new List<RewardListTable>();

	[Header("=====> UIs <=====")]
	[SerializeField] private TMP_Text m_oGuideText = null;
	[SerializeField] private TMP_Text m_oPriceText = null;
	[SerializeField] private TMP_Text m_oTicketCountText = null;
	[SerializeField] private SoundButton m_oCloseBtn = null;
	[SerializeField] private SoundButton m_oRespinAdsBtn = null;
	[SerializeField] private List<ContentSizeFitter> m_oSizeFitterList = new List<ContentSizeFitter>();

	[Header("=====> Game Objects <=====")]
	[SerializeField] private GameObject m_oFreeUIs = null;
	[SerializeField] private GameObject m_oVIPUIs = null;
	[SerializeField] private GameObject m_oAdsUIs = null;
	[SerializeField] private GameObject m_oTicketUIs = null;

	[SerializeField] private GameObject m_oSpinUIs = null;
	[SerializeField] private GameObject m_oRespinUIs = null;
	[SerializeField] private GameObject m_oCompleteUIs = null;

	[SerializeField] private GameObject m_oBtnUIs = null;
	[SerializeField] private List<GameObject> m_oRewardList = new List<GameObject>();
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	private int RespinPrice => this.Params.m_oTable.AgainMaterialCount;
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
	public void Awake()
	{
		base.Initialize();
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
		var oRewardList = RewardTable.GetGroup(a_stParams.m_oTable.RewardGroup);

		var oRewardTableList = RewardListTable.GetGroup(oRewardList[0].RewardListGroup);
		m_oFullRewardTableList = oRewardTableList;

		m_oRewardTableList.Clear();
		m_oSelRewardIdxList.Clear();

		// 콜백이 존재 할 경우
		if (a_stParams.m_oCallback != null)
		{
			oRewardTableList.ExShuffle();
		}

		for (int i = 0; i < m_oRewardList.Count; ++i)
		{
			m_oRewardList[i].SetActive(true);
		}

		for (int i = 0; i < m_oFullSlotDiceRewardList.Count; ++i)
		{
			m_oFullSlotDiceRewardList[i].gameObject.SetActive(false);
		}

		for (int i = 0; i < oRewardTableList.Count && i < m_oSlotDiceRewardList.Count; ++i)
		{
			m_oSlotDiceRewardList[i].SetIsSel(false);
			m_oSlotDiceRewardList[i].gameObject.SetActive(true);

			m_oRewardTableList.Add(oRewardTableList[i]);
		}

		// 콜백이 없을 경우
		if (a_stParams.m_oCallback == null)
		{
			for (int i = m_oSlotDiceRewardList.Count; i < oRewardTableList.Count && i < m_oFullSlotDiceRewardList.Count; ++i)
			{
				m_oFullSlotDiceRewardList[i].SetIsSel(false);
				m_oFullSlotDiceRewardList[i].SetIsCheck(false);

				m_oFullSlotDiceRewardList[i].gameObject.SetActive(true);
			}
		}
		else
		{
			for (int i = 1; i < m_oRewardList.Count; ++i)
			{
				m_oRewardList[i].SetActive(false);
			}
		}

		this.SetupRewardUIs();
		this.UpdateUIsState();
	}

	/** 초기화 */
	public void Start()
	{
		ComUtil.RebuildLayouts(this.gameObject);
	}

	/** 제거 되었을 경우 */
	public void OnDestroy()
	{
		ComUtil.AssignVal(ref m_oSpinAni, null);
	}

	/** 닫기 버튼을 눌렀을 경우 */
	public void OnTouchCloseBtn()
	{
		// 콜백이 존재 할 경우
		if (this.Params.m_oCallback != null)
		{
			return;
		}

		this.Close();
	}

	/** 돌리기 버튼을 눌렀을 경우 */
	public void OnTouchSpinBtn()
	{
		int nIdx = 0;
		int nIdxVal = m_oRewardTableList.Count * 5;
		int nExtraIdxVal = Random.Range(0, m_oRewardTableList.Count);

		int nFinalIdx = (nIdxVal + nExtraIdxVal) % m_oRewardTableList.Count;

		// 선택 된 보상 일 경우
		if (m_oSelRewardIdxList.Contains(nFinalIdx))
		{
			nExtraIdxVal += 1;
		}

		var oSequence = DOTween.Sequence();
		oSequence.Append(DOTween.To(() => nIdx, (a_nVal) => { m_nSelIdx = a_nVal % m_oRewardTableList.Count; this.UpdateUIsState(); }, nIdxVal + nExtraIdxVal, 5.0f).SetEase(Ease.OutQuad));
		oSequence.AppendCallback(() => this.OnCompleteSpinAni(oSequence));

		m_bIsSpin = true;
		this.UpdateUIsState();

		ComUtil.AssignVal(ref m_oSpinAni, oSequence);
	}

	/** 취소 버튼을 눌렀을 경우 */
	public void OnTouchCancelBtn()
	{
		this.OnTouchCompleteBtn();
	}

	/** 완료 버튼을 눌렀을 경우 */
	public void OnTouchCompleteBtn()
	{
		StartCoroutine(this.CoAcquireRewards());
	}

	/** 보상을 획득한다 */
	private IEnumerator CoAcquireRewards()
	{
		var oWaitPopup = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);
		yield return null;

		for (int i = 0; i < m_oSelRewardIdxList.Count; ++i)
		{
			int nIdx = m_oSelRewardIdxList[i];
			int nNumItems = Random.Range(m_oRewardTableList[nIdx].RewardCountMin, m_oRewardTableList[nIdx].RewardCountMax + 1);

			yield return GameManager.Singleton.AddItemCS(m_oRewardTableList[nIdx].RewardKey, nNumItems);
		}

		oWaitPopup.Close();

		this.Close();
		this.Params.m_oCallback?.Invoke(this);
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
		if (GameManager.Singleton.invenMaterial.CalcTotalCrystal() < this.RespinPrice)
		{
			MenuManager.Singleton.OpenPopup<PopupShopCrystal>(EUIPopup.PopupShopCrystal, true);
		}
		else
		{
			StartCoroutine(this.ConsumeCrystal(this.RespinPrice));
		}
	}

	/** UI 상태를 갱신한다 */
	private void UpdateUIsState()
	{
		bool bIsFirstSpin = m_oSelRewardIdxList.Count <= 0;
		bool bIsCompleteSpin = m_oSelRewardIdxList.Count > this.Params.m_oTable.AbleToAgain;

		m_oBtnUIs.SetActive(this.Params.m_oCallback != null);
		m_oSpinUIs.SetActive(!m_bIsSpin && bIsFirstSpin && !bIsCompleteSpin);
		m_oRespinUIs.SetActive(!m_bIsSpin && !bIsFirstSpin && !bIsCompleteSpin);
		m_oCompleteUIs.SetActive(!m_bIsSpin && bIsCompleteSpin);

		m_oCloseBtn.gameObject.SetActive(this.Params.m_oCallback == null);
		m_oGuideText.gameObject.SetActive(this.Params.m_oCallback == null);

		m_oPriceText.text = $"{this.RespinPrice}";
		m_oRespinAdsBtn.gameObject.SetActive(this.Params.m_oTable.AbleToADAgain != 0);

		SetTicket();
		SetVIPMarker();

		for (int i = 0; i < m_oSlotDiceRewardList.Count; ++i)
		{
			m_oSlotDiceRewardList[i].SetIsSel(m_nSelIdx == i);
			m_oSlotDiceRewardList[i].SetIsCheck(m_oSelRewardIdxList.Contains(i));
		}

		ComUtil.RebuildLayouts(this.gameObject);
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

	public void OnClickTicket()
	{
		PopupMaterial pop = m_MenuMgr.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial, true);
		pop.InitializeInfo(new ItemMaterial(0, GlobalTable.GetData<uint>("valueAdsRemoveTicketDecKey"), 0));
	}

	/** 보상 UI 를 설정한다 */
	private void SetupRewardUIs()
	{
		for (int i = 0; i < m_oRewardTableList.Count && i < m_oSlotDiceRewardList.Count; ++i)
		{
			m_oSlotDiceRewardList[i].Init(SlotDiceReward.MakeParams(m_oRewardTableList[i].RewardKey, m_oRewardTableList[i]));
		}

		for (int i = m_oSlotDiceRewardList.Count; i < m_oFullRewardTableList.Count && i < m_oFullSlotDiceRewardList.Count; ++i)
		{
			m_oFullSlotDiceRewardList[i].Init(SlotDiceReward.MakeParams(m_oFullRewardTableList[i].RewardKey, m_oFullRewardTableList[i]));
		}
	}

	/** 스핀 애니메이션이 완료 되었을 경우 */
	private void OnCompleteSpinAni(Sequence a_oSender)
	{
		a_oSender?.Kill();
		m_oSelRewardIdxList.ExAddVal(m_nSelIdx);

		m_bIsSpin = false;
		this.UpdateUIsState();
	}

	/** 보상 광고 출력 콜백을 수신했을 경우 */
	private void OnReceiveShowRewardAdsCallback(CAdsManager a_oSender, CAdsManager.STAdsRewardInfo a_stRewardInfo, bool a_bIsSuccess)
	{
		GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ADSView(EADSViewType.Dice, a_bIsSuccess));

		// 광고 시청 성공일 때
		if (a_bIsSuccess)
		{
			this.OnTouchSpinBtn();
			SetTicket();
		}
	}

	/** 백 키를 처리한다 */
	public override void Escape()
	{
		// 콜백이 존재 할 경우
		if (this.Params.m_oCallback != null)
		{
			return;
		}

		base.Escape();
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

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(DiceTable a_oTable, System.Action<PopupDiceReward> a_oCallback)
	{
		return new STParams()
		{
			m_oTable = a_oTable,
			m_oCallback = a_oCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}
