using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/** 전투 페이지 - 스킬 */
public partial class PageBattle : UIDialog
{
	/** 스킬 UI */
	[System.Serializable]
	private struct STSkillUIs
	{
		public Image m_oIconImg;
		public Image m_oUseGaugeImg;
		public Image m_oChargeGaugeImg;

		public GameObject m_oSkillUIs;
	}

	#region 변수
	[Header("=====> Page Battle (Skill) - UIs <=====")]
	[SerializeField] private STSkillUIs m_stSkillUIs;
	#endregion // 변수

	#region 함수
	// 스킬 버튼을 눌렀을 경우
	public void OnTouchSkillBtn()
	{
		bool bIsValid = !this.PlayerController.IsUseSkill;
		bIsValid = bIsValid && this.PlayerController.IsEnableActiveSkill;
		bIsValid = bIsValid && this.BattleController.StateMachine.State is not CStateBattleControllerReady;
		bIsValid = bIsValid && this.PlayerController.CurActiveSkillPoint.ExIsGreatEquals(this.PlayerController.MaxActiveSkillPoint);

		// 스킬 사용이 불가능 할 경우
		if(!bIsValid)
		{
			return;
		}

		this.PlayerController.ActivateSkill(this.PlayerController.ActiveSkillTable);
	}

	/** 스킬 UI 상태를 갱신한다 */
	private void UpdateSkillUIsState()
	{
		m_stSkillUIs.m_oSkillUIs.SetActive(this.PlayerController.IsEnableActiveSkill && 
			!this.BattleController.IsPlaySecneDirecting && this.BattleController.HintInfoQueue.Count <= 0 && this.BattleController.IsEnableUpdate);

		// 스킬 사용이 불가능 할 경우
		if(!this.PlayerController.IsEnableActiveSkill)
		{
			return;
		}

		float fPercent = this.PlayerController.CurActiveSkillPoint / this.PlayerController.MaxActiveSkillPoint;
		float fPercentUseTime = this.PlayerController.RemainSkillUseTime / this.PlayerController.MaxRemainSkillUseTime;

		fPercent = this.PlayerController.IsUseSkill ? fPercent * fPercentUseTime : fPercent;

		m_stSkillUIs.m_oUseGaugeImg.fillAmount = fPercent;
		m_stSkillUIs.m_oUseGaugeImg.gameObject.SetActive(this.PlayerController.IsUseSkill);

		m_stSkillUIs.m_oChargeGaugeImg.fillAmount = fPercent;
		m_stSkillUIs.m_oChargeGaugeImg.gameObject.SetActive(!this.PlayerController.IsUseSkill);
	}
	#endregion // 함수
}
