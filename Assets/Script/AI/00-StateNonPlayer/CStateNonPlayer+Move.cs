using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

/** NPC 이동 상태 */
public abstract partial class CStateNonPlayerMove : CStateUnitMove
{
	#region 프로퍼티
	public new NonPlayerController Owner => this.GetOwner<NonPlayerController>();
	#endregion // 프로퍼티

	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		this.Owner.StopLookAround();

		this.Owner.NavMeshAgent.enabled = true;
		this.Owner.NavMeshAgent.isStopped = !this.Owner.IsEnableMove;

		this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_MOVE, this.Owner.IsEnableMove);
	}

	/** 상태가 종료 되었을 경우 */
	public override void OnStateExit()
	{
		base.OnStateExit();
		this.Owner.NavMeshAgent.isStopped = true;

		this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_MOVE, false);
	}
	#endregion // 함수
}

/** NPC 순찰 이동 상태 */
public partial class CStateNonPlayerMovePatrol : CStateNonPlayerMove
{
	#region 변수
	private bool m_bIsCompleteMove = false;

	private int m_nWayPointIdx = 0;
	private int m_nNextWayPointIdxDirection = 1;
	#endregion // 변수

	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();

		this.Owner.NavMeshAgent.speed = this.Owner.MoveSpeed;
		this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_MOVE, this.Owner.IsEnableMove && this.Owner.WayPointInfoList.Count >= 1);
	}

	/** 상태를 갱신한다 */
	public override void OnStateCustomUpdate(float a_fDeltaTime)
	{
		base.OnStateCustomUpdate(a_fDeltaTime);
		this.Owner.NavMeshAgent.isStopped = !this.Owner.IsEnableMove || this.Owner.WayPointInfoList.Count <= 0;

		// 추적 대상이 존재 할 경우
		if (this.Owner.TrackingTarget != null)
		{
			this.Owner.StateMachine.SetState(this.Owner.CreateMoveState());
		}
		// 이동 지점 정보가 존재 할 경우
		else if (!m_bIsCompleteMove && this.Owner.WayPointInfoList.Count >= 1)
		{
			var oWayPointInfo = this.Owner.WayPointInfoList[m_nWayPointIdx];
			this.Owner.NavMeshAgent.SetDestination(oWayPointInfo.m_stTransInfo.m_stPos);

			// 이동 지점에 도착했을 경우
			if (this.Owner.transform.position.ExIsEquals(oWayPointInfo.m_stTransInfo.m_stPos))
			{
				m_bIsCompleteMove = true;
				this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_MOVE, false);

				this.Owner.ExLateCallFunc(this.OnCompleteMove, this.Owner.WayPointInfoList[m_nWayPointIdx].m_fPreDelay);
			}
		}
	}

	/** 다음 이동 지점을 설정한다 */
	private void SetupNextWayPoint()
	{
		// 유효하지 않을 경우
		if (!this.IsEnable)
		{
			return;
		}

		int nNextWayPointIdx = m_nWayPointIdx + m_nNextWayPointIdxDirection;
		this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_MOVE, this.Owner.IsEnableMove);

		m_nNextWayPointIdxDirection = (nNextWayPointIdx >= 0 && nNextWayPointIdx < this.Owner.WayPointInfoList.Count) ? m_nNextWayPointIdxDirection : -m_nNextWayPointIdxDirection;
		m_nWayPointIdx = Mathf.Clamp(m_nWayPointIdx + m_nNextWayPointIdxDirection, 0, this.Owner.WayPointInfoList.Count - 1);

		m_bIsCompleteMove = false;
	}

	/** 이동을 완료했을 경우 */
	private void OnCompleteMove(MonoBehaviour a_oSender)
	{
		switch (this.Owner.WayPointInfoList[m_nWayPointIdx].m_eWayPointType)
		{
			case EWayPointType.LOOK_AROUND: this.Owner.StartLookAround(this.Owner.WayPointInfoList[m_nWayPointIdx], this.OnCompleteLookAround); break;
			default: this.SetupNextWayPoint(); break;
		}
	}

	/** 주변 경계를 완료했을 경우 */
	private void OnCompleteLookAround(NonPlayerController a_oSender)
	{
		this.Owner.StopLookAround();
		this.Owner.ExLateCallFunc((a_oSender) => this.SetupNextWayPoint(), this.Owner.WayPointInfoList[m_nWayPointIdx].m_fPostDelay);
	}
	#endregion // 함수
}

