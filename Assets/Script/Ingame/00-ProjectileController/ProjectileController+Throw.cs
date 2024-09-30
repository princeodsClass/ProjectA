using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

/** 투사체 제어자 - 투척 */
public partial class ProjectileController : MonoBehaviour
{
	#region 변수
	private Collider[] m_oOverlapColliders = new Collider[ComType.G_MAX_NUM_OVERLAP_NON_ALLOC];
	#endregion // 변수

	#region 함수
	/** 투사체를 던진다 */
	public void Throw(Vector3 a_stPos,
		Vector3 a_stVelocity, List<EffectTable> a_oFXTableList, float a_fExplosionRangeRatio, System.Action<ProjectileController, Collider> a_oCallback, bool a_bIsEnableWarning, bool a_bIsEnableAngular, float a_fExtraRange = 0.0f)
	{
		this.ResetRigidbody();
		float fTimeDelayTrigger = GlobalTable.GetData<int>(ComType.G_TIME_DELAY_TRIGGER) * ComType.G_UNIT_MS_TO_S;

		var oFXTable = a_oFXTableList.ExGetData(EEffectType.HIT_EXPLOSION_DAMAGE, ERangeType.HIT_TARGET_RANGE, EEffectCategory.HIT);
		var oDelayFXTable = a_oFXTableList.ExGetData(EEffectType.HIT_EXPLOSION_DAMAGE_DELAY, ERangeType.HIT_TARGET_RANGE, EEffectCategory.HIT);

		// 효과 테이블이 존재 할 경우
		if (oFXTable != null || oDelayFXTable != null)
		{
			float fRange = (oFXTable != null) ? oFXTable.Value : oDelayFXTable.Value;
			float fExtraFXRange = fRange * a_fExplosionRangeRatio;

			m_fFXRange = (fRange * ComType.G_UNIT_MM_TO_M) + (fExtraFXRange * ComType.G_UNIT_MM_TO_M) + a_fExtraRange;
		}

		m_stTargetPos = a_stPos;
		m_oDelayFXTable = oDelayFXTable;
		m_fFXDuration = (oDelayFXTable != null) ? fTimeDelayTrigger : ComType.G_DURATION_EXPLOSION_ANI;
		m_fFXExtraRangeRatio = a_fExplosionRangeRatio;
		m_oCollisionCallback = a_oCallback;

		a_oFXTableList.ExCopyTo(m_oEffectTableList, (a_oFXTable) => a_oFXTable);
		this.PlayExplosionBoundsAni(m_fFXRange, m_fFXDuration, null);

		// 경고가 필요 할 경우
		if (a_bIsEnableWarning)
		{
			this.SetupThrowWarningFX(a_stPos, m_fFXRange);
		}

		m_oCollider.isTrigger = true;
		this.ExLateCallFunc((a_oSender) => m_oCollider.isTrigger = false, 0.25f);

		// 회전이 필요 할 경우
		if (a_bIsEnableAngular)
		{
			m_oRigidbody.AddForceAtPosition(a_stVelocity,
				this.transform.position + new Vector3(Random.Range(-1.0f, 1.0f), 1.0f, 1.0f), ForceMode.VelocityChange);
		}
		else
		{
			m_oRigidbody.AddForce(a_stVelocity, ForceMode.VelocityChange);
		}

		// 딜레이 효과 테이블이 존재 할 경우
		if (oDelayFXTable != null)
		{
			StartCoroutine(this.CoHandleDelayExplosion(fTimeDelayTrigger));
		}
	}

	/** 투사체 경고 효과를 설정한다 */
	private void SetupThrowWarningFX(Vector3 a_stPos, float a_fRange)
	{
		// 경고 효과가 없을 경우
		if (this.Params.m_oOwner.FXModelInfo == null || this.Params.m_oOwner.FXModelInfo.GrenadeWarningFX == null)
		{
			return;
		}

		this.SetupThrowWarningFX(this.Params.m_oOwner.FXModelInfo.GrenadeWarningFX, a_stPos, a_fRange, out GameObject oWarningFX);
	}

