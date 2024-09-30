using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

/** NPC 전투 등장 상태 */
public partial class CStateNonPlayerAppear : CStateUnitAppear
{
	#region 변수
	private bool m_bIsCompleteMove = false;
	private float m_fUpdateSkipTime = 0.0f;

	private Tween m_oMoveAnim = null;
	private NavMeshPath m_oNavMeshPath = new NavMeshPath();
	#endregion // 변수

	#region 프로퍼티
	public new NonPlayerController Owner => this.GetOwner<NonPlayerController>();
	#endregion // 프로퍼티

	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		this.Owner.StopLookAround();

		m_bIsCompleteMove = false;
		m_fUpdateSkipTime = 0.0f;

		this.Owner.LookAt(this.Owner.StartPos);

		this.Owner.NavMeshAgent.enabled = true;
		bool bIsValid = this.Owner.NavMeshAgent.CalculatePath(this.Owner.BattleController.PlayerController.transform.position, m_oNavMeshPath);

		this.Owner.NavMeshAgent.enabled = bIsValid && m_oNavMeshPath.status == NavMeshPathStatus.PathComplete;
		this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_MOVE, this.Owner.IsEnableMove);

		// 네비게이션 이동이 가능 할 경우
		if(this.Owner.NavMeshAgent.enabled)
		{
			this.Owner.NavMeshAgent.isStopped = false;
			this.Owner.NavMeshAgent.SetDestination(this.Owner.StartPos);
		}
		else
		{
			int nWalkable = 1 << NavMesh.GetAreaFromName(ComType.G_NAV_MESH_AREA_WALKABLE);
			NavMesh.SamplePosition(this.Owner.StartPos, out NavMeshHit stNavMeshHit, float.MaxValue / 2.0f, nWalkable);

			var stDelta = stNavMeshHit.position - this.Owner.transform.position;
			float fDuration = stDelta.magnitude / (float)this.Owner.MoveSpeed;

			m_oMoveAnim = this.Owner.transform.DOMove(stNavMeshHit.position, fDuration).SetEase(Ease.Linear);
			m_oMoveAnim.OnComplete(this.OnCompleteMove);
		}
	}

	/** 상태가 종료 되었을 경우 */
	public override void OnStateExit()
	{
		base.OnStateExit();
		ComUtil.AssignVal(ref m_oMoveAnim, null);
	}

	/** 상태를 갱신한다 */
	public override void OnStateCustomUpdate(float a_fDeltaTime)
	{
		base.OnStateCustomUpdate(a_fDeltaTime);
		bool bIsComplete = ComUtil.AlmostEquals(this.Owner.transform.position, this.Owner.StartPos, 0.01f);

		m_fUpdateSkipTime += a_fDeltaTime;

		// 이동 지점에 도착했을 경우
		if ((bIsComplete || m_fUpdateSkipTime.ExIsGreat(5.0f)) && !m_bIsCompleteMove)
		{
			this.Owner.ExLateCallFunc((a_oSender) => this.OnCompleteMove());
		}
	}

	/** 이동이 완료 되었을 경우 */
	private void OnCompleteMove()
	{
		m_bIsCompleteMove = true;
		this.Owner.Animator.SetBool(ComType.G_PARAMS_IS_MOVE, false);

		this.Owner.StateMachine.SetState(this.Owner.CreateMoveState());
	}
	#endregion // 함수
}
