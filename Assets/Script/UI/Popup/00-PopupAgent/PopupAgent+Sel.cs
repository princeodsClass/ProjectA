using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 에이전트 팝업 - 선택 */
public partial class PopupAgent : UIDialog
{
	#region 상수
	private const int MAX_NUM_SCROLLER_CELL_ON_ROW = 2;
	#endregion // 상수

	#region 변수
	[Header("=====> Popup Agent - UIs (Sel) <=====")]
	[SerializeField] private TMP_Text m_oNumTextAgentUnlock = null;
	[SerializeField] private Image m_oIconImgAgentUnlock = null;
	
	[SerializeField] private List<PopupAgentScrollerCellViewSel> m_oPopupAgentScrollerCellViewSelList = new List<PopupAgentScrollerCellViewSel>();

	[Header("=====> Popup Agent - Game Objects (Sel) <=====")]
	[SerializeField] private GameObject m_oAgentSelUIs = null;
	[SerializeField] private GameObject m_oAgentSelMenuUIs = null;

	[SerializeField] private GameObject m_oAgentSelMenuUIsOpen = null;
	[SerializeField] private GameObject m_oAgentSelMenuUIsClose = null;

	[SerializeField] private GameObject m_oAgentSelMenuUIsOpenSel = null;
	[SerializeField] private GameObject m_oAgentSelMenuUIsOpenUnsel = null;

	[SerializeField] private GameObject m_oSelScrollViewContents = null;
	#endregion // 변수

	#region 함수
	/** 선택 버튼을 눌렀을 경우 */
	public void OnTouchSelBtn()
	{
		m_oCurCharacterTable = m_oSelCharacterTable;
		StartCoroutine(this.CoSelAgent(m_oSelCharacterTable));
	}

	/** 의상 버튼을 눌렀을 경우 */
	public void OnTouchCostumeBtn()
	{
		// XXX: 의상 기능 구현 시 활성 {
#if DISABLE_THIS
		this.SetUIsMode(EUIsMode.COSTUME);
		this.UpdateUIsState();
#endif // #if DISABLE_THIS
		// XXX: 의상 기능 구현 시 활성 }
	}

	/** 강화 버튼을 눌렀을 경우 */
	public void OnTouchEnhanceBtn()
	{
		this.SetUIsMode(EUIsMode.ENHANCE);
		this.UpdateUIsState();
	}

    public void OnTouchEnhanceBtn(bool isTutorial)
    {
        this.SetUIsMode(EUIsMode.ENHANCE);
        this.UpdateUIsState();

        GameObject AgentEnhanceBtn = GameObject.Find("AgentEnhanceBtn");
        RectTransform rt = AgentEnhanceBtn.GetComponent<RectTransform>();
		GameManager.Singleton.tutorial.SetMessage("ui_tip_character_upgrade_01", -200);
        GameManager.Singleton.tutorial.SetFinger(AgentEnhanceBtn,
                                                 FindObjectOfType<PopupAgent>().OnTouchAgentEnhanceBtn,
                                                 rt.rect.width, rt.rect.height, 750);
    }

    /** 잠금 해제 버튼을 눌렀을 경우 */
    public void OnTouchOpenBtn()
	{
		// 에이전트 잠금 해제가 가능 할 경우
		if (ComUtil.IsEnableOpenAgent(m_oSelCharacterTable))
		{
			StartCoroutine(this.CoOpenAgent(m_oSelCharacterTable));
		}
		else
		{
			var mp = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
			mp.InitializeInfo("ui_error_title", "ui_error_resource_lick", "ui_popup_button_confirm");
		}
	}

	/** 획득 버튼을 눌렀을 경우 */
	public void OnTouchAcquireBtn()
	{
		var oPopupAgentBuy = MenuManager.Singleton.OpenPopup<PopupAgentBuy>(EUIPopup.PopupAgentBuy, true);
		oPopupAgentBuy.Init(PopupAgentBuy.MakeParams(this, m_oSelCharacterTable, this.OnReceivePopupAgentBuyCallback));
	}

	/** 선택 UI 를 설정한다 */
	private void SetupUIsSel()
	{
		var oCharacterTableList = CharacterTable.GetList();

		for (int i = 0; i < m_oPopupAgentScrollerCellViewSelList.Count; ++i)
		{
			var stParams = PopupAgentScrollerCellViewSel.MakeParams(i * MAX_NUM_SCROLLER_CELL_ON_ROW,
				this, oCharacterTableList, this.OnReceiveAgentSelCallbackSel);

			m_oPopupAgentScrollerCellViewSelList[i].Init(stParams);
		}
	}

