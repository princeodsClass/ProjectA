using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/** 플레이어 전투 준비 상태 */
public partial class CStatePlayerBattleReady : CStateUnitBattleReady
{
	#region 함수
	/** 상태를 갱신한다 */
	public override void OnStateUpdate(float a_fDeltaTime)
	{
		base.OnStateUpdate(a_fDeltaTime);

#if MOVING_SHOOT_ENABLE
		// 무빙 슛 일 경우
		if (this.GetOwner<PlayerController>().IsMovingShoot)
		{
			// 조준 대상이 없을 경우
			if (this.Owner.LockOnTarget == null || !this.Owner.LockOnTarget.IsSurvive)
			{
				this.Owner.StateMachine.SetState(this.Owner.Animator.GetBool(ComType.G_PARAMS_IS_MOVE) ?
					this.Owner.CreateMoveState() : this.Owner.CreateIdleState());
			}
		}
		else
		{
			// 이동 상태 일 경우
			if (this.Owner.Animator.GetBool(ComType.G_PARAMS_IS_MOVE))
			{
				this.Owner.StateMachine.SetState(this.Owner.CreateMoveState());
			}
			// 조준 대상이 없을 경우
			else if (this.Owner.LockOnTarget == null || !this.Owner.LockOnTarget.IsSurvive)
			{
				this.Owner.StateMachine.SetState(this.Owner.CreateIdleState());
			}
		}
#else
		// 이동 상태 일 경우
		if (this.Owner.Animator.GetBool(ComType.G_PARAMS_IS_MOVE))
		{
			this.Owner.StateMachine.SetState(this.Owner.CreateMoveState());
		}
		// 조준 대상이 없을 경우
		else if (this.Owner.LockOnTarget == null || !this.Owner.LockOnTarget.IsSurvive)
		{
			this.Owner.StateMachine.SetState(this.Owner.CreateIdleState());
		}
#endif // #if MOVING_SHOOT_ENABLE
	}
	#endregion // 함수
}

/** 플레이어 전투 조준 상태 */
public partial class CStatePlayerBattleLockOn : CStateUnitBattleLockOn
{
	#region 함수
	/** 상태를 갱신한다 */
	public override void OnStateUpdate(float a_fDeltaTime)
	{
		base.OnStateUpdate(a_fDeltaTime);
		this.SetUpdateSkipTime(this.Owner.IsFire ? 0.0f : this.UpdateSkipTime + a_fDeltaTime);

#if MOVING_SHOOT_ENABLE
		if (this.GetOwner<PlayerController>().IsMovingShoot)
		{
			// 조준 대상이 없을 경우
			if (this.Owner.LockOnTarget == null || this.Owner.Animator.GetFloat("Speed").ExIsGreat(this.GetOwner<PlayerController>().MovingShootSpeedRatio))
			{
				this.Owner.StateMachine.SetState(this.Owner.Animator.GetBool(ComType.G_PARAMS_IS_MOVE) ?
					this.Owner.CreateMoveState() : this.Owner.CreateIdleState());
			}
			// 조준 대상이 생존 상태 일 경우
			else if (this.Owner != null && this.Owner.LockOnTarget.IsSurvive)
			{
				float fDelay = this.Owner.IsAiming ? this.Owner.AimingDelay : 0.0f;
				fDelay = (!this.Owner.IsAiming && this.Owner.IsDoubleShot) ? this.Owner.DoubleShotDelay : fDelay;

				// 발사 지연 시간이 지났을 경우
				if (this.Owner.IsEnableAttack() && this.UpdateSkipTime.ExIsGreatEquals(fDelay * ComType.G_UNIT_MS_TO_S))
				{
					this.Owner.SetIsAiming(false);
					this.Owner.AttackLockOnTarget();

					this.SetUpdateSkipTime(0.0f);
					this.TrySetupDoubleShot();
				}
			}
		}
		else
		{
			// 이동 상태 일 경우
			if (this.Owner.Animator.GetBool(ComType.G_PARAMS_IS_MOVE))
			{
				this.Owner.StateMachine.SetState(this.Owner.CreateMoveState());
			}
			else
			{
				// 조준 대상이 없을 경우
				if (this.Owner.LockOnTarget == null)
				{
					this.Owner.StateMachine.SetState(this.Owner.CreateIdleState());
				}
				// 조준 대상이 생존 상태 일 경우
				else if (this.Owner.LockOnTarget.IsSurvive)
				{
					float fDelay = this.Owner.IsAiming ? this.Owner.AimingDelay : 0.0f;
					fDelay = (!this.Owner.IsAiming && this.Owner.IsDoubleShot) ? this.Owner.DoubleShotDelay : fDelay;

					// 발사 지연 시간이 지났을 경우
					if (this.Owner.IsEnableAttack() && this.UpdateSkipTime.ExIsGreatEquals(fDelay * ComType.G_UNIT_MS_TO_S))
					{
						this.Owner.SetIsAiming(false);
						this.Owner.AttackLockOnTarget();

						this.SetUpdateSkipTime(0.0f);
						this.TrySetupDoubleShot();
					}
				}
			}
		}
#else
		// 이동 상태 일 경우
		if (this.Owner.Animator.GetBool(ComType.G_PARAMS_IS_MOVE))
		{
			this.Owner.StateMachine.SetState(this.Owner.CreateMoveState());
		}
		else
		{
			// 조준 대상이 없을 경우
			if (this.Owner.LockOnTarget == null)
			{
				this.Owner.StateMachine.SetState(this.Owner.CreateIdleState());
			}
			// 조준 대상이 생존 상태 일 경우
			else if (this.Owner.LockOnTarget.IsSurvive)
			{
				float fDelay = this.Owner.IsAiming ? this.Owner.AimingDelay : 0.0f;
				fDelay = (!this.Owner.IsAiming && this.Owner.IsDoubleShot) ? this.Owner.DoubleShotDelay : fDelay;

				// 발사 지연 시간이 지났을 경우
				if (this.Owner.IsEnableAttack() && this.UpdateSkipTime.ExIsGreatEquals(fDelay * ComType.G_UNIT_MS_TO_S))
				{
					this.Owner.SetIsAiming(false);
					this.Owner.AttackLockOnTarget();

					this.SetUpdateSkipTime(0.0f);
					this.TrySetupDoubleShot();
				}
			}
		}
#endif // #if MOVING_SHOOT_ENABLE
	}

