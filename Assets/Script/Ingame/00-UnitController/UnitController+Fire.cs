using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 유닛 제어자 - 발사 */
public abstract partial class UnitController : MonoBehaviour, IUpdatable
{
	#region 함수
	/** 발사 이벤트를 수신했을 경우 */
	public virtual void Fire(Object a_oParams)
	{
		bool bIsSkillState = this.StateMachine.State is CStateUnitBattleSkill;
		bool bIsLockOnState = this.StateMachine.State is CStateUnitBattleLockOn;

		// 스킬 상태 일 경우
		if (bIsSkillState)
		{
			this.FireSkill(a_oParams);
		}
		// 조준 상태 일 경우
		else if (bIsLockOnState)
		{
			this.FireWeapon(a_oParams);
		}
	}

	/** 무기 발사 이벤트를 수신했을 경우 */
	public virtual void FireWeapon(Object a_oParams)
	{
		var oLockOnTarget = this.LockOnTarget ?? this.TrackingTarget;
		bool bIsBattleState = this.StateMachine.State is CStateUnitBattleLockOn;

		// 발사가 불가능 할 경우
		if (!bIsBattleState || oLockOnTarget == null)
		{
			return;
		}

		this.SetIsFire(false);

		switch (this.WeaponAniType)
		{
			case EWeaponAnimationType.Common: this.HandleFireCommon(oLockOnTarget); break;
			default: this.HandleFireProjectile(oLockOnTarget); break;
		}
	}

	/** 탄피를 발사한다 */
	public virtual void FireShell(GameObject a_oPrefab, Transform a_oDummy)
	{
		// 탄피가 없을 경우
		if (a_oPrefab == null || a_oDummy == null)
		{
			return;
		}

		var oShell = GameResourceManager.Singleton.CreateObject(a_oPrefab,
			this.BattleController.PathObjRoot, a_oDummy, ComType.G_LIFE_T_SHELL);

		oShell.transform.position = a_oDummy.position;

		var oRigidbody = oShell.GetComponentInChildren<Rigidbody>();
		oRigidbody.velocity = Vector3.zero;

		oRigidbody.AddForce(this.GetShellDisposeDirection() * Random.Range(ComType.G_MIN_FORCE_SHELL_DISPOSE,
			ComType.G_MAX_FORCE_SHELL_DISPOSE), ForceMode.VelocityChange);
	}

	/** 탄두를 발사한다 */
	public virtual void FireWarhead(GameObject a_oPrefab, Transform a_oDummy, UnitController a_oTarget, int a_nFireFXGroup)
	{
		// 탄두가 없을 경우
		if (a_oPrefab == null || a_oDummy == null)
		{
			return;
		}

		var eWeaponType = (this.CurWeapon != null) ? this.CurWeapon.eSubType : EWeaponType.Common;

		for (int i = 0; i < this.NumFireProjectilesAtOnce; ++i)
		{
			var oWarhead = GameResourceManager.Singleton.CreateObject(a_oPrefab,
				this.BattleController.PathObjRoot, a_oDummy, ComType.G_LIFE_T_PROJECTILE);

			var stParams = ProjectileController.MakeParams(this,
				this.CurAbilityValDict, this.AttackType, this.DamageType, eWeaponType, this.AttackPower, this.transform.position, this.AttackPower, this.CurWeapon);

			var oProjectileController = oWarhead.GetComponent<ProjectileController>();
			oProjectileController.Init(stParams);

			switch (this.AttackType)
			{
				case EAttackType.SHOOT: this.FireWarheadShoot(oProjectileController, a_oTarget, a_oDummy, i >= this.NumFireProjectilesAtOnce - 1); break;
				case EAttackType.THROW: this.FireWarheadThrow(oProjectileController, a_oTarget, a_oDummy, a_nFireFXGroup); break;
			}
		}
	}

	/** 화염 효과를 발사한다 */
	public virtual void FireMuzzleFlash(GameObject a_oPrefab, Transform a_oDummy)
	{
		// 화염 효과가 없을 경우
		if (a_oPrefab == null || a_oDummy == null)
		{
			return;
		}

		var oMuzzleFlash = GameResourceManager.Singleton.CreateObject(a_oPrefab,
			this.BattleController.PathObjRoot, a_oDummy, ComType.G_LIFE_T_MUZZLE_FLASH);

		var oParticle = oMuzzleFlash.GetComponentInChildren<ParticleSystem>();
		oParticle.Play(true);
	}

