using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/** 무한 모드 전투 보상 팝업 */
public partial class PopupBattleInfiniteReward : MonoBehaviour
{
	#region 변수
	[Header("=====> Popup Battle Infinite Reward - Etc <=====")]
	private int m_nNumKills = 0;
	private int m_nBestNumKills = 0;

	private Tween m_oGaugeIncrAni = null;
	private Tween m_oBestRecordAni = null;

	[Header("=====> Popup Battle Infinite Reward - UIs <=====")]
	[SerializeField] private TMP_Text m_oNumKillsText = null;
	[SerializeField] private TMP_Text m_oBestNumKillsText = null;

	[SerializeField] private Slider m_oGaugeSlider = null;
	private PopupBattleReward m_oPopupBattleReward = null;

	[Header("=====> Popup Battle Infinite Reward - Game Objects <=====")]
	[SerializeField] private GameObject m_oExpUIs = null;
	[SerializeField] private GameObject m_oBoxUIs = null;
	[SerializeField] private GameObject m_oFailUIs = null;

	[SerializeField] private GameObject m_oMissUIs = null;
	[SerializeField] private GameObject m_oGaugeUIs = null;
	[SerializeField] private GameObject m_oButtonUIs = null;

	[SerializeField] private GameObject m_oBestRecordUIs = null;
	[SerializeField] private GameObject m_oNumKillsGaugeUIs = null;
	[SerializeField] private GameObject m_oBestNumKillsGaugeUIs = null;
	#endregion // 변수

	#region 프로퍼티
	private bool IsBestRecord => m_nNumKills > m_nBestNumKills;
	private int MaxNumKills => Mathf.Max(m_nNumKills, m_nBestNumKills);
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		m_oPopupBattleReward = this.GetComponent<PopupBattleReward>();
	}

	/** 초기화 */
	public void Init(int a_nNumKills, int a_nBestNumKills)
	{
		// 무한 모드가 아닐 경우
		if (!GameDataManager.Singleton.IsInfiniteWaveMode())
		{
			return;
		}

		m_nNumKills = a_nNumKills;
		m_nBestNumKills = a_nBestNumKills;

		this.UpdateUIsState();
		StartCoroutine(this.CoInit());
	}

	/** 제거되었을 경우 */
	public void OnDestroy()
	{
		ComUtil.AssignVal(ref m_oGaugeIncrAni, null);
		ComUtil.AssignVal(ref m_oBestRecordAni, null);
	}

	/** 상태를 갱신한다 */
	public void LateUpdate()
	{
		m_oBoxUIs.SetActive(false);
		m_oFailUIs.SetActive(false);
		m_oMissUIs.SetActive(false);
		m_oButtonUIs.SetActive(true);

		m_oExpUIs.SetActive(GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.CAMPAIGN || 
			GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.TUTORIAL);
			
		m_oGaugeUIs.SetActive(GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.INFINITE);
	}

	/** UI 상태를 갱신한다 */
	private void UpdateUIsState()
	{
		var stSize = (m_oGaugeUIs.transform as RectTransform).sizeDelta;
		float fPercent = m_nBestNumKills / (float)this.MaxNumKills;

		string oNumKillsStr = UIStringTable.GetValue("ui_component_mission_zombie_count");
		string oBestNumKillsStr = UIStringTable.GetValue("ui_component_mission_zombie_max_count");

		m_oGaugeSlider.value = 0.0f;
		m_oBestRecordUIs.SetActive(false);

		m_oNumKillsText.text = $"{oNumKillsStr} : {m_nNumKills}";
		m_oBestNumKillsText.text = $"{oBestNumKillsStr} : {m_nBestNumKills}";

		// 무한 모드 일 경우
		if (GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.INFINITE)
		{
			m_oPopupBattleReward.txtTitle.text = UIStringTable.GetValue("ui_component_mission_zombie_title");
		}

		(m_oBestNumKillsGaugeUIs.transform as RectTransform).anchoredPosition = new Vector2(stSize.x * fPercent,
			(m_oBestNumKillsGaugeUIs.transform as RectTransform).anchoredPosition.y);
	}

	/** 게이지 애니메이션이 완료 되었을 경우 */
	private void OnCompleteGaugeAni()
	{
		// 최고 기록이 아닐 경우
		if(!this.IsBestRecord)
		{
			return;
		}

		m_oBestRecordUIs.SetActive(true);
		m_oBestRecordUIs.transform.localScale = new Vector3(1.0f, 0.0f, 1.0f);

		var oAni = m_oBestRecordUIs.transform.DOScaleY(1.0f, 0.25f);
		ComUtil.AssignVal(ref m_oBestRecordAni, oAni);
	}

	/** 게이지 애니메이션을 시작한다 */
	private void StartGaugeAni()
	{
		float fPercent = m_nNumKills / (float)this.MaxNumKills;
		var oAni = DOTween.To(() => m_oGaugeSlider.value, (a_fVal) => m_oGaugeSlider.value = a_fVal, fPercent, 2.0f);

		oAni.OnComplete(this.OnCompleteGaugeAni);
		ComUtil.AssignVal(ref m_oGaugeIncrAni, oAni);
	}
	#endregion // 함수
}

/** 무한 모드 전투 보상 팝업 - 코루틴 */
public partial class PopupBattleInfiniteReward : MonoBehaviour
{
	#region 함수
	/** 초기화 */
	private IEnumerator CoInit()
	{
		yield return new WaitForSeconds(0.5f);
		this.StartGaugeAni();
	}
	#endregion // 함수
}
