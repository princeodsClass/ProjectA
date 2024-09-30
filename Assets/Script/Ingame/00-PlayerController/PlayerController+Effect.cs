using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 플레이어 제어자 - 효과 */
public partial class PlayerController : UnitController
{
	#region 함수
	/** 추가 타격 효과를 처리한다 */
	public override void HandleExtraHitEffect(ProjectileController a_oController, Collider a_oCollider, Vector3 a_stPos)
	{
		base.HandleExtraHitEffect(a_oController, a_oCollider, a_stPos);

		switch (a_oController.Params.m_eAttackType)
		{
			case EAttackType.SHOOT: this.HandleExtraHitEffectShoot(a_oController, a_oCollider, a_stPos); break;
		}
	}

	/** 발사 추가 타격 효과를 처리한다 */
	private void HandleExtraHitEffectShoot(ProjectileController a_oController, Collider a_oCollider, Vector3 a_stPos)
	{
		// 무기 정보가 없을 경우
		if (a_oController.Params.m_oWeapon == null)
		{
			return;
		}

		var oPlayerController = a_oController.Params.m_oOwner as PlayerController;
		int nWeaponIdx = oPlayerController.GetWeaponIdx(a_oController.Params.m_oWeapon);

		// 효과 처리가 불가능 할 경우
		if (nWeaponIdx < 0 || !oPlayerController.AbilityValDicts[nWeaponIdx].ContainsKey(EEquipEffectType.SHOOT_HIT_EXPLOSION))
		{
			return;
		}

		// 폭발 효과가 존재 할 경우
		if(a_oController.ProjectileModelInfo.GetExplosionFX() != null)
		{
			var oFXObj = GameResourceManager.Singleton.CreateObject(a_oController.ProjectileModelInfo.GetExplosionFX(), 
				this.BattleController.PathObjRoot, null, ComType.G_LIFE_T_HIT_FX);
				
			oFXObj.transform.position = a_stPos;
			oFXObj.transform.rotation = Quaternion.identity;
			oFXObj.transform.localScale = Vector3.one * oPlayerController.AbilityValDicts[nWeaponIdx][EEquipEffectType.SHOOT_HIT_EXPLOSION] * 2.0f;

			oFXObj.GetComponentInChildren<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			oFXObj.GetComponentInChildren<ParticleSystem>()?.Play(true);
		}

		var oGameObjList = CCollectionPoolManager.Singleton.SpawnList<GameObject>();

		try
		{
			int nResult = Physics.OverlapSphereNonAlloc(a_stPos,
				oPlayerController.AbilityValDicts[nWeaponIdx][EEquipEffectType.SHOOT_HIT_EXPLOSION] - 0.15f, m_oOverlapColliders, this.NonPlayerLayerMask);

			for (int i = 0; i < nResult; ++i)
			{
				bool bIsValid01 = !oGameObjList.Contains(m_oOverlapColliders[i].gameObject);
				bool bIsValid02 = m_oOverlapColliders[i].TryGetComponent(out UnitController oController);

				// 타격이 불가능 할 경우
				if (!bIsValid01 || !bIsValid02 || oController == this || oController.gameObject == a_oCollider.gameObject)
				{
					continue;
				}

				var stDelta = oController.GetAttackRayOriginPos() - a_stPos;

				// 장애물이 존재 할 경우
				if (Physics.Raycast(a_stPos, stDelta.normalized, out RaycastHit stRaycastHit, stDelta.magnitude, this.StructureLayerMask))
				{
					continue;
				}

				oGameObjList.ExAddVal(m_oOverlapColliders[i].gameObject);
				oController.OnHit(this, a_oController);
			}
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oGameObjList);
		}
	}
	#endregion // 함수
}
