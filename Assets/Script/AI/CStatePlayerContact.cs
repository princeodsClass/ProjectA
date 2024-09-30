using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 플레이어 공중 접촉 상태 */
public class CStatePlayerContactAir : CStateUnitContactAir
{
	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		this.Owner.Animator.SetLayerWeight(1, 0.5f);
	}

	/** 상태를 갱신한다 */
	public override void OnStateUpdate(float a_fDeltaTime)
	{
		base.OnStateUpdate(a_fDeltaTime);
		var oPlayerController = this.GetOwner<PlayerController>();

		// 공중 상태 일 경우
		if (oPlayerController.IsAir)
		{
			var stAcceleration = new Vector3(oPlayerController.HAxis, 0.0f, oPlayerController.VAxis);
			oPlayerController.SetAcceleration(stAcceleration.normalized + (Vector3.down * this.Owner.Gravity));
		}
		else
		{
			oPlayerController.ContactStateMachine.SetState(oPlayerController.CreateContactGroundState());
		}
	}
	#endregion // 함수
}

/** 플레이어 공중 접촉 상태 */
public class CStatePlayerContactGround : CStateUnitContactGround
{
	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		this.Owner.Animator.SetLayerWeight(1, 0.0f);
	}

	/** 상태를 갱신한다 */
	public override void OnStateUpdate(float a_fDeltaTime)
	{
		base.OnStateUpdate(a_fDeltaTime);
		var oPlayerController = this.GetOwner<PlayerController>();

		// 공중 상태 일 경우
		if (oPlayerController.IsAir)
		{
			oPlayerController.ContactStateMachine.SetState(oPlayerController.CreateContactAirState());
		}
		else
		{
			var stDirection = new Vector3(oPlayerController.HAxis, 0.0f, oPlayerController.VAxis);
			var stMoveVelocity = stDirection.normalized * (oPlayerController.Speed * oPlayerController.RatioSpeed);

			oPlayerController.SetVelocity(stMoveVelocity + (Vector3.down * this.Owner.Gravity));
		}
	}
	#endregion // 함수
}
