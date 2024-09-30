using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 보너스 게이지 UI 처리자 */
public class CGaugeBonusUIsHandler : CGaugeBossUIsHandler
{
	#region 함수
	/** 초기화 */
	public override void Awake()
	{
		base.Awake();
		this.PercentList.Clear();

		for (int i = 0; i < 10; ++i)
		{
			this.PercentList.ExAddVal(i * 0.1f);
		}
	}
	#endregion // 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public new static STParams MakeParams(NPCTable a_oTable)
	{
		return new STParams()
		{
			m_oTable = a_oTable
		};
	}
	#endregion // 클래스 팩토리 함수
}
