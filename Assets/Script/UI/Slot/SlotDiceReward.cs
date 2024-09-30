using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** 주사위 보상 슬롯 */
public class SlotDiceReward : SlotMissionAdventureReward
{
	#region 변수
	[Header("=====> UIs <=====")]
	[SerializeField] private Image m_oSelImg = null;

	[Header("=====> Game Objects <=====")]
	[SerializeField] private GameObject m_oCheckUIs = null;
	#endregion // 변수

	#region 함수
	/** 초기화 */
	public override void Init(STParams a_stParams)
	{
		base.Init(a_stParams);
		this.UpdateUIsState();
	}

	/** UI 상태를 갱신한다 */
	private void UpdateUIsState()
	{
		m_oCheckUIs.SetActive(false);
		m_oSelImg.gameObject.SetActive(false);
	}
	#endregion // 함수

	#region 접근 함수
	/** 선택 여부를 변경한다 */
	public void SetIsSel(bool a_bIsSel)
	{
		m_oSelImg.gameObject.SetActive(a_bIsSel);
	}

	/** 체크 여부를 변경한다 */
	public void SetIsCheck(bool a_bIsCheck)
	{
		m_oCheckUIs.SetActive(a_bIsCheck);
	}
	#endregion // 접근 함수
}
