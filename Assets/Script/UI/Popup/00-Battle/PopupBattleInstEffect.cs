using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/** 전투 인스턴스 효과 팝업 */
public partial class PopupBattleInstEffect : UIDialog
{
	/** 매개 변수 */
	public struct STParams
	{
		public StageTable m_oStageTable;
		public System.Action<PopupBattleInstEffect, EffectTable, float> m_oCallback;
	}

	#region 변수
	[Header("=====> Popup Battle Inst Effect - Etc <=====")]
	[SerializeField] private AudioClip m_oInstEffectSpinAudioClip = null;
	private bool m_bIsEnableRespin = true;

	private List<EffectTable> m_oEffectTableList = new List<EffectTable>();
	private Dictionary<EffectTable, float> m_oEffectTableDict = new Dictionary<EffectTable, float>();

	[Header("=====> Popup Battle Inst Effect - UIs <=====")]
	[SerializeField] private TMP_Text m_oTitleText = null;
	[SerializeField] private TMP_Text m_oTicketCountText = null;

	[Header("=====> Popup Battle Inst Effect - Game Objects <=====")]
	[SerializeField] private GameObject m_oAdsUIs = null;
	[SerializeField] private GameObject m_oVIPUIs = null;
	[SerializeField] private GameObject m_oFreeUIs = null;
	[SerializeField] private GameObject m_oTicketUIs = null;
	[SerializeField] private GameObject m_oRespinUIs = null;

	[SerializeField] private List<GameObject> m_oInstEffectUIsList = new List<GameObject>();
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	public AudioClip InstEffectSpinAudioClip => m_oInstEffectSpinAudioClip;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		this.Initialize();
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams, bool a_bIsRespin = false)
	{
		this.Params = a_stParams;
		m_bIsEnableRespin = !a_bIsRespin;

		var oEffectGroupTableList = EffectGroupTable.GetGroup((int)a_stParams.m_oStageTable.EffectGroupInstanceLvUp);
		m_oRespinUIs.SetActive(false);

		m_oEffectTableDict = EffectGroupTable.RandomEffectInGroup((int)a_stParams.m_oStageTable.EffectGroupInstanceLvUp, 3);
		m_oEffectTableList.Clear();


#if DISABLE_THIS
		// FIXME: 임시 코드
		m_oEffectTableDict.Clear();
		m_oEffectTableDict.Add(EffectTable.GetData(713155329), -0.15f);
		m_oEffectTableDict.Add(EffectTable.GetData(713036801), -0.15f);
		m_oEffectTableDict.Add(EffectTable.GetData(713503489), 0.15f);
#endif // #if DISABLE_THIS


		for (int i = 0; i < oEffectGroupTableList.Count; ++i)
		{
			m_oEffectTableList.Add(EffectTable.GetData(oEffectGroupTableList[i].EffectKey));
		}

		this.UpdateUIsState();
		ComUtil.RebuildLayouts(this.gameObject);

		for (int i = 0; i < m_oInstEffectUIsList.Count; ++i)
		{
			var oInstEffectUIsHandler = m_oInstEffectUIsList[i].GetComponentInChildren<InstEffectUIsHandler>();
			oInstEffectUIsHandler.SelBtn.interactable = false;

			m_oInstEffectUIsList[i].SetActive(a_bIsRespin);
			oInstEffectUIsHandler.StartShowAnim(i, m_oInstEffectSpinAudioClip, oEffectGroupTableList, a_bIsRespin);
		}

		StartCoroutine(this.CoInit());
	}

	/** 백 키를 처리한다 */
	public override void Escape()
	{
		// Do Something
	}

	/** UI 상태를 갱신한다 */
	public void UpdateUIsState()
	{
		int nIdx = 0;
		this.SetTicket();

		foreach (var stKeyVal in m_oEffectTableDict)
		{
			this.UpdateInstEffectUIsState(nIdx, stKeyVal.Value, stKeyVal.Key);
			nIdx += 1;
		}
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

			m_oTicketUIs.SetActive(int.Parse(m_oTicketCountText.text) > 0);
			m_oAdsUIs.SetActive(!m_oTicketUIs.activeSelf);
		}
	}

	/** 인스턴스 효과 UI 상태를 갱신한다 */
	private void UpdateInstEffectUIsState(int a_nIdx, float a_fVal, EffectTable a_oEffectTable)
	{
		float fEnhanceVal = a_fVal.ExIsLess(1.0f) ? a_fVal * 100.0f : a_fVal;
		string oPostfixStr = a_fVal.ExIsLess(1.0f) ? "%" : string.Empty;

		var oInstEffectUIsHandler = m_oInstEffectUIsList[a_nIdx].GetComponentInChildren<InstEffectUIsHandler>();
		oInstEffectUIsHandler.IconImg.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Common, a_oEffectTable.Icon);

		oInstEffectUIsHandler.NameText.text = NameTable.GetValue(a_oEffectTable.NameKey);
		oInstEffectUIsHandler.DescText.text = a_fVal.ExIsLess(1.0f) ? $"<color=#FFD02A>{fEnhanceVal:0.00}{oPostfixStr}</color>" : $"<color=#FFD02A>{fEnhanceVal:0}{oPostfixStr}</color>";
		oInstEffectUIsHandler.DetailDescText.text = DescTable.GetValue(a_oEffectTable.DescKey);

		oInstEffectUIsHandler.SelBtn.onClick.RemoveAllListeners();
		oInstEffectUIsHandler.SelBtn.onClick.AddListener(() => this.OnTouchInstEffectUIsSelBtn(a_fVal, a_oEffectTable));
	}

	/** 보상 광고 출력 콜백을 수신했을 경우 */
	private void OnReceiveShowRewardAdsCallback(CAdsManager a_oSender, CAdsManager.STAdsRewardInfo a_stRewardInfo, bool a_bIsSuccess)
	{
		GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ADSView(EADSViewType.InstanceSkill, a_bIsSuccess));

		// 광고 시청 상태 일 경우
		if (a_bIsSuccess)
		{
			this.SetTicket();
			this.Init(this.Params, true);
		}
	}

	/** 인스턴스 효과 선택 버튼을 눌렀을 경우 */
	private void OnTouchInstEffectUIsSelBtn(float a_fVal, EffectTable a_oEffectTable)
	{
		this.Params.m_oCallback?.Invoke(this, a_oEffectTable, a_fVal);
		this.Close();
	}
	#endregion // 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(StageTable a_oStageTable, System.Action<PopupBattleInstEffect, EffectTable, float> a_oCallback)
	{
		return new STParams()
		{
			m_oStageTable = a_oStageTable,
			m_oCallback = a_oCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}

/** 전투 인스턴스 효과 팝업 - 코루틴 */
public partial class PopupBattleInstEffect : UIDialog
{
	#region 함수
	/** 초기화 */
	private IEnumerator CoInit()
	{
		yield return new WaitForSecondsRealtime(3.5f);
		m_oRespinUIs.SetActive(m_bIsEnableRespin);

		for (int i = 0; i < m_oInstEffectUIsList.Count; ++i)
		{
			var oInstEffectUIsHandler = m_oInstEffectUIsList[i].GetComponentInChildren<InstEffectUIsHandler>();
			oInstEffectUIsHandler.SelBtn.interactable = true;
		}
	}
	#endregion // 함수
}
