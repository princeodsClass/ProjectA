using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 유닛 제어자 - 능력치 */
public abstract partial class UnitController : MonoBehaviour, IUpdatable
{
	#region 프로퍼티
	public int HP { get; private set; } = 0;
	public virtual int MaxHP => this.GetMaxHP();
	public virtual int Accuracy => Mathf.FloorToInt(this.CurAbilityValDict.GetValueOrDefault(EEquipEffectType.ACCURACY));

	public virtual float AimingDelay => this.GetAimingDelay();
	public virtual float DefencePower => this.CurAbilityValDict.GetValueOrDefault(EEquipEffectType.DEF);
	public virtual float ExplosionRangeRatio => this.GetExplosionRangeRatio();

	public virtual float AttackRange => this.CurAbilityValDict.GetValueOrDefault(EEquipEffectType.AttackRange);
	public virtual float AttackPower => this.CurAbilityValDict.GetValueOrDefault(EEquipEffectType.ATK);
	public virtual float WarheadSpeed => this.CurAbilityValDict.GetValueOrDefault(EEquipEffectType.WARHEAD_SPEED);

	public virtual float RicochetChance => this.CurAbilityValDict.GetValueOrDefault(EEquipEffectType.RicochetChance);
	public virtual float DoubleShotChance => this.CurAbilityValDict.GetValueOrDefault(EEquipEffectType.DoubleShotChance);

	public virtual Dictionary<EEquipEffectType, float> CurAbilityValDict => null;
	public virtual Dictionary<EEquipEffectType, float> CurOriginAbilityValDict => null;
	public virtual Dictionary<EEquipEffectType, float> CurStandardAbilityValDict => null;
	#endregion // 프로퍼티

	#region 함수
	/** 어빌리티 값을 설정한다 */
	public virtual void SetupAbilityValues(bool a_bIsSetupEffectStackInfos = false)
	{
		// 효과 스택 정보 리셋 모드 일 경우
		if (a_bIsSetupEffectStackInfos)
		{
			this.SetupEffectStackInfos();
		}

		this.SetupAbilityValues(this.CurStandardAbilityValDict, this.EffectStackInfoList, this.CurAbilityValDict);
		this.SetupAbilityValues(this.CurStandardAbilityValDict, this.EffectStackInfoList, this.CurOriginAbilityValDict);
	}

	/** 어빌리티 값을 설정한다 */
	public virtual void SetupAbilityValues(Dictionary<EEquipEffectType, float> a_oStandardAbilityValDict,
		List<STEffectStackInfo> a_oEffectStackInfoList, Dictionary<EEquipEffectType, float> a_oOutAbilityValDict, bool a_bIsClear = true)
	{
		a_oStandardAbilityValDict.ExCopyTo(a_oOutAbilityValDict, (a_eKey, a_fVal) => a_fVal, a_bIsClear);

		for (int i = 0; i < a_oEffectStackInfoList.Count; ++i)
		{
			float fVal = a_oOutAbilityValDict.GetValueOrDefault(a_oEffectStackInfoList[i].m_eAbilityEffectType);
			float fExtraVal = a_oEffectStackInfoList[i].m_fVal * a_oEffectStackInfoList[i].m_nStackCount;

			float fIncrVal = a_oEffectStackInfoList[i].m_bIsIgnoreStandardAbility ?
				fExtraVal : this.GetIncrAbilityVal(a_oStandardAbilityValDict, a_oEffectStackInfoList[i].m_eAbilityEffectType, fExtraVal);

			a_oOutAbilityValDict.ExReplaceVal(a_oEffectStackInfoList[i].m_eAbilityEffectType, fVal + fIncrVal);
		}
	}
	#endregion // 함수

	#region 접근 함수
	/** 추가 피격 공격력을 반환한다 */
	public virtual float GetExtraHitATK(ProjectileController a_oController, UnitController a_oTarget)
	{
		return 0.0f;
	}

	/** 어빌리티 증가 값을 반환한다 */
	public float GetIncrAbilityVal(Dictionary<EEquipEffectType, float> a_oStandardAbilityValDict,
		EEquipEffectType a_eType, float a_fVal)
	{
		// 기준 값이 필요 할 경우
		if (ComType.StandardAbilityTypeDict.Contains(a_eType))
		{
			return a_oStandardAbilityValDict.GetValueOrDefault(a_eType) * a_fVal;
		}

		return a_fVal;
	}