/** NPC 전투 이동 상태 */
public partial class CStateNonPlayerMoveBattle : CStateNonPlayerMove
{
	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_MOVE, this.Owner.IsEnableTracking);

		this.Owner.NavMeshAgent.speed = this.Owner.MoveSpeed;
		this.Owner.NavMeshAgent.isStopped = !this.Owner.IsEnableTracking;

		// 추적 대상이 존재 할 경우
		if(this.Owner.TrackingTarget != null)
		{
			this.Owner.NavMeshAgent.SetDestination(this.Owner.TrackingTarget.transform.position);
		}
	}

	/** 상태가 종료 되었을 경우 */
	public override void OnStateExit()
	{
		base.OnStateExit();
		this.Owner.Animator.speed = 1.0f;
	}

	/** 상태를 갱신한다 */
	public override void OnStateUpdate(float a_fDeltaTime)
	{
		base.OnStateUpdate(a_fDeltaTime);
		this.Owner.Animator.speed = 1.0f + ComUtil.GetAbilityVal(EEquipEffectType.MoveSpeed, this.Owner.CurAbilityValDict);

		// 추적이 가능 할 경우
		if (this.Owner.IsEnableTracking)
		{
			return;
		}

		this.Owner.LookTarget(this.Owner.LockOnTarget ?? this.Owner.TrackingTarget, true, a_fDeltaTime * 15.0f);
	}

	/** 상태를 갱신한다 */
	public override void OnStateCustomUpdate(float a_fDeltaTime)
	{
		base.OnStateCustomUpdate(a_fDeltaTime);
		this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_BATTLE, true);

		// 추적 대상이 없을 경우
		if(this.Owner.TrackingTarget == null)
		{
			return;
		}

		this.Owner.NavMeshAgent.SetDestination(this.Owner.TrackingTarget.transform.position);

		// 조준 가능 할 경우
		if (this.Owner.LockOnTarget != null && this.Owner.IsAimableTarget(this.Owner.LockOnTarget))
		{
			this.Owner.StateMachine.SetState(this.Owner.CreateBattleReadyState());
		}
	}
	#endregion // 함수
}

/** NPC 회피 이동 상태 */
public partial class CStateNonPlayerMoveAvoid : CStateNonPlayerMove
{
	#region 변수
	private int m_nAvoidAreaMask = 0;
	private bool m_bIsAvoidDelay = false;
	private float m_fAvoidDelayRemainTime = 0.0f;

