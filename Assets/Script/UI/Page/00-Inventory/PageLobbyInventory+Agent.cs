using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 페이지 로비 인벤토리 - 에이전트 */
public partial class PageLobbyInventory : MonoBehaviour
{
	#region 함수
	/** 에이전트 버튼을 눌렀을 경우 */
	public void OnTouchAgentBtn()
	{
		var oPopupAgent = m_MenuMgr.OpenPopup<PopupAgent>(EUIPopup.PopupAgent);
		oPopupAgent.Init(PopupAgent.MakeParams(this.OnReceivePopupAgentResult));
	}

    public void OnTouchAgentBtn(bool isTutorial)
    {
        var oPopupAgent = m_MenuMgr.OpenPopup<PopupAgent>(EUIPopup.PopupAgent);
        oPopupAgent.Init(PopupAgent.MakeParams(this.OnReceivePopupAgentResult));

        GameObject EnhanceBtn = GameObject.Find("EnhanceBtn");
        RectTransform rt = EnhanceBtn.GetComponent<RectTransform>();
        GameManager.Singleton.tutorial.SetFinger(EnhanceBtn,
                                                 FindObjectOfType<PopupAgent>().OnTouchEnhanceBtn,
                                                 rt.rect.width, rt.rect.height, 750);
    }

    /** 에이전트 팝업 결과를 수신했을 경우 */
    private void OnReceivePopupAgentResult(PopupAgent a_oSender, CharacterTable a_oTable)
	{
		// Do Something
	}
	#endregion // 함수
}
