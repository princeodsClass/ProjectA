using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 유닛 제어자 - 피격 */
public abstract partial class UnitController : MonoBehaviour, IUpdatable
{
	#region 함수
	/** 피격 되었을 경우 */
	public virtual void OnHit(UnitController a_oAttacker)
	{
		// 생존 상태가 아닐 경우
		if (!this.IsSurvive)
		{
			return;
		}

		var stDirection = this.transform.position - a_oAttacker.transform.position;
		stDirection.y = 0.0f;

		this.HandleOnHit(a_oAttacker, stDirection.normalized, this.CalcDamage(a_oAttacker));
	}

	/** 피격 되었을 경우 */
	public virtual void OnHit(UnitController a_oAttacker, ProjectileController a_oController)
	{
		// 생존 상태가 아닐 경우
		if (!this.IsSurvive)
		{
			return;
		}

		var stDirection = (a_oController.Params.m_eAttackType == EAttackType.SHOOT) ?
			a_oController.transform.forward : this.transform.position - a_oController.transform.position;

		stDirection.y = 0.0f;
		this.HandleOnHit(a_oAttacker, stDirection.normalized, this.CalcDamage(a_oController));
	}

	/** 피격 되었을 경우 */
	public virtual void OnHit(UnitController a_oAttacker, SkillController a_oController)
	{
		// 생존 상태가 아닐 경우
		if (!this.IsSurvive)
		{
			return;
		}

		var stDirection = this.transform.position - a_oController.transform.position;
		stDirection.y = 0.0f;

		this.HandleOnHit(a_oAttacker, stDirection.normalized, this.CalcDamage(a_oController));
	}

	/** 피격 되었을 경우 */
	public virtual void OnHit(UnitController a_oAttacker, DamageFieldController a_oController)
	{
		// 생존 상태가 아닐 경우
		if (!this.IsSurvive)
		{
			return;
		}

		var stDirection = this.transform.position - a_oController.transform.position;
		stDirection.y = 0.0f;

		this.HandleOnHit(a_oAttacker, stDirection.normalized, this.CalcDamage(a_oController));
	}

	/** 데미지를 계산한다 */
	protected virtual STHitInfo CalcDamage(UnitController a_oAttacker)
	{
		float fATK = a_oAttacker.GetATK(a_oAttacker.AttackPower, EWeaponType.Common);
		float fDefense = this.GetDefense(EDamageType.MELEE);

		int nDamage = this.CalcDamage(fATK, fDefense);

		return new STHitInfo()
		{
			m_nDamage = nDamage,
			m_eHitType = EHitType.NORM,
			m_nOriginDamage = nDamage,

			m_fRagdollMinForce = ComUtil.GetAbilityVal(EEquipEffectType.FORCE_MIN, a_oAttacker.CurAbilityValDict),
			m_fRagdollMaxForce = ComUtil.GetAbilityVal(EEquipEffectType.FORCE_MIN, a_oAttacker.CurAbilityValDict)
		};
	}

	/** 데미지를 계산한다 */
	protected virtual STHitInfo CalcDamage(ProjectileController a_oAttacker)
	{
		float fExtraATK = (a_oAttacker.Params.m_oOwner != null) ?
			a_oAttacker.Params.m_oOwner.GetExtraHitATK(a_oAttacker, this) : 0.0f;

		float fATK = a_oAttacker.Params.m_oOwner.GetATK(a_oAttacker.Params.m_fATK, a_oAttacker.Params.m_eWeaponType, fExtraATK);
		float fDefense = this.GetDefense(a_oAttacker.Params.m_eDamageType);

		int nDamage = this.CalcDamage(fATK, fDefense);

		return new STHitInfo()
		{
			m_nDamage = nDamage,
			m_eHitType = EHitType.NORM,
			m_nOriginDamage = nDamage,
			m_eAtackWeaponType = a_oAttacker.Params.m_eWeaponType,

			m_fRagdollMinForce = ComUtil.GetAbilityVal(EEquipEffectType.FORCE_MIN, a_oAttacker.Params.m_oAbilityValDict),
			m_fRagdollMaxForce = ComUtil.GetAbilityVal(EEquipEffectType.FORCE_MAX, a_oAttacker.Params.m_oAbilityValDict)
		};
	}

	/** 스킬 데미지를 계산한다 */
	protected virtual STHitInfo CalcDamage(SkillController a_oAttacker)
	{
		float fATK = a_oAttacker.Params.m_oOwner.GetATK(a_oAttacker.Params.m_fATK, EWeaponType.Common);
		float fDefense = this.GetDefense((EDamageType)a_oAttacker.Params.m_oTable.DamageType);

		int nDamage = this.CalcDamage(fATK, fDefense);

		return new STHitInfo()
		{
			m_bIsSkill = true,
			m_nDamage = nDamage,
			m_eHitType = EHitType.NORM,
			m_nOriginDamage = nDamage
		};
	}

	/** 데미지를 계산한다 */
	protected virtual STHitInfo CalcDamage(DamageFieldController a_oAttacker)
	{
		float fATK = a_oAttacker.Params.m_oOwner.GetATK(a_oAttacker.Params.m_fATK, a_oAttacker.Params.m_eWeaponType);
		float fDefense = this.GetDefense(a_oAttacker.Params.m_eDamageType);

		int nDamage = this.CalcDamage(fATK, fDefense);

		return new STHitInfo()
		{
			m_nDamage = nDamage,
			m_eHitType = EHitType.NORM,
			m_eAtackWeaponType = a_oAttacker.Params.m_eWeaponType,
			m_nOriginDamage = nDamage
		};
	}

