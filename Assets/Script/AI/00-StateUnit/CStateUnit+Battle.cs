using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 유닛 전투 상태 */
public abstract partial class CStateUnitBattle : CStateUnit
{
	#region 프로퍼티
	public float SpineWeight { get; private set; } = 0.0f;
	public float UpdateSkipTime { get; private set; } = 0.0f;
	#endregion // 프로퍼티

	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		this.SetUpdateSkipTime(0.0f);

		this.Owner.SetIsFire(false);
		this.Owner.SetIsAiming(true);
		this.Owner.SetIsDoubleShot(false);
		this.Owner.SetIsDoubleShotFire(false);

		this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_MOVE, false);
		this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_BATTLE, true);
	}

	/** 상태가 종료 되었을 경우 */
	public override void OnStateExit()
	{
		base.OnStateExit();
		this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_BATTLE, false);
	}
	#endregion // 함수

	#region 접근 함수
	/** 스파인 가중치를 변경한다 */
	public void SetSpineWeight(float a_fWeight)
	{
		this.SpineWeight = a_fWeight;
	}

	/** 갱신 누적 시간을 변경한다 */
	public void SetUpdateSkipTime(float a_fSkipTime)
	{
		this.UpdateSkipTime = a_fSkipTime;
	}
	#endregion // 접근 함수
}

/** 유닛 전투 준비 상태 */
public abstract partial class CStateUnitBattleReady : CStateUnitBattle
{
	#region 변수
	private bool m_bIsPlayer = false;
	#endregion // 변수

	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		m_bIsPlayer = this is CStatePlayerBattleReady;

		this.Owner.Animator.SetTrigger(this.Owner.AimingDelay.ExIsLessEquals(0.0f) ? 
			ComType.G_PARAMS_LOCK_ON_IMMEDIATE : ComType.G_PARAMS_LOCK_ON);
	}

	/** 상태를 갱신한다 */
	public override void OnStateLateUpdate(float a_fDeltaTime)
	{
		base.OnStateLateUpdate(a_fDeltaTime);
		var oLockOnTarget = this.Owner.LockOnTarget ?? this.Owner.TrackingTarget;

		// 조준 대상이 없을 경우
		if (oLockOnTarget == null)
		{
			return;
		}

		var stDirection = oLockOnTarget.transform.position - this.Owner.transform.position;
		stDirection.y = 0.0f;

		float fRotateRate = m_bIsPlayer ? 50.0f : 5.0f;
		float fRotateEpsilon = m_bIsPlayer ? ComType.G_UNIT_PLAYER_ROTATE_EPSILON : ComType.G_UNIT_NON_PLAYER_ROTATE_EPSILON;

		this.Owner.LookTarget(oLockOnTarget, true, a_fDeltaTime * fRotateRate);

		// 조준 되었을 경우
		if (Vector3.Dot(this.Owner.ForwardTarget.transform.forward, stDirection.normalized).ExIsGreatEquals(fRotateEpsilon))
		{
			this.Owner.StateMachine.SetState(this.GetNextState());
		}
	}
	#endregion // 함수

	#region 접근 함수
	/** 다음 상태를 반환한다 */
	public virtual CStateUnitBattle GetNextState()
	{
		return this.Owner.CreateBattleLockOnState();
	}
	#endregion // 접근 함수
}

/** 유닛 전투 조준 상태 */
public abstract partial class CStateUnitBattleLockOn : CStateUnitBattle
{
	#region 변수
	private bool m_bIsPlayer = false;
	#endregion // 변수

	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		m_bIsPlayer = this is CStatePlayerBattleLockOn;

		this.Owner.CurSightLineHandler?.gameObject.SetActive(this.Owner.CurWeaponModelInfo != null && 
			this.Owner.CurWeaponModelInfo.GetSightLine() != null);

