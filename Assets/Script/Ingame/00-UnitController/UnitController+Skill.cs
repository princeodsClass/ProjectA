using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/** 유닛 제어자 - 스킬 */
public abstract partial class UnitController : MonoBehaviour, IUpdatable
{
	#region 함수
	/** 스킬을 적용한다 */
	public virtual void ApplySkill(SkillTable a_oSkillTable, bool a_bIsContinuing)
	{
		// Do Something
	}

	/** 스킬을 적용한다 */
	public virtual void ApplySkill(SkillTable a_oSkillTable, Vector3 a_stTargetPos, UnitController a_oTarget, bool a_bIsContinuing)
	{
		var stParams = SkillController.MakeParams(this.AttackPower * a_oSkillTable.ratioAttackPower,
			EWeaponType.Common, a_stTargetPos, a_oSkillTable, this, a_oTarget, this.HandleOnApplySkill, this.OnCompleteApplySkill, a_bIsContinuing);

		var oSkillController = GameResourceManager.Singleton.CreateObject<SkillController>(EResourceType.Etc,
			"Skill", this.transform, ComType.G_LIFE_T_SKILL);

		oSkillController.Init(stParams);
	}

	/** 스킬 그룹을 적용한다 */
	public virtual void ApplySkillGroup(int a_nGroup, bool a_bIsContinuing, bool a_bIsIgnoreNextSelectionSkill = false)
	{
		var oGroupTableList = SkillGroupTable.GetGroup(a_nGroup);
		var oAlwaysGroupTable = CCollectionPoolManager.Singleton.SpawnList<SkillGroupTable>();

		try
		{
			for (int i = 0; i < oGroupTableList.Count; ++i)
			{
				// 선택 발동 스킬 일 경우
				if (oGroupTableList[i].SelectionType < 1)
				{
					continue;
				}

				oAlwaysGroupTable.Add(oGroupTableList[i]);
			}

			for (int i = 0; i < oAlwaysGroupTable.Count; ++i)
			{
				this.ApplySkillGroup(oAlwaysGroupTable[i], a_bIsContinuing);
			}
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oAlwaysGroupTable);
		}
	}


	/** 스킬 발사 이벤트를 수신했을 경우 */
	public virtual void FireSkill(Object a_oParams)
	{
		int nSkillType = this.Animator.GetInteger(ComType.G_PARAMS_SKILL_TYPE);

		// 스킬 타입이 유효하지 않을 경우
		if (nSkillType <= (int)ESkillType.EMPTY)
		{
			return;
		}

		var oSkillTable = SkillTable.GetData((uint)nSkillType);

		switch ((ESkillType)oSkillTable.SkillType)
		{
			case ESkillType.GRENADE:
			case ESkillType.GRENADE_DELAY: this.HandleFireSkillGrenade(oSkillTable, (ESkillType)oSkillTable.SkillType == ESkillType.GRENADE); break;

			case ESkillType.FAN_SHOT: this.HandleFireSkillFanShot(oSkillTable); break;
			case ESkillType.GRENADE_SPLIT: this.HandleFireSkillSplitGrenade(oSkillTable); break;
			case ESkillType.BOMBING_REQUEST: this.HandleFireSkillBombingRequest(oSkillTable); break;
		}
	}

	/** 적용 스킬 정보를 설정한다 */
	protected virtual void SetupApplySkillInfo(SkillTable a_oSkillTable, bool a_bIsContinuing)
	{
		this.Animator.SetBool(ComType.G_PARAMS_IS_CONTINUING_SKILL, this.IsContinuingSkill(a_oSkillTable));
		this.Animator.SetInteger(ComType.G_PARAMS_SKILL_TYPE, (int)a_oSkillTable.PrimaryKey);
		this.Animator.SetFloat(ComType.G_PARAMS_SKILL_ANI_TYPE, a_oSkillTable.SkillAniType);

		this.Animator.SetTrigger(this.IsCastSkill(a_oSkillTable) ? ComType.G_PARAMS_SKILL_CAST : ComType.G_PARAMS_SKILL_FIRE);
	}

	/** 스킬 적용이 완료 되었을 경우 */
	protected virtual void OnCompleteApplySkill(SkillController a_oSender)
	{
		// Do Something
	}

	/** 스킬 그룹을 적용한다 */
	protected virtual void ApplySkillGroup(SkillGroupTable a_oGroupTable, bool a_bIsContinuing)
	{
		this.ApplySkill(SkillTable.GetData(a_oGroupTable.Skill), a_bIsContinuing);
	}

	/** 스킬 적용을 처리한다 */
	protected virtual void HandleOnApplySkill(SkillController a_oSender, Collider a_oCollider)
	{
#if DISABLE_THIS
		// 유닛이 아닐 경우
		if (a_oCollider.CompareTag(m_oOriginTag) || !this.IsUnit(a_oCollider.gameObject))
		{
			return;
		}
#else
		// 유닛이 아닐 경우
		if (!this.IsUnit(a_oCollider.gameObject) || a_oCollider.gameObject.layer == this.gameObject.layer)
		{
			return;
		}
#endif // #if DISABLE_THIS

		// 유닛 제어자가 없을 경우
		if (!a_oCollider.gameObject.TryGetComponent(out UnitController oController))
		{
			return;
		}

		oController.OnHit(this, a_oSender);
	}

	/** 유탄 스킬 발사를 처리한다 */
	protected virtual void HandleFireSkillGrenade(SkillTable a_oSkillTable, bool a_bIsEnableWarning)
	{
		// 스킬 발사체 정보가 없을 경우
		if (this.CurSkillModelInfo == null)
		{
			return;
		}

		this.ThrowSkillProjectile(this.CurSkillModelInfo.GrenadeProjectileInfo, a_oSkillTable, a_bIsEnableWarning);
	}

	/** 부채꼴 연사 스킬 발사를 처리한다 */
	protected virtual void HandleFireSkillFanShot(SkillTable a_oSkillTable)
	{
		// 스킬 발사체 정보가 없을 경우
		if (this.CurSkillModelInfo == null)
		{
			return;
		}

		this.ShootSkillProjectile(this.CurSkillModelInfo.FanShotProjectileInfo, a_oSkillTable);
		this.BattleController.PlaySound(this.CurSoundModelInfo?.FireSoundList, this.gameObject);
	}

	/** 분할 유탄 스킬 발사를 처리한다 */
	protected virtual void HandleFireSkillSplitGrenade(SkillTable a_oSkillTable)
	{
		// 스킬 발사체 정보가 없을 경우
		if (this.CurSkillModelInfo == null)
		{
			return;
		}

		this.ThrowSkillProjectile(this.CurSkillModelInfo.SplitGrenadeProjectileInfo, a_oSkillTable, true);
	}

	/** 폭격 요청 스킬 발사를 처리한다 */
	protected virtual void HandleFireSkillBombingRequest(SkillTable a_oSkillTable)
	{
		// 스킬 발사체 정보가 없을 경우
		if (this.CurSkillModelInfo == null)
		{
			return;
		}

		this.DropSkillProjectile(this.CurSkillModelInfo.BombingRequestProjectileInfo, a_oSkillTable);
	}

	/** 스킬 발사체를 발사한다 */
	private void ShootSkillProjectile(SkillModelInfo.STProjectileInfo a_stProjectileInfo, SkillTable a_oSkillTable)
	{
		// 발사체가 없을 경우
		if (a_stProjectileInfo.m_oWarhead == null || a_stProjectileInfo.m_oWarheadDummy == null)
		{
			return;
		}

		var oProjectileController = this.CreateProjectile(a_stProjectileInfo.m_oWarhead,
			a_stProjectileInfo.m_oWarheadDummy, this.BattleController.PathObjRoot.gameObject, EAttackType.SHOOT, (EDamageType)a_oSkillTable.DamageType, EWeaponType.Common, this.AttackPower * a_oSkillTable.ratioAttackPower);

		oProjectileController.Shoot(a_stProjectileInfo.m_oWarheadDummy.forward * this.WarheadSpeed, this.HandleOnTriggerEnterProjectile);
		this.HandleSkillProjectileFX(a_stProjectileInfo);
	}

	/** 스킬 발사체를 던진다 */
	private void ThrowSkillProjectile(SkillModelInfo.STProjectileInfo a_stProjectileInfo, SkillTable a_oSkillTable, bool a_bIsEnableWarning)
	{
		// 발사체가 없을 경우
		if (a_stProjectileInfo.m_oWarhead == null || a_stProjectileInfo.m_oWarheadDummy == null)
		{
			return;
		}

		var oFXTableList = CCollectionPoolManager.Singleton.SpawnList<EffectTable>();

		try
		{
			// 효과 그룹 키가 존재 할 경우
			if (a_oSkillTable.HitEffectGroup > 0)
			{
				EffectTable.GetGroup(a_oSkillTable.HitEffectGroup, oFXTableList);
			}

			var oTarget = this.LockOnTarget ?? this.TrackingTarget;

			var stVelocity = ComUtil.GetParabolaVelocity(a_stProjectileInfo.m_oWarheadDummy.position,
				oTarget.transform.position, Physics.gravity, GlobalTable.GetData<float>(ComType.G_DEGREE_FOR_FIRE_GRENADE));

			var oProjectile = this.CreateProjectile(a_stProjectileInfo.m_oWarhead,
				a_stProjectileInfo.m_oWarheadDummy, this.BattleController.PathObjRoot.gameObject, EAttackType.THROW, (EDamageType)a_oSkillTable.DamageType, EWeaponType.Common, this.AttackPower * a_oSkillTable.ratioAttackPower);

			oProjectile.Throw(oTarget.transform.position,
				stVelocity, oFXTableList, this.ExplosionRangeRatio, this.HandleOnTriggerEnterProjectile, a_bIsEnableWarning, false);

			this.HandleSkillProjectileFX(a_stProjectileInfo);
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oFXTableList);
		}
	}

	/** 스킬 발사체를 떨어뜨린다 */
	private void DropSkillProjectile(SkillModelInfo.STProjectileInfo a_stProjectileInfo, SkillTable a_oSkillTable)
	{
		// 발사체가 없을 경우
		if (a_stProjectileInfo.m_oWarhead == null)
		{
			return;
		}

		var oFXTableList = CCollectionPoolManager.Singleton.SpawnList<EffectTable>();

		try
		{
			// 효과 그룹 키가 존재 할 경우
			if (a_oSkillTable.HitEffectGroup > 0)
			{
				EffectTable.GetGroup(a_oSkillTable.HitEffectGroup, oFXTableList);
			}

			for (int i = 0; i < oFXTableList.Count; ++i)
			{
				var oTarget = this.LockOnTarget ?? this.TrackingTarget;
				float fRange = this.AttackRange * ComType.G_UNIT_MM_TO_M;

				var stTargetPos = oTarget.transform.position;
				stTargetPos = stTargetPos + new Vector3(Random.Range(-fRange, fRange), 0.0f, Random.Range(-fRange, fRange));

				// 내비게이션 영역 일 경우
				if (NavMesh.SamplePosition(stTargetPos + (Vector3.up * 1.5f), out NavMeshHit stNavMeshHit, float.MaxValue / 2.0f, m_nWalkableAreaMask))
				{
					stTargetPos = stNavMeshHit.position;
				}

				var oProjectile = this.CreateProjectile(a_stProjectileInfo.m_oWarhead,
					a_stProjectileInfo.m_oWarheadDummy, this.BattleController.PathObjRoot.gameObject, EAttackType.DROP, (EDamageType)a_oSkillTable.DamageType, EWeaponType.Common, this.AttackPower * a_oSkillTable.ratioAttackPower);

				oProjectile.transform.position = stTargetPos + (Vector3.up * 50.0f);
				oProjectile.Drop(stTargetPos, oFXTableList[i], 2.5f, this.ExplosionRangeRatio, this.HandleOnTriggerEnterProjectile, true, 0.5f);
			}
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oFXTableList);
		}
	}

	/** 스킬 발사체 효과를 처리한다 */
	private void HandleSkillProjectileFX(SkillModelInfo.STProjectileInfo a_stProjectileInfo)
	{
		this.FireShell(a_stProjectileInfo.m_oShell, a_stProjectileInfo.m_oShellDummy);
		this.FireMuzzleFlash(a_stProjectileInfo.m_oMuzzleFlash, a_stProjectileInfo.m_oMuzzleFlashDummy);
	}
	#endregion // 함수

	#region 접근 함수
	/** 캐스팅 스킬 여부를 검사한다 */
	public bool IsCastSkill(SkillTable a_oSkillTable)
	{
		return a_oSkillTable.CastingTime >= 1;
	}

	/** 지속 스킬 여부를 검사한다 */
	public bool IsContinuingSkill(SkillTable a_oSkillTable)
	{
		return (ESkillType)a_oSkillTable.SkillType == ESkillType.JUMP_ATTACK;
	}
	#endregion // 접근 함수

	#region 팩토리 함수
	/** 발사체를 생성한다 */
	protected virtual ProjectileController CreateProjectile(GameObject a_oPrefab,
		Transform a_oDummy, GameObject a_oParent, EAttackType a_eAttackType, EDamageType a_eDamageType, EWeaponType a_eWeaponType, float a_fATK)
	{
		var oWarhead = GameResourceManager.Singleton.CreateObject(a_oPrefab,
			a_oParent.transform, a_oDummy, ComType.G_LIFE_T_PROJECTILE);

		var stParams = ProjectileController.MakeParams(this,
			this.CurAbilityValDict, a_eAttackType, a_eDamageType, a_eWeaponType, a_fATK, this.transform.position);

		var oProjectileController = oWarhead.GetComponent<ProjectileController>();
		oProjectileController.Init(stParams);

		return oProjectileController;
	}
	#endregion // 팩토리 함수
}
