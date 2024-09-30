using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** NPC 제어자 - 능력치 */
public partial class NonPlayerController : UnitController
{
	#region 프로퍼티
	public override float WarheadSpeed => this.GetWarheadSpeed();
	#endregion // 프로퍼티

	#region 함수
	/** 어빌리티 값을 설정한다 */
	public override void SetupAbilityValues(bool a_bIsSetupEffectStackInfos = false)
	{
		base.SetupAbilityValues(a_bIsSetupEffectStackInfos);

		// 값 설정이 가능 할 경우
		if(this.BattleController != null)
		{
			this.SetupAbilityValues(this.CurStandardAbilityValDict,
				this.BattleController.NonPlayerEffectStackInfoList, this.CurAbilityValDict, false);

			this.SetupAbilityValues(this.CurStandardAbilityValDict,
				this.BattleController.NonPlayerEffectStackInfoList, this.CurOriginAbilityValDict, false);
		}
		
		this.NavMeshAgent.speed = this.MoveSpeed;
	}
	#endregion // 함수

	#region 접근 함수
	/** 탄두 속도를 반환한다 */
	public virtual float GetWarheadSpeed()
	{
		float fVal = this.CurAbilityValDict.GetValueOrDefault(EEquipEffectType.WARHEAD_SPEED);
		return Mathf.Max(0.1f, fVal * (1.0f + this.CurAbilityValDict.GetValueOrDefault(EEquipEffectType.AmmoSpeedRatio)));
	}
	#endregion // 접근 함수
}