	/** 투사체 경고 효과를 설정한다 */
	private void SetupThrowWarningFX(GameObject a_oOriginFX, Vector3 a_stPos, float a_fRange, out GameObject a_oOutWarningFX)
	{
		// 경고 효과가 없을 경우
		if (a_oOriginFX == null)
		{
			a_oOutWarningFX = null;
			return;
		}

		this.ResetFX();

		m_oExplosionWarning = GameResourceManager.Singleton.CreateObject(a_oOriginFX,
			this.BattleController.PathObjRoot, null, ComType.G_LIFE_T_EXPLOSION_WARNING);

		m_oExplosionWarning.transform.position = a_stPos + (Vector3.up * 0.5f);
		m_oExplosionWarning.transform.localScale = Vector3.one * (a_fRange * 2.0f);
		m_oExplosionWarning.transform.localEulerAngles = Vector3.zero;

		var oWarningParticle = m_oExplosionWarning.GetComponentInChildren<ParticleSystem>();
		oWarningParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		oWarningParticle.Play(true);

		a_oOutWarningFX = m_oExplosionWarning;
	}

	/** 투척 타격 효과를 재생한다 */
	private void PlayHitFXThrow(Collider a_oCollider)
	{
		var oFXObj = GameResourceManager.Singleton.CreateObject(this.ProjectileModelInfo.GetExplosionFX(),
			this.BattleController.PathObjRoot, this.transform, ComType.G_LIFE_T_HIT_FX);

		oFXObj.transform.rotation = Quaternion.identity;
		oFXObj.transform.localScale = Vector3.one * (m_fFXRange * 1.5f);

		oFXObj.GetComponentInChildren<ParticleSystem>()?.Play(true);
	}

	/** 폭발 범위 애니메이션을 재생한다 */
	private void PlayExplosionBoundsAni(float a_fRange, float a_fDuration, System.Action<Tween> a_oCallback)
	{
		// 폭발 범위 스피어가 없을 경우
		if (m_oExplosionBoundsSphere == null)
		{
			return;
		}

		var oSequence = DOTween.Sequence();
		oSequence.Append(m_oExplosionBoundsSphere.transform.DOScale(a_fRange * 2.0f, a_fDuration).SetEase(Ease.Linear));
		oSequence.AppendCallback(() => a_oCallback?.Invoke(oSequence));

		ComUtil.AssignVal(ref m_oExplosionBoundsAni, oSequence);

		m_oExplosionBoundsSphere.SetActive(true);
		m_oExplosionBoundsSphere.transform.localScale = Vector3.zero;
	}

	/** 충돌 시작을 처리한다 */
	private void HandleOnCollisionEnter(CCollisionDispatcher a_oSender, Collision a_oCollision)
	{
		var oFXTable = m_oEffectTableList.ExGetData(EEffectType.HIT_EXPLOSION_DAMAGE_DELAY,
			ERangeType.HIT_TARGET_RANGE, EEffectCategory.HIT);

		// 리지드 바디 빌보드가 존재 할 경우
		if (m_oRigidbodyBillboard != null)
		{
			m_oRigidbodyBillboard.enabled = false;
		}

		// 즉시 폭발이 아닐 경우
		if (oFXTable != null)
		{
			this.HandleExplosionDelay(a_oSender, a_oCollision);
			return;
		}

		this.HandleExplosion(a_oSender, a_oCollision);
	}

