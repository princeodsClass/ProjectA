using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** 캠페인 게이지 UI 처리자 */
public class CGaugeCampaignUIsHandler : CGaugeUIsHandler
{
	#region 변수
	[Header("=====> Gauge Campaign UIs Handler - UIs <=====")]
	[SerializeField] private Image m_oGaugeImg = null;
	[SerializeField] private Slider m_oGuideSlider = null;
	#endregion // 변수

	#region 함수
	/** 초기화 */
	public override void Awake()
	{
		base.Awake();
		this.SetGaugePercent(0.0f, 0.0f);
	}
	#endregion // 함수

	#region 접근 함수
	/** 게이지 퍼센트를 변경한다 */
	public void SetGaugePercent(float a_fCurPercent, float a_fPrevPercent)
	{
		this.SetGaugeVal(a_fPrevPercent);
		this.SetMidGaugeVal(a_fCurPercent);

		m_oGuideSlider.value = a_fPrevPercent.ExIsLessEquals(0.0f) ? 
			Mathf.Clamp01(a_fCurPercent * 0.75f) : Mathf.Clamp01(a_fCurPercent - (a_fCurPercent - a_fPrevPercent) / 2.0f);
	}
	#endregion // 접근 함수
}
