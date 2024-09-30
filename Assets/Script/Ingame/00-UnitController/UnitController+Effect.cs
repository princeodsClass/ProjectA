using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 유닛 제어자 - 효과 */
public abstract partial class UnitController : MonoBehaviour, IUpdatable
{
	#region 함수
	/** 효과 이펙트 프리팹을 반환한다 */
	public GameObject GetEffectFXPrefab(EEquipEffectType a_eEffectType)
	{
		switch (a_eEffectType)
		{
			// 버프
			case EEquipEffectType.ATTACK_POWER_UP: return this.BattleController.FXModelInfo.BuffFXInfo.m_oAttackPowerBuffFX;
			case EEquipEffectType.MOVE_SPEED_UP: return this.BattleController.FXModelInfo.BuffFXInfo.m_oMoveSpeedBuffFX;

			// 디버프
			case EEquipEffectType.FREEZE: return this.BattleController.FXModelInfo.IceFXInfo.m_oIceFX;
			case EEquipEffectType.BLEEDING: return this.BattleController.FXModelInfo.DebuffFXInfo.m_oBleedingDebuffFX;
			case EEquipEffectType.MOVE_SPEED_DOWN: return this.BattleController.FXModelInfo.DebuffFXInfo.m_oMoveSpeedDebuffFX;
		}

		return null;
	}

	/** 효과 이펙트를 재생한다 */
	public GameObject PlayEffectFX(EEquipEffectType a_eEffectType, bool a_bIsChildFX = true)
	{
		// 효과 이펙트가 없을 경우
		if (!this.EffectFXDict.TryGetValue(a_eEffectType, out GameObject oEffectFX))
		{
			oEffectFX = GameResourceManager.Singleton.CreateObject(this.GetEffectFXPrefab(a_eEffectType),
				a_bIsChildFX ? this.transform : this.BattleController.PathObjRoot, null);

			this.EffectFXDict.TryAdd(a_eEffectType, oEffectFX);
		}

		// 비활성 상태 일 경우
		if (!oEffectFX.activeSelf)
		{
			oEffectFX.GetComponentInChildren<TrailRenderer>()?.Clear();
		}

		// 자식 효과 이펙트 일 경우
		if(a_bIsChildFX)
		{
			oEffectFX.transform.localPosition = Vector3.zero;
		}
		else
		{
			oEffectFX.transform.position = this.transform.position;
		}

		oEffectFX.SetActive(true);
		return oEffectFX;
	}

	/** 액티브 효과를 추가한다 */
	public void AddActiveEffects(List<EffectTable> a_oEffectTableList)
	{
		for (int i = 0; i < a_oEffectTableList.Count; ++i)
		{
			ComUtil.AddEffect(a_oEffectTableList[i], this.ActiveEffectStackInfoList);
		}
	}

	/** 패시브 효과를 설정한다 */
	public void AddPassiveEffects(List<EffectTable> a_oEffectTableList, bool a_bIsIgnoreOperationType = true)
	{
		for (int i = 0; i < a_oEffectTableList.Count; ++i)
		{
			ComUtil.AddEffect(a_oEffectTableList[i], this.PassiveEffectStackInfoList, a_bIsIgnoreOperationType);
		}
	}

	/** 효과를 제거한다 */
	public void RemoveEffect(EEquipEffectType a_eEffectType, List<STEffectStackInfo> a_oOutEffectStackInfo)
	{
		int nResult = a_oOutEffectStackInfo.FindIndex((a_stStackInfo) => a_stStackInfo.m_eEffectType == a_eEffectType);

		// 효과 제거가 불가능 할 경우
		if (!a_oOutEffectStackInfo.ExIsValidIdx(nResult))
		{
			return;
		}

		a_oOutEffectStackInfo.ExRemoveValAt(nResult);
		this.OnRemoveFX(a_eEffectType);
	}

	/** 이펙트 스택 정보 상태를 갱신한다 */
	private void UpdateEffectStackInfoState()
	{
		this.RemoveEffectStackInfoList.Clear();

		for (int i = 0; i < this.EffectStackInfoList.Count; ++i)
		{
			// 영구 지속 효과 일 경우
			if (this.EffectStackInfoList[i].m_fDuration.ExIsLessEquals(0.0f))
			{
				continue;
			}

			var stStackInfo = this.EffectStackInfoList[i];
			stStackInfo.m_fRemainTime -= Time.deltaTime;

			switch (stStackInfo.m_eEffectType)
			{
				case EEquipEffectType.BLEEDING: this.HandleBleedingState(ref stStackInfo); break;
			}

			this.EffectStackInfoList[i] = stStackInfo;

			// 지속 시간이 남았을 경우
			if (this.IsSurvive && stStackInfo.m_fRemainTime.ExIsGreat(0.0f))
			{
				continue;
			}

			this.RemoveEffectStackInfoList.Add(stStackInfo);

			// 빙결 효과가 아닐 경우
			if (stStackInfo.m_eEffectType != EEquipEffectType.FREEZE)
			{
				this.EffectFXDict.GetValueOrDefault(stStackInfo.m_eEffectType)?.SetActive(false);
			}
		}

		// 제거 될 효과 스택 정보가 없을 경우
		if (!this.RemoveEffectStackInfoList.ExIsValid())
		{
			return;
		}

		for (int i = 0; i < this.RemoveEffectStackInfoList.Count; ++i)
		{
			int nResult = this.ActiveEffectStackInfoList.FindIndex((a_stStackInfo) =>
			{
				bool bIsEquals01 = this.RemoveEffectStackInfoList[i].m_fVal.ExIsEquals(a_stStackInfo.m_fVal);
				bool bIsEquals02 = this.RemoveEffectStackInfoList[i].m_fDuration.ExIsEquals(a_stStackInfo.m_fDuration);

				return bIsEquals01 && bIsEquals02 && this.RemoveEffectStackInfoList[i].m_eEffectType == a_stStackInfo.m_eEffectType;
			});

			// 제거 될 효과 스택 정보가 없을 경우
			if (!this.ActiveEffectStackInfoList.ExIsValidIdx(nResult))
			{
				continue;
			}

			this.OnRemoveFX(this.ActiveEffectStackInfoList[nResult].m_eEffectType);
			this.ActiveEffectStackInfoList.ExRemoveValAt(nResult);
		}

		this.SetupAbilityValues(true);
	}

	/** 효과가 제거 되었을 경우 */
	public void OnRemoveFX(EEquipEffectType a_eType)
	{
		switch (a_eType)
		{
			case EEquipEffectType.FREEZE:
				this.Animator.SetBool(ComType.G_PARAMS_IS_FREEZE, false);
				break;
		}
	}

	/** 출혈 상태를 처리한다 */
	private void HandleBleedingState(ref STEffectStackInfo a_stStackInfo)
	{
		float fDeltaTime = a_stStackInfo.m_fDuration - a_stStackInfo.m_fRemainTime;
		float fApplyTime = a_stStackInfo.m_nApplyTimes * a_stStackInfo.m_fInterval;

		// 효과 발동 대기 시간이 남았을 경우
		if (fDeltaTime.ExIsLess(fApplyTime))
		{
			return;
		}

		a_stStackInfo.m_nApplyTimes += 1;

		this.HandleOnHit(null, Vector3.zero, new STHitInfo()
		{
			m_nDamage = Mathf.Abs(Mathf.FloorToInt(a_stStackInfo.m_fVal)),
			m_eHitType = EHitType.NORM
		});
	}
	#endregion // 함수
}
