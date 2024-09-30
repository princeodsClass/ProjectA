using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

/** 스킬 제어자 - 스킬 */
public partial class SkillController : MonoBehaviour
{
	#region 변수
	private Collider[] m_oOverlapColliders = new Collider[ComType.G_MAX_NUM_OVERLAP_NON_ALLOC];
	#endregion // 변수

	#region 함수
	/** 소환 애니메이션이 완료 되었을 경우 */
	private void OnCompleteSummonAni(Tween a_oSender, GameObject a_oSummonFX)
	{
		a_oSender?.ExKill();
		GameResourceManager.Singleton.ReleaseObject(a_oSummonFX);

		bool bIsValid = this.BattleController.TargetNonPlayerControllerList.Count >= 1;

		// 내비게이션 메시 영역에서 벗어났을 경우
		if (!bIsValid || !NavMesh.SamplePosition(a_oSummonFX.transform.position + (Vector3.up * 1.5f), out NavMeshHit stNavMeshHit, float.MaxValue / 2.0f, m_nWalkableAreaMask))
		{
			return;
		}

#if DISABLE_THIS
		int nIdx = Random.Range(0, this.BattleController.SummonNPCTableList.Count);
		var oNPCTable = this.BattleController.SummonNPCTableList[nIdx];
#else
		var oNPCGroupTableList = CCollectionPoolManager.Singleton.SpawnList<NPCGroupTable>();
		NPCTable oNPCTable = null;

		NPCGroupTable.GetGroup(this.Params.m_oTable.HitEffectGroup, oNPCGroupTableList);

		try
		{
			int nSelectionFactor = 0;
			int nSumSelectionFactor = oNPCGroupTableList.Sum((a_oGroupTable) => a_oGroupTable.SelectionFactor);

			float fRandomSelectionFactor = Random.Range(0.0f, (float)nSumSelectionFactor);

			for (int i = 0; i < oNPCGroupTableList.Count; ++i)
			{
				nSelectionFactor += oNPCGroupTableList[i].SelectionFactor;

				// NPC 생성이 가능 할 경우
				if (fRandomSelectionFactor.ExIsLess((float)nSelectionFactor))
				{
					oNPCTable = NPCTable.GetData(oNPCGroupTableList[i].NPCKey);
					break;
				}
			}
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oNPCGroupTableList);
		}
#endif // #if DISABLE_THIS

		// NPC 소환이 불가능 할 경우
		if(oNPCTable == null)
		{
			return;
		}

		var oNonPlayerController = this.BattleController.CreateNonPlayer(oNPCTable,
			stNavMeshHit.position, this.BattleController.PlayMapObjsRoot, null, a_oEffectTableList: this.BattleController.NPCEffectTableList);

		oNonPlayerController.SetTrackingTarget(this.Params.m_oTarget);

		this.ExLateCallFunc((a_oFuncSender) =>
		{
			float fStandardHP = oNonPlayerController.CurStandardAbilityValDict.GetValueOrDefault(EEquipEffectType.HP);
			oNonPlayerController.CurStandardAbilityValDict.ExReplaceVal(EEquipEffectType.HP, fStandardHP * this.Params.m_oTable.ratioAttackPower);
			oNonPlayerController.SetupAbilityValues(true);

			oNonPlayerController.OnBattlePlay();
		});