	/** 선택 UI 상태를 갱신한다 */
	private void UpdateUIsStateSel()
	{
		bool bIsSel = this.IsSelAgent(m_oSelCharacterTable);
		bool bIsOpen = this.IsOpenAgent(m_oSelCharacterTable);

		m_oAgentSelMenuUIsOpen.SetActive(bIsOpen);
		m_oAgentSelMenuUIsClose.SetActive(!bIsOpen);

		m_oAgentSelMenuUIsOpenSel.SetActive(bIsSel);
		m_oAgentSelMenuUIsOpenUnsel.SetActive(!bIsSel);

		for (int i = 0; i < m_oPopupAgentScrollerCellViewSelList.Count; ++i)
		{
			m_oPopupAgentScrollerCellViewSelList[i].UpdateUIsState();
		}

		// 잠금 해제 상태 일 경우
		if(this.IsOpenAgent(m_oSelCharacterTable))
		{
			return;
		}

		int nNumItems = m_oSelCharacterTable.RequireItemCount;
		int nCurNumItems = GameManager.Singleton.invenMaterial.GetItemCount(m_oSelCharacterTable.RequireItemKey);

		m_oNumTextAgentUnlock.text = $"{nCurNumItems}/<color=#ffffff>{nNumItems}</color>";
		m_oIconImgAgentUnlock.sprite = ComUtil.GetIcon(m_oSelCharacterTable.RequireItemKey);

		ComUtil.RebuildLayouts(m_oAgentSelMenuUIsOpen);
		ComUtil.RebuildLayouts(m_oAgentSelMenuUIsClose);
	}

	/** 에이전트 선택 콜백을 수신했을 경우 */
	private void OnReceiveAgentSelCallbackSel(PopupAgentScrollerCellView a_oSender, int a_nIdx)
	{
		var oScrollerCellViewSel = a_oSender as PopupAgentScrollerCellViewSel;

		// 동일한 에이전트 일 경우
		if(m_oSelCharacterTable == oScrollerCellViewSel.Params.m_oCharacterTableList[a_nIdx])
		{
			return;
		}

		m_oVideoPlayer.clip = m_oVideoClipList[a_nIdx];
		m_oSelCharacterTable = oScrollerCellViewSel.Params.m_oCharacterTableList[a_nIdx];

		m_oVideoPlayer.Stop();
		m_oVideoPlayer.Play();

		this.SetupAgent();
		this.UpdateUIsState();
	}

	/** 에이전트 구입 팝업 콜백을 수신했을 경우 */
	private void OnReceivePopupAgentBuyCallback(PopupAgentBuy a_oSender, CharacterTable a_oCharacterTable)
	{
		a_oSender.Close();
		this.UpdateUIsState();

		PopupSysMessage s = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
        s.InitializeInfo("ui_popup_option_alarm_caption", "ui_shop_deal_complete", "ui_popup_button_confirm");
	}
	#endregion // 함수
}

/** 에이전트 팝업 - 선택 (코루틴) */
public partial class PopupAgent : UIDialog
{
	#region 함수
	/** 에이전트를 선택한다 */
	private IEnumerator CoSelAgent(CharacterTable a_oCharacterTable)
	{
		var oWaitPopup = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);
		var oItemCharacter = ComUtil.GetItemCharacter(a_oCharacterTable);

		yield return GameManager.Singleton.EquipCharacter(oItemCharacter.id);
		GameObject.Find("InventoryPage")?.GetComponent<PageLobbyInventory>().InitializeCharacter();

		oWaitPopup.Close();
		this.SetUIsMode(EUIsMode.SEL);
		
		this.UpdateUIsState();
	}

	/** 에이전트를 잠금 해제한다 */
	private IEnumerator CoOpenAgent(CharacterTable a_oCharacterTable)
	{
		var oWaitPopup = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);
		yield return GameManager.Singleton.AddItemCS(a_oCharacterTable.PrimaryKey, 1, null);

		int nNumItems = a_oCharacterTable.RequireItemCount;
		yield return GameManager.Singleton.StartCoroutine(GameManager.Singleton.AddItemCS(a_oCharacterTable.RequireItemKey, -nNumItems));

		oWaitPopup.Close();
		this.UpdateUIsState();

		PopupSysMessage s = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
        s.InitializeInfo("ui_popup_option_alarm_caption", "ui_shop_deal_complete", "ui_popup_button_confirm");
	}
	#endregion // 함수
}
