using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;

/** 강화 에이전트 팝업 하단 스크롤러 셀 뷰 */
public class PopupAgentScrollerCellViewEnhanceBG : PopupAgentScrollerCellView
{
	/** 라인 키 */
	private enum ELineKey
	{
		NONE = -1,
		UP_LINE,
		DOWN_LINE,
		SINGLE_LINE,
		[HideInInspector] MAX_VAL
	}

	#region 변수
	[Header("=====> Popup Agent Scroller Cell View Enhance BG - UIs <=====")]
	[SerializeField] private TMP_Text m_oLevelText = null;

	[SerializeField] private Image m_oGradeImg = null;
	[SerializeField] private Image m_oSeparateImg = null;

	[SerializeField] private List<UILineRenderer> m_oPathLineListA = new List<UILineRenderer>();
	[SerializeField] private List<UILineRenderer> m_oPathLineListB = new List<UILineRenderer>();

	[Header("=====> Popup Agent Scroller Cell View Enhance BG - Game Objects <=====")]
	[SerializeField] private GameObject m_oLineUIs = null;
	#endregion // 변수

	#region 프로퍼티
	public List<UILineRenderer> PathLineListA => m_oPathLineListA;
	public List<UILineRenderer> PathLineListB => m_oPathLineListB;
	#endregion // 프로퍼티

	#region 함수
	/** UI 상태를 갱신한다 */
	public void UpdateUIsState()
	{
		int nLevelIdx = this.Params.m_nIdx + 1;

		var oEnhanceKeyList = ComUtil.GetAgentEnhanceKeys(this.Params.m_oPopupAgent.SelCharacterTable, this.Params.m_nIdx);
		var oPrevEnhanceKeyList = ComUtil.GetAgentEnhanceKeys(this.Params.m_oPopupAgent.SelCharacterTable, this.Params.m_nIdx - 1);

		int nOrderVal = ComUtil.GetAgentEnhanceOrderVal(this.Params.m_oPopupAgent.SelCharacterTable, this.Params.m_nIdx);
		int nPrevOrderVal = ComUtil.GetAgentEnhanceOrderVal(this.Params.m_oPopupAgent.SelCharacterTable, this.Params.m_nIdx - 1);

		string oLevelStr = UIStringTable.GetValue("ui_level");
		m_oLevelText.text = $"<color=#ffffff>{oLevelStr}</color> {nLevelIdx * ComType.G_OFFSET_AGENT_SKILL_LEVEL}";

		m_oLineUIs.SetActive(this.Params.m_nIdx >= 1);
		m_oSeparateImg.gameObject.SetActive(this.Params.m_nIdx >= 1);

		bool bIsValidLine = oEnhanceKeyList != null && 
			oPrevEnhanceKeyList != null && ComUtil.IsOpenAgentEnhance(this.Params.m_oPopupAgent.SelCharacterTable, this.Params.m_nIdx);

		bool bIsValidUpLineA = bIsValidLine && !this.IsSingleAgentEnhance(oEnhanceKeyList) && nOrderVal >= 1;
		bool bIsValidDownLineA = bIsValidLine && !this.IsSingleAgentEnhance(oEnhanceKeyList) && nOrderVal <= -1;
		bool bIsValidSingleLineA = bIsValidLine && this.IsSingleAgentEnhance(oEnhanceKeyList) && nOrderVal >= 1;

		bool bIsValidUpLineB = bIsValidLine && !this.IsSingleAgentEnhance(oPrevEnhanceKeyList) && nPrevOrderVal >= 1;
		bool bIsValidDownLineB = bIsValidLine && !this.IsSingleAgentEnhance(oPrevEnhanceKeyList) && nPrevOrderVal <= -1;
		bool bIsValidSingleLineB = bIsValidLine && this.IsSingleAgentEnhance(oPrevEnhanceKeyList) && nPrevOrderVal >= 1;

		m_oPathLineListA[(int)ELineKey.UP_LINE].gameObject.SetActive(bIsValidUpLineA && bIsValidSingleLineB);
		m_oPathLineListA[(int)ELineKey.DOWN_LINE].gameObject.SetActive(bIsValidDownLineA && bIsValidSingleLineB);
		m_oPathLineListA[(int)ELineKey.SINGLE_LINE].gameObject.SetActive(bIsValidSingleLineA && bIsValidSingleLineB);

		m_oPathLineListB[(int)ELineKey.UP_LINE].gameObject.SetActive(bIsValidUpLineB && bIsValidSingleLineA);
		m_oPathLineListB[(int)ELineKey.DOWN_LINE].gameObject.SetActive(bIsValidDownLineB && bIsValidSingleLineA);
		m_oPathLineListB[(int)ELineKey.SINGLE_LINE].gameObject.SetActive(bIsValidSingleLineB && bIsValidSingleLineA);
	}

	/** 싱글 에이전트 강화 여부를 검사한다 */
	private bool IsSingleAgentEnhance(List<uint> a_oEnhanceKeyList)
	{
		return a_oEnhanceKeyList != null && a_oEnhanceKeyList.Count((a_nKey) => a_nKey != 0) == 1;
	}
	#endregion // 함수
}
