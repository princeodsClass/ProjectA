using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** 무한 게이지 UI 처리자 */
public class CGaugeInfiniteUIsHandler : CGaugeUIsHandler
{
	#region 변수
	[Header("=====> Gauge Infinite UIs Handler - UIs <=====")]
	[SerializeField] private Image m_oGaugeImg = null;
	#endregion // 변수

	#region 함수
	/** 초기화 */
	public override void Awake()
	{
		base.Awake();
		this.SetGaugeVal(0.0f);

		this.SetMidGaugeVal(0.0f);
	}
	#endregion // 함수
}
