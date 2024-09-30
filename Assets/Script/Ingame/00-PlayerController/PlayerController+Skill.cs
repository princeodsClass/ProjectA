using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 플레이어 제어자 - 스킬 */
public partial class PlayerController : UnitController
{
	#region 프로퍼티
	public SkillTable ApplySkillTable { get; private set; } = null;
	#endregion // 프로퍼티

	#region 함수
	/** 지속 시간 스킬 여부를 검사한다 */
	public bool IsTimeSkill(SkillTable a_oSkillTable)
	{
		bool bIsTimeSkill = a_oSkillTable.SkillType == (int)ESkillType.RICOCHET;
		bIsTimeSkill = bIsTimeSkill || a_oSkillTable.SkillType == (int)ESkillType.UNTOUCHABLE;

		return bIsTimeSkill;
	}

	/** 액션 스킬 여부를 검사한다 */
	public bool IsActionSkill(SkillTable a_oSkillTable)
	{
		return a_oSkillTable.SkillType == (int)ESkillType.JUMP_ATTACK;
	}

	/** 타겟팅 스킬 여부를 검사한다 */
	public bool IsTargetingSkill(SkillTable a_oSkillTable)
	{
		return a_oSkillTable.SkillType == (int)ESkillType.JUMP_ATTACK;
	}

	/** 즉시 발동 스킬 여부를 검사한다 */
	public bool IsImmediateSkill(SkillTable a_oSkillTable)
	{
		return a_oSkillTable.SkillType != (int)ESkillType.JUMP_ATTACK;
	}

	/** 스킬을 적용한다 */
	public void ApplySkill(SkillTable a_oSkillTable)
	{
		this.ApplySkill(a_oSkillTable, false);

		// 지속 시간 스킬이 아닐 경우
		if (!this.IsTimeSkill(a_oSkillTable))
		{
			this.CurActiveSkillPoint = 0;
		}

		// 점프 공격 스킬 일 경우
		if (a_oSkillTable.SkillType == (int)ESkillType.JUMP_ATTACK)
		{
			var oCamDummy = this.BattleController.CamDummy.GetComponent<CamDummy>();
			oCamDummy.SetIsEnableUpdate(false);
		}
	}

	/** 스킬을 적용한다 */
	public override void ApplySkill(SkillTable a_oSkillTable, bool a_bIsContinuing)
	{
		var oBattleState = this.StateMachine.State as CStatePlayerBattleSkill;

		// 스킬 발동이 불가능 할 경우
		if (this.IsAir || oBattleState == null)
		{
			return;
		}

		// 즉시 발동 스킬 일 경우
		if (this.IsImmediateSkill(a_oSkillTable))
		{
			this.ApplySkill(a_oSkillTable, this.transform.position, null, a_bIsContinuing);
		}
		// 타겟팅 스킬 일 경우
		else if (this.IsTargetingSkill(a_oSkillTable))
		{
			this.ApplySkill(a_oSkillTable, oBattleState.TargetPos, null, a_bIsContinuing);
		}

		this.ApplySkillTable = a_oSkillTable;
		this.SetupApplySkillInfo(a_oSkillTable, a_bIsContinuing);
	}

	/** 스킬을 발동시킨다 */
	public void ActivateSkill(SkillTable a_oSkillTable)
	{
		this.IsUseSkill = true;
		this.ApplySkillTable = a_oSkillTable;

		var oEffectTableList = EffectTable.GetGroup(a_oSkillTable.HitEffectGroup);
		
		switch ((ESkillType)a_oSkillTable.SkillType)
		{
			case ESkillType.RICOCHET:
			case ESkillType.UNTOUCHABLE:
				this.RemainSkillUseTime = this.MaxRemainSkillUseTime = oEffectTableList[0].Duration * ComType.G_UNIT_MS_TO_S;
				break;

			case ESkillType.JUMP_ATTACK:
				this.RemainSkillUseTime = this.MaxRemainSkillUseTime = ComType.G_DELTA_T_SKILL;
				break;

			default:
				this.RemainSkillUseTime = this.MaxRemainSkillUseTime = 0.0f;
				break;
		}

		this.StateMachine.SetState(this.CreateBattleSkillState());
	}
	#endregion // 함수

	#region 접근 함수
	/** 스킬 사용 여부를 변경한다 */
	public void SetIsUseSkill(bool a_bIsUse)
	{
		this.IsUseSkill = a_bIsUse;
	}
	#endregion // 접근 함수
}
