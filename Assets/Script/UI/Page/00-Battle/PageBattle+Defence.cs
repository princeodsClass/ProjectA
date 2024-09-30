using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/** 전투 페이지 - 방어전 */
public partial class PageBattle : UIDialog
{
	/** 광고 UI */
	[System.Serializable]
	private struct STAdsUIs
	{
		public TMP_Text m_oBoostText;
		public GameObject m_oIconUIs;

		public Image m_oAdsIconImg;
		public Image m_oVIPIconImg;
		public Image m_oTicketIconImg;
	}

	#region 변수
	[Header("=====> Page Battle - Etc (Defence) <=====")]
	[SerializeField] private Animator m_oDefenceWaveNotiAnimator = null;

	private bool m_bIsBoost = false;
	private bool m_bIsEnableBoost = false;

	private uint m_nAdsTicketKey = 0;

	[Header("=====> Page Battle - UIs (Defence) <=====")]
	[SerializeField] private STAdsUIs m_oBoostAdsUIs;

	[SerializeField] private TMP_Text m_oBoostGuideText = null;
	[SerializeField] private TMP_Text m_oNumAdsTicketsText = null;

	[SerializeField] private TMP_Text m_oDefenceTitleText = null;
	[SerializeField] private TMP_Text m_oDefenceNotiTitleText = null;
	[SerializeField] private TMP_Text m_oDefenceWarningText = null;

	[Header("=====> Page Battle - Game Objects (Defence) <=====")]
	[SerializeField] private GameObject m_oBoostUIs = null;
	[SerializeField] private GameObject m_oDefenceUIs = null;
	[SerializeField] private GameObject m_oDefenceWarningUIs = null;
	[SerializeField] private GameObject m_oDefenceWaveNotiUIs = null;
	#endregion // 변수

	#region 프로퍼티
	public bool IsBoost => m_bIsBoost;
	public TMP_Text DefenceTitleText => m_oDefenceTitleText;
	public TMP_Text DefenceWarningText => m_oDefenceWarningText;

	public GameObject DefenceWarningUIs => m_oDefenceWarningUIs;
	public GameObject DefenceWaveNotiUIs => m_oDefenceWaveNotiUIs;
	#endregion // 프로퍼티

	#region 함수
	/** 부스트 버튼을 눌렀을 경우 */
	public void OnTouchBoostBtn()
	{
		// 부스트 모드가 가능 할 경우
		if (m_bIsEnableBoost)
		{
			m_bIsBoost = !m_bIsBoost;
			this.SetIsDirtyUpdateUIsState(true);

			ComUtil.SetTimeScale(m_bIsBoost ? 2.0f : 1.0f, true);
		}
		else
		{
			C3rdPartySDKManager.Singleton.ShowRewardAds(this.OnReceiveShowRewardAdsCallback);
		}
	}

	/** 방어전 웨이브 시작 연출을 시작한다 */
	public void StartDefenceWaveStartDirecting(int a_nWaveOrder, float a_fDuration)
	{
		int nWaveOrder = Mathf.Min(a_nWaveOrder + 1, this.BattleController.DefenceTableList.Count);
		m_oDefenceNotiTitleText.text = $"Wave {nWaveOrder}";

		m_oDefenceWaveNotiUIs.SetActive(true);
		m_oDefenceWaveNotiAnimator.SetTrigger("Start");
	}

	/** 방어전 웨이브 클리어 연출을 시작한다 */
	public void StartDefenceWaveClearDirecting(int a_nWaveOrder, float a_fDuration)
	{
		string oClearMsg = UIStringTable.GetValue("ui_popup_battlereward_titleresult_success");
		string oDefenceName = this.BattleController.DefenceName;

		m_oDefenceNotiTitleText.text = oClearMsg;
		this.DefenceTitleText.text = $"{oDefenceName}\n{oClearMsg}";

		m_oDefenceWaveNotiUIs.SetActive(true);
		m_oDefenceWaveNotiAnimator.SetTrigger("Start");
	}

	/** 방어전 UI 상태를 갱신한다 */
	private void UpdateDefenceUIsState()
	{
		int nNumAdsTicket = GameManager.Singleton.invenMaterial.GetItemCount(m_nAdsTicketKey);

		bool bIsVIP = GameManager.Singleton.user.IsVIP();
		bool bIsReverseAdsTicket = !bIsVIP && nNumAdsTicket > 0;

		m_oNumAdsTicketsText.text = $"{nNumAdsTicket}";
		m_oBoostAdsUIs.m_oBoostText.text = m_bIsBoost ? "x1" : "x2";

		m_oBoostAdsUIs.m_oVIPIconImg.gameObject.SetActive(bIsVIP);
		m_oBoostAdsUIs.m_oTicketIconImg.gameObject.SetActive(bIsReverseAdsTicket);
		m_oBoostAdsUIs.m_oAdsIconImg.gameObject.SetActive(!bIsVIP && !bIsReverseAdsTicket);

		m_oBoostAdsUIs.m_oIconUIs.SetActive(!m_bIsEnableBoost);
		m_oBoostGuideText.gameObject.SetActive(m_bIsBoost);
	}

	/** 보상 광고 출력 콜백을 수신했을 경우 */
	private void OnReceiveShowRewardAdsCallback(CAdsManager a_oSender, CAdsManager.STAdsRewardInfo a_stRewardInfo, bool a_bIsSuccess)
	{
		// 광고 시청에 실패했을 경우
		if(!a_bIsSuccess)
		{
			return;
		}

		m_bIsEnableBoost = true;

		this.OnTouchBoostBtn();
		GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ADSView(EADSViewType.DefenceSpeed));
	}
	#endregion // 함수
}