	/** 피격을 처리한다 */
	protected virtual void HandleOnHit(UnitController a_oAttacker,
		Vector3 a_stDirection, STHitInfo a_stHitInfo, bool a_bIsShowDamage = true)
	{
		// 무적 상태 일 경우
		if (this.IsUntouchable || a_stHitInfo.m_nDamage <= 0)
		{
			return;
		}

		m_bIsDirtyUpdateUIsState = true;
		this.SetHP(Mathf.Max(0, this.HP - a_stHitInfo.m_nDamage));

		this.StartHitDirecting(a_stHitInfo, a_bIsShowDamage);

		// 공격자가 존재 할 경우
		if (a_oAttacker != null && this.HP > 0)
		{
			float fBleedingRatio = ComUtil.GetAbilityVal(EEquipEffectType.BleedingAfterAttack, a_oAttacker.CurAbilityValDict);
			float fMoveSpeedRatio = ComUtil.GetAbilityVal(EEquipEffectType.MoveSpeedRatioAfterAttack, a_oAttacker.CurAbilityValDict);

			// 출혈 효과 발동이 가능 할 경우
			if (!fBleedingRatio.ExIsEquals(0.0f))
			{
				float fMaxBleedingDamageRatio = GlobalTable.GetData<float>(ComType.G_RATIO_MAX_BLEEDING);
				float fMaxDamage = Mathf.Floor(a_stHitInfo.m_nOriginDamage * fMaxBleedingDamageRatio);
				float fDamage = Mathf.Floor(Mathf.Abs(this.MaxHP * fBleedingRatio));

				ComUtil.AddEffect(EEquipEffectType.BLEEDING,
					EEquipEffectType.BLEEDING, Mathf.Min(fDamage, fMaxDamage), GameManager.Singleton.user.m_nMsBleedingAfterAttackDuration * ComType.G_UNIT_MS_TO_S, GameManager.Singleton.user.m_nMsBleedingAfterAttackInterval * ComType.G_UNIT_MS_TO_S, this.ActiveEffectStackInfoList, (int)GameManager.Singleton.user.m_nBleedingAfterAttackMaxStack);

				this.PlayEffectFX(EEquipEffectType.BLEEDING);
			}

			// 슬로우 효과 발동이 가능 할 경우
			if (!fMoveSpeedRatio.ExIsEquals(0.0f))
			{
				ComUtil.AddEffect(EEquipEffectType.MOVE_SPEED_DOWN,
					EEquipEffectType.MoveSpeed, fMoveSpeedRatio, GameManager.Singleton.user.m_nMsMoveSpeedRatioAfterAttackDuration * ComType.G_UNIT_MS_TO_S, 0.0f, this.ActiveEffectStackInfoList, (int)GameManager.Singleton.user.m_nMoveSpeedRatioAfterAttackMaxStack);

				this.PlayEffectFX(EEquipEffectType.MOVE_SPEED_DOWN);
			}

			// 능력치 설정이 필요 할 경우
			if (!fBleedingRatio.ExIsEquals(0.0f) || !fMoveSpeedRatio.ExIsEquals(0.0f))
			{
				this.SetupAbilityValues(true);
			}
		}

		bool bIsFreeze = false;

		// 빙결 상태 일 경우
		if (this.CurAbilityValDict.ContainsKey(EEquipEffectType.FREEZE))
		{
			this.PlayStatusFX(EEquipEffectType.FREEZE);
			this.RemoveEffect(EEquipEffectType.FREEZE, this.ActiveEffectStackInfoList);

			bIsFreeze = true;
			this.SetupAbilityValues(true);
		}

		this.UpdateUIsState();

		// 체력이 존재 할 경우
		if (this.HP > 0)
		{
			return;
		}

		// 빙결 상태 일 경우
		if (bIsFreeze)
		{
			this.gameObject.SetActive(false);
		}

		this.HandleDieState(a_oAttacker, a_stDirection, a_stHitInfo);
	}

	/** 대화 여부를 처리한다 */
	public void TryHandleTalk(uint a_nHitGroup)
	{
		string oHintGroupKey = string.Format(ComType.G_KEY_FMT_HINT_GROUP, a_nHitGroup);

		// 힌트 출력이 필요 없을 경우
		if (a_nHitGroup <= 0 || PlayerPrefs.GetInt(oHintGroupKey) > 0)
		{
			return;
		}

		var oHintGroupTableList = NPCHintGroupTable.GetGroup((int)a_nHitGroup);

		// 힌트 그룹이 없을 경우
		if (!oHintGroupTableList.ExIsValid())
		{
			return;
		}

		// 반복 힌트가 아닐 경우
		if (!oHintGroupTableList[0].isRepeat)
		{
			PlayerPrefs.SetInt(oHintGroupKey, 1);
			PlayerPrefs.Save();
		}

		this.BattleController.AddHintInfo(this, a_nHitGroup);
	}
	#endregion // 함수
}
