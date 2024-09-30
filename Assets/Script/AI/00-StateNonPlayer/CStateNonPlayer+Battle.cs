using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** NPC 전투 준비 상태 */
public partial class CStateNonPlayerBattleReady : CStateUnitBattleReady
{
	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		this.GetOwner<NonPlayerController>().StopLookAround();
	}
	#endregion // 함수

	#region 접근 함수
	/** 다음 상태를 반환한다 */
	public override CStateUnitBattle GetNextState()
	{
		return (this.GetOwner<NonPlayerController>().NextSelectionSkillGroupTable != null) ?
			this.Owner.CreateBattleSkillState() : base.GetNextState();
	}
	#endregion // 접근 함수
}

/** NPC 전투 조준 상태 */
public partial class CStateNonPlayerBattleLockOn : CStateUnitBattleLockOn
{
	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		this.Owner.SetMagazineInfo(new STMagazineInfo() { m_nNumBullets = 1 });

		this.Owner.Animator.GetBehaviour<CStateMachineBehaviourFire>().SetExitCallback(this.HandleOnStateExit);
	}

	/** 상태를 갱신한다 */
	public override void OnStateUpdate(float a_fDeltaTime)
	{
		base.OnStateUpdate(a_fDeltaTime);
		this.SetUpdateSkipTime(this.Owner.IsFire ? 0.0f : this.UpdateSkipTime + a_fDeltaTime);

		// 발사 지연 시간이 지났을 경우
		if (this.Owner.IsEnableAttack() && this.UpdateSkipTime.ExIsGreatEquals(this.Owner.AimingDelay * ComType.G_UNIT_MS_TO_S))
		{
			this.SetUpdateSkipTime(0.0f);
			this.Owner.AttackLockOnTarget();
		}
	}

	/** 상태 종료를 처리한다 */
	private void HandleOnStateExit(Animator a_oSender, AnimatorStateInfo a_stStateInfo, int a_nLayerIdx)
	{
		// 행동 불능 상태 일 경우
		if (this.Owner.Animator.GetBool(ComType.G_PARAMS_IS_FREEZE))
		{
			return;
		}

		this.Owner.StateMachine.SetState(this.Owner.CreateBattleReloadState());
	}
	#endregion // 함수
}

/** NPC 전투 재장전 상태 */
public partial class CStateNonPlayerBattleReload : CStateUnitBattleReload
{
	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		this.Owner.Animator.SetTrigger(ComType.G_PARAMS_RELOAD);

		this.Owner.Animator.GetBehaviour<CStateMachineBehaviourReload>().SetExitCallback(this.HandleOnStateExit);
	}

	/** 상태가 종료 되었을 경우 */
	public override void OnStateExit()
	{
		base.OnStateExit();
		this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_BATTLE, this.Owner.TrackingTarget != null);
	}

	/** 상태 종료를 처리한다 */
	private void HandleOnStateExit(Animator a_oSender, AnimatorStateInfo a_stStateInfo, int a_nLayerIdx)
	{
		var oOwner = this.GetOwner<NonPlayerController>();
		oOwner.SetIsAvoidAction((EActionType)oOwner.Table.ActionType >= EActionType.AVOID);

		var stMagazineInfo = this.Owner.MagazineInfo;
		stMagazineInfo.m_nNumBullets = 1;

		this.Owner.SetMagazineInfo(stMagazineInfo);
		this.Owner.StateMachine.SetState(this.Owner.CreateIdleState());
	}
	#endregion // 함수
}

/** NPC 전투 스킬 상태 */
public partial class CStateNonPlayerBattleSkill : CStateUnitBattleSkill
{
	#region 변수
	private bool m_bIsEnableUseSkill = true;
	#endregion // 변수

	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		m_bIsEnableUseSkill = true;
	}

	/** 상태를 갱신한다 */
	public override void OnStateUpdate(float a_fDeltaTime)
	{
		base.OnStateUpdate(a_fDeltaTime);
		this.SetUpdateSkipTime(m_bIsEnableUseSkill ? this.UpdateSkipTime + a_fDeltaTime : 0.0f);

		// 발사 지연 시간이 지났을 경우
		if (m_bIsEnableUseSkill && this.UpdateSkipTime.ExIsGreatEquals(this.Owner.AimingDelay * ComType.G_UNIT_MS_TO_S))
		{
			var oOwner = this.GetOwner<NonPlayerController>();
			oOwner.ApplySkillGroup(oOwner.StatTable.AttackSkillGroup, false);

			m_bIsEnableUseSkill = false;
		}
	}

	/** 스킬 발사 상태 종료를 처리한다 */
	protected override void HandleOnStateExitSkillFire(Animator a_oSender, AnimatorStateInfo a_stStateInfo, int a_nLayerIdx)
	{
		base.HandleOnStateExitSkillFire(a_oSender, a_stStateInfo, a_nLayerIdx);

		// 행동 불능 상태 일 경우
		if (this.Owner.Animator.GetBool(ComType.G_PARAMS_IS_FREEZE))
		{
			return;
		}

		// 지속 스킬 일 경우
		if (this.Owner.Animator.GetBool(ComType.G_PARAMS_IS_CONTINUING_SKILL))
		{
			return;
		}

		this.Owner.StateMachine.SetState(this.Owner.CreateBattleReloadState());
	}

	/** 스킬 마무리 상태 종료를 처리한다 */
	protected override void HandleOnStateExitSkillFinish(Animator a_oSender, AnimatorStateInfo a_stStateInfo, int a_nLayerIdx)
	{
		base.HandleOnStateExitSkillFinish(a_oSender, a_stStateInfo, a_nLayerIdx);
		this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_CONTINUING_SKILL, false);

		// 행동 불능 상태 일 경우
		if (this.Owner.Animator.GetBool(ComType.G_PARAMS_IS_FREEZE))
		{
			return;
		}

		this.Owner.StateMachine.SetState(this.Owner.CreateBattleReloadState());
	}
	#endregion // 함수
}
