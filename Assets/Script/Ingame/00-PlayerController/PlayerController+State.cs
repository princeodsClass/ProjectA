using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 플레이어 제어자 - 상태 */
public partial class PlayerController : UnitController
{
	#region 함수
	/** 사망 상태를 처리한다 */
	protected override void HandleDieState(UnitController a_oAttacker, Vector3 a_stDirection, STHitInfo a_stHitInfo)
	{
		base.HandleDieState(a_oAttacker, a_stDirection, a_stHitInfo);

		for(int i = 0; i < this.SightLineHandlers.Length; ++i)
		{
			this.SightLineHandlers[i]?.gameObject.SetActive(false);
		}
	}
	#endregion // 함수

	#region 팩토리 함수
	/** 초기 상태를 생성한다 */
	public override CStateUnit CreateInitState()
	{
		return this.CreateIdleState();
	}

	/** 대기 상태를 생성한다 */
	public override CStateUnitIdle CreateIdleState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStatePlayerIdle>();
	}

	/** 이동 상태를 생성한다 */
	public override CStateUnitMove CreateMoveState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStatePlayerMove>();
	}

	/** 전투 준비 상태를 생성한다 */
	public override CStateUnitBattleReady CreateBattleReadyState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStatePlayerBattleReady>();
	}

	/** 전투 상태를 생성한다 */
	public override CStateUnitBattleLockOn CreateBattleLockOnState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStatePlayerBattleLockOn>();
	}

	/** 전투 재장전 상태를 생성한다 */
	public override CStateUnitBattleReload CreateBattleReloadState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStatePlayerBattleReload>();
	}

	/** 전투 스킬 상태를 생성한다 */
	public override CStateUnitBattleSkill CreateBattleSkillState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStatePlayerBattleSkill>();
	}

	/** 공중 접촉 상태를 생성한다 */
	public override CStateUnitContactAir CreateContactAirState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStatePlayerContactAir>();
	}

	/** 바닥 접촉 상태를 생성한다 */
	public override CStateUnitContactGround CreateContactGroundState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStatePlayerContactGround>();
	}
	#endregion // 팩토리 함수
}