	/** 발사형 탄두를 발사한다 */
	private void FireWarheadShoot(ProjectileController a_oController, UnitController a_oTarget, Transform a_oDummy, bool a_bIsPlaySnd)
	{
		float fHitRange = this.GetHitRangeShoot();
		float fAttackRange = this.AttackRangeStandard * ComType.G_UNIT_MM_TO_M;

		var stDelta = a_oTarget.ShootingPoint.transform.position - a_oDummy.position;
		var stUpDirection = a_oTarget.transform.up * Random.Range(-1.0f, 1.0f);
		var stRightDirection = a_oTarget.transform.right * Random.Range(-1.0f, 1.0f);

		var stShootingPoint = a_oDummy.position + (stDelta.normalized * fAttackRange);
		var stFinalShootingPoint = stShootingPoint + (stUpDirection + stRightDirection).normalized * Random.Range(-fHitRange, fHitRange);

		var stDirection = stFinalShootingPoint - a_oDummy.position;
		a_oController.Shoot(stDirection.normalized * this.WarheadSpeed, this.HandleOnTriggerEnterProjectile);

		// 사운드 재생 모드 일 경우
		if (a_bIsPlaySnd)
		{
			this.BattleController.PlaySound(this.CurSoundModelInfo?.FireSoundList, this.gameObject);
		}
	}

	/** 투척형 탄두를 발사한다 */
	private void FireWarheadThrow(ProjectileController a_oController, UnitController a_oTarget, Transform a_oDummy, int a_nFireFXGroup)
	{
		float fHitRange = this.GetHitRangeThrow();
		float fThrowDistance = Vector3.Distance(this.transform.position, a_oTarget.transform.position);

		var stThrowPos = a_oTarget.transform.position;

		// 타겟이 이동 상태 일 경우
		if (a_oTarget.Animator.GetBool(ComType.G_PARAMS_IS_MOVE))
		{
			float fDistance = Vector3.Distance(this.transform.position, a_oTarget.transform.position);
			float fHitOffset = GlobalTable.GetData<int>(ComType.G_DISTANCE_GRENADE_OFFSET) * ComType.G_UNIT_MM_TO_M;

			fHitRange = fDistance.ExIsLessEquals(fHitRange + fHitOffset) ? 0.0f : fHitRange;
			stThrowPos = stThrowPos + (a_oTarget.transform.forward * Mathf.Min(fHitOffset, fDistance));
		}

		var stFinalThrowPos = stThrowPos + new Vector3(Random.Range(-fHitRange, fHitRange), 0.0f, Random.Range(-fHitRange, fHitRange));
		var stVelocity = ComUtil.GetParabolaVelocity(a_oDummy.position, stFinalThrowPos, Physics.gravity, GlobalTable.GetData<float>(ComType.G_DEGREE_FOR_FIRE_GRENADE));

		var oFXTableList = CCollectionPoolManager.Singleton.SpawnList<EffectTable>();

		try
		{
			// 효과 그룹 키가 존재 할 경우
			if (a_nFireFXGroup > 0)
			{
				EffectTable.GetGroup(a_nFireFXGroup, oFXTableList);
			}

			a_oController.Throw(stFinalThrowPos,
				stVelocity, oFXTableList, this.ExplosionRangeRatio, this.HandleOnTriggerEnterProjectile, false, true, ComUtil.GetAbilityVal(EEquipEffectType.ExplosionRangeRatio, 0.0f, this.CurAbilityValDict));

			this.BattleController.PlaySound(this.CurSoundModelInfo?.FireSoundList, this.gameObject);
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oFXTableList);
		}
	}

	/** 일반 발사 이벤트를 처리한다 */
	private void HandleFireCommon(UnitController a_oTarget)
	{
		this.BattleController.PlaySound(this.CurSoundModelInfo?.FireSoundList, this.gameObject);

		// 공격 범위 안에 존재 할 경우
		if (this.IsAimableTarget(a_oTarget, this.AttackRange * ComType.G_UNIT_MM_TO_M))
		{
			a_oTarget.OnHit(this);
			this.BattleController.PlaySound(this.CurSoundModelInfo?.UnitHitSoundList, this.gameObject);
		}
		// 공격 범위 안에 장애물이 존재 할 경우
		else if (Physics.Raycast(this.GetAttackRayOriginPos(), this.transform.forward, this.AttackRange * ComType.G_UNIT_MM_TO_M, this.AttackLayerMask))
		{
			this.BattleController.PlaySound(this.CurSoundModelInfo?.StructureHitSoundList, this.gameObject);
		}
	}

	/** 투사체 발사 이벤트를 처리한다 */
	private void HandleFireProjectile(UnitController a_oTarget)
	{
		var oShellObj = this.CurWeaponModelInfo.GetShell();
		var oShellDummy = this.CurWeaponModelInfo.GetShellDummyTransform();

		var oWarheadObj = this.CurWeaponModelInfo.GetWarhead();
		var oWarheadDummy = this.CurWeaponModelInfo.GetWarheadDummyTransform();

		var oMuzzleFlashObj = this.CurWeaponModelInfo.GetMuzzleFlash();
		var oMuzzleFlashDummy = this.CurWeaponModelInfo.GetMuzzleFlashDummyTransform();

		this.FireShell(oShellObj, oShellDummy);
		this.FireWarhead(oWarheadObj, oWarheadDummy, a_oTarget, this.FireFXGroup);
		this.FireMuzzleFlash(oMuzzleFlashObj, oMuzzleFlashDummy);
	}
	#endregion // 합수
}
