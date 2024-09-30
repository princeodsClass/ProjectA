using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** NPC 제어자 - 상태 */
public partial class NonPlayerController : UnitController
{
	#region 함수
	/** 사망 상태를 처리한다 */
	protected override void HandleDieState(UnitController a_oAttacker, Vector3 a_stDirection, STHitInfo a_stHitInfo)
	{
		base.HandleDieState(a_oAttacker, a_stDirection, a_stHitInfo);
		var oPlayerController = a_oAttacker as PlayerController;

		// 웨이브 NPC 일 경우
		if (this.IsWaveNPC)
		{
			this.Renderer.material.SetColor(ComType.G_DAMAGE_TINT, ComType.G_WAVE_NPC_COLOR);
		}

		// 공격자가 플레이어 일 경우
		if (oPlayerController != null && !this.BattleController.IsBossStage)
		{
			float fAccumulateStageInstEXP = this.BattleController.PlayerController.AccumulateStageInstEXP;
			oPlayerController.SetAccumulateStageInstEXP(fAccumulateStageInstEXP + (this.Table.InstancePoint * this.BattleController.InstEXPRatio));

			// 스킬 포인트 누적이 가능 할 경우
			if (!a_stHitInfo.m_bIsSkill)
			{
				int nSkillPoint = this.Table.SkillPoint;
				float fSkillPointRatio = ComUtil.GetAbilityVal(EEquipEffectType.ActiveSkillChargeRatio, a_oAttacker.CurAbilityValDict);

				oPlayerController.IncrActiveSkillPoint(Mathf.FloorToInt(nSkillPoint * (1.0f + fSkillPointRatio)));
			}
		}

		// 사망 사운드가 존재 할 경우
		if (oPlayerController != null && this.BattleController.SoundModelInfo != null && this.BattleController.SoundModelInfo.UnitKillSoundList.ExIsValid())
		{
			var stCurTime = System.DateTime.Now;
			var stDeltaTime = stCurTime - this.BattleController.PrevKillSoundPlayTime;

			float fPitchOffset = GlobalTable.GetData<float>(ComType.G_VALUE_PITCH_OFFSET);
			float fContinuouslyKillTime = GlobalTable.GetData<int>(ComType.G_TIME_CONTINUOUSLY_KILL) * ComType.G_UNIT_MS_TO_S;
			float fIgnoreContinuouslyKillTime = GlobalTable.GetData<int>(ComType.G_TIME_IGNORE_CONTINUOUSLY_KILL) * ComType.G_UNIT_MS_TO_S;

			// 사운드 재생이 가능 할 경우
			if (stDeltaTime.TotalSeconds.ExIsGreatEquals(fIgnoreContinuouslyKillTime))
			{
				// 연속 재생이 가능 할 경우
				if (stDeltaTime.TotalSeconds.ExIsLessEquals(fContinuouslyKillTime))
				{
					this.BattleController.SetCurSkillSoundIdx(this.BattleController.CurSkillSoundIdx + 1);
				}
				else
				{
					this.BattleController.SetCurSkillSoundIdx(0);
				}

				int nSoundIdx = 0;
				this.BattleController.SetPrevKillSoundPlayTime(stCurTime);

				this.BattleController.PlaySound(this.BattleController.SoundModelInfo.UnitKillSoundList[nSoundIdx],
					null, "Master/SFX/Gacha", 1.0f + (this.BattleController.CurSkillSoundIdx * fPitchOffset));
			}
		}

		this.BattleController.IncrAbyssPoint(this.Table.AbyssPoint);
		this.BattleController.IncrInfinitePoint(this.Table.EndPoint);
		this.BattleController.AddReleaseUnit(this.gameObject, this.CurWeaponObj);
	}
	#endregion // 함수

	#region 팩토리 함수
	/** 초기 상태를 생성한다 */
	public override CStateUnit CreateInitState()
	{
		// 등장 처리가 필요 할 경우
		if (this.IsNeedAppear)
		{
			return this.CreateAppearState();
		}

		bool bIsPatrolTypeA = this.ObjInfo != null && this.ObjInfo.m_ePatrolType == EPatrolType.FIX;
		bool bIsPatrolTypeB = this.ObjInfo != null && this.ObjInfo.m_ePatrolType == EPatrolType.WAY_POINT;

		// 정찰 타입 일 경우
		if (this.Table.ActionType != (int)EActionType.ONLY_AVOID && (bIsPatrolTypeA || bIsPatrolTypeB))
		{
			return this.CreateMoveState();
		}

		return this.CreateIdleState();
	}

	/** 대기 상태를 생성한다 */
	public override CStateUnitIdle CreateIdleState()
	{
		// 주변 경계가 가능 할 경우
		if (this.ObjInfo != null && (this.ObjInfo.m_ePatrolType == EPatrolType.LOOK_AROUND && this.TrackingTarget == null))
		{
			return CObjsPoolManager.Singleton.SpawnObj<CStateNonPlayerIdleLookAround>();
		}

		return CObjsPoolManager.Singleton.SpawnObj<CStateNonPlayerIdle>();
	}

	/** 이동 상태를 생성한다 */
	public override CStateUnitMove CreateMoveState()
	{
		// 추적 대상이 없을 경우
		if (this.TrackingTarget == null)
		{
			return CObjsPoolManager.Singleton.SpawnObj<CStateNonPlayerMovePatrol>();
		}

		return CObjsPoolManager.Singleton.SpawnObj<CStateNonPlayerMoveBattle>();
	}

	/** 도망 상태를 생성한다 */
	public override CStateUnitMove CreateAvoidState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStateNonPlayerMoveAvoid>();
	}

	/** 전투 준비 상태를 생성한다 */
	public override CStateUnitBattleReady CreateBattleReadyState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStateNonPlayerBattleReady>();
	}

	/** 전투 상태를 생성한다 */
	public override CStateUnitBattleLockOn CreateBattleLockOnState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStateNonPlayerBattleLockOn>();
	}

	/** 전투 재장전 상태를 생성한다 */
	public override CStateUnitBattleReload CreateBattleReloadState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStateNonPlayerBattleReload>();
	}

	/** 전투 스킬 상태를 생성한다 */
	public override CStateUnitBattleSkill CreateBattleSkillState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStateNonPlayerBattleSkill>();
	}

	/** 등장 상태를 생성한다 */
	private CStateUnitAppear CreateAppearState()
	{
		return CObjsPoolManager.Singleton.SpawnObj<CStateNonPlayerAppear>();
	}
	#endregion // 팩토리 함수
}
