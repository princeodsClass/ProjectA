using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 플레이어 제어자 - 능력치 */
public partial class PlayerController : UnitController
{
	#region 프로퍼티
	public float MaxMoveSpeedRatio => this.CurAbilityValDict.GetValueOrDefault(EEquipEffectType.FORCE_WALK, 1.0f);
	public override float AttackPower => this.GetAttackPower();
	#endregion // 프로퍼티

	#region 함수
	/** 어빌리티 값을 설정한다 */
	public override void SetupAbilityValues(bool a_bIsSetupEffectStackInfos = false)
	{
		base.SetupAbilityValues(a_bIsSetupEffectStackInfos);

		for (int i = 0; i < this.AttackDelayList.Count; ++i)
		{
			// 슬롯이 비어 있을 경우
			if (this.IsEmptySlot(i))
			{
				continue;
			}

			float fReloadTime = this.AbilityValDicts[i].GetValueOrDefault(EEquipEffectType.RELOAD_TIME) * ComType.G_UNIT_MS_TO_S;

			var stReloadInfo = this.ReloadInfoList[i];
			stReloadInfo.m_fMaxReloadTime = fReloadTime * (1.0f + ComUtil.GetAbilityVal(EEquipEffectType.ReloadTime, this.AbilityValDicts[i]));
			stReloadInfo.m_fRemainTime = Mathf.Min(stReloadInfo.m_fRemainTime, stReloadInfo.m_fMaxReloadTime);

			this.ReloadInfoList[i] = stReloadInfo;
		}
	}
	#endregion // 함수

	#region 접근 함수
	/** 추가 피격 공격력을 반환한다 */
	public override float GetExtraHitATK(ProjectileController a_oController, UnitController a_oTarget)
	{
		switch(a_oController.Params.m_eAttackType)
		{
			case EAttackType.SHOOT: return this.GetExtraHitATKShoot(a_oController, a_oTarget);
			case EAttackType.THROW: return this.GetExtraHitATKThrow(a_oController, a_oTarget);
		}

		return base.GetExtraHitATK(a_oController, a_oTarget);
	}

	/** 공격력을 반환한다 */
	private float GetAttackPower()
	{
		float fATK = this.CurAbilityValDict.GetValueOrDefault(EEquipEffectType.ATK);

		// 잔탄 공격력 증가가 불가능 할 경우
		if(!this.CurAbilityValDict.ContainsKey(EEquipEffectType.ATTACK_POWER_UP_BY_MAGAZIN))
		{
			return fATK;
		}

		float fPercent = (this.CurMagazineInfo.m_nNumBullets - 1) / (float)this.CurMagazineInfo.m_nMaxNumBullets;
		fPercent = 1.0f - fPercent;

		float fExtraATK = fATK * (this.CurAbilityValDict[EEquipEffectType.ATTACK_POWER_UP_BY_MAGAZIN] * fPercent);
		return fATK + fExtraATK;
	}

	/** 발사 추가 피격 공격력을 반환한다 */
	private float GetExtraHitATKShoot(ProjectileController a_oController, UnitController a_oTarget)
	{
		var oPlayerController = a_oController.Params.m_oOwner as PlayerController;
		int nWeaponIdx = oPlayerController.GetWeaponIdx(a_oController.Params.m_oWeapon);

		// 효과 처리가 불가능 할 경우
		if (nWeaponIdx < 0 || !oPlayerController.AbilityValDicts[nWeaponIdx].ContainsKey(EEquipEffectType.ATTACK_POWER_UP_BY_DISTANCE))
		{
			return 0.0f;
		}

		var stDelta = a_oTarget.transform.position - a_oController.Params.m_stFirePos;
		float fATKRatio = oPlayerController.AbilityValDicts[nWeaponIdx][EEquipEffectType.ATTACK_POWER_UP_BY_DISTANCE];

		float fRange = oPlayerController.AbilityValDicts[nWeaponIdx].GetValueOrDefault(EEquipEffectType.AttackRange) * 
			ComType.G_UNIT_MM_TO_M;

		float fPercent = Mathf.Min(1.0f, stDelta.magnitude / fRange);
		return a_oController.Params.m_fATK * (fATKRatio * fPercent);
	}

	/** 투척 추가 피격 공격력을 반환한다 */
	private float GetExtraHitATKThrow(ProjectileController a_oController, UnitController a_oTarget)
	{
		var oPlayerController = a_oController.Params.m_oOwner as PlayerController;
		int nWeaponIdx = oPlayerController.GetWeaponIdx(a_oController.Params.m_oWeapon);

		// 효과 처리가 불가능 할 경우
		if (nWeaponIdx < 0 || !oPlayerController.AbilityValDicts[nWeaponIdx].ContainsKey(EEquipEffectType.ATTACK_POWER_UP_BY_EXPLOSION))
		{
			return 0.0f;
		}

		float fVal = oPlayerController.AbilityValDicts[nWeaponIdx][EEquipEffectType.ATTACK_POWER_UP_BY_EXPLOSION];
		return a_oController.Params.m_fATK * (fVal * (a_oController.NumHitTargets - 1));
	}
	#endregion // 접근 함수
}
