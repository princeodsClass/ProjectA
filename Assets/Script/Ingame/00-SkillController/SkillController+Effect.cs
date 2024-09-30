using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/** 스킬 제어자 - 효과 */
public partial class SkillController : MonoBehaviour
{
	#region 함수
	/** 스킬 효과를 적용한다 */
	private void ApplySkillFX(EffectTable a_oFXTable)
	{
		switch ((EEffectType)a_oFXTable.Type)
		{
			case EEffectType.LIMIT_WEAPON_LOCK: this.HandleLockWeaponSkillFX(a_oFXTable); break;
		}
	}

	/** 효과를 시작한다 */
	private GameObject PlayFX(GameObject a_oFX, GameObject a_oParent, Vector3 a_stPos, Vector3 a_stScale)
	{
		// 효과가 없을 경우
		if (a_oFX == null)
		{
			return null;
		}

		var oFX = GameResourceManager.Singleton.CreateObject(a_oFX, a_oParent?.transform, null);
		oFX.transform.position = a_stPos;
		oFX.transform.localScale = a_stScale;

		var oParticleSystem = oFX.GetComponentInChildren<ParticleSystem>();
		oParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		oParticleSystem.Play(true);

		return oFX;
	}

	/** 무기 잠금 스킬 효과를 처리한다 */
	private void HandleLockWeaponSkillFX(EffectTable a_oFXTable)
	{
		var oSlotIdxList = CCollectionPoolManager.Singleton.SpawnList<int>();

		try
		{
			var oPlayerController = this.Params.m_oTarget as PlayerController;
			var oNonPlayerController = this.Params.m_oTarget as NonPlayerController;

			oNonPlayerController?.LockWeapon(0);

			// 플레이어 제어자 일 경우
			if (oPlayerController != null)
			{
				for (int j = 0; j < oPlayerController.EquipWeapons.Length; ++j)
				{
					// 무기가 존재 할 경우
					if (!oPlayerController.IsEmptySlot(j))
					{
						oSlotIdxList.Add(j);
					}
				}

				oSlotIdxList.ExShuffle();

				for (int i = 0; i < a_oFXTable.Value && i < oSlotIdxList.Count; ++i)
				{
					oPlayerController.LockWeapon(oSlotIdxList[i]);
				}
			}

			this.ExLateCallFunc((a_oSender) => GameResourceManager.Singleton.ReleaseObject(this.gameObject));
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnList(oSlotIdxList);
		}
	}
	#endregion // 함수
}
