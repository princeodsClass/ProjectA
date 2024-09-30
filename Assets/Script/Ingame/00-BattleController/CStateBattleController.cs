using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 전투 제어자 상태 */
public class CStateBattleController : CState<BattleController>
{
	#region 프로퍼티
	public bool IsCompleteCameraDirecting { get; private set; } = false;

	public new BattleController Owner => this.GetOwner<BattleController>();
	public virtual bool IsEnableFinishBattle => true;
	#endregion // 프로퍼티

	#region 함수
	/** 전투를 종료한다 */
	public virtual void FinishBattle(bool a_bIsWin, float a_fDelay)
	{
		// 전투 종료가 불가능 할 경우
		if (!this.IsEnableFinishBattle)
		{
			return;
		}

		// 승리 상태 일 경우
		if (a_bIsWin)
		{
			this.Owner.PlayerController.SetUntouchableTime(float.MaxValue / 2.0f);
		}

		GameDataManager.Singleton.ResetPlayerInfos();
		this.Owner.StartCoroutine(this.Owner.CoFinishBattle(a_bIsWin, a_fDelay));
	}
	#endregion // 함수

	#region 접근 함수
	/** 카메라 연출 완료 여부를 변경한다 */
	public void SetIsCompleteCameraDirecting(bool a_bIsComplete)
	{
		this.IsCompleteCameraDirecting = a_bIsComplete;
	}
	#endregion // 접근 함수
}

/** 전투 제어자 준비 상태 */
public class CStateBattleControllerReady : CStateBattleController
{
	#region 변수
	private float m_fUpdateSkipTime = 0.0f;
	#endregion // 변수

	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		this.SetIsCompleteCameraDirecting(!this.Owner.IsBossStage);

		// 웨이브 모드 일 경우
		if (GameDataManager.Singleton.IsWaveMode())
		{
			this.Owner.InitWaveState();
		}

		// 보스 스테이지 일 경우
		if (this.Owner.IsBossStage && GameDataManager.Singleton.PlayMapInfoType != EMapInfoType.ADVENTURE)
		{
			this.Owner.StartBossEnterDirecting();
		}
	}

	/** 상태가 종료 되었을 경우 */
	public override void OnStateExit()
	{
		base.OnStateExit();
		this.Owner.PageBattle.BottomUIs.SetActive(true);

		GameDataManager.Singleton.BuffEffectTableInfoList.ExCopyTo(this.Owner.PageBattle.BuffEffectTableInfoList,
			(a_oTableInfo) => a_oTableInfo);

		GameDataManager.Singleton.NonPlayerPassiveEffectStackInfoList.ExCopyTo(this.Owner.NonPlayerPassiveEffectStackInfoList,
			(a_stStackInfo) => a_stStackInfo);

		this.Owner.SetupNonPlayerEffectStackInfos();
		this.Owner.PageBattle.SetIsDirtyUpdateUIsState(true);

		for (int i = 0; i < this.Owner.NonPlayerControllerList.Count; ++i)
		{
			this.Owner.NonPlayerControllerList[i].OnBattlePlay();
		}

		// 웨이브 모드 일 경우
		if (GameDataManager.Singleton.IsWaveMode())
		{
			this.Owner.StartWave();
		}
		else
		{
			this.Owner.PlayerController.OnBattlePlay();
		}
	}

	/** 상태를 갱신한다 */
	public override void OnStateUpdate(float a_fDeltaTime)
	{
		base.OnStateUpdate(a_fDeltaTime);
		m_fUpdateSkipTime += a_fDeltaTime;

		// 보스 연출이 진행 중 일 경우
		if (!this.IsCompleteCameraDirecting)
		{
			return;
		}

		float fVertical = Input.GetAxis("Vertical") + this.Owner.Joystick.Vertical;
		float fHorizontal = Input.GetAxis("Horizontal") + this.Owner.Joystick.Horizontal;

		bool bIsEnablePlay = this.Owner.IsRunning;
		bIsEnablePlay = bIsEnablePlay && !fVertical.ExIsEquals(fHorizontal);
		bIsEnablePlay = bIsEnablePlay && this.Owner.StateMachine.State is CStateBattleControllerReady;

		// 입력이 감지 되었을 경우
		if ((m_fUpdateSkipTime.ExIsGreatEquals(0.25f) || bIsEnablePlay) && !this.Owner.IsPlaySecneDirecting)
		{
			this.Owner.StateMachine.SetState(this.Owner.CreatePlayState());
		}
	}
	#endregion // 함수
}

/** 전투 제어자 플레이 상태 */
public class CStateBattleControllerPlay : CStateBattleController
{
	#region 변수
	private float m_fUpdateSkipTime = 0.0f;
	#endregion // 변수

	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		this.Owner.SetIsEnableUpdate(true);

		// 효과 적용 상태 일 경우
		if (this.Owner.IsApplyEffects)
		{
			return;
		}

		this.Owner.SetIsApplyEffects(true);

		var oEffectTable = this.Owner.NPCEffectTableList.ExGetData(EEffectType.LIMIT_FORCE_WALK,
			ERangeType.HIT_TARGET_RANGE, EEffectCategory.TRAP_EFFECT);

		// 강제 걷기 효과가 없을 경우
		if (oEffectTable == null)
		{
			return;
		}

		// TODO: 맵 효과 적용 구문 수정 필요
		this.Owner.ApplyDamageFieldEffect(oEffectTable, false);
	}

	/** 상태를 갱신한다 */
	public override void OnStateCustomUpdate(float a_fDeltaTime)
	{
		base.OnStateCustomUpdate(a_fDeltaTime);

		// 웨이브 모드 일 경우
		if (GameDataManager.Singleton.IsWaveMode())
		{
			this.Owner.UpdateWaveState(a_fDeltaTime);
		}

		// 지속 스킬 적용 상태 일 경우
		if(this.Owner.IsApplyContinueSkill)
		{
			return;
		}

		m_fUpdateSkipTime += a_fDeltaTime;

		// 지속 스킬 적용이 가능 할 경우
		if(m_fUpdateSkipTime.ExIsGreat(2.5f))
		{
			this.Owner.SetIsApplyContinueSkill(true);

			for (int i = 0; i < this.Owner.NonPlayerControllerList.Count; ++i)
			{
				int nSkillGroup = this.Owner.NonPlayerControllerList[i].StatTable.ContinuouslySkillGroup;
				this.Owner.NonPlayerControllerList[i].ApplySkillGroup(nSkillGroup, true, true);
			}
		}
	}
	#endregion // 함수
}

/** 전투 제어자 이어하기 상태 */
public class CStateBattleControllerContinue : CStateBattleController
{
	// Do Something
}

/** 전투 제어자 종료 상태 */
public class CStateBattleControllerFinish : CStateBattleController
{
	#region 프로퍼티
	public override bool IsEnableFinishBattle => false;
	#endregion // 프로퍼티

	#region 함수
	/** 상태가 시작 되었을 경우 */
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		this.Owner.SetIsEnableUpdate(false);
	}
	#endregion // 함수
}