	private Vector3 m_stAvoidPos = Vector3.zero;
	private NavMeshPath m_oNavMeshPath = new NavMeshPath();
	private RaycastHit[] m_oRaycastHits = new RaycastHit[ComType.G_MAX_NUM_RAYCAST_NON_ALLOC];
	#endregion // 변수

	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_BATTLE, this.Owner.TrackingTarget != null);

		m_nAvoidAreaMask = 1 << NavMesh.GetAreaFromName(ComType.G_NAV_MESH_AREA_WALKABLE);

		// 도망 타입 일 경우
		if (this.Owner.Table.ActionType == (int)EActionType.ONLY_AVOID)
		{
			m_stAvoidPos = this.Owner.BattleController.GetSummonPos(this.Owner.NavMeshAgent);
		}
		else
		{
			m_stAvoidPos = this.IsValidAvoidPos(this.Owner.transform.position) ? this.Owner.transform.position : this.FindAvoidPos();
		}

		this.Owner.NavMeshAgent.speed = this.Owner.MoveSpeed;
		this.OnStateCustomUpdate(0.0f);
	}

	/** 상태를 갱신한다 */
	public override void OnStateCustomUpdate(float a_fDeltaTime)
	{
		base.OnStateCustomUpdate(a_fDeltaTime);
		this.Owner.NavMeshAgent.SetDestination(m_stAvoidPos);

		// 도망 지연 상태 일 경우
		if (m_bIsAvoidDelay)
		{
			m_fAvoidDelayRemainTime = Mathf.Max(0.0f, m_fAvoidDelayRemainTime - a_fDeltaTime);

			// 도망 지연 중 일 경우
			if (m_fAvoidDelayRemainTime.ExIsGreat(0.0f))
			{
				return;
			}

			var stAvoidPos = this.FindAvoidPos();
			this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_MOVE, true);

			m_stAvoidPos = this.IsCompleteAvoid(stAvoidPos) ?
				this.Owner.BattleController.GetSummonPos(this.Owner.NavMeshAgent) : stAvoidPos;

			m_bIsAvoidDelay = false;
			this.Owner.NavMeshAgent.SetDestination(m_stAvoidPos);
		}

		// 회피 완료 상태가 아닐 경우
		if (!this.IsCompleteAvoid(m_stAvoidPos))
		{
			return;
		}

		// 회피 위치가 유효하지 않을 경우
		if (!this.IsValidAvoidPos(m_stAvoidPos))
		{
			m_stAvoidPos = this.FindAvoidPos();

			// 회피가 완료 되었을 경우
			if (this.Owner.Table.ActionType == (int)EActionType.ONLY_AVOID && this.IsCompleteAvoid(m_stAvoidPos))
			{
				m_stAvoidPos = this.Owner.BattleController.GetSummonPos(this.Owner.NavMeshAgent);
			}
		}

		// 회피가 완료 되었을 경우
		if (this.IsCompleteAvoid(m_stAvoidPos))
		{
			this.HandleOnCompleteAvoid();
		}
	}

	/** 회피 위치를 탐색한다 */
	private Vector3 FindAvoidPos()
	{
		// 추적 대상이 없을 경우
		if(this.Owner.TrackingTarget == null)
		{
			return this.Owner.transform.position;
		}

		var oAvoidPosList = CCollectionPoolManager.Singleton.SpawnList<Vector3>();

		try
		{
			float fAttackRange = this.Owner.AttackRange * ComType.G_UNIT_MM_TO_M;

			var stDelta = this.Owner.GetAttackRayOriginPos() - this.Owner.TrackingTarget.GetAttackRayOriginPos();
			var stCenter = this.Owner.TrackingTarget.GetAttackRayOriginPos() + (stDelta.normalized * fAttackRange / 2.0f);

			for (int i = 0; i <= ComType.G_ANGLE_FIND_AVOID_POS; i += ComType.G_OFFSET_FIND_AVOID_POS)
			{
				var stAvoidDirection = (Vector3)(Quaternion.AngleAxis(i - (ComType.G_ANGLE_FIND_AVOID_POS / 2.0f), Vector3.up) * stDelta.normalized);
				var stAvoidEdgePos = stCenter + (stAvoidDirection.normalized * fAttackRange / 2.0f);

				// 내비게이션 메쉬 영역을 벗어났을 경우
				if (!NavMesh.SamplePosition(stAvoidEdgePos + (Vector3.up * 1.5f), out NavMeshHit stNavMeshHit, float.MaxValue / 2.0f, m_nAvoidAreaMask))
				{
					continue;
				}

				var stAvoidDelta = this.Owner.TrackingTarget.GetAttackRayOriginPos() - stNavMeshHit.position;
				int nNumColliders = Physics.RaycastNonAlloc(stNavMeshHit.position, stAvoidDelta.normalized, m_oRaycastHits, stAvoidDelta.magnitude, this.Owner.AvoidLayerMask);
				float fMaxDistance = float.MinValue / 2.0f;

				for (int j = 0; j < nNumColliders; ++j)
				{
					float fDistance = Vector3.Distance(stNavMeshHit.position, m_oRaycastHits[j].transform.position);
					fMaxDistance = fMaxDistance.ExIsGreat(fDistance) ? fMaxDistance : fDistance;
				}

				var stOffset = new Vector3(stAvoidDelta.normalized.x, 0.0f, stAvoidDelta.normalized.z);
				var stAvoidPos = stNavMeshHit.position + (stAvoidDelta.normalized * fMaxDistance) - stOffset.normalized;

				bool bIsValid = nNumColliders >= 1 && NavMesh.SamplePosition(stAvoidPos, out stNavMeshHit, float.MaxValue / 2.0f, m_nAvoidAreaMask);
				bIsValid = bIsValid && Vector3.Distance(stNavMeshHit.position, this.Owner.TrackingTarget.transform.position).ExIsLessEquals(fAttackRange);
				bIsValid = bIsValid && this.IsValidAvoidPos(stNavMeshHit.position) && this.Owner.BattleController.PlayerNavMeshBounds.ExIsContainsAABB(stNavMeshHit.position);
				bIsValid = bIsValid && this.Owner.NavMeshAgent.CalculatePath(stNavMeshHit.position, m_oNavMeshPath);

				// 충돌 정보가 존재 할 경우
				if (bIsValid && m_oNavMeshPath.status == NavMeshPathStatus.PathComplete)
				{
					oAvoidPosList.Add(stNavMeshHit.position);
				}
			}

			oAvoidPosList.ExShuffle();
			return (oAvoidPosList.Count >= 1) ? oAvoidPosList[0] : this.Owner.transform.position;
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oAvoidPosList);
		}
	}

	/** 회피 완료 상태를 처리한다 */
	private void HandleOnCompleteAvoid()
	{
		switch ((EActionType)this.Owner.Table.ActionType)
		{
			case EActionType.ONLY_AVOID:
				{
					m_bIsAvoidDelay = true;
					m_fAvoidDelayRemainTime = 1.0f;

					this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_MOVE, false);
					break;
				}
			default:
				{
					this.Owner.SetIsAvoidAction(false);
					this.Owner.StateMachine.SetState(this.Owner.CreateIdleState());

					break;
				}
		}
	}
	#endregion // 함수

	#region 접근 함수
	/** 올바른 회피 위치 여부를 검사한다 */
	private bool IsValidAvoidPos(Vector3 a_stPos)
	{
		// 추적 대상이 없을 경우
		if(this.Owner.TrackingTarget == null)
		{
			return false;
		}

		var stDelta = this.Owner.TrackingTarget.transform.position - a_stPos;
		return Physics.Raycast(a_stPos, stDelta.normalized, stDelta.magnitude, this.Owner.AvoidLayerMask);
	}

	/** 회피 완료 여부를 검사한다 */
	private bool IsCompleteAvoid(Vector3 a_stPos)
	{
		return a_stPos.ExIsEquals(this.Owner.transform.position) || Vector3.Distance(a_stPos, this.Owner.transform.position).ExIsLessEquals(0.35f);
	}
	#endregion // 접근 함수
}
