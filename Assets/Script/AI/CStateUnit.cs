using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 유닛 상태 */
public abstract partial class CStateUnit : CState<UnitController>
{
	#region 프로퍼티
	public virtual bool IsForceUpdateLockOnTarget => false;
	public new UnitController Owner => this.GetOwner<UnitController>();
	#endregion // 프로퍼티

	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();

		// 조준선을 리셋한다
		this.Owner.ResetSightLineHandler();

		// 트리거를 리셋한다 {
		this.Owner.Animator.ResetTrigger(ComType.G_PARAMS_SHOT);
		this.Owner.Animator.ResetTrigger(ComType.G_PARAMS_RELOAD);

		this.Owner.Animator.ResetTrigger(ComType.G_PARAMS_LOCK_ON);
		this.Owner.Animator.ResetTrigger(ComType.G_PARAMS_LOCK_ON_IMMEDIATE);

		this.Owner.Animator.ResetTrigger(ComType.G_PARAMS_SKILL_CAST);
		this.Owner.Animator.ResetTrigger(ComType.G_PARAMS_SKILL_FIRE);
		this.Owner.Animator.ResetTrigger(ComType.G_PARAMS_SKILL_FINISH);
		// 트리거를 리셋한다 }
	}

	/** 상태를 갱신한다 */
	public override void OnStateUpdate(float a_fDeltaTime)
	{
		base.OnStateUpdate(a_fDeltaTime);
		this.Owner.UpdateMoveState();
	}

	/** 상태를 갱신한다 */
	public override void OnStateFixedUpdate(float a_fDeltaTime)
	{
		base.OnStateFixedUpdate(a_fDeltaTime);
		this.Owner.UpdatePhysicsState();
	}

	/** 상태를 갱신한다 */
	public override void OnStateCustomUpdate(float a_fDeltaTime)
	{
		base.OnStateCustomUpdate(a_fDeltaTime);
		this.Owner.UpdateLockOnTarget(this.IsForceUpdateLockOnTarget);
	}
	#endregion // 함수
}

/** 유닛 등장 상태 */
public abstract partial class CStateUnitAppear : CStateUnit
{
	// Do Something
}

/** 유닛 대기 상태 */
public abstract partial class CStateUnitIdle : CStateUnit
{
	// Do Something
}

/** 유닛 이동 상태 */
public abstract partial class CStateUnitMove : CStateUnit
{
	#region 프로퍼티
	public override bool IsForceUpdateLockOnTarget => true;
	#endregion // 프로퍼티
}