	/** 공격력을 반환한다 */
	public float GetATK(float a_fATK, EWeaponType a_eType, float a_fExtraATK = 0.0f)
	{
		float fATKRatio = ComUtil.GetAbilityVal(EEquipEffectType.AttackPowerRatio, this.CurAbilityValDict);

		switch (a_eType)
		{
			case EWeaponType.Pistol:
				fATKRatio += ComUtil.GetAbilityVal(EEquipEffectType.AttackPowerPistol, this.CurAbilityValDict);
				break;

			case EWeaponType.AR: 
				fATKRatio += ComUtil.GetAbilityVal(EEquipEffectType.AttackPowerAR, this.CurAbilityValDict);
				break;

			case EWeaponType.SR:
				fATKRatio += ComUtil.GetAbilityVal(EEquipEffectType.AttackPowerSR, this.CurAbilityValDict);
				break;

			case EWeaponType.SG:
				fATKRatio += ComUtil.GetAbilityVal(EEquipEffectType.AttackPowerSG, this.CurAbilityValDict);
				break;

			case EWeaponType.Grenade:
				fATKRatio += ComUtil.GetAbilityVal(EEquipEffectType.AttackPowerGE, this.CurAbilityValDict);
				break;

			case EWeaponType.SMG:
				fATKRatio += ComUtil.GetAbilityVal(EEquipEffectType.AttackPowerSMG, this.CurAbilityValDict);
				break;
		}

		return (a_fATK + a_fExtraATK) * (1.0f + fATKRatio);
	}

	/** 방어력을 반환한다 */
	public float GetDefense(EDamageType a_eType, float a_fExtraDEFRatio = 0.0f)
	{
		float fDefence = this.DefencePower;
		fDefence += this.CurAbilityValDict.GetValueOrDefault(EEquipEffectType.DefencePowerRatio + ((int)EOperationType.ADD * (int)EEquipEffectType.OPERATION_TYPE_VAL));

		a_fExtraDEFRatio += this.CurAbilityValDict.GetValueOrDefault(EEquipEffectType.DefencePowerRatio);

		switch (a_eType)
		{
			case EDamageType.MELEE: 
				a_fExtraDEFRatio += ComUtil.GetAbilityVal(EEquipEffectType.DefencePowerMelee, this.CurAbilityValDict);
				break;

			case EDamageType.RANGE:
				a_fExtraDEFRatio += ComUtil.GetAbilityVal(EEquipEffectType.DefencePowerRange, this.CurAbilityValDict);
				break;

			case EDamageType.EXPLOSION:
				a_fExtraDEFRatio += ComUtil.GetAbilityVal(EEquipEffectType.DefencePowerExplosion, this.CurAbilityValDict);
				break;
		}

		return fDefence * (1.0f + a_fExtraDEFRatio);
	}

	/** 체력을 변경한다 */
	public void SetHP(int a_nHP)
	{
		this.HP = Mathf.Min(a_nHP, this.MaxHP);
	}

	/** 최대 체력을 반환한다 */
	private int GetMaxHP()
	{
		float fHP = this.CurOriginAbilityValDict.GetValueOrDefault(EEquipEffectType.HP);
		return Mathf.FloorToInt(fHP * (1.0f + ComUtil.GetAbilityVal(EEquipEffectType.MaxHPRatio, this.CurOriginAbilityValDict)));
	}

	/** 조준 딜레이를 반환한다 */
	private float GetAimingDelay()
	{
		float fAimingDelay = this.CurAbilityValDict.GetValueOrDefault(EEquipEffectType.AIMING_DELAY);
		return fAimingDelay * (1.0f + ComUtil.GetAbilityVal(EEquipEffectType.AimTime, this.CurAbilityValDict));
	}

	/** 폭발 범위를 반환한다 */
	private float GetExplosionRangeRatio()
	{
		return this.CurAbilityValDict.GetValueOrDefault(EEquipEffectType.ExplosionRangeRatio);
	}
	#endregion // 접근 함수
}
