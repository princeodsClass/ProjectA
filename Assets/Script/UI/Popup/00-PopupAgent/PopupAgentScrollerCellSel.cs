using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 에이전트 팝업 선택 스크롤러 셀 */
public class PopupAgentScrollerCellSel : MonoBehaviour
{
	/** 매개 변수 */
	public struct STParams
	{
		public PopupAgent m_oPopupAgent;
		public CharacterTable m_oCharacterTable;
		public System.Action<PopupAgentScrollerCellSel> m_oSelCallback;
	}

	#region 변수
	[Header("=====> Popup Agent Scroller Cell Sel - Etc <=====")]
	[SerializeField] private Animator m_oEnhanceAnimator = null;

	[Header("=====> Popup Agent Scroller Cell Sel - UIs <=====")]
	[SerializeField] private TMP_Text m_oCPText = null;
	[SerializeField] private TMP_Text m_oNumText = null;
	[SerializeField] private TMP_Text m_oNameText = null;
	[SerializeField] private TMP_Text m_oLevelText = null;

	[SerializeField] private Image m_oGlowImg = null;
	[SerializeField] private Image m_oBlindImg = null;
	[SerializeField] private Image m_oNewTagImg = null;
	[SerializeField] private Image m_oAgentIconImg = null;
	[SerializeField] private Image m_oEnhanceItemImg = null;

	[SerializeField] private Slider m_oSlider = null;

	[Header("=====> Popup Agent Scroller Cell Sel - Game Objects <=====")]
	[SerializeField] private GameObject m_oSelUIs = null;
	[SerializeField] private GameObject m_oOpenUIs = null;
	[SerializeField] private GameObject m_oLockUIs = null;
	[SerializeField] private GameObject m_oEnhanceUIs = null;
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
		ComUtil.RebuildLayouts(this.gameObject);
	}

	/** UI 상태를 갱신한다 */
	public void UpdateUIsState()
	{
		this.UpdateUIsStateOpen();
		var oItemCharacter = ComUtil.GetItemCharacter(this.Params.m_oCharacterTable);

		bool bIsCurAgent = this.Params.m_oPopupAgent.IsCurAgent(this.Params.m_oCharacterTable);
		bool bIsSelAgent = this.Params.m_oPopupAgent.IsSelAgent(this.Params.m_oCharacterTable);
		bool bIsOpenAgent = this.Params.m_oPopupAgent.IsOpenAgent(this.Params.m_oCharacterTable);

		bool bIsEnableOpenAgent = ComUtil.IsEnableOpenAgent(this.Params.m_oCharacterTable);
		bool bIsEnableEnhanceAgent = ComUtil.IsEnableEnhanceAgent(this.Params.m_oCharacterTable);

		int nLevel = (oItemCharacter != null) ? oItemCharacter.nCurUpgrade : 0;

		string oNameStr = NameTable.GetValue(this.Params.m_oCharacterTable.NameKey);
		string oLockStr = UIStringTable.GetValue("ui_character_lock");
		string oLevelStr = UIStringTable.GetValue("ui_level");

		m_oEnhanceAnimator.ResetTrigger(ComType.G_PARAMS_RESTART);
		m_oEnhanceAnimator.SetTrigger(ComType.G_PARAMS_RESTART);

		m_oSelUIs.SetActive(bIsSelAgent);
		m_oEnhanceUIs.SetActive(bIsOpenAgent && bIsEnableEnhanceAgent);

#if DISABLE_THIS
		// 기존 구문
		m_oOpenUIs.SetActive(bIsOpenAgent);
		m_oLockUIs.SetActive(!bIsOpenAgent);
		m_oEnhanceItemImg.gameObject.SetActive(!bIsEnableEnhanceAgent);
#else
		m_oOpenUIs.SetActive(true);
		m_oLockUIs.SetActive(false);
		m_oEnhanceItemImg.gameObject.SetActive(true);
#endif // #if DISABLE_THIS

		m_oGlowImg.gameObject.SetActive(bIsCurAgent);
		m_oBlindImg.gameObject.SetActive(!bIsOpenAgent);
		m_oNewTagImg.gameObject.SetActive(!bIsOpenAgent && bIsEnableOpenAgent);

		m_oNameText.text = oNameStr;
		m_oLevelText.text = bIsOpenAgent ? $"<color=#ffffff>{oLevelStr}</color> {nLevel + 1}" : oLockStr;
		
		m_oAgentIconImg.sprite = ComUtil.GetIcon(this.Params.m_oCharacterTable.PrimaryKey);
	}

	/** 오픈 UI 상태를 갱신한다 */
	private void UpdateUIsStateOpen()
	{
		uint nItemKey = this.Params.m_oCharacterTable.RequireItemKey;

		int nNumItems = GameManager.Singleton.invenMaterial.GetItemCount(nItemKey);
		int nMaxNumItems = this.Params.m_oCharacterTable.RequireItemCount;

		// 잠금 해제 상태 일 경우
		if (this.Params.m_oPopupAgent.IsOpenAgent(this.Params.m_oCharacterTable))
		{
			int nMaxLevel = ComUtil.GetAgentMaxLevel(this.Params.m_oCharacterTable);

			var oItemCharacter = ComUtil.GetItemCharacter(this.Params.m_oCharacterTable);
			var oLevelTable = CharacterLevelTable.GetTable(this.Params.m_oCharacterTable.PrimaryKey, Mathf.Min(nMaxLevel, oItemCharacter.nCurUpgrade));

			nItemKey = oLevelTable.RequireItemKey00;
			nNumItems = GameManager.Singleton.invenMaterial.GetItemCount(oLevelTable.RequireItemKey00);
			nMaxNumItems = oLevelTable.RequireItemCount00;
		}
		
		m_oSlider.value = nNumItems / (float)nMaxNumItems;
		m_oNumText.text = $"{nNumItems}/{nMaxNumItems}";

		m_oEnhanceItemImg.sprite = ComUtil.GetIcon(nItemKey);
	}

	/** 선택 버튼을 눌렀을 경우 */
	public void OnTouchSelBtn()
	{
		this.Params.m_oSelCallback?.Invoke(this);
	}
	#endregion // 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(PopupAgent a_oPopupAgent,
		CharacterTable a_oCharacterTable, System.Action<PopupAgentScrollerCellSel> a_oSelCallback)
	{
		return new STParams()
		{
			m_oPopupAgent = a_oPopupAgent,
			m_oCharacterTable = a_oCharacterTable,
			m_oSelCallback = a_oSelCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}
