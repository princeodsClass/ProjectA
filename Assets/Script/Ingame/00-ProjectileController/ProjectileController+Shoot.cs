using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 투사체 제어자 - 발사 */
public partial class ProjectileController : MonoBehaviour
{
	#region 함수
	/** 투사체를 발사한다 */
	public void Shoot(Vector3 a_stVelocity, System.Action<ProjectileController, Collider> a_oCallback)
	{
		this.ResetRigidbody();
		m_oCollisionCallback = a_oCallback;

		this.transform.forward = a_stVelocity.normalized;

		m_oRigidbody.AddForce(a_stVelocity, ForceMode.VelocityChange);
		m_oParticleSystem?.Play(true);
	}

	/** 발사 타격 효과를 재생한다 */
	private void PlayHitFXShoot(Collider a_oCollider)
	{
		// 투사체 모델 정보가 없을 경우
		if (this.ProjectileModelInfo == null)
		{
			return;
		}

		var eImpactType = ComUtil.IsUnit(a_oCollider.gameObject) ?
			ProjectileModelInfo.EImpactType.Human : ProjectileModelInfo.EImpactType.Common;

		eImpactType = a_oCollider.CompareTag(ComType.G_TAG_STRUCTURE_WOOD) ?
			ProjectileModelInfo.EImpactType.Wood : eImpactType;

		var stOriginPos = this.transform.position + (-this.transform.forward * 2.5f);
		var oImpactGameObj = this.ProjectileModelInfo.GetImpactFXObject(eImpactType);

		var oFXObj = GameResourceManager.Singleton.CreateObject(oImpactGameObj,
			this.BattleController.PathObjRoot, this.transform, ComType.G_LIFE_T_HIT_FX);

		oFXObj.transform.forward = -this.transform.forward;
		oFXObj.transform.position += -this.transform.forward * ComType.G_OFFSET_RATE_HIT_FX;

		oFXObj.GetComponentInChildren<ParticleSystem>()?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		oFXObj.GetComponentInChildren<ParticleSystem>()?.Play(true);

		// 유닛 일 경우
		if (ComUtil.IsUnit(a_oCollider.gameObject))
		{
			var stUnitController = a_oCollider.GetComponentInParent<UnitController>();

			var stDirection = stUnitController.GetAttackRayOriginPos() - this.transform.position;
			stDirection = -this.transform.forward * Vector3.Dot(-this.transform.forward, stDirection);

			var stDeltaPos = this.transform.position + stDirection;
			stDeltaPos = stDeltaPos - stUnitController.GetAttackRayOriginPos();

			oFXObj.transform.position = stUnitController.GetAttackRayOriginPos() + stDeltaPos;
		}
		// 구조물 일 경우
		else if (ComUtil.IsStructure(a_oCollider.gameObject) && Physics.Raycast(stOriginPos, this.transform.forward, out RaycastHit stRaycastHit, 2.5f, m_nStructureLayerMask))
		{
			var eDecalImpactType = a_oCollider.CompareTag(ComType.G_TAG_STRUCTURE_WOOD) ? ProjectileModelInfo.EImpactType.Wood : ProjectileModelInfo.EImpactType.Common;
			var oImpactDecalGameObj = this.ProjectileModelInfo.GetImpactDecalObject(eDecalImpactType);

			var oDecalObj = GameResourceManager.Singleton.CreateObject(oImpactDecalGameObj, this.BattleController.PathObjRoot, this.transform, ComType.G_LIFE_T_DECAL);
			oDecalObj.transform.forward = -stRaycastHit.normal;
			oDecalObj.transform.position = stRaycastHit.point;
		}

		this.PlayExtraHitFXShoot(a_oCollider, oFXObj.transform.position);
	}

	/** 발사 추가 타격 효과를 재생한다 */
	private void PlayExtraHitFXShoot(Collider a_oCollider, Vector3 a_stPos)
	{
		// 추가 타격 효과 처리가 불가능 할 경우
		if (this.Params.m_oOwner == null)
		{
			return;
		}

		this.Params.m_oOwner.HandleExtraHitEffect(this, a_oCollider, a_stPos);
	}

	/** 충돌 시작을 처리한다 */
	private void HandleOnTriggerEnter(CTriggerDispatcher a_oSender, Collider a_oCollider)
	{
		// 발사 상태가 아닐 경우
		if (this.Params.m_eAttackType != EAttackType.SHOOT)
		{
			return;
		}

		// 오디오 수신자 일 경우
		if (a_oCollider.CompareTag(ComType.G_TAG_AUDIO_LISTENER))
		{
			this.BattleController.PlaySound(this.SoundModelInfo?.PassBySoundList, this.gameObject);
		}

		// 구조물 일 경우
		if (ComUtil.IsStructure(a_oCollider.gameObject))
		{
			this.PlayHitFX(a_oCollider);
			this.BattleController.PlaySound(this.SoundModelInfo?.StructureHitSoundList, this.gameObject);

			GameResourceManager.Singleton.ReleaseObject(a_oSender.gameObject);
		}
		// 충돌 전달이 가능 할 경우
		else if (a_oCollider.TryGetComponent(out UnitController oController) && oController != this.Params.m_oOwner)
		{
			this.BattleController.PlaySound(this.SoundModelInfo?.UnitHitSoundList, this.gameObject);
			m_oCollisionCallback?.Invoke(this, a_oCollider);
		}
	}
	#endregion // 함수
}
