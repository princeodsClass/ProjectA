using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 유닛 접촉 상태 */
public class CStateUnitContact : CState<UnitController>
{
	#region 프로퍼티
	public new UnitController Owner => this.GetOwner<UnitController>();
	#endregion // 프로퍼티
}

/** 유닛 공중 접촉 상태 */
public class CStateUnitContactAir : CStateUnitContact
{
	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		var stVelocity = new Vector3(this.Owner.Velocity.x, 0.0f, this.Owner.Velocity.z);

		// 최소 속도보다 느릴 경우
		if (stVelocity.magnitude.ExIsLess(2.5f))
		{
			stVelocity = stVelocity.normalized * 2.5f;
		}

		this.Owner.SetVelocity(stVelocity);
	}

	/** 상태가 종료 되었을 경우 */
	public override void OnStateExit()
	{
		base.OnStateExit();
		this.Owner.BattleController.PlayLandSound(this.Owner.gameObject);
	}
	#endregion // 함수
}

/** 유닛 바닥 접촉 상태 */
public class CStateUnitContactGround : CStateUnitContact
{
	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		this.Owner.SetAcceleration(Vector3.down * this.Owner.Gravity);
	}
	#endregion // 함수
}

/** 유닛 워프 접촉 상태 */
public class CStateUnitContactWarp : CStateUnitContact
{
	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();

		this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_AIR, true);
		this.Owner.StateMachine.SetState(this.Owner.CreateIdleState());
	}

	/** 상태가 종료 되었을 경우 */
	public override void OnStateExit()
	{
		base.OnStateExit();
		this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_AIR, false);
	}
	#endregion // 함수
}
