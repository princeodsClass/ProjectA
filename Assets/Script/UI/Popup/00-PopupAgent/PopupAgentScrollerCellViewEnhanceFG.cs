using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 강화 에이전트 팝업 상단 스크롤러 셀 뷰 */
public class PopupAgentScrollerCellViewEnhanceFG : PopupAgentScrollerCellView
{
	#region 변수
	[Header("=====> Popup Agent Scroller Cell View Enhance FG - Etc <=====")]
	[SerializeField] private Animator m_oEnhanceAnimator = null;

	[Header("=====> Popup Agent Scroller Cell View Enhance FG - Game Objects <=====")]
	[SerializeField] private GameObject m_oBottomUIs = null;
	[SerializeField] private List<PopupAgentScrollerCellEnhanceFG> m_oScrollerCellEnhanceFGList = new List<PopupAgentScrollerCellEnhanceFG>();
	#endregion // 변수

	#region 함수
	/** UI 상태를 갱신한다 */
	public void UpdateUIsState()
	{
		int nIdx = this.Params.m_nIdx;
		int nOrderVal = ComUtil.GetAgentEnhanceOrderVal(this.Params.m_oPopupAgent.SelCharacterTable, this.Params.m_nIdx);

		bool bIsOpen = ComUtil.IsOpenAgentEnhance(this.Params.m_oPopupAgent.SelCharacterTable, nIdx);
		bool bIsEnableEnhanceSkill = this.Params.m_oPopupAgent.IsEnableEnhanceSkill(this.Params.m_oPopupAgent.SelCharacterTable, this.Params.m_nIdx);
		bool bIsEnhanceSkill = this.Params.m_oPopupAgent.IsEnhanceSkill(this.Params.m_oPopupAgent.SelCharacterTable, this.Params.m_nIdx);

		var oEnhanceKeyList = ComUtil.GetAgentEnhanceKeys(this.Params.m_oPopupAgent.SelCharacterTable, nIdx);
		m_oBottomUIs.SetActive(bIsOpen && bIsEnableEnhanceSkill && bIsEnhanceSkill);

		m_oEnhanceAnimator.ResetTrigger(ComType.G_PARAMS_RESTART);
		m_oEnhanceAnimator.SetTrigger(ComType.G_PARAMS_RESTART);

		for (int i = 0; i < m_oScrollerCellEnhanceFGList.Count; ++i)
		{
			var oSkillTable = ComUtil.GetAgentEnhanceSkillTable(this.Params.m_oPopupAgent.SelCharacterTable, nIdx, i, nOrderVal);
			bool bIsActiveEnhance = this.Params.m_oPopupAgent.IsActiveAgentEnhance(this.Params.m_oPopupAgent.SelCharacterTable, nIdx, i);

			m_oScrollerCellEnhanceFGList[i].gameObject.SetActive(oEnhanceKeyList[i] != 0);
			m_oScrollerCellEnhanceFGList[i].CheckImg.gameObject.SetActive(bIsOpen && bIsActiveEnhance);

			m_oScrollerCellEnhanceFGList[i].LockUIs.SetActive(!bIsOpen);
			m_oScrollerCellEnhanceFGList[i].DescUIs.SetActive(oSkillTable != null && oSkillTable.UseType != (int)ESkillUseType.PASSIVE);

			// 스킬 테이블이 없을 경우
			if(oSkillTable == null)
			{
				continue;
			}

			var oEffectTableList = EffectTable.GetGroup(oSkillTable.HitEffectGroup);

			m_oScrollerCellEnhanceFGList[i].NameText.text = ComUtil.GetAgentEnhanceName(oSkillTable);
			m_oScrollerCellEnhanceFGList[i].IconImg.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Common, oEffectTableList[0].Icon);
		}
	}
	#endregion // 함수
}