		this.Owner.Animator.SetTrigger(this.Owner.AimingDelay.ExIsLessEquals(0.0f) ? 
			ComType.G_PARAMS_LOCK_ON_IMMEDIATE : ComType.G_PARAMS_LOCK_ON);
	}

	/** 상태를 갱신한다 */
	public override void OnStateLateUpdate(float a_fDeltaTime)
	{
		base.OnStateLateUpdate(a_fDeltaTime);
		this.SetSpineWeight(Mathf.Min(1.0f, this.SpineWeight + (a_fDeltaTime * 3.0f)));

		float fRotateRate = m_bIsPlayer ? 50.0f : 15.0f;

		this.Owner.LookTarget(this.Owner.LockOnTarget ?? this.Owner.TrackingTarget, true, a_fDeltaTime * fRotateRate);
		this.Owner.RotateSpine(this.Owner.LockOnTarget ?? this.Owner.TrackingTarget, this.SpineWeight);

		// 조준선 제어자가 존재 할 경우
		if (this.Owner.CurSightLineHandler != null && this.UpdateSkipTime.ExIsGreat(0.0f))
		{
			float fTime = GlobalTable.GetData<int>(ComType.G_TIME_COLOR_CHANGE_TO_FIRE) * ComType.G_UNIT_MS_TO_S;
			float fAimingTime = this.Owner.AimingDelay * ComType.G_UNIT_MS_TO_S;
			float fWarningTime = fAimingTime - fTime;

			var eState = this.UpdateSkipTime.ExIsLess(fWarningTime) ? ELockOnState.NORM : ELockOnState.WARNING;

			this.Owner.CurSightLineHandler.SetColor(this.Owner.PageBattle.SightLineColorWrapper.GetColor((int)eState));
			this.Owner.CurSightLineHandler.SetWidthRate(1.0f - Mathf.Min(0.5f, this.UpdateSkipTime / fAimingTime));
		}
	}
	#endregion // 함수
}

/** 유닛 전투 재장전 상태 */
public abstract partial class CStateUnitBattleReload : CStateUnitBattle
{
	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		this.SetSpineWeight(1.0f);
	}

	/** 상태를 갱신한다 */
	public override void OnStateLateUpdate(float a_fDeltaTime)
	{
		base.OnStateLateUpdate(a_fDeltaTime);
		this.SetSpineWeight(Mathf.Max(0.0f, this.SpineWeight - (a_fDeltaTime * 3.0f)));
		
		this.Owner.RotateSpine(this.Owner.LockOnTarget ?? this.Owner.TrackingTarget, this.SpineWeight);
	}
	#endregion // 함수
}

/** 유닛 전투 스킬 상태 */
public abstract partial class CStateUnitBattleSkill : CStateUnitBattle
{
#if DISABLE_THIS
	/** 상태를 갱신한다 */
	public override void OnStateLateUpdate(float a_fDeltaTime)
	{
		base.OnStateLateUpdate(a_fDeltaTime);
		this.Owner.LookTarget(this.Owner.LockOnTarget ?? this.Owner.TrackingTarget, true, a_fDeltaTime * 50.0f);
	}

	/** 상태를 갱신한다 */
	public override void OnStateLateUpdate(float a_fDeltaTime)
	{
		base.OnStateLateUpdate(a_fDeltaTime);
		this.Owner.RotateSpine(this.Owner.LockOnTarget ?? this.Owner.TrackingTarget, 1.0f);
	}
#endif // #if DISABLE_THIS

	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		this.Owner.Animator.SetTrigger(ComType.G_PARAMS_LOCK_ON);

		this.Owner.Animator.GetBehaviour<CStateMachineBehaviourSkillFire>().SetExitCallback(this.HandleOnStateExitSkillFire);
		this.Owner.Animator.GetBehaviour<CStateMachineBehaviourSkillFinish>().SetExitCallback(this.HandleOnStateExitSkillFinish);
	}
	
	/** 스킬 발사 상태 종료를 처리한다 */
	protected virtual void HandleOnStateExitSkillFire(Animator a_oSender, AnimatorStateInfo a_stStateInfo, int a_nLayerIdx)
	{
		// Do Something
	}

	/** 스킬 마무리 상태 종료를 처리한다 */
	protected virtual void HandleOnStateExitSkillFinish(Animator a_oSender, AnimatorStateInfo a_stStateInfo, int a_nLayerIdx)
	{
		// Do Something
	}
	#endregion // 함수
}
