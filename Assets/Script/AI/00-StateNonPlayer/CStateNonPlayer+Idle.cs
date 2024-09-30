using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** NPC 대기 상태 */
public partial class CStateNonPlayerIdle : CStateUnitIdle
{
	#region 프로퍼티
	public new NonPlayerController Owner => this.GetOwner<NonPlayerController>();
	#endregion // 프로퍼티

	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		this.Owner.NavMeshAgent.isStopped = true;

		this.Owner.PreSetupNextAttackAction();
	}

	/** 상태를 갱신한다 */
	public override void OnStateCustomUpdate(float a_fDeltaTime)
	{
		base.OnStateCustomUpdate(a_fDeltaTime);

		// 행동 불능 상태 일 경우
		if(this.Owner.Animator.GetBool(ComType.G_PARAMS_IS_FREEZE))
		{
			return;
		}

		// 도망 상태 일 경우
		if (this.Owner.IsAvoidAction)
		{
			this.Owner.StateMachine.SetState(this.Owner.CreateAvoidState());
		}
		// 추적 대상이 존재 할 경우
		else if (this.Owner.TrackingTarget != null)
		{
			// 조준 가능 할 경우
			if (this.Owner.LockOnTarget != null && this.Owner.IsAimableTarget(this.Owner.LockOnTarget))
			{
				this.Owner.StateMachine.SetState(this.Owner.CreateBattleReadyState());
			}
			// 추적 대상이 생존 상태 일 경우
			else if (this.Owner.BattleController.IsPlaying && this.Owner.TrackingTarget.IsSurvive)
			{
				this.Owner.StateMachine.SetState(this.Owner.CreateMoveState());
			}
		}
	}
	#endregion // 함수
}

/** NPC 경계 대기 상태 */
public partial class CStateNonPlayerIdleLookAround : CStateNonPlayerIdle
{
	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		this.OnCompleteLookAround(null);
	}

	/** 주변 경계를 완료했을 경우 */
	private void OnCompleteLookAround(NonPlayerController a_oSender)
	{
		this.Owner.ExLateCallFunc(this.DoOnCompleteLookAround, this.Owner.ObjInfo.m_fPreDelay);
	}

	/** 주변 경계를 완료했을 경우 */
	private void DoOnCompleteLookAround(MonoBehaviour a_oSender)
	{
		// 추적 대상이 없을 경우
		if (this.Owner.TrackingTarget == null)
		{
			this.Owner.StartLookAround(this.OnCompleteLookAround);
		}
	}
	#endregion // 함수
}
