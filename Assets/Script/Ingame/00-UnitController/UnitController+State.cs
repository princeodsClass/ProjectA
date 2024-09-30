using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 유닛 제어자 */
public abstract partial class UnitController : MonoBehaviour, IUpdatable
{
	#region 팩토리 함수
	/** 초기 상태를 생성한다 */
	public virtual CStateUnit CreateInitState()
	{
		return null;
	}

	/** 대기 상태를 생성한다 */
	public virtual CStateUnitIdle CreateIdleState()
	{
		return null;
	}

	/** 이동 상태를 생성한다 */
	public virtual CStateUnitMove CreateMoveState()
	{
		return null;
	}

	/** 도망 상태를 생성한다 */
	public virtual CStateUnitMove CreateAvoidState()
	{
		return null;
	}

	/** 전투 준비 상태를 생성한다 */
	public virtual CStateUnitBattleReady CreateBattleReadyState()
	{
		return null;
	}

	/** 전투 조준 상태를 생성한다 */
	public virtual CStateUnitBattleLockOn CreateBattleLockOnState()
	{
		return null;
	}

	/** 전투 재장전 상태를 생성한다 */
	public virtual CStateUnitBattleReload CreateBattleReloadState()
	{
		return null;
	}

	/** 전투 스킬 상태를 생성한다 */
	public virtual CStateUnitBattleSkill CreateBattleSkillState()
	{
		return null;
	}

	/** 공중 접촉 상태를 생성한다 */
	public virtual CStateUnitContactAir CreateContactAirState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStateUnitContactAir>();
	}

	/** 바닥 접촉 상태를 생성한다 */
	public virtual CStateUnitContactGround CreateContactGroundState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStateUnitContactGround>();
	}

	/** 워프 접촉 상태를 생성한다 */
	public virtual CStateUnitContactWarp CreateContactWarpState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStateUnitContactWarp>();
	}
	#endregion // 팩토리 함수
}
