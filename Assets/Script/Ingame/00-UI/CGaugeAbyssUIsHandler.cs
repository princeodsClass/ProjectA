using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** 심연 게이지 UI 처리자 */
public class CGaugeAbyssUIsHandler : CGaugeUIsHandler
{
	#region 변수
	[Header("=====> GaugeAbyssUIsHandler - UIs <=====")]
	[SerializeField] private Image m_oGaugeImg = null;
	[SerializeField] private Image m_oGaugeBossIconImg = null;
	#endregion // 변수

	#region 함수
	/** 초기화 */
	public override void Awake()
	{
		base.Awake();

		this.SetGaugeVal(0.0f);
		this.SetMidGaugeVal(0.0f);
	}

	/** UI 상태를 갱신한다 */
	public void UpdateUIsState()
	{
		// 플레이 시간 제한이 없을 경우
		if (this.BattleController.MaxPlayTime.ExIsLessEquals(0.0f))
		{
			return;
		}

		var oGaugeImgTrans = m_oGaugeImg.transform as RectTransform;
		var oGaugeSliderTrans = this.GaugeSlider.transform as RectTransform;

		float fMaxPlayTime = this.BattleController.MaxPlayTime;
		float fRemainPlayTime = this.BattleController.RemainPlayTime;

		float fSkipTime = Mathf.Max(0.0f, fMaxPlayTime - fRemainPlayTime);
		float fPercent = Mathf.Clamp01(fSkipTime / fMaxPlayTime);

		oGaugeImgTrans.anchoredPosition = new Vector2(oGaugeSliderTrans.rect.width * fPercent,
			oGaugeImgTrans.anchoredPosition.y);
	}
	#endregion // 함수
}
