using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 에이전트 팝업 - 의상 */
public partial class PopupAgent : UIDialog
{
	#region 변수
	[Header("=====> Popup Agent - UIs (Costume) <=====")]
	private List<PopupAgentScrollerCellViewCostume> m_oPopupAgentScrollerCellViewCostumeList = new List<PopupAgentScrollerCellViewCostume>();

	[Header("=====> Popup Agent - Game Objects (Costume) <=====")]
	[SerializeField] private GameObject m_oAgentCostumeUIs = null;
	[SerializeField] private GameObject m_oAgentCostumeMenuUIs = null;
	[SerializeField] private GameObject m_oCostumeScrollViewContents = null;
	#endregion // 변수

	#region 함수
	/** 의상 UI 를 설정한다 */
	private void SetupUIsCostume()
	{
		// Do Something
	}

	/** 의상 UI 상태를 갱신한다 */
	private void UpdateUIsStateCostume()
	{
		// Do Something
	}

	/** 에이전트 의상 선택 콜백을 수신했을 경우 */
	private void OnReceiveAgentSelCallbackCostume(PopupAgentScrollerCellView a_oSender, int a_nIdx)
	{
		var oScrollerCellViewCostume = a_oSender as PopupAgentScrollerCellViewCostume;
	}
	#endregion // 함수
}