	/** 폭발을 처리한다 */
	private void HandleExplosion(CCollisionDispatcher a_oSender, Collision a_oCollision)
	{
		// 폭발 상태 일 경우
		if (m_bIsExplosion)
		{
			return;
		}

		m_bIsExplosion = true;

		var oGameObjList = CCollectionPoolManager.Singleton.SpawnList<GameObject>();
		var oColliderList = CCollectionPoolManager.Singleton.SpawnList<Collider>();

		try
		{
			var stPos = this.transform.position;
			var stExplosionPos = (Vector3.Distance(m_stTargetPos, stPos) <= 0.5f) ? m_stTargetPos : stPos;

			int nResult = Physics.OverlapSphereNonAlloc(stExplosionPos,
				m_fFXRange - 0.25f, m_oOverlapColliders, m_nTargetLayerMask);

			this.ResetFX();
			this.PageBattle.StartCameraShakeDirecting(Vector3.zero);
			this.BattleController.PlaySound(this.SoundModelInfo?.ExplosionSoundList, this.gameObject);

			for (int i = 0; i < nResult; ++i)
			{
				bool bIsValid01 = !oGameObjList.Contains(m_oOverlapColliders[i].gameObject);
				bool bIsValid02 = m_oOverlapColliders[i].TryGetComponent(out UnitController oController);

				// 타격이 불가능 할 경우
				if (!bIsValid01 || !bIsValid02 || oController == this.Params.m_oOwner || oController.TargetGroup == this.Params.m_oOwner.TargetGroup)
				{
					continue;
				}

				var stDelta = oController.GetAttackRayOriginPos() - stExplosionPos;

				// 장애물이 존재 할 경우
				if (Physics.Raycast(stExplosionPos, stDelta.normalized, out RaycastHit stRaycastHit, stDelta.magnitude, m_nStructureLayerMask))
				{
					continue;
				}

				oGameObjList.ExAddVal(m_oOverlapColliders[i].gameObject);
				oColliderList.ExAddVal(m_oOverlapColliders[i]);
			}

			this.NumHitTargets = oColliderList.Count;

			for (int i = 0; i < oColliderList.Count; ++i)
			{
				m_oCollisionCallback?.Invoke(this, oColliderList[i]);
			}

			this.PlayHitFX(null);
			this.TryHandleDamageField(stExplosionPos);
			this.TryHandleSplitExplosion(stExplosionPos);

			GameResourceManager.Singleton.ReleaseObject(a_oSender.gameObject);
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oGameObjList);
			CCollectionPoolManager.Singleton.DespawnList(oColliderList);
		}
	}

	/** 지연 폭발을 처리한다 */
	private void HandleExplosionDelay(CCollisionDispatcher a_oSender, Collision a_oCollision)
	{
		// 폭발 처리가 불가능 할 경우
		if (m_oExplosionWarning != null)
		{
			return;
		}

		this.SetupThrowWarningFX(this.BattleController.FXModelInfo.GrenadeWarningFX,
			this.transform.position, m_fFXRange, out GameObject oWarningFX);

		// 경고 효과 설정이 불가능 할 경우
		if (oWarningFX == null)
		{
			return;
		}

		oWarningFX.transform.SetParent(this.transform, false);
		oWarningFX.transform.localPosition = Vector3.zero;

		// 트랜스 폼 프리징이 없을 경우
		if (!oWarningFX.TryGetComponent(out CTransFreezing oTransFreezing))
		{
			oTransFreezing = oWarningFX.AddComponent<CTransFreezing>();
		}

		oTransFreezing.enabled = true;
		oTransFreezing.SetPosOffset(Vector3.up * 0.15f);
	}

	/** 데미지 필드를 처리한다 */
	private void TryHandleDamageField(Vector3 a_stExplosionPos)
	{
		// 데미지 필드 효과가 없을 경우
		if (this.ProjectileModelInfo.GetDamageFieldFX() == null)
		{
			return;
		}

		var oRangeFXTable = m_oEffectTableList.ExGetData(EEffectType.HIT_EXPLOSION_DAMAGE, ERangeType.HIT_TARGET_RANGE, EEffectCategory.HIT);
		var oDamageFieldFXTable = m_oEffectTableList.ExGetData(EEffectType.HIT_FLAME_DAMAGE, ERangeType.HIT_TARGET_RANGE, EEffectCategory.HIT);

		// 효과 테이블이 없을 경우
		if (oRangeFXTable == null || oDamageFieldFXTable == null)
		{
			return;
		}

		float fRange = m_fFXRange;
		float fDuration = oDamageFieldFXTable.Duration * ComType.G_UNIT_MS_TO_S;

		var stParams = DamageFieldController.MakeParams(this.Params.m_fDamageFieldATK,
			fRange, fDuration, this.Params.m_eDamageType, this.Params.m_eWeaponType, oDamageFieldFXTable, this.Params.m_oOwner, this.Params.m_oOwner.HandleOnApplyDamageField, null);

		var oDamageFieldController = GameResourceManager.Singleton.CreateObject<DamageFieldController>(this.ProjectileModelInfo.GetDamageFieldFX(),
			this.BattleController.PathObjRoot, null, fDuration * 10.0f);

		oDamageFieldController.transform.position = a_stExplosionPos + (Vector3.up * 0.1f);
		oDamageFieldController.transform.localScale = Vector3.one;
		oDamageFieldController.transform.localEulerAngles = Vector3.zero;

		oDamageFieldController.Init(stParams);
		oDamageFieldController.ApplyDamage();
	}

	/** 분할 폭발을 처리한다 */
	private void TryHandleSplitExplosion(Vector3 a_stExplosionPos)
	{
		var oFXTable = m_oEffectTableList.ExGetData(EEffectType.HIT_EXPLOSION_CREATE, ERangeType.HIT_TARGET_RANGE, EEffectCategory.HIT);
		var oRangeFXTable = m_oEffectTableList.ExGetData(EEffectType.HIT_EXPLOSION_DAMAGE, ERangeType.HIT_TARGET_RANGE, EEffectCategory.HIT);

		// 효과 테이블이 없을 경우
		if (oFXTable == null || oRangeFXTable == null)
		{
			return;
		}

		var oSplitFXTableList = CCollectionPoolManager.Singleton.SpawnList<EffectTable>();

		try
		{
			oSplitFXTableList.ExAddVal(oRangeFXTable);
			float fRange = Random.Range(1.0f, oFXTable.Value * ComType.G_UNIT_MM_TO_M);

			var stDirection = new Vector3(Random.Range(-1.0f, 1.0f), 0.0f, Random.Range(-1.0f, 1.0f));
			var stSplitPos = this.transform.position + (stDirection.normalized * fRange);

			// 내비게이션 영역이 아닐 경우
			if (!NavMesh.SamplePosition(stSplitPos + (Vector3.up * 1.5f), out NavMeshHit stNavMeshHit, float.MaxValue / 2.0f, m_nWalkableAreaMask))
			{
				return;
			}

			var oWarhead = GameResourceManager.Singleton.CreateObject(this.gameObject,
				this.BattleController.PathObjRoot, this.transform, ComType.G_LIFE_T_PROJECTILE);

			var oProjectileController = oWarhead.GetComponent<ProjectileController>();
			oProjectileController.transform.position = this.transform.position + (Vector3.up * 2.0f);
			oProjectileController.Init(this.Params);

			var stVelocity = ComUtil.GetParabolaVelocity(this.transform.position + (Vector3.up * 2.0f), stNavMeshHit.position, Physics.gravity, GlobalTable.GetData<float>(ComType.G_DEGREE_FOR_FIRE_GRENADE));
			oProjectileController.Throw(stNavMeshHit.position, stVelocity, oSplitFXTableList, m_fFXExtraRangeRatio, m_oCollisionCallback, true, false);
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oSplitFXTableList);
		}
	}

	/** 지연 폭발을 처리한다 */
	private IEnumerator CoHandleDelayExplosion(float a_fDuration)
	{
		yield return YieldInstructionCache.WaitForSeconds(a_fDuration);
		m_stTargetPos = m_oCollisionDispatcher.transform.position;

		this.HandleExplosion(m_oCollisionDispatcher, null);
	}
	#endregion // 함수
}