	/** 더블 샷을 설정한다 */
	private void TrySetupDoubleShot()
	{
		// 더블 샷 일 경우
		if (this.Owner.IsDoubleShot)
		{
			this.Owner.SetIsDoubleShot(false);
			this.Owner.SetIsDoubleShotFire(true);
		}
		else
		{
			float fDoubleShotPercent = Random.Range(0.0f, 1.0f);

			// 더블 샷이 가능 할 경우
			if (fDoubleShotPercent.ExIsLess(this.Owner.DoubleShotChance))
			{
				this.Owner.SetIsDoubleShot(true);
			}
		}
	}
	#endregion // 함수
}

/** 플레이어 전투 재장전 상태 */
public partial class CStatePlayerBattleReload : CStateUnitBattleReload
{
	// Do Something
}

/** 플레이어 전투 스킬 상태 */
public partial class CStatePlayerBattleSkill : CStateUnitBattleSkill
{
	#region 변수
	private int m_nAreaMask = 0;
	private int m_nStructureLayerMask = 0;

	private bool m_bIsTouch = false;
	private NavMeshPath m_oNavMeshPath = new NavMeshPath();

	private SkillTable m_oApplySkillTable = null;
	private EffectTable m_oApplySkillEffectTable = null;
	#endregion // 변수

	#region 프로퍼티
	public bool IsApplySkill { get; private set; } = false;
	public Vector3 TargetPos { get; private set; } = Vector3.zero;

	public new PlayerController Owner => this.GetOwner<PlayerController>();
	#endregion // 프로퍼티

