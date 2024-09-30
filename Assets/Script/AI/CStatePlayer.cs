using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/** 플레이어 대기 상태 */
public partial class CStatePlayerIdle : CStateUnitIdle
{
	#region 함수
	/** 상태를 갱신한다 */
	public override void OnStateUpdate(float a_fDeltaTime)
	{
		base.OnStateUpdate(a_fDeltaTime);

#if MOVING_SHOOT_ENABLE
		if (this.GetOwner<PlayerController>().IsMovingShoot)
		{
			// 조준 대상이 존재 할 경우
			if (this.Owner.LockOnTarget != null)
			{
				this.Owner.StateMachine.SetState(this.Owner.CreateBattleReadyState());
			}
		}
		else
		{
			// 이동 상태 일 경우
			if (this.Owner.Animator.GetBool(ComType.G_PARAMS_IS_MOVE))
			{
				this.Owner.StateMachine.SetState(this.Owner.CreateMoveState());
			}
			// 조준 대상이 존재 할 경우
			else if (this.Owner.LockOnTarget != null)
			{
				this.Owner.StateMachine.SetState(this.Owner.CreateBattleReadyState());
			}
		}
#else
		// 이동 상태 일 경우
		if (this.Owner.Animator.GetBool(ComType.G_PARAMS_IS_MOVE))
		{
			this.Owner.StateMachine.SetState(this.Owner.CreateMoveState());
		}
		// 조준 대상이 존재 할 경우
		else if (this.Owner.LockOnTarget != null)
		{
			this.Owner.StateMachine.SetState(this.Owner.CreateBattleReadyState());
		}
#endif // #if MOVING_SHOOT_ENABLE
	}
	#endregion // 함수
}

/** 플레이어 이동 상태 */
public partial class CStatePlayerMove : CStateUnitMove
{
	#region 함수
	/** 상태를 갱신한다 */
	public override void OnStateUpdate(float a_fDeltaTime)
	{
		base.OnStateUpdate(a_fDeltaTime);

#if MOVING_SHOOT_ENABLE
		if (this.GetOwner<PlayerController>().IsMovingShoot)
		{
			// 이동 상태가 아닐 경우
			if (!this.Owner.Animator.GetBool(ComType.G_PARAMS_IS_MOVE))
			{
				this.Owner.StateMachine.SetState(this.Owner.CreateIdleState());
			}
			// 조준 대상이 존재 할 경우
			else if (this.Owner.LockOnTarget != null && this.Owner.Animator.GetFloat("Speed").ExIsLessEquals(this.GetOwner<PlayerController>().MovingShootSpeedRatio))
			{
				this.Owner.StateMachine.SetState(this.Owner.CreateBattleReadyState());
			}
		}
		else
		{
			// 이동 상태가 아닐 경우
			if (!this.Owner.Animator.GetBool(ComType.G_PARAMS_IS_MOVE))
			{
				this.Owner.StateMachine.SetState(this.Owner.CreateIdleState());
			}
		}
#else
		// 이동 상태가 아닐 경우
		if (!this.Owner.Animator.GetBool(ComType.G_PARAMS_IS_MOVE))
		{
			this.Owner.StateMachine.SetState(this.Owner.CreateIdleState());
		}
#endif // #if MOVING_SHOOT_ENABLE
	}
	#endregion // 함수
}
