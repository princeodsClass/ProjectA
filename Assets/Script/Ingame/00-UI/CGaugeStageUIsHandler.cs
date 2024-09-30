using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 스테이지 게이지 UI 처리자 */
public class CGaugeStageUIsHandler : CGaugeUIsHandler
{
	#region 변수
	[Header("=====> Gauge Stage UIs Handler - UIs <=====")]
	[SerializeField] private TMP_Text m_oLevelText = null;
	[SerializeField] private Image m_oGaugeImg = null;
	#endregion // 변수

	#region 프로퍼티
	public TMP_Text LevelText => m_oLevelText;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public override void Awake()
	{
		base.Awake();
		this.SetGaugeVal(0.0f);

		this.SetMidGaugeVal(0.0f);
	}
	#endregion // 함수

	#region 접근 함수
	/** 게이지 퍼센트를 변경한다 */
	public void SetGaugePercent(float a_fPercent, bool a_bIsImmediate = false)
	{
		this.ResetAni();

		// 즉시 모드 일 경우
		if (a_bIsImmediate)
		{
			this.SetGaugeVal(a_fPercent);
			this.SetMidGaugeVal(a_fPercent);
		}
		else
		{
			this.IncrPercent(a_fPercent);
		}
	}
	#endregion // 접근 함수
}