	#region 함수
	/** 상태가 시작되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();

		m_bIsTouch = false;
		m_nStructureLayerMask = LayerMask.GetMask(ComType.G_LAYER_STRUCTURE);

		m_nAreaMask = 1 << NavMesh.GetAreaFromName(ComType.G_NAV_MESH_AREA_WALKABLE);
		m_nAreaMask = m_nAreaMask | (1 << NavMesh.GetAreaFromName(ComType.G_NAV_MESH_AREA_NOT_WALKABLE));

		m_oApplySkillTable = this.Owner.ApplySkillTable;
		m_oApplySkillEffectTable = EffectTable.GetGroup(this.Owner.ApplySkillTable.HitEffectGroup)[0];

		// 타겟팅 스킬 일 경우
		if (this.Owner.IsTargetingSkill(m_oApplySkillTable))
		{
			ComUtil.SetTimeScale(0.01f, true);
		}
		// 액션 스킬이 아닐 경우
		else if (!this.Owner.IsActionSkill(m_oApplySkillTable))
		{
			this.ApplySkill();
			this.HandleOnStateExitSkillFinish(null, default, 0);

			this.Owner.Animator.ResetTrigger(ComType.G_PARAMS_LOCK_ON);
		}
	}

	/** 스킬을 적용한다 */
	public void TryApplySkill()
	{
		// 스킬 적용이 불가능 할 경우
		if (this.IsApplySkill)
		{
			return;
		}

		// 타겟팅 스킬 일 경우
		if (this.Owner.IsTargetingSkill(m_oApplySkillTable))
		{
			this.TryApplySkillTargeting();
		}
	}

	/** 스킬 발사 상태 종료를 처리한다 */
	protected override void HandleOnStateExitSkillFire(Animator a_oSender, AnimatorStateInfo a_stStateInfo, int a_nLayerIdx)
	{
		base.HandleOnStateExitSkillFire(a_oSender, a_stStateInfo, a_nLayerIdx);

		// 지속 스킬 일 경우
		if (this.Owner.Animator.GetBool(ComType.G_PARAMS_IS_CONTINUING_SKILL))
		{
			return;
		}

		this.HandleOnStateExitSkillFinish(a_oSender, a_stStateInfo, a_nLayerIdx);
	}

	/** 스킬 마무리 상태 종료를 처리한다 */
	protected override void HandleOnStateExitSkillFinish(Animator a_oSender, AnimatorStateInfo a_stStateInfo, int a_nLayerIdx)
	{
		base.HandleOnStateExitSkillFinish(a_oSender, a_stStateInfo, a_nLayerIdx);
		this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_CONTINUING_SKILL, false);

		this.Owner.StateMachine.SetState(this.Owner.Animator.GetBool(ComType.G_PARAMS_IS_MOVE) ?
			this.Owner.CreateMoveState() : this.Owner.CreateIdleState());

		this.Owner.BattleController.CamDummy.GetComponent<CamDummy>().SetIsEnableUpdate(true);
	}

	/** 스킬을 적용한다 */
	private void ApplySkill()
	{
		this.IsApplySkill = true;
		this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_MOVE, false);

		var oPlayerController = this.GetOwner<PlayerController>();
		oPlayerController.ApplySkill(m_oApplySkillTable);
	}

	/** 타겟팅 스킬을 적용한다 */
	private void TryApplySkillTargeting()
	{
		var stRay = this.Owner.BattleController.MainCamera.ScreenPointToRay(Input.mousePosition);
		this.Owner.BattleController.TouchPointHandler.transform.localScale = Vector3.one * m_oApplySkillEffectTable.Value * ComType.G_UNIT_MM_TO_M * 2.0f;

		// 스킬 발동이 불가능 할 경우
		if (!Physics.Raycast(stRay, out RaycastHit stRaycastHit, float.MaxValue / 2.0f, m_nStructureLayerMask))
		{
			return;
		}

		// 터치 상태 일 경우
		if (Input.GetMouseButton(0))
		{
			this.Owner.BattleController.TouchPointHandler.SetIsOn(true);
			this.Owner.BattleController.TouchPointHandler.gameObject.SetActive(true);

			m_bIsTouch = true;
			this.Owner.BattleController.TouchPointHandler.transform.position = stRaycastHit.point + (Vector3.up * 0.15f);
		}

		this.Owner.NavMeshAgent.enabled = true;
		bool bIsValid = m_bIsTouch && this.Owner.NavMeshAgent.CalculatePath(stRaycastHit.point, m_oNavMeshPath);

		this.TargetPos = stRaycastHit.point;
		this.Owner.NavMeshAgent.enabled = false;

		// 스킬 발동이 불가능 할 경우
		if (!bIsValid || !Input.GetMouseButtonUp(0) || m_oNavMeshPath.status != NavMeshPathStatus.PathComplete)
		{
			this.Owner.BattleController.TouchPointHandler.SetIsOn(m_oNavMeshPath.status == NavMeshPathStatus.PathComplete);
			return;
		}

		this.ApplySkill();
		ComUtil.SetTimeScale(1.0f, true);
	}
	#endregion // 함수
}