		// 소환 완료 효과가 존재 할 경우
		if (m_oFXModelInfo.SummonCompleteFX != null)
		{
			var oSummonCompleteFX = GameResourceManager.Singleton.CreateObject<ParticleSystem>(m_oFXModelInfo.SummonCompleteFX,
				oNonPlayerController.transform, null, 5.0f);

			oSummonCompleteFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			oSummonCompleteFX.Play(true);
		}
	}

	/** 점프 공격 애니메이션이 시작 되었을 경우 */
	private void OnStartJumpAttackAni(Sequence a_oSender, Vector3 a_stPos)
	{
		this.PlayFX(m_oFXModelInfo.JumpAttackStartFX, this.BattleController.PathObjRoot.gameObject, a_stPos, Vector3.one);
		this.BattleController.PlaySound(m_oSoundModelInfo.FlySoundList, this.Params.m_oOwner.gameObject);
	}

	/** 점프 공격 애니메이션이 진행 중 일 경우 */
	private void OnContinueJumpAttackAni(Sequence a_oSender, Vector3 a_stPos, float a_fRange, float a_fDuration)
	{
		var oFX = this.PlayFX(m_oFXModelInfo.JumpAttackWarningFX,
			this.BattleController.PathObjRoot.gameObject, a_stPos + (Vector3.up * 0.15f), Vector3.one * (a_fRange * 2.0f));

		var oParticleSystem = oFX.GetComponentInChildren<ParticleSystem>();

		var stMainModule = oParticleSystem.main;
		stMainModule.startLifetime = a_fDuration;
	}

	/** 점프 공격 애니메이션이 준비 되었을 경우 */
	private void OnReadyJumpAttackAni(Sequence a_oSender, Vector3 a_stPos, Vector3 a_stTargetPos)
	{
		this.Params.m_oOwner.Animator.SetTrigger(ComType.G_PARAMS_SKILL_FINISH);
		this.BattleController.PlaySound(m_oSoundModelInfo.FlySoundList, a_stTargetPos);
	}

	/** 점프 공격 애니메이션이 완료 되었을 경우 */
	private void OnCompleteJumpAttackAni(Sequence a_oSender, Vector3 a_stPos, float a_fRange)
	{
		a_oSender?.Kill();

		// 사망 상태 일 경우
		if (!this.Params.m_oOwner.IsSurvive)
		{
			return;
		}

		var oGameObjList = CCollectionPoolManager.Singleton.SpawnList<GameObject>();

		try
		{
			this.PageBattle.StartCameraShakeDirecting(Vector3.zero);
			this.BattleController.PlaySound(this.BattleController.SoundModelInfo?.ExplosionSoundList, this.Params.m_oOwner.gameObject);

			int nResult = Physics.OverlapSphereNonAlloc(a_stPos,
				a_fRange - 0.25f, m_oOverlapColliders, m_nTargetLayerMask);

			for (int i = 0; i < nResult; ++i)
			{
				bool bIsValid01 = !oGameObjList.Contains(m_oOverlapColliders[i].gameObject);
				bool bIsValid02 = m_oOverlapColliders[i].TryGetComponent(out UnitController oController);

				// 타격이 불가능 할 경우
				if (!bIsValid01 || !bIsValid02 || oController == this.Params.m_oOwner || oController.TargetGroup == this.Params.m_oOwner.TargetGroup)
				{
					continue;
				}

				oGameObjList.ExAddVal(m_oOverlapColliders[i].gameObject);
				this.Params.m_oCallback?.Invoke(this, m_oOverlapColliders[i]);
			}

			this.PlayFX(m_oFXModelInfo.JumpAttackFinishFX,
				this.BattleController.PathObjRoot.gameObject, a_stPos, Vector3.one * (a_fRange * 2.0f));
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oGameObjList);
		}
	}

	/** 소환 스킬을 처리한다 */
	private void HandleSummonSkill()
	{
		// 소환 효과가 없을 경우
		if (m_oFXModelInfo.SummonFX == null)
		{
			return;
		}

		var stSummonPos = this.BattleController.GetSummonPos(this.Params.m_oOwner.NavMeshAgent);

		// 소환 위치가 유효하지 않을 경우
		if (stSummonPos.ExIsEquals(Vector3.zero))
		{
			return;
		}

		var oSummonFX = GameResourceManager.Singleton.CreateObject(m_oFXModelInfo.SummonFX,
			this.Params.m_oOwner.transform, null);

		oSummonFX.GetComponentInChildren<TrailRenderer>()?.Clear();

		var oParticleSystem = oSummonFX.GetComponentInChildren<ParticleSystem>();
		oParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		oParticleSystem.Play(true);

		m_oSummonAni?.ExKill(a_bIsComplete: true);

		m_oSummonAni = oSummonFX.transform.DOJump(stSummonPos, Random.Range(5.0f, 10.0f), 1, ComType.G_DURATION_SUMMON_ANI);
		m_oSummonAni.OnComplete(() => this.OnCompleteSummonAni(m_oSummonAni, oSummonFX.gameObject));
	}

	/** 무기 잠금 스킬을 처리한다 */
	private void HandleLockWeaponSkill()
	{
		var oFXTableList = CCollectionPoolManager.Singleton.SpawnList<EffectTable>();

		try
		{
			EffectTable.GetGroup(this.Params.m_oTable.HitEffectGroup, oFXTableList);

			for (int i = 0; i < oFXTableList.Count; ++i)
			{
				this.ApplySkillFX(oFXTableList[i]);
			}
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oFXTableList);
		}
	}

	/** 폭격 요청 스킬을 처리한다 */
	private void HandleBombingRequestSkill()
	{
		// Do Something
	}

	/** 점프 공격 스킬을 처리한다 */
	private void HandleJumpAttackSkill()
	{
		var oFXTableList = CCollectionPoolManager.Singleton.SpawnList<EffectTable>();

		try
		{
			EffectTable.GetGroup(this.Params.m_oTable.HitEffectGroup, oFXTableList);
			this.Params.m_oOwner.LookAt(this.Params.m_stTargetPos);

			var oFXTable = oFXTableList.ExGetData(EEffectType.LIMIT_JUMP_ATTACK,
				ERangeType.HIT_TARGET_RANGE, EEffectCategory.HIT);

			StopCoroutine("CoHandleJumpAttackSkill");
			StartCoroutine(this.CoHandleJumpAttackSkill(oFXTable));
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oFXTableList);
		}
	}

	/** 화염 필드 스킬을 처리한다 */
	private void HandleFlameFieldSkill()
	{
		StopCoroutine("CoHandleFlameFieldSkill");
		StartCoroutine(this.CoHandleFlameFieldSkill());
	}

	/** 데미지 필드 스킬을 처리한다 */
	private void HandleDamageFieldSkill(EffectTable a_oFXTable)
	{
		float fATK = this.Params.m_fATK;
		float fRange = a_oFXTable.Value * ComType.G_UNIT_MM_TO_M;
		float fDuration = a_oFXTable.Duration * ComType.G_UNIT_MS_TO_S;

		var stTargetPos = this.Params.m_oTarget.transform.position;
		var stDamageFieldPos = stTargetPos + new Vector3(Random.Range(-fRange, fRange), 0.0f, Random.Range(-fRange, fRange));

		var stParams = DamageFieldController.MakeParams(fATK,
			fRange, fDuration, (EDamageType)this.Params.m_oTable.DamageType, this.Params.m_eWeaponType, a_oFXTable, this.Params.m_oOwner, this.Params.m_oOwner.HandleOnApplyDamageField, this.OnCompleteApplyDamage);

		var oDamageFieldController = GameResourceManager.Singleton.CreateObject<DamageFieldController>(m_oFXModelInfo.FlameFieldFX,
			this.BattleController.PathObjRoot, null, fDuration * 10.0f);

		oDamageFieldController.transform.position = stDamageFieldPos + (Vector3.up * 0.1f);
		oDamageFieldController.Init(stParams);
		oDamageFieldController.ApplyDamage();
	}

	/** 도탄 스킬을 처리한다 */
	private void HandleRicochetSkill()
	{
		var oEffectTableList = EffectTable.GetGroup(this.Params.m_oTable.HitEffectGroup);

		ComUtil.AddEffect(EEquipEffectType.FORCE_RICOCHET_CHANCE,
			EEquipEffectType.FORCE_RICOCHET_CHANCE, oEffectTableList[0].Value, oEffectTableList[0].Duration * ComType.G_UNIT_MS_TO_S, 0.0f, this.Params.m_oOwner.ActiveEffectStackInfoList, 1);

		this.Params.m_oOwner.SetupAbilityValues(true);
	}

	/** 무적 스킬을 처리한다 */
	private void HandleUntouchableSkill()
	{
		var oEffectTableList = EffectTable.GetGroup(this.Params.m_oTable.HitEffectGroup);
		this.Params.m_oOwner.StartUntouchableFX(oEffectTableList[0].Duration * ComType.G_UNIT_MS_TO_S);
	}

	/** 아이스 스킬을 처리한다 */
	private void HandleIceSkill()
	{
		var oFXObj = GameResourceManager.Singleton.CreateObject(this.m_oFXModelInfo.IceFXInfo.m_oIceFieldFX,
			this.BattleController.PathObjRoot, this.transform, ComType.G_LIFE_T_FX);

		var oEffectTableList = EffectTable.GetGroup(this.Params.m_oTable.HitEffectGroup);

		oFXObj.transform.rotation = Quaternion.identity;
		oFXObj.transform.localScale = Vector3.one * oEffectTableList[0].Value * ComType.G_UNIT_MM_TO_M * 2.0f;

		oFXObj.GetComponentInChildren<ParticleSystem>()?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		oFXObj.GetComponentInChildren<ParticleSystem>()?.Play(true);

		this.ExLateCallFunc((a_oSender) =>
		{
			this.DoHandleIceSkill(oEffectTableList[0].Value * ComType.G_UNIT_MM_TO_M, 
				oEffectTableList[0].Duration * ComType.G_UNIT_MS_TO_S);
		});
	}

	/** 데미지 적용이 완료 되었을 경우 */
	private void OnCompleteApplyDamage(DamageFieldController a_oSender)
	{
		this.ExLateCallFunc((a_oFuncSender) => this.Params.m_oCompleteCallback?.Invoke(this));
	}

	/** 아이스 스킬을 처리한다 */
	private void DoHandleIceSkill(float a_fRange, float a_fDuration)
	{
		// 사망 상태 일 경우
		if (!this.Params.m_oOwner.IsSurvive)
		{
			return;
		}

		var oGameObjList = CCollectionPoolManager.Singleton.SpawnList<GameObject>();

		try
		{
			int nResult = Physics.OverlapSphereNonAlloc(this.transform.position,
				a_fRange - 0.25f, m_oOverlapColliders, m_nTargetLayerMask);

			for (int i = 0; i < nResult; ++i)
			{
				bool bIsValid01 = !oGameObjList.Contains(m_oOverlapColliders[i].gameObject);
				bool bIsValid02 = m_oOverlapColliders[i].TryGetComponent(out UnitController oController);

				// 타격이 불가능 할 경우
				if (!bIsValid01 || !bIsValid02 || oController == this.Params.m_oOwner || oController.TargetGroup == this.Params.m_oOwner.TargetGroup)
				{
					continue;
				}

				oGameObjList.ExAddVal(m_oOverlapColliders[i].gameObject);
				this.Params.m_oCallback?.Invoke(this, m_oOverlapColliders[i]);

				// 생존 상태 일 경우
				if (oController.IsSurvive)
				{
					oController.Animator.SetBool(ComType.G_PARAMS_IS_FREEZE, true);
					oController.StateMachine.SetState(oController.CreateIdleState());

					ComUtil.AddEffect(EEquipEffectType.FREEZE,
						EEquipEffectType.FREEZE, 0.0f, a_fDuration, 0.0f, oController.ActiveEffectStackInfoList, 1);

					var oFXObj = oController.PlayEffectFX(EEquipEffectType.FREEZE, false);
					oFXObj.GetComponentInChildren<CTimeFXHandler>()?.StartFX(a_fDuration);

					oController.SetupAbilityValues(true);
				}
				else
				{
					oController.gameObject.SetActive(false);
					oController.PlayStatusFX(EEquipEffectType.FREEZE);
				}
			}
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oGameObjList);
		}
	}
	#endregion // 함수

	#region 코루틴 함수
	/** 점프 공격 스킬을 처리한다 */
	private IEnumerator CoHandleJumpAttackSkill(EffectTable a_oFXTable)
	{
		float fCastTime = this.Params.m_oTable.CastingTime * ComType.G_UNIT_MS_TO_S;
		float fInvokeTime = this.Params.m_oTable.InvokeTime * ComType.G_UNIT_MS_TO_S;
		float fFinalInvokeTime = fInvokeTime - (m_fJumpAttackSkillJumpDuration * 2.0f);

		yield return YieldInstructionCache.WaitForSeconds(fCastTime);
		this.Params.m_oOwner.Animator.SetTrigger(ComType.G_PARAMS_SKILL_FIRE);

		m_oJumpAttackAni?.Kill();
		var stTargetPos = this.Params.m_stTargetPos;

		float fHitRange = (this.Params.m_oTarget != null) ? this.Params.m_oTarget.GetHitRangeThrow() : 0.0f;
		float fAttackRange = a_oFXTable.Value * ComType.G_UNIT_MM_TO_M;

		// 타겟이 이동 상태 일 경우
		if (this.Params.m_oTarget != null && this.Params.m_oTarget.Animator.GetBool(ComType.G_PARAMS_IS_MOVE))
		{
			float fDistance = Vector3.Distance(this.transform.position, this.Params.m_oTarget.transform.position);
			float fHitOffset = GlobalTable.GetData<int>(ComType.G_DISTANCE_GRENADE_OFFSET) * ComType.G_UNIT_MM_TO_M;

			fHitRange = fDistance.ExIsLessEquals(fHitRange + fHitOffset) ? 0.0f : fHitRange;
			stTargetPos = this.Params.m_oTarget.transform.position + (this.Params.m_oTarget.transform.forward * Mathf.Min(fHitOffset, fDistance));
		}

		var stFinalTargetPos = stTargetPos + new Vector3(Random.Range(-fHitRange, fHitRange), 0.0f, Random.Range(-fHitRange, fHitRange));
		bool bIsValidPos = NavMesh.SamplePosition(stFinalTargetPos + (Vector3.up * 1.5f), out NavMeshHit stNavMeshHit, float.MaxValue / 2.0f, m_nWalkableAreaMask);

		stFinalTargetPos = bIsValidPos ? stNavMeshHit.position : this.Params.m_oOwner.transform.position;

		var oSequence = DOTween.Sequence();
		oSequence.AppendInterval(m_fJumpAttackSkillJumpDelay);
		oSequence.AppendCallback(() => this.OnStartJumpAttackAni(oSequence, this.Params.m_oOwner.transform.position));
		oSequence.Append(this.Params.m_oOwner.transform.DOMove(this.Params.m_oOwner.transform.position + (Vector3.up * m_fJumpAttackJumpOffset), m_fJumpAttackSkillJumpDuration).SetEase(Ease.InQuad));
		oSequence.AppendCallback(() => this.OnContinueJumpAttackAni(oSequence, stFinalTargetPos, fAttackRange, fFinalInvokeTime + m_fJumpAttackSkillJumpDuration));
		oSequence.Append(this.Params.m_oOwner.transform.DOMove(stFinalTargetPos + (Vector3.up * m_fJumpAttackJumpOffset), fFinalInvokeTime));
		oSequence.AppendCallback(() => this.OnReadyJumpAttackAni(oSequence, this.Params.m_oOwner.transform.position, stFinalTargetPos));
		oSequence.Append(this.Params.m_oOwner.transform.DOMove(stFinalTargetPos, m_fJumpAttackSkillJumpDuration));
		oSequence.AppendCallback(() => this.OnCompleteJumpAttackAni(oSequence, stFinalTargetPos, fAttackRange));

		ComUtil.AssignVal(ref m_oJumpAttackAni, oSequence);
	}

	/** 화염 필드 스킬을 처리한다 */
	private IEnumerator CoHandleFlameFieldSkill()
	{
		yield return YieldInstructionCache.WaitForSeconds(this.Params.m_oTable.CastingTime * ComType.G_UNIT_MS_TO_S);
		var oFXTableList = CCollectionPoolManager.Singleton.SpawnList<EffectTable>();

		try
		{
			// 명중 효과 그룹이 존재 할 경우
			if (this.Params.m_oTable.HitEffectGroup > 0)
			{
				EffectTable.GetGroup(this.Params.m_oTable.HitEffectGroup, oFXTableList);
			}

			var oFXTable = oFXTableList.ExGetData(EEffectType.HIT_FLAME_DAMAGE,
				ERangeType.HIT_TARGET_RANGE, EEffectCategory.HIT);

			// 효과 테이블이 없을 경우
			if (oFXTable == null)
			{
				yield break;
			}

			this.HandleDamageFieldSkill(oFXTable);
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oFXTableList);
		}
	}
	#endregion // 코루틴 함수
}
